using System;
using System.IO;
using System.Text.Json;
using Universal_x86_Tuning_Utility.Models;

namespace Universal_x86_Tuning_Utility.Services
{
    public sealed class OverlaySettingsStore
    {
        private readonly object _sync = new();
        private readonly string _settingsPath;
        private OverlayOptions _current;

        public OverlaySettingsStore()
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "UXTU");
            Directory.CreateDirectory(directory);
            _settingsPath = Path.Combine(directory, "overlay.json");
            _current = Load();
        }

        public OverlayOptions Current
        {
            get
            {
                lock (_sync)
                    return _current with { };
            }
        }

        public void Save(OverlayOptions options)
        {
            OverlayOptions validated = options with
            {
                RefreshIntervalMs = Math.Clamp(options.RefreshIntervalMs, 100, 5000),
                Scale = Math.Clamp(options.Scale, 0.5, 2),
                Opacity = Math.Clamp(options.Opacity, 0.25, 1)
            };

            lock (_sync)
            {
                string temporaryPath = _settingsPath + ".tmp";
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(validated, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temporaryPath, _settingsPath, true);
                _current = validated;
            }
        }

        private OverlayOptions Load()
        {
            try
            {
                if (File.Exists(_settingsPath))
                    return JsonSerializer.Deserialize<OverlayOptions>(File.ReadAllText(_settingsPath)) ?? new OverlayOptions();
            }
            catch
            {
            }

            return new OverlayOptions();
        }
    }
}
