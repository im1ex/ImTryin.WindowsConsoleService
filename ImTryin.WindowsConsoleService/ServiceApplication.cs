using System;

namespace ImTryin.WindowsConsoleService;

public static class ServiceApplication
{
    public static void Run(string[] args, ServiceInfo serviceInfo, IActualService actualService)
    {
        if (args.Length == 0)
        {
            new ConsoleServiceApplication().Run(serviceInfo, actualService, false);
            return;
        }

        if (args.Length == 1)
        {
            if (string.Equals(args[0], "/hidden", StringComparison.OrdinalIgnoreCase))
            {
                new ConsoleServiceApplication().Run(serviceInfo, actualService, true);
                return;
            }

            if (string.Equals(args[0], "/installService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication().Install(serviceInfo);
                return;
            }

            if (string.Equals(args[0], "/uninstallService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication().Uninstall(serviceInfo);
                return;
            }

            if (string.Equals(args[0], "/startService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication().Start(serviceInfo);
                return;
            }

            if (string.Equals(args[0], "/stopService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication().Stop(serviceInfo);
                return;
            }

            if (string.Equals(args[0], "/service", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication().Run(serviceInfo, actualService);
                return;
            }
        }

        PrintUsage(serviceInfo);
    }

    private static void PrintUsage(ServiceInfo serviceInfo)
    {
        Console.WriteLine(serviceInfo.DisplayName);
        Console.WriteLine("Following arguments available:");

        Console.WriteLine("                     - runs as console service. Only one app instance is supported.");
        Console.WriteLine("  /hidden            - runs as console service and hide console. It is possible to show hidden console by starting app one more time.");

        Console.WriteLine("  /installService    - installs as Windows service. Administrative privileges are required.");
        Console.WriteLine("  /uninstallService  - installs as Windows service. Administrative privileges are required.");

        Console.WriteLine("  /startService      - starts Windows service. Administrative privileges are required.");
        Console.WriteLine("  /stopService       - stops Windows service. Administrative privileges are required.");

        Console.WriteLine("  /service           - runs as Windows service. Only usable then starting by Window Service Control Manager.");
    }
}