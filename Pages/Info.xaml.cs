using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSmu;
using RyzenSMUBackend;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UXTU.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class Info : UserControl
    {
        private static Smu RyzenAccess;
        private static bool EnableDebug;
        private static uint[] Args;
        private static DispatcherTimer sensor = new DispatcherTimer();

        public Info()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;
            apuName.Text = $"APU: {Settings.Default.APUName.Replace("AMD ", "")}with Radeon Graphics";
            if (Families.FAMID == 3) updateInfoRNR();
            if (Families.FAMID == 7) updateInfoCZN();
            if (Families.FAMID == 8) updateInfoCZN();

            //set up timer for sensor 
            sensor.Interval = TimeSpan.FromSeconds(1);
            sensor.Tick += SensorUpdate_Tick;
            sensor.Start();

            if (Families.FAMID == 7 || Families.FAMID == 8)
            {
                CCX2.Visibility = Visibility.Hidden;
                CCX1Con.Margin = new Thickness(0, 5, 0, 5);
                CCX1Con.BorderThickness = new Thickness(2, 2, 0, 2);
                CCX1Con.Padding = new Thickness(5, 5, 0, 5);
                CCX1Con.CornerRadius = new CornerRadius(4, 0, 0, 4);
                CCX2Con.BorderThickness = new Thickness(0, 2, 2, 2);
                CCX2Con.Padding = new Thickness(0, 5, 5, 5);
                CCX2Con.CornerRadius = new CornerRadius(0, 4, 4, 0);
                this.MaxWidth = 903;
            }
        }
        void SensorUpdate_Tick(object sender, EventArgs e)
        {
            if (Families.FAMID == 3) updateInfoRNR();
            if (Families.FAMID == 7) updateInfoCZN();
            if (Families.FAMID == 8) updateInfoCZN();
        }

        private void updateInfoRNR()
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (!MainWindow.menu.ToLower().Contains("info")) sensor.Stop();

                TextBlock[] Core = { Core1, Core2, Core3, Core4, Core5, Core6, Core7, Core8 };
                TextBlock[] CoreTemp = { Core1Temp, Core2Temp, Core3Temp, Core4Temp, Core5Temp, Core6Temp, Core7Temp, Core8Temp };
                TextBlock[] CoreClock = { Core1Clock, Core2Clock, Core3Clock, Core4Clock, Core5Clock, Core6Clock, Core7Clock, Core8Clock };
                TextBlock[] CorePower = { Core1Power, Core2Power, Core3Power, Core4Power, Core5Power, Core6Power, Core7Power, Core8Power };
                TextBlock[] CoreVolt = { Core1Voltage, Core2Voltage, Core3Voltage, Core4Voltage, Core5Voltage, Core6Voltage, Core7Voltage, Core8Voltage };
                TextBlock[] CoreCO = { Core1CO, Core2CO, Core3CO, Core4CO, Core5CO, Core6CO, Core7CO, Core8CO };


                iGPUTemp.Text = $"Temperature: {(int)GetSensor.getSensorValve($"GFX_TEMP")}°C";

                if (GetSensor.getSensorValve($"GFX_FREQ") == float.NaN || GetSensor.getSensorValve($"GFX_FREQ") == 0)
                {
                    iGPUName.Text = $"iGPU (Disabled):";
                    iGPUClock.Visibility = Visibility.Hidden;
                    iGPUVoltage.Visibility = Visibility.Hidden;
                    iGPUUsage.Visibility = Visibility.Hidden;
                }
                else
                {
                    iGPUClock.Text = $"Frequency: {(int)GetSensor.getSensorValve($"GFX_FREQ")}MHz";
                    iGPUVoltage.Text = $"Voltage: {(int)(GetSensor.getSensorValve($"GFX_VOLTAGE") * 1000)}mV";
                    iGPUUsage.Text = $"Usage: {(int)GetSensor.getSensorValve($"GFX_BUSY")}%";
                }



                pl0.Text = $"Power Limit 0: {(int)GetSensor.getSensorValve($"STAPM_LIMIT")}W";
                pl1.Text = $"Power Limit 1: {(int)GetSensor.getSensorValve($"PPT_LIMIT_SLOW")}W";
                pl2.Text = $"Power Limit 2: {(int)GetSensor.getSensorValve($"PPT_LIMIT_FAST")}W";

                MemClock.Text = $"RAM Clock: {(int)GetSensor.getSensorValve("MEMCLK_FREQ")}MHz";
                FabricClock.Text = $"Fabric Clock: {(int)GetSensor.getSensorValve("FCLK_FREQ")}MHz";
                Uncore.Text = $"Uncore Clock: {(int)GetSensor.getSensorValve("UCLK_FREQ")}MHz";
                SoCVoltage.Text = $"Voltage: {(int)(GetSensor.getSensorValve($"SOC_TELEMETRY_VOLTAGE") * 1000)}mV";
                SoCPower.Text = $"Power Draw: {GetSensor.getSensorValve("VDDCR_SOC_POWER").ToString("0.00")}W";

                SocketPower.Text = $"Socket Power: {Math.Round(GetSensor.getSensorValve("SOCKET_POWER"), 2)}W";

                MemBandwidth.Text = $"Max Bandwidth: {Math.Round(GetSensor.getSensorValve("MaxDramBW"), 2)}GB/s";

                int i = 0;
                do
                {
                    CoreTemp[i].Text = $"Temperature: {(int)GetSensor.getSensorValve($"CORE_TEMP_{i}")}°C";

                    if (GetSensor.getSensorValve($"CORE_POWER_{i}") == float.NaN || GetSensor.getSensorValve($"CORE_POWER_{i}") == 0)
                    {
                        Core[i].Text = $"Core {i + 1} (Disabled):";
                        CoreClock[i].Visibility = Visibility.Hidden;
                        CorePower[i].Visibility = Visibility.Hidden;
                        CoreVolt[i].Visibility = Visibility.Hidden;
                        CoreCO[i].Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Core[i].Text = $"Core {i + 1}:";
                        CoreClock[i].Text = $"Frequency: {(int)(GetSensor.getSensorValve($"CORE_FREQ_{i}") * 1000)}MHz";
                        CorePower[i].Text = $"Power Draw: {Math.Round(GetSensor.getSensorValve($"CORE_POWER_{i}"), 2)}W";
                        CoreVolt[i].Text = $"Voltage: {(int)(GetSensor.getSensorValve($"CORE_VOLTAGE_{i}") * 1000)}mV";

                        CoreCO[i].Visibility = Visibility.Hidden;
                    }

                    i++;
                }
                while (i < CoreCO.Length);



                L3Logic.Text = $"L3$ Logic Power: {Math.Round(GetSensor.getSensorValve("L3_LOGIC_POWER_0"), 2) + Math.Round(GetSensor.getSensorValve("L3_VDDM_POWER_1"), 2)}W";
                L3VDDM.Text = $"L3$ VDDM Power: {Math.Round(GetSensor.getSensorValve("L3_VDDM_POWER_0"), 2) + Math.Round(GetSensor.getSensorValve("L3_VDDM_POWER_1"), 2)}W";
                L3Temp.Text = $"L3$ Temp: {(int)GetSensor.getSensorValve("L3_TEMP_0")}/{(int)GetSensor.getSensorValve("L3_TEMP_1")}°C";
                L3Clock.Text = $"L3$ Clock: {(int)(GetSensor.getSensorValve("L3_FREQ_0") * 1000)}/{(int)(GetSensor.getSensorValve("L3_FREQ_1") * 1000)}MHz";


                TDC.Text = $"TDC: {(int)GetSensor.getSensorValve("TDC_VALUE_VDD")}A/{(int)GetSensor.getSensorValve("TDC_LIMIT_VDD")}A";
                EDC.Text = $"EDC: {(int)GetSensor.getSensorValve("EDC_VALUE_VDD")}A/{(int)GetSensor.getSensorValve("EDC_LIMIT_VDD")}A";
            }, DispatcherPriority.ApplicationIdle);
        }
        private void updateInfoCZN()
        {
            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (!MainWindow.menu.ToLower().Contains("info")) sensor.Stop();

                TextBlock[] Core = { Core1, Core2, Core5, Core6, Core3, Core4, Core7, Core8 };
                TextBlock[] CoreTemp = { Core1Temp, Core2Temp, Core5Temp, Core6Temp, Core3Temp, Core4Temp, Core7Temp, Core8Temp };
                TextBlock[] CoreClock = { Core1Clock, Core2Clock, Core5Clock, Core6Clock, Core3Clock, Core4Clock, Core7Clock, Core8Clock };
                TextBlock[] CorePower = { Core1Power, Core2Power, Core5Power, Core6Power, Core3Power, Core4Power, Core7Power, Core8Power };
                TextBlock[] CoreVolt = { Core1Voltage, Core2Voltage, Core5Voltage, Core6Voltage, Core3Voltage, Core4Voltage, Core7Voltage, Core8Voltage };
                TextBlock[] CoreCO = { Core1CO, Core2CO, Core5CO, Core6CO, Core3CO, Core4CO, Core7CO, Core8CO };


                iGPUTemp.Text = $"Temperature: {(int)GetSensor.getSensorValve($"GFX_TEMP")}°C";

                if (GetSensor.getSensorValve($"GFX_FREQ") == float.NaN || GetSensor.getSensorValve($"GFX_FREQ") == 0)
                {
                    iGPUName.Text = $"iGPU (Disabled):";
                    iGPUClock.Visibility = Visibility.Hidden;
                    iGPUVoltage.Visibility = Visibility.Hidden;
                    iGPUUsage.Visibility = Visibility.Hidden;
                }
                else
                {
                    iGPUClock.Text = $"iGPU Clock: {(int)GetSensor.getSensorValve($"GFX_FREQ")}MHz";
                    iGPUVoltage.Text = $"iGPU Voltage: {(int)(GetSensor.getSensorValve($"GFX_VOLTAGE") * 1000)}mV";
                    iGPUUsage.Text = $"iGPU Usage: {(int)GetSensor.getSensorValve($"GFX_BUSY")}%";
                }



                pl0.Text = $"Power Limit 0: {(int)GetSensor.getSensorValve($"STAPM_LIMIT")}W";
                pl1.Text = $"Power Limit 1: {(int)GetSensor.getSensorValve($"PPT_LIMIT_SLOW")}W";
                pl2.Text = $"Power Limit 2: {(int)GetSensor.getSensorValve($"PPT_LIMIT_FAST")}W";

                MemClock.Text = $"RAM Clock: {(int)GetSensor.getSensorValve("MEMCLK_FREQEFF")}MHz";
                FabricClock.Text = $"Fabric Clock: {(int)GetSensor.getSensorValve("FCLK_FREQEFF")}MHz";
                Uncore.Text = $"Uncore Clock: {(int)GetSensor.getSensorValve("UCLK_FREQEFF")}MHz";
                SoCVoltage.Text = $"Voltage: {(int)(GetSensor.getSensorValve($"SOC_TELEMETRY_VOLTAGE") * 1000)}mV";
                SoCPower.Text = $"Power Draw: {GetSensor.getSensorValve("VDDCR_SOC_POWER").ToString("0.00")}W";

                SocketPower.Text = $"Socket Power: {Math.Round(GetSensor.getSensorValve("SOCKET_POWER"), 2)}W";

                MemBandwidth.Text = $"Max Bandwidth: {Math.Round(GetSensor.getSensorValve("MaxDramBW"), 2)}GB/s";

                int i = 0;
                do
                {
                    CoreTemp[i].Text = $"Temperature: {(int)GetSensor.getSensorValve($"CORE_TEMP_{i}")}°C";

                    if (GetSensor.getSensorValve($"CORE_POWER_{i}") == float.NaN || GetSensor.getSensorValve($"CORE_POWER_{i}") == 0)
                    {
                        Core[i].Text = $"Core {i + 1} (Disabled):";
                        CoreClock[i].Visibility = Visibility.Hidden;
                        CorePower[i].Visibility = Visibility.Hidden;
                        CoreVolt[i].Visibility = Visibility.Hidden;
                        CoreCO[i].Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Core[i].Text = $"Core {i + 1}:";
                        CoreClock[i].Text = $"Frequency: {(int)(GetSensor.getSensorValve($"CORE_FREQ_{i}") * 1000)}MHz";
                        CorePower[i].Text = $"Power Draw: {Math.Round(GetSensor.getSensorValve($"CORE_POWER_{i}"), 2)}W";
                        CoreVolt[i].Text = $"Voltage: {(int)(GetSensor.getSensorValve($"CORE_VOLTAGE_{i}") * 1000)}mV";

                        RyzenAccess = new Smu(EnableDebug);
                        RyzenAccess.Initialize();
                        Args = new uint[6];
                        Args[0] = Convert.ToUInt32(i & 0xFFFF);
                        RyzenAccess.SendMp1(0x62, ref Args);
                        CoreCO[i].Text = $"CO Offset: {(int)Args[0]}";
                        RyzenAccess.Deinitialize();
                    }

                    i++;
                }
                while (i < CoreCO.Length);

                L3Logic.Text = $"L3$ Logic Power: {Math.Round(GetSensor.getSensorValve("L3_LOGIC_POWER"), 2)}W";
                L3VDDM.Text = $"L3$ VDDM Power: {Math.Round(GetSensor.getSensorValve("L3_VDDM_POWER"), 2)}W";
                L3Temp.Text = $"L3$ Temp: {(int)GetSensor.getSensorValve("L3_TEMP")}°C";
                L3Clock.Text = $"L3$ Clock: {(int)(GetSensor.getSensorValve("L3_FREQEFF") * 1000)}MHz";

                TDC.Text = $"TDC: {(int)GetSensor.getSensorValve("TDC_VALUE_VDD")}A/{(int)GetSensor.getSensorValve("TDC_LIMIT_VDD")}A";
                EDC.Text = $"EDC: {(int)GetSensor.getSensorValve("EDC_VALUE_VDD")}A/{(int)GetSensor.getSensorValve("EDC_LIMIT_VDD")}A";
            }, DispatcherPriority.ApplicationIdle);
        }
    }
}
