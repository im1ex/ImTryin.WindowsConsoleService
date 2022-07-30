using System;
using System.Runtime.InteropServices;

namespace ImTryin.WindowsConsoleService.Api;

internal static class MessageLoopApi
{
    public struct Point
    {
        public int X;
        public int Y;
    }

    public struct Msg
    {
        public IntPtr WindowHandle;
        public int Message;
        public IntPtr WParam;
        public IntPtr LParam;
        public int Time;
        public Point Point;
        public int LPrivate;
    }

    [DllImport("User32.dll", SetLastError = true)]
    public static extern int GetMessage(out Msg msg, IntPtr windowHandle, int messageFilterMin, int messageFilterMax);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool TranslateMessage(ref Msg msg);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern IntPtr DispatchMessage(ref Msg msg);
}