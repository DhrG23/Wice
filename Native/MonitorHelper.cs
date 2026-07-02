using Avalonia.Controls;
using Avalonia.Platform;
using System;

namespace Wice.Native;

public static class MonitorHelper
{
    // Converts logical (Avalonia) pixels to physical (Win32) pixels
    public static int ToPhysical(double logicalPixels, double scale) =>
        (int)Math.Round(logicalPixels * scale);

    // Converts physical (Win32) pixels to logical (Avalonia) pixels
    public static double ToLogical(int physicalPixels, double scale) =>
        physicalPixels / scale;

    // Helper to define a RECT based on physical screen bounds and logical UI height
    public static NativeMethods.RECT GetPhysicalRect(Screen screen, double logicalHeight)
    {
        double scale = screen.Scaling;
        return new NativeMethods.RECT
        {
            Left = screen.Bounds.X,
            Top = screen.Bounds.Y,
            Right = screen.Bounds.Right,
            Bottom = screen.Bounds.Y + ToPhysical(logicalHeight, scale)
        };
    }
}