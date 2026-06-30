using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Wice.Views; // Ensure this matches your folder

namespace Wice;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Set the TopBarWindow as the primary window instead of MainWindow
            desktop.MainWindow = new TopBarWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}