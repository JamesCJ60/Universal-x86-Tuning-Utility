using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace UXTU.Scripts
{
    internal class BatteryInfo
    {
        public static string batPercent = "";
        public static int batPercentInt = 0;
        public static UInt16 statuscode = 9999;

        //Pull battery sensor info from Windows
        public async static Task getBattery()
        {
            int batteryLife = 0;
            try
            {
                ManagementClass wmi = new ManagementClass("Win32_Battery");
                ManagementObjectCollection allBatteries = wmi.GetInstances();

                double batteryLevel = 0;

                //Get battery level from each system battery detected
                foreach (var battery in allBatteries)
                {
                    batteryLevel = Convert.ToDouble(battery["EstimatedChargeRemaining"]);
                    statuscode = (UInt16)battery["BatteryStatus"];
                }
                //Set battery level as an int
                batteryLife = (int)batteryLevel;
                batPercentInt = batteryLife;

                //Update battery level string
                batPercent = batteryLife.ToString() + "%";
            }
            catch (Exception ex)
            {

            }
        }
    }
}
