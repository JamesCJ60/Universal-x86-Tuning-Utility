using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class GetSystemInfo
    {
        private static ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
        private static ManagementObjectSearcher motherboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");



        static public string Manufacturer
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get())
                    {
                        return queryObj["Manufacturer"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string Product
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get())
                    {
                        return queryObj["Product"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string SystemName
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get())
                    {
                        return queryObj["SystemName"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string Version
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get())
                    {
                        return queryObj["Version"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        public static decimal GetBatteryRate()
        {

            try
            {
                ManagementScope scope = new ManagementScope("root\\WMI");
                ObjectQuery query = new ObjectQuery("SELECT * FROM BatteryStatus");

                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    decimal chargeRate = Convert.ToDecimal(obj["ChargeRate"]);
                    decimal dischargeRate = Convert.ToDecimal(obj["DischargeRate"]);
                    if (chargeRate > 0)
                        return chargeRate;
                    else
                        return -dischargeRate;
                }

                return 0;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static decimal ReadFullChargeCapacity()    {

            try
            {
                ManagementScope scope = new ManagementScope("root\\WMI");
                ObjectQuery query = new ObjectQuery("SELECT * FROM BatteryFullChargedCapacity");

                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    return Convert.ToDecimal(obj["FullChargedCapacity"]);
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public static decimal ReadDesignCapacity()
        {
            try
            {
                ManagementScope scope = new ManagementScope("root\\WMI");
                ObjectQuery query = new ObjectQuery("SELECT * FROM BatteryStaticData");

                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    return Convert.ToDecimal(obj["DesignedCapacity"]);
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static int GetBatteryCycle()
        {
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\WMI",
                    "SELECT * FROM BatteryCycleCount");

                foreach (ManagementObject queryObj in searcher.Get())
                {

                    return Convert.ToInt32(queryObj["CycleCount"]);
                }
                return 0;
            }
            catch (ManagementException e)
            {
                return 0;
            }
        }

        public static decimal GetBatteryHealth()
        {
            var designCap = ReadDesignCapacity();
            var fullCap = ReadFullChargeCapacity();

            decimal health = (decimal)fullCap / (decimal)designCap;

            return health;
        }
    }
}
