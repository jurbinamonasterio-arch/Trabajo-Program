using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Sigbim.BLL;
using Sigbim.Entidades;
using Sigbim.UI.Reportes;

namespace Sigbim.UI.Ventanas;

/// <summary>
/// Consulta de la bitácora (inicios de sesión, creaciones, modificaciones,
/// desactivaciones y movimientos) y descarga del reporte en PDF.
/// </summary>
public class VentanaBitacora : Window
{
    private readonly BitacoraBLL _bll = new();
    private List<EventoBitacora> _eventos = new();

    private readonly DataGrid _grilla = new()
    {
        IsReadOnly = true,
        AutoGenerateColumns = false,
        GridLinesVisibility = DataGridGridLinesVisibility.Horizontal
    };
    private readonly TextBlock _lblEstado = new() { Margin = new Thickness(12, 6), Opacity = 0.7 };

    public VentanaBitacora()
    {
        Title = "SIGBIM - Bitácora y reporte PDF";
        Width = 1100;
        Height = 650;
        MinWidth = 800;
        MinHeight = 450;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Columna("Fecha", nameof(EventoBitacora.Fecha), "{0:dd-MM-yyyy HH:mm:ss}");
        Columna("Nivel", nameof(EventoBitacora.Nivel));
        Columna("Usuario", nameof(EventoBitacora.Usuario));
        Columna("Operación", nameof(EventoBitacora.Operacion));
        Columna("Detalle", nameof(EventoBitacora.Detalle));

        var btnRefrescar = new Button { Content = "Refrescar" };
        var btnPdf = new Button { Content = "Descargar PDF" };
        btnPdf.Classes.Add("accent");
        btnRefrescar.Click += async (_, _) => await RecargarAsync();
        btnPdf.Click += async (_, _) => await DescargarPdfAsync();

        var barra = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(12),
            Children = { btnRefrescar, btnPdf }
        };

        var dock = new DockPanel();
        DockPanel.SetDock(barra, Dock.Top);
        DockPanel.SetDock(_lblEstado, Dock.Bottom);
        dock.Children.Add(barra);
        dock.Children.Add(_lblEstado);
        dock.Children.Add(_grilla);
        Content = dock;

        Opened += async (_, _) => await RecargarAsync();
    }

    private void Columna(string encabezado, string propiedad, string? formato = null) =>
        _grilla.Columns.Add(new DataGridTextColumn
        {
            Header = encabezado,
            Binding = new Binding(propiedad) { StringFormat = formato }
        });

    private async Task RecargarAsync()
    {
        await Ui.EjecutarAsync(this, () =>
        {
            _eventos = _bll.Listar();
            // En pantalla se muestra primero lo más reciente; el PDF va en orden cronológico
            _grilla.ItemsSource = _eventos.AsEnumerable().Reverse().ToList();
            _lblEstado.Text = $"{_eventos.Count} evento(s) registrados";
        });
    }

    private async Task DescargarPdfAsync()
    {
        await RecargarAsync();

        var archivo = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Guardar reporte de bitácora",
            SuggestedFileName = $"SIGBIM_Bitacora_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
            DefaultExtension = "pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } }
        });
        if (archivo?.TryGetLocalPath() is not { } ruta)
            return;   // el usuario canceló

        var ok = await Ui.EjecutarAsync(this, () =>
        {
            ReporteBitacoraPdf.Generar(ruta, _eventos);
            _bll.RegistrarExportacion(ruta, _eventos.Count);
        });
        if (!ok)
            return;

        // Abre el PDF con el visor predeterminado (Vista Previa en macOS)
        Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        await RecargarAsync();
    }
}
