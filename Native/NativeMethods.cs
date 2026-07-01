using System;
using System.Runtime.InteropServices;

namespace Wice.Native;

public static partial class NativeMethods
{
    // AppBar Messages
    public const uint ABM_NEW = 0x00000000;
    public const uint ABM_REMOVE = 0x00000001;
    public const uint ABM_QUERYPOS = 0x00000002;
    public const uint ABM_SETPOS = 0x00000003;
    public const uint ABE_TOP = 1;

    // Window Style Indexes
    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;

    // Window Styles
    public const uint WS_POPUP = 0x80000000;
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    // Fixed PInvokes with correct native EntryPoints mapped for .NET Core Source Generators
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static partial int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static partial IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static partial int GetWindowLong32(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static partial IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    // Cross-platform wrapper to handle 32-bit (x86) and 64-bit (x64) architectures safely
    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    [LibraryImport("shell32.dll", SetLastError = true)]
    public static partial IntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }
}
