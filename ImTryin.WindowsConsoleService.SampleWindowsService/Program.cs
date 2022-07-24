using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService.SampleWindowsService;

internal class Program
{
    public static void Main(string[] args)
    {
        ServiceApplication.Run(args, new ServiceInfo
        {
            Name = "SampleWindowsService",
            DisplayName = "Sample Windows Service",
            Description = "This is Sample Windows Service.",
            ConsoleServiceInfo = new ConsoleServiceInfo
            {
                SingletonId = "SampleWindowsService-9b7d25ae-ce3e-42d6-a411-d1e279b5f020"
            },
            WindowsServiceInfo = new WindowsServiceInfo
            {
                Account = ServiceAccount.LocalService,
                StartType = ServiceStartMode.Automatic
            }
        }, new ActualService());
    }
}