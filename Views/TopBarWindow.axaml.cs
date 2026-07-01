using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using Wice.Native;


namespace Wice.Views;


public partial class TopBarWindow : Window
{
    private const int BarHeightDip = 40;

    private readonly Screen _targetScreen;
    private IntPtr _hwnd;
    private bool _registered;


    public TopBarWindow(Screen screen)
    {
        InitializeComponent();
        _targetScreen = screen;

        this.Position = _targetScreen.Bounds.Position;

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

        // IMPORTANT: Windows might change abd.rc here if it needs to shift you!
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);

        // Tell Windows: "I am taking this (possibly adjusted) space"
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);

        // in sync regardless of monitor DPI.
        this.Position = new PixelPoint(abd.rc.Left, abd.rc.Top);
        this.Width = 2560;
        this.Height = (abd.rc.Bottom - abd.rc.Top) / scaling;

        _registered = true;
    }

    private void UnregisterSystemBar()
    {
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