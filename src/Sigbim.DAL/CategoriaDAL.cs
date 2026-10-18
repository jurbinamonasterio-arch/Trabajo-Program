using System.Data.Common;
using Sigbim.Entidades;

namespace Sigbim.DAL;

/// <summary>Acceso a datos de Categoria mediante procedimientos almacenados.</summary>
public class CategoriaDAL
{
    public int Insertar(Categoria c, string usuario) =>
        Db.EscalarEntero("sp_categoria_insertar",
            ("p_Nombre", c.Nombre),
            ("p_Descripcion", c.Descripcion),
            ("p_Usuario", usuario));

    public int Actualizar(Categoria c, string usuario) =>
        Db.EscalarEntero("sp_categoria_actualizar",
            ("p_IdCategoria", c.IdCategoria),
            ("p_Nombre", c.Nombre),
            ("p_Descripcion", c.Descripcion),
            ("p_Activo", c.Activo),
            ("p_Usuario", usuario));

    public int Eliminar(int idCategoria, string usuario) =>
        Db.EscalarEntero("sp_categoria_eliminar",
            ("p_IdCategoria", idCategoria),
            ("p_Usuario", usuario));

    public List<Categoria> Listar(bool soloActivos) =>
        Db.Lista("sp_categoria_listar", Mapear, ("p_SoloActivos", soloActivos));

    private static Categoria Mapear(DbDataReader r) => new()
    {
        IdCategoria = r.Entero("IdCategoria"),
        Nombre = r.Texto("Nombre"),
        Descripcion = r.TextoNulo("Descripcion"),
        Activo = r.Booleano("Activo"),
        Version = r.Entero("Version"),
        FechaCreacion = r.Fecha("FechaCreacion"),
        UsuarioCreacion = r.Texto("UsuarioCreacion"),
        FechaModificacion = r.FechaNula("FechaModificacion"),
        UsuarioModificacion = r.TextoNulo("UsuarioModificacion")
    };
}
