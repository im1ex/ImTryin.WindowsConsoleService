using ImTryin.WindowsConsoleService.Api;
using System;
using System.IO;
using System.Threading;

namespace ImTryin.WindowsConsoleService;

internal class ConsoleServiceApplication
{
    private readonly ServiceInfo _serviceInfo;
    private readonly string _singletonId;

    public ConsoleServiceApplication(ServiceInfo serviceInfo)
    {
        _serviceInfo = serviceInfo;

        if (serviceInfo.ConsoleServiceInfo == null || string.IsNullOrWhiteSpace(serviceInfo.ConsoleServiceInfo.SingletonId))
            throw new ArgumentNullException(nameof(serviceInfo), nameof(serviceInfo.ConsoleServiceInfo.SingletonId) + " is not specified!");

        _singletonId = serviceInfo.ConsoleServiceInfo.SingletonId;
    }

    private string GetProgramsShortcutPath()
    {
        var programsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

        return Path.Combine(programsFolderPath, _serviceInfo.DisplayName + ".lnk");
    }

    private string GetStartupShortcutPath()
    {
        var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        return Path.Combine(startupFolderPath, _serviceInfo.DisplayName + ".lnk");
    }

    public void Install()
    {
        dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));

        var programsShortcutPath = GetProgramsShortcutPath();
        if (File.Exists(programsShortcutPath))
        {
            Console.WriteLine("'{0}' programs shortcut already exists!", programsShortcutPath);
        }
        else
        {
            dynamic shortcut = shell.CreateShortcut(programsShortcutPath);

            shortcut.TargetPath = _serviceInfo.ExecutableLocation;
            shortcut.WorkingDirectory = Path.GetDirectoryName(_serviceInfo.ExecutableLocation);

            shortcut.Save();
        }

        var startupShortcutPath = GetStartupShortcutPath();
        if (File.Exists(startupShortcutPath))
        {
            Console.WriteLine("'{0}' startup shortcut already exists!", startupShortcutPath);
        }
        else
        {
            dynamic shortcut = shell.CreateShortcut(startupShortcutPath);

            shortcut.TargetPath = _serviceInfo.ExecutableLocation;
            shortcut.Arguments = "/hidden";
            shortcut.WorkingDirectory = Path.GetDirectoryName(_serviceInfo.ExecutableLocation);

            shortcut.Save();
        }
    }

    public void Uninstall()
    {
        var programsShortcutPath = GetProgramsShortcutPath();
        if (!File.Exists(programsShortcutPath))
            Console.WriteLine("'{0}' programs shortcut is not found!", programsShortcutPath);
        else
            File.Delete(programsShortcutPath);

        var startupShortcutPath = GetStartupShortcutPath();
        if (!File.Exists(startupShortcutPath))
            Console.WriteLine("'{0}' startup shortcut is not found!", startupShortcutPath);
        else
            File.Delete(startupShortcutPath);
    }

    private IntPtr _consoleWindowHandle;
    private volatile bool _stopping;

    public void Run(IActualService actualService, bool hidden)
    {
        using var singletonAppWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, _singletonId, out var createdNew);
        if (!createdNew)
        {
            singletonAppWaitHandle.Set();

            Console.WriteLine("{0} is already running.", _serviceInfo.DisplayName);

            return;
        }

        _consoleWindowHandle = ConsoleApi.GetConsoleWindow();

        if (hidden)
        {
            if (ConsoleApi.GetConsoleProcessList(new int[1], 1) > 1)
                Console.WriteLine("Do not use /hidden argument when running application from existing console. Skip hiding.");
            else
            {
                WindowApi.ShowWindow(_consoleWindowHandle, WindowApi.CommandShow.Hide);

                new Action(HookConsoleEvents).BeginInvoke(null, null);
            }
        }

        new Action<EventWaitHandle>(ActivateConsole).BeginInvoke(singletonAppWaitHandle, null, null);

        Console.WriteLine(_serviceInfo.DisplayName);

        if (actualService.Start(false))
        {
            Console.WriteLine("{0} started. Press Ctrl+C to shut down.", _serviceInfo.DisplayName);

            var cancelWaitHandle = new ManualResetEventSlim(false);
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => {
                cancelWaitHandle.Set();
                e.Cancel = true;
            };

            cancelWaitHandle.Wait();
        }

        _stopping = true;
        singletonAppWaitHandle.Set();

        actualService.Stop();
    }

    private void ActivateConsole(EventWaitHandle eventWaitHandle)
    {
        while (true)
        {
            eventWaitHandle.WaitOne();

            if (_stopping)
                break;

            WindowApi.ShowWindow(_consoleWindowHandle, WindowApi.CommandShow.Show);
            WindowApi.SetForegroundWindow(_consoleWindowHandle);
        }
    }

    private void HookConsoleEvents()
    {
        var consoleProcessId = GetConsoleHostProcessId();
        if (consoleProcessId == 0)
        {
            Console.WriteLine("Unable to find console host process! Hide on minimize functionality is not enabled!");
            return;
        }

        var winEventHookHandle = EventHookApi.SetWinEventHook(
            EventHookApi.EventObjectLocationChange,
            EventHookApi.EventObjectLocationChange,
            IntPtr.Zero,
            OnConsoleWindowEvent,
            consoleProcessId,
            0,
            EventHookApi.WinEventHookFlags.OutOfContext);
        if (winEventHookHandle == IntPtr.Zero)
        {
            Console.WriteLine("Unable to hook console host process events! Hide on minimize functionality is not enabled!");
            return;
        }

        try
        {
            int result;
            while (!_stopping && (result = MessageLoopApi.GetMessage(out var msg, IntPtr.Zero, 0, 0)) != 0)
            {
                if (result == -1)
                {
                    Console.WriteLine("HookConsoleEvents error on GetMessage");
                }
                else
                {
                    MessageLoopApi.TranslateMessage(ref msg);
                    MessageLoopApi.DispatchMessage(ref msg);
                }
            }
        }
        finally
        {
            EventHookApi.UnhookWinEvent(winEventHookHandle);
        }
    }

    private int GetConsoleHostProcessId()
    {
        WindowApi.GetWindowThreadProcessId(_consoleWindowHandle, out int consoleWindowProcessId);

        var snapshotHandle = ProcessApi.CreateToolhelp32Snapshot(ProcessApi.Toolhelp32SnapshotFlags.Process, 0);
        try
        {
            var processEntry32 = new ProcessApi.ProcessEntry32();
            for (var b = ProcessApi.Process32First(snapshotHandle, ref processEntry32); b; b = ProcessApi.Process32Next(snapshotHandle, ref processEntry32))
            {
                if (processEntry32.th32ParentProcessID == consoleWindowProcessId)
                {
                    if (string.Equals(processEntry32.szExeFile, "conhost.exe", StringComparison.OrdinalIgnoreCase))
                        return processEntry32.th32ProcessID;
                }
            }
        }
        finally
        {
            ProcessApi.CloseHandle(snapshotHandle);
        }

        return 0;
    }

    private void OnConsoleWindowEvent(
        IntPtr windowEventHookHandle,
        int @event,
        IntPtr windowHandle,
        int objectIdentifier,
        int childIdentifier,
        int eventThreadIdentifier,
        int eventTimeMs)
    {
        if (windowHandle != _consoleWindowHandle || objectIdentifier != 0 || childIdentifier != 0)
            return;

        if (WindowApi.IsIconic(_consoleWindowHandle))
            WindowApi.ShowWindow(_consoleWindowHandle, WindowApi.CommandShow.Hide);
    }
}