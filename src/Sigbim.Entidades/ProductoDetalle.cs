namespace Sigbim.Entidades;

/// <summary>
/// Proyección de la vista vw_productodetalle: producto con los datos
/// de su categoría, proveedor, estado de stock e indicadores calculados.
/// </summary>
public class ProductoDetalle : Producto
{
    public string Categoria { get; set; } = string.Empty;
    public string RutProveedor { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public string? ContactoProveedor { get; set; }
    public string? TelefonoProveedor { get; set; }
    public decimal Margen { get; set; }
    public string EstadoStock { get; set; } = string.Empty;
    public decimal ValorInventario { get; set; }
    public int TotalMovimientos { get; set; }
    public DateTime? UltimoMovimiento { get; set; }
}
