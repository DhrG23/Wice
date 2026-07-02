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

        // Use Logical pixels for Avalonia properties
        this.Width = _targetScreen.Bounds.Width / _targetScreen.Scaling;
        this.Height = DesiredLogicalHeight;

        // Use Physical pixels for initial Position
        this.Position = _targetScreen.Bounds.TopLeft;

        this.Opened += (s, e) => RegisterAsSystemBar();
    }

    private void RegisterAsSystemBar()
    {
        var hwnd = this.TryGetPlatformHandle()?.Handle;
        if (hwnd == null) return;

        // Hide from Alt+Tab & taskbar
        IntPtr currentStyle = NativeMethods.GetWindowLongPtr(hwnd.Value, NativeMethods.GWL_EXSTYLE);

        NativeMethods.SetWindowLongPtr(hwnd.Value, NativeMethods.GWL_EXSTYLE,
            currentStyle | (IntPtr)NativeMethods.WS_EX_TOOLWINDOW);

        var abd = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = hwnd.Value,
            uEdge = NativeMethods.ABE_TOP,
            // Define the EXACT physical space to reserve
            rc = MonitorHelper.GetPhysicalRect(_targetScreen, DesiredLogicalHeight)
        };

        NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref abd);
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);

        // Sync Avalonia window to what Windows actually approved
        this.Position = new PixelPoint(abd.rc.Left, abd.rc.Top);
        this.Height = MonitorHelper.ToLogical(Math.Abs(abd.rc.Bottom-abd.rc.Top), _targetScreen.Scaling);
    }
}
