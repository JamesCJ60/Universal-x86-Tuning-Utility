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
using Microsoft.Diagnostics.Tracing.Session;
using System.Windows.Forms;
using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSMUBackend;
using RyzenSmu;

namespace AATUV3
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class SensorWindow : Window
    {

        public static int iGPUTemp;
        public static int iGPUClock;
        public static int iGPUSoCClock;
        public static int iGPUMemClock;
        public static int iGPUPower;
        public static int iGPULoad;
        public static float iGPUMemUsed;
        public static float iGPUVolt;
        public static int fabricClk;

        public static int battery;
        public static float batTime;

        public static int Framerate = 1;

        int cpuTemp;
        int cpuClock;
        int cpuLoad;
        int cpuPower;
        float cpuVolt;

        int RAMLoad;
        int RAMUsed;


        public int f = 1;

        public const int EventID_D3D9PresentStart = 1;
        public const int EventID_DxgiPresentStart = 42;

        //ETW provider codes
        public static readonly Guid DXGI_provider = Guid.Parse("{CA11C036-0102-4A2D-A6AD-F03CFED5D3C9}");
        public static readonly Guid D3D9_provider = Guid.Parse("{783ACA0A-790E-4D7F-8451-AA850511C6B9}");

        static TraceEventSession m_EtwSession;
        static Dictionary<int, TimestampCollection> frames = new Dictionary<int, TimestampCollection>();
        static Stopwatch watch = null;
        static object sync = new object();

        static void EtwThreadProc()
        {
            //start tracing
            m_EtwSession.Source.Process();
        }

        public SensorWindow()
        {
            InitializeComponent();

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.15);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(1);
            sensor.Tick += SensorUpdate_Tick;
            sensor.Start();

            thisPC = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsBatteryEnabled = true,
                IsPsuEnabled = true,
                IsControllerEnabled = true,
                IsMotherboardEnabled = true
            };
            thisPC.Open();
            thisPC.Accept(new UpdateVisitor());

            updateSensorsInfo();
        }

        public static Thread thETW = new Thread(EtwThreadProc);
        public static Thread thOutput = new Thread(OutputThreadProc);

        public void startFPS()
        {
            try
            {
                //create ETW session and register providers
                m_EtwSession = new TraceEventSession("mysess");
                m_EtwSession.StopOnDispose = true;
                m_EtwSession.EnableProvider("Microsoft-Windows-D3D9");
                m_EtwSession.EnableProvider("Microsoft-Windows-DXGI");

                //handle event
                m_EtwSession.Source.AllEvents += data =>
                {
                    //filter out frame presentation events
                    if (((int)data.ID == EventID_D3D9PresentStart && data.ProviderGuid == D3D9_provider) ||
                        ((int)data.ID == EventID_DxgiPresentStart && data.ProviderGuid == DXGI_provider))
                    {
                        int pid = data.ProcessID;
                        long t;

                        lock (sync)
                        {
                            t = watch.ElapsedMilliseconds;

                            //if process is not yet in Dictionary, add it
                            if (!frames.ContainsKey(pid))
                            {
                                frames[pid] = new TimestampCollection();

                                string name = "";
                                var proc = Process.GetProcessById(pid);
                                if (proc != null)
                                {
                                    using (proc)
                                    {
                                        name = proc.ProcessName;
                                    }
                                }
                                else name = pid.ToString();

                                frames[pid].Name = name;
                            }

                            //store frame timestamp in collection
                            frames[pid].Add(t);
                        }
                    }
                };

                watch = new Stopwatch();
                watch.Start();

                thETW.IsBackground = true;
                thETW.Start();

                thOutput.IsBackground = true;
                thOutput.Start();
            }
            catch (Exception ex)
            {
            }
        }

        public static Computer thisPC;

        private static Controller controller;
        private static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

        public static bool hidden = false;
        void KeyShortCuts_Tick(object sender, EventArgs e)
        {

            //Get controller
            controller = new Controller(UserIndex.One);

            bool connected = controller.IsConnected;

            if (connected)
            {
                //get controller state
                var state = controller.GetState();

                //detect if keyboard or controller combo is being activated
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
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
            if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.O) & KeyStates.Down) > 0)
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
                    updateSensorsInfo();
                }
            }

        }

        void SensorUpdate_Tick(object sender, EventArgs e)
        {
            if (hidden == false)
            {
                updateSensorsInfo();
            }

            Addresses.UpdateTable();
        }

        int i = 0;
        int d = 1;

        private void updateSensorsInfo()
        {
            try
            {
                if (i < 1)
                {
                    startFPS();
                    i++;
                }


                OutputThreadProc();

                getCPUInfo();
                getGPUInfo();
                getRAMInfo();


                PowerStatus pwr = System.Windows.Forms.SystemInformation.PowerStatus;
                //Get battery life
                battery = Convert.ToInt32(pwr.BatteryLifePercent * 100);

                batTime = (float)pwr.BatteryLifeRemaining;

                if (pwr.BatteryChargeStatus == BatteryChargeStatus.Charging)
                {
                    batTime = 0;

                }

                //avFrame = avFrame + Framerate;

                //avFrame = avFrame / f;
                //f++;

                TimeSpan time = TimeSpan.FromSeconds(batTime);


                if (i == d)
                {


                    if (Addresses.PMTableVersion == 400005 || Addresses.PMTableVersion == 400004)
                    {
                        iGPUClock = (int)GetSensor.getSensorValve("GFX_FREQEFF");
                        iGPULoad = (int)GetSensor.getSensorValve("GFX_BUSY");
                        iGPUSoCClock = (int)GetSensor.getSensorValve("SOCCLK_FREQEFF");
                        iGPUTemp = (int)GetSensor.getSensorValve("GFX_TEMP");
                        iGPUVolt = GetSensor.getSensorValve("GFX_VOLTAGE");
                        fabricClk = (int)GetSensor.getSensorValve("FCLK_FREQEFF");
                        iGPUMemClock = (int)GetSensor.getSensorValve("MEMCLK_FREQEFF");
                        cpuVolt = GetSensor.getSensorValve("CPU_TELEMETRY_VOLTAGE");
                        cpuPower = (int)GetSensor.getSensorValve("SOCKET_POWER");

                        lbliGPU.Content = $"{iGPUTemp}°C   {iGPULoad}%   {iGPUClock}MHz   {iGPUSoCClock}MHz   {iGPUVolt.ToString("0.00")}V";
                        lblRAM.Content = $"{RAMLoad}%   {RAMUsed}MB   {iGPUMemClock}MHz   {fabricClk}MHz";
                        lblCPU.Content = $"{cpuTemp}°C   {cpuLoad}%   {cpuClock}MHz   {cpuVolt.ToString("0.00")}V   {cpuPower}W";
                    }
                    else
                    {
                        lbliGPU.Content = $"{iGPUTemp}°C   {iGPULoad}%   {iGPUClock}MHz   {iGPUSoCClock}MHz";
                        lblRAM.Content = $"{RAMLoad}%   {RAMUsed}MB   {iGPUMemClock}MHz";
                        lblCPU.Content = $"{cpuTemp}°C   {cpuLoad}%   {cpuClock}MHz   {cpuPower}w";
                    }

                    d = i + 2;
                }
                lblFPS.Content = $"{Framerate}FPS";

                if (batTime <= 1)
                {
                    imgBat.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/battery-charge-line.png"));
                    lblBat.Content = $"{battery}%";
                }
                else if (battery > 50)
                {
                    imgBat.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/battery-fill.png"));
                    lblBat.Content = $"{battery}%   {time:%h} Hours {time:%m} Minutes";
                }
                else if (battery < 45 && battery > 0)
                {
                    imgBat.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/battery-low-line.png"));
                    lblBat.Content = $"{battery}%   {time:%h} Hours {time:%m} Minutes";
                }
            }
            catch (Exception ex)
            {
            }

            if (Framerate <= 0)
            {
                FPS.Visibility = Visibility.Collapsed;
            }
            else
            {
                FPS.Visibility = Visibility.Visible;
            }

            i++;
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
                                cpuTemp = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Package"))
                            {
                                cpuPower = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("1"))
                            {
                                cpuClock = (int)sensor.Value.GetValueOrDefault();
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

        public static void getGPUInfo()
        {
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.GpuAmd)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {
                                        iGPUClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());

                                    }
                                }
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("SoC"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {
                                        iGPUSoCClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());

                                    }
                                }
                            }

                            if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {
                                        iGPUTemp = Convert.ToInt32(sensor.Value.GetValueOrDefault());

                                    }
                                }
                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {
                                        iGPUPower = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                    }
                                }
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Mem"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {
                                        iGPUMemClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                    }
                                }
                            }

                            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {
                                        iGPULoad = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    getGPUInfo();
                }
                catch (Exception ex2)
                {

                }

            }
        }

        public void getRAMInfo()
        {
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Memory)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load && !sensor.Name.Contains("Virtual"))
                            {
                                RAMLoad = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Memory Used") && !sensor.Name.Contains("Virtual Memory Used"))
                            {
                                RAMUsed = Convert.ToInt32(sensor.Value.GetValueOrDefault() * 1000);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void OutputThreadProc()
        {
            try
            {
                long t1, t2;
                long dt = 2000;

                lock (sync)
                {
                    t2 = watch.ElapsedMilliseconds;
                    t1 = t2 - dt;

                    foreach (var x in frames.Values)
                    {

                        //get the number of frames
                        int count = x.QueryCount(t1, t2);

                        //calculate FPS
                        Framerate = Convert.ToInt32((double)count / dt * 1000.0);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        //helper class to store frame timestamps
        public class TimestampCollection
        {
            const int MAXNUM = 2000;

            public string Name { get; set; }

            List<long> timestamps = new List<long>(MAXNUM + 1);
            object sync = new object();

            //add value to the collection
            public void Add(long timestamp)
            {
                try
                {
                    lock (sync)
                    {
                        timestamps.Add(timestamp);
                        if (timestamps.Count > MAXNUM) timestamps.RemoveAt(0);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            //get the number of timestamps withing interval
            public int QueryCount(long from, long to)
            {
                int c = 0;
                try
                {

                    lock (sync)
                    {
                        foreach (var ts in timestamps)
                        {
                            if (ts >= from && ts <= to) c++;
                        }
                    }
                    return c;
                }
                catch
                {
                    return c;
                }
            }
        }

    }
}
