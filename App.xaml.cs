using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AATUV3
{

    public partial class App : Application
    {
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("magpie")) process.Kill();
            foreach (var process in Process.GetProcessesByName("Magpie")) process.Kill();
        }
    }
}
