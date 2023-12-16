using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Wpf.Ui.Common.Interfaces;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : INavigableView<ViewModels.SettingsViewModel>
    {
        public ViewModels.SettingsViewModel ViewModel
        {
            get;
        }

        public SettingsPage(ViewModels.SettingsViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            cbStartBoot.IsChecked = Settings.Default.StartOnBoot;
            cbStartMini.IsChecked = Settings.Default.StartMini;
            cbApplyStart.IsChecked = Settings.Default.ApplyOnStart;
            cbAutoReapply.IsChecked = Settings.Default.AutoReapply;
            nudAutoReapply.Value = Settings.Default.AutoReapplyTime;
            cbAutoCheck.IsChecked = Settings.Default.UpdateCheck;
            cbAdaptive.IsChecked = Settings.Default.isStartAdpative;
            cbTrack.IsChecked = Settings.Default.isTrack;

            checkUpdate();
        }

        private void cbStartBoot_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (TaskService ts = new TaskService())
            {
                if (ts.RootFolder.AllTasks.Any(t => t.Name == "UXTU"))
                {
                    // Remove the task we just created
                    ts.RootFolder.DeleteTask("UXTU");
                }
            }

            if (cbStartBoot.IsChecked == true)
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
                        td.Settings.DisallowStartIfOnBatteries = false;
                        td.Settings.StopIfGoingOnBatteries = false;
                        td.Settings.DisallowStartOnRemoteAppSession = false;

                        // Create a trigger that will fire the task at this time every other day
                        td.Triggers.Add(new LogonTrigger());

                        string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                        path = path.Replace("Universal x86 Tuning Utility.dll", "Universal x86 Tuning Utility.exe");

                        // Create an action that will launch Notepad whenever the trigger fires
                        td.Actions.Add(path);

                        // Register the task in the root folder
                        ts.RootFolder.RegisterTaskDefinition(@"UXTU", td);
                    }
                }
            }

            Settings.Default.StartOnBoot = (bool)cbStartBoot.IsChecked;
            Settings.Default.Save();
        }

        private void cbStartMini_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.StartMini = (bool)cbStartMini.IsChecked;
            Settings.Default.Save();
        }

        private void cbAutoReapply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.AutoReapply = (bool)cbAutoReapply.IsChecked; 
            Settings.Default.Save();
        }

        private void nudAutoReapply_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Settings.Default.AutoReapplyTime = (int)nudAutoReapply.Value;
            Settings.Default.Save();
        }

        private void cbApplyStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.ApplyOnStart = (bool)cbApplyStart.IsChecked;
            Settings.Default.Save();
        }

        private async void btnCheck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            checkUpdate(true);
        }

        private async void checkUpdate(bool isUserCheck = false)
        {
            if (IsInternetAvailable())
            {
                var updateManager = new UpdateManager("JamesCJ60", "Universal-x86-Tuning-Utility", App.version, "C:\\");

                var isUpdateAvailable = await updateManager.IsUpdateAvailable();

                if (isUpdateAvailable)
                {
                    tbDownloadMsg.Text = "An update for Universal x86 Tuning Utility has been found!";
                    btnDownload.Visibility = System.Windows.Visibility.Visible;
                }
                else if(isUserCheck) tbDownloadMsg.Text = "Universal x86 Tuning Utility is up to date!";
            }
            else if (isUserCheck) tbDownloadMsg.Text = "No internet connection!";
        }

        private async void btnDownload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var updateManager = new UpdateManager("JamesCJ60", "Universal-x86-Tuning-Utility", App.version, "C:\\");

            var isUpdateAvailable = await updateManager.IsUpdateAvailable();

            if (isUpdateAvailable)
            {
                tbDownloadMsg.Text = "Universal x86 Tuning Utility will close and the installer will open when the download is complete";

                await updateManager.DownloadAndInstallUpdate();

                string filePath = "C:\\Universal.x86.Tuning.Utility.V2.msi";

                try
                {
                    // show the MSI and close the main application
                    Process p = new Process();
                    p.StartInfo.FileName = "msiexec";
                    p.StartInfo.Arguments = $"/i {filePath}";
                    p.Start();
                    System.Windows.Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    // log error or display error message to user
                    MessageBox.Show("Failed to launch MSI: " + ex.Message);
                }
            }
        }

        private static bool IsInternetAvailable()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var result = ping.Send("8.8.8.8", 2000); // ping Google DNS server
                    return result.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void cbAutoCheck_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.UpdateCheck = (bool)cbAutoCheck.IsChecked;
            Settings.Default.Save();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            Garbage.Garbage_Collect();
        }

        private void btnStressTest_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Settings.Default.Path + @"\Assets\Stress-Test\AVX2 Stress Test.exe"))
            {
                Process process = new Process();
                process.StartInfo.FileName = Settings.Default.Path + @"\Assets\Stress-Test\AVX2 Stress Test.exe";
                process.Start();

                process.Dispose();
                process = null;
            }
        }

        private void cbAdaptive_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.isStartAdpative = (bool)cbAdaptive.IsChecked;
            Settings.Default.Save();
        }

        private void cbTrack_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.isTrack = (bool)cbTrack.IsChecked;
            Settings.Default.Save();
        }
    }
}