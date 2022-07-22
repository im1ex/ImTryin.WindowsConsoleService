using System;
using System.Collections;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService;

public static class ServiceApplication
{
    public static void Run(string[] args, ServiceInfo serviceInfo, IActualService actualService)
    {
        if (args.Length == 0)
        {
            RunAsConsole(serviceInfo, actualService);
            return;
        }

        if (args.Length == 1)
        {
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

    private static void RunAsConsole(ServiceInfo serviceInfo, IActualService actualService)
    {
        Console.WriteLine(serviceInfo.DisplayName);

        if (actualService.Start(false))
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        actualService.Stop();
    }

    private static void RunAsService(ServiceInfo serviceInfo, IActualService actualService)
    {
        ServiceBase.Run(new WindowsService(serviceInfo, actualService));
    }

    private static Installer CreateInstaller(ServiceInfo serviceInfo)
    {
        var serviceProcessInstaller = new ServiceProcessInstaller
        {
            Account = serviceInfo.Account,
            Username = serviceInfo.Username,
            Password = serviceInfo.Password,
        };

        var commandLine = "\"" + Assembly.GetEntryAssembly()!.Location + "\" /service";
        serviceProcessInstaller.Context = new InstallContext {Parameters = {["assemblypath"] = commandLine}};

        serviceProcessInstaller.Installers.Add(new ServiceInstaller
        {
            DisplayName = serviceInfo.DisplayName,
            Description = serviceInfo.Description,
            ServiceName = serviceInfo.Name,
            StartType = serviceInfo.StartType,
            DelayedAutoStart = serviceInfo.DelayedAutoStart,
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