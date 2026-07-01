using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Wice.Views;

namespace Wice;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Detect all monitors and launch a bar on each
            var screens = new Window().Screens.All;
            foreach (var screen in screens)
            {
                var topBar = new TopBarWindow(screen);
                topBar.Show();
            }

            // Set one as the MainWindow so the app doesn't immediately close
            // (or set ShutdownMode to OnExplicitShutdown in App.axaml)
        }
        base.OnFrameworkInitializationCompleted();
    }
}