using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System.Runtime.InteropServices;
using Wice.Native;


namespace Wice.Views;


public partial class TopBarWindow : Window
{
    private readonly Screen _targetScreen;


    public TopBarWindow(Screen screen)
    {
        InitializeComponent();
        _targetScreen = screen;

        // Pin to the correct monitor's coordinates
        this.Position = _targetScreen.Bounds.Center;
        this.Width = _targetScreen.Bounds.Width;
        this.Height = 40;

        this.Opened += (s, e) => RegisterAsSystemBar();
    }

    private void RegisterAsSystemBar()
    {
        var hwnd = this.TryGetPlatformHandle()?.Handle;
        if (hwnd == null) return;

        var abd = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = hwnd.Value,
            uEdge = 1 // ABE_TOP
        };

        // Tell Windows: "I am a new AppBar"
        NativeMethods.SHAppBarMessage(0, ref abd); // ABM_NEW

        // Define your desired area
        abd.rc = new NativeMethods.RECT
        {
            Left = _targetScreen.Bounds.X,
            Top = _targetScreen.Bounds.Y,
            Right = _targetScreen.Bounds.X + (int)this.Width,
            Bottom = _targetScreen.Bounds.Y + 40
        };

        // Ask Windows: "Is this space okay?"
        // IMPORTANT: Windows might change abd.rc here if it needs to shift you!
        NativeMethods.SHAppBarMessage(2, ref abd); // ABM_QUERYPOS

        // Tell Windows: "I am taking this (possibly adjusted) space"
        NativeMethods.SHAppBarMessage(3, ref abd); // ABM_SETPOS

        // Update the Avalonia window to match what Windows approved
        this.Position = new PixelPoint(abd.rc.Left, abd.rc.Top);
        this.Height = abd.rc.Bottom - abd.rc.Top;
    }

}