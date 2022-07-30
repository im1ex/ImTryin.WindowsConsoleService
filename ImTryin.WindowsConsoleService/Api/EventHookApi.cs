using System;
using System.Runtime.InteropServices;

namespace ImTryin.WindowsConsoleService.Api;

internal static class EventHookApi
{
    public static readonly int EventMin = 0x00000001;
    public static readonly int EventObjectLocationChange = 0x800B;
    public static readonly int EventMax = 0x7FFFFFFF;

    public delegate void WindowEventProcedure(
        IntPtr windowEventHookHandle,
        int @event,
        IntPtr windowHandle,
        int objectIdentifier,
        int childIdentifier,
        int eventThreadIdentifier,
        int eventTimeMs);

    [Flags]
    public enum WinEventHookFlags
    {
        OutOfContext = 0x0000,
        SkipOwnThread = 0x0001,
        SkipOwnProcess = 0x0002,
        InContext = 0x0004
    }

    [DllImport("User32.dll", SetLastError = true)]
    public static extern IntPtr SetWinEventHook(
        int eventMin,
        int eventMax,
        IntPtr moduleHandle,
        WindowEventProcedure windowEventProcedure,
        int processId,
        int threadId,
        WinEventHookFlags flags);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool UnhookWinEvent(IntPtr windowEventHookHandle);
}