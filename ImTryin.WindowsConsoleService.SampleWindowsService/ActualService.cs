using System;

namespace ImTryin.WindowsConsoleService.SampleWindowsService;

public class ActualService : IActualService
{
    public bool Start(bool runningAsService)
    {
        Console.WriteLine("ActualService started...");
        return true;
    }

    public void Stop()
    {
        Console.WriteLine("ActualService stopped!");
    }
}