using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Sigbim.BLL;
using Sigbim.Entidades;

namespace Sigbim.UI;

/// <summary>
/// Utilidades de la capa de presentación: construcción de formularios,
/// validación de formato de los campos y manejo centralizado de errores.
/// </summary>
public static class Ui
{
    // ---------- Construcción de controles ----------

    public static TextBox Texto(int largoMaximo, string? marcaAgua = null) =>
        new() { MaxLength = largoMaximo, Watermark = marcaAgua };

    public static TextBox TextoLargo(int largoMaximo) =>
        new() { MaxLength = largoMaximo, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 70 };

    /// <summary>Arma un formulario de dos columnas: etiqueta | control.</summary>
    public static Grid Formulario(params (string Etiqueta, Control Control)[] filas)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions(string.Join(",", filas.Select(_ => "Auto")))
        };

        for (var i = 0; i < filas.Length; i++)
        {
            var etiqueta = new TextBlock
            {
                Text = filas[i].Etiqueta,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 12, 5)
            };
            Grid.SetRow(etiqueta, i);
            grid.Children.Add(etiqueta);

            var control = filas[i].Control;
            control.Margin = new Thickness(0, 5);
            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(control, i);
            Grid.SetColumn(control, 1);
            grid.Children.Add(control);
        }

        return grid;
    }

    // ---------- Validación de formato en la UI ----------

    /// <summary>Lee un número decimal aceptando "1500", "1500,5", "1.500,50" o "1500.5".</summary>
    public static decimal LeerDecimal(TextBox caja, string campo, List<string> errores)
    {
        var texto = (caja.Text ?? string.Empty).Trim().Replace(" ", "").Replace("$", "");
        if (texto.Length == 0)
        {
            errores.Add($"{campo} es obligatorio.");
            return 0;
        }

        if (texto.Contains('.') && texto.Contains(','))
            texto = texto.Replace(".", "");          // 1.500,50 -> 1500,50
        texto = texto.Replace(',', '.');             // 1500,50  -> 1500.50

        if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
            return valor;

        errores.Add($"{campo} debe ser un número válido.");
        return 0;
    }

    public static int LeerEntero(TextBox caja, string campo, List<string> errores)
    {
        var texto = (caja.Text ?? string.Empty).Trim().Replace(".", "");
        if (texto.Length == 0)
        {
            errores.Add($"{campo} es obligatorio.");
            return 0;
        }

        if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor))
            return valor;

        errores.Add($"{campo} debe ser un número entero.");
        return 0;
    }

    public static string Numero(decimal valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);

    // ---------- Mensajes ----------

    public static Task MensajeAsync(Window propietario, string titulo, string texto) =>
        new Dialogo(titulo, texto, confirmar: false).ShowDialog(propietario);

    public static Task<bool> ConfirmarAsync(Window propietario, string titulo, string texto) =>
        new Dialogo(titulo, texto, confirmar: true).ShowDialog<bool>(propietario);

    /// <summary>
    /// Ejecuta una acción y traduce cualquier excepción a un mensaje para el usuario.
    /// Devuelve true si la acción terminó sin errores.
    /// </summary>
    public static async Task<bool> EjecutarAsync(Window propietario, Action accion)
    {
        try
        {
            accion();
            return true;
        }
        catch (ValidacionException ex)
        {
            await MensajeAsync(propietario, "Revise los datos", "• " + string.Join(Environment.NewLine + "• ", ex.Errores));
        }
        catch (DatosException ex)
        {
            await MensajeAsync(propietario, "Error de base de datos", ex.Message);
        }
        catch (Exception ex)
        {
            RegistroEventos.Error("Interfaz", ex);
            await MensajeAsync(propietario, "Error inesperado",
                "Ocurrió un error inesperado. El detalle quedó registrado en la carpeta logs.");
        }

        return false;
    }
}
