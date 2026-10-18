using System.Globalization;
using System.Text.RegularExpressions;
using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>
/// Bitácora local de la aplicación (ISO 27001 A.8.15 Registro de eventos).
/// Escribe en la carpeta "logs" junto al ejecutable, un archivo por día.
/// Solo registra la operación, el usuario y el resultado: nunca contraseñas
/// ni la cadena de conexión.
/// </summary>
public static class RegistroEventos
{
    private static readonly object Bloqueo = new();
    private static readonly string Carpeta = Path.Combine(AppContext.BaseDirectory, "logs");

    // Formato de cada línea: "2026-10-18 19:07:24 [INFO] [usuario] Operación - Detalle"
    private static readonly Regex FormatoLinea =
        new(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[(\w+)\] \[(.*?)\] (.*?) - (.*)$");

    public static void Info(string operacion, string detalle) => Escribir("INFO", operacion, detalle);

    public static void Error(string operacion, Exception ex)
    {
        var detalle = ex switch
        {
            DatosException d => $"Código BD {d.CodigoError}: {d.InnerException?.Message ?? d.Message}",
            ValidacionException v => string.Join(" | ", v.Errores),
            _ => $"{ex.GetType().Name}: {ex.Message}"
        };
        Escribir(ex is ValidacionException ? "WARN" : "ERROR", operacion, detalle);
    }

    /// <summary>Lee todos los archivos de la bitácora y devuelve los eventos en orden cronológico.</summary>
    public static List<EventoBitacora> Leer()
    {
        var eventos = new List<EventoBitacora>();
        lock (Bloqueo)
        {
            if (!Directory.Exists(Carpeta))
                return eventos;

            foreach (var archivo in Directory.GetFiles(Carpeta, "sigbim-*.log").Order())
            {
                foreach (var linea in File.ReadLines(archivo))
                {
                    var m = FormatoLinea.Match(linea);
                    if (!m.Success)
                        continue;   // líneas con otro formato se ignoran

                    eventos.Add(new EventoBitacora
                    {
                        Fecha = DateTime.ParseExact(m.Groups[1].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Nivel = m.Groups[2].Value,
                        Usuario = m.Groups[3].Value,
                        Operacion = m.Groups[4].Value,
                        Detalle = m.Groups[5].Value
                    });
                }
            }
        }

        return eventos.OrderBy(e => e.Fecha).ToList();
    }

    private static void Escribir(string nivel, string operacion, string detalle)
    {
        try
        {
            lock (Bloqueo)
            {
                Directory.CreateDirectory(Carpeta);
                var archivo = Path.Combine(Carpeta, $"sigbim-{DateTime.Now:yyyyMMdd}.log");
                var linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{nivel}] [{Sesion.UsuarioActual}] {operacion} - {detalle}";
                File.AppendAllText(archivo, linea + Environment.NewLine);
            }
        }
        catch
        {
            // La falla del log nunca debe detener la operación del usuario.
        }
    }
}
