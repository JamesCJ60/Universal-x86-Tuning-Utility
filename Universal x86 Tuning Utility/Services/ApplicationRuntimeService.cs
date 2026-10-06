using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Win32;
using AutoOC.Controllers;
using AutoOC.Monitors;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Scripts.UXTU_Super_Resolution;

namespace Universal_x86_Tuning_Utility.Services;
public sealed class ApplicationRuntimeService : IDisposable
{
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    private readonly DispatcherTimer _misc = new();
    private readonly DispatcherTimer autoReapply = new();
    private readonly DispatcherTimer _restore = new();
    private readonly List<Task> _pending = new();
    private bool _started, _stopping, _reapplying;
    private bool _startupApplyPending = true;
    private bool _automaticTuningSuppressed;
    public ApplicationRuntimeService()
    {
        _misc.Interval = _restore.Interval = TimeSpan.FromSeconds(1);
        _misc.Tick += (_, _) => Track(RunMiscAsync());
        autoReapply.Tick += (_, _) => Track(RunReapplyAsync());
        _restore.Tick += Controller.AutoRestore_Tick;
    }
    public Task StartAsync()
    {
        if (_started || _stopping) return Task.CompletedTask;
        _started = true;
        autoReapply.Interval = TimeSpan.FromSeconds(Math.Max(1, Settings.Default.AutoReapplyTime));
        _misc.Start(); autoReapply.Start(); _restore.Start();
        SystemEvents.PowerModeChanged += PowerModeChanged;
        var startup = ApplyOnStartAsync();
        Track(startup);
        return startup;
    }
    private void Track(Task task) { _pending.RemoveAll(t => t.IsCompleted); _pending.Add(task); }
    private async Task RunMiscAsync()
    {
        if (_stopping || _startupApplyPending || _automaticTuningSuppressed || miscTickRunning != 0) return;
        try { await MiscAsync(); }
        finally { Interlocked.Exchange(ref miscTickRunning, 0); }
    }
    private async Task RunReapplyAsync()
    {
        if (_stopping || _reapplying) return;
        _reapplying = true;
        try { await ReapplyAsync(); }
        finally { _reapplying = false; }
    }
    private void PowerModeChanged(object sender, PowerModeChangedEventArgs args)
    {
        if (_stopping || _dispatcher.HasShutdownStarted) return;
        _dispatcher.BeginInvoke(new Action(() => { if (!_stopping) Track(ApplyPowerModeAsync(args)); }));
    }
    public async Task StopAsync()
    {
        Dispose();
        await Task.WhenAll(_pending.ToArray());
        await DisableCpuUndervoltAsync();
        await DisableIgpuUndervoltAsync();
        if ((object?)monitor is IDisposable disposable) disposable.Dispose();
        else monitor?.GetType().GetMethod("Stop", Type.EmptyTypes)?.Invoke(monitor, null);
        monitor = null;
    }
    public void Dispose()
    {
        _stopping = true;
        _misc.Stop(); autoReapply.Stop(); _restore.Stop();
        SystemEvents.PowerModeChanged -= PowerModeChanged;
    }
        private async Task ApplyOnStartAsync()
        {
            try
            {
                if (!Settings.Default.ApplyOnStart || string.IsNullOrWhiteSpace(Settings.Default.CommandString))
                    return;

                await Task.Run(GetBatteryStatus);

                bool isCharging = statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8;
                string commands = Settings.Default.CommandString;
                string? preset = null;

                if (isCharging && !string.IsNullOrWhiteSpace(Settings.Default.acCommandString))
                {
                    commands = Settings.Default.acCommandString;
                    preset = Settings.Default.acPreset;
                }
                else if (!isCharging && !string.IsNullOrWhiteSpace(Settings.Default.dcCommandString))
                {
                    commands = Settings.Default.dcCommandString;
                    preset = Settings.Default.dcPreset;
                }

                if (ContainsRiskyUndervolt(commands))
                {
                    ToastNotification.ShowToastNotification(
                        LocalizationService.Get("Start-up tuning delayed"),
                        LocalizationService.Get("Hold Shift within 10 seconds to skip saved Curve Optimiser and Intel undervolt settings for this session."));

                    for (int elapsed = 0; elapsed < 100; elapsed++)
                    {
                        if (IsShiftPressed())
                        {
                            _automaticTuningSuppressed = true;
                            ToastNotification.ShowToastNotification(
                                LocalizationService.Get("Automatic tuning skipped"),
                                LocalizationService.Get("Saved Curve Optimiser and Intel undervolt settings will not be applied automatically during this session."));
                            return;
                        }

                        if (_stopping) return;
                        await Task.Delay(100);
                    }
                }

                Settings.Default.CommandString = commands;
                Settings.Default.Save();

                if (!string.IsNullOrWhiteSpace(preset))
                {
                    await TranslatePresetAsync(commands, preset);
                    ToastNotification.ShowToastNotification(
                        isCharging ? "Charge Preset Applied!" : "Discharge Preset Applied!",
                        isCharging ? "Your charge preset settings have been applied!" : "Your discharge preset settings have been applied!");
                }
                else
                {
                    await RyzenAdj_To_UXTU.TranslateAsync(commands);
                    ToastNotification.ShowToastNotification("Settings Reapplied!", "Your last applied settings have been reapplied!");
                }
            }
            finally
            {
                _startupApplyPending = false;
            }
        }

        private static bool ContainsRiskyUndervolt(string commands) =>
            commands.Contains("--set-coall=", StringComparison.OrdinalIgnoreCase) ||
            commands.Contains("--set-coper=", StringComparison.OrdinalIgnoreCase) ||
            commands.Contains("--set-cogfx=", StringComparison.OrdinalIgnoreCase) ||
            commands.Contains("--intel-volt-", StringComparison.OrdinalIgnoreCase);

        private static bool IsShiftPressed() => (GetAsyncKeyState(0x10) & 0x8000) != 0;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);


        InstabilityMonitor monitor = null;

        AdaptiveUndervoltController cpuController = null;
        AdaptiveUndervoltController iGpuController = null;

        int lastCPUUVOffset = 0;
        int lastiGPUUVOffset = 0;
        private int miscTickRunning;

        private async Task MiscAsync()
        {
            try
            {
                if (Interlocked.Exchange(ref miscTickRunning, 1) != 0)
                return;


                try
                {
                    await ProcessCpuUndervoltAsync();
                }
                catch (Exception ex)
                {
                    DiagnosticLogger.LogError(ex, "Failed during adaptive CPU undervolt tick");
                }

                try
                {
                    await ProcessIgpuUndervoltAsync();
                }
                catch (Exception ex)
                {
                    DiagnosticLogger.LogError(ex, "Failed during adaptive iGPU undervolt tick");
                }

                if (!Settings.Default.isAutoUvCPU &&
                    !Settings.Default.isAutoUviGPU &&
                    cpuController == null &&
                    iGpuController == null &&
                    monitor != null)
                {
                    var stoppedMonitor = (object)monitor;
                    monitor = null;
                    if (stoppedMonitor is IDisposable disposable)
                        disposable.Dispose();
                    else
                        stoppedMonitor.GetType().GetMethod("Stop", Type.EmptyTypes)?.Invoke(stoppedMonitor, null);
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to release the adaptive undervolt monitor");
            }
        }

        private async Task ProcessCpuUndervoltAsync()
        {
            try
            {
                if (!Settings.Default.isAutoUvCPU)
                {
                    await DisableCpuUndervoltAsync();
                    return;
                }

                monitor ??= new InstabilityMonitor();

                cpuController ??= new AdaptiveUndervoltController(
                    monitor,
                    minOffset: -50,
                    stepSize: 1,
                    stableThreshold: 8,
                    cooldownThreshold: 4,
                    isIgpu: false,
                    minimumEvaluationIntervalMilliseconds: 1000,
                    idleEntrySamples: 3,
                    idleExitSamples: 2,
                    idleExitMarginPercent: 2f
                );

                int requestedOffset = cpuController.UpdateOffset();

                if (lastCPUUVOffset != requestedOffset)
                {
                    string commandValues = BuildCpuOffsetCommand(requestedOffset);
                    await RyzenAdj_To_UXTU.TranslateAsync(commandValues, false, true);
                    lastCPUUVOffset = requestedOffset;
                }

                if (cpuController.GetLastAppliedOffset() != requestedOffset)
                    cpuController.RecordAppliedOffset(requestedOffset);
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed CPU undervolt");
            }
        }

        private async Task ProcessIgpuUndervoltAsync()
        {
            try
            {
                if (!Settings.Default.isAutoUviGPU)
                {
                    await DisableIgpuUndervoltAsync();
                    return;
                }

                monitor ??= new InstabilityMonitor();

                iGpuController ??= new AdaptiveUndervoltController(
                    monitor,
                    minOffset: -50,
                    stepSize: 1,
                    stableThreshold: 8,
                    cooldownThreshold: 4,
                    isIgpu: true,
                    minimumEvaluationIntervalMilliseconds: 1000
                );

                int requestedOffset = iGpuController.UpdateOffset();

                if (lastiGPUUVOffset != requestedOffset)
                {
                    string commandValues = BuildIgpuOffsetCommand(requestedOffset);
                    await RyzenAdj_To_UXTU.TranslateAsync(commandValues, false, true);
                    lastiGPUUVOffset = requestedOffset;
                }

                if (iGpuController.GetLastAppliedOffset() != requestedOffset)
                    iGpuController.RecordAppliedOffset(requestedOffset);
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed iGPU undervolt");
            }
        }

        private async Task DisableCpuUndervoltAsync()
        {
            try
            {
                int appliedOffset = cpuController?.GetLastAppliedOffset() ?? lastCPUUVOffset;

                if (appliedOffset != 0 || lastCPUUVOffset != 0)
                {
                    await RyzenAdj_To_UXTU.TranslateAsync(
                        BuildCpuOffsetCommand(0),
                        false,
                        true
                    );

                    lastCPUUVOffset = 0;
                    cpuController?.RecordAppliedOffset(0);
                }

                cpuController?.Dispose();
                cpuController = null;
            } catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to disable CPU undervolt");
            }
        }

        private async Task DisableIgpuUndervoltAsync()
        {
            try
            {
                int appliedOffset = iGpuController?.GetLastAppliedOffset() ?? lastiGPUUVOffset;

                if (appliedOffset != 0 || lastiGPUUVOffset != 0)
                {
                    await RyzenAdj_To_UXTU.TranslateAsync(
                        BuildIgpuOffsetCommand(0),
                        false,
                        true
                    );

                    lastiGPUUVOffset = 0;
                    iGpuController?.RecordAppliedOffset(0);
                }

                iGpuController?.Dispose();
                iGpuController = null;
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to disable iGPU undervolt");
            }
        }

        private string BuildCpuOffsetCommand(int offset)
        {
            if (Family.FAM < Family.RyzenFamily.Renoir)
                return $"--set-coper={offset & 0xFFFF} ";

            return $"--set-coall={EncodeCurveOptimiserOffset(offset)} ";
        }

        private static string BuildIgpuOffsetCommand(int offset)
        {
            return $"--set-cogfx={EncodeCurveOptimiserOffset(offset)} ";
        }

        private static uint EncodeCurveOptimiserOffset(int offset)
        {
            if (offset >= 0)
                return (uint)offset;

            long magnitude = -(long)offset;

            if (magnitude > 0x100000)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return 0x100000u - (uint)magnitude;
        }

        private static ushort statuscode;

        public static void GetBatteryStatus()
        {
            try
            {
                using var batteryClass = new ManagementClass("Win32_Battery");
                using var batteries = batteryClass.GetInstances();

                foreach (ManagementObject battery in batteries)
                {
                    using (battery)
                        statuscode = (ushort)battery["BatteryStatus"];
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
                DiagnosticLogger.LogError(ex, "Failed to get battery status");
            }
        }

        private async Task ReapplyAsync()
        {
            try
            {

                if (_startupApplyPending || _automaticTuningSuppressed)
                    return;

                if ((bool)Settings.Default.AutoReapply == true && (bool)Settings.Default.isAdaptiveModeRunning == false)
                {
                    string commands = (string)Settings.Default.CommandString;
                    //Check if RyzenAdjArguments is populated
                    if (commands != null && commands != "")
                    {
                        await RyzenAdj_To_UXTU.TranslateAsync(commands, isAutoReapply: true);
                    }

                    if (autoReapply.Interval != TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime))
                    {
                        autoReapply.Stop();
                        autoReapply.Interval = TimeSpan.FromSeconds((int)Settings.Default.AutoReapplyTime);
                        autoReapply.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed during auto-reapply tick");
            }
        }
        private static void UpdateTimerInterval(DispatcherTimer timer, int newInterval)
        {
            if (timer.Interval == TimeSpan.FromSeconds(newInterval)) return;

            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(newInterval);
            timer.Start();
        }

        static string lastAppliedState = "";
        private async Task ApplyPowerModeAsync(PowerModeChangedEventArgs e)
        {
            try
            {
                if (_automaticTuningSuppressed)
                    return;

                if ((bool)Settings.Default.isAdaptiveModeRunning == false)
                {
                    if (e.Mode == PowerModes.StatusChange)
                    {
                        await Task.Run(() => GetBatteryStatus());
                        await Task.Run(() => PremadePresets.SetPremadePresets());

                        if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                        {
                            if (Settings.Default.acCommandString != null && Settings.Default.acCommandString != "" && Settings.Default.acPreset != "None")
                            {
                                if (Settings.Default.acPreset.Contains("PM - Eco"))
                                {
                                    Settings.Default.premadePreset = 0;
                                    Settings.Default.acCommandString = PremadePresets.EcoPreset;
                                }
                                else if (Settings.Default.acPreset.Contains("PM - Bal"))
                                {
                                    Settings.Default.premadePreset = 1;
                                    Settings.Default.acCommandString = PremadePresets.BalPreset;
                                }
                                else if (Settings.Default.acPreset.Contains("PM - Perf"))
                                {
                                    Settings.Default.premadePreset = 2;
                                    Settings.Default.acCommandString = PremadePresets.PerformancePreset;
                                }
                                else if (Settings.Default.acPreset.Contains("PM - Ext"))
                                {
                                    Settings.Default.premadePreset = 3;
                                    Settings.Default.acCommandString = PremadePresets.ExtremePreset;
                                }

                                Settings.Default.CommandString = Settings.Default.acCommandString;
                                Settings.Default.Save();
                                await TranslatePresetAsync(Settings.Default.acCommandString, Settings.Default.acPreset);

                                if (lastAppliedState != "ac") ToastNotification.ShowToastNotification("Charge Preset Applied!", $"Your charge preset settings have been applied!");
                                lastAppliedState = "ac";
                            }
                        }
                        else
                        {
                            if (Settings.Default.dcCommandString != null && Settings.Default.dcCommandString != "" && Settings.Default.dcPreset != "None")
                            {
                                if (Settings.Default.dcPreset.Contains("PM - Eco"))
                                {
                                    Settings.Default.premadePreset = 0;
                                    Settings.Default.dcCommandString = PremadePresets.EcoPreset;
                                }
                                else if (Settings.Default.dcPreset.Contains("PM - Bal"))
                                {
                                    Settings.Default.premadePreset = 1;
                                    Settings.Default.dcCommandString = PremadePresets.BalPreset;
                                }
                                else if (Settings.Default.dcPreset.Contains("PM - Perf"))
                                {
                                    Settings.Default.premadePreset = 2;
                                    Settings.Default.dcCommandString = PremadePresets.PerformancePreset;
                                }
                                else if (Settings.Default.dcPreset.Contains("PM - Ext"))
                                {
                                    Settings.Default.premadePreset = 3;
                                    Settings.Default.dcCommandString = PremadePresets.ExtremePreset;
                                }
                                Settings.Default.CommandString = Settings.Default.dcCommandString;
                                Settings.Default.Save();
                                await TranslatePresetAsync(Settings.Default.dcCommandString, Settings.Default.dcPreset);

                                if (lastAppliedState != "dc") ToastNotification.ShowToastNotification("Discharge Preset Applied!", $"Your discharge preset settings have been applied!");
                                lastAppliedState = "dc";
                            }
                        }
                    }

                    if (e.Mode == PowerModes.Resume)
                    {
                        if (Settings.Default.resumeCommandString != null && Settings.Default.resumeCommandString != "" && Settings.Default.resumePreset != "None")
                        {
                            if (Settings.Default.resumePreset.Contains("PM - Eco"))
                            {
                                Settings.Default.premadePreset = 0;
                                Settings.Default.resumeCommandString = PremadePresets.EcoPreset;
                            }
                            else if (Settings.Default.resumePreset.Contains("PM - Bal"))
                            {
                                Settings.Default.premadePreset = 1;
                                Settings.Default.resumeCommandString = PremadePresets.BalPreset;
                            }
                            else if (Settings.Default.resumePreset.Contains("PM - Perf"))
                            {
                                Settings.Default.premadePreset = 2;
                                Settings.Default.resumeCommandString = PremadePresets.PerformancePreset;
                            }
                            else if (Settings.Default.resumePreset.Contains("PM - Ext"))
                            {
                                Settings.Default.premadePreset = 3;
                                Settings.Default.resumeCommandString = PremadePresets.ExtremePreset;
                            }
                            Settings.Default.CommandString = Settings.Default.resumeCommandString;
                            Settings.Default.Save();
                            await TranslatePresetAsync(Settings.Default.resumeCommandString, Settings.Default.resumePreset);

                            if (lastAppliedState != "resume") ToastNotification.ShowToastNotification("Resume Preset Applied!", $"Your resume preset settings have been applied!");
                            lastAppliedState = "resume";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed during power mode change handling");
            }
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


}
