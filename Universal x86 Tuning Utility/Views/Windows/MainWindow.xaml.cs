using Microsoft.Win32;
using RyzenSmu;
using System;
using System.Globalization;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.AMD_Backend;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Universal_x86_Tuning_Utility.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public ViewModels.MainWindowViewModel ViewModel
        {
            get; set;
        }

        DispatcherTimer GC = new DispatcherTimer();
        DispatcherTimer Misc = new DispatcherTimer();
        INavigationService _navigationService;
        public DispatcherTimer autoReapply = new DispatcherTimer();
        public MainWindow(ViewModels.MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            _ = Tablet.TabletDevices;
            SetPageService(pageService);
            if (Settings.Default.StartMini == true) { this.WindowState = WindowState.Minimized; }
            else
            {
                if(GetSystemInfo.Manufacturer.ToUpper().Contains("AYANEO") || GetSystemInfo.Manufacturer.ToUpper().Contains("GPD") || GetSystemInfo.Product.ToUpper().Contains("ONEXPLAYER"))
                {
                    int displayCount = Screen.AllScreens.Length;
                    if(displayCount < 2)
                    {
                        this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                        mainWindow.WindowState = WindowState.Maximized;
                    }
                }
            }
            navigationService.SetNavigationControl(RootNavigation);
            _navigationService = navigationService;

            GC.Interval = TimeSpan.FromSeconds(2);
            GC.Tick += GC_Tick;
            GC.Start();

            Misc.Interval = TimeSpan.FromSeconds(2.5);
            Misc.Tick += Misc_Tick;
            Misc.Start();

            autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
            autoReapply.Tick += AutoReapply_Tick;
            autoReapply.Start();


            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            getBatTempData();

            tbMain.Title = $"Universal x86 Tuning Utility - {Family.CPUName}";

            //PowerPlans.HideAttribute("SUB_PROCESSOR", "PERFEPP");
            //PowerPlans.HideAttribute("SUB_PROCESSOR", "PERFEPP1");
            //PowerPlans.HideAttribute("SUB_PROCESSOR", "PERFAUTONOMOUS");

            if (Settings.Default.ApplyOnStart) 
                if (Settings.Default.CommandString != null && Settings.Default.CommandString != "") 
                    if (Family.TYPE != Family.ProcessorType.Intel)
                    {
                        RyzenAdj_To_UXTU.Translate(Settings.Default.CommandString);
                        ToastNotification.ShowToastNotification("Settings Reapplied!", $"Your last applied settings have been reapplied!");
                    }

            Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Mica, true);
        }

        private async void getBatTempData()
        {
            try
            {
                await Task.Run(() => PerfCounters.ReadSensors());

                float bat = (float)PerfCounters.BatteryDischarge;
                if (bat > 0 && miCharge.Visibility == Visibility.Collapsed) miCharge.Visibility = Visibility.Visible;
                if (bat <= 0 && miCharge.Visibility == Visibility.Visible) miCharge.Visibility = Visibility.Collapsed;

                if (bat > 0 && miCharge.Visibility == Visibility.Visible) miCharge.Header = $"Charge Rate: -{bat.ToString("0.00")}W";
            } catch
            {
                miCharge.Visibility = Visibility.Collapsed;
            }
        }

        private async void Misc_Tick(object sender, EventArgs e)
        {
            getBatTempData();

            Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Mica, true);
        }

        private async void updateDownloads()
        {
            try
            {
                if (App.IsInternetAvailable())
                {
                    string owner = "JamesCJ60";
                    string repo = "Universal-x86-Tuning-Utility";

                    int downloadCount = await GetGitHubDownloadCount(owner, repo); 
                    if (downloadCount > 100000000) ViewModel.Downloads = $"Downloads: {((downloadCount / 1000) / 1000).ToString("#.#")}m";
                    else if (downloadCount > 1000) ViewModel.Downloads = $"Downloads: {(downloadCount / 1000).ToString("#.#")}k";
                    else ViewModel.Downloads = $"Downloads: {downloadCount}";

                    ViewModel.IsDownloads = true;
                }
                else ViewModel.IsDownloads = false;
            }
            catch (Exception ex) { ViewModel.IsDownloads = false; }
        }

        private async void AutoReapply_Tick(object sender, EventArgs e)
        {
            if ((bool)Settings.Default.AutoReapply == true && (bool)Settings.Default.isAdaptiveModeRunning == false)
            {
                string commands = (string)Settings.Default.CommandString;
                //Check if RyzenAdjArguments is populated
                if (commands != null && commands != "")
                {
                    await Task.Run(() =>
                    {
                        RyzenAdj_To_UXTU.Translate(commands);
                        Thread.Sleep(10);
                    });
                }

                if (autoReapply.Interval != TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime))
                {
                    autoReapply.Stop();
                    autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
                    autoReapply.Start();
                }
            }
        }


        int i = 0;
        bool setup = false;
        async void GC_Tick(object sender, EventArgs e)
        {
            Task.Run(() => Garbage.Garbage_Collect()); // Execute Garbage_Collect on a separate thread

            if (i < 4) i++;

            if (i > 2 && setup == false)
            {
                if (Settings.Default.StartMini == true && this.WindowState == WindowState.Minimized) this.ShowInTaskbar = false;
                GC.Stop();
                GC.Interval = TimeSpan.FromSeconds(20);
                GC.Tick += GC_Tick;
                GC.Start();
                setup = true;
            }
        }

        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        private void UiWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
                updateDownloads();
            }
        }

        private void NotifyIcon_LeftClick(Wpf.Ui.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
        }

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if ((bool)Settings.Default.isAdaptiveModeRunning == false)
            {
                if (e.Mode == PowerModes.StatusChange)
                {
                    await Task.Run(() => getBattery());
                    await Task.Run(() => getBattery());

                    if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                    {
                        if (Settings.Default.acCommandString != null && Settings.Default.acCommandString != "")
                        {
                            Settings.Default.CommandString = Settings.Default.acCommandString;
                            Settings.Default.Save();
                            await Task.Run(() => RyzenAdj_To_UXTU.Translate(Settings.Default.acCommandString));
                            ToastNotification.ShowToastNotification("Charge Preset Applied!", $"Your charge preset settings have been applied!");
                        }
                    }
                    else
                    {
                        if (Settings.Default.dcCommandString != null && Settings.Default.dcCommandString != "")
                        {
                            Settings.Default.CommandString = Settings.Default.dcCommandString;
                            Settings.Default.Save();
                            await Task.Run(() => RyzenAdj_To_UXTU.Translate(Settings.Default.dcCommandString));
                            ToastNotification.ShowToastNotification("Discharge Preset Applied!", $"Your discharge preset settings have been applied!");
                        }
                    }
                }

                if (e.Mode == PowerModes.Resume)
                {
                    if (Settings.Default.resumeCommandString != null && Settings.Default.resumeCommandString != "")
                    {
                        Settings.Default.CommandString = Settings.Default.resumeCommandString;
                        Settings.Default.Save();
                        Task.Run(() => RyzenAdj_To_UXTU.Translate(Settings.Default.resumeCommandString));
                        ToastNotification.ShowToastNotification("Resume Preset Applied!", $"Your resume preset settings have been applied!");
                    }
                }
            }
        }

        static UInt16 statuscode = 0;
        public static void getBattery()
        {
            int i = 0;
            do
            {
                try
                {
                    ManagementClass wmi = new ManagementClass("Win32_Battery");
                    ManagementObjectCollection allBatteries = wmi.GetInstances();

                    //Get battery level from each system battery detected
                    foreach (var battery in allBatteries)
                    {
                        statuscode = (UInt16)battery["BatteryStatus"];
                    }

                    i++;
                }
                catch (Exception ex)
                {

                }
            } while (i < 2);
        }

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.isAdaptiveModeRunning = false;
            Settings.Default.Save();
            System.Windows.Application.Current.Shutdown();
        }

        private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.isAdaptiveModeRunning = false;
            Settings.Default.Save();
            Fan_Control.disableFanControl();
        }

        static async Task<int> GetGitHubDownloadCount(string owner, string repo)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppName", "1.0"));

                string apiUrl = $"https://api.github.com/repos/{owner}/{repo}";
                string releasesUrl = $"{apiUrl}/releases";

                // Retrieve releases
                var response = await client.GetAsync(releasesUrl);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                var releases = JsonDocument.Parse(responseContent).RootElement;

                int downloadCount = 0;

                // Calculate total download count
                foreach (var release in releases.EnumerateArray())
                {
                    foreach (var asset in release.GetProperty("assets").EnumerateArray())
                    {
                        downloadCount += asset.GetProperty("download_count").GetInt32();
                    }
                }

                return downloadCount;
            }
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            updateDownloads();
        }
    }
}

