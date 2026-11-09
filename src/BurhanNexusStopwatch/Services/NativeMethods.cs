using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BurhanNexusStopwatch.Services;

/// <summary>Native Windows "always on top" (HWND_TOPMOST) helpers.</summary>
internal static class NativeMethods
{
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    private const int SM_CXSIZEFRAME = 32;
    private const int SM_CXPADDEDBORDER = 92;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetricsForDpi(int index, uint dpi);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    /// <summary>
    /// How far (in DIPs) a borderless WindowChrome window hangs off-screen when maximised,
    /// so the content can be inset by the same amount.
    /// </summary>
    public static double MaximizedOverhang(double dpiScale)
    {
        int px;
        try
        {
            uint dpi = (uint)Math.Round(96 * dpiScale);
            px = GetSystemMetricsForDpi(SM_CXSIZEFRAME, dpi) + GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);
        }
        catch (EntryPointNotFoundException)
        {
            px = GetSystemMetrics(SM_CXSIZEFRAME) + GetSystemMetrics(SM_CXPADDEDBORDER);
        }
        return px / dpiScale;
    }

    /// <summary>Puts the window in (or out of) the top-most z-order band.</summary>
    public static void SetTopMost(Window window, bool topMost)
    {
        window.Topmost = topMost; // WPF calls SetWindowPos(HWND_TOPMOST) internally.
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;
        // Re-assert explicitly so the z-order is correct even after minimise/restore.
        SetWindowPos(hwnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    private const uint FLASHW_ALL = 0x3;
    private const uint FLASHW_TIMERNOFG = 0xC;
    private const uint FLASHW_STOP = 0;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FLASHWINFO info);

    /// <summary>Flashes the taskbar button until the window comes to the foreground (or stops it).</summary>
    public static void FlashTaskbar(Window window, bool flash)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;
        var info = new FLASHWINFO
        {
            cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
            hwnd = hwnd,
            dwFlags = flash ? FLASHW_ALL | FLASHW_TIMERNOFG : FLASHW_STOP,
            uCount = uint.MaxValue,
            dwTimeout = 0
        };
        FlashWindowEx(ref info);
    }
}
