using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
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

        // Use physical pixels (PixelPoint) for the initial position
        // TopLeft is logical, so multiply by Scaling for the PixelPoint
        var physicalX = (int)(_targetScreen.Bounds.X * _targetScreen.Scaling);
        var physicalY = (int)(_targetScreen.Bounds.Y * _targetScreen.Scaling);
        
        this.Position = new PixelPoint(physicalX, physicalY);
        this.Width = _targetScreen.Bounds.Width; // Avalonia Width is logical
        this.Height = DesiredLogicalHeight;     // Avalonia Height is logical

        this.Opened += (s, e) => RegisterAsSystemBar();
    }

    private void RegisterAsSystemBar()
    {
        var hwnd = this.TryGetPlatformHandle()?.Handle;
        if (hwnd == null) return;

        // Get the scaling factor (e.g., 1.5 for 150%)
        double scale = _targetScreen.Scaling;

        var abd = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = hwnd.Value,
            uEdge = 1 // ABE_TOP
        };

        // 1. Register the bar
        NativeMethods.SHAppBarMessage(0, ref abd); // ABM_NEW

        // 2. Calculate PHYSICAL RECT for Win32
        // We must multiply Avalonia logical pixels by the scale factor
        int physicalTop = (int)(_targetScreen.Bounds.Y * scale);
        int physicalLeft = (int)(_targetScreen.Bounds.X * scale);
        int physicalRight = physicalLeft + (int)(_targetScreen.Bounds.Width * scale);
        int physicalBottom = physicalTop + (int)(40 * scale); // 40 is our logical height

        abd.rc = new NativeMethods.RECT
        {
            Left = physicalLeft,
            Top = physicalTop,
            Right = physicalRight,
            Bottom = physicalBottom
        };

        // 3. Negotiate space (This handles shifting if another bar exists)
        NativeMethods.SHAppBarMessage(2, ref abd); // ABM_QUERYPOS
        NativeMethods.SHAppBarMessage(3, ref abd); // ABM_SETPOS

        // 4. Update Avalonia Window using the approved PHYSICAL coordinates
        // PixelPoint is physical, so we use abd.rc directly
        this.Position = new PixelPoint(abd.rc.Left, abd.rc.Top);

        // Width and Height in Avalonia are logical, so we divide back
        this.Width = (abd.rc.Right - abd.rc.Left) / scale;
        this.Height = (abd.rc.Bottom - abd.rc.Top) / scale;
    }
}