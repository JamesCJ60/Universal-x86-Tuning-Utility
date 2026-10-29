using System.IO;
using System.Linq;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Universal_x86_Tuning_Utility.ViewModels;
using Wpf.Ui.Abstractions.Controls;

namespace Universal_x86_Tuning_Utility.Views.Pages;

public partial class SettingsPage : INavigableView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }
    private bool _languageSelectionReady;

    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        cbxLanguage.ItemsSource = LocalizationService.SupportedLanguages;
        cbxLanguage.SelectedItem = LocalizationService.SupportedLanguages.First(language => language.CultureName == LocalizationService.CurrentCultureName);
        cbxLogLevel.SelectedIndex = Settings.Default.DiagnosticLogLevel;
        cbPreReleases.IsChecked = Settings.Default.IncludePreReleases;
        tbAppVerion.Text = $"Universal x86 Tuning Utility - {App.version}";
        _languageSelectionReady = true;
        Loaded += async (_, _) => await ViewModel.ActivateAsync();
        Unloaded += (_, _) => ViewModel.Deactivate();
    }

        private void cbPreReleases_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.IncludePreReleases = cbPreReleases.IsChecked == true;
            Settings.Default.Save();
            ViewModel.CheckUpdateCommand.Execute(null);
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
