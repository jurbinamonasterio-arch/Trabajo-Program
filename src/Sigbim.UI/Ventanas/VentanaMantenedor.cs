using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Sigbim.UI.Ventanas;

/// <summary>
/// Plantilla común para los mantenedores CRUD: grilla a la izquierda, formulario
/// a la derecha y botones Nuevo / Guardar / Desactivar. Cada mantenedor concreto
/// solo define sus columnas, su formulario y qué servicio de la BLL llamar.
/// </summary>
public abstract class VentanaMantenedor<T> : Window where T : class
{
    protected readonly DataGrid Grilla = new()
    {
        IsReadOnly = true,
        AutoGenerateColumns = false,
        SelectionMode = DataGridSelectionMode.Single,
        GridLinesVisibility = DataGridGridLinesVisibility.Horizontal
    };

    protected readonly TextBox TxtBuscar = new() { Watermark = "Buscar por SKU o nombre...", Width = 260 };
    protected readonly CheckBox ChkInactivos = new() { Content = "Mostrar inactivos", VerticalAlignment = VerticalAlignment.Center };

    private readonly TextBlock _lblEstado = new() { Margin = new Thickness(12, 6), Opacity = 0.7 };
    private readonly TextBlock _lblModo = new() { FontSize = 18, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 12), TextWrapping = TextWrapping.Wrap };
    private readonly Button _btnNuevo = new() { Content = "Nuevo" };
    private readonly Button _btnGuardar = new() { Content = "Guardar" };
    private readonly Button _btnEliminar = new() { Content = "Desactivar" };
    private readonly bool _conBuscador;

    /// <summary>Registro que se está editando; null significa "nuevo registro".</summary>
    protected T? Actual { get; private set; }

    protected VentanaMantenedor(string titulo, bool conBuscador = false)
    {
        Title = $"SIGBIM - {titulo}";
        Width = 1180;
        Height = 700;
        MinWidth = 900;
        MinHeight = 520;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _conBuscador = conBuscador;
    }

    // ---------- Lo que cada mantenedor debe implementar ----------

    protected abstract void ConfigurarColumnas();
    protected abstract Control CrearFormulario();
    protected abstract IEnumerable<T> Cargar(string? buscar, bool incluirInactivos);
    protected abstract void MostrarEnFormulario(T? registro);
    protected abstract int Guardar();
    protected abstract void Eliminar(T registro);
    protected abstract int ObtenerId(T registro);
    protected abstract string Describir(T registro);

    /// <summary>Permite recargar combos u otras listas antes de llenar la grilla.</summary>
    protected virtual void CargarListasAuxiliares() { }

    // ---------- Construcción de la ventana ----------

    /// <summary>Debe llamarse al final del constructor de la clase hija.</summary>
    protected void Inicializar()
    {
        ConfigurarColumnas();

        var barra = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(12) };
        if (_conBuscador)
        {
            var btnBuscar = new Button { Content = "Buscar" };
            btnBuscar.Click += async (_, _) => await RecargarAsync();
            TxtBuscar.KeyUp += async (_, e) =>
            {
                if (e.Key == Key.Enter)
                    await RecargarAsync();
            };
            barra.Children.Add(TxtBuscar);
            barra.Children.Add(btnBuscar);
        }

        var btnRefrescar = new Button { Content = "Refrescar" };
        btnRefrescar.Click += async (_, _) => await RecargarAsync();
        ChkInactivos.Click += async (_, _) => await RecargarAsync();
        barra.Children.Add(btnRefrescar);
        barra.Children.Add(ChkInactivos);

        _btnGuardar.Classes.Add("accent");
        var botones = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 16, 0, 0),
            Children = { _btnNuevo, _btnGuardar, _btnEliminar }
        };

        var panelFormulario = new Border
        {
            Width = 420,
            Padding = new Thickness(16),
            BorderThickness = new Thickness(1, 0, 0, 0),
            BorderBrush = Brushes.Gray,
            Child = new ScrollViewer
            {
                Content = new StackPanel { Children = { _lblModo, CrearFormulario(), botones } }
            }
        };

        var dock = new DockPanel();
        DockPanel.SetDock(barra, Dock.Top);
        DockPanel.SetDock(_lblEstado, Dock.Bottom);
        DockPanel.SetDock(panelFormulario, Dock.Right);
        dock.Children.Add(barra);
        dock.Children.Add(_lblEstado);
        dock.Children.Add(panelFormulario);
        dock.Children.Add(Grilla);
        Content = dock;

        Grilla.SelectionChanged += (_, _) =>
        {
            if (Grilla.SelectedItem is T registro)
                Editar(registro);
        };
        _btnNuevo.Click += (_, _) =>
        {
            Grilla.SelectedItem = null;
            Editar(null);
        };
        _btnGuardar.Click += async (_, _) => await GuardarAsync();
        _btnEliminar.Click += async (_, _) => await EliminarAsync();
        Opened += async (_, _) => await RecargarAsync();

        Editar(null);
    }

    protected void Columna(string encabezado, string propiedad, string? formato = null)
    {
        Grilla.Columns.Add(new DataGridTextColumn
        {
            Header = encabezado,
            Binding = new Binding(propiedad) { StringFormat = formato }
        });
    }

    // ---------- Flujo CRUD ----------

    private void Editar(T? registro)
    {
        Actual = registro;
        _lblModo.Text = registro is null ? "Nuevo registro" : $"Editando: {Describir(registro)}";
        _btnEliminar.IsEnabled = registro is not null;
        MostrarEnFormulario(registro);
    }

    protected async Task RecargarAsync(int? seleccionarId = null)
    {
        await Ui.EjecutarAsync(this, () =>
        {
            CargarListasAuxiliares();
            var datos = Cargar(_conBuscador ? TxtBuscar.Text : null, ChkInactivos.IsChecked == true).ToList();
            Grilla.ItemsSource = datos;
            _lblEstado.Text = $"{datos.Count} registro(s)";

            var seleccion = seleccionarId is null ? null : datos.FirstOrDefault(d => ObtenerId(d) == seleccionarId);
            if (seleccion is not null)
            {
                Grilla.SelectedItem = seleccion;
                Editar(seleccion);
            }
            else
            {
                Grilla.SelectedItem = null;
                Editar(null);
            }
        });
    }

    private async Task GuardarAsync()
    {
        var esNuevo = Actual is null;
        var idGuardado = 0;
        var ok = await Ui.EjecutarAsync(this, () => idGuardado = Guardar());
        if (!ok)
            return;

        await Ui.MensajeAsync(this, "Guardado", esNuevo ? "Registro creado correctamente." : "Cambios guardados correctamente.");
        await RecargarAsync(idGuardado);
    }

    private async Task EliminarAsync()
    {
        if (Actual is not { } registro)
            return;

        var confirma = await Ui.ConfirmarAsync(this, "Confirmar",
            $"¿Desea desactivar \"{Describir(registro)}\"?\nEl registro se conserva en el historial y puede reactivarse.");
        if (!confirma)
            return;

        if (await Ui.EjecutarAsync(this, () => Eliminar(registro)))
            await RecargarAsync();
    }
}
