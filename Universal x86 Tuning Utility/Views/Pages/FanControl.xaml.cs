using Microsoft.VisualBasic.Devices;
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
using Universal_x86_Tuning_Utility.Scripts.Misc;
using LibreHardwareMonitor.Hardware;
using Windows.Devices.Sensors;
using Computer = LibreHardwareMonitor.Hardware.Computer;
using SensorType = LibreHardwareMonitor.Hardware.SensorType;
using System.Windows.Threading;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for FanControl.xaml
    /// </summary>
    public partial class FanControl : Page
    {
        private DispatcherTimer timer;
        private bool isTimerRunning = false;

        public FanControl()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;
            Fan_Control.UpdateAddresses();

            string fanConfig = $"{GetSystemInfo.Manufacturer.ToUpper()}_{GetSystemInfo.Product.ToUpper()}.json";
            tbConfigName.Text = fanConfig;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2.5);
            timer.Tick += Timer_Tick;
        }

        void updateFanSpeed()
        {
            Fan_Control.setFanSpeed((int)nudFanSpeed.Value);
        }

        private void btnEn_Click(object sender, RoutedEventArgs e)
        {
            Fan_Control.enableFanControl();
        }

        private void btnDis_Click(object sender, RoutedEventArgs e)
        {
            if (isTimerRunning)
            {
                // Stop timer
                timer.Stop();
                isTimerRunning = false;
                tbFanSpeed.Text = $"Disabled";
            }
            Fan_Control.disableFanControl();
        }

        private void btnFanSpeed_Click(object sender, RoutedEventArgs e)
        {
            updateFanSpeed();
        }

        private static int Interpolate(int[] yValues, int[] xValues, int x)
        {
            int i = Array.FindIndex(xValues, t => t >= x);

            if (i == -1) // temperature is lower than the first input point
            {
                return yValues[0];
            }
            else if (i == 0) // temperature is equal to or higher than the first input point
            {
                return yValues[0];
            }
            else if (i == xValues.Length) // temperature is higher than the last input point
            {
                return yValues[xValues.Length - 1];
            }
            else // interpolate between two closest input points
            {
                return Interpolate(yValues[i - 1], xValues[i - 1], yValues[i], xValues[i], x);
            }
        }

        private static int Interpolate(int y1, int x1, int y2, int x2, int x)
        {
            return (y1 * (x2 - x) + y2 * (x - x1)) / (x2 - x1);
        }

        private void btnFanCurve_Click(object sender, RoutedEventArgs e)
        {
            if (isTimerRunning == true)
            {
                // Stop timer
                timer.Stop();
                isTimerRunning = false;
                tbFanSpeed.Text = $"Disabled";
            }
            else if (isTimerRunning == false)
            {
                isTimerRunning = true;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                int[] temps = { 25, 35, 45, 55, 65, 75, 85, 95 };
                int[] speeds = { 0, 5, 15, 25, 40, 55, 70, 100 };

                int cpuTemperature = GetCpuTemperature();

                var fanSpeed = Interpolate(speeds, temps, cpuTemperature);

                if (Fan_Control.fanControlEnabled) Fan_Control.setFanSpeed(fanSpeed);

                tbFanSpeed.Text = $"Enabled - {fanSpeed}% - {cpuTemperature}°C";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private int GetCpuTemperature()
        {
            try
            {
                Computer computer = new Computer
                {
                    IsCpuEnabled = true,
                };
                computer.Open();
                var cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
                cpu.Update();
                var temperature = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
                if (temperature != null)
                {
                    return (int)temperature.Value;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                // Log exception
                return 0;
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            Fan_Control.UpdateAddresses();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            string fanConfig = $"{GetSystemInfo.Manufacturer.ToUpper()}_{GetSystemInfo.Product.ToUpper()}.json";
            Clipboard.SetText(fanConfig);
        }
    }
}
