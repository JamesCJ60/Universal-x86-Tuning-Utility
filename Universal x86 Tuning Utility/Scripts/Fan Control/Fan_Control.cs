//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using Universal_x86_Tuning_Utility.Scripts.Fan_Control;
//using Universal_x86_Tuning_Utility.Services;

//namespace Universal_x86_Tuning_Utility.Scripts.Misc
//{
//    internal class Fan_Control
//    {
//        public static int MaxFanSpeed = 100;
//        public static int MinFanSpeed = 0;
//        public static int MinFanSpeedPercentage = 25;

//        public static double FanSpeed = 0;

//        public static ushort FanToggleAddress = Convert.ToUInt16("0x0", 16);
//        public static ushort FanChangeAddress = Convert.ToUInt16("0x0", 16);

//        public static byte EnableToggleAddress = Convert.ToByte("0x0", 16);
//        public static byte DisableToggleAddress = Convert.ToByte("0x0", 16);

//        public static ushort RegAddress = Convert.ToByte("0x0", 16);
//        public static ushort RegData = Convert.ToByte("0x0", 16);

//        public static bool fanControlEnabled = false;

//        public static void UpdateAddresses()
//        {
//            string fanConfig = "";
//            fanConfig = $"{GetSystemInfo.Manufacturer.ToUpper()}_{GetSystemInfo.Product.ToUpper()}.json";
//            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
//            path = path.Replace("Universal x86 Tuning Utility.dll", null);

//            path = "Fan Configs\\" + fanConfig;

//            if (File.Exists(path))
//            {
//                var fanControlDataManager = new FanConfigManager(path);

//                var dataForDevice = fanControlDataManager.GetDataForDevice();

//                // Access data for the device
//                MinFanSpeed = dataForDevice.MinFanSpeed; 
//                MaxFanSpeed = dataForDevice.MaxFanSpeed;
//                MinFanSpeedPercentage = dataForDevice.MinFanSpeedPercentage;
//                FanToggleAddress = Convert.ToUInt16(dataForDevice.FanControlAddress, 16);
//                FanChangeAddress = Convert.ToUInt16(dataForDevice.FanSetAddress, 16);
//                EnableToggleAddress = Convert.ToByte(dataForDevice.EnableToggleAddress, 16);
//                DisableToggleAddress = Convert.ToByte(dataForDevice.DisableToggleAddress, 16);

//                RegAddress = Convert.ToUInt16(dataForDevice.RegAddress, 16);
//                RegData = Convert.ToUInt16(dataForDevice.RegData, 16);

//                WinRingEC_Management.reg_addr = RegAddress;
//                WinRingEC_Management.reg_data = RegData;
//            }
//        }

//        public static void enableFanControl()
//        {
//            WinRingEC_Management.ECRamWrite(FanToggleAddress, EnableToggleAddress);
//            fanControlEnabled = true;
//        }

//        public static void disableFanControl()
//        {
//            WinRingEC_Management.ECRamWrite(FanToggleAddress, DisableToggleAddress);
//            fanControlEnabled = false;
//        }

//        public static bool fanIsEnabled()
//        {
//            byte returnvalue = WinRingEC_Management.ECRamRead(FanToggleAddress);
//            if (returnvalue == 0) { return false; } else { return true; }
//        }

//        public static void setFanSpeed(int speedPercentage)
//        {
//            if (speedPercentage < MinFanSpeedPercentage && speedPercentage > 0)
//            {
//                speedPercentage = MinFanSpeedPercentage;
//            }

//            byte setValue = (byte)Math.Round(((double)speedPercentage / 100) * MaxFanSpeed, 0);
//            WinRingEC_Management.ECRamWrite(FanChangeAddress, setValue);

//            FanSpeed = speedPercentage;
//        }

//        public static void readFanSpeed()
//        {
//            int fanSpeed = 0;

//            byte returnvalue = WinRingEC_Management.ECRamRead(FanChangeAddress);

//            double fanPercentage = Math.Round(100 * (Convert.ToDouble(returnvalue) / MaxFanSpeed), 0);
//            FanSpeed = fanPercentage;
//        }
//    }
//}
