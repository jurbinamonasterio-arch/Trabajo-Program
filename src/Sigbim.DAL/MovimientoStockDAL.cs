using System.Data.Common;
using Sigbim.Entidades;

namespace Sigbim.DAL;

/// <summary>Acceso a datos de MovimientoStock mediante procedimientos almacenados.</summary>
public class MovimientoStockDAL
{
    /// <summary>
    /// Registra el movimiento. El SP actualiza el stock del producto dentro de
    /// una transacción y devuelve el Id del movimiento y el stock resultante.
    /// </summary>
    public (int IdMovimiento, int StockNuevo) Registrar(MovimientoStock m, string usuario)
    {
        var fila = Db.Primero("sp_movimiento_registrar",
            r => new MovimientoStock
            {
                IdMovimiento = r.Entero("IdMovimiento"),
                StockNuevo = r.Entero("StockNuevo")
            },
            ("p_IdProducto", m.IdProducto),
            ("p_Tipo", m.Tipo.ToString()),
            ("p_Cantidad", m.Cantidad),
            ("p_Motivo", m.Motivo),
            ("p_Documento", m.DocumentoReferencia),
            ("p_Usuario", usuario));

        return fila is null ? (0, 0) : (fila.IdMovimiento, fila.StockNuevo);
    }

    public List<MovimientoStock> Listar(int? idProducto) =>
        Db.Lista("sp_movimiento_listar", Mapear, ("p_IdProducto", idProducto));

    private static MovimientoStock Mapear(DbDataReader r) => new()
    {
        IdMovimiento = r.Entero("IdMovimiento"),
        IdProducto = r.Entero("IdProducto"),
        Tipo = Enum.Parse<TipoMovimiento>(r.Texto("Tipo"), ignoreCase: true),
        Cantidad = r.Entero("Cantidad"),
        StockAnterior = r.Entero("StockAnterior"),
        StockNuevo = r.Entero("StockNuevo"),
        Motivo = r.TextoNulo("Motivo"),
        DocumentoReferencia = r.TextoNulo("DocumentoReferencia"),
        Fecha = r.Fecha("Fecha"),
        UsuarioResponsable = r.Texto("UsuarioResponsable"),
        Sku = r.Texto("Sku"),
        Producto = r.Texto("Producto")
    };
}
