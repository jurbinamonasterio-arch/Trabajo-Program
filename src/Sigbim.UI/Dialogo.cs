using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Sigbim.UI;

/// <summary>Ventana modal simple para avisos y confirmaciones (Avalonia no trae MessageBox).</summary>
public class Dialogo : Window
{
    public Dialogo(string titulo, string texto, bool confirmar)
    {
        Title = titulo;
        Width = 440;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var mensaje = new TextBlock
        {
            Text = texto,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var botones = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        if (confirmar)
        {
            var no = new Button { Content = "No", MinWidth = 90, IsCancel = true };
            no.Click += (_, _) => Close(false);
            botones.Children.Add(no);
        }

        var si = new Button { Content = confirmar ? "Sí" : "Aceptar", MinWidth = 90, IsDefault = true };
        si.Classes.Add("accent");
        si.Click += (_, _) => Close(true);
        botones.Children.Add(si);

        Content = new StackPanel
        {
            Margin = new Thickness(24),
            Children = { mensaje, botones }
        };
    }
}
