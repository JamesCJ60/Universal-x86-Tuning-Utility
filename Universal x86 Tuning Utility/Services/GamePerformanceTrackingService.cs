using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services.Performance;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;

namespace Universal_x86_Tuning_Utility.Services
{
    internal sealed class GamePerformanceTrackingService : BackgroundService
    {
        private const int SampleLimit = 120;
        private readonly IFpsMonitoringService _fps;

        public GamePerformanceTrackingService(IFpsMonitoringService fps) => _fps = fps;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IDisposable? lease = null;
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    if (!Settings.Default.isTrack)
                    {
                        lease?.Dispose();
                        lease = null;
                        continue;
                    }

                    lease ??= _fps.Acquire();
                    FpsMetricsSnapshot snapshot = _fps.Latest;
                    if (!snapshot.IsAvailable || snapshot.Fps <= 0)
                        continue;

                    GameLauncherItem? game = FindGame(snapshot);
                    if (game is null)
                        continue;

                    UpdateGameData(game, snapshot);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            finally
            {
                lease?.Dispose();
            }
        }

        private static GameLauncherItem? FindGame(FpsMetricsSnapshot snapshot)
        {
            IReadOnlyList<GameLauncherItem>? games = Game_Manager.installedGames;
            if (games is null)
                return null;

            string executableName = Path.GetFileName(snapshot.ExecutablePath);
            foreach (GameLauncherItem game in games)
            {
                if (!string.IsNullOrWhiteSpace(game.exe) && executableName.Contains(game.exe, StringComparison.OrdinalIgnoreCase))
                    return game;
                if (!string.IsNullOrWhiteSpace(game.path) && snapshot.ExecutablePath.Contains(game.path, StringComparison.OrdinalIgnoreCase))
                    return game;
                if (!string.IsNullOrWhiteSpace(game.gameName) && snapshot.ProcessName.Contains(GetImages.CleanFileName(game.gameName), StringComparison.OrdinalIgnoreCase))
                    return game;
            }

            return null;
        }

        private static void UpdateGameData(GameLauncherItem game, FpsMetricsSnapshot snapshot)
        {
            string path = Path.Combine(Settings.Default.Path, "gameData.json");
            var manager = new GameDataManager(path);
            GameData data = manager.GetPreset(game.gameName) ?? new GameData();
            List<double> fpsValues = AppendSample(data.fpsAvData, snapshot.Fps);
            List<double> frameTimes = AppendSample(data.msAvData, snapshot.FrameTimeMs);
            data.fpsData = fpsValues.Average().ToString("0", CultureInfo.InvariantCulture);
            data.fpsAvData = string.Join(",", fpsValues.Select(value => value.ToString("0", CultureInfo.InvariantCulture)));
            data.msData = frameTimes.Average().ToString("0.##", CultureInfo.InvariantCulture);
            data.msAvData = string.Join(",", frameTimes.Select(value => value.ToString("0.##", CultureInfo.InvariantCulture)));
            manager.SavePreset(game.gameName, data);
        }

        private static List<double> AppendSample(string? serialized, double sample)
        {
            var values = new List<double>(SampleLimit);
            if (!string.IsNullOrWhiteSpace(serialized))
            {
                foreach (string value in serialized.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                        values.Add(parsed);
                }
            }

            values.Add(sample);
            if (values.Count > SampleLimit)
                values.RemoveRange(0, values.Count - SampleLimit);
            return values;
        }
    }
}
