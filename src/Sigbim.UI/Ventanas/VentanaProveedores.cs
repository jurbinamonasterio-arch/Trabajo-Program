using Avalonia.Controls;
using Sigbim.BLL;
using Sigbim.Entidades;

namespace Sigbim.UI.Ventanas;

/// <summary>Mantenedor CRUD de proveedores.</summary>
public class VentanaProveedores : VentanaMantenedor<Proveedor>
{
    private readonly ProveedorBLL _bll = new();

    private readonly TextBox _txtRut = Ui.Texto(12, "12345678-9");
    private readonly TextBox _txtRazonSocial = Ui.Texto(120);
    private readonly TextBox _txtContacto = Ui.Texto(120);
    private readonly TextBox _txtEmail = Ui.Texto(120, "correo@empresa.cl");
    private readonly TextBox _txtTelefono = Ui.Texto(20, "+56 9 1234 5678");
    private readonly TextBox _txtDireccion = Ui.Texto(150);
    private readonly TextBox _txtCiudad = Ui.Texto(60);
    private readonly CheckBox _chkActivo = new() { Content = "Activo" };

    public VentanaProveedores() : base("Proveedores")
    {
        Inicializar();
    }

    protected override void ConfigurarColumnas()
    {
        Columna("RUT", nameof(Proveedor.Rut));
        Columna("Razón social", nameof(Proveedor.RazonSocial));
        Columna("Contacto", nameof(Proveedor.NombreContacto));
        Columna("Email", nameof(Proveedor.Email));
        Columna("Teléfono", nameof(Proveedor.Telefono));
        Columna("Ciudad", nameof(Proveedor.Ciudad));
        Columna("Estado", nameof(Proveedor.EstadoTexto));
    }

    protected override Control CrearFormulario() => Ui.Formulario(
        ("RUT *", _txtRut),
        ("Razón social *", _txtRazonSocial),
        ("Contacto", _txtContacto),
        ("Email", _txtEmail),
        ("Teléfono", _txtTelefono),
        ("Dirección", _txtDireccion),
        ("Ciudad", _txtCiudad),
        ("", _chkActivo));

    protected override IEnumerable<Proveedor> Cargar(string? buscar, bool incluirInactivos) =>
        _bll.Listar(soloActivos: !incluirInactivos);

    protected override void MostrarEnFormulario(Proveedor? p)
    {
        _txtRut.Text = p?.Rut ?? string.Empty;
        _txtRazonSocial.Text = p?.RazonSocial ?? string.Empty;
        _txtContacto.Text = p?.NombreContacto ?? string.Empty;
        _txtEmail.Text = p?.Email ?? string.Empty;
        _txtTelefono.Text = p?.Telefono ?? string.Empty;
        _txtDireccion.Text = p?.Direccion ?? string.Empty;
        _txtCiudad.Text = p?.Ciudad ?? string.Empty;
        _chkActivo.IsChecked = p?.Activo ?? true;
        _chkActivo.IsEnabled = p is not null;
    }

    protected override int Guardar()
    {
        var proveedor = new Proveedor
        {
            IdProveedor = Actual?.IdProveedor ?? 0,
            Rut = _txtRut.Text ?? string.Empty,
            RazonSocial = _txtRazonSocial.Text ?? string.Empty,
            NombreContacto = _txtContacto.Text,
            Email = _txtEmail.Text,
            Telefono = _txtTelefono.Text,
            Direccion = _txtDireccion.Text,
            Ciudad = _txtCiudad.Text,
            Activo = _chkActivo.IsChecked == true
        };
        return _bll.Guardar(proveedor);
    }

    protected override void Eliminar(Proveedor p) => _bll.Eliminar(p.IdProveedor);
    protected override int ObtenerId(Proveedor p) => p.IdProveedor;
    protected override string Describir(Proveedor p) => p.RazonSocial;
}
