using Accord.Math.Distances;
using AutoOC.Controllers;
using AutoOC.Monitors;
using DuoVia.FuzzyStrings;
using GameLib.Plugin.RiotGames.Model;
using Gma.System.MouseKeyHook;
using HidSharp.Utility;
using Microsoft.Win32;
using RyzenSmu;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Scripts.UXTU_Super_Resolution;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui.Controls;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public ViewModels.MainWindowViewModel ViewModel { get; set; }

        DispatcherTimer Misc = new DispatcherTimer();
        public DispatcherTimer autoReapply = new DispatcherTimer();
        public DispatcherTimer autoRestore = new DispatcherTimer();

        public static bool isMini { get; private set; }
        public static NavigationView _mainWindowNav;
        private static INavigationService _navigationService;
        public static bool IsPageSelected(Type pageType) =>
            _mainWindowNav?.SelectedItem is INavigationViewItem item && item.TargetPageType == pageType;

        public MainWindow(ViewModels.MainWindowViewModel viewModel, INavigationViewPageProvider pageProvider, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            _navigationService = navigationService;
            _mainWindowNav = RootNavigation;

            Misc.Interval = TimeSpan.FromSeconds(1);
            Misc.Tick += Misc_Tick;
            Misc.Start();

            autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
            autoReapply.Tick += AutoReapply_Tick;
            autoReapply.Start();

            autoRestore.Interval = TimeSpan.FromSeconds(1);
            autoRestore.Tick += Controller.AutoRestore_Tick;
            autoRestore.Start();

            SetupNavigationService(pageProvider);

            SetupUI();
            ApplyOnStart();

            SystemEvents.PowerModeChanged += HandlePowerModeChange;
        }

        private void SetupNavigationService(INavigationViewPageProvider pageProvider)
        {
            _navigationService.SetNavigationControl(RootNavigation);
            RootNavigation.SetPageProviderService(pageProvider);
        }

        private void SetupUI()
        {
            tbMain.Title = $"Universal x86 Tuning Utility - {Family.CPUName}";
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this, WindowBackdropType.Mica, true);
        }

        private async void ApplyOnStart()
        {
            if (Settings.Default.ApplyOnStart)
                if (Settings.Default.CommandString != null && Settings.Default.CommandString != "")
                {

                    await Task.Run(() => GetBatteryStatus());

                    if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                    {
                        if (Settings.Default.acCommandString != null && Settings.Default.acCommandString != "")
                        {
                            Settings.Default.CommandString = Settings.Default.acCommandString;
                            Settings.Default.Save();
                            await RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.acCommandString);
                            ToastNotification.ShowToastNotification("Charge Preset Applied!", $"Your charge preset settings have been applied!");
                        }
                        else
                        {
                            await RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.CommandString);
                            ToastNotification.ShowToastNotification("Settings Reapplied!", $"Your last applied settings have been reapplied!");
                        }
                    }
                    else
                    {
                        if (Settings.Default.dcCommandString != null && Settings.Default.dcCommandString != "")
                        {
                            Settings.Default.CommandString = Settings.Default.dcCommandString;
                            Settings.Default.Save();
                            await RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.dcCommandString);
                            ToastNotification.ShowToastNotification("Discharge Preset Applied!", $"Your discharge preset settings have been applied!");
                        }
                        else
                        {
                            await RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.CommandString);
                            ToastNotification.ShowToastNotification("Settings Reapplied!", $"Your last applied settings have been reapplied!");
                        }
                    }
                }
        }


        InstabilityMonitor monitor = null;

        AdaptiveUndervoltController cpuController = null;
        AdaptiveUndervoltController iGpuController = null;

        int lastCPUUVOffset = 0;
        int lastiGPUUVOffset = 0;
        private async void Misc_Tick(object sender, EventArgs e)
        {
            try
            {
                if(Settings.Default.isAutoUvCPU == true)
                {

                    if(monitor == null) monitor = new InstabilityMonitor();

                    if (cpuController == null) cpuController = new AdaptiveUndervoltController(
                        monitor,
                        minOffset: -50,
                        stepSize: 1,
                        stableThreshold: 8,
                        cooldownThreshold: 4,
                        isIgpu: false
                    );

                    var newOffset = cpuController.UpdateOffset();
                    var commandValues = "";

                    if (Family.FAM < Family.RyzenFamily.Renoir) commandValues = $"--set-coper={(0 << 20) | (newOffset & 0xFFFF)} ";
                    else
                    {
                        if (newOffset >= 0) commandValues = commandValues + $"--set-coall={newOffset} ";
                        if (newOffset < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)newOffset))} ";
                    }

                    if (lastCPUUVOffset != newOffset)
                    {
                        await RyzenAdj_To_UXTU.TranslateAsync(commandValues, false, true);
                        lastCPUUVOffset = newOffset;
                    }

                    cpuController.RecordAppliedOffset(newOffset);
                }
                else if (Settings.Default.isAutoUvCPU == false && cpuController != null)
                {
                    cpuController.Dispose();
                    cpuController = null;
                }


                if (Settings.Default.isAutoUviGPU == true)
                {
                    if (monitor == null) monitor = new InstabilityMonitor();

                    if(iGpuController == null) iGpuController = new AdaptiveUndervoltController(
                        monitor,
                        minOffset: -50,
                        stepSize: 1,
                        stableThreshold: 8,
                        cooldownThreshold: 4,
                        isIgpu : true
                    );

                    var newOffset = iGpuController.UpdateOffset();
                    var commandValues = "";

                    if (newOffset >= 0) commandValues = commandValues + $"--set-cogfx={newOffset} ";
                    if (newOffset < 0) commandValues = commandValues + $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)newOffset))} ";

                    if (lastiGPUUVOffset != newOffset)
                    {
                        await RyzenAdj_To_UXTU.TranslateAsync(commandValues, false, true);
                        lastiGPUUVOffset = newOffset;
                    }

                    iGpuController.RecordAppliedOffset(newOffset);
                }
                else if (Settings.Default.isAutoUviGPU == false && iGpuController != null)
                {
                    iGpuController.Dispose();
                    iGpuController = null;
                }

                if(Settings.Default.isAutoUvCPU == false && Settings.Default.isAutoUviGPU == false && monitor != null)
                {
                    monitor.Stop();
                    monitor = null;
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed during adaptive undervolt monitoring tick");
            }
        }

        private static ushort statuscode;

        public static void GetBatteryStatus()
        {
            try
            {
                var batteryClass = new ManagementClass("Win32_Battery");
                var batteries = batteryClass.GetInstances();

                foreach (var battery in batteries)
                {
                    statuscode = (ushort)battery["BatteryStatus"];
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
                DiagnosticLogger.LogError(ex, "Failed to get battery status");
            }
        }

        private async void AutoReapply_Tick(object sender, EventArgs e)
        {
            try
            {
                if (niTray.Visibility == Visibility.Hidden && this.Visibility == Visibility.Hidden) niTray.Visibility = Visibility.Visible;

                if ((bool)Settings.Default.AutoReapply == true && (bool)Settings.Default.isAdaptiveModeRunning == false)
                {
                    string commands = (string)Settings.Default.CommandString;
                    //Check if RyzenAdjArguments is populated
                    if (commands != null && commands != "")
                    {
                        await RyzenAdj_To_UXTU.TranslateAsync(commands);
                    }

                    if (autoReapply.Interval != TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime))
                    {
                        autoReapply.Stop();
                        autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
                        autoReapply.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed during auto-reapply tick");
            }
        }
        private static void UpdateTimerInterval(DispatcherTimer timer, int newInterval)
        {
            if (timer.Interval == TimeSpan.FromSeconds(newInterval)) return;

            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(newInterval);
            timer.Start();
        }

        static string lastAppliedState = "";
        private async void HandlePowerModeChange(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                if ((bool)Settings.Default.isAdaptiveModeRunning == false)
                {
                    if (e.Mode == PowerModes.StatusChange)
                    {
                        await Task.Run(() => GetBatteryStatus());
                        await Task.Run(() => PremadePresets.SetPremadePresets());

                        if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                        {
                            if (Settings.Default.acCommandString != null && Settings.Default.acCommandString != "" && Settings.Default.acPreset != "None")
                            {
                                if (Settings.Default.acPreset.Contains("PM - Eco"))
                                {
                                    Settings.Default.premadePreset = 0;
                                    Settings.Default.acCommandString = PremadePresets.EcoPreset;
                                }
                                else if (Settings.Default.acPreset.Contains("PM - Bal"))
                                {
                                    Settings.Default.premadePreset = 1;
                                    Settings.Default.acCommandString = PremadePresets.BalPreset;
                                }
                                else if (Settings.Default.acPreset.Contains("PM - Perf"))
                                {
                                    Settings.Default.premadePreset = 2;
                                    Settings.Default.acCommandString = PremadePresets.PerformancePreset;
                                }
                                else if (Settings.Default.acPreset.Contains("PM - Ext"))
                                {
                                    Settings.Default.premadePreset = 3;
                                    Settings.Default.acCommandString = PremadePresets.ExtremePreset;
                                }

                                Settings.Default.CommandString = Settings.Default.acCommandString;
                                Settings.Default.Save();
                                await RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.acCommandString);

                                if (lastAppliedState != "ac") ToastNotification.ShowToastNotification("Charge Preset Applied!", $"Your charge preset settings have been applied!");
                                lastAppliedState = "ac";
                            }
                        }
                        else
                        {
                            if (Settings.Default.dcCommandString != null && Settings.Default.dcCommandString != "" && Settings.Default.dcPreset != "None")
                            {
                                if (Settings.Default.dcPreset.Contains("PM - Eco"))
                                {
                                    Settings.Default.premadePreset = 0;
                                    Settings.Default.dcCommandString = PremadePresets.EcoPreset;
                                }
                                else if (Settings.Default.dcPreset.Contains("PM - Bal"))
                                {
                                    Settings.Default.premadePreset = 1;
                                    Settings.Default.dcCommandString = PremadePresets.BalPreset;
                                }
                                else if (Settings.Default.dcPreset.Contains("PM - Perf"))
                                {
                                    Settings.Default.premadePreset = 2;
                                    Settings.Default.dcCommandString = PremadePresets.PerformancePreset;
                                }
                                else if (Settings.Default.dcPreset.Contains("PM - Ext"))
                                {
                                    Settings.Default.premadePreset = 3;
                                    Settings.Default.dcCommandString = PremadePresets.ExtremePreset;
                                }
                                Settings.Default.CommandString = Settings.Default.dcCommandString;
                                Settings.Default.Save();
                                await RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.dcCommandString);

                                if (lastAppliedState != "dc") ToastNotification.ShowToastNotification("Discharge Preset Applied!", $"Your discharge preset settings have been applied!");
                                lastAppliedState = "dc";
                            }
                        }
                    }

                    if (e.Mode == PowerModes.Resume)
                    {
                        if (Settings.Default.resumeCommandString != null && Settings.Default.resumeCommandString != "" && Settings.Default.resumePreset != "None")
                        {
                            if (Settings.Default.resumePreset.Contains("PM - Eco"))
                            {
                                Settings.Default.premadePreset = 0;
                                Settings.Default.resumeCommandString = PremadePresets.EcoPreset;
                            }
                            else if (Settings.Default.resumePreset.Contains("PM - Bal"))
                            {
                                Settings.Default.premadePreset = 1;
                                Settings.Default.resumeCommandString = PremadePresets.BalPreset;
                            }
                            else if (Settings.Default.resumePreset.Contains("PM - Perf"))
                            {
                                Settings.Default.premadePreset = 2;
                                Settings.Default.resumeCommandString = PremadePresets.PerformancePreset;
                            }
                            else if (Settings.Default.resumePreset.Contains("PM - Ext"))
                            {
                                Settings.Default.premadePreset = 3;
                                Settings.Default.resumeCommandString = PremadePresets.ExtremePreset;
                            }
                            Settings.Default.CommandString = Settings.Default.resumeCommandString;
                            Settings.Default.Save();
                            _ = RyzenAdj_To_UXTU.TranslateAsync(Settings.Default.resumeCommandString);

                            if (lastAppliedState != "resume") ToastNotification.ShowToastNotification("Resume Preset Applied!", $"Your resume preset settings have been applied!");
                            lastAppliedState = "resume";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed during power mode change handling");
            }
        }

        #region INavigationWindow Methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider pageProvider) => RootNavigation.SetPageProviderService(pageProvider);

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(INavigationViewPageProvider)) is INavigationViewPageProvider pageProvider)
                SetPageService(pageProvider);
        }

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void UiWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                isMini = true;
                this.WindowStyle = WindowStyle.ToolWindow;
                this.ShowInTaskbar = false;
            }
            else
            {
                isMini = false;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ShowInTaskbar = true;
            }

        }

        private void NotifyIcon_LeftClick(Wpf.Ui.Tray.Controls.NotifyIcon sender, RoutedEventArgs e)
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

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.isAdaptiveModeRunning = false;
            Settings.Default.Save();
            Controller.magWindow?.Dispose();
            Application.Current.Shutdown();
        }

        private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Settings.Default.MinimizeClose)
            {
                WindowState = WindowState.Minimized;
                e.Cancel = true;
                return;
            }
            
            Settings.Default.isAdaptiveModeRunning = false;
            Settings.Default.Save();
            Controller.magWindow?.Dispose();
            //Fan_Control.disableFanControl();
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new Action(() => Controller.SetUpMagWindow(this)));

            if (Settings.Default.StartMini == true)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                if (GetSystemInfo.Manufacturer.ToUpper().Contains("AYANEO") || GetSystemInfo.Manufacturer.ToUpper().Contains("GPD") || GetSystemInfo.Product.ToUpper().Contains("ONEXPLAYER"))
                {
                    int displayCount = Screen.AllScreens.Length;
                    if (displayCount < 2)
                    {
                        this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                        this.WindowState = WindowState.Maximized;
                    }
                }
            }

            _ = Task.Run(PremadePresets.SetPremadePresets);
        }
    }
}
