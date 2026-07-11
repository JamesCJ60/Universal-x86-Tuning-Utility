using LibreHardwareMonitor.Hardware;
using System;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class GetSensor
    {
        private static readonly object Sync = new();
        private static IDisposable? _lease;

        private static IHardwareMonitoringService MonitoringService =>
            App.GetService<IHardwareMonitoringService>()
            ?? throw new InvalidOperationException("Hardware monitoring is not available.");

        public static void OpenSensor()
        {
            lock (Sync)
                _lease ??= MonitoringService.Acquire(HardwareMonitoringCategory.Cpu);
        }

        public static void CloseSensor()
        {
            lock (Sync)
            {
                _lease?.Dispose();
                _lease = null;
            }
        }

        public static float GetCPUInfo(SensorType sensorType, string sensorName)
        {
            return MonitoringService.ReadSensor(HardwareType.Cpu, sensorType, sensorName);
        }

        public static float GetAMDGPUInfo(SensorType sensorType, string sensorName)
        {
            using IDisposable lease = MonitoringService.Acquire(HardwareMonitoringCategory.Gpu);
            return MonitoringService.ReadSensor(HardwareType.GpuAmd, sensorType, sensorName);
        }

        public static float GetNVGPUInfo(SensorType sensorType, string sensorName)
        {
            using IDisposable lease = MonitoringService.Acquire(HardwareMonitoringCategory.Gpu);
            return MonitoringService.ReadSensor(HardwareType.GpuNvidia, sensorType, sensorName);
        }
    }
}
