using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService;

internal class WindowsServiceApplication
{
    private readonly ServiceInfo _serviceInfo;
    private readonly WindowsServiceInfo _windowsServiceInfo;

    public WindowsServiceApplication(ServiceInfo serviceInfo)
    {
        _serviceInfo = serviceInfo;

        _windowsServiceInfo = _windowsServiceInfo ??
                              throw new ArgumentNullException(nameof(serviceInfo), nameof(_windowsServiceInfo) + " is not specified!");
    }

    private Installer CreateInstaller()
    {
        var serviceProcessInstaller = new ServiceProcessInstaller
        {
            Account = _windowsServiceInfo.Account,
            Username = _windowsServiceInfo.Username,
            Password = _windowsServiceInfo.Password,
        };

        var commandLine = "\"" + _serviceInfo.ExecutableLocation + "\" /service";
        serviceProcessInstaller.Context = new InstallContext {Parameters = {["assemblypath"] = commandLine}};

        serviceProcessInstaller.Installers.Add(new ServiceInstaller
        {
            DisplayName = _serviceInfo.DisplayName,
            Description = _serviceInfo.Description,
            ServiceName = _serviceInfo.Name,
            StartType = _windowsServiceInfo.StartType,
            DelayedAutoStart = _windowsServiceInfo.DelayedAutoStart,
        });

        return serviceProcessInstaller;
    }

    public void Install()
    {
        using var installer = CreateInstaller();

        installer.Install(new Hashtable());
    }

    public void Uninstall()
    {
        using var installer = CreateInstaller();

        installer.Uninstall(null);
    }

    public void Start()
    {
        var serviceController = ServiceController.GetServices().SingleOrDefault(x => x.ServiceName == _serviceInfo.Name);
        if (serviceController != null)
            serviceController.Start();
        else
            Console.WriteLine("Unable to find {0} Windows service. Is it installed?", _serviceInfo.Name);
    }

    public void Stop()
    {
        var serviceController = ServiceController.GetServices().SingleOrDefault(x => x.ServiceName == _serviceInfo.Name);
        if (serviceController != null)
            serviceController.Stop();
        else
            Console.WriteLine("Unable to find {0} Windows service. Is it installed?", _serviceInfo.Name);
    }

    public void Run(IActualService actualService)
    {
        ServiceBase.Run(new WindowsService(_serviceInfo, actualService));
    }
}