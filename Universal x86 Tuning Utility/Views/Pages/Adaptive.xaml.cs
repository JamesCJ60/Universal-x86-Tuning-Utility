using GameLib.Core;
using GameLib;
using LibreHardwareMonitor.Hardware;
using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Adaptive;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using static System.Net.Mime.MediaTypeNames;
using GameLib.Plugin.Steam.Model;
using Windows.ApplicationModel.Search;
using Windows.Gaming.Preview.GamesEnumeration;
using System.Management;
using RyzenSmu;
using Universal_x86_Tuning_Utility.Scripts.GPUs.AMD;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;
using System.ComponentModel;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for Automations.xaml
    /// </summary>
    public partial class Adaptive : Page
    {
        System.Windows.Threading.DispatcherTimer adaptiveMode = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer sensors = new System.Windows.Threading.DispatcherTimer();
        private static int coreCount = 0;
        public Adaptive()
        {
            InitializeComponent();

            _ = Tablet.TabletDevices;
            setUp();

            adaptiveMode.Interval = TimeSpan.FromSeconds(2);
            adaptiveMode.Tick += new EventHandler(adaptive_Tick);
            adaptiveMode.Start();

            sensors.Interval = TimeSpan.FromSeconds(2);
            sensors.Tick += new EventHandler(sensors_Tick);
            sensors.Start();

            nudPolling.Value = Settings.Default.polling;

            cbAutoSwitch.IsChecked = Settings.Default.autoSwitch;

            if (!Settings.Default.isASUS) sdAsusPower.Visibility = Visibility.Collapsed;
        }
        private static AdaptivePresetManager adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
        private async void setUp()
        {
            try
            {
                if (GetRadeonGPUCount() <= 0)
                {
                    sdTBOiGPU.Visibility = Visibility.Collapsed;
                    sdADLX.Visibility = Visibility.Collapsed;
                }

                if (GetNVIDIAGPUCount() < 1) sdNVIDIA.Visibility = Visibility.Collapsed;

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu || Family.FAM == Family.RyzenFamily.DragonRange) nudPowerLimit.Value = 86;
                else nudPowerLimit.Value = 28;
                nudMaxGfxClk.Value = 1900;
                nudMinGfxClk.Value = 400;
                nudTemp.Value = 95;
                nudMinCpuClk.Value = 1500;

                await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library());

                cbxPowerPreset.Items.Add("Default");
                foreach (GameLauncherItem item in Game_Manager.installedGames) cbxPowerPreset.Items.Add(item.gameName);

                cbxPowerPreset.SelectedIndex = 0;

                IEnumerable<string> presetNames = adaptivePresetManager.GetPresetNames();

                foreach (GameLauncherItem item in Game_Manager.installedGames)
                {
                    bool containsName = false;

                    foreach (string names in presetNames)
                    {
                        if (names.Contains(item.gameName)) containsName = true;
                    }

                    if (containsName == false)
                    {
                        AdaptivePreset preset = new AdaptivePreset
                        {
                            Temp = (int)nudTemp.Value,
                            Power = (int)nudPowerLimit.Value,
                            CO = (int)nudCurve.Value,
                            minGFX = (int)nudMinGfxClk.Value,
                            MaxGFX = (int)nudMaxGfxClk.Value,
                            minCPU = (int)nudMinCpuClk.Value,
                            isCO = (bool)cbCurve.IsChecked,
                            isGFX = (bool)tsTBOiGPU.IsChecked,
                            rsr = (int)nudRSR.Value,
                            boost = (int)nudBoost.Value,
                            imageSharp = (int)nudImageSharp.Value,
                            isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                            isRSR = (bool)cbRSR.IsChecked,
                            isBoost = (bool)cbBoost.IsChecked,
                            isAntiLag = (bool)cbAntiLag.IsChecked,
                            isImageSharp = (bool)cbImageSharp.IsChecked,
                            isSync = (bool)cbSync.IsChecked,
                        };
                        adaptivePresetManager.SavePreset(item.gameName, preset);
                    }

                    if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        spCO.Visibility = Visibility.Collapsed;
                        sdTBOiGPU.Visibility = Visibility.Collapsed;
                    }

                }

                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) coreCount += int.Parse(item["NumberOfCores"].ToString());

                btnStart.IsEnabled = true;
                btnSave.IsEnabled = true;

                if (Settings.Default.isStartAdpative) ToggleAdaptiveMode();
            }
            catch (Exception ex) { }
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }
        bool start = false;
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ToggleAdaptiveMode();
        }

        private async void ToggleAdaptiveMode()
        {
            try
            {
                if (start)
                {
                    start = false;
                    siStartIcon.Symbol = Wpf.Ui.Common.SymbolRegular.Play20;
                    tbxStartText.Text = "Start Adaptive Mode";
                    GetSensor.closeSensor();
                    Settings.Default.isAdaptiveModeRunning = false;
                    Settings.Default.Save();

                }
                else
                {
                    start = true;
                    siStartIcon.Symbol = Wpf.Ui.Common.SymbolRegular.Stop20;
                    tbxStartText.Text = "Stop Adaptive Mode";
                    await Task.Run(() => GetSensor.openSensor());
                    Settings.Default.isAdaptiveModeRunning = true;
                    Settings.Default.Save();
                }
            }
            catch { }
        }

        public static int CPUTemp, CPULoad, CPUClock, CPUPower, GPULoad, GPUClock, GPUMemClock;

        private void mainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(0, 0, -12, 0);
            else mainCon.Margin = new Thickness(0, 0, 0, 0);
        }
        int i = 0;

        private async void adaptive_Tick(object sender, EventArgs e)
        {
            if (start == true)
            {
                update();
            }
            if (Settings.Default.polling != nudPolling.Value)
            {
                Settings.Default.polling = (double)nudPolling.Value;
                Settings.Default.Save();
            }

            if (adaptiveMode.Interval != TimeSpan.FromSeconds((double)nudPolling.Value))
            {
                adaptiveMode.Stop();
                adaptiveMode.Interval = TimeSpan.FromSeconds((double)nudPolling.Value);
                adaptiveMode.Start();
            }
            if (sensors.Interval != TimeSpan.FromSeconds((double)nudPolling.Value))
            {
                sensors.Stop();
                sensors.Interval = TimeSpan.FromSeconds((double)nudPolling.Value);
                sensors.Start();
            }
        }

        private void cbxPowerPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string presetName = (sender as ComboBox).SelectedItem as string;
            loadPreset(presetName);
        }

        private void loadPreset(string presetName)
        {
            try
            {
                adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
                AdaptivePreset myPreset = adaptivePresetManager.GetPreset(presetName);

                if (myPreset != null)
                {
                    nudTemp.Value = myPreset.Temp;
                    nudPowerLimit.Value = myPreset.Power;
                    nudCurve.Value = myPreset.CO;
                    nudMaxGfxClk.Value = myPreset.MaxGFX;
                    nudMinGfxClk.Value = myPreset.minGFX;
                    nudMinCpuClk.Value = myPreset.minCPU;

                    cbCurve.IsChecked = myPreset.isCO;
                    tsTBOiGPU.IsChecked = myPreset.isGFX;

                    tsRadeonGraph.IsChecked = myPreset.isRadeonGraphics;
                    cbAntiLag.IsChecked = myPreset.isAntiLag;
                    cbRSR.IsChecked = myPreset.isRSR;
                    cbBoost.IsChecked = myPreset.isBoost;
                    cbImageSharp.IsChecked = myPreset.isImageSharp;
                    cbSync.IsChecked = myPreset.isSync;
                    nudRSR.Value = myPreset.rsr;
                    nudBoost.Value = myPreset.boost;
                    nudImageSharp.Value = myPreset.imageSharp;

                    tsNV.IsChecked = myPreset.isNVIDIA;
                    nudNVCore.Value = myPreset.nvCoreClk;
                    nudNVMem.Value = myPreset.nvMemClk;

                    cbxAsusPower.SelectedIndex = myPreset.asusPowerProfile;

                    tsUXTUSR.IsChecked = myPreset.isMag;
                    cbVSync.IsChecked = myPreset.isVsync;
                    cbAutoCap.IsChecked = myPreset.isRecap;
                    nudSharp.Value = myPreset.Sharpness;
                    cbxResScale.SelectedIndex = myPreset.ResScaleIndex;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void savePreset(string presetName)
        {
            try
            {
                AdaptivePreset preset = new AdaptivePreset
                {
                    Temp = (int)nudTemp.Value,
                    Power = (int)nudPowerLimit.Value,
                    CO = (int)nudCurve.Value,
                    minGFX = (int)nudMinGfxClk.Value,
                    MaxGFX = (int)nudMaxGfxClk.Value,
                    minCPU = (int)nudMinCpuClk.Value,
                    isCO = (bool)cbCurve.IsChecked,
                    isGFX = (bool)tsTBOiGPU.IsChecked,
                    rsr = (int)nudRSR.Value,
                    boost = (int)nudBoost.Value,
                    imageSharp = (int)nudImageSharp.Value,
                    isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                    isRSR = (bool)cbRSR.IsChecked,
                    isBoost = (bool)cbBoost.IsChecked,
                    isAntiLag = (bool)cbAntiLag.IsChecked,
                    isImageSharp = (bool)cbImageSharp.IsChecked,
                    isSync = (bool)cbSync.IsChecked,
                    isNVIDIA = (bool)tsNV.IsChecked,
                    nvCoreClk = (int)nudNVCore.Value,
                    nvMemClk = (int)nudNVMem.Value,
                    asusPowerProfile = (int)cbxAsusPower.SelectedIndex,
                    isMag = (bool)tsUXTUSR.IsChecked,
                    isVsync = (bool)cbVSync.IsChecked,
                    isRecap = (bool)cbAutoCap.IsChecked,
                    Sharpness = (int)nudSharp.Value,
                    ResScaleIndex = (int)cbxResScale.SelectedIndex,
                };
                adaptivePresetManager.SavePreset(presetName, preset);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private static LASTINPUTINFO lastInput = new LASTINPUTINFO();

        private static int minCPUClock = 1440;

        private async void btnReloadApps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cbxPowerPreset.ItemsSource = new List<string>();
                await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library());
                cbxPowerPreset.Items.Clear();
                cbxPowerPreset.Items.Add("Default");
                foreach (GameLauncherItem item in Game_Manager.installedGames) cbxPowerPreset.Items.Add(item.gameName);
                cbxPowerPreset.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            savePreset(cbxPowerPreset.SelectedItem.ToString());
        }

        private static int newMinCPUClock = 1440;
        private async void sensors_Tick(object sender, EventArgs e)
        {
            try
            {
                if (start == true)
                {
                    await Task.Run(() =>
                    {
                        //GetSensor.updateSensor();
                        GetSensor.updateCPU = true;
                        GetSensor.updateAMDGPU = true;
                        if (Family.TYPE == Family.ProcessorType.Intel) CPUTemp = (int)GetSensor.getCPUInfo(SensorType.Temperature, "Package");
                        else CPUTemp = (int)GetSensor.getCPUInfo(SensorType.Temperature, "Core");
                        CPULoad = (int)GetSensor.getCPUInfo(SensorType.Load, "Total");

                        int i = 1;
                        do
                        {
                            if (i <= i) CPUClock = CPUClock + (int)GetSensor.getCPUInfo(SensorType.Clock, $"Core #{i}");
                            i++;
                        }
                        while (i <= coreCount);

                        CPUClock = (int)(CPUClock / i);

                        //CPUPower = (int)GetSensor.getCPUInfo(SensorType.Power, "Package");

                        if (GetRadeonGPUCount() <= 0)
                        {
                            GPULoad = ADLXBackend.GetGPUMetrics(0, 7);
                            GPUClock = ADLXBackend.GetGPUMetrics(0, 0);
                            GPUMemClock = ADLXBackend.GetGPUMetrics(0, 1);
                        }

                        if (GetNVIDIAGPUCount() < 1) sdNVIDIA.Visibility = Visibility.Collapsed;
                    });


                    minCPUClock = Convert.ToInt32(nudMinCpuClk.Value);
                    if (CPULoad < (100 / coreCount) + 5) newMinCPUClock = minCPUClock + 500;
                    else newMinCPUClock = minCPUClock;

                    await Task.Run(() => isGameRunning());

                    if (cbxPowerPreset.Items.Count > 0 && cbAutoSwitch.IsChecked == true)
                    {
                        string selectedGameName = string.Empty;

                        Dispatcher.Invoke(() =>
                        {
                            selectedGameName = cbxPowerPreset.SelectedItem.ToString();
                        });

                        if (selectedGameName != runningGameName)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                getRunningGame(runningGameName);
                            });
                        }
                    }

                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public static int GetRadeonGPUCount()
        {
            int count = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] as string;
                    if (name != null && name.Contains("Radeon"))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static int GetNVIDIAGPUCount()
        {
            int count = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] as string;
                    if (name != null && name.Contains("NVIDIA"))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        string lastCPU = "";
        string lastCO = "";
        string lastiGPU = "";
        private async void update()
        {
            try
            {
                if (start == true)
                {
                    if (i < 2)
                    {
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, (int)nudPowerLimit.Value, (int)nudPowerLimit.Value - 5, (int)nudTemp.Value);
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, (int)nudPowerLimit.Value, (int)nudPowerLimit.Value - 5, (int)nudTemp.Value);
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, (int)nudPowerLimit.Value, (int)nudPowerLimit.Value - 5, (int)nudTemp.Value);
                        i++;
                    }
                    else
                    {
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, (int)nudPowerLimit.Value, 8, (int)nudTemp.Value);

                        if (cbCurve.IsChecked == true) CPUControl.CurveOptimiserLimit(CPULoad, (int)nudCurve.Value);

                        if (tsTBOiGPU.IsChecked == true) iGPUControl.UpdateiGPUClock((int)nudMaxGfxClk.Value, (int)nudMinGfxClk.Value, (int)nudTemp.Value, CPUPower, CPUTemp, GPUClock, GPULoad, GPUMemClock, CPUClock, minCPUClock);

                        string commandString = "";

                        commandString = commandString + $"--UXTUSR={tsUXTUSR.IsChecked}-{cbVSync.IsChecked}-{nudSharp.Value / 100}-{cbxResScale.SelectedIndex}-{cbAutoCap.IsChecked} ";

                        if (Settings.Default.isASUS)
                        {
                            if (cbxAsusPower.SelectedIndex > 0) commandString = commandString + $"--ASUS-Power={cbxAsusPower.SelectedIndex} ";
                        }

                        if (CPUControl.cpuCommand != lastCPU)
                        {
                            commandString = commandString + CPUControl.cpuCommand;
                            lastCPU = CPUControl.cpuCommand;
                        }

                        if (CPUControl.coCommand != null && CPUControl.coCommand != "" && cbCurve.IsChecked == true && CPUControl.coCommand != lastCO)
                        {
                            commandString = commandString + CPUControl.coCommand;
                            lastCO = CPUControl.coCommand;
                        }

                        if (iGPUControl.commmand != null && iGPUControl.commmand != "" && tsTBOiGPU.IsChecked == true && iGPUControl.commmand != lastiGPU)
                        {
                            commandString = commandString + iGPUControl.commmand;
                            lastiGPU = iGPUControl.commmand;
                        }

                        if (tsRadeonGraph.IsChecked == true)
                        {
                            if (cbAntiLag.IsChecked == true) commandString = commandString + $"--ADLX-Lag=0-true --ADLX-Lag=1-true ";
                            else commandString = commandString + $"--ADLX-Lag=0-false --ADLX-Lag=1-false ";

                            if (cbRSR.IsChecked == true) commandString = commandString + $"--ADLX-RSR=true-{(int)nudRSR.Value} ";
                            else commandString = commandString + $"--ADLX-RSR=false-{(int)nudRSR.Value} ";

                            if (cbBoost.IsChecked == true) commandString = commandString + $"--ADLX-Boost=0-true-{(int)nudBoost.Value} --ADLX-Boost=1-true-{(int)nudBoost.Value} ";
                            else commandString = commandString + $"--ADLX-Boost=0-false-{(int)nudBoost.Value} --ADLX-Boost=1-false-{(int)nudBoost.Value} ";

                            if (cbImageSharp.IsChecked == true) commandString = commandString + $"--ADLX-ImageSharp=0-true-{(int)nudImageSharp.Value} --ADLX-ImageSharp=1-true-{(int)nudImageSharp.Value} ";
                            else commandString = commandString + $"--ADLX-ImageSharp=0-false-{(int)nudImageSharp.Value} --ADLX-ImageSharp=1-false-{(int)nudImageSharp.Value} ";

                            if (cbSync.IsChecked == true) commandString = commandString + $"--ADLX-Sync=0-true --ADLX-Sync=1-true ";
                            else commandString = commandString + $"--ADLX-Sync=0-false --ADLX-Sync=1-false ";
                        }

                        if (tsNV.IsChecked == true)
                        {
                            commandString = commandString + $"--NVIDIA-Clocks={nudNVCore.Value}-{nudNVMem.Value} ";
                        }

                        if (commandString != null && commandString != "") await Task.Run(() => RyzenAdj_To_UXTU.Translate(commandString));
                    }

                    if (RTSS.RTSSRunning() && tsRTSS.IsChecked == true) RTSS.setRTSSFPSLimit((int)nudRTSS.Value);
                    

                    //if (RTSS.RTSSRunning())
                    //{
                    //    int i = 0;
                    //    bool found = false;
                    //    do
                    //    {
                    //        AppFlags appFlag = RunningGames.appFlags[i];
                    //        var appEntries = OSD.GetAppEntries(appFlag);
                    //        foreach (var app in appEntries)
                    //        {
                    //            found = true;
                    //            osd.Update($"{RunningGames.appFlags[i]} {app.InstantaneousFrames}FPS {app.InstantaneousFrameTime.Milliseconds}ms");
                    //        }
                    //        i++;
                    //    } while (i < RunningGames.appFlags.Count && found == false);
                    //}
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public bool IsScrollBarVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null) throw new ArgumentNullException(nameof(scrollViewer));

            return scrollViewer.ExtentHeight > scrollViewer.ViewportHeight;
        }

        private void cbAutoSwitch_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.autoSwitch = (bool)cbAutoSwitch.IsChecked;
            Settings.Default.Save();
        }

        private static LauncherManager launcherManager = new LauncherManager(new LauncherOptions() { QueryOnlineData = true });

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Garbage.Garbage_Collect();
        }


        string runningGameName = "Default";
        private void isGameRunning()
        {
            foreach (GameLauncherItem item in installedGames)
            {
                //var gamePath = game.Split("~");

                int i = 0;
                do
                {
                    Process[] processes = Process.GetProcesses();

                    foreach (Process process in processes)
                    {
                        try
                        {
                            string executablePath = process.MainModule.FileName;
                            string executableDirectory = System.IO.Path.GetDirectoryName(executablePath);
                            string executableName = System.IO.Path.GetFileName(executablePath);

                            if (executablePath.Contains(item.path))
                            {
                                runningGameName = item.gameName;
                                return;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    i++;
                } while (i < 2);
            }
            runningGameName = "Default";
        }


        private void getRunningGame(string presetName)
        {
            int selectedIndex = 0; // index to select if the search fails

            foreach (var item in cbxPowerPreset.Items)
            {
                if (item.ToString() == presetName)
                {
                    cbxPowerPreset.SelectedItem = item;
                    return;
                }
            }
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
            if (checkBox == cbBoost)
            {
                cbRSR.IsChecked = false;
                cbAntiLag.IsChecked = false;
            }

            if (checkBox == cbAntiLag)
            {
                cbBoost.IsChecked = false;
            }

            if (checkBox == cbRSR)
            {
                cbBoost.IsChecked = false;
                cbImageSharp.IsChecked = false;
            }

            if (checkBox == cbImageSharp) cbRSR.IsChecked = false;
        }
    }
}
