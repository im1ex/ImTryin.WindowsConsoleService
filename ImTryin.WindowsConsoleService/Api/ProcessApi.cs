using System;
using System.Runtime.InteropServices;

namespace ImTryin.WindowsConsoleService.Api;

internal static class ProcessApi
{
    [Flags]
    public enum Toolhelp32SnapshotFlags : uint
    {
        HeapList = 0x00000001,
        Process = 0x00000002,
        Thread = 0x00000004,
        Module = 0x00000008,
        Module32 = 0x00000010,
        All = HeapList | Module | Process | Thread,
        Inherit = 0x80000000,
    }

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateToolhelp32Snapshot(Toolhelp32SnapshotFlags flags, int processId);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ProcessEntry32
    {
        public int dwSize;
        public int cntUsage;
        public int th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public int th32ModuleID;
        public int cntThreads;
        public int th32ParentProcessID;
        public int pcPriClassBase;
        public int dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;

        public ProcessEntry32()
        {
            dwSize = Marshal.SizeOf<ProcessEntry32>();
            cntUsage = 0;
            th32ProcessID = 0;
            th32DefaultHeapID = IntPtr.Zero;
            th32ModuleID = 0;
            cntThreads = 0;
            th32ParentProcessID = 0;
            pcPriClassBase = 0;
            dwFlags = 0;
            szExeFile = string.Empty;
        }
    }

    [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "Process32FirstW", CharSet = CharSet.Unicode)]
    public static extern bool Process32First(IntPtr snapshotHandle, ref ProcessEntry32 processEntry32);

    [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "Process32NextW", CharSet = CharSet.Unicode)]
    public static extern bool Process32Next(IntPtr snapshotHandle, ref ProcessEntry32 processEntry32);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);
}