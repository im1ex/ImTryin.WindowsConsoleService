using System;

namespace ImTryin.WindowsConsoleService.SampleWindowsService;

public class ActualService : IActualService
{
    public bool Start(bool runningAsService)
    {
        Console.WriteLine("SampleWindowsService started...");
        return true;
    }

    public void Stop()
    {
        Console.WriteLine("SampleWindowsService stopped!");
    }
}