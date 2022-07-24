# ImTryin.WindowsConsoleService
Library for creating Windows Services able to run as service and as console application.

Following command line arguments available:

                       - runs as console service. Only one app instance is supported.
    /hidden            - runs as console service and hide console. It is possible to show hidden console by starting app one more time.
    /installService    - installs as Windows service. Administrative privileges are required.
    /uninstallService  - installs as Windows service. Administrative privileges are required.
    /startService      - starts Windows service. Administrative privileges are required.
    /stopService       - stops Windows service. Administrative privileges are required.
    /service           - runs as Windows service. Only usable then starting by Window Service Control Manager.

See https://github.com/imlex/ImTryin.WindowsConsoleService/tree/master/ImTryin.WindowsConsoleService.SampleWindowsService for example.
