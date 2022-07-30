using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService;

internal class WindowsService : ServiceBase
{
    private readonly IActualService _actualService;

    public WindowsService(ServiceInfo serviceInfo, IActualService actualService)
    {
        _actualService = actualService;
        ServiceName = serviceInfo.Name;
    }

    protected override void OnStart(string[] args)
    {
        if (!_actualService.Start(true))
            Stop();
    }

    protected override void OnStop()
    {
        _actualService.Stop();
    }
}