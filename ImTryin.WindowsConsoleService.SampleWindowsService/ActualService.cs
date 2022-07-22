using System;

namespace ImTryin.WindowsConsoleService.SampleWindowsService;

public class ActualService : IActualService
{
    public void Start(string[] args)
    {
        Console.WriteLine("SampleWindowsService started...");
    }

    public void Stop()
    {
        Console.WriteLine("SampleWindowsService stopped!");
    }
}