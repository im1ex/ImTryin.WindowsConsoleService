using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService.SampleWindowsService;

internal class Program
{
    public static void Main(string[] args)
    {
        ServiceApplication.Run(args, new ServiceInfo
        {
            Account = ServiceAccount.LocalService,
            Name = "SampleWindowsService",
            DisplayName = "Sample Windows Service",
            Description = "This is Sample Windows Service.",
            StartType = ServiceStartMode.Automatic,
        }, new ActualService());
    }
}