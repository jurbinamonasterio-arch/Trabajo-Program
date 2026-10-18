using Sigbim.DAL;
using Sigbim.Entidades;

namespace Sigbim.BLL;

/// <summary>Reglas de negocio del mantenedor de proveedores.</summary>
public class ProveedorBLL : ServicioBase
{
    private readonly ProveedorDAL _dal;

    public ProveedorBLL() : this(new ProveedorDAL()) { }
    public ProveedorBLL(ProveedorDAL dal) => _dal = dal;

    public List<Proveedor> Listar(bool soloActivos = true) =>
        Ejecutar("Listar proveedores", () => _dal.Listar(soloActivos));

    /// <summary>Inserta si IdProveedor = 0; si no, actualiza. Devuelve el Id.</summary>
    public int Guardar(Proveedor p)
    {
        Validar(p);
        Normalizar(p);

        if (p.IdProveedor == 0)
            return Ejecutar("Crear proveedor", () => _dal.Insertar(p, Sesion.UsuarioActual),
                id => $"IdProveedor={id} Rut={p.Rut}");

        Ejecutar("Actualizar proveedor", () => ExigirFila(_dal.Actualizar(p, Sesion.UsuarioActual)),
            _ => $"IdProveedor={p.IdProveedor}");
        return p.IdProveedor;
    }

    public void Eliminar(int idProveedor)
    {
        if (idProveedor <= 0)
            throw new ValidacionException("Debe seleccionar un proveedor.");

        Ejecutar("Desactivar proveedor", () => ExigirFila(_dal.Eliminar(idProveedor, Sesion.UsuarioActual)),
            _ => $"IdProveedor={idProveedor}");
    }

    private static void Validar(Proveedor p)
    {
        new Validador()
            .Requerido(p.Rut, "RUT")
            .Rut(p.Rut)
            .Requerido(p.RazonSocial, "Razón social")
            .LargoMaximo(p.RazonSocial, 120, "Razón social")
            .LargoMaximo(p.NombreContacto, 120, "Nombre de contacto")
            .LargoMaximo(p.Email, 120, "Email")
            .Email(p.Email, "Email")
            .Telefono(p.Telefono, "Teléfono")
            .LargoMaximo(p.Direccion, 150, "Dirección")
            .LargoMaximo(p.Ciudad, 60, "Ciudad")
            .LanzarSiHayErrores();
    }

    private static void Normalizar(Proveedor p)
    {
        static string? Limpio(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        p.Rut = Validador.NormalizarRut(p.Rut);
        p.RazonSocial = p.RazonSocial.Trim();
        p.NombreContacto = Limpio(p.NombreContacto);
        p.Email = Limpio(p.Email)?.ToLowerInvariant();
        p.Telefono = Limpio(p.Telefono);
        p.Direccion = Limpio(p.Direccion);
        p.Ciudad = Limpio(p.Ciudad);
    }

    private static int ExigirFila(int filas) =>
        filas > 0 ? filas : throw new ValidacionException("El proveedor ya no existe.");
}
