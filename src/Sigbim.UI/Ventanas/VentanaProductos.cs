using Avalonia.Controls;
using Avalonia.Media;
using Sigbim.BLL;
using Sigbim.Entidades;

namespace Sigbim.UI.Ventanas;

/// <summary>Mantenedor CRUD de productos (lectura desde la vista vw_productodetalle).</summary>
public class VentanaProductos : VentanaMantenedor<ProductoDetalle>
{
    private readonly ProductoBLL _bll = new();
    private readonly CategoriaBLL _categoriaBll = new();
    private readonly ProveedorBLL _proveedorBll = new();
    private List<Categoria> _categorias = new();
    private List<Proveedor> _proveedores = new();

    private readonly TextBox _txtSku = Ui.Texto(20, "Ej: GUA-001");
    private readonly TextBox _txtNombre = Ui.Texto(120);
    private readonly TextBox _txtDescripcion = Ui.TextoLargo(250);
    private readonly ComboBox _cmbCategoria = new() { PlaceholderText = "Seleccione..." };
    private readonly ComboBox _cmbProveedor = new() { PlaceholderText = "Seleccione..." };
    private readonly TextBox _txtPrecioCompra = Ui.Texto(15, "0");
    private readonly TextBox _txtPrecioVenta = Ui.Texto(15, "0");
    private readonly TextBox _txtStockMinimo = Ui.Texto(9, "0");
    private readonly TextBlock _lblStock = new() { FontWeight = FontWeight.SemiBold };
    private readonly CalendarDatePicker _dtpVencimiento = new() { Watermark = "Sin vencimiento" };
    private readonly CheckBox _chkReceta = new() { Content = "Requiere receta" };
    private readonly CheckBox _chkActivo = new() { Content = "Activo" };

    public VentanaProductos() : base("Productos", conBuscador: true)
    {
        Inicializar();
    }

    protected override void ConfigurarColumnas()
    {
        Columna("SKU", nameof(ProductoDetalle.Sku));
        Columna("Producto", nameof(ProductoDetalle.Nombre));
        Columna("Categoría", nameof(ProductoDetalle.Categoria));
        Columna("Proveedor", nameof(ProductoDetalle.Proveedor));
        Columna("P. compra", nameof(ProductoDetalle.PrecioCompra), "{0:N0}");
        Columna("P. venta", nameof(ProductoDetalle.PrecioVenta), "{0:N0}");
        Columna("Stock", nameof(ProductoDetalle.Stock));
        Columna("Mínimo", nameof(ProductoDetalle.StockMinimo));
        Columna("Estado stock", nameof(ProductoDetalle.EstadoStock));
        Columna("Vence", nameof(ProductoDetalle.FechaVencimiento), "{0:dd-MM-yyyy}");
        Columna("Estado", nameof(ProductoDetalle.EstadoTexto));
    }

    protected override Control CrearFormulario() => Ui.Formulario(
        ("SKU *", _txtSku),
        ("Nombre *", _txtNombre),
        ("Descripción", _txtDescripcion),
        ("Categoría *", _cmbCategoria),
        ("Proveedor *", _cmbProveedor),
        ("Precio compra *", _txtPrecioCompra),
        ("Precio venta *", _txtPrecioVenta),
        ("Stock mínimo *", _txtStockMinimo),
        ("Stock actual", _lblStock),
        ("Vencimiento", _dtpVencimiento),
        ("", _chkReceta),
        ("", _chkActivo));

    protected override void CargarListasAuxiliares()
    {
        _categorias = _categoriaBll.Listar(soloActivos: true);
        _proveedores = _proveedorBll.Listar(soloActivos: true);
        _cmbCategoria.ItemsSource = _categorias;
        _cmbProveedor.ItemsSource = _proveedores;
    }

    protected override IEnumerable<ProductoDetalle> Cargar(string? buscar, bool incluirInactivos) =>
        _bll.Listar(buscar, soloActivos: !incluirInactivos);

    protected override void MostrarEnFormulario(ProductoDetalle? p)
    {
        _txtSku.Text = p?.Sku ?? string.Empty;
        _txtNombre.Text = p?.Nombre ?? string.Empty;
        _txtDescripcion.Text = p?.Descripcion ?? string.Empty;
        _cmbCategoria.SelectedItem = p is null ? null : _categorias.FirstOrDefault(c => c.IdCategoria == p.IdCategoria);
        _cmbProveedor.SelectedItem = p is null ? null : _proveedores.FirstOrDefault(x => x.IdProveedor == p.IdProveedor);
        _txtPrecioCompra.Text = p is null ? string.Empty : Ui.Numero(p.PrecioCompra);
        _txtPrecioVenta.Text = p is null ? string.Empty : Ui.Numero(p.PrecioVenta);
        _txtStockMinimo.Text = p?.StockMinimo.ToString() ?? string.Empty;
        _lblStock.Text = p is null
            ? "0 (se carga con un movimiento de ENTRADA)"
            : $"{p.Stock} unidades — {p.EstadoStock}";
        _dtpVencimiento.SelectedDate = p?.FechaVencimiento;
        _chkReceta.IsChecked = p?.RequiereReceta ?? false;
        _chkActivo.IsChecked = p?.Activo ?? true;
        _chkActivo.IsEnabled = p is not null;
    }

    protected override int Guardar()
    {
        // Validación de formato en la UI (los campos numéricos deben ser números)
        var errores = new List<string>();
        var precioCompra = Ui.LeerDecimal(_txtPrecioCompra, "Precio de compra", errores);
        var precioVenta = Ui.LeerDecimal(_txtPrecioVenta, "Precio de venta", errores);
        var stockMinimo = Ui.LeerEntero(_txtStockMinimo, "Stock mínimo", errores);
        if (errores.Count > 0)
            throw new ValidacionException(errores);

        var producto = new Producto
        {
            IdProducto = Actual?.IdProducto ?? 0,
            Version = Actual?.Version ?? 1,
            Sku = _txtSku.Text ?? string.Empty,
            Nombre = _txtNombre.Text ?? string.Empty,
            Descripcion = _txtDescripcion.Text,
            IdCategoria = (_cmbCategoria.SelectedItem as Categoria)?.IdCategoria ?? 0,
            IdProveedor = (_cmbProveedor.SelectedItem as Proveedor)?.IdProveedor ?? 0,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            StockMinimo = stockMinimo,
            FechaVencimiento = _dtpVencimiento.SelectedDate,
            RequiereReceta = _chkReceta.IsChecked == true,
            Activo = _chkActivo.IsChecked == true
        };
        return _bll.Guardar(producto);
    }

    protected override void Eliminar(ProductoDetalle p) => _bll.Eliminar(p.IdProducto);
    protected override int ObtenerId(ProductoDetalle p) => p.IdProducto;
    protected override string Describir(ProductoDetalle p) => $"{p.Sku} - {p.Nombre}";
}
