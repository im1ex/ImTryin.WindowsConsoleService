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

    private string GetStartupLinkPath()
    {
        var startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        return Path.Combine(startupFolderPath, _serviceInfo.DisplayName + ".lnk");
    }

    public void Install()
    {
        var startupLinkPath = GetStartupLinkPath();
        if (File.Exists(startupLinkPath))
        {
            Console.WriteLine("'{0}' file already exists!", startupLinkPath);
            return;
        }

        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        dynamic shell = Activator.CreateInstance(shellType);
        dynamic shortcut = shell.CreateShortcut(startupLinkPath);

        shortcut.TargetPath = _serviceInfo.ExecutableLocation;
        shortcut.Arguments = "/hidden";
        shortcut.WorkingDirectory = Path.GetDirectoryName(_serviceInfo.ExecutableLocation);

        shortcut.Save();
    }

    public void Uninstall()
    {
        var startupLinkPath = GetStartupLinkPath();
        if (!File.Exists(startupLinkPath))
            Console.WriteLine("'{0}' file is not found!", startupLinkPath);
        else
            File.Delete(startupLinkPath);
    }

    private volatile bool _stopping;

    public void Run(IActualService actualService, bool hidden)
    {
        using var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, _singletonId, out var createdNew);
        if (!createdNew)
        {
            eventWaitHandle.Set();

            Console.WriteLine("{0} is already running.", _serviceInfo.DisplayName);

            return;
        }


        if (hidden)
        {
            if (WindowApi.GetConsoleProcessList(new int[1], 1) > 1)
                Console.WriteLine("Do not use /hidden argument when running application from existing console. Skip hiding.");
            else
                WindowApi.ShowWindow(WindowApi.GetConsoleWindow(), WindowApi.CommandShow.Hide);
        }

        new Action<EventWaitHandle>(ActivateConsole).BeginInvoke(eventWaitHandle, null, null);

        Console.WriteLine(_serviceInfo.DisplayName);

        if (actualService.Start(false))
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        _stopping = true;
        eventWaitHandle.Set();

        actualService.Stop();
    }

    private void ActivateConsole(EventWaitHandle eventWaitHandle)
    {
        while (true)
        {
            eventWaitHandle.WaitOne();

            if (_stopping)
                break;

            var consoleWindowHandle = WindowApi.GetConsoleWindow();

            WindowApi.ShowWindow(consoleWindowHandle, WindowApi.CommandShow.Show);
            WindowApi.SetForegroundWindow(consoleWindowHandle);
        }
    }
}