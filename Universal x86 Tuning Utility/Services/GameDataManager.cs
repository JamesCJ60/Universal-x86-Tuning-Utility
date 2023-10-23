using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Universal_x86_Tuning_Utility.Services
{
    public class GameData
    {
        public string fpsData { get; set; } = "No Data";
    }

    public class GameDataManager
    {
        private string _filePath;
        private Dictionary<string, GameData> _presets;

        public GameDataManager(string filePath)
        {
            _filePath = filePath;
            _presets = new Dictionary<string, GameData>();
            LoadPresets();
        }

        public IEnumerable<string> GetPresetNames()
        {
            return _presets.Keys;
        }

        public GameData GetPreset(string presetName)
        {
            if (_presets.ContainsKey(presetName))
            {
                return _presets[presetName];
            }
            else
            {
                return null;
            }
        }

        public void SavePreset(string name, GameData preset)
        {
            _presets[name] = preset;
            SavePresets();
        }

        public void DeletePreset(string name)
        {
            _presets.Remove(name);
            SavePresets();
        }

        private void LoadPresets()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _presets = JsonConvert.DeserializeObject<Dictionary<string, GameData>>(json);
            }
            else
            {
                _presets = new Dictionary<string, GameData>();
            }
        }


        private void SavePresets()
        {
            string json = JsonConvert.SerializeObject(_presets, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
