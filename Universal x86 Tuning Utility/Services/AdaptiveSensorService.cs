using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.GPUs.AMD;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Services;

public sealed record AdaptiveReadings(int CpuTemperature, int CpuLoad, int CpuClock, int CpuPower, int GpuLoad, int GpuClock, int GpuMemoryClock);

public interface IAdaptiveSensorService
{
    Task OpenAsync();
    void Close();
    Task<AdaptiveReadings> ReadAsync();
    Task<string> FindRunningGameAsync();
}

public sealed class AdaptiveSensorService : IAdaptiveSensorService
{
    private readonly IGraphicsHardwareService _graphics;
    private int _coreCount = 1;
    public AdaptiveSensorService(IGraphicsHardwareService graphics) => _graphics = graphics;

    public Task OpenAsync() => Task.Run(() =>
    {
        using var query = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor");
        using var cpus = query.Get();
        _coreCount = Math.Max(1, cpus.Cast<ManagementObject>().Sum(cpu => Convert.ToInt32(cpu["NumberOfCores"])));
        GetSensor.OpenSensor();
    });

    public void Close() => GetSensor.CloseSensor();

    public Task<AdaptiveReadings> ReadAsync() => Task.Run(() =>
    {
        var temperature = (int)GetSensor.GetCPUInfo(SensorType.Temperature, Family.TYPE == Family.ProcessorType.Intel ? "Package" : "Core");
        var load = (int)GetSensor.GetCPUInfo(SensorType.Load, "Total");
        var clock = Enumerable.Range(1, _coreCount).Sum(core => (int)GetSensor.GetCPUInfo(SensorType.Clock, $"Core #{core}")) / _coreCount;
        var hasRadeon = _graphics.CountRadeonGpus() > 0;
        return new AdaptiveReadings(temperature, load, clock, (int)GetSensor.GetCPUInfo(SensorType.Power, "Package"),
            hasRadeon ? ADLXBackend.GetGPUMetrics(0, 7) : 0,
            hasRadeon ? ADLXBackend.GetGPUMetrics(0, 0) : 0,
            hasRadeon ? ADLXBackend.GetGPUMetrics(0, 1) : 0);
    });

    public Task<string> FindRunningGameAsync() => Task.Run(() =>
    {
        var games = Game_Manager.installedGames?.ToArray() ?? Array.Empty<Game_Manager.GameLauncherItem>();
        var presets = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
        var processes = Process.GetProcesses();
        try
        {
            foreach (var game in games)
            {
                if (presets.GetPreset(game.gameName)?.isAutoSwitch == false) continue;
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.MainModule?.FileName is string executablePath && MatchesExecutable(game, executablePath)) return game.gameName;
                    }
                    catch (System.ComponentModel.Win32Exception) { }
                    catch (InvalidOperationException) { }
                }
            }
            return "Default";
        }
        finally { foreach (var process in processes) process.Dispose(); }
    });

        private static bool MatchesExecutable(Game_Manager.GameLauncherItem item, string executablePath)
        {
            if (!string.IsNullOrWhiteSpace(item.exe))
            {
                if (System.IO.Path.IsPathFullyQualified(item.exe))
                    return string.Equals(System.IO.Path.GetFullPath(item.exe), System.IO.Path.GetFullPath(executablePath), StringComparison.OrdinalIgnoreCase);

                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(item.exe), System.IO.Path.GetFileNameWithoutExtension(executablePath), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (string.IsNullOrWhiteSpace(item.path))
                return false;

            if (!System.IO.Path.IsPathFullyQualified(item.path))
                return string.Equals(System.IO.Path.GetFileNameWithoutExtension(item.path), System.IO.Path.GetFileNameWithoutExtension(executablePath), StringComparison.OrdinalIgnoreCase);

            string gamePath = System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(item.path));
            string processPath = System.IO.Path.GetFullPath(executablePath);
            return processPath.StartsWith(gamePath + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(processPath, gamePath, StringComparison.OrdinalIgnoreCase);
        }
}
