using System;
using System.Windows;

namespace Universal_x86_Tuning_Utility.Services
{
    public sealed record LastAppliedSettings(string Arguments, string? PresetName, bool LocalizePresetName);

    public static class LastAppliedSettingsService
    {
        private static readonly object Sync = new();
        private static LastAppliedSettings? current;

        public static event EventHandler? Changed;

        public static LastAppliedSettings? Current
        {
            get
            {
                lock (Sync)
                {
                    return current;
                }
            }
        }

        public static void Update(string arguments, string? presetName, bool localizePresetName)
        {
            LastAppliedSettings value;
            lock (Sync)
            {
                if (string.IsNullOrWhiteSpace(presetName) && string.Equals(current?.Arguments, arguments, StringComparison.Ordinal))
                {
                    presetName = current?.PresetName;
                    localizePresetName = current?.LocalizePresetName ?? false;
                }

                value = new LastAppliedSettings(arguments, presetName, localizePresetName);
                if (value == current)
                {
                    return;
                }

                current = value;
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(RaiseChanged));
                return;
            }

            RaiseChanged();
        }

        private static void RaiseChanged() => Changed?.Invoke(null, EventArgs.Empty);
    }
}
