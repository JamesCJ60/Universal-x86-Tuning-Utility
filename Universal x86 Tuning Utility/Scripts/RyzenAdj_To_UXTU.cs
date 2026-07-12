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
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

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

        public static void Translate(string ryzenAdjString, bool isAutoReapply = false, bool isAutoOC = false) =>
            _ = TranslateAsync(ryzenAdjString, isAutoReapply, isAutoOC);

        public static Task TranslateAsync(string ryzenAdjString, bool isAutoReapply = false, bool isAutoOC = false) =>
            Task.Run(() => TranslateCore(ryzenAdjString, isAutoReapply, isAutoOC));

        private static void TranslateCore(string ryzenAdjString, bool isAutoReapply, bool isAutoOC)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ryzenAdjString))
                    return;

                string normalizedCommands = ryzenAdjString.Trim();
                string[] ryzenAdjCommands = normalizedCommands
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();

                DiagnosticLogger.LogDebug($"Translate string: {normalizedCommands}");
                foreach (string ryzenAdjCommand in ryzenAdjCommands)
                {
                        try
                        {
                            int separatorIndex = ryzenAdjCommand.IndexOf('=');
                            string ryzenAdjCommandString = (separatorIndex >= 0 ? ryzenAdjCommand[..separatorIndex] : ryzenAdjCommand).TrimStart('-');
                            string ryzenAdjCommandValueString = separatorIndex >= 0 ? ryzenAdjCommand[(separatorIndex + 1)..] : "0";

                            DiagnosticLogger.LogDebug($"Processing: {ryzenAdjCommandString}={ryzenAdjCommandValueString}");

                            if (ryzenAdjCommandString.Contains("UXTUSR"))
                            {
                                UXTUSR(ryzenAdjCommandString, ryzenAdjCommandValueString);
                            }
                            else if (ryzenAdjCommandString.Contains("CCD-Affinity"))
                            {
                                CpuAffinityManager.SetGlobalAffinity(Convert.ToInt32(ryzenAdjCommandValueString));
                            }
                            else if (ryzenAdjCommandString.Contains("Win-Power"))
                            {
                                if(ryzenAdjCommandValueString == "0") PowerSetActiveOverlayScheme(new Guid(powerSaverPowerScheme.ToLower()));
                                else if (ryzenAdjCommandValueString == "1") PowerSetActiveOverlayScheme(new Guid(balancedPowerScheme.ToLower()));
                                else if (ryzenAdjCommandValueString == "2") PowerSetActiveOverlayScheme(new Guid(highPerformancePowerScheme.ToLower()));
                            }
                            else if (ryzenAdjCommandString.Contains("ASUS"))
                            {
                                AsusWmi(ryzenAdjCommandString, ryzenAdjCommandValueString);
                            }
                            else if (ryzenAdjCommandString.Contains("Refresh-Rate"))
                            {
                                Universal_x86_Tuning_Utility.Scripts.Misc.Display.ApplySettings(Convert.ToInt32(ryzenAdjCommandValueString));
                            }
                            else if (ryzenAdjCommandString.Contains("ADLX"))
                            {
                                ADLX(ryzenAdjCommandString, ryzenAdjCommandValueString);
                            }
                            else if (ryzenAdjCommandString.Contains("NVIDIA"))
                            {
                                NVIDIA(ryzenAdjCommandString, ryzenAdjCommandValueString);
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
                                    int ryzenAdjCommandValue = Convert.ToInt32(ryzenAdjCommandValueString);

                                    if (ryzenAdjCommandString == "intel-pl") Intel_Management.changeTDPAll(ryzenAdjCommandValue);
                                    else if (ryzenAdjCommandString == "intel-volt-cpu") Intel_Management.changeVoltageOffset(ryzenAdjCommandValue, 0);
                                    else if (ryzenAdjCommandString == "intel-volt-gpu") Intel_Management.changeVoltageOffset(ryzenAdjCommandValue, 1);
                                    else if (ryzenAdjCommandString == "intel-volt-cache") Intel_Management.changeVoltageOffset(ryzenAdjCommandValue, 2);
                                    else if (ryzenAdjCommandString == "intel-volt-sa") Intel_Management.changeVoltageOffset(ryzenAdjCommandValue, 3);
                                    else if (ryzenAdjCommandString == "intel-bal-cpu") Intel_Management.changePowerBalance(ryzenAdjCommandValue, 0);
                                    else if (ryzenAdjCommandString == "intel-bal-gpu") Intel_Management.changePowerBalance(ryzenAdjCommandValue, 1);
                                    else if (ryzenAdjCommandString == "intel-gpu") Intel_Management.changeGpuClock(ryzenAdjCommandValue);
                                }
                            }
                            else
                            {
                                uint ryzenAdjCommandValue = Convert.ToUInt32(ryzenAdjCommandValueString);

                                if (ryzenAdjCommand.Contains("skin")) ryzenAdjCommandValue *= 256;

                                if (ryzenAdjCommand.Contains("coall") && Settings.Default.isAutoUvCPU == true && isAutoOC == false) continue;
                                if (ryzenAdjCommand.Contains("coper") && Settings.Default.isAutoUvCPU == true && isAutoOC == false) continue;
                                if (ryzenAdjCommand.Contains("cogfx") && Settings.Default.isAutoUviGPU == true && isAutoOC == false) continue;

                                if (ryzenAdjCommandValue <= 0 && !ryzenAdjCommandString.Contains("co")) SMUCommands.applySettings(ryzenAdjCommandString, 0x0);
                                else SMUCommands.applySettings(ryzenAdjCommandString, ryzenAdjCommandValue);

                                DiagnosticLogger.LogDebug($"SMU applied: {ryzenAdjCommandString}=0x{ryzenAdjCommandValue:X}");
                            }
                        }
                        catch (Exception ex)
                        {
                            DiagnosticLogger.LogError(ex, $"Failed to process command: {ryzenAdjCommand}");
                        }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to translate RyzenAdj command string");
            }
        }

        private static void ADLX(string command, string value)
        {
            try
            {
                DiagnosticLogger.LogDebug($"ADLX: {command}={value}");
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
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply ADLX settings");
            }
        }

        private static void UXTUSR(string command, string value)
        {
            try
            {
                DiagnosticLogger.LogDebug($"UXTUSR: {command}={value}");
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
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply UXTUSR settings");
            }
        }

        private static void NVIDIA(string command, string value)
        {
            try
            {
                DiagnosticLogger.LogDebug($"NVIDIA: {command}={value}");
                string[] variables = value.Split('-');

                if (command == "NVIDIA-Clocks" && variables.Length == 2) NvTuning.SetClocks(int.Parse(variables[0]), int.Parse(variables[1]));
                else if (command == "NVIDIA-Clocks" && variables.Length == 3)
                {
                    NvTuning.SetMaxGPUClock(int.Parse(variables[0]));
                    NvTuning.SetClocks(int.Parse(variables[1]), int.Parse(variables[2]));
                }
                else if (command == "NVIDIA-Clocks" && variables.Length == 4)
                {
                    NvTuning.SetMaxGPUClock(int.Parse(variables[0]));
                    NvTuning.SetClocks(int.Parse(variables[1]), int.Parse(variables[2]));
                    NvTuning.SetPowerLimit(int.Parse(variables[3]));
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply NVIDIA clocks");
            }
        }

        static bool isMessageBoxOpen = false, isUpdatingUltiMode = false;
        private static async void AsusWmi(string command, string value)
        {
            try
            {
                DiagnosticLogger.LogDebug($"AsusWmi: {command}={value}");
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

                            messageBox.Title = LocalizationService.Get("GPU Ultimate Mode");
                            messageBox.Content = LocalizationService.Get("Switching the GPU to Ultimate Mode requires a restart to take effect!");
                            messageBox.PrimaryButtonText = LocalizationService.Get("Restart");
                            messageBox.CloseButtonText = LocalizationService.Get("Cancel");

                            if (await messageBox.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary)
                                RestartWithMuxMode(id, 0);
                            else
                                isMessageBoxOpen = false;


                        }
                        else if (mux < 1 && mux > -1 && value.ToLower() == "false")
                        {
                            isMessageBoxOpen = true;

                            var messageBox = new Wpf.Ui.Controls.MessageBox();

                            messageBox.Title = LocalizationService.Get("GPU Ultimate Mode");
                            messageBox.Content = LocalizationService.Get("Disabling GPU Ultimate Mode requires a restart to take effect!");
                            messageBox.PrimaryButtonText = LocalizationService.Get("Restart");
                            messageBox.CloseButtonText = LocalizationService.Get("Cancel");

                            if (await messageBox.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary)
                                RestartWithMuxMode(id, 1);
                            else
                                isMessageBoxOpen = false;
                        }
                    }
                }
            } 
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply ASUS WMI settings");
            }
        }

        private static void RestartWithMuxMode(uint id, int mode)
        {
            App.wmi.DeviceSet(id, mode, "MUX");
            isMessageBoxOpen = false;
            isUpdatingUltiMode = true;
            Process.Start("shutdown", "/r /t 1");
        }

    }
}
