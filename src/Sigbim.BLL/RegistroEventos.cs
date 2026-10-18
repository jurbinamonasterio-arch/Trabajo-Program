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
