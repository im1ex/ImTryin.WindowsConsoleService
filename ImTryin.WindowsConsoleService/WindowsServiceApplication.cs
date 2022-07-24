using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace ImTryin.WindowsConsoleService;

public class WindowsServiceApplication
{
    private Installer CreateInstaller(ServiceInfo serviceInfo)
    {
        if (serviceInfo.WindowsServiceInfo == null)
            throw new ArgumentNullException(nameof(serviceInfo), nameof(serviceInfo.WindowsServiceInfo) + " is not specified!");

        var serviceProcessInstaller = new ServiceProcessInstaller
        {
            Account = serviceInfo.WindowsServiceInfo.Account,
            Username = serviceInfo.WindowsServiceInfo.Username,
            Password = serviceInfo.WindowsServiceInfo.Password,
        };

        var commandLine = "\"" + Assembly.GetEntryAssembly()!.Location + "\" /service";
        serviceProcessInstaller.Context = new InstallContext {Parameters = {["assemblypath"] = commandLine}};

        serviceProcessInstaller.Installers.Add(new ServiceInstaller
        {
            DisplayName = serviceInfo.DisplayName,
            Description = serviceInfo.Description,
            ServiceName = serviceInfo.Name,
            StartType = serviceInfo.WindowsServiceInfo.StartType,
            DelayedAutoStart = serviceInfo.WindowsServiceInfo.DelayedAutoStart,
        });

        return serviceProcessInstaller;
    }

    public void Install(ServiceInfo serviceInfo)
    {
        using var installer = CreateInstaller(serviceInfo);

        installer.Install(new Hashtable());
    }

    public void Uninstall(ServiceInfo serviceInfo)
    {
        using var installer = CreateInstaller(serviceInfo);

        installer.Uninstall(null);
    }

    public void Start(ServiceInfo serviceInfo)
    {
        var serviceController = ServiceController.GetServices().SingleOrDefault(x => x.ServiceName == serviceInfo.Name);
        if (serviceController != null)
            serviceController.Start();
        else
            Console.WriteLine("Unable to find {0} Windows service. Is it installed?", serviceInfo.Name);
    }

    public void Stop(ServiceInfo serviceInfo)
    {
        var serviceController = ServiceController.GetServices().SingleOrDefault(x => x.ServiceName == serviceInfo.Name);
        if (serviceController != null)
            serviceController.Stop();
        else
            Console.WriteLine("Unable to find {0} Windows service. Is it installed?", serviceInfo.Name);
    }

    public void Run(ServiceInfo serviceInfo, IActualService actualService)
    {
        ServiceBase.Run(new WindowsService(serviceInfo, actualService));
    }
}