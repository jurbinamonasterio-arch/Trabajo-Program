using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Sigbim.UI.Ventanas;

/// <summary>
/// Punto único de administración del catálogo: aquí se crean, modifican y desactivan
/// los artículos (productos), y dentro se encuentra el mantenedor de categorías.
/// El menú "Productos" queda solo como consulta.
/// </summary>
public class VentanaArticulosCategorias : Window
{
    public VentanaArticulosCategorias()
    {
        Title = "SIGBIM - Artículos y categorías";
        Width = 480;
        Height = 340;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Content = new StackPanel
        {
            Margin = new Thickness(32),
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children =
            {
                new TextBlock { Text = "Artículos y categorías", FontSize = 22, FontWeight = FontWeight.SemiBold, HorizontalAlignment = HorizontalAlignment.Center },
                new TextBlock { Text = "Creación, modificación y desactivación del catálogo", Opacity = 0.7, Margin = new Thickness(0, 0, 0, 16), HorizontalAlignment = HorizontalAlignment.Center },
                Opcion("Gestión de artículos", () => new VentanaProductos()),
                Opcion("Categorías", () => new VentanaCategorias())
            }
        };
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
}
