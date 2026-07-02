using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Wice.Views;

namespace Wice;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;


            // Detect all monitors and launch a bar on each
            var provider = TopLevel.GetTopLevel(null);
            if (provider?.Screens != null)
            {
                foreach (var screen in provider.Screens.All)
                {
                    var topBar = new TopBarWindow(screen);
                    topBar.Show();
                }
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    public void OnSettingsClick(object? sender, EventArgs e)
    {
        // Open settings window
    }

    public void OnRestartClick(object? sender, EventArgs e)
    {
        // Restart logic
    }

    public void OnExitClick(object? sender, EventArgs e)
    {
        // Properly shut down the app since ShutdownMode is Explicit
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}