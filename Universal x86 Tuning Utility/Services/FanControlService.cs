using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Services;

public interface IFanControlService
{
    string ConfigurationName { get; }
    bool IsEnabled { get; }
    void Reload();
    void Enable();
    void Disable();
    void SetSpeed(int percentage);
    int ReadCpuTemperature();
}

public sealed class FanControlService : IFanControlService
{
    public string ConfigurationName => $"{GetSystemInfo.Manufacturer.ToUpperInvariant()}_{GetSystemInfo.Product.ToUpperInvariant()}.json";
    // The hardware fan-control backend is disabled in Dev; retain curve preview only.
    public bool IsEnabled => false;
    public void Reload() { }
    public void Enable() { }
    public void Disable() { }
    public void SetSpeed(int percentage) { }
    public int ReadCpuTemperature()
    {
        var computer = new Computer { IsCpuEnabled = true };
        try
        {
            computer.Open();
            var cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            cpu?.Update();
            var temperature = cpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature)?.Value;
            return temperature.HasValue ? (int)temperature.Value : throw new InvalidOperationException("CPU temperature is unavailable.");
        }
        finally { computer.Close(); }
    }
}
