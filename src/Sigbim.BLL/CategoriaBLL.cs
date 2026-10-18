using Sigbim.DAL;
using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>Reglas de negocio del mantenedor de categorías.</summary>
public class CategoriaBLL : ServicioBase
{
    private readonly CategoriaDAL _dal;

    public CategoriaBLL() : this(new CategoriaDAL()) { }
    public CategoriaBLL(CategoriaDAL dal) => _dal = dal;

    public List<Categoria> Listar(bool soloActivos = true) =>
        Ejecutar("Listar categorías", () => _dal.Listar(soloActivos));

    /// <summary>Inserta si IdCategoria = 0; si no, actualiza. Devuelve el Id.</summary>
    public int Guardar(Categoria c)
    {
        Validar(c);
        c.Nombre = c.Nombre.Trim();
        c.Descripcion = string.IsNullOrWhiteSpace(c.Descripcion) ? null : c.Descripcion.Trim();

        if (c.IdCategoria == 0)
            return Ejecutar("Crear categoría", () => _dal.Insertar(c, Sesion.UsuarioActual),
                id => $"IdCategoria={id} Nombre={c.Nombre}");

        Ejecutar("Actualizar categoría", () => ExigirFila(_dal.Actualizar(c, Sesion.UsuarioActual)),
            _ => $"IdCategoria={c.IdCategoria}");
        return c.IdCategoria;
    }

    public void Eliminar(int idCategoria)
    {
        if (idCategoria <= 0)
            throw new ValidacionException("Debe seleccionar una categoría.");

        Ejecutar("Desactivar categoría", () => ExigirFila(_dal.Eliminar(idCategoria, Sesion.UsuarioActual)),
            _ => $"IdCategoria={idCategoria}");
    }

    private static void Validar(Categoria c)
    {
        new Validador()
            .Requerido(c.Nombre, "Nombre")
            .LargoMaximo(c.Nombre, 60, "Nombre")
            .LargoMaximo(c.Descripcion, 200, "Descripción")
            .LanzarSiHayErrores();
    }

    private static int ExigirFila(int filas) =>
        filas > 0 ? filas : throw new ValidacionException("La categoría ya no existe.");
}
