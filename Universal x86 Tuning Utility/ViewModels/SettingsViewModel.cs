using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
    private readonly ISettingsActionsService _actions;
    private readonly IUserInteractionService _interaction;
    private bool _loading = true;
    [ObservableProperty] private bool _startOnBoot;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private bool _minimizeOnClose;
    [ObservableProperty] private bool _applyOnStart;
    [ObservableProperty] private bool _autoReapply;
    [ObservableProperty] private double _reapplyInterval = 1;
    [ObservableProperty] private bool _checkUpdates;
    [ObservableProperty] private bool _startAdaptive;
    [ObservableProperty] private bool _trackGames;
    [ObservableProperty] private string _updateMessage = "";
    [ObservableProperty] private Visibility _downloadVisibility = Visibility.Collapsed;
    [ObservableProperty] private Wpf.Ui.Appearance.ApplicationTheme _currentTheme;
    public string AppVersion => $"Universal x86 Tuning Utility - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

    public SettingsViewModel(ISettingsActionsService actions, IUserInteractionService interaction)
    {
        _actions = actions;
        _interaction = interaction;
    }

    protected override async Task InitializeAsync()
    {
        _loading = true;
        StartOnBoot = Settings.Default.StartOnBoot;
        StartMinimized = Settings.Default.StartMini;
        MinimizeOnClose = Settings.Default.MinimizeClose;
        ApplyOnStart = Settings.Default.ApplyOnStart;
        AutoReapply = Settings.Default.AutoReapply;
        ReapplyInterval = Math.Clamp(Settings.Default.AutoReapplyTime, 1, 90);
        CheckUpdates = Settings.Default.UpdateCheck;
        StartAdaptive = Settings.Default.isStartAdpative;
        TrackGames = Settings.Default.isTrack;
        CurrentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
        _loading = false;
        await CheckForUpdatesAsync(false);
    }

    partial void OnStartOnBootChanged(bool value)
    {
        if (_loading) return;
        try { _actions.SetStartOnBoot(value); Save(); }
        catch (Exception error)
        {
            _loading = true;
            StartOnBoot = Settings.Default.StartOnBoot;
            _loading = false;
            _interaction.ShowError(error.Message);
        }
    }
    partial void OnStartMinimizedChanged(bool value) => Save();
    partial void OnMinimizeOnCloseChanged(bool value) => Save();
    partial void OnApplyOnStartChanged(bool value) => Save();
    partial void OnAutoReapplyChanged(bool value) => Save();
    partial void OnReapplyIntervalChanged(double value) => Save();
    partial void OnCheckUpdatesChanged(bool value) => Save();
    partial void OnStartAdaptiveChanged(bool value) => Save();
    partial void OnTrackGamesChanged(bool value) => Save();

    private void Save()
    {
        if (_loading) return;
        Settings.Default.StartOnBoot = StartOnBoot;
        Settings.Default.StartMini = StartMinimized;
        Settings.Default.MinimizeClose = MinimizeOnClose;
        Settings.Default.ApplyOnStart = ApplyOnStart;
        Settings.Default.AutoReapply = AutoReapply;
        Settings.Default.AutoReapplyTime = (int)Math.Clamp(ReapplyInterval, 1, 90);
        Settings.Default.UpdateCheck = CheckUpdates;
        Settings.Default.isStartAdpative = StartAdaptive;
        Settings.Default.isTrack = TrackGames;
        Settings.Default.Save();
    }

    [RelayCommand] private Task CheckUpdateAsync() => CheckForUpdatesAsync(true);
    private Task CheckForUpdatesAsync(bool userCheck) => RunCommandAsync(async () =>
    {
        try
        {
            var version = await _actions.CheckUpdateAsync();
            DownloadVisibility = Visibility.Collapsed;
            if (version != null)
            {
                UpdateMessage = "An update for Universal x86 Tuning Utility has been found!";
                DownloadVisibility = Visibility.Visible;
            }
            else if (userCheck) UpdateMessage = "Universal x86 Tuning Utility is up to date!";
        }
        catch (Exception error)
        {
            if (userCheck) UpdateMessage = error.Message;
            Serilog.Log.Error(error, "Failed to check for updates");
        }
    });

    [RelayCommand]
    private Task DownloadUpdateAsync() => RunCommandAsync(async () =>
    {
        UpdateMessage = "Universal x86 Tuning Utility will close and the installer will open when the download is complete";
        await _actions.DownloadUpdateAsync();
    });
    [RelayCommand] private void StartStressTest() => _actions.StartStressTest();
    [RelayCommand]
    private void ChangeTheme(string parameter)
    {
        CurrentTheme = parameter == "theme_light" ? Wpf.Ui.Appearance.ApplicationTheme.Light : Wpf.Ui.Appearance.ApplicationTheme.Dark;
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(CurrentTheme);
    }
}
