using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace YiffBrowser.BaseFramework.Extensions;

public static class WindowExtension {

    public const int SW_RESTORE = 9;

    // from winuser.h
    public const int GWL_STYLE = -16,
                      WS_MAXIMIZEBOX = 0x10000,
                      WS_MINIMIZEBOX = 0x20000;

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int value);

    public static void HideMinimizeAndMaximizeButtons(this Window window) {
        IntPtr hwnd = HandleOf(window);
        if (hwnd == IntPtr.Zero) {
            return;
        }
        int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
        SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        RefreshFrame(hwnd);
    }

    public static void HideMinimizeButton(this Window window) {
        IntPtr hwnd = HandleOf(window);
        if (hwnd == IntPtr.Zero) {
            return;
        }
        int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
        SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
        RefreshFrame(hwnd);
    }

    private static IntPtr HandleOf(Window window) {
        if (PresentationSource.FromVisual(window) is HwndSource source && source.Handle != IntPtr.Zero) {
            return source.Handle;
        }
        return new WindowInteropHelper(window).EnsureHandle();
    }

    private static void RefreshFrame(IntPtr hwnd) {
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

}


public static class OSInterop {
    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int smIndex);
    public const int SM_CMONITORS = 80;

    [DllImport("user32.dll")]
    public static extern bool SystemParametersInfo(int nAction, int nParam, ref RECT rc, int nUpdate);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

    public struct RECT {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public int width => right - left;
        public int height => bottom - top;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class MONITORINFOEX {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        public RECT rcMonitor = new();
        public RECT rcWork = new();
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
        public int dwFlags;
    }
}

public static class WPFExtensionMethods {
    public static Point GetAbsolutePosition(this Window w) {
        if (w.WindowState != WindowState.Maximized) {
            return new Point(w.Left, w.Top);
        }

        Int32Rect r;
        bool multimonSupported = OSInterop.GetSystemMetrics(OSInterop.SM_CMONITORS) != 0;
        if (!multimonSupported) {
            OSInterop.RECT rc = new();
            OSInterop.SystemParametersInfo(48, 0, ref rc, 0);
            r = new Int32Rect(rc.left, rc.top, rc.width, rc.height);
        } else {
            WindowInteropHelper helper = new(w);
            IntPtr hmonitor = OSInterop.MonitorFromWindow(new HandleRef(null, helper.EnsureHandle()), 2);
            OSInterop.MONITORINFOEX info = new();
            OSInterop.GetMonitorInfo(new HandleRef(null, hmonitor), info);
            r = new Int32Rect(info.rcWork.left, info.rcWork.top, info.rcWork.width, info.rcWork.height);
        }
        return new Point(r.X, r.Y);
    }
}
