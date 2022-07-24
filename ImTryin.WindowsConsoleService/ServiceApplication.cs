using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace ImTryin.WindowsConsoleService;

public static class ServiceApplication
{
    public static void Run(string[] args, ServiceInfo serviceInfo, IActualService actualService)
    {
        if (args.Length == 0)
        {
            RunAsConsole(serviceInfo, actualService, false);
            return;
        }

        if (args.Length == 1)
        {
            if (string.Equals(args[0], "/hidden", StringComparison.OrdinalIgnoreCase))
            {
                RunAsConsole(serviceInfo, actualService, true);
                return;
            }

            if (string.Equals(args[0], "/service", StringComparison.OrdinalIgnoreCase))
            {
                RunAsService(serviceInfo, actualService);
                return;
            }

            if (string.Equals(args[0], "/installService", StringComparison.OrdinalIgnoreCase))
            {
                InstallService(serviceInfo);
                return;
            }

            if (string.Equals(args[0], "/uninstallService", StringComparison.OrdinalIgnoreCase))
            {
                UninstallService(serviceInfo);
                return;
            }
        }

        PrintUsage(serviceInfo);
    }

    private static void PrintUsage(ServiceInfo serviceInfo)
    {
        Console.WriteLine(serviceInfo.DisplayName);
        Console.WriteLine();
    }

    private static volatile bool __stopping;

    private static void RunAsConsole(ServiceInfo serviceInfo, IActualService actualService, bool hidden)
    {
        if (serviceInfo.ConsoleServiceInfo == null || string.IsNullOrWhiteSpace(serviceInfo.ConsoleServiceInfo.SingletonId))
            throw new ArgumentNullException(nameof(serviceInfo), nameof(serviceInfo.ConsoleServiceInfo.SingletonId) + " is not specified!");

        using var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, serviceInfo.ConsoleServiceInfo.SingletonId, out var createdNew);
        if (!createdNew)
        {
            eventWaitHandle.Set();

            Console.WriteLine("{0} is already running.", serviceInfo.DisplayName);

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

        Console.WriteLine(serviceInfo.DisplayName);

        if (actualService.Start(false))
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        __stopping = true;
        eventWaitHandle.Set();

        actualService.Stop();
    }

    private static void ActivateConsole(EventWaitHandle eventWaitHandle)
    {
        while (true)
        {
            eventWaitHandle.WaitOne();

            if (__stopping)
                break;

            var consoleWindowHandle = WindowApi.GetConsoleWindow();

            WindowApi.ShowWindow(consoleWindowHandle, WindowApi.CommandShow.Show);
            WindowApi.SetForegroundWindow(consoleWindowHandle);
        }
    }


    private static void RunAsService(ServiceInfo serviceInfo, IActualService actualService)
    {
        ServiceBase.Run(new WindowsService(serviceInfo, actualService));
    }

    private static Installer CreateInstaller(ServiceInfo serviceInfo)
    {
        if (serviceInfo.WindowsServiceInfo == null)
            throw new ArgumentNullException(nameof(serviceInfo), nameof(serviceInfo.WindowsServiceInfo) + " is not specified!");

        var serviceProcessInstaller = new ServiceProcessInstaller
        {
            Account = serviceInfo.WindowsServiceInfo.Account,
            Username = serviceInfo.WindowsServiceInfo.Username,
            Password = serviceInfo.WindowsServiceInfo.Password,
        };

        var commandLine = "\"" + Assembly.GetEntryAssembly()!.Location + "\" /service";
        serviceProcessInstaller.Context = new InstallContext {Parameters = {["assemblypath"] = commandLine}};

        serviceProcessInstaller.Installers.Add(new ServiceInstaller
        {
            DisplayName = serviceInfo.DisplayName,
            Description = serviceInfo.Description,
            ServiceName = serviceInfo.Name,
            StartType = serviceInfo.WindowsServiceInfo.StartType,
            DelayedAutoStart = serviceInfo.WindowsServiceInfo.DelayedAutoStart,
        });

        return serviceProcessInstaller;
    }

    private static void InstallService(ServiceInfo serviceInfo)
    {
        using var installer = CreateInstaller(serviceInfo);

        installer.Install(new Hashtable());
    }

    private static void UninstallService(ServiceInfo serviceInfo)
    {
        using var installer = CreateInstaller(serviceInfo);

        installer.Uninstall(null);
    }
}