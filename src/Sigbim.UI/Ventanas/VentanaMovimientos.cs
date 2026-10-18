using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Sigbim.BLL;
using Sigbim.Entidades;

namespace Sigbim.UI.Ventanas;

/// <summary>
/// Registro de entradas, salidas y ajustes de stock, con el historial (kardex)
/// del producto seleccionado.
/// </summary>
public class VentanaMovimientos : Window
{
    private readonly MovimientoStockBLL _bll = new();
    private readonly ProductoBLL _productoBll = new();
    private List<ProductoDetalle> _productos = new();

    private readonly ComboBox _cmbProducto = new() { PlaceholderText = "Seleccione un producto..." };
    private readonly ComboBox _cmbTipo = new() { ItemsSource = Enum.GetValues<TipoMovimiento>() };
    private readonly TextBox _txtCantidad = Ui.Texto(9, "0");
    private readonly TextBox _txtMotivo = Ui.TextoLargo(200);
    private readonly TextBox _txtDocumento = Ui.Texto(40, "Ej: FAC-1001, GD-2001");
    private readonly TextBlock _lblStock = new() { FontSize = 16, FontWeight = FontWeight.SemiBold };
    private readonly TextBlock _lblAyuda = new() { TextWrapping = TextWrapping.Wrap, Opacity = 0.7, FontSize = 12 };
    private readonly CheckBox _chkTodos = new() { Content = "Ver movimientos de todos los productos" };
    private readonly DataGrid _grilla = new()
    {
        IsReadOnly = true,
        AutoGenerateColumns = false,
        GridLinesVisibility = DataGridGridLinesVisibility.Horizontal
    };

    public VentanaMovimientos()
    {
        Title = "SIGBIM - Movimientos de stock";
        Width = 1180;
        Height = 700;
        MinWidth = 900;
        MinHeight = 520;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        ConfigurarGrilla();
        Content = ConstruirLayout();

        _cmbTipo.SelectedItem = TipoMovimiento.ENTRADA;
        _cmbTipo.SelectionChanged += (_, _) => ActualizarAyuda();
        _cmbProducto.SelectionChanged += async (_, _) => await MostrarProductoAsync();
        _chkTodos.Click += async (_, _) => await CargarHistorialAsync();
        ActualizarAyuda();

        Opened += async (_, _) => await CargarProductosAsync(null);
    }

    private void ConfigurarGrilla()
    {
        void Col(string encabezado, string propiedad, string? formato = null) =>
            _grilla.Columns.Add(new DataGridTextColumn
            {
                Header = encabezado,
                Binding = new Binding(propiedad) { StringFormat = formato }
            });

        Col("Fecha", nameof(MovimientoStock.Fecha), "{0:dd-MM-yyyy HH:mm}");
        Col("SKU", nameof(MovimientoStock.Sku));
        Col("Producto", nameof(MovimientoStock.Producto));
        Col("Tipo", nameof(MovimientoStock.Tipo));
        Col("Cantidad", nameof(MovimientoStock.Cantidad));
        Col("Stock anterior", nameof(MovimientoStock.StockAnterior));
        Col("Stock nuevo", nameof(MovimientoStock.StockNuevo));
        Col("Motivo", nameof(MovimientoStock.Motivo));
        Col("Documento", nameof(MovimientoStock.DocumentoReferencia));
        Col("Usuario", nameof(MovimientoStock.UsuarioResponsable));
    }

    private Control ConstruirLayout()
    {
        var btnRegistrar = new Button { Content = "Registrar movimiento", Margin = new Thickness(0, 16, 0, 0) };
        btnRegistrar.Classes.Add("accent");
        btnRegistrar.Click += async (_, _) => await RegistrarAsync();

        var formulario = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "Nuevo movimiento", FontSize = 18, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 12) },
                Ui.Formulario(
                    ("Producto *", _cmbProducto),
                    ("Stock actual", _lblStock),
                    ("Tipo *", _cmbTipo),
                    ("Cantidad *", _txtCantidad),
                    ("Motivo", _txtMotivo),
                    ("Documento", _txtDocumento)),
                _lblAyuda,
                btnRegistrar
            }
        };

        var panelFormulario = new Border
        {
            Width = 420,
            Padding = new Thickness(16),
            BorderThickness = new Thickness(1, 0, 0, 0),
            BorderBrush = Brushes.Gray,
            Child = new ScrollViewer { Content = formulario }
        };

        var encabezado = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Margin = new Thickness(12),
            Children =
            {
                new TextBlock { Text = "Historial (kardex)", FontSize = 18, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center },
                _chkTodos
            }
        };

        var dock = new DockPanel();
        DockPanel.SetDock(panelFormulario, Dock.Right);
        DockPanel.SetDock(encabezado, Dock.Top);
        dock.Children.Add(panelFormulario);
        dock.Children.Add(encabezado);
        dock.Children.Add(_grilla);
        return dock;
    }

    private void ActualizarAyuda()
    {
        _lblAyuda.Text = (_cmbTipo.SelectedItem as TipoMovimiento?) switch
        {
            TipoMovimiento.ENTRADA => "ENTRADA: suma la cantidad al stock (compras, devoluciones de clientes).",
            TipoMovimiento.SALIDA => "SALIDA: resta la cantidad del stock. Requiere motivo. No permite dejar stock negativo.",
            TipoMovimiento.AJUSTE => "AJUSTE: fija el stock en la cantidad indicada según el conteo físico. Requiere motivo.",
            _ => string.Empty
        };
    }

    private ProductoDetalle? ProductoSeleccionado => _cmbProducto.SelectedItem as ProductoDetalle;

    private async Task CargarProductosAsync(int? seleccionarId)
    {
        await Ui.EjecutarAsync(this, () =>
        {
            _productos = _productoBll.Listar(soloActivos: true);
            _cmbProducto.ItemsSource = _productos;
            _cmbProducto.SelectedItem = seleccionarId is null
                ? null
                : _productos.FirstOrDefault(p => p.IdProducto == seleccionarId);
        });
        await MostrarProductoAsync();
    }

    private async Task MostrarProductoAsync()
    {
        var p = ProductoSeleccionado;
        _lblStock.Text = p is null ? "-" : $"{p.Stock} unidades ({p.EstadoStock})";
        await CargarHistorialAsync();
    }

    private async Task CargarHistorialAsync()
    {
        await Ui.EjecutarAsync(this, () =>
        {
            int? id = _chkTodos.IsChecked == true ? null : ProductoSeleccionado?.IdProducto;
            _grilla.ItemsSource = id is null && _chkTodos.IsChecked != true
                ? new List<MovimientoStock>()
                : _bll.Listar(id);
        });
    }

    private async Task RegistrarAsync()
    {
        var producto = ProductoSeleccionado;
        var stockNuevo = 0;

        var ok = await Ui.EjecutarAsync(this, () =>
        {
            var errores = new List<string>();
            var cantidad = Ui.LeerEntero(_txtCantidad, "Cantidad", errores);
            if (_cmbTipo.SelectedItem is not TipoMovimiento)
                errores.Add("Debe seleccionar el tipo de movimiento.");
            if (errores.Count > 0)
                throw new ValidacionException(errores);

            stockNuevo = _bll.Registrar(new MovimientoStock
            {
                IdProducto = producto?.IdProducto ?? 0,
                Tipo = (TipoMovimiento)_cmbTipo.SelectedItem!,
                Cantidad = cantidad,
                Motivo = _txtMotivo.Text,
                DocumentoReferencia = _txtDocumento.Text
            });
        });

        if (!ok || producto is null)
            return;

        await Ui.MensajeAsync(this, "Movimiento registrado",
            $"{producto.Nombre}: stock anterior {producto.Stock}, stock nuevo {stockNuevo}.");

        _txtCantidad.Text = string.Empty;
        _txtMotivo.Text = string.Empty;
        _txtDocumento.Text = string.Empty;
        await CargarProductosAsync(producto.IdProducto);
    }
}
