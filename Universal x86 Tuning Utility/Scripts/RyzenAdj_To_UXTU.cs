using CpuAffinityUtility;
using GameLib.Plugin.RiotGames.Model;
using NvAPIWrapper.Display;
using RyzenSmu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Scripts.ASUS;
using Universal_x86_Tuning_Utility.Scripts.GPUs.AMD;
using Universal_x86_Tuning_Utility.Scripts.GPUs.NVIDIA;
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.Scripts
{
    internal class RyzenAdj_To_UXTU
    {
        static int i = 0;

        [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveOverlayScheme")]
        public static extern uint PowerSetActiveOverlayScheme(Guid OverlaySchemeGuid);

        static string balancedPowerScheme = "00000000-0000-0000-0000-000000000000";
        static string highPerformancePowerScheme = "DED574B5-45A0-4F42-8737-46345C09C238";
        static string powerSaverPowerScheme = "961CC777-2547-4F9D-8174-7D86181b8A7A";

        //Translate RyzenAdj like cli arguments to UXTU
        public static async void Translate(string _ryzenAdjString, bool isAutoReapply = false)
        {
            try
            {
                //Remove last space off cli arguments 
                _ryzenAdjString = _ryzenAdjString.Substring(0, _ryzenAdjString.Length - 1);
                //Split cli arguments into array
                string[] ryzenAdjCommands = _ryzenAdjString.Split(' ');
                ryzenAdjCommands = ryzenAdjCommands.Distinct().ToArray();

                //MessageBox.Show(_ryzenAdjString);
                //Run through array
                foreach (string ryzenAdjCommand in ryzenAdjCommands)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            string command = ryzenAdjCommand;
                            if (!command.Contains("=")) command = ryzenAdjCommand + "=0";
                            // Extract the command string before the "=" sign
                            string ryzenAdjCommandString = command.Split('=')[0].Replace("=", null).Replace("--", null);
                            // Extract the command string after the "=" sign
                            string ryzenAdjCommandValueString = command.Substring(ryzenAdjCommand.IndexOf('=') + 1);


                            if (ryzenAdjCommandString.Contains("UXTUSR"))
                            {
                                UXTUSR(ryzenAdjCommandString, ryzenAdjCommandValueString);
                                Task.Delay(50);
                            }
                            else if (ryzenAdjCommandString.Contains("CCD-Affinity"))
                            {
                                CpuAffinityManager.SetGlobalAffinity(Convert.ToInt32(ryzenAdjCommandValueString));
                                Task.Delay(50);
                            }
                            else if (ryzenAdjCommandString.Contains("Win-Power"))
                            {
                                if(ryzenAdjCommandValueString == "0") PowerSetActiveOverlayScheme(new Guid(powerSaverPowerScheme.ToLower()));
                                else if (ryzenAdjCommandValueString == "1") PowerSetActiveOverlayScheme(new Guid(balancedPowerScheme.ToLower()));
                                else if (ryzenAdjCommandValueString == "2") PowerSetActiveOverlayScheme(new Guid(highPerformancePowerScheme.ToLower()));
                                Task.Delay(50);
                            }
                            else if (ryzenAdjCommandString.Contains("ASUS"))
                            {
                                AsusWmi(ryzenAdjCommandString, ryzenAdjCommandValueString);
                                Task.Delay(50);
                            }
                            else if (ryzenAdjCommandString.Contains("Refresh-Rate"))
                            {
                                Universal_x86_Tuning_Utility.Scripts.Misc.Display.ApplySettings(Convert.ToInt32(ryzenAdjCommandValueString));
                            }
                            else if (ryzenAdjCommandString.Contains("ADLX"))
                            {
                                ADLX(ryzenAdjCommandString, ryzenAdjCommandValueString);
                                Task.Delay(50);
                            }
                            else if (ryzenAdjCommandString.Contains("NVIDIA"))
                            {
                                NVIDIA(ryzenAdjCommandString, ryzenAdjCommandValueString);
                                Task.Delay(50);
                            }
                            else if (ryzenAdjCommandString.Contains("intel"))
                            {
                                if (ryzenAdjCommandValueString.Contains("-"))
                                {
                                    if (ryzenAdjCommandString == "intel-ratio")
                                    {
                                        string[] stringArray = ryzenAdjCommandValueString.Split('-');
                                        int[] intArray = stringArray.Select(int.Parse).ToArray();

                                        Intel_Management.changeClockRatioOffset(intArray);
                                    }
                                }
                                else
                                {
                                    //Convert value of select cli argument to int
                                    int ryzenAdjCommandValue = Convert.ToInt32(ryzenAdjCommandValueString);

                                    if (ryzenAdjCommandString == "intel-pl") Intel_Management.changeTDPAll(ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-volt-cpu") Intel_Management.changeVoltageOffset(0, ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-volt-gpu") Intel_Management.changeVoltageOffset(1, ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-volt-cache") Intel_Management.changeVoltageOffset(2, ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-volt-sa") Intel_Management.changeVoltageOffset(3, ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-bal-cpu") Intel_Management.changePowerBalance(0, ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-bal-gpu") Intel_Management.changePowerBalance(1, ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-gpu") Intel_Management.changeGpuClock(ryzenAdjCommandValue);
                                    //else if (ryzenAdjCommandString == "power-limit-1") TDP_Management.changePL1(ryzenAdjCommandValue);
                                    //else if (ryzenAdjCommandString == "power-limit-2") TDP_Management.changePL2(ryzenAdjCommandValue);
                                }
                            }
                            else
                            {
                                //Convert value of select cli argument to uint
                                uint ryzenAdjCommandValue = Convert.ToUInt32(ryzenAdjCommandValueString);

                                if (ryzenAdjCommand.Contains("skin")) ryzenAdjCommandValue *= 256;

                                if (ryzenAdjCommandValue <= 0 && !ryzenAdjCommandString.Contains("co")) SMUCommands.applySettings(ryzenAdjCommandString, 0x0);
                                else SMUCommands.applySettings(ryzenAdjCommandString, ryzenAdjCommandValue);
                                Task.Delay(50);
                            }
                        }
                        catch (Exception ex) { }
                    });
                }
            }
            catch (Exception ex) { }
        }
        private static void ADLX(string command, string value)
        {
            try
            {
                string[] variables = value.Split('-');

                if (command == "ADLX-Lag") ADLXBackend.SetAntiLag(int.Parse(variables[0]), bool.Parse(variables[1]));
                if (command == "ADLX-Boost") ADLXBackend.SetBoost(int.Parse(variables[0]), bool.Parse(variables[1]), int.Parse(variables[2]));
                if (command == "ADLX-RSR")
                {
                    ADLXBackend.SetRSR(bool.Parse(variables[0]));
                    ADLXBackend.SetRSRSharpness(int.Parse(variables[1]));
                }
                if (command == "ADLX-Chill") ADLXBackend.SetChill(int.Parse(variables[0]), bool.Parse(variables[1]), int.Parse(variables[2]), int.Parse(variables[3]));
                if (command == "ADLX-Sync") ADLXBackend.SetEnhancedSync(int.Parse(variables[0]), bool.Parse(variables[1]));
                if (command == "ADLX-ImageSharp") ADLXBackend.SetImageSharpning(int.Parse(variables[0]), bool.Parse(variables[1]), int.Parse(variables[2]));
            }
            catch { }
        }

        private static void UXTUSR(string command, string value)
        {
            try
            {
                string[] variables = value.Split('-');

                if (command == "UXTUSR")
                {
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.AdapterIdx = 0;
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.isMagpie = Convert.ToBoolean(variables[0]);
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.VSync = Convert.ToBoolean(variables[1]);
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.Sharpness = Convert.ToDouble(variables[2]);
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.ResMode = Convert.ToInt32(variables[3]);
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.AutoRestore = Convert.ToBoolean(variables[0]);
                    Universal_x86_Tuning_Utility.Properties.Settings.Default.Save();
                }
            }
            catch { }
        }

        private static void NVIDIA(string command, string value)
        {
            try
            {
                string[] variables = value.Split('-');

                if (command == "NVIDIA-Clocks" && variables.Length == 2) NvTuning.SetClocks(int.Parse(variables[0]), int.Parse(variables[1]));
                else if (command == "NVIDIA-Clocks" && variables.Length == 3)
                {
                    NvTuning.SetMaxGPUClock(int.Parse(variables[0]));
                    NvTuning.SetClocks(int.Parse(variables[1]), int.Parse(variables[2]));
                }
            }
            catch { }
        }

        static bool isMessageBoxOpen = false, isUpdatingUltiMode = false;
        private static void AsusWmi(string command, string value)
        {
            try
            {
                uint id = 0;
                int mode = 0;
                if (command == "ASUS-Power")
                {
                    if (App.product.Contains("ROG") || App.product.Contains("TUF")) id = ASUSWmi.PerformanceMode;
                    else id = ASUSWmi.VivoBookMode;

                    mode = (int)ASUSWmi.AsusMode.Balanced;
                    if(value == "1") mode = (int)ASUSWmi.AsusMode.Silent;
                    else if (value == "2") mode = (int)ASUSWmi.AsusMode.Balanced;
                    else if (value == "3") mode = (int)ASUSWmi.AsusMode.Turbo;
                    if (App.wmi.DeviceGet(id) != mode) App.wmi.DeviceSet(id, mode, "PowerMode");
                }
                if(command == "ASUS-Eco")
                {
                    if(value.ToLower() == "true") App.wmi.SetGPUEco(1);
                    else App.wmi.SetGPUEco(0);
                }
                if (command == "ASUS-MUX")
                {
                    if (!isMessageBoxOpen && !isUpdatingUltiMode)
                    {
                        if (App.product.Contains("ROG") || App.product.Contains("TUF")) id = ASUSWmi.GPUMux;
                        else id = ASUSWmi.GPUMuxVivo;

                        int mux = App.wmi.DeviceGet(id);
                        if (mux > 0 && value.ToLower() == "true")
                        {
                            isMessageBoxOpen = true;

                            var messageBox = new Wpf.Ui.Controls.MessageBox();

                            messageBox.ButtonLeftName = "Restart";
                            messageBox.ButtonRightName = "Cancel";

                            messageBox.ButtonLeftClick += MessageBox_Enable;
                            messageBox.ButtonRightClick += MessageBox_Close;

                            messageBox.Show("GPU Ultimate Mode", "Switching the GPU to Ultimate Mode requires a restart to take\naffect!");


                        }
                        else if (mux < 1 && mux > -1 && value.ToLower() == "false")
                        {
                            isMessageBoxOpen = true;

                            var messageBox = new Wpf.Ui.Controls.MessageBox();

                            messageBox.ButtonLeftName = "Restart";
                            messageBox.ButtonRightName = "Cancel";

                            messageBox.ButtonLeftClick += MessageBox_Disable;
                            messageBox.ButtonRightClick += MessageBox_Close;

                            messageBox.Show("GPU Ultimate Mode", "Disabling GPU Ultimate Mode requires a restart to take\naffect!");
                        }
                    }
                }
            } 
            catch { }
        }

        private static void MessageBox_Enable(object sender, System.Windows.RoutedEventArgs e)
        {
            uint id = 0;
            if (App.product.Contains("ROG") || App.product.Contains("TUF")) id = ASUSWmi.GPUMux;
            else id = ASUSWmi.GPUMuxVivo;
            App.wmi.DeviceSet(id, 0, "MUX");
            Thread.Sleep(250);
            Process.Start("shutdown", "/r /t 1");

            (sender as Wpf.Ui.Controls.MessageBox)?.Close();
            isMessageBoxOpen = false;
            isUpdatingUltiMode = true;
        }

        private static void MessageBox_Disable(object sender, System.Windows.RoutedEventArgs e)
        {
            uint id = 0;
            if (App.product.Contains("ROG") || App.product.Contains("TUF")) id = ASUSWmi.GPUMux;
            else id = ASUSWmi.GPUMuxVivo;
            App.wmi.DeviceSet(id, 1, "MUX");
            Thread.Sleep(250);
            Process.Start("shutdown", "/r /t 1");

            (sender as Wpf.Ui.Controls.MessageBox)?.Close();
            isMessageBoxOpen = false;
            isUpdatingUltiMode = true;
        }

        private static void MessageBox_Close(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as Wpf.Ui.Controls.MessageBox)?.Close();
            isMessageBoxOpen = false;
        }
    }
}
