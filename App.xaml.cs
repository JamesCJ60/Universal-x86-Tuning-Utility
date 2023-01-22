using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using UXTU.Scripts;
using Windows.Foundation.Metadata;

namespace AATUV3
{

    public partial class App : Application
    {
        //check if program has admin rights
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _ = Tablet.TabletDevices;
            new HwndSource(new HwndSourceParameters());

            if (!App.IsAdministrator())
            {
                // Restart and run as admin
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                startInfo.Arguments = "restart";
                Process.Start(startInfo);
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
