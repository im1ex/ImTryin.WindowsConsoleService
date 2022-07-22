using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService;

public class WindowsService : ServiceBase
{
    private readonly IActualService _actualService;

    public WindowsService(ServiceInfo serviceInfo, IActualService actualService)
    {
        _actualService = actualService;
        ServiceName = serviceInfo.Name;
    }

    protected override void OnStart(string[] args)
    {
        _actualService.Start(args);
    }

    protected override void OnStop()
    {
        _actualService.Stop();
    }
}