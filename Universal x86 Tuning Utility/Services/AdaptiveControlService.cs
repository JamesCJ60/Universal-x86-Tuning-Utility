using System;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Adaptive;

namespace Universal_x86_Tuning_Utility.Services;

public interface IAdaptiveControlState
{
    int WindowsBoostModeIndex { get; }
    bool IsWindowsMinStateEnabled { get; }
    double WindowsMinState { get; }
    bool IsWindowsMaxStateEnabled { get; }
    double WindowsMaxState { get; }
    bool IsWindowsMaxFrequencyEnabled { get; }
    double WindowsMaxFrequency { get; }
    bool IsWindowsEppEnabled { get; }
    double WindowsEpp { get; }
    bool IsWindowsCoreParkingEnabled { get; }
    double WindowsCoreParking { get; }
    bool IsWindowsMaxUnparkedCoresEnabled { get; }
    double WindowsMaxUnparkedCores { get; }
    double MinCpuClk { get; }
    double Temp { get; }
    double PowerLimit { get; }
    bool IsCurveEnabled { get; }
    double Curve { get; }
    bool IsTBOiGPUEnabled { get; }
    double MaxGfxClk { get; }
    double MinGfxClk { get; }
    bool IsUXTUSREnabled { get; }
    bool IsVSyncEnabled { get; }
    bool IsAutoCapEnabled { get; }
    double Sharp { get; }
    int ResScaleIndex { get; }
    bool IsRadeonGraphEnabled { get; }
    bool IsAntiLagEnabled { get; }
    bool IsRSREnabled { get; }
    double RSR { get; }
    bool IsBoostEnabled { get; }
    double Boost { get; }
    bool IsImageSharpEnabled { get; }
    double ImageSharp { get; }
    bool IsSyncEnabled { get; }
    bool IsNVEnabled { get; }
    double NVMaxCore { get; }
    double NVCore { get; }
    double NVMem { get; }
    int AsusPowerIndex { get; }
    bool IsRTSSEnabled { get; }
    double FrameRateLimit { get; }
}

public interface IAdaptiveControlService
{
    void Reset();
    Task UpdateAsync(IAdaptiveControlState state, AdaptiveReadings readings, bool isAsus);
}

public sealed class AdaptiveControlService : IAdaptiveControlService
{
    private readonly IPresetApplicationService _application;
    private int i;
    private string lastCPU = "", lastCO = "", lastiGPU = "";
    public AdaptiveControlService(IPresetApplicationService application) => _application = application;
    public void Reset() { i = 0; lastCPU = lastCO = lastiGPU = ""; }
    public async Task UpdateAsync(IAdaptiveControlState state, AdaptiveReadings readings, bool isAsus)
    {
        try
        {
            if (readings != null)
            {
                if (i < 2)
                {
                    CPUControl.UpdatePowerLimit(readings.CpuTemperature, readings.CpuLoad, (int)state.PowerLimit, (int)state.PowerLimit - 5, (int)state.Temp);
                    CPUControl.UpdatePowerLimit(readings.CpuTemperature, readings.CpuLoad, (int)state.PowerLimit, (int)state.PowerLimit - 5, (int)state.Temp);
                    CPUControl.UpdatePowerLimit(readings.CpuTemperature, readings.CpuLoad, (int)state.PowerLimit, (int)state.PowerLimit - 5, (int)state.Temp);
                    i++;
                }
                else
                {
                    CPUControl.UpdatePowerLimit(readings.CpuTemperature, readings.CpuLoad, (int)state.PowerLimit, 8, (int)state.Temp);

                    if (state.IsCurveEnabled == true) CPUControl.CurveOptimiserLimit(readings.CpuLoad, (int)state.Curve);

                    if (state.IsTBOiGPUEnabled == true) iGPUControl.UpdateiGPUClock((int)state.MaxGfxClk, (int)state.MinGfxClk, (int)state.Temp, readings.CpuPower, readings.CpuTemperature, readings.GpuClock, readings.GpuLoad, readings.GpuMemoryClock, readings.CpuClock, (int)state.MinCpuClk);

                    var boostMode = state.WindowsBoostModeIndex > 0 ? state.WindowsBoostModeIndex - 1 : -1;
                    var minState = state.IsWindowsMinStateEnabled ? (int)state.WindowsMinState : -1;
                    var maxState = state.IsWindowsMaxStateEnabled ? (int)state.WindowsMaxState : -1;
                    var frequency = state.IsWindowsMaxFrequencyEnabled ? (int)state.WindowsMaxFrequency : -1;
                    var epp = state.IsWindowsEppEnabled ? (int)state.WindowsEpp : -1;
                    var parking = state.IsWindowsCoreParkingEnabled ? (int)state.WindowsCoreParking : -1;
                    var unparked = state.IsWindowsMaxUnparkedCoresEnabled ? (int)state.WindowsMaxUnparkedCores : -1;
                    string commandString = boostMode >= 0 || minState >= 0 || maxState >= 0 || frequency >= 0 || epp >= 0 || parking >= 0 || unparked >= 0
                        ? $"--Win-CPU={boostMode},{maxState},{frequency},{epp},{minState},{parking},{unparked} " : "";

                    commandString = commandString + $"--UXTUSR={state.IsUXTUSREnabled}-{state.IsVSyncEnabled}-{state.Sharp / 100}-{state.ResScaleIndex}-{state.IsAutoCapEnabled} ";

                    if (isAsus)
                    {
                        if (state.AsusPowerIndex > 0) commandString = commandString + $"--ASUS-Power={state.AsusPowerIndex} ";
                    }

                    if (CPUControl.cpuCommand != lastCPU)
                    {
                        commandString = commandString + CPUControl.cpuCommand;
                        lastCPU = CPUControl.cpuCommand;
                    }

                    if (CPUControl.coCommand != null && CPUControl.coCommand != "" && state.IsCurveEnabled == true && CPUControl.coCommand != lastCO)
                    {
                        commandString = commandString + CPUControl.coCommand;
                        lastCO = CPUControl.coCommand;
                    }

                    if (iGPUControl.commmand != null && iGPUControl.commmand != "" && state.IsTBOiGPUEnabled == true && iGPUControl.commmand != lastiGPU)
                    {
                        commandString = commandString + iGPUControl.commmand;
                        lastiGPU = iGPUControl.commmand;
                    }

                    if (state.IsRadeonGraphEnabled == true)
                    {
                        if (state.IsAntiLagEnabled == true) commandString = commandString + $"--ADLX-Lag=0-true --ADLX-Lag=1-true ";
                        else commandString = commandString + $"--ADLX-Lag=0-false --ADLX-Lag=1-false ";

                        if (state.IsRSREnabled == true) commandString = commandString + $"--ADLX-RSR=true-{(int)state.RSR} ";
                        else commandString = commandString + $"--ADLX-RSR=false-{(int)state.RSR} ";

                        if (state.IsBoostEnabled == true) commandString = commandString + $"--ADLX-Boost=0-true-{(int)state.Boost} --ADLX-Boost=1-true-{(int)state.Boost} ";
                        else commandString = commandString + $"--ADLX-Boost=0-false-{(int)state.Boost} --ADLX-Boost=1-false-{(int)state.Boost} ";

                        if (state.IsImageSharpEnabled == true) commandString = commandString + $"--ADLX-ImageSharp=0-true-{(int)state.ImageSharp} --ADLX-ImageSharp=1-true-{(int)state.ImageSharp} ";
                        else commandString = commandString + $"--ADLX-ImageSharp=0-false-{(int)state.ImageSharp} --ADLX-ImageSharp=1-false-{(int)state.ImageSharp} ";

                        if (state.IsSyncEnabled == true) commandString = commandString + $"--ADLX-Sync=0-true --ADLX-Sync=1-true ";
                        else commandString = commandString + $"--ADLX-Sync=0-false --ADLX-Sync=1-false ";
                    }

                    if (state.IsNVEnabled == true)
                    {
                        commandString = commandString + $"--NVIDIA-Clocks={state.NVMaxCore}-{state.NVCore}-{state.NVMem} ";
                    }

                    if (commandString != null && commandString != "") await _application.ApplyAsync(commandString, isAutoReapply: true);
                }

                if (RTSS.RTSSRunning() && state.IsRTSSEnabled == true) RTSS.setRTSSFPSLimit((int)state.FrameRateLimit);


            }
        }
        catch (Exception ex) { Serilog.Log.Error(ex, "Failed to apply adaptive settings"); }
    }
}
