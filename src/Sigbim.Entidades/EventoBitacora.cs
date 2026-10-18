namespace Sigbim.Entidades;

/// <summary>Una línea de la bitácora de la aplicación (logs/sigbim-AAAAMMDD.log).</summary>
public class EventoBitacora
{
    public DateTime Fecha { get; set; }
    public string Nivel { get; set; } = string.Empty;       // INFO / WARN / ERROR
    public string Usuario { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
}
