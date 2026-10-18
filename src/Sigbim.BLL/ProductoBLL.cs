using Sigbim.DAL;
using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>Reglas de negocio del mantenedor de productos.</summary>
public class ProductoBLL : ServicioBase
{
    private readonly ProductoDAL _dal;

    public ProductoBLL() : this(new ProductoDAL()) { }
    public ProductoBLL(ProductoDAL dal) => _dal = dal;

    public List<ProductoDetalle> Listar(string? buscar = null, bool soloActivos = true)
    {
        var texto = string.IsNullOrWhiteSpace(buscar) ? null : buscar.Trim();
        if (texto is { Length: > 120 })
            throw new ValidacionException("El texto de búsqueda es demasiado largo.");

        return Ejecutar("Listar productos", () => _dal.Listar(texto, soloActivos));
    }

    public ProductoDetalle? Obtener(int idProducto) =>
        Ejecutar("Obtener producto", () => _dal.Obtener(idProducto));

    /// <summary>Inserta si IdProducto = 0; si no, actualiza. Devuelve el Id.</summary>
    public int Guardar(Producto p)
    {
        Validar(p);
        p.Sku = p.Sku.Trim().ToUpperInvariant();
        p.Nombre = p.Nombre.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(p.Descripcion) ? null : p.Descripcion.Trim();

        if (p.IdProducto == 0)
            return Ejecutar("Crear producto", () => _dal.Insertar(p, Sesion.UsuarioActual),
                id => $"IdProducto={id} Sku={p.Sku}");

        Ejecutar("Actualizar producto", () => _dal.Actualizar(p, Sesion.UsuarioActual),
            _ => $"IdProducto={p.IdProducto} Sku={p.Sku}");
        return p.IdProducto;
    }

    /// <summary>
    /// Baja lógica. Regla de negocio: no se puede desactivar un producto que aún
    /// tiene stock, para no perder unidades físicas del control de inventario.
    /// </summary>
    public void Eliminar(int idProducto)
    {
        if (idProducto <= 0)
            throw new ValidacionException("Debe seleccionar un producto.");

        var actual = Obtener(idProducto) ?? throw new ValidacionException("El producto ya no existe.");
        if (actual.Stock > 0)
            throw new ValidacionException(
                $"No se puede eliminar '{actual.Nombre}' porque tiene {actual.Stock} unidades en stock. " +
                "Registre primero una salida o un ajuste a 0.");

        Ejecutar("Desactivar producto", () => _dal.Eliminar(idProducto, Sesion.UsuarioActual),
            _ => $"IdProducto={idProducto}");
    }

    private static void Validar(Producto p)
    {
        new Validador()
            .Requerido(p.Sku, "SKU")
            .Sku(p.Sku)
            .Requerido(p.Nombre, "Nombre")
            .LargoMaximo(p.Nombre, 120, "Nombre")
            .LargoMaximo(p.Descripcion, 250, "Descripción")
            .Condicion(p.IdCategoria > 0, "Debe seleccionar una categoría.")
            .Condicion(p.IdProveedor > 0, "Debe seleccionar un proveedor.")
            .Condicion(p.PrecioCompra >= 0, "El precio de compra no puede ser negativo.")
            .Condicion(p.PrecioVenta >= 0, "El precio de venta no puede ser negativo.")
            .Condicion(p.PrecioCompra <= 9_999_999_999m && p.PrecioVenta <= 9_999_999_999m,
                       "Los precios superan el máximo permitido.")
            .Condicion(p.PrecioVenta >= p.PrecioCompra,
                       "El precio de venta no puede ser menor al precio de compra.")
            .Condicion(p.StockMinimo >= 0, "El stock mínimo no puede ser negativo.")
            .Condicion(p.FechaVencimiento is null || p.IdProducto > 0 || p.FechaVencimiento.Value.Date > DateTime.Today,
                       "La fecha de vencimiento de un producto nuevo debe ser futura.")
            .LanzarSiHayErrores();
    }
}
