using System;
using System.Runtime.InteropServices;

namespace ImTryin.WindowsConsoleService.Api;

internal static class ConsoleApi
{
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int GetConsoleProcessList([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] processIds, int processIdsSize);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetConsoleWindow();
}