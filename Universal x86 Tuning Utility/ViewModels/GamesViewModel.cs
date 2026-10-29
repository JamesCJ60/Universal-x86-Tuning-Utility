using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class GamesViewModel : PageViewModel
{
    private readonly IGameLibraryService _library;
    private readonly IUserInteractionService _interaction;
    private readonly DispatcherTimer _timer;
    public ObservableCollection<GameLibraryEntry> Games { get; } = new();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoadingVisibility), nameof(CanEdit), nameof(EmptyVisibility))]
    private bool _isLoading;
    public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
    public bool CanEdit => !IsLoading;
    public Visibility EmptyVisibility => Games.Count == 0 && !IsLoading ? Visibility.Visible : Visibility.Collapsed;
    public Visibility GamesVisibility => Games.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public GamesViewModel(IGameLibraryService library, IUserInteractionService interaction)
    {
        _library = library;
        Games.CollectionChanged += (_, _) => { OnPropertyChanged(nameof(EmptyVisibility)); OnPropertyChanged(nameof(GamesVisibility)); };
        _interaction = interaction;
        _timer = OwnTimer(new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) });
        _timer.Tick += (_, _) =>
        {
            if (!IsLoading && IsActive && !IsDisposed)
                try { _library.RefreshStatistics(Games); }
                catch (Exception error) { Serilog.Log.Error(error, "Failed to refresh game statistics"); }
        };
    }

    protected override Task InitializeAsync() => ReloadAsync();
    protected override void OnActivated() => _timer.Start();
    protected override void OnDeactivated() => _timer.Stop();

    [RelayCommand]
    private Task ReloadAsync() => RunCommandAsync(async () =>
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            var games = await _library.LoadAsync();
            if (IsDisposed) return;
            Games.Clear();
            foreach (var game in games) Games.Add(game);
        }
        finally { IsLoading = false; }
    });

    [RelayCommand]
    private void AddGame()
    {
        if (IsLoading) return;
        var path = _interaction.SelectGameExecutable();
        if (path == null) return;
        try { Games.Add(_library.AddExecutable(path)); }
        catch (Exception error) { _interaction.ShowError(error.Message); }
    }

    [RelayCommand]
    private Task LaunchAsync(GameLibraryEntry? game) => RunCommandAsync(async () =>
    {
        if (game == null) return;
        await _library.LaunchAsync(game);
        _interaction.Notify($"Launching {game.Name}", "This should only take a few moments!");
    });
}
