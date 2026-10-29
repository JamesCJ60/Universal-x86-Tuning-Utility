using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RTSSSharedMemoryNET;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Services;

public interface IGamePerformanceService { Task UpdateAsync(); }

public sealed class GamePerformanceService : IGamePerformanceService
{
    private List<Game_Manager.GameLauncherItem>? _games;

    public async Task UpdateAsync()
    {
        if (!Settings.Default.isTrack || !File.Exists(Settings.Default.Path + "gameData.json")) return;
        if (!RTSS.RTSSRunning()) { RTSS.startRTSS(); return; }
        _games ??= await Task.Run(() => Game_Manager.syncGame_Library());
        if (_games == null) return;
        var entries = OSD.GetAppEntries().Where(x => (x.Flags & AppFlags.MASK) != AppFlags.None).ToArray();
        var manager = new GameDataManager(Settings.Default.Path + "gameData.json");
        foreach (var game in _games)
            foreach (var entry in entries)
            {
                if (!Matches(game, entry.Name)) continue;
                var data = manager.GetPreset(game.gameName) ?? new GameData();
                UpdateStatistics(data, entry.InstantaneousFrames, entry.InstantaneousFrameTime);
                manager.SavePreset(game.gameName, data);
            }
    }

    private static bool Matches(Game_Manager.GameLauncherItem game, string process) =>
        (!string.IsNullOrWhiteSpace(game.path) && process.Contains(game.path, StringComparison.OrdinalIgnoreCase)) ||
        (!string.IsNullOrWhiteSpace(game.exe) && process.Contains(game.exe, StringComparison.OrdinalIgnoreCase)) ||
        (!string.IsNullOrWhiteSpace(game.gameName) && process.Contains(GetImages.CleanFileName(game.gameName), StringComparison.OrdinalIgnoreCase));

    public static void UpdateStatistics(GameData data, uint frames, TimeSpan frameTime)
    {
        var fps = (data.fpsAvData ?? "").Split(',').Select(s => uint.TryParse(s, out var n) ? (uint?)n : null)
            .Where(n => n.HasValue).Select(n => n!.Value).Append(frames).TakeLast(100).ToArray();
        var times = (data.msAvData ?? "").Split(',').Select(s => TimeSpan.TryParse(s, out var t) ? (TimeSpan?)t : null)
            .Where(t => t.HasValue).Select(t => t!.Value).Append(frameTime).TakeLast(100).ToArray();
        data.fpsData = ((uint)fps.Average(x => (double)x)).ToString();
        data.fpsAvData = string.Join(",", fps);
        data.msData = TimeSpan.FromTicks((long)times.Average(x => (double)x.Ticks)).TotalMilliseconds.ToString("0.##");
        data.msAvData = string.Join(",", times);
    }
}
