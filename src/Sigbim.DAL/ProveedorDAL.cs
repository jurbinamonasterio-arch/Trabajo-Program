using System.Data.Common;
using Sigbim.Entidades;

namespace Sigbim.DAL;

/// <summary>Acceso a datos de Proveedor mediante procedimientos almacenados.</summary>
public class ProveedorDAL
{
    public int Insertar(Proveedor p, string usuario) =>
        Db.EscalarEntero("sp_proveedor_insertar",
            ("p_Rut", p.Rut),
            ("p_RazonSocial", p.RazonSocial),
            ("p_NombreContacto", p.NombreContacto),
            ("p_Email", p.Email),
            ("p_Telefono", p.Telefono),
            ("p_Direccion", p.Direccion),
            ("p_Ciudad", p.Ciudad),
            ("p_Usuario", usuario));

    public int Actualizar(Proveedor p, string usuario) =>
        Db.EscalarEntero("sp_proveedor_actualizar",
            ("p_IdProveedor", p.IdProveedor),
            ("p_Rut", p.Rut),
            ("p_RazonSocial", p.RazonSocial),
            ("p_NombreContacto", p.NombreContacto),
            ("p_Email", p.Email),
            ("p_Telefono", p.Telefono),
            ("p_Direccion", p.Direccion),
            ("p_Ciudad", p.Ciudad),
            ("p_Activo", p.Activo),
            ("p_Usuario", usuario));

    public int Eliminar(int idProveedor, string usuario) =>
        Db.EscalarEntero("sp_proveedor_eliminar",
            ("p_IdProveedor", idProveedor),
            ("p_Usuario", usuario));

    public List<Proveedor> Listar(bool soloActivos) =>
        Db.Lista("sp_proveedor_listar", Mapear, ("p_SoloActivos", soloActivos));

    private static Proveedor Mapear(DbDataReader r) => new()
    {
        IdProveedor = r.Entero("IdProveedor"),
        Rut = r.Texto("Rut"),
        RazonSocial = r.Texto("RazonSocial"),
        NombreContacto = r.TextoNulo("NombreContacto"),
        Email = r.TextoNulo("Email"),
        Telefono = r.TextoNulo("Telefono"),
        Direccion = r.TextoNulo("Direccion"),
        Ciudad = r.TextoNulo("Ciudad"),
        Activo = r.Booleano("Activo"),
        Version = r.Entero("Version"),
        FechaCreacion = r.Fecha("FechaCreacion"),
        UsuarioCreacion = r.Texto("UsuarioCreacion"),
        FechaModificacion = r.FechaNula("FechaModificacion"),
        UsuarioModificacion = r.TextoNulo("UsuarioModificacion")
    };
}
