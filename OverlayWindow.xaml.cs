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
using LibreHardwareMonitor.Hardware;
using AATUV3.Properties;

namespace AATUV3
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {

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
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += SensorUpdate_Tick;
            //sensor.Start();

            autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default["AutoReapplyTime"]);
            autoReapply.Tick += SensorUpdate_Tick;
            autoReapply.Start();

            thisPC = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
            };
            thisPC.Open();
            thisPC.Accept(new UpdateVisitor());
        }

        public static Computer thisPC;

        private static Controller controller;
        private static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

        public static bool hidden = true;

        void AutoReapply_Tick(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(Settings.Default["AutoReapply"]) == true)
            {
                //Check if RyzenAdjArguments is populated
                if (Settings.Default["RyzenAdjArguments"].ToString() != null || Settings.Default["RyzenAdjArguments"].ToString() != "")
                {
                    //Cehck to make sure that Adpative Performance menu is not open
                    if (!MainWindow.menu.ToLower().Contains("adaptive"))
                    {
                        //Get RyzenAdj path
                        string path = "\\bin\\ryzenadj\\ryzenadj.exe";
                        //Pass settings on to be applied
                        Backend.ApplySettings(path, Settings.Default["RyzenAdjArguments"].ToString(), true);
                    }
                }
            }

            if(autoReapply.Interval != TimeSpan.FromSeconds((int)Settings.Default["AutoReapplyTime"]))
            {
                autoReapply.Stop();
                autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default["AutoReapplyTime"]);
                autoReapply.Start();
            }
        }


        void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            //Resize scrollview 
            rightView.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 54;
            rightView.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 54;

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
            }

            //detect if keyboard or controller combo is being activated
            if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.A) & KeyStates.Down) > 0)
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
            if (hidden == false)
            {
                Sensors.updateSensors();
                updateSensorsInfo();
                getCPUInfo();
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

        private void brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateBrightness(Convert.ToInt32(brightness.Value));
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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
                                if ((int)sensor.Value.GetValueOrDefault() < 10)
                                {
                                    lblTemp.Content = "0" + (int)sensor.Value.GetValueOrDefault() + "°C";
                                }
                                else
                                {
                                    lblTemp.Content = (int)sensor.Value.GetValueOrDefault() + "°C";
                                }
                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Package"))
                            {
                                if ((int)sensor.Value.GetValueOrDefault() < 10)
                                {
                                    lblPower.Content = "0" + (int)sensor.Value.GetValueOrDefault() + "w";
                                }
                                else
                                {
                                    lblPower.Content = (int)sensor.Value.GetValueOrDefault() + "w";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
