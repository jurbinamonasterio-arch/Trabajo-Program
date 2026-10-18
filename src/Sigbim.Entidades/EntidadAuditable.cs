namespace Sigbim.Entidades;

/// <summary>
/// Clase base con los campos de auditoría comunes a las tablas maestras.
/// Soporta la trazabilidad exigida por ISO/IEC 27001 A.8.15 (registro de eventos).
/// </summary>
public abstract class EntidadAuditable
{
    public bool Activo { get; set; } = true;
    public int Version { get; set; } = 1;
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
}
