using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>
/// Consulta de la bitácora (inicios de sesión, creaciones, modificaciones,
/// desactivaciones y movimientos) para mostrarla y exportarla a PDF (ISO 27001 A.8.15).
/// </summary>
public class BitacoraBLL : ServicioBase
{
    public List<EventoBitacora> Listar() =>
        Ejecutar("Consultar bitácora", RegistroEventos.Leer);

    /// <summary>Deja constancia de quién exportó la bitácora y a qué archivo.</summary>
    public void RegistrarExportacion(string archivo, int cantidadEventos) =>
        RegistroEventos.Info("Exportar bitácora PDF", $"Archivo={Path.GetFileName(archivo)} Eventos={cantidadEventos}");
}
