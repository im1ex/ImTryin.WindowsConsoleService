using System;

namespace ImTryin.WindowsConsoleService;

public static class ServiceApplication
{
    public static void Run(string[] args, ServiceInfo serviceInfo, IActualService actualService)
    {
        if (args.Length == 0)
        {
            new ConsoleServiceApplication(serviceInfo).Run(actualService, false);
            return;
        }

        if (args.Length == 1)
        {
            if (string.Equals(args[0], "/hidden", StringComparison.OrdinalIgnoreCase))
            {
                new ConsoleServiceApplication(serviceInfo).Run(actualService, true);
                return;
            }

            if (string.Equals(args[0], "/installConsole", StringComparison.OrdinalIgnoreCase))
            {
                new ConsoleServiceApplication(serviceInfo).Install();
                return;
            }

            if (string.Equals(args[0], "/uninstallConsole", StringComparison.OrdinalIgnoreCase))
            {
                new ConsoleServiceApplication(serviceInfo).Uninstall();
                return;
            }

            if (string.Equals(args[0], "/installService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication(serviceInfo).Install();
                return;
            }

            if (string.Equals(args[0], "/uninstallService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication(serviceInfo).Uninstall();
                return;
            }

            if (string.Equals(args[0], "/startService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication(serviceInfo).Start();
                return;
            }

            if (string.Equals(args[0], "/stopService", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication(serviceInfo).Stop();
                return;
            }

            if (string.Equals(args[0], "/service", StringComparison.OrdinalIgnoreCase))
            {
                new WindowsServiceApplication(serviceInfo).Run(actualService);
                return;
            }
        }

        PrintUsage(serviceInfo);
    }

    private static void PrintUsage(ServiceInfo serviceInfo)
    {
        Console.WriteLine(serviceInfo.DisplayName);
        Console.WriteLine("Following arguments available:");

        if (serviceInfo.ConsoleServiceInfo != null)
        {
            Console.WriteLine("                     - runs as Console service. Only one app instance is supported.");

            Console.WriteLine("  /hidden            - runs as Console service and hide console.");
            Console.WriteLine("                       It is possible to show hidden console by starting app one more time.");
            Console.WriteLine("                       And hide it again when console minimizes.");

            Console.WriteLine("  /installConsole    - installs Console service. Creates shortcut in User's Startup folder.");
            Console.WriteLine("  /uninstallConsole  - installs Console service. Deletes shortcut from User's Startup folder.");
        }

        if (serviceInfo.WindowsServiceInfo != null)
        {
            Console.WriteLine("  /installService    - installs Windows service. Administrative privileges are required.");
            Console.WriteLine("  /uninstallService  - uninstalls Windows service. Administrative privileges are required.");

            Console.WriteLine("  /startService      - starts Windows service. Administrative privileges are required.");
            Console.WriteLine("  /stopService       - stops Windows service. Administrative privileges are required.");

            Console.WriteLine("  /service           - runs as Windows service.");
            Console.WriteLine("                       Only usable then starting by Window Service Control Manager.");
        }
    }
}