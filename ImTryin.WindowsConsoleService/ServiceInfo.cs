namespace ImTryin.WindowsConsoleService;

public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ConsoleServiceInfo? ConsoleServiceInfo { get; set; }

    public WindowsServiceInfo? WindowsServiceInfo { get; set; }
}