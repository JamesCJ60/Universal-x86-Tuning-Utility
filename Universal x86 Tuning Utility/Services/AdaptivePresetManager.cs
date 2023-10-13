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
    public class AdaptivePreset
    {
        public int Temp { get; set; }
        public int Power { get; set; }
        public int CO { get; set; }
        public int minGFX { get; set; }
        public int MaxGFX { get; set; }
        public int minCPU { get; set; }
        public bool isCO { get; set; }
        public bool isGFX { get; set; }

        public int rsr { get; set; }
        public int boost { get; set; }
        public int imageSharp { get; set; }

        public bool isRadeonGraphics { get; set; }
        public bool isAntiLag { get; set; }
        public bool isRSR { get; set; }
        public bool isBoost { get; set; }
        public bool isImageSharp { get; set; }
        public bool isSync { get; set; }

        public bool isNVIDIA { get; set; }
        public int nvCoreClk { get; set; }
        public int nvMemClk { get; set; }

        public int asusPowerProfile { get; set; }

        public bool isMag { get; set; }
        public bool isVsync { get; set; }
        public bool isRecap { get; set; }
        public int Sharpness { get; set; }
        public int ResScaleIndex { get; set; }
    }

    public class AdaptivePresetManager
    {
        private string _filePath;
        private Dictionary<string, AdaptivePreset> _presets;

        public AdaptivePresetManager(string filePath)
        {
            _filePath = filePath;
            _presets = new Dictionary<string, AdaptivePreset>();
            LoadPresets();
        }

        public IEnumerable<string> GetPresetNames()
        {
            return _presets.Keys;
        }

        public AdaptivePreset GetPreset(string presetName)
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

        public void SavePreset(string name, AdaptivePreset preset)
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
                _presets = JsonConvert.DeserializeObject<Dictionary<string, AdaptivePreset>>(json);
            }
            else
            {
                _presets = new Dictionary<string, AdaptivePreset>();
            }
        }


        private void SavePresets()
        {
            string json = JsonConvert.SerializeObject(_presets, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
