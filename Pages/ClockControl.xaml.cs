using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RyzenSMUBackend;
using AATUV3.Scripts.SMU_Backend_Scripts;
using AATUV3.Scripts;
using UXTU.Properties;
using UXTU.Scripts.Adpative_Modes.Performance;
using System.Threading;
using System.IO;
using System.Diagnostics;
using LibreHardwareMonitor.Hardware;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class ClockControl : UserControl
    {
        public ClockControl()
        {
            InitializeComponent();
            nudCoreClock.Value = Settings.Default.AllCoreClk;
            nudCoreVolt.Value = Settings.Default.CPUVID;
            nudBus.Value = (decimal)Settings.Default.BusCLK;
            nudCOCPU.Value = Settings.Default.COCPU;
            nudCOIGPU.Value = Settings.Default.COiGPU;
            nudiGPU.Value = Settings.Default.iGPUClk;
            nuddGPUCore.Value = Settings.Default.dGPUCLK;
            nuddGPUMem.Value = Settings.Default.dGPUMem;

            cbCoreClock.IsChecked = Settings.Default.isAllCoreCLK;
            cbCoreVolt.IsChecked = Settings.Default.isVID;
            cbBus.IsChecked = Settings.Default.isBUS;
            cbCOCPU.IsChecked = Settings.Default.isCPUCO;
            cbCOIGPU.IsChecked = Settings.Default.isGPUCO;
            cbdGPUCore.IsChecked = Settings.Default.isNV;
            cbiGPU.IsChecked = Settings.Default.isiGPUClk;

            cbRaddGPUCore.IsChecked = Settings.Default.isRadOC;

            if (Settings.Default.RadOption == 0) rbFactory.IsChecked = true;
            else if (Settings.Default.RadOption == 1) rbUVGPU.IsChecked = true;
            else if (Settings.Default.RadOption == 2) rbOCGPU.IsChecked = true;
            else if (Settings.Default.RadOption == 3) rbOCVRAM.IsChecked = true;
            else if (Settings.Default.RadOption == 4) rbMan.IsChecked = true;

            GetPowerInfo();
            getGPURange();
            getVRAMInfo();
            getGPUClocks();
        }

        private void Disable_Click(object sender, RoutedEventArgs e)
        {
            SendCommand.set_disable_oc();
        }

        public static int power, minClock, maxClock, Volt, VRAMClock, VRAMMode;
        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            Settings.Default.AllCoreClk = (int)nudCoreClock.Value;
            Settings.Default.CPUVID = (int)nudCoreVolt.Value;
            Settings.Default.BusCLK = (double)nudBus.Value;
            Settings.Default.COCPU = (int)nudCOCPU.Value;
            Settings.Default.COiGPU = (int)nudCOIGPU.Value;
            Settings.Default.iGPUClk = (int)nudiGPU.Value;
            Settings.Default.dGPUCLK = (int)nuddGPUCore.Value;
            Settings.Default.dGPUMem = (int)nuddGPUMem.Value;
            Settings.Default.Save();

            if (cbCoreClock.IsChecked == true)
            {
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                i++;
            }

            if (cbCoreVolt.IsChecked == true)
            {
                double vid = Math.Round((double)nudCoreVolt.Value / 1000, 2);
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_enable_oc();
                i++;
            }

            if (cbBus.IsChecked == true)
            {
                RwMmioAmd MMIO = new RwMmioAmd();
                MMIO.SetBclk(Convert.ToDouble(nudBus.Value));
                i++;
            }

            if (cbCOCPU.IsChecked == true)
            {
                if (nudCOCPU.Value >= 0)
                {
                    SendCommand.set_coall((uint)nudCOCPU.Value);
                }
                else
                {
                    SendCommand.set_coall(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudCOCPU.Value)));
                }
                i++;
            }

            if (cbCOIGPU.IsChecked == true)
            {
                if (nudCOCPU.Value >= 0)
                {
                    SendCommand.set_cogfx((uint)nudCOIGPU.Value);
                }
                else
                {
                    SendCommand.set_cogfx(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudCOIGPU.Value)));
                }
                i++;
            }

            if (cbiGPU.IsChecked == true)
            {
                SendCommand.set_gfx_clk((uint)nudiGPU.Value);
                i++;
            }

            if (cbdGPUCore.IsChecked == true)
            {
                //Get RyzenAdj path
                string path = "\\bin\\oc.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(path, "0 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                BasicExeBackend.ApplySettings(path, "1 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                BasicExeBackend.ApplySettings(path, "2 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                i++;
            }

            if (cbRaddGPUCore.IsChecked == true)
            {
                // Get path
                string path = "\\bin\\vramoc.exe";
                string path2 = "\\bin\\coreoc.exe";
                string path3 = "\\bin\\auto.exe";
                string path4 = "\\bin\\power.exe";

                // Pass settings on to be applied
                if (rbFactory.IsChecked == true) BasicExeBackend.ApplySettings(path3, "0", true);
                else if (rbUVGPU.IsChecked == true) BasicExeBackend.ApplySettings(path3, "1 0", true);
                else if (rbOCGPU.IsChecked == true) BasicExeBackend.ApplySettings(path3, "1 1", true);
                else if (rbOCVRAM.IsChecked == true) BasicExeBackend.ApplySettings(path3, "1 2", true);
                else
                {
                    power = (int)nudPower.Value;
                    minClock = (int)nudMinClockCore.Value;
                    maxClock = (int)nudMaxClockCore.Value;
                    Volt = (int)nudVoltage.Value;
                    VRAMClock = (int)nudVRAMClockMem.Value;
                    VRAMMode = 1;

                    await Task.Run(() =>
                    {
                        BasicExeBackend.ApplySettings(path3, "0", true);
                        Thread.Sleep(250);
                        BasicExeBackend.ApplySettings(path2, "3 " + minClock + " " + maxClock + " " + Volt, true);
                        Thread.Sleep(100);
                        BasicExeBackend.ApplySettings(path4, "1 " + power, true);
                        Thread.Sleep(100);
                        BasicExeBackend.ApplySettings(path, "4 " + VRAMClock, true);
                        Thread.Sleep(250);
                        BasicExeBackend.ApplySettings(path, "3 " + VRAMMode, true);
                        Thread.Sleep(100);
                    });

                }
                i++;
            }

            if (i == 0)
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }
            else
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
            }

            Settings.Default.isAllCoreCLK = (bool)cbCoreClock.IsChecked;
            Settings.Default.isVID = (bool)cbCoreVolt.IsChecked;
            Settings.Default.isBUS = (bool)cbBus.IsChecked;
            Settings.Default.isCPUCO = (bool)cbCOCPU.IsChecked;
            Settings.Default.isGPUCO = (bool)cbCOIGPU.IsChecked;
            Settings.Default.isNV = (bool)cbdGPUCore.IsChecked;
            Settings.Default.isiGPUClk = (bool)cbiGPU.IsChecked;
            Settings.Default.isRadOC = (bool)cbRaddGPUCore.IsChecked;

            if (rbFactory.IsChecked == true) Settings.Default.RadOption = 0;
            else if (rbUVGPU.IsChecked == true) Settings.Default.RadOption = 1;
            else if (rbOCGPU.IsChecked == true) Settings.Default.RadOption = 2;
            else if (rbOCVRAM.IsChecked == true) Settings.Default.RadOption = 3;
            else if (rbMan.IsChecked == true) Settings.Default.RadOption = 4;

            Settings.Default.Save();
        }


        private void rbMan_Unchecked(object sender, RoutedEventArgs e)
        {
            spManual.Visibility = Visibility.Collapsed;
        }

        private void rbMan_Checked(object sender, RoutedEventArgs e)
        {
            spManual.Visibility = Visibility.Visible;
        }

        private void getGPURange()
        {
            using (Process process = new Process())
            {
                string path = Settings.Default.Path + "\\bin\\coreoc.exe";
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "1";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();


                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                lines[0] = lines[0].Substring(lines[0].IndexOf(',') + 1);
                lines[0] = lines[0].Replace(" ", null);
                int index = lines[0].LastIndexOf(")");
                lines[0] = lines[0].Substring(0, index);

                nudMinClockCore.MaxValue = Convert.ToInt32(lines[0]);
                nudMaxClockCore.MaxValue = Convert.ToInt32(lines[0]);

                lines[2] = lines[2].Substring(lines[2].IndexOf('(') + 1);
                lines[2] = lines[2].Replace(" ", null);

                index = lines[2].LastIndexOf(")");
                lines[2] = lines[2].Substring(0, index);

                string[] Range = lines[2].Split(new string[] { "," }, StringSplitOptions.None);

                nudVoltage.MinValue = Convert.ToInt32(Range[0]);
                nudVoltage.MaxValue = Convert.ToInt32(Range[1]);

                process.WaitForExit();
            }
        }

        private void GetPowerInfo()
        {
            using (Process process = new Process())
            {
                string path = Settings.Default.Path + "\\bin\\power.exe";
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "0";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();


                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                lines[0] = lines[0].Substring(lines[0].IndexOf(':') + 1);
                lines[0] = lines[0].Replace(" ", null);
                nudPower.Value = Convert.ToInt32(lines[0]);

                lines[1] = lines[1].Substring(lines[1].IndexOf(':') + 1);
                lines[1] = lines[1].Replace(" ", null);
                nudPower.Value = Convert.ToInt32(lines[1]);

                process.WaitForExit();
            }
        }

        private void getGPUClocks()
        {
            using (Process process = new Process())
            {
                string path = Settings.Default.Path + "\\bin\\coreoc.exe";
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "2";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();


                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                lines[0] = lines[0].Substring(lines[0].IndexOf(':') + 1);
                lines[0] = lines[0].Replace(" ", null);
                nudMinClockCore.Value = Convert.ToInt32(lines[0]);

                lines[1] = lines[1].Substring(lines[1].IndexOf(':') + 1);
                lines[1] = lines[1].Replace(" ", null);
                nudMaxClockCore.Value = Convert.ToInt32(lines[1]);

                lines[2] = lines[2].Substring(lines[2].IndexOf(':') + 1);
                lines[2] = lines[2].Replace(" ", null);
                nudVoltage.Value = Convert.ToInt32(lines[2]);

                process.WaitForExit();
            }
        }
        private void getVRAMInfo()
        {
            using (Process process = new Process())
            {
                string path = Settings.Default.Path + "\\bin\\vramoc.exe";
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "2";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();


                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                lines[0] = lines[0].Substring(lines[0].IndexOf(':') + 1);
                lines[0] = lines[0].Replace(" ", null);
                nudVRAMClockMem.Value = Convert.ToInt32(lines[0]);

                lines[1] = lines[1].Substring(lines[1].IndexOf('[') + 1);
                lines[1] = lines[1].Replace(" ", null);

                int index = lines[1].LastIndexOf("]");
                lines[1] = lines[1].Substring(0, index);

                string[] Range = lines[1].Split(new string[] { "," }, StringSplitOptions.None);

                nudVRAMClockMem.MinValue = Convert.ToInt32(Range[0]);
                nudVRAMClockMem.MaxValue = Convert.ToInt32(Range[1]);

                lines[2] = lines[2].Substring(lines[2].IndexOf(':') + 1);
                lines[2] = lines[2].Replace(" ", null);

                if (lines[2].Contains("Default")) rbDefault.IsChecked = true;
                else rbFast.IsChecked = true;

                process.WaitForExit();
            }
        }
    }
}
