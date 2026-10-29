using System;
using System.Diagnostics;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Scripts
{
    internal static class WindowsProcessorPower
    {
        private static readonly object Sync = new();

        public static void Apply(string value)
        {
            var values = value.Split(',');
            if ((values.Length != 4 && values.Length != 6 && values.Length != 7) ||
                !int.TryParse(values[0], out var boostMode) ||
                !int.TryParse(values[1], out var maximumState) ||
                !int.TryParse(values[2], out var maximumFrequency) ||
                !int.TryParse(values[3], out var energyPreference))
            {
                return;
            }

            var minimumState = values.Length == 6 && int.TryParse(values[4], out var parsedMinimumState)
                ? parsedMinimumState
                : -1;
            var minimumUnparkedCores = values.Length == 6 && int.TryParse(values[5], out var parsedMinimumUnparkedCores)
                ? parsedMinimumUnparkedCores
                : -1;
            if (values.Length == 7 && int.TryParse(values[5], out parsedMinimumUnparkedCores))
            {
                minimumUnparkedCores = parsedMinimumUnparkedCores;
            }
            var maximumUnparkedCores = values.Length == 7 && int.TryParse(values[6], out var parsedMaximumUnparkedCores)
                ? parsedMaximumUnparkedCores
                : -1;

            if (minimumState >= 0 && maximumState >= 0)
            {
                minimumState = Math.Min(minimumState, maximumState);
            }
            if (minimumUnparkedCores >= 0 && maximumUnparkedCores >= 0)
            {
                minimumUnparkedCores = Math.Min(minimumUnparkedCores, maximumUnparkedCores);
            }

            lock (Sync)
            {
                ApplySetting("PERFBOOSTMODE", boostMode, 0, 6);
                ApplySetting("PROCTHROTTLEMIN", minimumState, 0, 100);
                ApplySetting("PROCTHROTTLEMAX", maximumState, 1, 100);
                ApplySetting("PROCFREQMAX", maximumFrequency, 100, 64000);
                ApplySetting("PERFEPP", energyPreference, 0, 100);
                ApplySetting("CPMINCORES", minimumUnparkedCores, 0, 100);
                ApplySetting("CPMAXCORES", maximumUnparkedCores, 0, 100);
                RunPowerCfg("/setactive", "SCHEME_CURRENT");
            }
        }

        private static void ApplySetting(string alias, int value, int minimum, int maximum)
        {
            if (value < 0)
            {
                return;
            }

            value = Math.Clamp(value, minimum, maximum);
            RunPowerCfg("/setacvalueindex", "SCHEME_CURRENT", "SUB_PROCESSOR", alias, value.ToString());
            RunPowerCfg("/setdcvalueindex", "SCHEME_CURRENT", "SUB_PROCESSOR", alias, value.ToString());
        }

        private static void RunPowerCfg(params string[] arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo("powercfg.exe")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                foreach (var argument in arguments)
                {
                    startInfo.ArgumentList.Add(argument);
                }

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return;
                }

                process.WaitForExit(5000);
                if (process.HasExited && process.ExitCode != 0)
                {
                    DiagnosticLogger.LogDebug($"powercfg exited with code {process.ExitCode}: {process.StandardError.ReadToEnd()}");
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to apply Windows processor power settings");
            }
        }
    }
}
