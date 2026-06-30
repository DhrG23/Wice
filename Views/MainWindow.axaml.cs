using Avalonia.Controls;
using Wice.Views; // Ensure access to TopBarWindow

namespace Wice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 1. Instantiate the shell component
        var topBar = new TopBarWindow();

        // 2. Show it as a separate persistent window
        topBar.Show();

        // 3. Close this placeholder MainWindow to leave only the shell
        this.Close();
    }
}