using System.Data.Common;
using Sigbim.Entidades;

namespace Sigbim.DAL;

/// <summary>
/// Acceso a datos de Producto mediante procedimientos almacenados.
/// Las lecturas usan la vista vw_productodetalle (a través de los SPs).
/// </summary>
public class ProductoDAL
{
    public int Insertar(Producto p, string usuario) =>
        Db.EscalarEntero("sp_producto_insertar",
            ("p_Sku", p.Sku),
            ("p_Nombre", p.Nombre),
            ("p_Descripcion", p.Descripcion),
            ("p_IdCategoria", p.IdCategoria),
            ("p_IdProveedor", p.IdProveedor),
            ("p_PrecioCompra", p.PrecioCompra),
            ("p_PrecioVenta", p.PrecioVenta),
            ("p_StockMinimo", p.StockMinimo),
            ("p_FechaVencimiento", p.FechaVencimiento?.Date),
            ("p_RequiereReceta", p.RequiereReceta),
            ("p_Usuario", usuario));

    /// <summary>Actualiza usando control de concurrencia optimista (campo Version).</summary>
    public int Actualizar(Producto p, string usuario) =>
        Db.EscalarEntero("sp_producto_actualizar",
            ("p_IdProducto", p.IdProducto),
            ("p_Sku", p.Sku),
            ("p_Nombre", p.Nombre),
            ("p_Descripcion", p.Descripcion),
            ("p_IdCategoria", p.IdCategoria),
            ("p_IdProveedor", p.IdProveedor),
            ("p_PrecioCompra", p.PrecioCompra),
            ("p_PrecioVenta", p.PrecioVenta),
            ("p_StockMinimo", p.StockMinimo),
            ("p_FechaVencimiento", p.FechaVencimiento?.Date),
            ("p_RequiereReceta", p.RequiereReceta),
            ("p_Activo", p.Activo),
            ("p_Version", p.Version),
            ("p_Usuario", usuario));

    public int Eliminar(int idProducto, string usuario) =>
        Db.EscalarEntero("sp_producto_eliminar",
            ("p_IdProducto", idProducto),
            ("p_Usuario", usuario));

    public ProductoDetalle? Obtener(int idProducto) =>
        Db.Primero("sp_producto_obtener", Mapear, ("p_IdProducto", idProducto));

    public List<ProductoDetalle> Listar(string? buscar, bool soloActivos) =>
        Db.Lista("sp_producto_listar", Mapear,
            ("p_Buscar", buscar),
            ("p_SoloActivos", soloActivos));

    private static ProductoDetalle Mapear(DbDataReader r) => new()
    {
        IdProducto = r.Entero("IdProducto"),
        Sku = r.Texto("Sku"),
        Nombre = r.Texto("Producto"),
        Descripcion = r.TextoNulo("Descripcion"),
        IdCategoria = r.Entero("IdCategoria"),
        Categoria = r.Texto("Categoria"),
        IdProveedor = r.Entero("IdProveedor"),
        RutProveedor = r.Texto("RutProveedor"),
        Proveedor = r.Texto("Proveedor"),
        ContactoProveedor = r.TextoNulo("ContactoProveedor"),
        TelefonoProveedor = r.TextoNulo("TelefonoProveedor"),
        PrecioCompra = r.Decimal("PrecioCompra"),
        PrecioVenta = r.Decimal("PrecioVenta"),
        Margen = r.Decimal("Margen"),
        Stock = r.Entero("Stock"),
        StockMinimo = r.Entero("StockMinimo"),
        EstadoStock = r.Texto("EstadoStock"),
        ValorInventario = r.Decimal("ValorInventario"),
        FechaVencimiento = r.FechaNula("FechaVencimiento"),
        RequiereReceta = r.Booleano("RequiereReceta"),
        Activo = r.Booleano("Activo"),
        Version = r.Entero("Version"),
        TotalMovimientos = r.Entero("TotalMovimientos"),
        UltimoMovimiento = r.FechaNula("UltimoMovimiento"),
        FechaCreacion = r.Fecha("FechaCreacion"),
        UsuarioCreacion = r.Texto("UsuarioCreacion"),
        FechaModificacion = r.FechaNula("FechaModificacion"),
        UsuarioModificacion = r.TextoNulo("UsuarioModificacion")
    };
}
