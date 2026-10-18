using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Sigbim.BLL;

namespace Sigbim.UI.Ventanas;

/// <summary>Menú principal del sistema.</summary>
public class VentanaPrincipal : Window
{
    private readonly TextBlock _lblConexion = new() { Opacity = 0.8, Margin = new Thickness(0, 16, 0, 0), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center };
    private readonly StackPanel _menu = new() { Spacing = 10, IsEnabled = false };

    public VentanaPrincipal()
    {
        Title = "SIGBIM - Inventario de insumos médicos";
        Width = 560;
        Height = 520;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _menu.Children.Add(Opcion("Productos", () => new VentanaProductos(soloLectura: true)));
        _menu.Children.Add(Opcion("Movimientos de stock", () => new VentanaMovimientos()));
        _menu.Children.Add(Opcion("Artículos y categorías", () => new VentanaArticulosCategorias()));
        _menu.Children.Add(Opcion("Proveedores", () => new VentanaProveedores()));

        Content = new StackPanel
        {
            Margin = new Thickness(40),
            HorizontalAlignment = HorizontalAlignment.Center,
            Children =
            {
                new TextBlock { Text = "SIGBIM", FontSize = 34, FontWeight = FontWeight.Bold, HorizontalAlignment = HorizontalAlignment.Center },
                new TextBlock { Text = "Sistema de Gestión de Bodega de Insumos Médicos", Opacity = 0.8, HorizontalAlignment = HorizontalAlignment.Center },
                new TextBlock { Text = $"Usuario: {Sesion.UsuarioActual}", Opacity = 0.6, Margin = new Thickness(0, 4, 0, 28), HorizontalAlignment = HorizontalAlignment.Center },
                _menu,
                _lblConexion
            }
        };

        Opened += async (_, _) => await VerificarConexionAsync();
    }

    private Button Opcion(string texto, Func<Window> crearVentana)
    {
        var boton = new Button
        {
            Content = texto,
            Width = 300,
            Height = 44,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        boton.Click += (_, _) => crearVentana().Show(this);
        return boton;
    }

    private async Task VerificarConexionAsync()
    {
        _lblConexion.Text = "Conectando con la base de datos...";
        var ok = await Ui.EjecutarAsync(this, SistemaBLL.ProbarConexion);
        _menu.IsEnabled = ok;
        _lblConexion.Text = ok
            ? "● Conectado a la base de datos"
            : "Sin conexión. Revisa appsettings.json y que MySQL esté encendido, luego reinicia la aplicación.";
    }
}
