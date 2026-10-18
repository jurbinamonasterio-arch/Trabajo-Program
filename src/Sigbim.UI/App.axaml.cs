using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Sigbim.BLL;
using Sigbim.UI.Ventanas;

namespace Sigbim.UI;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // Último recurso: cualquier excepción no controlada queda en la bitácora
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                RegistroEventos.Error("Excepción no controlada", ex);
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime escritorio)
            escritorio.MainWindow = new VentanaPrincipal();

        base.OnFrameworkInitializationCompleted();
    }
}
