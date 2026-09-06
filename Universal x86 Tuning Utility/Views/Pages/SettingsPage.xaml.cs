using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Logging;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Wpf.Ui.Abstractions.Controls;
using System.Diagnostics.Eventing.Reader;
using Universal_x86_Tuning_Utility.Services;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    public partial class SettingsPage : INavigableView<ViewModels.SettingsViewModel>
    {
        private readonly ILogger<SettingsPage> _logger;
        private bool _languageSelectionReady;
        private UpdateManager? _updateManager;
        private bool _updateBusy;
        private int _updateCheckGeneration;
        private string _updateMessage = string.Empty;

        public ViewModels.SettingsViewModel ViewModel
        {
            get;
        }

        public SettingsPage(ViewModels.SettingsViewModel viewModel, ILogger<SettingsPage> logger)
        {
            ViewModel = viewModel;
            _logger = logger;

            InitializeComponent();
            Unloaded += SettingsPage_Unloaded;

            cbxLanguage.ItemsSource = LocalizationService.SupportedLanguages;
            cbxLanguage.SelectedItem = LocalizationService.SupportedLanguages.First(language => language.CultureName == LocalizationService.CurrentCultureName);
            _languageSelectionReady = true;

            cbStartBoot.IsChecked = Settings.Default.StartOnBoot;
            cbStartMini.IsChecked = Settings.Default.StartMini;
            cbMinimizeClose.IsChecked = Settings.Default.MinimizeClose;
            cbApplyStart.IsChecked = Settings.Default.ApplyOnStart;
            cbAutoReapply.IsChecked = Settings.Default.AutoReapply;
            nudAutoReapply.Value = Settings.Default.AutoReapplyTime;
            nudAutoReapply.Text = Convert.ToString(Settings.Default.AutoReapplyTime);
            cbAutoCheck.IsChecked = Settings.Default.UpdateCheck;
            cbPreReleases.IsChecked = Settings.Default.IncludePreReleases;
            cbAdaptive.IsChecked = Settings.Default.isStartAdpative;
            cbTrack.IsChecked = Settings.Default.isTrack;

            cbxLogLevel.SelectedIndex = Settings.Default.DiagnosticLogLevel;

            tbAppVerion.Text = $"Universal x86 Tuning Utility - {App.version}";

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

        private void cbMinimizeClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.MinimizeClose = (bool)cbMinimizeClose.IsChecked;
            Settings.Default.Save();
        }

        private void cbAutoReapply_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.AutoReapply = (bool)cbAutoReapply.IsChecked;
            Settings.Default.AutoReapplyTime = (int)nudAutoReapply.Value;
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

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            checkUpdate(true);
        }

        private static UpdateManager CreateUpdateManager() => new(
            "JamesCJ60",
            "Universal-x86-Tuning-Utility",
            App.version,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UXTU", "Updates"),
            Settings.Default.IncludePreReleases);

        private async void checkUpdate(bool isUserCheck = false)
        {
            if (_updateBusy)
                return;

            var generation = ++_updateCheckGeneration;
            _updateManager = null;
            btnDownload.Visibility = Visibility.Collapsed;
            btnCheck.IsEnabled = false;
            SetUpdateMessage(string.Empty);

            try
            {
                var manager = CreateUpdateManager();
                var available = await manager.IsUpdateAvailable();
                if (generation != _updateCheckGeneration)
                    return;

                _updateManager = available ? manager : null;
                btnDownload.Visibility = available ? Visibility.Visible : Visibility.Collapsed;
                if (available)
                    SetUpdateMessage("An update for Universal x86 Tuning Utility has been found!");
                else if (isUserCheck)
                    SetUpdateMessage("Universal x86 Tuning Utility is up to date!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check GitHub for updates");
                if (generation == _updateCheckGeneration)
                    SetUpdateMessage("Unable to check for updates. Please try again later.");
            }
            finally
            {
                if (generation == _updateCheckGeneration)
                    btnCheck.IsEnabled = true;
            }
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (_updateBusy || _updateManager == null)
                return;

            _updateBusy = true;
            btnCheck.IsEnabled = false;
            btnDownload.IsEnabled = false;
            cbPreReleases.IsEnabled = false;
            SetUpdateMessage("Universal x86 Tuning Utility will close and the installer will open when the download is complete");

            try
            {
                if (await _updateManager.DownloadAndInstallUpdate())
                    System.Windows.Application.Current.Shutdown();
                else
                {
                    btnDownload.Visibility = Visibility.Collapsed;
                    SetUpdateMessage("Universal x86 Tuning Utility is up to date!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download or launch the update installer");
                SetUpdateMessage("The update could not be downloaded or started. Please try again.");
            }
            finally
            {
                _updateBusy = false;
                btnCheck.IsEnabled = true;
                btnDownload.IsEnabled = true;
                cbPreReleases.IsEnabled = true;
            }
        }

        private void cbPreReleases_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.IncludePreReleases = cbPreReleases.IsChecked == true;
            Settings.Default.Save();
            checkUpdate(true);
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
            LocalizationService.CultureChanged -= UpdateStatusLanguage;
            LocalizationService.CultureChanged += UpdateStatusLanguage;
            UpdateStatusLanguage(this, EventArgs.Empty);
        }

        private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            LocalizationService.CultureChanged -= UpdateStatusLanguage;
        }

        private void SetUpdateMessage(string message)
        {
            _updateMessage = message;
            UpdateStatusLanguage(this, EventArgs.Empty);
        }

        private void UpdateStatusLanguage(object? sender, EventArgs e)
        {
            ViewModel.UpdateStatus = LocalizationService.Get(_updateMessage);
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

        private void cbxLogLevel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxLogLevel == null)
            {
                return;
            }

            Settings.Default.DiagnosticLogLevel = cbxLogLevel.SelectedIndex;
            Settings.Default.Save();
            DiagnosticLogger.ApplySettingsLevel();
        }

        private void cbxLanguage_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_languageSelectionReady || cbxLanguage.SelectedItem is not LanguageOption language)
            {
                return;
            }

            Settings.Default.Language = language.CultureName;
            Settings.Default.Save();
            LocalizationService.SetCulture(language.CultureName);
        }

        private void nudAutoReapply_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (nudAutoReapply != null && nudAutoReapply.Value != null)
            {
                Settings.Default.AutoReapplyTime = (int)nudAutoReapply.Value;
                Settings.Default.Save();
            }
        }

        private async void btnBackupPresets_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".uxtupresets",
                FileName = $"UXTU-Presets-{DateTime.Now:yyyy-MM-dd}.uxtupresets",
                Filter = $"{LocalizationService.Get("UXTU preset backup")} (*.uxtupresets)|*.uxtupresets"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            SetPresetBackupBusy(true);
            try
            {
                var result = await PresetBackupService.ExportAsync(Settings.Default.Path, dialog.FileName);
                ShowPresetBackupStatus(
                    LocalizationService.Get("Preset backup saved"),
                    LocalizationService.Format("Backed up {0} custom presets and {1} adaptive mode presets.", result.CustomPresetCount, result.AdaptivePresetCount),
                    Wpf.Ui.Controls.InfoBarSeverity.Success);
            }
            catch (Exception exception)
            {
                DiagnosticLogger.LogError(exception, "Failed to back up presets");
                ShowPresetBackupStatus(
                    LocalizationService.Get("Preset backup failed"),
                    LocalizationService.Format("The presets could not be backed up.\n\n{0}", exception.Message),
                    Wpf.Ui.Controls.InfoBarSeverity.Error);
            }
            finally
            {
                SetPresetBackupBusy(false);
            }
        }

        private async void btnImportPresets_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = ".uxtupresets",
                Filter = $"{LocalizationService.Get("UXTU preset backup")} (*.uxtupresets;*.json)|*.uxtupresets;*.json"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            SetPresetBackupBusy(true);
            try
            {
                var result = await PresetBackupService.ImportAsync(Settings.Default.Path, dialog.FileName);
                ShowPresetBackupStatus(
                    LocalizationService.Get("Preset import complete"),
                    LocalizationService.Format("Imported {0} custom presets and {1} adaptive mode presets.", result.CustomPresetCount, result.AdaptivePresetCount),
                    Wpf.Ui.Controls.InfoBarSeverity.Success);
            }
            catch (InvalidDataException)
            {
                ShowPresetBackupStatus(
                    LocalizationService.Get("Preset import failed"),
                    LocalizationService.Get("The selected file is not a valid UXTU preset backup."),
                    Wpf.Ui.Controls.InfoBarSeverity.Error);
            }
            catch (Exception exception)
            {
                DiagnosticLogger.LogError(exception, "Failed to import presets");
                ShowPresetBackupStatus(
                    LocalizationService.Get("Preset import failed"),
                    LocalizationService.Format("The presets could not be imported.\n\n{0}", exception.Message),
                    Wpf.Ui.Controls.InfoBarSeverity.Error);
            }
            finally
            {
                SetPresetBackupBusy(false);
            }
        }

        private void SetPresetBackupBusy(bool isBusy)
        {
            btnBackupPresets.IsEnabled = !isBusy;
            btnImportPresets.IsEnabled = !isBusy;
        }

        private void ShowPresetBackupStatus(string title, string message, Wpf.Ui.Controls.InfoBarSeverity severity)
        {
            PresetBackupStatus.Title = title;
            PresetBackupStatus.Message = message;
            PresetBackupStatus.Severity = severity;
            PresetBackupStatus.IsOpen = true;
        }
    }
}
