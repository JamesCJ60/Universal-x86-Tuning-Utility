using LibreHardwareMonitor.Hardware;
using System;
using System.Linq;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class GetSensor
    {
        private static readonly Computer thisPC = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        private static DateTime lastUpdate;
        private static readonly TimeSpan updateInterval = TimeSpan.FromSeconds(1);

        public static void OpenSensor()
        {
            thisPC.Open();
        }

        public static void CloseSensor()
        {
            thisPC.Close();
        }

        private static void UpdateAllHardware()
        {
            if (DateTime.UtcNow - lastUpdate < updateInterval)
            {
                return;
            }

            foreach (var hardware in thisPC.Hardware)
            {
                hardware.Update();
            }

            lastUpdate = DateTime.UtcNow;
        }

        public static float GetCPUInfo(SensorType sensorType, string sensorName)
        {
            UpdateAllHardware();
            return GetSensorValue(thisPC.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu), sensorType, sensorName);
        }

        public static float GetAMDGPUInfo(SensorType sensorType, string sensorName)
        {
            UpdateAllHardware();
            return GetSensorValue(thisPC.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd), sensorType, sensorName);
        }

        public static float GetNVGPUInfo(SensorType sensorType, string sensorName)
        {
            UpdateAllHardware();
            return thisPC.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuNvidia)
                .Select(h => GetSensorValue(h, sensorType, sensorName))
                .FirstOrDefault();
        }

        private static float GetSensorValue(IHardware hardware, SensorType sensorType, string sensorName)
        {
            if (hardware == null)
            {
                return 0;
            }

            var sensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == sensorType && s.Name.Contains(sensorName));
            return sensor?.Value ?? 0;
        }
    }
}