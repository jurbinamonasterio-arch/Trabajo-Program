using Avalonia.Controls;
using Sigbim.BLL;
using Sigbim.Entidades;

namespace Sigbim.UI.Ventanas;

/// <summary>Mantenedor CRUD de categorías.</summary>
public class VentanaCategorias : VentanaMantenedor<Categoria>
{
    private readonly CategoriaBLL _bll = new();

    private readonly TextBox _txtNombre = Ui.Texto(60, "Ej: Insumos clínicos");
    private readonly TextBox _txtDescripcion = Ui.TextoLargo(200);
    private readonly CheckBox _chkActivo = new() { Content = "Activo" };

    public VentanaCategorias() : base("Categorías")
    {
        Inicializar();
    }

    protected override void ConfigurarColumnas()
    {
        Columna("Id", nameof(Categoria.IdCategoria));
        Columna("Nombre", nameof(Categoria.Nombre));
        Columna("Descripción", nameof(Categoria.Descripcion));
        Columna("Estado", nameof(Categoria.EstadoTexto));
        Columna("Creado por", nameof(Categoria.UsuarioCreacion));
        Columna("Modificado", nameof(Categoria.FechaModificacion), "{0:dd-MM-yyyy HH:mm}");
    }

    protected override Control CrearFormulario() => Ui.Formulario(
        ("Nombre *", _txtNombre),
        ("Descripción", _txtDescripcion),
        ("", _chkActivo));

    protected override IEnumerable<Categoria> Cargar(string? buscar, bool incluirInactivos) =>
        _bll.Listar(soloActivos: !incluirInactivos);

    protected override void MostrarEnFormulario(Categoria? c)
    {
        _txtNombre.Text = c?.Nombre ?? string.Empty;
        _txtDescripcion.Text = c?.Descripcion ?? string.Empty;
        _chkActivo.IsChecked = c?.Activo ?? true;
        _chkActivo.IsEnabled = c is not null;   // un registro nuevo siempre nace activo
    }

    protected override int Guardar()
    {
        var categoria = new Categoria
        {
            IdCategoria = Actual?.IdCategoria ?? 0,
            Nombre = _txtNombre.Text ?? string.Empty,
            Descripcion = _txtDescripcion.Text,
            Activo = _chkActivo.IsChecked == true
        };
        return _bll.Guardar(categoria);
    }

    protected override void Eliminar(Categoria c) => _bll.Eliminar(c.IdCategoria);
    protected override int ObtenerId(Categoria c) => c.IdCategoria;
    protected override string Describir(Categoria c) => c.Nombre;
}
