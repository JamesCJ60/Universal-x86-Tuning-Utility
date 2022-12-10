using AATUV3.Scripts;
using AudioSwitcher.AudioApi.CoreAudio;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using UXTU.Scripts.Intel;
using LibreHardwareMonitor.Hardware;
using UXTU.Properties;
using RyzenSMUBackend;
using System.Windows.Forms;
using UXTU.Scripts;
using UXTU.Scripts.Adpative_Modes.Performance;
using AATUV3.Scripts.SMU_Backend_Scripts;
using MessageBox = System.Windows.MessageBox;

namespace AATUV3
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public int NVGPU = 0;
        public DispatcherTimer autoReapply = new DispatcherTimer();
        public OverlayWindow()
        {
            InitializeComponent();
            this.WindowState = System.Windows.WindowState.Maximized;
            this.Hide();

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.15);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            //checkKeyInput.Start();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(1);
            sensor.Tick += SensorUpdate_Tick;
            sensor.Start();

            //set up timer for sensor update
            DispatcherTimer Adaptive = new DispatcherTimer();
            Adaptive.Interval = TimeSpan.FromSeconds(1);
            Adaptive.Tick += Adaptive_Tick;
            Adaptive.Start();

            autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
            autoReapply.Tick += AutoReapply_Tick;
            autoReapply.Start();

            if (MainWindow.AppName.Contains("Intel"))
            {
                CO.Visibility = Visibility.Collapsed;
            }
            else
            {
                CO.Visibility = Visibility.Visible;
            }

            iGPU.Visibility = Visibility.Collapsed;

            if (MainWindow.AppName.Contains("AMD APU"))
            {
                iGPU.Visibility = Visibility.Visible;
            }

            ASUSAC.Visibility = Visibility.Collapsed;

            //if (MainWindow.mbo.Contains("asus"))
            //{
            //    ASUSAC.Visibility = Visibility.Visible;
            //}

            thisPC = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
            };
            thisPC.Open();
            thisPC.Accept(new UpdateVisitor());

            getGPUInfo();

            if (NVGPU <= 0)
            {
                NVDGPU.Visibility = Visibility.Collapsed;
            }
        }

        public static Computer thisPC;

        private static Controller controller;
        private static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

        public static bool hidden = true;


        void Adaptive_Tick(object sender, EventArgs e)
        {
            if (GlobalVariables.AdaptivePerf == true)
            {
                int maxPower, minPower, maxTemp, maxCO;

                maxCO = 10;
                maxPower = 65;
                minPower = 5;
                maxTemp = 95;

                if (Families.FAMID == 3 || Families.FAMID == 7)
                {
                    maxTemp = 100;

                    if (Settings.Default.CPUName.Contains("GE")) { maxPower = 75; maxCO = 20; minPower = 8; maxTemp = 95; }
                    else if (Settings.Default.CPUName.Contains("G")) { maxPower = 100; maxCO = 22; minPower = 8; maxTemp = 95; }
                    else if (Settings.Default.CPUName.Contains("HX")) { maxPower = 105; maxCO = 22; }
                    else if (Settings.Default.CPUName.Contains("HS")) { maxPower = 65; maxCO = 18; }
                    else if (Settings.Default.CPUName.Contains("H")) { maxPower = 80; maxCO = 16; }
                    else if (Settings.Default.CPUName.Contains("U")) { maxPower = 40; maxCO = 14; }
                    else maxPower = 65;

                    int cpuTemp = (int)GetSensor.getSensorValve("THM_VALUE_CORE");

                    CpuPowerLimiter.GetCurrentPowerLimit(maxPower, minPower, maxTemp);
                    CpuPowerLimiter.UpdatePowerLimit(cpuTemp, cpuLoad, maxPower, minPower, maxTemp);
                    CpuPowerLimiter.CurveOptimiserLimit(cpuLoad);
                }
                else if (Families.FAMID == 4 || Families.FAMID == 6)
                {
                    minPower = 35;

                    if (Settings.Default.CPUName.Contains("Ryzen 9") && Settings.Default.CPUName.Contains("X")) { maxPower = 235; maxCO = 15; }
                    else if (Settings.Default.CPUName.Contains("Ryzen 9") && Settings.Default.CPUName.Contains("X3D")) { maxPower = 235; maxCO = 28; }
                    else if (Settings.Default.CPUName.Contains("Ryzen 7") && Settings.Default.CPUName.Contains("5800X3D")) { maxPower = 140; maxCO = 35; }
                    else if (Settings.Default.CPUName.Contains("Ryzen 7") && Settings.Default.CPUName.Contains("X3D")) { maxPower = 230; maxCO = 35; }
                    else if (Settings.Default.CPUName.Contains("Ryzen 7") && Settings.Default.CPUName.Contains("X")) { maxPower = 160; maxCO = 15; }
                    else if (Settings.Default.CPUName.Contains("Ryzen 5") && Settings.Default.CPUName.Contains("X")) { maxPower = 140; maxCO = 15; }
                    else
                    {
                        maxPower = 95; maxCO = 15;
                    }

                    int cpuTemp = (int)GetSensor.getSensorValve("THM_VALUE");

                    CpuPowerLimiter.GetCurrentPowerLimit(maxPower, minPower, maxTemp);
                    CpuPowerLimiter.UpdatePowerLimit(cpuTemp, cpuLoad, maxPower, minPower, maxTemp);
                    CpuPowerLimiter.CurveOptimiserLimit(cpuLoad);
                }
            }

        }

        public int cpuLoad = 0;

        void AutoReapply_Tick(object sender, EventArgs e)
        {
            if ((bool)Settings.Default.AutoReapply == true && GlobalVariables.AdaptivePerf == false)
            {
                string commands = (string)Settings.Default.RyzenAdjArguments;
                //Check if RyzenAdjArguments is populated
                if (commands != null || commands != "")
                {
                    //Check to make sure that Adpative Performance menu is not open
                    if (!MainWindow.menu.ToLower().Contains("adaptive"))
                    {
                        //Get RyzenAdj path
                        string path = "\\bin\\ryzenadj\\ryzenadj.exe";
                        //Pass settings on to be applied
                        BasicExeBackend.ApplySettings(path, commands, true);
                    }
                }
            }

            if (autoReapply.Interval != TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime))
            {
                autoReapply.Stop();
                autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
                autoReapply.Start();
            }
        }



        public static string[] widthArray;
        public static string[] heightArray;
        public static string[] refreshArray;


        void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            //Resize scrollview 
            rightView.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 54;
            rightView.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 54;

            MainPan.Visibility = Visibility.Visible;

            //Get controller
            controller = new Controller(UserIndex.One);

            bool connected = controller.IsConnected;

            if (connected)
            {
                //get controller state
                var state = controller.GetState();

                //detect if keyboard or controller combo is being activated
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    //if hidden show window
                    if (hidden == false)
                    {
                        hidden = true;
                        this.Hide();
                    }
                    //else hide window
                    else
                    {
                        hidden = false;
                        this.Show();
                    }
                }

                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    SendKeys.SendWait("%F");
                }
            }

            //detect if keyboard or controller combo is being activated
            if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.M) & KeyStates.Down) > 0)
            {
                //if hidden show window
                if (hidden == false)
                {
                    hidden = true;
                    this.Hide();
                }
                //else hide window
                else
                {
                    Sensors.i = 0;
                    i = 0;
                    hidden = false;
                    this.Show();
                    Sensors.updateSensors();
                    updateSensorsInfo();
                }
            }

        }

        void SensorUpdate_Tick(object sender, EventArgs e)
        {
            if (hidden == false || GlobalVariables.AdaptivePerf == true)
            {
                Sensors.updateSensors();
                if (hidden == false) updateSensorsInfo();
                getCPUInfo();
            }

            if (MainWindow.AppName.Contains("AMD"))
            {
                Addresses.UpdateTable();
            }
        }

        int i = 0;

        private async void updateSensorsInfo()
        {
            lblBattery.Content = Sensors.battery.ToString() + "%";
            lblTime.Content = Sensors.time.ToString();

            if (i < 1)
            {
                if (brightness.Value != Convert.ToInt32(Sensors.brightness) && brightness.IsMouseDirectlyOver != true) brightness.Value = Convert.ToInt32(Sensors.brightness);

                //if (volume.Value != Convert.ToInt32(Sensors.volume) && volume.IsMouseDirectlyOver != true) volume.Value = Convert.ToInt32(Sensors.volume);

                i++;
            }
        }

        private void brightness_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            updateBrightness(Convert.ToInt32(brightness.Value));
        }

        private void volume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            updateVolume(Convert.ToInt32(volume.Value));
        }

        static async void updateBrightness(int newBirghtness)
        {
            await Task.Run(() =>
            {
                var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
                {
                    Scope = new ManagementScope(@"\\.\root\wmi")
                };
                var instances = mclass.GetInstances();
                var args = new object[] { 1, newBirghtness };
                foreach (ManagementObject instance in instances)
                {
                    instance.InvokeMethod("WmiSetBrightness", args);
                }
                GC.SuppressFinalize(true);
                return;
            });
        }

        static async void updateVolume(int newVolume)
        {
            await Task.Run(() =>
            {
                //Set volume of current sound device
                defaultPlaybackDevice.Volume = newVolume;
                return;
            });
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        public void getCPUInfo()
        {
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                            {

                                lblTemp.Content = (int)sensor.Value.GetValueOrDefault() + "°C";

                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Package"))
                            {

                                lblPower.Content = (int)sensor.Value.GetValueOrDefault() + "w";

                            }

                            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Total"))
                            {
                                cpuLoad = (int)sensor.Value.GetValueOrDefault();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void getGPUInfo()
        {
            foreach (var hardware in thisPC.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        NVGPU = 1;
                    }
                }
            }
        }

        private void NVdGPU_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //Get RyzenAdj path
            string path = "\\bin\\oc.exe";
            //Pass settings on to be applied
            BasicExeBackend.ApplySettings(path, "0 " + (int)core.Value + " " + (int)mem.Value, true);
            BasicExeBackend.ApplySettings(path, "1 " + (int)core.Value + " " + (int)mem.Value, true);
            BasicExeBackend.ApplySettings(path, "2 " + (int)core.Value + " " + (int)mem.Value, true);
        }

        private void dGPU_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblCore.Content = ((int)core.Value).ToString() + "MHz";
            lblMem.Content = ((int)mem.Value).ToString() + "MHz";
        }

        private void COCPU_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (COCPU.Value >= 0)
            {
                SendCommand.set_coall((uint)COCPU.Value);
            }
            else
            {
                SendCommand.set_coall(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)COCPU.Value)));
            }
        }

        private void iGPUCO_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (iGPUCO.Value >= 0)
            {
                SendCommand.set_cogfx((uint)iGPUCO.Value);
            }
            else
            {
                SendCommand.set_cogfx(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)iGPUCO.Value)));
            }
        }

        private void Preset1_Click(object sender, RoutedEventArgs e)
        {
            int preset1 = 25;
            string preset1Name = "";

            applySettings(preset1, preset1Name);
        }

        private void Preset2_Click(object sender, RoutedEventArgs e)
        {
            int preset2 = 35;
            string preset2Name = "";

            applySettings(preset2, preset2Name);
        }

        private void Preset3_Click(object sender, RoutedEventArgs e)
        {
            int preset3 = 45;
            string preset3Name = "";

            applySettings(preset3, preset3Name);
        }

        private void Preset4_Click(object sender, RoutedEventArgs e)
        {
            int preset4 = 55;
            string preset4Name = "";

            applySettings(preset4, preset4Name);
        }


        public static async void applySettings(int tdp, string presetName)
        {
            string ryzenadj = "";

            if (MainWindow.AppName.Contains("AMD APU"))
            {
                if (presetName == "" || presetName == null)
                {
                    SendCommand.set_tctl_temp(95);
                    SendCommand.set_apu_skin_temp_limit(95);
                    SendCommand.set_fast_limit((uint)tdp * 1000);
                    SendCommand.set_slow_limit((uint)tdp * 1000);
                    SendCommand.set_stapm_limit((uint)tdp * 1000);
                    SendCommand.set_vrm_current((uint)((tdp * 1000) * 1.33));
                    SendCommand.set_vrmmax_current((uint)((tdp * 1000) * 1.33));
                    ryzenadj = $"--stapm-limit={tdp * 1000} --fast-limit={tdp * 1000} --slow-limit={tdp * 1000} --tctl-temp={95} --apu-skin-temp={95} --vrm-current={(uint)((tdp * 1000) * 1.2)} --vrmmax-current={(uint)((tdp * 1000) * 1.2)}";
                }
                else
                {
                    //TODO: load custom presets
                }
            }
            else if (MainWindow.AppName.Contains("AMD CPU"))
            {
                if (presetName == "" || presetName == null)
                {
                    SendCommand.set_ppt((uint)tdp * 1000);
                    SendCommand.set_edc((uint)((tdp * 1000) * 1.33));
                    SendCommand.set_tdc((uint)((tdp * 1000) * 1.33));
                }
            }
            else if (MainWindow.AppName.Contains("Intel CPU"))
            {
                if (presetName == "" || presetName == null)
                {
                    await Task.Run(() => ChangeTDP.changeTDP(tdp, tdp));
                }
            }

            Settings.Default["RyzenAdjArguments"] = ryzenadj;
            Settings.Default.Save();
        }

        private void CO_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbliGPUCO.Content = ((int)iGPUCO.Value).ToString() + " Offset";
            lblCOCPU.Content = ((int)COCPU.Value).ToString() + " Offset";
        }

        private void core_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SendCommand.set_gfx_clk((uint)((int)iGP.Value));
        }

        private void core_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            lbliGPCore.Content = ((int)iGP.Value).ToString() + "MHz";
        }
    }
}
