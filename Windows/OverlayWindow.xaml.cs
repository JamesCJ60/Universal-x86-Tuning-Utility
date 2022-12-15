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

            //set up timer for sensor update
            DispatcherTimer Adaptive = new DispatcherTimer();
            Adaptive.Interval = TimeSpan.FromSeconds(1);
            Adaptive.Tick += Adaptive_Tick;
            Adaptive.Start();

            autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
            autoReapply.Tick += AutoReapply_Tick;
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

        public static bool hidden = true;


        void Adaptive_Tick(object sender, EventArgs e)
        {
            try
            {
                getCPUInfo();

                if (MainWindow.AppName.Contains("AMD"))
                {
                    Addresses.UpdateTable();
                }

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
                        CpuPowerLimiter.CurveOptimiserLimit(cpuLoad, maxCO);
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
                        CpuPowerLimiter.CurveOptimiserLimit(cpuLoad, maxCO);
                    }
                }
            }
            catch { }
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
                if (autoReapply.Interval != TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime))
            {
                autoReapply.Stop();
                autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
                autoReapply.Start();
            }
            }
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
    }
}
