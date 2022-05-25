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
using AATUV3.Properties;
using AATUV3.Scripts;
using RyzenSmu;
using RyzenSMUBackend;

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

        public static DispatcherTimer reApply = new DispatcherTimer();

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

            if((bool)Settings.Default["firstBoot"] == true)
            {
                Backend.ApplySettings("InstallDriver.exe", "", true);
                Settings.Default["firstBoot"] = false;
                Settings.Default.Save();
            }
            

            

            InitializeComponent();
            //load main menu on load
            PagesNavigation.Navigate(new System.Uri("Pages/HomeMenu.xaml", UriKind.RelativeOrAbsolute));

            //Set menu lable to menu name
            string menu = (string)rdHome.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";

            Families.SetFam();

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

            //Set up auto reapply timer
            reApply.Interval = TimeSpan.FromSeconds(Convert.ToInt32(Settings.Default["AutoReapplyTime"]));
            reApply.Tick += Auto_Reapply_Tick;
            reApply.Start();

            //Garbage collection
            DispatcherTimer Garbage = new DispatcherTimer();
            Garbage.Interval = TimeSpan.FromSeconds(8);
            Garbage.Tick += GarbageCollect_Tick;
            Garbage.Start();

            //OverlayWindow overlay = new OverlayWindow();
            //overlay.Show();


            if((bool)Settings.Default["SensorOverlay"] == true){
                try
                {
                    SensorWindow sensorWindow = new SensorWindow();
                    sensorWindow.Show();
                }
                catch(Exception ex)
                {

                }
            }
            

            //Detect if an AYA Neo is being used
            string mbo = "";
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

            Backend.Garbage_Collect();

            if (Convert.ToBoolean(Settings.Default["StartMinimised"]) == true)
            {
                this.WindowState = WindowState.Minimized;
            }

            string CPUName = "";

            //Get CPU name
            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in myProcessorObject.Get())
            {
                CPUName = obj["Name"].ToString();
            }

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

            Settings.Default["APUName"] = CPUName;
            Settings.Default.Save();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //Close application
            System.Windows.Application.Current.Shutdown();
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
            WindowState = WindowState.Minimized;
        }

        private void rdHome_Click(object sender, RoutedEventArgs e)
        {
            //Backend.Garbage_Collect();
            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/HomeMenu.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdHome.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
        }

        private void rdSnow_Click(object sender, RoutedEventArgs e)
        {
            //Backend.Garbage_Collect();
            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/ProjectSnowdrop.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdSnow.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
        }

        private void rdCustom_Click(object sender, RoutedEventArgs e)
        {
            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/CustomPresets.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdCustom.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
        }

        private void rdAdaptive_Click(object sender, RoutedEventArgs e)
        {
            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdAdaptive.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
        }

        private void rdClock_Click(object sender, RoutedEventArgs e)
        {
            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdClock.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
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
            //Load menu
            //PagesNavigation.Navigate(new System.Uri("Pages/Info.xaml", UriKind.RelativeOrAbsolute));
            PagesNavigation.Navigate(new System.Uri("Pages/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdInfo.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
        }

        private void rdSettings_Click(object sender, RoutedEventArgs e)
        {
            //Load menu
            PagesNavigation.Navigate(new System.Uri("Pages/Settings.xaml", UriKind.RelativeOrAbsolute));
            //Set menu lable to menu name
            string menu = (string)rdSettings.Content;
            lblMenu.Content = menu.ToUpper();

            //Change window title
            this.Title = $"AMD APU Tuning Utility - {menu}";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Make window follow cursor when button is down on top bar
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        //Auto reapply
        void Auto_Reapply_Tick(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(Settings.Default["AutoReapply"]) == true)
            {
                //Check if RyzenAdjArguments is populated
                if (Settings.Default["RyzenAdjArguments"].ToString() != null || Settings.Default["RyzenAdjArguments"].ToString() != "")
                {
                    //Cehck to make sure that Adpative Performance menu is not open
                    if (lblMenu.Content.ToString() != rdAdaptive.Content.ToString().ToUpper())
                    {
                        //Get RyzenAdj path
                        string path = "\\bin\\ryzenadj\\ryzenadj.exe";
                        //Pass settings on to be applied
                        Backend.ApplySettings(path, Settings.Default["RyzenAdjArguments"].ToString(), true);
                    }
                }
            }

            reApply.Interval = TimeSpan.FromSeconds(Convert.ToInt32(Settings.Default["AutoReapplyTime"]));
        }

        void GarbageCollect_Tick(object sender, EventArgs e)
        {
            //Free resources back to system
            Backend.Garbage_Collect();
        }

        private void home_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Close application
            //System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
