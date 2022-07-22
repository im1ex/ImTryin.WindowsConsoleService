using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService;

public class ServiceInfo
{
    public ServiceAccount Account { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceStartMode StartType { get; set; }
    public bool DelayedAutoStart { get; set; }
}