using Sigbim.DAL;
using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>Reglas de negocio para entradas, salidas y ajustes de stock.</summary>
public class MovimientoStockBLL : ServicioBase
{
    private readonly MovimientoStockDAL _dal;

    public MovimientoStockBLL() : this(new MovimientoStockDAL()) { }
    public MovimientoStockBLL(MovimientoStockDAL dal) => _dal = dal;

    public List<MovimientoStock> Listar(int? idProducto = null) =>
        Ejecutar("Listar movimientos", () => _dal.Listar(idProducto));

    /// <summary>Registra el movimiento y devuelve el nuevo stock del producto.</summary>
    public int Registrar(MovimientoStock m)
    {
        new Validador()
            .Condicion(m.IdProducto > 0, "Debe seleccionar un producto.")
            .Condicion(m.Cantidad > 0, "La cantidad debe ser mayor a 0.")
            .Condicion(m.Cantidad <= 1_000_000, "La cantidad supera el máximo permitido por movimiento.")
            .Condicion(m.Tipo == TipoMovimiento.ENTRADA || !string.IsNullOrWhiteSpace(m.Motivo),
                       "Las salidas y ajustes deben indicar un motivo.")
            .LargoMaximo(m.Motivo, 200, "Motivo")
            .LargoMaximo(m.DocumentoReferencia, 40, "Documento de referencia")
            .LanzarSiHayErrores();

        m.Motivo = string.IsNullOrWhiteSpace(m.Motivo) ? null : m.Motivo.Trim();
        m.DocumentoReferencia = string.IsNullOrWhiteSpace(m.DocumentoReferencia)
            ? null
            : m.DocumentoReferencia.Trim().ToUpperInvariant();

        var (_, stockNuevo) = Ejecutar($"Registrar {m.Tipo}",
            () => _dal.Registrar(m, Sesion.UsuarioActual),
            r => $"IdMovimiento={r.IdMovimiento} IdProducto={m.IdProducto} Cantidad={m.Cantidad} StockNuevo={r.StockNuevo}");

        return stockNuevo;
    }
}
