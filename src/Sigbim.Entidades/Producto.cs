namespace Sigbim.Entidades;

/// <summary>Insumo o medicamento del inventario (tabla Producto).</summary>
public class Producto : EntidadAuditable
{
    public int IdProducto { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int IdCategoria { get; set; }
    public int IdProveedor { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }

    /// <summary>Solo lectura para la UI: el stock cambia únicamente mediante movimientos.</summary>
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool RequiereReceta { get; set; }

    public override string ToString() => $"{Sku} - {Nombre}";
}
