using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using Wice.Native;


namespace Wice.Views;


public partial class TopBarWindow : Window
{
    // Desired bar height in device-independent units (DIPs). This is what you should
    // tweak to change the bar's visual thickness.
    private const int BarHeightDip = 40;

    private readonly Screen _targetScreen;
    private IntPtr _hwnd;
    private bool _registered;


    public TopBarWindow(Screen screen)
    {
        InitializeComponent();
        _targetScreen = screen;

        // Pin to the correct monitor's top-left corner.
        // Window.Position is PixelPoint == physical pixels, so no conversion needed here.
        this.Position = _targetScreen.Bounds.Position;

        // Window.Width/Height are DIPs, but Screen.Bounds is physical pixels.
        // On any monitor that isn't running at 100% scaling, assigning physical values
        // directly makes the window the wrong size, so we divide by the scale factor.
        var startupScaling = _targetScreen.Scaling <= 0 ? 1.0 : _targetScreen.Scaling;
        this.Width = _targetScreen.Bounds.Width / startupScaling;
        this.Height = BarHeightDip;

        this.Opened += (s, e) => RegisterAsSystemBar();
        this.Closing += (s, e) => UnregisterSystemBar();
    }

    private void RegisterAsSystemBar()
    {
        var handle = this.TryGetPlatformHandle()?.Handle;
        if (handle == null) return;
        _hwnd = handle.Value;

        // RenderScaling is the actual DIP <-> physical-pixel ratio Avalonia is using to
        // paint *this* window on *this* monitor (per-monitor DPI aware). We must use the
        // same ratio in both directions so the physical rect Windows reserves and the
        // physical size Avalonia actually renders always agree - this is the fix for the
        // "bar gets clipped by maximized windows" bug.
        var scaling = this.RenderScaling <= 0 ? 1.0 : this.RenderScaling;

        var abd = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = _hwnd,
            uEdge = NativeMethods.ABE_TOP
        };

        // Tell Windows: "I am a new AppBar"
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref abd);

        // Define the desired area entirely in physical pixels (Screen.Bounds is physical,
        // and so is RECT - never mix in `this.Width`/`this.Height` here, those are DIPs).
        var barHeightPx = (int)Math.Round(BarHeightDip * scaling);
        abd.rc = new NativeMethods.RECT
        {
            Left = _targetScreen.Bounds.X,
            Top = _targetScreen.Bounds.Y,
            Right = _targetScreen.Bounds.X + _targetScreen.Bounds.Width,
            Bottom = _targetScreen.Bounds.Y + barHeightPx
        };

        // Ask Windows: "Is this space okay?"
        // IMPORTANT: Windows might change abd.rc here if it needs to shift you!
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);

        // Tell Windows: "I am taking this (possibly adjusted) space"
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);

        // Update the Avalonia window to match what Windows approved.
        // Position is physical (no conversion). Width/Height are DIPs, so convert the
        // approved physical rect back down using the SAME scale factor used above -
        // this keeps the reserved work-area strip and the rendered bar pixel-for-pixel
        // in sync regardless of monitor DPI.
        this.Position = new PixelPoint(abd.rc.Left, abd.rc.Top);
        this.Width = (abd.rc.Right - abd.rc.Left) / scaling;
        this.Height = (abd.rc.Bottom - abd.rc.Top) / scaling;

        _registered = true;
    }

    private void UnregisterSystemBar()
    {
        // Without this, closing/restarting the app (very common while ricing/iterating)
        // leaves a permanently reserved gap in the desktop work area until Explorer restarts.
        if (!_registered || _hwnd == IntPtr.Zero) return;

        var abd = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = _hwnd
        };

        NativeMethods.SHAppBarMessage(NativeMethods.ABM_REMOVE, ref abd);
        _registered = false;
    }

}