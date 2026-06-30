using Avalonia.Controls;
using Wice.Views; // Ensure access to TopBarWindow

namespace Wice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Create and show the bar as its own top-level window
        var topBar = new TopBarWindow();
        topBar.Show();

        // Close the manager window if it's no longer needed
        this.Close();
    }
}