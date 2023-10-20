using RTSSSharedMemoryNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Properties;
using Windows.Security.EnterpriseData;

namespace Universal_x86_Tuning_Utility.Scripts
{
    public static class RTSS
    {
        public static string rtssDirectory = Settings.Default.directoryRTSS;
        public static int fps = 0;
        public static void getRTSSFPSLimit()
        {
            if (RTSSRunning())
            {
                RTSS.LoadProfile();
                int fpsLimit = 0;
                RTSS.GetProfileProperty("FramerateLimit", out fpsLimit);
                fps = fpsLimit;
            }
        }

        public static bool directoryRTSSExists()
        {
            if (File.Exists(rtssDirectory + @"\RTSS.exe"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void startRTSS()
        {
            if (File.Exists(rtssDirectory + @"\RTSS.exe"))
            {
                Process process = new Process();
                process.StartInfo.FileName = rtssDirectory + @"\RTSS.exe";
                process.Start();

                process.Dispose();
                process = null;
            }
        }
        public static bool RTSSRunning()
        {
            Process[] pname = Process.GetProcessesByName("rtss");
            if (pname.Length != 0) return true;
            else return false;
        }

        public static void setRTSSFPSLimit(int frameLimit)
        {
            if (RTSSRunning())
            {
                RTSS.LoadProfile();
                RTSS.SetProfileProperty("FramerateLimit", frameLimit);
                fps = frameLimit;
                RTSS.SaveProfile();
                RTSS.UpdateProfiles();
            }

        }


        public static bool GetProfileProperty<T>(string propertyName, out T value)
        {
            var bytes = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            value = default;
            try
            {
                if (!GetProfileProperty(propertyName, handle.AddrOfPinnedObject(), (uint)bytes.Length))
                    return false;

                value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                handle.Free();
            }
        }

        public static bool SetProfileProperty<T>(string propertyName, T value)
        {
            var bytes = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
                return SetProfileProperty(propertyName, handle.AddrOfPinnedObject(), (uint)bytes.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("RTSSHooks64.dll")]
        public static extern uint SetFlags(uint dwAND, uint dwXOR);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void LoadProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void SaveProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void DeleteProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern bool GetProfileProperty(string propertyName, IntPtr value, uint size);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern bool SetProfileProperty(string propertyName, IntPtr value, uint size);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void ResetProfile(string profile = GLOBAL_PROFILE);

        [DllImport("RTSSHooks64.dll", CharSet = CharSet.Ansi)]
        public static extern void UpdateProfiles();

        private static void PostMessage(uint Msg, IntPtr wParam, IntPtr lParam)
        {
            var hWnd = FindWindow(null, "RTSS");
            if (hWnd == IntPtr.Zero)
                hWnd = FindWindow(null, "RivaTuner Statistics Server");

            if (hWnd != IntPtr.Zero)
                PostMessage(hWnd, Msg, wParam, lParam);
        }

        public static uint EnableFlag(uint flag, bool status)
        {
            var current = SetFlags(~flag, status ? flag : 0);
            UpdateSettings();
            return current;
        }

        public static void UpdateSettings()
        {
            PostMessage(WM_RTSS_UPDATESETTINGS, IntPtr.Zero, IntPtr.Zero);
        }

        public const uint WM_APP = 0x8000;
        public const uint WM_RTSS_UPDATESETTINGS = WM_APP + 100;
        public const uint WM_RTSS_SHOW_PROPERTIES = WM_APP + 102;

        public const uint RTSSHOOKSFLAG_OSD_VISIBLE = 1;
        public const uint RTSSHOOKSFLAG_LIMITER_DISABLED = 4;
        public const string GLOBAL_PROFILE = "";
    }

    public static class RunningGames
    {
        public static List<AppFlags> appFlags = new List<AppFlags>()
        {
            {AppFlags.Direct3D12 },
            {AppFlags.Direct3D12AFR },
            {AppFlags.Direct3D9Ex },
            {AppFlags.Direct3D9 },
            {AppFlags.Direct3D10 },
            {AppFlags.Direct3D11 },
            {AppFlags.Vulkan },
            {AppFlags.OpenGL }

        };
        public static unsafe int closeGame()
        {
            int processID = 0;
            if (RTSS.RTSSRunning())
            {

                AppFlags appFlag = appFlags[0];
                AppEntry[] appEntries = OSD.GetAppEntries(appFlag);


                while (appEntries.Length == 0)
                {
                    foreach (AppFlags af in appFlags)
                    {
                        appEntries = OSD.GetAppEntries(af);
                        if (appEntries.Length > 0) { appFlag = af; break; }
                    }

                }

                foreach (var app in appEntries)
                {
                    processID = app.ProcessId;

                    System.Diagnostics.Process procs = null;

                    try
                    {
                        procs = Process.GetProcessById(processID);



                        if (!procs.HasExited)
                        {
                            procs.CloseMainWindow();
                        }
                    }
                    finally
                    {
                        if (procs != null)
                        {
                            procs.Dispose();
                        }
                    }
                }



            }
            return processID;


        }
        public static Dictionary<string, int> gameRunningDictionary()
        {
            //RTSS01
            Dictionary<string, int> returnDictionary = new Dictionary<string, int>();
            try
            {

                if (RTSS.RTSSRunning())
                {


                    AppEntry[] appEntries;


                    foreach (AppFlags af in appFlags)
                    {
                        appEntries = OSD.GetAppEntries(af);
                        if (appEntries.Length > 0)
                        {
                            foreach (var app in appEntries)
                            {
                                string[] gamedir = app.Name.Split('\\');
                                if (gamedir.Length > 0)
                                {
                                    string currGameName = gamedir[gamedir.Length - 1].Substring(0, gamedir[gamedir.Length - 1].Length - 4);
                                    returnDictionary.Add(currGameName, app.ProcessId);
                                }

                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return returnDictionary;
            }
            return returnDictionary;
        }
        public static unsafe int gameRunningProcessID()
        {

            int gameRunning = 0;
            try
            {

                if (RTSS.RTSSRunning())
                {

                    AppFlags appFlag = appFlags[0];
                    AppEntry[] appEntries = OSD.GetAppEntries(appFlag);

                    foreach (AppFlags af in appFlags)
                    {
                        appEntries = OSD.GetAppEntries(af);
                        if (appEntries.Length > 0) { appFlag = af; break; }
                    }

                    foreach (var app in appEntries)
                    {
                        gameRunning = app.ProcessId;
                        break;

                    }

                }




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
            return gameRunning;
        }


    }
}