using Microsoft.Extensions.Logging;
using System;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;

using static Universal_x86_Tuning_Utility.Scripts.Misc.GetSystemInfo;


using System.Windows.Forms;
using Universal_x86_Tuning_Utility.Models;

namespace Universal_x86_Tuning_Utility.Services
{
    public interface ISystemInformationService
    {
        object MemoryTimings { get; }
        Task<SystemInformationState> ReadAsync();
        Task<string> ReadBatteryRateAsync();
    }
    public sealed class SystemInformationService : ISystemInformationService
    {
        public object MemoryTimings => null!;
        public async Task<SystemInformationState> ReadAsync()
        {
            var information = new SystemInformationState();


            information.RAMTimeVisibility = Visibility.Collapsed;
            await getCPUInfo(information);
            await getRAMInfo(information);
            getDeviceInfo(information);
            if (SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery) getBatteryInfo(information);
            else
            {
                information.BatteryVisibility = Visibility.Collapsed;
                information.RAMMargin = new Thickness(0, 9, 15, 15);
            }

            await Task.CompletedTask;

            return information;
        }
        public async Task<string> ReadBatteryRateAsync()
        {
            var rate = await Task.Run(() => GetSystemInfo.GetBatteryRate() / 1000);
            return $"{rate:0.##}W";
        }
        private async Task getCPUInfo(SystemInformationState information)
        {
            try
            {
                information.CPUVisibility = Visibility.Collapsed;
                // CPU information using WMI
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

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

                information.ProcessorText = name;
                information.CaptionText = description;
                string codeName = GetSystemInfo.Codename();
                if (codeName != "") information.CodenameText = codeName;
                else
                {
                    information.CodenameVisibility = Visibility.Collapsed;
                    information.CodeVisibility = Visibility.Collapsed;
                }

                information.ProducerText = manufacturer;
                if (numberOfLogicalProcessors == numberOfCores) information.CoresText = numberOfCores.ToString();
                else information.CoresText = GetSystemInfo.getBigLITTLE(numberOfCores, l2Size);
                information.ThreadsText = numberOfLogicalProcessors.ToString();
                information.L3CacheText = $"{l3Size.ToString("0.##")} MB";

                uint sum = 0;
                foreach (uint number in GetSystemInfo.GetCacheSizes(CacheLevel.Level1)) sum += number;
                decimal total = sum;
                total = total / 1024;
                information.L1CacheText = $"{total.ToString("0.##")} MB";

                sum = 0;
                foreach (uint number in GetSystemInfo.GetCacheSizes(CacheLevel.Level2)) sum += number;
                total = sum;
                total = total / 1024;
                information.L2CacheText = $"{total.ToString("0.##")} MB";

                information.BaseClockText = $"{baseClock} MHz";

                information.InstructionsText = GetSystemInfo.InstructionSets();

                information.CPUVisibility = Visibility.Visible;
            }
            catch (ManagementException ex)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + ex.Message);
            }
        }

        private async Task getRAMInfo(SystemInformationState information)
        {
            information.RAMVisibility = Visibility.Collapsed;
            double capacity = 0;
            int speed = 0;
            int type = 0;
            int width = 0;
            int slots = 0;
            string producer = "";
            string model = "";

            try
            {
                using ManagementObjectSearcher searcher =
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

                information.RAMText = $"{capacity} GB {DDRType} @ {speed} MT/s";
                information.RAMProducerText = producer;
                information.RAMModelText = model.Replace(" ", null);
                information.WidthText = $"{width} bit";
                information.SlotsText = $"{slots} * {(slots == 0 ? 0 : width / slots)} bit";

                information.RAMVisibility = Visibility.Visible;
            }
            catch (Exception ex)
            {

            }
        }

        private void getDeviceInfo(SystemInformationState information)
        {
            information.DeviceNameText = GetSystemInfo.SystemName;
            information.DeviceModelText = GetSystemInfo.Product;
            information.DeviceProducerText = GetSystemInfo.Manufacturer;
        }

        private void getBatteryInfo(SystemInformationState information)
        {
            try
            {
                information.HealthText = $"{(GetSystemInfo.GetBatteryHealth() * 100).ToString("0.##")}%";
                information.CycleText = $"{GetSystemInfo.GetBatteryCycle()}";
                information.CapcityText = $"Full Charge: {GetSystemInfo.ReadFullChargeCapacity()} mAh | Design: {GetSystemInfo.ReadDesignCapacity()} mAh";

                information.ChargeRateText = $"{(GetSystemInfo.GetBatteryRate() / 1000).ToString("0.##")}W";

            }
            catch
            {
                if (information.BatteryVisibility != Visibility.Collapsed) information.BatteryVisibility = Visibility.Collapsed;
            }
        }
    }
}
