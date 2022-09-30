using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AATUV3.Pages;
using UXTU.Properties;
using AATUV3.Scripts;
using RyzenSmu;
using RyzenSMUBackend;
using System.IO;
using AATUV3.Scripts.SMU_Backend_Scripts;
using Forms = System.Windows.Forms;

namespace AATUV3
{
    public partial class MainWindow : Window
    {
        //check if program has admin rights
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static string menu = "";
        public static string AppName = "";
        public static string mbo = "";
        public static Timer reApply;

        [DllImport("inpoutx64.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysLong(UIntPtr memAddress, ref uint DData);

        [DllImport("inpoutx64.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsInpOutDriverOpen();

        [DllImport("inpoutx64.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetPhysLong(UIntPtr memAddress, uint DData);

        public MainWindow()
        {
            if (!MainWindow.IsAdministrator())
            {
                // Restart and run as admin
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                startInfo.Arguments = "restart";
                Process.Start(startInfo);
                this.Close();
            }

            InitializeComponent();
            //load main menu on load
            PagesNavigation.Navigate(new System.Uri("Pages/HomeMenu.xaml", UriKind.RelativeOrAbsolute));

            //Set menu lable to menu name
            string menu = (string)rdHome.Content;
            lblMenu.Content = menu.ToUpper();

            //Get current directory
            if (Settings.Default["Path"].ToString() == "" || Settings.Default["Path"].ToString() == null || Settings.Default["Path"].ToString().Contains("System32"))
            {
                //Get current path
                var path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

                //Save APU Name
                Settings.Default["APUName"] = System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

                //Save CPUID
                ManagementClass managClass = new ManagementClass("win32_processor");
                ManagementObjectCollection managCollec = managClass.GetInstances();

                foreach (ManagementObject managObj in managCollec)
                {
                    Settings.Default["APUID"] = managObj.Properties["processorID"].Value.ToString();
                    break;
                }

                //Save path
                Settings.Default["Path"] = path;
                Settings.Default.Save();

                // Debug path MessageBox.Show(path);
            }

            //Detect if an AYA Neo is being used
            ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject queryObj in baseboardSearcher.Get())
            {
                mbo = queryObj["Manufacturer"].ToString();
                mbo = mbo.ToLower();
            }

            if (mbo.Contains("aya") && this.WindowState == System.Windows.WindowState.Normal)
            {
                //Get screen working bounds
                this.MaxHeight = SystemParameters.WorkArea.Height;
                this.MaxWidth = SystemParameters.WorkArea.Width;
                this.WindowState = System.Windows.WindowState.Maximized;
            }

            string CPUName = "";

            //Get CPU name
            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in myProcessorObject.Get())
            {
                CPUName = obj["Name"].ToString();
            }

            if (CPUName.Contains("AMD") || CPUName.Contains("Ryzen") || CPUName.Contains("Athlon") || CPUName.Contains("Radeon") || CPUName.Contains("AMD Custom APU 0405"))
            {
                if ((bool)Settings.Default["firstBoot"] == true)
                {
                    BasicExeBackend.ApplySettings("\\InstallDriver.exe", "", false);
                    Settings.Default["firstBoot"] = false;
                    Settings.Default.Save();
                }

                Families.SetFam();

                //Remove everything before Ryzen/Althon
                if (CPUName.Contains("R"))
                {
                    CPUName = CPUName.Substring(CPUName.IndexOf("R"));
                }
                else
                {
                    CPUName = CPUName.Substring(CPUName.IndexOf("A"));
                }

                //Remove everything after the model identifier
                if (CPUName.Contains("HX"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("X") + 1);
                }
                else if (CPUName.Contains("HS"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("S") + 1);
                }
                else if (CPUName.Contains("H"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("H") + 1);
                }
                else if (CPUName.Contains("U"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("U") + 1);
                }
                else if (CPUName.Contains("X"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("X") + 1);
                }
                else if (CPUName.Contains("V"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("V") + 1);
                }
                else if (CPUName.Contains("X"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("Z") + 1);

                }
                else if (CPUName.Contains("E"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("E") + 1);
                }
                else if (CPUName.Contains("e"))
                {
                    CPUName = CPUName.Substring(0, CPUName.IndexOf("e") + 1);
                }

                var startColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#f10046");
                var endColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#be0037");

                Application.Current.Resources["PrimaryBlueColor"] = new LinearGradientBrush(startColour.Color, endColour.Color, new Point(0, 1), new Point(1, 0));

                startColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#eb0045");
                endColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#b80036");

                Application.Current.Resources["PrimaryBlueColorHover"] = new LinearGradientBrush(startColour.Color, endColour.Color, new Point(0, 0), new Point(1, 1));

                startColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#a80132");
                endColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#750123");
                Application.Current.Resources["PrimaryBlueColorDown"] = new LinearGradientBrush(startColour.Color, endColour.Color, new Point(0, 0), new Point(1, 1));

                //RAVEN - 0
                //PICASSO - 1
                //DALI - 2
                //RENOIR/LUCIENNE - 3
                //MATISSE - 4
                //VANGOGH - 5
                //VERMEER - 6
                //CEZANNE/BARCELO - 7
                //REMBRANDT - 8
                //PHEONIX - 9
                //RAPHAEL/DRAGON RANGE - 10

                switch (Families.FAMID)
                {
                    case 4:
                    case 6:
                    case 10:
                        AppName = "AMD CPU Tuning Utility";
                        break;
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 5:
                    case 7:
                    case 8:
                    case 9:
                        AppName = "AMD APU Tuning Utility";
                        break;
                    default:
                        AppName = "AMD APU Tuning Utility";
                        break;
                }
            }
            else if (CPUName.Contains("Intel"))
            {
                AppName = "Intel CPU Tuning Utility";

                var startColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#1e90ff");
                var endColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#1ec8ff");

                Application.Current.Resources["PrimaryBlueColor"] = new LinearGradientBrush(startColour.Color, endColour.Color, new Point(0, 1), new Point(1, 0));

                startColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#5160ff");
                endColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#51a9ff");

                Application.Current.Resources["PrimaryBlueColorHover"] = new LinearGradientBrush(startColour.Color, endColour.Color, new Point(0, 0), new Point(1, 1));

                startColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#0059d1");
                endColour = (SolidColorBrush)new BrushConverter().ConvertFrom("#008dd1");
                Application.Current.Resources["PrimaryBlueColorDown"] = new LinearGradientBrush(startColour.Color, endColour.Color, new Point(0, 0), new Point(1, 1));


                //Application.Current.Resources["PrimaryBlueColorHover"] = IntelBlueHover;
                //Application.Current.Resources["PrimaryBlueColorDown"] = IntelBlueDown;

                rdClock.Visibility = Visibility.Collapsed;
                rdSnow.Visibility = Visibility.Collapsed;
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";

            //Garbage collection
            DispatcherTimer Garbage = new DispatcherTimer();
            Garbage.Interval = TimeSpan.FromSeconds(8);
            Garbage.Tick += GarbageCollect_Tick;
            Garbage.Start();

            OverlayWindow overlay = new OverlayWindow();
            overlay.Show();


            if ((bool)Settings.Default["SensorOverlay"] == true)
            {
                try
                {
                    SensorWindow sensorWindow = new SensorWindow();
                    sensorWindow.Show();
                }
                catch (Exception ex)
                {

                }
            }

            Settings.Default["APUName"] = CPUName;
            Settings.Default.Save();
            Tray();

            if (Convert.ToBoolean(Settings.Default["StartMinimised"]) == true)
            {
                this.WindowState = WindowState.Minimized;
                _trayIcon.Visible = true;
                this.Hide();
            }

            if (Settings.Default.ApplyOCAtStart) ApplyOC();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //Close application
            System.Windows.Application.Current.Shutdown();
        }

        private Forms.NotifyIcon _trayIcon;


        private void Tray()
        {
            _trayIcon = new Forms.NotifyIcon();
            _trayIcon.Icon = new System.Drawing.Icon(Settings.Default.Path + "\\favicon.ico");
            _trayIcon.Text = $"UXTU";
            _trayIcon.Click += trayIcon_Click;
        }

        private void trayIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            WindowState = WindowState.Normal;
            _trayIcon.Visible = false;
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            //Get screen working bounds
            this.MaxHeight = SystemParameters.WorkArea.Height;
            this.MaxWidth = SystemParameters.WorkArea.Width;

            //Maximise window if Window is in normal view else set normal window view
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            //Minimise window
            _trayIcon.Visible = true;
            WindowState = WindowState.Minimized;
            this.Hide();
        }

        private void rdHome_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD APU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/HomeMenu.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdHome.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("AMD CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/HomeMenu.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdHome.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/HomeMenu.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdHome.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void rdAC_Click(object sender, RoutedEventArgs e)
        {

            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/ASUSAC.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            menu = (string)rdAC.Content;
            lblMenu.Content = menu.ToUpper();


            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void rdSnow_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD APU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ProjectSnowdrop.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdSnow.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("AMD CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdSnow.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdSnow.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void rdCustom_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD APU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/CustomPresets.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdCustom.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("AMD CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/AMDCPU/AMDCustomPresets.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdCustom.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/Intel/IntelCustomPresets.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdCustom.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void rdAdaptive_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD APU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdAdaptive.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("AMD CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdAdaptive.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdAdaptive.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void rdClock_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD APU") || AppName.Contains("AMD CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ClockControl.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdClock.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdClock.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        //private void rdAC_Click(object sender, RoutedEventArgs e)
        //{
        //    //Load menu
        //    PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
        //    //Set menu lable to menu name
        //    string menu = (string)rdAC.Content;
        //    lblMenu.Content = menu.ToUpper();

        //    //Change window title
        //    this.Title = $"AMD APU Tuning Utility - {menu}";
        //}

        private void rdInfo_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD"))
            {
                if (pmtables.PMT_Sensors.Length == 0 || pmtables.PMT_Sensors == null)
                {
                    if (File.Exists(Settings.Default["Path"].ToString() + $"\\bin\\pmtables\\0x00{string.Format("{0:x}", Addresses.PMTableVersion)}-sensors.txt"))
                    {
                        pmtables.PMT_Sensors = File.ReadAllLines(Settings.Default["Path"].ToString() + $"\\bin\\pmtables\\0x00{string.Format("{0:x}", Addresses.PMTableVersion)}-sensors.txt");
                    }
                }
            }

            if (File.Exists(Settings.Default["Path"].ToString() + $"\\bin\\pmtables\\0x00{string.Format("{0:x}", Addresses.PMTableVersion)}-sensors.txt"))
            {
                PagesNavigation.Navigate(new System.Uri("Pages/BasicSensor.xaml", UriKind.RelativeOrAbsolute));
            }
            else
            {
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdInfo.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void rdSettings_Click(object sender, RoutedEventArgs e)
        {
            if (AppName.Contains("AMD APU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/Settings.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdSettings.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("AMD CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/Settings.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdSettings.Content;
                lblMenu.Content = menu.ToUpper();
            }

            if (AppName.Contains("Intel CPU"))
            {
                //Load menu
                PagesNavigation.Navigate(new System.Uri("Pages/Settings.xaml", UriKind.RelativeOrAbsolute));
                //Set menu lable to menu name
                menu = (string)rdSettings.Content;
                lblMenu.Content = menu.ToUpper();
            }

            //Change window title
            this.Title = $"{AppName} - {menu}";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Make window follow cursor when button is down on top bar
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        async void GarbageCollect_Tick(object sender, EventArgs e)
        {
            //Free resources back to system
            await BasicExeBackend.Garbage_Collect();
        }

        private void home_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Close application
            //System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void ApplyOC()
        {
            int i = 0;
            if (Settings.Default.isAllCoreCLK == true)
            {
                SendCommand.set_oc_clk((uint)Settings.Default.AllCoreClk);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)Settings.Default.AllCoreClk);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)Settings.Default.AllCoreClk);
                SendCommand.set_enable_oc();
                i++;
            }

            if (Settings.Default.isVID == true)
            {
                double vid = Math.Round((double)Settings.Default.CPUVID / 1000, 2);
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_enable_oc();
                i++;
            }

            if (Settings.Default.isBUS == true)
            {
                RwMmioAmd MMIO = new RwMmioAmd();
                MMIO.SetBclk(Convert.ToDouble(Settings.Default.BusCLK));
                i++;
            }

            if (Settings.Default.isCPUCO == true)
            {
                if (Settings.Default.COCPU >= 0)
                {
                    SendCommand.set_coall((uint)Settings.Default.COCPU);
                }
                else
                {
                    SendCommand.set_coall(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)Settings.Default.COCPU)));
                }
                i++;
            }

            if (Settings.Default.isGPUCO == true)
            {
                if (Settings.Default.COiGPU >= 0)
                {
                    SendCommand.set_cogfx((uint)Settings.Default.COiGPU);
                }
                else
                {
                    SendCommand.set_cogfx(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)Settings.Default.COiGPU)));
                }
                i++;
            }

            //if (cbiGPU.IsChecked == true)
            //{
            //    SendCommand.set_gfx_clk((uint)nudiGPU.Value);
            //    i++;
            //}

            if (Settings.Default.isNV == true)
            {
                //Get RyzenAdj path
                string path = "\\bin\\oc.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(path, "0 " + Settings.Default.dGPUCLK + " " + Settings.Default.dGPUMem, true);
                BasicExeBackend.ApplySettings(path, "1 " + Settings.Default.dGPUCLK + " " + Settings.Default.dGPUMem, true);
                BasicExeBackend.ApplySettings(path, "2 " + Settings.Default.dGPUCLK + " " + Settings.Default.dGPUMem, true);
                i++;
            }

            if (i == 0)
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }
            else
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
            }
        }
    }
}
