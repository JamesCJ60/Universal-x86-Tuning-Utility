using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Views.Windows;
using static Universal_x86_Tuning_Utility.Scripts.Misc.GetSystemInfo;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for SystemInfo.xaml
    /// </summary>
    public partial class SystemInfo : Page
    {
        public SystemInfo()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            if(Family.TYPE != Family.ProcessorType.Intel) this.DataContext = App.memTimings;
            else spRAMTime.Visibility = Visibility.Collapsed;
            getCPUInfo();
            getRAMInfo();
            getDeviceInfo();
            if (SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery) getBatteryInfo();
            else
            {
                sdBattery.Visibility = Visibility.Collapsed;
                sdRAM.Margin = new Thickness(0, 9, 15, 15);
            }
        }

        private void mainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(0, 0, -12, 0);
            else mainCon.Margin = new Thickness(0, 0, 0, 0);
        }

        public bool IsScrollBarVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null) throw new ArgumentNullException(nameof(scrollViewer));

            return scrollViewer.ExtentHeight > scrollViewer.ViewportHeight;
        }

        private async void getCPUInfo()
        {
            try
            {
                sdCPU.Visibility = Visibility.Collapsed;
                // CPU information using WMI
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

                string name = "";
                string description = "";
                string manufacturer = "";
                int numberOfCores = 0;
                int numberOfLogicalProcessors = 0;
                double l2Size = 0;
                double l3Size = 0;
                string baseClock = "";

                await Task.Run(() =>
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        name = queryObj["Name"].ToString();
                        description = queryObj["Description"].ToString();
                        manufacturer = queryObj["Manufacturer"].ToString();
                        numberOfCores = Convert.ToInt32(queryObj["NumberOfCores"]);
                        numberOfLogicalProcessors = Convert.ToInt32(queryObj["NumberOfLogicalProcessors"]);
                        l2Size = Convert.ToDouble(queryObj["L2CacheSize"]) / 1024;
                        l3Size = Convert.ToDouble(queryObj["L3CacheSize"]) / 1024;
                        baseClock = queryObj["MaxClockSpeed"].ToString();
                    }
                });

                tbProcessor.Text = name;
                tbCaption.Text = description;
                string codeName = GetSystemInfo.Codename();
                if (codeName != "") tbCodename.Text = codeName;
                else
                {
                    tbCodename.Visibility = Visibility.Collapsed;
                    tbCode.Visibility = Visibility.Collapsed;
                }

                tbProducer.Text = manufacturer;
                if(numberOfLogicalProcessors == numberOfCores) tbCores.Text = numberOfCores.ToString();
                else tbCores.Text = GetSystemInfo.getBigLITTLE(numberOfCores, l2Size);
                tbThreads.Text = numberOfLogicalProcessors.ToString();
                tbL3Cache.Text = $"{l3Size.ToString("0.##")} MB";

                uint sum = 0;
                foreach (uint number in GetSystemInfo.GetCacheSizes(CacheLevel.Level1)) sum += number;
                decimal total = sum;
                total = total / 1024;
                tbL1Cache.Text = $"{total.ToString("0.##")} MB";

                sum = 0;
                foreach (uint number in GetSystemInfo.GetCacheSizes(CacheLevel.Level2)) sum += number;
                total = sum;
                total = total / 1024;
                tbL2Cache.Text = $"{total.ToString("0.##")} MB";

                tbBaseClock.Text = $"{baseClock} MHz";

                tbInstructions.Text = GetSystemInfo.InstructionSets();

                sdCPU.Visibility = Visibility.Visible;
            }
            catch (ManagementException ex)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + ex.Message);
            }
        }
        private async void getRAMInfo()
        {
            sdRAM.Visibility = Visibility.Collapsed;
            double capacity = 0;
            int speed = 0;
            int type = 0;
            int width = 0;
            int slots = 0;
            string producer = "";
            string model = "";

            try
            {
                ManagementObjectSearcher searcher =
            new ManagementObjectSearcher("root\\CIMV2",
            "SELECT * FROM Win32_PhysicalMemory");
                await Task.Run(() =>
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        if (producer == "") producer = queryObj["Manufacturer"].ToString();
                        else if (!producer.Contains(queryObj["Manufacturer"].ToString())) producer = $"{producer}/{queryObj["Manufacturer"]}";

                        if (model == "") model = queryObj["PartNumber"].ToString();
                        else if (!model.Contains(queryObj["PartNumber"].ToString())) model = $"{model}/{queryObj["PartNumber"]}";

                        capacity = capacity + Convert.ToDouble(queryObj["Capacity"]);
                        speed = Convert.ToInt32(queryObj["ConfiguredClockSpeed"]);
                        type = Convert.ToInt32(queryObj["SMBIOSMemoryType"]);
                        width = width + Convert.ToInt32(queryObj["DataWidth"]);
                        slots++;
                    }
                });

                if (width > 128 && Family.FAM == Family.RyzenFamily.StrixHalo) if (width > 256) width = 256;
                else if (width > 64 && Family.FAM == Family.RyzenFamily.Mendocino) width = 64;
                else if (width > 128 && Family.FAM < Family.RyzenFamily.FireRange && Family.TYPE != Family.ProcessorType.Intel) width = 128;

                capacity = capacity / 1024 / 1024 / 1024;

                string DDRType = "";
                if (type == 20) DDRType = "DDR";
                else if (type == 21) DDRType = "DDR2";
                else if (type == 24) DDRType = "DDR3";
                else if (type == 26) DDRType = "DDR4";
                else if (type == 30) DDRType = "LPDDR4";
                else if (type == 34) DDRType = "DDR5";
                else if (type == 35) DDRType = "LPDDR5";
                else DDRType = $"Unknown ({type})";

                tbRAM.Text = $"{capacity} GB {DDRType} @ {speed} MT/s";
                tbRAMProducer.Text = producer;
                tbRAMModel.Text = model.Replace(" ", null);
                tbWidth.Text = $"{width} bit";
                tbSlots.Text = $"{slots} * {width / slots} bit";

                sdRAM.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {

            }
        }
        private void getBatteryInfo()
        {
            try
            {
                tbHealth.Text = $"{(GetSystemInfo.GetBatteryHealth() * 100).ToString("0.##")}%";
                tbCycle.Text = $"{GetSystemInfo.GetBatteryCycle()}";
                tbCapcity.Text = $"Full Charge: {GetSystemInfo.ReadFullChargeCapacity()} mAh | Design: {GetSystemInfo.ReadDesignCapacity()} mAh";

                tbChargeRate.Text = $"{(GetSystemInfo.GetBatteryRate() / 1000).ToString("0.##")}W";
                DispatcherTimer bat = new DispatcherTimer();
                bat.Interval = TimeSpan.FromSeconds(2);
                bat.Tick += Bat_Tick;
                bat.Start();
            } catch
            {
                if(sdBattery.Visibility != Visibility.Collapsed) sdBattery.Visibility = Visibility.Collapsed;
            }
        }

        private async void Bat_Tick(object sender, EventArgs e)
        {
            if (Family.TYPE == Family.ProcessorType.Intel && MainWindow._mainWindowNav.SelectedPageIndex == 5 || Family.TYPE != Family.ProcessorType.Intel && MainWindow._mainWindowNav.SelectedPageIndex == 6)
            {
                decimal batRate = 0;
                await Task.Run(() => batRate = GetSystemInfo.GetBatteryRate() / 1000);

                tbChargeRate.Text = $"{batRate.ToString("0.##")}W";
            } 
        }

        private void getDeviceInfo()
        {
            tbDeviceName.Text = GetSystemInfo.SystemName;
            tbDeviceModel.Text = GetSystemInfo.Product;
            tbDeviceProducer.Text = GetSystemInfo.Manufacturer;
        }
    }
}
