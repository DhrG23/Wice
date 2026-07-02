using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using Wice.Native;

namespace Wice.Views;

public partial class TopBarWindow : Window
{
    private readonly Screen _targetScreen;
    private const int DesiredLogicalHeight = 40;

    public TopBarWindow(Screen screen)
    {
        InitializeComponent();
        _targetScreen = screen;

        // Set initial Logical dimensions
        this.Width = _targetScreen.Bounds.Width / _targetScreen.Scaling;
        this.Height = DesiredLogicalHeight;

        // Initial physical position
        this.Position = _targetScreen.Bounds.TopLeft;

        this.Opened += (s, e) => RegisterAsSystemBar();
    }

    private void RegisterAsSystemBar()
    {
        // 1. Get the handle FIRST so it can be used for styles
        var hwnd = this.TryGetPlatformHandle()?.Handle;
        if (hwnd == null) return;

        // 2. Set ToolWindow style for Virtual Desktop persistence
        IntPtr currentStyle = NativeMethods.GetWindowLongPtr(hwnd.Value, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLongPtr(hwnd.Value, NativeMethods.GWL_EXSTYLE, currentStyle | (IntPtr)NativeMethods.WS_EX_TOOLWINDOW);

        double scale = _targetScreen.Scaling;
        var abd = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = hwnd.Value,
            uEdge = NativeMethods.ABE_TOP
        };

        // 3. Register with Shell
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref abd);

        // 4. Define PHYSICAL area (Bounds is already physical)
        int physicalHeight = (int)(DesiredLogicalHeight * scale);
        abd.rc = new NativeMethods.RECT
        {
            Left = _targetScreen.Bounds.X,
            Top = _targetScreen.Bounds.Y,
            Right = _targetScreen.Bounds.Right,
            Bottom = _targetScreen.Bounds.Y + physicalHeight
        };

        // 5. Negotiate and Set Position (Handshake)
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);

        // 6. Update Avalonia Window to match APPROVED space
        this.Position = new PixelPoint(abd.rc.Left, abd.rc.Top);
        this.Height = (abd.rc.Bottom - abd.rc.Top) / scale; // Convert physical back to logical
    }
}
