using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class PowerPlans
    {
        public async static void HideAttribute(string subGroup, string attribute)
        {
            await Task.Run(() =>
            {
                // Execute the "powercfg -attributes" command to hide the attribute
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = $"-attributes {subGroup} {attribute} -ATTRIB_HIDE",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                finally
                {
                    process.Dispose();
                }
            });
        }
        public async static void SetPowerValue(string scheme, string subGroup, string powerSetting, uint value, bool isAC)
        {
            await Task.Run(() =>
            {
                // Execute the "powercfg /setacvalueindex" or "powercfg /setdcvalueindex" command to set the power value
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = $"/set{(isAC ? "ac" : "dc")}valueindex {scheme} {subGroup} {powerSetting} {value}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                finally
                {
                    process.Dispose();
                }
            });
        }

        public async static void SetActiveScheme(string scheme)
        {
            await Task.Run(() =>
            {
                // Execute the "powercfg /setactive" command to activate the power scheme
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = $"/setactive {scheme}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                finally
                {
                    process.Dispose();
                }
            });
        }
    }
}
