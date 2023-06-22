using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Services
{
    public class FanData
    {
        public int MinFanSpeed { get; set; }
        public int MaxFanSpeed { get; set; }
        public int MinFanSpeedPercentage { get; set; }
        public string FanControlAddress { get; set; }
        public string FanSetAddress { get; set; }
        public string EnableToggleAddress { get; set; }
        public string DisableToggleAddress { get; set; }
        public string RegAddress { get; set; }
        public string RegData { get; set; }
    }

    internal class FanConfigManager
    {
        private readonly string _configDirectory;

        public FanConfigManager(string configDirectory)
        {
            _configDirectory = configDirectory;
        }

        public FanData GetDataForDevice()
        {
            var json = File.ReadAllText(_configDirectory);
            return JsonConvert.DeserializeObject<FanData>(json);
        }
    }
}
