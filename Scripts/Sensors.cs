using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using AudioSwitcher.AudioApi.CoreAudio;

namespace AATUV3.Scripts
{
    internal class Sensors
    {
        public static int battery;
        public static string time;
        public static string brightness;
        public static float batTime;

        //create a management scope object
        public static ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\WMI");

        public static int i = 0;

        public static void updateSensors()
        {
            if (OverlayWindow.hidden == false)
            {
                getBattery();
                getTime();

                if (i < 1)
                {
                    getBrightness();
                }
            }

        }

        public static void getBattery()
        {
            PowerStatus pwr = SystemInformation.PowerStatus;
                //Get battery life
                battery = Convert.ToInt32(pwr.BatteryLifePercent * 100);

                if(pwr.BatteryChargeStatus != BatteryChargeStatus.Charging)
                {
                    batTime = (float)pwr.BatteryLifeRemaining;
                }
                else
                {
                    batTime = 0;
                }
        }

        private static void getTime()
        {
            //Get current time
            DateTime currentTime = DateTime.Now;
            time = currentTime.ToString("HH:mm");
        }

        private static void getBrightness()
        {
            //create object query
            ObjectQuery query = new ObjectQuery("SELECT * FROM WmiMonitorBrightness");

            //create object searcher
            ManagementObjectSearcher searcher =
                                    new ManagementObjectSearcher(scope, query);

            //get a collection of WMI objects
            ManagementObjectCollection queryCollection = searcher.Get();

            //enumerate the collection.
            foreach (ManagementObject m in queryCollection)
            {
                // access properties of the WMI object
                brightness = m["CurrentBrightness"].ToString();
            }
        }
    }
}
