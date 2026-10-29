using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using Universal_x86_Tuning_Utility.Properties;
using Task = System.Threading.Tasks.Task;

namespace Universal_x86_Tuning_Utility.Services;

public interface ISettingsActionsService
{
    void SetStartOnBoot(bool enabled);
    void StartStressTest();
    Task<string?> CheckUpdateAsync();
    Task DownloadUpdateAsync();
}

public sealed class SettingsActionsService : ISettingsActionsService
{
    private readonly ApplicationExitService _exit;

    public SettingsActionsService(ApplicationExitService exit) => _exit = exit;

    private static UpdateManager CreateUpdater() => new("JamesCJ60", "Universal-x86-Tuning-Utility", App.version,
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UXTU", "Updates"), Settings.Default.IncludePreReleases);

    public void SetStartOnBoot(bool enabled)
    {
        using var scheduler = new TaskService();
        if (scheduler.RootFolder.AllTasks.Any(t => t.Name == "UXTU")) scheduler.RootFolder.DeleteTask("UXTU");
        if (!enabled) return;
        var definition = scheduler.NewTask();
        definition.Principal.RunLevel = TaskRunLevel.Highest;
        definition.RegistrationInfo.Description = "Start UXTU";
        definition.Settings.DisallowStartIfOnBatteries = false;
        definition.Settings.StopIfGoingOnBatteries = false;
        definition.Settings.DisallowStartOnRemoteAppSession = false;
        definition.Triggers.Add(new LogonTrigger());
        definition.Actions.Add(Path.ChangeExtension(System.Reflection.Assembly.GetEntryAssembly()!.Location, ".exe"));
        scheduler.RootFolder.RegisterTaskDefinition("UXTU", definition);
    }

    public void StartStressTest()
    {
        var path = Path.Combine(Settings.Default.Path, "Assets", "Stress-Test", "AVX2 Stress Test.exe");
        if (File.Exists(path)) Process.Start(new ProcessStartInfo(path))?.Dispose();
    }

    public async Task<string?> CheckUpdateAsync()
    {
        using var ping = new Ping();
        try
        {
            if ((await ping.SendPingAsync("8.8.8.8", 2000)).Status != IPStatus.Success)
                throw new IOException("No internet connection!");
        }
        catch (PingException error) { throw new IOException("No internet connection!", error); }
        var updater = CreateUpdater();
        return await updater.IsUpdateAvailable() ? updater.NewVersion : null;
    }

    public async Task DownloadUpdateAsync()
    {
        var updater = CreateUpdater();
        if (!await updater.IsUpdateAvailable()) return;
        if (!await updater.DownloadAndInstallUpdate()) throw new IOException("The update could not be downloaded.");
        _exit.RequestExit(() => { });
    }
}
