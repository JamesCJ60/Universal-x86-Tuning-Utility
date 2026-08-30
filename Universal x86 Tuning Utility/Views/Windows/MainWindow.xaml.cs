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
using System.Runtime.InteropServices;
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
        private bool _isTrayPresetApplying;
        private bool _startupApplyPending = true;
        private bool _automaticTuningSuppressed;
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
            miPremadePresets.Visibility = Visibility.Visible;
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this, WindowBackdropType.Mica, true);
        }

        private async void ApplyOnStart()
        {
            try
            {
                if (!Settings.Default.ApplyOnStart || string.IsNullOrWhiteSpace(Settings.Default.CommandString))
                    return;

                await Task.Run(GetBatteryStatus);

                bool isCharging = statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8;
                string commands = Settings.Default.CommandString;
                string? preset = null;

                if (isCharging && !string.IsNullOrWhiteSpace(Settings.Default.acCommandString))
                {
                    commands = Settings.Default.acCommandString;
                    preset = Settings.Default.acPreset;
                }
                else if (!isCharging && !string.IsNullOrWhiteSpace(Settings.Default.dcCommandString))
                {
                    commands = Settings.Default.dcCommandString;
                    preset = Settings.Default.dcPreset;
                }

                if (ContainsRiskyUndervolt(commands))
                {
                    ToastNotification.ShowToastNotification(
                        LocalizationService.Get("Start-up tuning delayed"),
                        LocalizationService.Get("Hold Shift within 10 seconds to skip saved Curve Optimiser and Intel undervolt settings for this session."));

                    for (int elapsed = 0; elapsed < 100; elapsed++)
                    {
                        if (IsShiftPressed())
                        {
                            _automaticTuningSuppressed = true;
                            ToastNotification.ShowToastNotification(
                                LocalizationService.Get("Automatic tuning skipped"),
                                LocalizationService.Get("Saved Curve Optimiser and Intel undervolt settings will not be applied automatically during this session."));
                            return;
                        }

                        await Task.Delay(100);
                    }
                }

                Settings.Default.CommandString = commands;
                Settings.Default.Save();

                if (!string.IsNullOrWhiteSpace(preset))
                {
                    await TranslatePresetAsync(commands, preset);
                    ToastNotification.ShowToastNotification(
                        isCharging ? "Charge Preset Applied!" : "Discharge Preset Applied!",
                        isCharging ? "Your charge preset settings have been applied!" : "Your discharge preset settings have been applied!");
                }
                else
                {
                    await RyzenAdj_To_UXTU.TranslateAsync(commands);
                    ToastNotification.ShowToastNotification("Settings Reapplied!", "Your last applied settings have been reapplied!");
                }
            }
            finally
            {
                _startupApplyPending = false;
            }
        }

        private static bool ContainsRiskyUndervolt(string commands) =>
            commands.Contains("--set-coall=", StringComparison.OrdinalIgnoreCase) ||
            commands.Contains("--set-coper=", StringComparison.OrdinalIgnoreCase) ||
            commands.Contains("--set-cogfx=", StringComparison.OrdinalIgnoreCase) ||
            commands.Contains("--intel-volt-", StringComparison.OrdinalIgnoreCase);

        private static bool IsShiftPressed() => (GetAsyncKeyState(0x10) & 0x8000) != 0;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);


        InstabilityMonitor monitor = null;

        AdaptiveUndervoltController cpuController = null;
        AdaptiveUndervoltController iGpuController = null;

        int lastCPUUVOffset = 0;
        int lastiGPUUVOffset = 0;
        private int miscTickRunning;

        private async void Misc_Tick(object sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref miscTickRunning, 1) != 0)
                return;

            try
            {
                try
                {
                    await ProcessCpuUndervoltAsync();
                }
                catch (Exception ex)
                {
                    DiagnosticLogger.LogError(ex, "Failed during adaptive CPU undervolt tick");
                }

                try
                {
                    await ProcessIgpuUndervoltAsync();
                }
                catch (Exception ex)
                {
                    DiagnosticLogger.LogError(ex, "Failed during adaptive iGPU undervolt tick");
                }

                if (!Settings.Default.isAutoUvCPU &&
                    !Settings.Default.isAutoUviGPU &&
                    cpuController == null &&
                    iGpuController == null &&
                    monitor != null)
                {
                    monitor.Dispose();
                    monitor = null;
                }
            }
            finally
            {
                Volatile.Write(ref miscTickRunning, 0);
            }
        }

        private async Task ProcessCpuUndervoltAsync()
        {
            if (!Settings.Default.isAutoUvCPU)
            {
                await DisableCpuUndervoltAsync();
                return;
            }

            monitor ??= new InstabilityMonitor();

            cpuController ??= new AdaptiveUndervoltController(
                monitor,
                minOffset: -50,
                stepSize: 1,
                stableThreshold: 8,
                cooldownThreshold: 4,
                isIgpu: false,
                minimumEvaluationIntervalMilliseconds: 1000,
                idleEntrySamples: 3,
                idleExitSamples: 2,
                idleExitMarginPercent: 2f
            );

            int requestedOffset = cpuController.UpdateOffset();

            if (lastCPUUVOffset != requestedOffset)
            {
                string commandValues = BuildCpuOffsetCommand(requestedOffset);
                await RyzenAdj_To_UXTU.TranslateAsync(commandValues, false, true);
                lastCPUUVOffset = requestedOffset;
            }

            if (cpuController.GetLastAppliedOffset() != requestedOffset)
                cpuController.RecordAppliedOffset(requestedOffset);
        }

        private async Task ProcessIgpuUndervoltAsync()
        {
            if (!Settings.Default.isAutoUviGPU)
            {
                await DisableIgpuUndervoltAsync();
                return;
            }

            monitor ??= new InstabilityMonitor();

            iGpuController ??= new AdaptiveUndervoltController(
                monitor,
                minOffset: -50,
                stepSize: 1,
                stableThreshold: 8,
                cooldownThreshold: 4,
                isIgpu: true,
                minimumEvaluationIntervalMilliseconds: 1000
            );

            int requestedOffset = iGpuController.UpdateOffset();

            if (lastiGPUUVOffset != requestedOffset)
            {
                string commandValues = BuildIgpuOffsetCommand(requestedOffset);
                await RyzenAdj_To_UXTU.TranslateAsync(commandValues, false, true);
                lastiGPUUVOffset = requestedOffset;
            }

            if (iGpuController.GetLastAppliedOffset() != requestedOffset)
                iGpuController.RecordAppliedOffset(requestedOffset);
        }

        private async Task DisableCpuUndervoltAsync()
        {
            int appliedOffset = cpuController?.GetLastAppliedOffset() ?? lastCPUUVOffset;

            if (appliedOffset != 0 || lastCPUUVOffset != 0)
            {
                await RyzenAdj_To_UXTU.TranslateAsync(
                    BuildCpuOffsetCommand(0),
                    false,
                    true
                );

                lastCPUUVOffset = 0;
                cpuController?.RecordAppliedOffset(0);
            }

            cpuController?.Dispose();
            cpuController = null;
        }

        private async Task DisableIgpuUndervoltAsync()
        {
            int appliedOffset = iGpuController?.GetLastAppliedOffset() ?? lastiGPUUVOffset;

            if (appliedOffset != 0 || lastiGPUUVOffset != 0)
            {
                await RyzenAdj_To_UXTU.TranslateAsync(
                    BuildIgpuOffsetCommand(0),
                    false,
                    true
                );

                lastiGPUUVOffset = 0;
                iGpuController?.RecordAppliedOffset(0);
            }

            iGpuController?.Dispose();
            iGpuController = null;
        }

        private string BuildCpuOffsetCommand(int offset)
        {
            if (Family.FAM < Family.RyzenFamily.Renoir)
                return $"--set-coper={offset & 0xFFFF} ";

            return $"--set-coall={EncodeCurveOptimiserOffset(offset)} ";
        }

        private static string BuildIgpuOffsetCommand(int offset)
        {
            return $"--set-cogfx={EncodeCurveOptimiserOffset(offset)} ";
        }

        private static uint EncodeCurveOptimiserOffset(int offset)
        {
            if (offset >= 0)
                return (uint)offset;

            long magnitude = -(long)offset;

            if (magnitude > 0x100000)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return 0x100000u - (uint)magnitude;
        }

        private static ushort statuscode;

        public static void GetBatteryStatus()
        {
            try
            {
                using var batteryClass = new ManagementClass("Win32_Battery");
                using var batteries = batteryClass.GetInstances();

                foreach (ManagementObject battery in batteries)
                {
                    using (battery)
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

                if (_startupApplyPending || _automaticTuningSuppressed)
                    return;

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
                if (_automaticTuningSuppressed)
                    return;

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
                                await TranslatePresetAsync(Settings.Default.acCommandString, Settings.Default.acPreset);

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
                                await TranslatePresetAsync(Settings.Default.dcCommandString, Settings.Default.dcPreset);

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
                            _ = TranslatePresetAsync(Settings.Default.resumeCommandString, Settings.Default.resumePreset);

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

        private async void TrayPremadePresets_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            await Task.Run(PremadePresets.SetPremadePresets);
            RefreshTrayPremadePresetChecks();
        }

        private async void TrayPremadePreset_Click(object sender, RoutedEventArgs e)
        {
            if (_isTrayPresetApplying || sender is not System.Windows.Controls.MenuItem { Tag: string presetId })
            {
                return;
            }

            _isTrayPresetApplying = true;
            try
            {
                await Task.Run(PremadePresets.SetPremadePresets);

                var selection = presetId switch
                {
                    "eco" => (0, PremadePresets.EcoPreset, "Eco Preset", "Eco Preset Applied!", "The eco premade power preset has been applied!"),
                    "balanced" => (1, PremadePresets.BalPreset, "Balanced Preset", "Balanced Preset Applied!", "The balanced premade power preset has been applied!"),
                    "performance" => (2, PremadePresets.PerformancePreset, "Performance Preset", "Performance Preset Applied!", "The performance premade power preset has been applied!"),
                    "extreme" => (3, PremadePresets.ExtremePreset, "Extreme Preset", "Extreme Preset Applied!", "The extreme premade power preset has been applied!"),
                    _ => (-1, string.Empty, string.Empty, string.Empty, string.Empty)
                };

                if (selection.Item1 < 0 || string.IsNullOrWhiteSpace(selection.Item2))
                {
                    return;
                }

                await RyzenAdj_To_UXTU.TranslateAsync(selection.Item2, appliedName: selection.Item3, localizeAppliedName: true);
                Settings.Default.CommandString = selection.Item2;
                Settings.Default.premadePreset = selection.Item1;
                Settings.Default.Save();
                ToastNotification.ShowToastNotification(selection.Item4, selection.Item5);
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply a premade preset from the tray menu");
            }
            finally
            {
                RefreshTrayPremadePresetChecks();
                _isTrayPresetApplying = false;
            }
        }

        private void TrayMenu_Opened(object sender, RoutedEventArgs e)
        {
            miCustomPresets.Items.Clear();
            try
            {
                var manager = CreateCustomPresetManager();
                var names = manager.GetPresetNames()
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                    .ToArray();

                if (names.Length == 0)
                {
                    miCustomPresets.Items.Add(new System.Windows.Controls.MenuItem
                    {
                        Header = LocalizationService.Get("No custom presets found"),
                        IsEnabled = false
                    });
                    return;
                }

                foreach (var name in names)
                {
                    var item = new System.Windows.Controls.MenuItem
                    {
                        Header = name,
                        Tag = name
                    };
                    item.Click += TrayCustomPreset_Click;
                    miCustomPresets.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to populate custom presets in the tray menu");
                miCustomPresets.Items.Add(new System.Windows.Controls.MenuItem
                {
                    Header = LocalizationService.Get("No custom presets found"),
                    IsEnabled = false
                });
            }
        }

        private async void TrayCustomPreset_Click(object sender, RoutedEventArgs e)
        {
            if (_isTrayPresetApplying || sender is not System.Windows.Controls.MenuItem { Tag: string presetName })
            {
                return;
            }

            _isTrayPresetApplying = true;
            try
            {
                var preset = CreateCustomPresetManager().GetPreset(presetName);
                if (preset == null)
                {
                    return;
                }

                var command = preset.commandValue ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(command))
                {
                    await RyzenAdj_To_UXTU.TranslateAsync(command, appliedName: presetName);
                    ToastNotification.ShowToastNotification("Preset Applied", "Your custom preset settings have been applied!");
                }

                Settings.Default.cstmPreset = presetName;
                Settings.Default.CommandString = command;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply a custom preset from the tray menu");
            }
            finally
            {
                RefreshTrayPremadePresetChecks();
                _isTrayPresetApplying = false;
            }
        }

        private static PresetManager CreateCustomPresetManager()
        {
            var fileName = Family.TYPE switch
            {
                Family.ProcessorType.Amd_Apu => "apuPresets.json",
                Family.ProcessorType.Amd_Desktop_Cpu => "amdDtCpuPresets.json",
                Family.ProcessorType.Intel => "intelPresets.json",
                _ => "apuPresets.json"
            };

            return new PresetManager(Settings.Default.Path + fileName);
        }

        private static Task TranslatePresetAsync(string commands, string? configuredName)
        {
            var context = configuredName switch
            {
                string value when value.Contains("PM - Eco", StringComparison.Ordinal) => ("Eco Preset", true),
                string value when value.Contains("PM - Bal", StringComparison.Ordinal) => ("Balanced Preset", true),
                string value when value.Contains("PM - Perf", StringComparison.Ordinal) => ("Performance Preset", true),
                string value when value.Contains("PM - Ext", StringComparison.Ordinal) => ("Extreme Preset", true),
                string value when !string.IsNullOrWhiteSpace(value) && !string.Equals(value, "None", StringComparison.OrdinalIgnoreCase) => (value, false),
                _ => ((string?)null, false)
            };

            return RyzenAdj_To_UXTU.TranslateAsync(commands, appliedName: context.Item1, localizeAppliedName: context.Item2);
        }

        private void RefreshTrayPremadePresetChecks()
        {
            var command = Settings.Default.CommandString;
            miTrayEcoPreset.IsChecked = !string.IsNullOrWhiteSpace(PremadePresets.EcoPreset) && string.Equals(command, PremadePresets.EcoPreset, StringComparison.Ordinal);
            miTrayBalancedPreset.IsChecked = !string.IsNullOrWhiteSpace(PremadePresets.BalPreset) && string.Equals(command, PremadePresets.BalPreset, StringComparison.Ordinal);
            miTrayPerformancePreset.IsChecked = !string.IsNullOrWhiteSpace(PremadePresets.PerformancePreset) && string.Equals(command, PremadePresets.PerformancePreset, StringComparison.Ordinal);
            miTrayExtremePreset.IsChecked = !string.IsNullOrWhiteSpace(PremadePresets.ExtremePreset) && string.Equals(command, PremadePresets.ExtremePreset, StringComparison.Ordinal);
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
