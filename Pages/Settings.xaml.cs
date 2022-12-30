using UXTU.Properties;
using AATUV3.Scripts;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using UXTU.Scripts;
using Microsoft.Win32.TaskScheduler;
using System.Reflection;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class SettingsMenu : UserControl
    {
        public SettingsMenu()
        {
            InitializeComponent();
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default["AutoReapply"] = (bool)cbAutoReapply.IsChecked;
            Settings.Default["AutoReapplyTime"] = (int)nudAutoReapply.Value;
            Settings.Default["StartMinimised"] = (bool)cbStartMinimised.IsChecked;
            Settings.Default["StartOnBoot"] = (bool)cbStartOnBoot.IsChecked;
            Settings.Default["SensorOverlay"] = (bool)cbSensorOverlay.IsChecked;
            Settings.Default.ApplyOCAtStart = (bool)cbApplyOC.IsChecked;
            Settings.Default.isMagpie = (bool)cbMagpie.IsChecked;
            Settings.Default.Save();

            if (cbStartOnBoot.IsChecked == true)
            {
                //var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                //RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                //key.SetValue("MyApplication", System.Reflection.Assembly.GetExecutingAssembly().Location.ToString());
                try
                {
                    var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                    key.DeleteValue("MyApplication", false);
                }
                catch { }

                try
                {
                    // Get the service on the local machine
                    using (TaskService ts = new TaskService())
                    {
                        if (!ts.RootFolder.AllTasks.Any(t => t.Name == "UXTU"))
                        {
                            // Create a new task definition and assign properties
                            TaskDefinition td = ts.NewTask();
                            td.Principal.RunLevel = TaskRunLevel.Highest;
                            td.RegistrationInfo.Description = "Start UXTU";

                            // Create a trigger that will fire the task at this time every other day
                            td.Triggers.Add(new LogonTrigger());

                            // Create an action that will launch Notepad whenever the trigger fires
                            td.Actions.Add(System.Reflection.Assembly.GetExecutingAssembly().Location);

                            // Register the task in the root folder
                            ts.RootFolder.RegisterTaskDefinition(@"UXTU", td);
                        }

                    }
                } catch { }
            }
            else
            {


                Settings.Default.StartUp = 0;
                Settings.Default.Save();
                // Get the service on the local machine
                using (TaskService ts = new TaskService())
                {
                    if (ts.RootFolder.AllTasks.Any(t => t.Name == "UXTU"))
                    {
                        // Remove the task we just created
                        ts.RootFolder.DeleteTask("UXTU");
                    }
                }
            }

            if (cbMagpie.IsChecked == true)
            {
                int i = 0;

                foreach (var process in Process.GetProcessesByName("magpie")) i++;
                foreach (var process in Process.GetProcessesByName("Magpie")) i++;

                if (i <= 0) GlobalVariables.UpdateMagpie();
            }

            BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Saved! Settings-have-been-saved-successfully.", false);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            cbAutoReapply.IsChecked = Convert.ToBoolean(Settings.Default["AutoReapply"]);
            nudAutoReapply.Value = Convert.ToDecimal(Settings.Default["AutoReapplyTime"]);
            cbStartMinimised.IsChecked = Convert.ToBoolean(Settings.Default["StartMinimised"]);
            cbStartOnBoot.IsChecked = Convert.ToBoolean(Settings.Default["StartOnBoot"]);
            cbSensorOverlay.IsChecked = Convert.ToBoolean(Settings.Default["SensorOverlay"]);
            cbMagpie.IsChecked = Settings.Default.isMagpie;
            cbApplyOC.IsChecked = Settings.Default.ApplyOCAtStart;
        }
    }
}
