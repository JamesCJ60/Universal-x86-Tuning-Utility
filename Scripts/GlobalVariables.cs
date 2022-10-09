using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UXTU.Properties;

namespace UXTU.Scripts
{
    internal class GlobalVariables
    {
        public static System.Diagnostics.Process magpieProcess = new System.Diagnostics.Process();
        public static System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        public static void UpdateMagpie()
        {
            string path = (string)Settings.Default["Path"] + "\\bin\\magpie\\Magpie.exe";
            //Pass on path and arguments
            startInfo.FileName = path;
            magpieProcess.StartInfo = startInfo;
            //Start program
            magpieProcess.Start();
        }
    }
}
