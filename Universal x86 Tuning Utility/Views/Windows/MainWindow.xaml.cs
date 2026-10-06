using Accord.Math.Distances;
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

        private readonly ApplicationRuntimeService _runtime;
        private readonly ApplicationExitService _exit;
        private bool _forceClose, _closing, _shutdownComplete;
        public static bool isMini { get; private set; }
        public static NavigationView _mainWindowNav;
        private static INavigationService _navigationService;
        private bool _isTrayPresetApplying;
        public static bool IsPageSelected(Type pageType) =>
            _mainWindowNav?.SelectedItem is INavigationViewItem item && item.TargetPageType == pageType;

        public MainWindow(ViewModels.MainWindowViewModel viewModel, INavigationViewPageProvider pageProvider, INavigationService navigationService, ApplicationRuntimeService runtime, ApplicationExitService exit)
        {
            _runtime = runtime;
            _exit = exit;
            _exit.ExitRequested += OnExitRequested;
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            _navigationService = navigationService;
            _mainWindowNav = RootNavigation;

            SetupNavigationService(pageProvider);

            SetupUI();
            Loaded += async (_, _) => { try { await _runtime.StartAsync(); } catch (Exception error) { DiagnosticLogger.LogError(error, "Failed to start application runtime"); } };
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

        private void OnExitRequested() { _forceClose = true; Close(); }
        private void miClose_Click(object sender, RoutedEventArgs e) => OnExitRequested();
        private async void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_shutdownComplete) return;
            e.Cancel = true;
            if (!_forceClose && Settings.Default.MinimizeClose) { WindowState = WindowState.Minimized; return; }
            if (_closing) return;
            _closing = true;
            try
            {
                await _runtime.StopAsync();
                await ViewModels.PageViewModel.StopAllAsync();
                Settings.Default.isAdaptiveModeRunning = false;
                Settings.Default.Save();
                Controller.magWindow?.Dispose();
                _exit.ExitRequested -= OnExitRequested;
                _exit.CompleteExit();
            }
            catch (Exception error) { DiagnosticLogger.LogError(error, "Failed during shutdown"); }
            finally { _shutdownComplete = true; Application.Current.Shutdown(); }
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
