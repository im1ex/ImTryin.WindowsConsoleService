namespace ImTryin.WindowsConsoleService;

public interface IActualService
{
    bool Start(bool runningAsService);
    void Stop();
}