using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Services;

public interface IGameLibraryService
{
    Task<IReadOnlyList<GameLibraryEntry>> LoadAsync();
    GameLibraryEntry AddExecutable(string path);
    void RefreshStatistics(IEnumerable<GameLibraryEntry> games);
    Task LaunchAsync(GameLibraryEntry game);
}

public sealed class GameLibraryService : IGameLibraryService, IDisposable
{
    private readonly List<GameLibraryEntry> _manualGames = new();
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private string? _iconsDirectory;
    private static GameDataManager CreateManager() => new(Settings.Default.Path + "gameData.json");

    public async Task<IReadOnlyList<GameLibraryEntry>> LoadAsync()
    {
        await _loadGate.WaitAsync();
        try
        {
            var installed = await Task.Run(() => Game_Manager.syncGame_Library());
            Game_Manager.installedGames = installed;
            var games = new List<GameLibraryEntry>();
            var manager = CreateManager();
            foreach (var game in installed ?? new())
            {
                if (manager.GetPreset(game.gameName) == null)
                    manager.SavePreset(game.gameName, new GameData { fpsData = "No Data" });
                games.Add(new GameLibraryEntry
                {
                    Id = game.gameID ?? "",
                    Name = game.gameName,
                    Launcher = game.appType,
                    Path = game.path ?? "",
                    Executable = game.exe ?? "",
                    LaunchCommand = game.launchCommand,
                    IconPath = await GetImages.GetIconImageUrl(game.gameName)
                });
            }
            games.AddRange(_manualGames);
            RefreshStatistics(games);
            return games.OrderBy(g => g.Name).ToList();
        }
        finally { _loadGate.Release(); }
    }

    public GameLibraryEntry AddExecutable(string path)
    {
        _iconsDirectory ??= Directory.CreateTempSubdirectory("uxtu-game-icons-").FullName;
        var iconPath = System.IO.Path.Combine(_iconsDirectory, Guid.NewGuid() + ".ico");
        using (var icon = Icon.ExtractAssociatedIcon(path))
        {
            if (icon != null)
            {
                using var stream = File.Create(iconPath);
                icon.Save(stream);
            }
        }
        var entry = new GameLibraryEntry
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(path),
            Launcher = "Manually added game",
            Path = System.IO.Path.GetDirectoryName(path) ?? "",
            Executable = path,
            IconPath = iconPath
        };
        _manualGames.Add(entry);
        var manager = CreateManager();
        if (manager.GetPreset(entry.Name) == null) manager.SavePreset(entry.Name, new GameData { fpsData = "No Data" });
        return entry;
    }

    public void RefreshStatistics(IEnumerable<GameLibraryEntry> games)
    {
        var manager = CreateManager();
        foreach (var game in games)
        {
            var data = manager.GetPreset(game.Name);
            game.FramesPerSecond = string.IsNullOrEmpty(data?.fpsData) || data.fpsData == "No Data" ? "No Data" : data.fpsData + " FPS";
            game.FrameTime = string.IsNullOrEmpty(data?.msData) || data.msData == "No Data" ? "No Data" : data.msData + " ms";
        }
    }

    public Task LaunchAsync(GameLibraryEntry game) => Task.Run(() =>
    {
        if (game.Launcher == "Manually added game")
            Game_Manager.LaunchApp("0", "Exe", game.Executable, game.Executable);
        else
        {
            var command = game.LaunchCommand;
            var prefix = game.Launcher + "-";
            var suffix = "-" + game.Id + "-" + game.Name;
            if (command.StartsWith(prefix) && command.EndsWith(suffix))
                command = command.Substring(prefix.Length, command.Length - prefix.Length - suffix.Length);
            Game_Manager.LaunchApp(game.Id, game.Launcher, command, command);
        }
    });

    public void Dispose()
    {
        try
        {
            if (_iconsDirectory != null && Directory.Exists(_iconsDirectory)) Directory.Delete(_iconsDirectory, true);
        }
        catch (IOException error) { Serilog.Log.Warning(error, "Could not remove temporary game icons"); }
        catch (UnauthorizedAccessException error) { Serilog.Log.Warning(error, "Could not remove temporary game icons"); }
    }
}
