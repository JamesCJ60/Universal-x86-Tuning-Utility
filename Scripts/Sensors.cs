using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using AudioSwitcher.AudioApi.CoreAudio;
using System.Runtime.InteropServices;

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
            try
            {
                PowerStatus pwr = SystemInformation.PowerStatus;
                //Get battery life
                battery = Convert.ToInt32(pwr.BatteryLifePercent * 100);

                if (pwr.BatteryChargeStatus != BatteryChargeStatus.Charging)
                {
                    batTime = (float)pwr.BatteryLifeRemaining;
                }
                else
                {
                    batTime = 0;
                }
            } catch (Exception ex)
            {

            }
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);
        const int ENUM_CURRENT_SETTINGS = -1;

        const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {

            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;

        }

        public static List<string> width = new List<string>();
        public static List<string> height = new List<string>();
        public static List<string> refresh = new List<string>();


        private static void getTime()
        {
            //Get current time
            DateTime currentTime = DateTime.Now;
            time = currentTime.ToString("HH:mm");
        }

        private static void getBrightness()
        {
            try
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
            } catch (Exception ex)
            {

            }
        } 
    }
}
