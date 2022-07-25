using System.Reflection;

namespace ImTryin.WindowsConsoleService;

public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ConsoleServiceInfo? ConsoleServiceInfo { get; set; }

    public WindowsServiceInfo? WindowsServiceInfo { get; set; }

    private string? _executableLocation;
    public string ExecutableLocation => _executableLocation ??= Assembly.GetEntryAssembly()!.Location;
}