using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RyzenSmu;
using Universal_x86_Tuning_Utility.Scripts;

namespace Universal_x86_Tuning_Utility.Services;

public interface ICustomPresetCommandState
{
    bool IsUXTUSREnabled { get; }
    bool IsVSyncEnabled { get; }
    double Sharp { get; }
    int ResScaleIndex { get; }
    bool IsAutoCapEnabled { get; }
    int AsusPowerIndex { get; }
    Visibility AsusEcoVisibility { get; }
    bool IsASUSEcoEnabled { get; }
    Visibility AsusUltiVisibility { get; }
    bool IsASUSUltiEnabled { get; }
    Visibility RefreshRateVisibility { get; }
    int RefreshRateIndex { get; }
    Visibility PowerModeVisibility { get; }
    int PowerModeIndex { get; }
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
    bool IsAPUTempEnabled { get; }
    double APUTemp { get; }
    bool IsAPUSkinTempEnabled { get; }
    double APUSkinTemp { get; }
    bool IsSTAPMPowEnabled { get; }
    double STAPMPow { get; }
    bool IsFastPowEnabled { get; }
    double FastPow { get; }
    bool IsFastTimeEnabled { get; }
    double FastTime { get; }
    bool IsSlowPowEnabled { get; }
    double SlowPow { get; }
    bool IsSlowTimeEnabled { get; }
    double SlowTime { get; }
    bool IsCpuVrmTdcEnabled { get; }
    double CpuVrmTdc { get; }
    bool IsCpuVrmEdcEnabled { get; }
    double CpuVrmEdc { get; }
    bool IsSocVrmTdcEnabled { get; }
    double SocVrmTdc { get; }
    bool IsSocVrmEdcEnabled { get; }
    double SocVrmEdc { get; }
    bool IsGfxVrmTdcEnabled { get; }
    double GfxVrmTdc { get; }
    bool IsGfxVrmEdcEnabled { get; }
    double GfxVrmEdc { get; }
    bool IsAPUiGPUClkEnabled { get; }
    double APUiGPUClk { get; }
    bool IsPBOScalerEnabled { get; }
    double PBOScaler { get; }
    bool IsAllCOEnabled { get; }
    double AllCO { get; }
    bool IsGfxCOEnabled { get; }
    double GfxCO { get; }
    bool IsSoftMiniGPUClkEnabled { get; }
    double SoftMiniGPUClk { get; }
    bool IsSoftMaxiGPUClkEnabled { get; }
    double SoftMaxiGPUClk { get; }
    bool IsSoftMinCPUClkEnabled { get; }
    double SoftMinCPUClk { get; }
    bool IsSoftMaxCPUClkEnabled { get; }
    double SoftMaxCPUClk { get; }
    bool IsSoftMinDataClkEnabled { get; }
    double SoftMinDataClk { get; }
    bool IsSoftMaxDataClkEnabled { get; }
    double SoftMaxDataClk { get; }
    bool IsSoftMinVCNClkEnabled { get; }
    double SoftMinVCNClk { get; }
    bool IsSoftMaxVCNClkEnabled { get; }
    double SoftMaxVCNClk { get; }
    bool IsSoftMinFabClkEnabled { get; }
    double SoftMinFabClk { get; }
    bool IsSoftMaxFabClkEnabled { get; }
    double SoftMaxFabClk { get; }
    bool IsSoftMinSoCClkEnabled { get; }
    double SoftMinSoCClk { get; }
    bool IsSoftMaxSoCClkEnabled { get; }
    double SoftMaxSoCClk { get; }
    int BoostIndex { get; }
    bool IsCCD1Core1Enabled { get; }
    double CCD1Core1 { get; }
    bool IsCCD1Core2Enabled { get; }
    double CCD1Core2 { get; }
    bool IsCCD1Core3Enabled { get; }
    double CCD1Core3 { get; }
    bool IsCCD1Core4Enabled { get; }
    double CCD1Core4 { get; }
    bool IsCCD1Core5Enabled { get; }
    double CCD1Core5 { get; }
    bool IsCCD1Core6Enabled { get; }
    double CCD1Core6 { get; }
    bool IsCCD1Core7Enabled { get; }
    double CCD1Core7 { get; }
    bool IsCCD1Core8Enabled { get; }
    double CCD1Core8 { get; }
    bool IsCCD1Core9Enabled { get; }
    double CCD1Core9 { get; }
    bool IsCCD1Core10Enabled { get; }
    double CCD1Core10 { get; }
    bool IsCCD1Core11Enabled { get; }
    double CCD1Core11 { get; }
    bool IsCCD1Core12Enabled { get; }
    double CCD1Core12 { get; }
    bool IsCCD2Core1Enabled { get; }
    double CCD2Core1 { get; }
    bool IsCCD2Core2Enabled { get; }
    double CCD2Core2 { get; }
    bool IsCCD2Core3Enabled { get; }
    double CCD2Core3 { get; }
    bool IsCCD2Core4Enabled { get; }
    double CCD2Core4 { get; }
    bool IsCCD2Core5Enabled { get; }
    double CCD2Core5 { get; }
    bool IsCCD2Core6Enabled { get; }
    double CCD2Core6 { get; }
    bool IsCCD2Core7Enabled { get; }
    double CCD2Core7 { get; }
    bool IsCCD2Core8Enabled { get; }
    double CCD2Core8 { get; }
    bool IsCCD2Core9Enabled { get; }
    double CCD2Core9 { get; }
    bool IsCCD2Core10Enabled { get; }
    double CCD2Core10 { get; }
    bool IsCCD2Core11Enabled { get; }
    double CCD2Core11 { get; }
    bool IsCCD2Core12Enabled { get; }
    double CCD2Core12 { get; }
    bool IsAmdOCEnabled { get; }
    double AmdCpuClk { get; }
    double AmdVID { get; }
    bool IsCPUTempEnabled { get; }
    double CPUTemp { get; }
    bool IsPPTEnabled { get; }
    double PPT { get; }
    bool IsTDCEnabled { get; }
    double TDC { get; }
    bool IsEDCEnabled { get; }
    double EDC { get; }
    bool IsIntelRatioCoreEnabled { get; }
    double IntelRatioC1 { get; }
    double IntelRatioC2 { get; }
    double IntelRatioC3 { get; }
    double IntelRatioC4 { get; }
    double IntelRatioC5 { get; }
    double IntelRatioC6 { get; }
    double IntelRatioC7 { get; }
    double IntelRatioC8 { get; }
    bool IsIntelPL1Enabled { get; }
    bool IsIntelPL2Enabled { get; }
    double IntelPL1 { get; }
    double IntelPL2 { get; }
    bool IsIntelUVEnabled { get; }
    double IntelCoreUV { get; }
    double IntelGfxUV { get; }
    double IntelCacheUV { get; }
    double IntelSAUV { get; }
    bool IsIntelBalEnabled { get; }
    double IntelCpuBal { get; }
    double IntelGpuBal { get; }
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
    double NVPower { get; }
    Visibility CcdAffinityVisibility { get; }
    int CcdAffinityIndex { get; }
}
public sealed record PresetHardwareContext(Family.ProcessorType ProcessorType, Family.RyzenFamily Family, bool IsAsus, IReadOnlyList<int> RefreshRates, int ClockRatioCount);
public interface IPresetCommandBuilder
{
    string Build(ICustomPresetCommandState state, PresetHardwareContext context);
}
public sealed class PresetCommandBuilder : IPresetCommandBuilder
{
        public string Build(ICustomPresetCommandState state, PresetHardwareContext hardware)
        {
            string commandValues = "";

            commandValues = commandValues + $"--UXTUSR={state.IsUXTUSREnabled}-{state.IsVSyncEnabled}-{state.Sharp / 100}-{state.ResScaleIndex}-{state.IsAutoCapEnabled} ";

            if (hardware.IsAsus)
            {
                if (state.AsusPowerIndex > 0) commandValues = commandValues + $"--ASUS-Power={state.AsusPowerIndex} ";
                if (state.AsusEcoVisibility == Visibility.Visible) commandValues = commandValues + $"--ASUS-Eco={state.IsASUSEcoEnabled} ";
                if (state.AsusUltiVisibility == Visibility.Visible) commandValues = commandValues + $"--ASUS-MUX={state.IsASUSUltiEnabled} ";
            }

            if (state.RefreshRateVisibility == Visibility.Visible && state.RefreshRateIndex > 0) commandValues = commandValues + $"--Refresh-Rate={hardware.RefreshRates[state.RefreshRateIndex - 1]} ";

            if (state.PowerModeVisibility == Visibility.Visible && state.PowerModeIndex > 0) commandValues = commandValues + $"--Win-Power={state.PowerModeIndex - 1} ";

            var windowsBoostMode = state.WindowsBoostModeIndex > 0 ? state.WindowsBoostModeIndex - 1 : -1;
            var windowsMinState = state.IsWindowsMinStateEnabled == true ? (int)state.WindowsMinState : -1;
            var windowsMaxState = state.IsWindowsMaxStateEnabled == true ? (int)state.WindowsMaxState : -1;
            var windowsMaxFrequency = state.IsWindowsMaxFrequencyEnabled == true ? (int)state.WindowsMaxFrequency : -1;
            var windowsEpp = state.IsWindowsEppEnabled == true ? (int)state.WindowsEpp : -1;
            var windowsCoreParking = state.IsWindowsCoreParkingEnabled == true ? (int)state.WindowsCoreParking : -1;
            var windowsMaxUnparkedCores = state.IsWindowsMaxUnparkedCoresEnabled == true ? (int)state.WindowsMaxUnparkedCores : -1;
            if (windowsBoostMode >= 0 || windowsMinState >= 0 || windowsMaxState >= 0 || windowsMaxFrequency >= 0 || windowsEpp >= 0 || windowsCoreParking >= 0 || windowsMaxUnparkedCores >= 0)
                commandValues = commandValues + $"--Win-CPU={windowsBoostMode},{windowsMaxState},{windowsMaxFrequency},{windowsEpp},{windowsMinState},{windowsCoreParking},{windowsMaxUnparkedCores} ";

            if (hardware.ProcessorType == Family.ProcessorType.Amd_Apu)
            {
                if (state.IsAPUTempEnabled == true) commandValues = commandValues + $"--tctl-temp={state.APUTemp} --cHTC-temp={state.APUTemp} ";
                if (state.IsAPUSkinTempEnabled == true) commandValues = commandValues + $"--apu-skin-temp={state.APUSkinTemp} ";
                if (state.IsSTAPMPowEnabled == true) commandValues = commandValues + $"--stapm-limit={state.STAPMPow * 1000} ";
                if (state.IsFastPowEnabled == true) commandValues = commandValues + $"--fast-limit={state.FastPow * 1000} ";
                if (state.IsFastTimeEnabled == true) commandValues = commandValues + $"--stapm-time={state.FastTime} ";
                if (state.IsSlowPowEnabled == true) commandValues = commandValues + $"--slow-limit={state.SlowPow * 1000} ";
                if (state.IsSlowTimeEnabled == true) commandValues = commandValues + $"--slow-time={state.SlowTime} ";
                if (state.IsCpuVrmTdcEnabled == true) commandValues = commandValues + $"--vrm-current={state.CpuVrmTdc * 1000} ";
                if (state.IsCpuVrmEdcEnabled == true) commandValues = commandValues + $"--vrmmax-current={state.CpuVrmEdc * 1000} ";
                if (state.IsSocVrmTdcEnabled == true) commandValues = commandValues + $"--vrmsoc-current={state.SocVrmTdc * 1000} ";
                if (state.IsSocVrmEdcEnabled == true) commandValues = commandValues + $"--vrmsocmax-current={state.SocVrmEdc * 1000} ";
                if (state.IsGfxVrmTdcEnabled == true) commandValues = commandValues + $"--vrmgfx-current={state.GfxVrmTdc * 1000} ";
                if (state.IsGfxVrmEdcEnabled == true) commandValues = commandValues + $"--vrmgfxmax-current={state.GfxVrmEdc * 1000} ";
                if (state.IsAPUiGPUClkEnabled == true) commandValues = commandValues + $"--gfx-clk={state.APUiGPUClk} ";
                if (state.IsPBOScalerEnabled == true) commandValues = commandValues + $"--pbo-scalar={state.PBOScaler * 100} ";

                if (state.IsAllCOEnabled == true)
                {

                    if(hardware.Family < Family.RyzenFamily.Renoir) commandValues = commandValues + $"--set-coper={(0 << 20) | ((int)state.AllCO & 0xFFFF)} ";
                    else
                    {
                        if (state.AllCO >= 0) commandValues = commandValues + $"--set-coall={state.AllCO} ";
                        if (state.AllCO < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)state.AllCO))} ";
                    }
                }

                if (state.IsGfxCOEnabled == true)
                {
                    if (state.GfxCO >= 0) commandValues = commandValues + $"--set-cogfx={state.GfxCO} ";
                    if (state.GfxCO < 0) commandValues = commandValues + $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)state.GfxCO))} ";
                }

                if (state.IsSoftMiniGPUClkEnabled == true) commandValues = commandValues + $"--min-gfxclk={state.SoftMiniGPUClk} ";
                if (state.IsSoftMaxiGPUClkEnabled == true) commandValues = commandValues + $"--max-gfxclk={state.SoftMaxiGPUClk} ";

                if (state.IsSoftMinCPUClkEnabled == true) commandValues = commandValues + $"--min-cpuclk={state.SoftMinCPUClk} ";
                if (state.IsSoftMaxCPUClkEnabled == true) commandValues = commandValues + $"--max-cpuclk={state.SoftMaxCPUClk} ";

                if (state.IsSoftMinDataClkEnabled == true) commandValues = commandValues + $"--min-lclk={state.SoftMinDataClk} ";
                if (state.IsSoftMaxDataClkEnabled == true) commandValues = commandValues + $"--max-lclk={state.SoftMaxDataClk} ";

                if (state.IsSoftMinVCNClkEnabled == true) commandValues = commandValues + $"--min-vcn={state.SoftMinVCNClk} ";
                if (state.IsSoftMaxVCNClkEnabled == true) commandValues = commandValues + $"--max-vcn={state.SoftMaxVCNClk} ";

                if (state.IsSoftMinFabClkEnabled == true) commandValues = commandValues + $"--min-fclk-frequency={state.SoftMinFabClk} ";
                if (state.IsSoftMaxFabClkEnabled == true) commandValues = commandValues + $"--max-fclk-frequency={state.SoftMaxFabClk} ";

                if (state.IsSoftMinSoCClkEnabled == true) commandValues = commandValues + $"--min-socclk-frequency={state.SoftMinSoCClk} ";
                if (state.IsSoftMaxSoCClkEnabled == true) commandValues = commandValues + $"--max-socclk-frequency={state.SoftMaxSoCClk} ";

                if (state.BoostIndex > 0)
                {
                    if (state.BoostIndex == 1) commandValues = commandValues + $"--power-saving ";
                    if (state.BoostIndex == 2) commandValues = commandValues + $"--max-performance ";
                }

                if (hardware.Family == Family.RyzenFamily.DragonRange || hardware.Family == Family.RyzenFamily.FireRange || hardware.Family == Family.RyzenFamily.StrixHalo)
                {
                    if (state.IsCCD1Core1Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 0, (int)state.CCD1Core1)} ";
                    if (state.IsCCD1Core2Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 1, (int)state.CCD1Core2)} ";
                    if (state.IsCCD1Core3Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 2, (int)state.CCD1Core3)} ";
                    if (state.IsCCD1Core4Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 3, (int)state.CCD1Core4)} ";
                    if (state.IsCCD1Core5Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 4, (int)state.CCD1Core5)} ";
                    if (state.IsCCD1Core6Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 5, (int)state.CCD1Core6)} ";
                    if (state.IsCCD1Core7Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 6, (int)state.CCD1Core7)} ";
                    if (state.IsCCD1Core8Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 7, (int)state.CCD1Core8)} ";
                    if (state.IsCCD1Core9Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 8, (int)state.CCD1Core9)} ";
                    if (state.IsCCD1Core10Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 9, (int)state.CCD1Core10)} ";
                    if (state.IsCCD1Core11Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 10, (int)state.CCD1Core11)} ";
                    if (state.IsCCD1Core12Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 11, (int)state.CCD1Core12)} ";

                    if (state.IsCCD2Core1Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 0, (int)state.CCD2Core1)} ";
                    if (state.IsCCD2Core2Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 1, (int)state.CCD2Core2)} ";
                    if (state.IsCCD2Core3Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 2, (int)state.CCD2Core3)} ";
                    if (state.IsCCD2Core4Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 3, (int)state.CCD2Core4)} ";
                    if (state.IsCCD2Core5Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 4, (int)state.CCD2Core5)} ";
                    if (state.IsCCD2Core6Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 5, (int)state.CCD2Core6)} ";
                    if (state.IsCCD2Core7Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 6, (int)state.CCD2Core7)} ";
                    if (state.IsCCD2Core8Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 7, (int)state.CCD2Core8)} ";
                    if (state.IsCCD2Core9Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 8, (int)state.CCD2Core9)} ";
                    if (state.IsCCD2Core10Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 9, (int)state.CCD2Core10)} ";
                    if (state.IsCCD2Core11Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 10, (int)state.CCD2Core11)} ";
                    if (state.IsCCD2Core12Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 11, (int)state.CCD2Core12)} ";
                }
                else
                {
                    if (state.IsCCD1Core1Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 0, (int)state.CCD1Core1)} ";
                    if (state.IsCCD1Core2Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 1, (int)state.CCD1Core2)} ";
                    if (state.IsCCD1Core3Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 2, (int)state.CCD1Core3)} ";
                    if (state.IsCCD1Core4Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 3, (int)state.CCD1Core4)} ";
                    if (state.IsCCD1Core5Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 4, (int)state.CCD1Core5)} ";
                    if (state.IsCCD1Core6Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 5, (int)state.CCD1Core6)} ";
                    if (state.IsCCD1Core7Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 6, (int)state.CCD1Core7)} ";
                    if (state.IsCCD1Core8Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 7, (int)state.CCD1Core8)} ";
                    if (state.IsCCD1Core9Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 8, (int)state.CCD1Core9)} ";
                    if (state.IsCCD1Core10Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 9, (int)state.CCD1Core10)} ";
                    if (state.IsCCD1Core11Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 10, (int)state.CCD1Core11)} ";
                    if (state.IsCCD1Core12Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 11, (int)state.CCD1Core12)} ";
                }

                if (state.IsAmdOCEnabled == true && hardware.Family is
                    Family.RyzenFamily.Renoir or
                    Family.RyzenFamily.Lucienne or
                    Family.RyzenFamily.Cezanne_Barcelo or
                    Family.RyzenFamily.Rembrandt or
                    Family.RyzenFamily.Medusa1 or
                    Family.RyzenFamily.Medusa2)
                {
                    commandValues = commandValues + $"--oc-clk={(int)state.AmdCpuClk} ";

                    if (hardware.Family is Family.RyzenFamily.Medusa1 or Family.RyzenFamily.Medusa2)
                    {
                        commandValues = commandValues + $"--oc-volt={(uint)state.AmdVID} ";
                    }
                    else if (hardware.Family != Family.RyzenFamily.Rembrandt)
                    {
                        double voltage = Math.Round((double)state.AmdVID / 1000, 2);
                        commandValues = commandValues + $"--oc-volt={Convert.ToUInt32((1.55 - voltage) / 0.00625)} ";
                    }

                    commandValues = commandValues + "--enable-oc ";
                }

            }

            if (hardware.ProcessorType == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                if (state.IsCPUTempEnabled == true) commandValues = commandValues + $"--tctl-temp={state.CPUTemp} ";
                if (state.IsPPTEnabled == true) commandValues = commandValues + $"--ppt-limit={state.PPT * 1000} ";
                if (state.IsTDCEnabled == true) commandValues = commandValues + $"--tdc-limit={state.TDC * 1000} ";
                if (state.IsEDCEnabled == true) commandValues = commandValues + $"--edc-limit={state.EDC * 1000} ";
                if (state.IsPBOScalerEnabled == true) commandValues = commandValues + $"--pbo-scalar={state.PBOScaler * 100} ";

                if (state.IsAllCOEnabled == true)
                {
                    if (state.AllCO >= 0) commandValues = commandValues + $"--set-coall={state.AllCO} ";
                    if (state.AllCO < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)state.AllCO))} ";
                }

                if (state.IsGfxCOEnabled == true)
                {
                    if (state.GfxCO >= 0) commandValues = commandValues + $"--set-cogfx={state.GfxCO} ";
                    if (state.GfxCO < 0) commandValues = commandValues + $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)state.GfxCO))} ";
                }

                if (state.IsCCD1Core1Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 0, (int)state.CCD1Core1)} ";
                if (state.IsCCD1Core2Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 1, (int)state.CCD1Core2)} ";
                if (state.IsCCD1Core3Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 2, (int)state.CCD1Core3)} ";
                if (state.IsCCD1Core4Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 3, (int)state.CCD1Core4)} ";
                if (state.IsCCD1Core5Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 4, (int)state.CCD1Core5)} ";
                if (state.IsCCD1Core6Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 5, (int)state.CCD1Core6)} ";
                if (state.IsCCD1Core7Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 6, (int)state.CCD1Core7)} ";
                if (state.IsCCD1Core8Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 7, (int)state.CCD1Core8)} ";
                if (state.IsCCD1Core9Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 8, (int)state.CCD1Core9)} ";
                if (state.IsCCD1Core10Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 9, (int)state.CCD1Core10)} ";
                if (state.IsCCD1Core11Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 10, (int)state.CCD1Core11)} ";
                if (state.IsCCD1Core12Enabled == true) commandValues += $"--set-coper={BuildCoperArg(0, 11, (int)state.CCD1Core12)} ";

                if (state.IsCCD2Core1Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 0, (int)state.CCD2Core1)} ";
                if (state.IsCCD2Core2Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 1, (int)state.CCD2Core2)} ";
                if (state.IsCCD2Core3Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 2, (int)state.CCD2Core3)} ";
                if (state.IsCCD2Core4Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 3, (int)state.CCD2Core4)} ";
                if (state.IsCCD2Core5Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 4, (int)state.CCD2Core5)} ";
                if (state.IsCCD2Core6Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 5, (int)state.CCD2Core6)} ";
                if (state.IsCCD2Core7Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 6, (int)state.CCD2Core7)} ";
                if (state.IsCCD2Core8Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 7, (int)state.CCD2Core8)} ";
                if (state.IsCCD2Core9Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 8, (int)state.CCD2Core9)} ";
                if (state.IsCCD2Core10Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 9, (int)state.CCD2Core10)} ";
                if (state.IsCCD2Core11Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 10, (int)state.CCD2Core11)} ";
                if (state.IsCCD2Core12Enabled == true) commandValues += $"--set-coper={BuildCoperArg(1, 11, (int)state.CCD2Core12)} ";

                if (state.IsAmdOCEnabled == true)
                {
                    double vid = 0;

                    vid = ((double)state.AmdVID - 1125) / 5 + 1200;
                    commandValues = commandValues + $"--oc-clk={(int)state.AmdCpuClk} --oc-clk={(int)state.AmdCpuClk} ";

                    if (hardware.Family == Family.RyzenFamily.OlympicRidge)
                    {
                        commandValues = commandValues + $"--oc-volt={(uint)state.AmdVID} --oc-volt={(uint)state.AmdVID} ";
                    }
                    else if (hardware.Family >= Family.RyzenFamily.Rembrandt)
                    {
                        vid = ((double)state.AmdVID - 1125) / 5 + 1200;
                        commandValues = commandValues + $"--oc-volt={vid} --oc-volt={vid} ";
                    }
                    else
                    {
                        vid = Math.Round((double)state.AmdVID / 1000, 2);
                        commandValues = commandValues + $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} ";
                    }

                    commandValues = commandValues + $"--enable-oc --enable-oc ";
                }
            }

            if (hardware.ProcessorType == Family.ProcessorType.Intel)
            {
                if (state.IsIntelRatioCoreEnabled == true)
                {
                    commandValues = commandValues + $"--intel-ratio=";
                    var intelRatioControls = new[] { state.IntelRatioC1, state.IntelRatioC2, state.IntelRatioC3, state.IntelRatioC4, state.IntelRatioC5, state.IntelRatioC6, state.IntelRatioC7, state.IntelRatioC8 };
                    int core = 0;
                    foreach(int clock in new int[hardware.ClockRatioCount])
                    {
                        if (core < intelRatioControls.Length)
                        {
                            if (core == hardware.ClockRatioCount -1) commandValues = commandValues + $"{intelRatioControls[core]} ";
                            else commandValues = commandValues + $"{intelRatioControls[core]}-";
                        }
                        core++;
                    }
                }
                if (state.IsIntelPL1Enabled == true || state.IsIntelPL2Enabled == true)
                    commandValues = commandValues + $"--intel-pl={(int)state.IntelPL1},{Math.Max((int)state.IntelPL1 + 2, (int)state.IntelPL2)} ";
                if (state.IsIntelUVEnabled == true) commandValues = commandValues + $"--intel-volt-cpu={state.IntelCoreUV} --intel-volt-gpu={state.IntelGfxUV} --intel-volt-cache={state.IntelCacheUV} --intel-volt-sa={state.IntelSAUV} ";
                if (state.IsIntelBalEnabled == true) commandValues = commandValues + $"--intel-bal-cpu={state.IntelCpuBal} --intel-bal-gpu={state.IntelGpuBal} ";
                if (state.IsAPUiGPUClkEnabled == true) commandValues = commandValues + $"--intel-gpu={state.APUiGPUClk} ";

            }

            if (state.IsRadeonGraphEnabled == true)
            {
                if (state.IsAntiLagEnabled == true) commandValues = commandValues + $"--ADLX-Lag=0-true --ADLX-Lag=1-true ";
                else commandValues = commandValues + $"--ADLX-Lag=0-false --ADLX-Lag=1-false ";

                if (state.IsRSREnabled == true) commandValues = commandValues + $"--ADLX-RSR=true-{(int)state.RSR} ";
                else commandValues = commandValues + $"--ADLX-RSR=false-{(int)state.RSR} ";

                if (state.IsBoostEnabled == true) commandValues = commandValues + $"--ADLX-Boost=0-true-{(int)state.Boost} --ADLX-Boost=1-true-{(int)state.Boost} ";
                else commandValues = commandValues + $"--ADLX-Boost=0-false-{(int)state.Boost} --ADLX-Boost=1-false-{(int)state.Boost} ";

                if (state.IsImageSharpEnabled == true) commandValues = commandValues + $"--ADLX-ImageSharp=0-true-{(int)state.ImageSharp} --ADLX-ImageSharp=1-true-{(int)state.ImageSharp} ";
                else commandValues = commandValues + $"--ADLX-ImageSharp=0-false-{(int)state.ImageSharp} --ADLX-ImageSharp=1-false-{(int)state.ImageSharp} ";

                if (state.IsSyncEnabled == true) commandValues = commandValues + $"--ADLX-Sync=0-true --ADLX-Sync=1-true ";
                else commandValues = commandValues + $"--ADLX-Sync=0-false --ADLX-Sync=1-false ";
            }

            if (state.IsNVEnabled == true) commandValues = commandValues + $"--NVIDIA-Clocks={state.NVMaxCore}-{state.NVCore}-{state.NVMem}-{state.NVPower} ";

            if (state.CcdAffinityVisibility == Visibility.Visible) commandValues = commandValues + $"--CCD-Affinity={state.CcdAffinityIndex} ";

            return commandValues;
        }
        private static uint BuildCoperArg(int ccd, int core, int offset)
        {
            if (SMUCommands.UseHsmp)
            {
                int apicId = ((ccd << 4) | core) << 1;
                ushort margin = unchecked((ushort)(short)Math.Clamp(offset, short.MinValue, short.MaxValue));
                return ((uint)apicId << 16) | margin;
            }

            int magnitude = Math.Min(Math.Abs(offset), 0xFFFFF);

            uint encoded20 =
                offset < 0
                    ? (uint)((0x100000 - magnitude) & 0xFFFFF)
                    : (uint)(magnitude & 0xFFFFF);

            uint prefix = (uint)((((ccd << 4) | (core / 8 & 15)) << 4 | (core % 8 & 15)) << 20);
            return prefix | encoded20;
        }
}
