using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Services
{
    public sealed record PresetBackupResult(int CustomPresetCount, int AdaptivePresetCount);

    public static class PresetBackupService
    {
        private const string BackupFormat = "UXTU Preset Backup";
        private const int BackupVersion = 1;
        private const long MaximumBackupSize = 64 * 1024 * 1024;

        private static readonly IReadOnlyDictionary<string, string> CustomPresetFiles = new Dictionary<string, string>
        {
            ["amdApu"] = "apuPresets.json",
            ["amdDesktop"] = "amdDtCpuPresets.json",
            ["intel"] = "intelPresets.json"
        };

        public static async Task<PresetBackupResult> ExportAsync(string presetDirectory, string destinationPath)
        {
            var customPresets = new JObject();
            var customCount = 0;

            foreach (var entry in CustomPresetFiles)
            {
                var presets = await ReadPresetObjectAsync(Path.Combine(presetDirectory, entry.Value));
                customPresets[entry.Key] = presets;
                customCount += presets.Count;
            }

            var adaptivePresets = await ReadPresetObjectAsync(Path.Combine(presetDirectory, "adaptivePresets.json"));
            var backup = new JObject
            {
                ["format"] = BackupFormat,
                ["version"] = BackupVersion,
                ["createdUtc"] = DateTime.UtcNow.ToString("O"),
                ["customPresets"] = customPresets,
                ["adaptivePresets"] = adaptivePresets
            };

            await WriteAtomicAsync(destinationPath, backup.ToString(Formatting.Indented));
            return new PresetBackupResult(customCount, adaptivePresets.Count);
        }

        public static async Task<PresetBackupResult> ImportAsync(string presetDirectory, string sourcePath)
        {
            var file = new FileInfo(sourcePath);
            if (!file.Exists || file.Length == 0 || file.Length > MaximumBackupSize)
            {
                throw new InvalidDataException();
            }

            JObject backup;
            try
            {
                backup = JObject.Parse(await File.ReadAllTextAsync(sourcePath));
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(null, exception);
            }

            if (!string.Equals(backup.Value<string>("format"), BackupFormat, StringComparison.Ordinal) ||
                backup.Value<int?>("version") != BackupVersion ||
                backup["customPresets"] is not JObject customPresets ||
                backup["adaptivePresets"] is not JObject adaptivePresets)
            {
                throw new InvalidDataException();
            }

            var preparedFiles = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            var customCount = 0;

            foreach (var entry in CustomPresetFiles)
            {
                if (customPresets[entry.Key] is not JObject imported)
                {
                    throw new InvalidDataException();
                }

                ValidatePresets(imported);
                var path = Path.Combine(presetDirectory, entry.Value);
                var merged = await ReadPresetObjectAsync(path);
                MergePresets(merged, imported);
                preparedFiles[path] = merged;
                customCount += imported.Count;
            }

            ValidatePresets(adaptivePresets);
            var adaptivePath = Path.Combine(presetDirectory, "adaptivePresets.json");
            var mergedAdaptive = await ReadPresetObjectAsync(adaptivePath);
            MergePresets(mergedAdaptive, adaptivePresets);
            preparedFiles[adaptivePath] = mergedAdaptive;

            await WritePresetFilesAsync(preparedFiles);
            return new PresetBackupResult(customCount, adaptivePresets.Count);
        }

        private static async Task<JObject> ReadPresetObjectAsync(string path)
        {
            if (!File.Exists(path))
            {
                return new JObject();
            }

            var json = await File.ReadAllTextAsync(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new JObject();
            }

            try
            {
                var presets = JObject.Parse(json);
                ValidatePresets(presets);
                return presets;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(null, exception);
            }
        }

        private static void ValidatePresets(JObject presets)
        {
            if (presets.Properties().Any(property => string.IsNullOrWhiteSpace(property.Name) || property.Value.Type != JTokenType.Object))
            {
                throw new InvalidDataException();
            }
        }

        private static void MergePresets(JObject destination, JObject source)
        {
            foreach (var property in source.Properties())
            {
                destination[property.Name] = property.Value.DeepClone();
            }
        }

        private static async Task WritePresetFilesAsync(IReadOnlyDictionary<string, JObject> files)
        {
            var originals = files.Keys.ToDictionary(
                path => path,
                path => File.Exists(path) ? File.ReadAllText(path) : null,
                StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var file in files)
                {
                    await WriteAtomicAsync(file.Key, file.Value.ToString(Formatting.Indented));
                }
            }
            catch
            {
                foreach (var original in originals)
                {
                    if (original.Value == null)
                    {
                        if (File.Exists(original.Key))
                        {
                            File.Delete(original.Key);
                        }
                    }
                    else
                    {
                        await WriteAtomicAsync(original.Key, original.Value);
                    }
                }

                throw;
            }
        }

        private static async Task WriteAtomicAsync(string path, string contents)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? throw new InvalidOperationException();
            Directory.CreateDirectory(directory);
            var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

            try
            {
                await File.WriteAllTextAsync(temporaryPath, contents);
                File.Move(temporaryPath, path, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }
}
