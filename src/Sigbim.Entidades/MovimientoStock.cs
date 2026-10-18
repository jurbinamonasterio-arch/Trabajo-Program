namespace Sigbim.Entidades;

public enum TipoMovimiento
{
    ENTRADA,
    SALIDA,
    AJUSTE
}

/// <summary>Registro inmutable de un cambio de stock (tabla MovimientoStock).</summary>
public class MovimientoStock
{
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public int StockAnterior { get; set; }
    public int StockNuevo { get; set; }
    public string? Motivo { get; set; }
    public string? DocumentoReferencia { get; set; }
    public DateTime Fecha { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;

    // Datos de lectura provenientes del JOIN con Producto
    public string Sku { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
}
