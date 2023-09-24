using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Universal_x86_Tuning_Utility.Views.Windows;
using static Universal_x86_Tuning_Utility.Scripts.Misc.ScreenInterrogatory;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class Display
    {

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, uint iModeNum, ref DEVMODE lpDevMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public uint cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;

            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
        }

        public static string targetDisplayName = FindLaptopScreen();
        public static List<string> uniqueResolutions = GetSupportedResolutions(targetDisplayName);
        public static List<int> uniqueRefreshRates = GetSupportedRefreshRates(targetDisplayName);

        public static void setUpLists()
        {;
            uniqueRefreshRates = GetSupportedRefreshRates(targetDisplayName);
            uniqueRefreshRates = uniqueRefreshRates.Distinct().ToList();
            uniqueRefreshRates.Sort();
            uniqueRefreshRates.Reverse();
        }

        static List<string> GetSupportedResolutions(string targetDisplayName)
        {
            List<string> resolutions = new List<string>();
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = (uint)Marshal.SizeOf(displayDevice);

            for (uint deviceIndex = 0; EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0); deviceIndex++)
            {
                if (displayDevice.DeviceName == targetDisplayName)
                {
                    DEVMODE devMode = new DEVMODE();
                    devMode.dmSize = (short)Marshal.SizeOf(devMode);
                    for (uint modeIndex = 0; EnumDisplaySettings(displayDevice.DeviceName, modeIndex, ref devMode); modeIndex++)
                    {
                        string resolution = $"{devMode.dmPelsWidth} x {devMode.dmPelsHeight}";
                        resolutions.Add(resolution);
                    }
                    break;
                }
            }

            return resolutions;
        }

        static List<int> GetSupportedRefreshRates(string targetDisplayName)
        {
            List<int> refreshRates = new List<int>();
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = (uint)Marshal.SizeOf(displayDevice);
            int displayCount = Screen.AllScreens.Length;
            if (App.product.Contains("aya"))
            {
                refreshRates.Add(60);
            }
            else
            {
                for (uint deviceIndex = 0; EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0); deviceIndex++)
                {
                    if (displayDevice.DeviceName == targetDisplayName)
                    {
                        DEVMODE devMode = new DEVMODE();
                        devMode.dmSize = (short)Marshal.SizeOf(devMode);
                        for (uint modeIndex = 0; EnumDisplaySettings(displayDevice.DeviceName, modeIndex, ref devMode); modeIndex++)
                        {
                            if (devMode.dmDisplayFrequency > 0) // Exclude modes with zero refresh rate
                            {
                                refreshRates.Add(devMode.dmDisplayFrequency);
                            }
                        }
                        break;
                    }
                }
            }

            return refreshRates;
        }

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

        public static void ChangeDisplaySettings(string targetDisplayName, int newRefreshRate)
        {
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            if (EnumDisplaySettings(null, 0, ref devMode))
            {
                devMode.dmDisplayFrequency = newRefreshRate;
                devMode.dmFields = (int)DisplaySettingsFlags.DM_PELSWIDTH | (int)DisplaySettingsFlags.DM_PELSHEIGHT | (int)DisplaySettingsFlags.DM_DISPLAYFREQUENCY;

                int result = ChangeDisplaySettingsEx(targetDisplayName, ref devMode, IntPtr.Zero, (uint)ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);

                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    Console.WriteLine("\nDisplay settings changed successfully.");
                }
                else
                {
                    Console.WriteLine("\nFailed to change display settings.");
                }
            }
        }

        public static void ApplySettings(int newHz)
        {
            targetDisplayName = FindLaptopScreen();
            if (newHz > 0)
            {
                Display.ChangeDisplaySettings(Display.targetDisplayName, newHz);
            }
            else
            {
                Console.WriteLine("Invalid input format.");
            }
        }

        const int DISP_CHANGE_SUCCESSFUL = 0;

        [Flags()]
        public enum DisplaySettingsFlags : int
        {
            DM_PELSWIDTH = 0x00080000,
            DM_PELSHEIGHT = 0x00100000,
            DM_DISPLAYFREQUENCY = 0x00400000
        }

        [Flags()]
        public enum ChangeDisplaySettingsFlags : uint
        {
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_RESET = 0x40000000,
            CDS_NORESET = 0x10000000
        }

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const string defaultDevice = @"\\.\DISPLAY1";

        public static string? FindLaptopScreen(bool log = false)
        {
            string? laptopScreen = null;
            var screens = Screen.AllScreens;
            try
            {
                var devices = GetAllDevices().ToArray();

                int count = 0, displayNum = -1;

                foreach (var device in devices)
                {
                    if (device.outputTechnology == DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL ||
                        device.outputTechnology == DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED)
                    {
                        displayNum = count;
                    }

                    count++;
                }

                count = 0;
                foreach (var screen in screens)
                {
                    if (count == displayNum) laptopScreen = screen.DeviceName;
                    count++;
                }

                if (displayNum > 0 && count == 0) laptopScreen = defaultDevice;
            }
            catch (Exception ex)
            {
                laptopScreen = Screen.PrimaryScreen.DeviceName;
            }

            return laptopScreen;
        }
    }
}