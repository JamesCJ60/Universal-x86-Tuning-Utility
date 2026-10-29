using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CpuAffinityUtility;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.ASUS;
using Universal_x86_Tuning_Utility.Scripts.GPUs.NVIDIA;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class CustomPresetsViewModel : PageViewModel, ICustomPresetCommandState
{
        [ObservableProperty]
        private ObservableCollection<string> _powerPresetOptions = new();

        [ObservableProperty]
        private string _selectedPresetName = "";

        [ObservableProperty]
        private string _presetNameText = "";

        [ObservableProperty]
        private Visibility _undoVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility _amdApuThermalVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isAPUTempEnabled = false;

        [ObservableProperty]
        private double _aPUTemp = 10;

        [ObservableProperty]
        private bool _isAPUSkinTempEnabled = false;

        [ObservableProperty]
        private double _aPUSkinTemp = 8;

        [ObservableProperty]
        private Visibility _amdCpuThermalVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isCPUTempEnabled = false;

        [ObservableProperty]
        private double _cPUTemp = 10;

        [ObservableProperty]
        private Visibility _amdApuCPUVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isSTAPMPowEnabled = false;

        [ObservableProperty]
        private double _sTAPMPow = 5;

        [ObservableProperty]
        private bool _isSlowPowEnabled = false;

        [ObservableProperty]
        private double _slowPow = 5;

        [ObservableProperty]
        private bool _isSlowTimeEnabled = false;

        [ObservableProperty]
        private double _slowTime = 2;

        [ObservableProperty]
        private bool _isFastPowEnabled = false;

        [ObservableProperty]
        private double _fastPow = 5;

        [ObservableProperty]
        private bool _isFastTimeEnabled = false;

        [ObservableProperty]
        private double _fastTime = 2;

        [ObservableProperty]
        private Visibility _amdCPUVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isPPTEnabled = false;

        [ObservableProperty]
        private double _pPT = 8;

        [ObservableProperty]
        private bool _isEDCEnabled = false;

        [ObservableProperty]
        private double _eDC = 8;

        [ObservableProperty]
        private bool _isTDCEnabled = false;

        [ObservableProperty]
        private double _tDC = 8;

        [ObservableProperty]
        private Visibility _intelCPUVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isIntelPL1Enabled = false;

        [ObservableProperty]
        private double _intelPL1 = 8;

        [ObservableProperty]
        private bool _isIntelPL2Enabled = false;

        [ObservableProperty]
        private double _intelPL2 = 8;

        [ObservableProperty]
        private Visibility _amdApuVRMVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isCpuVrmTdcEnabled = false;

        [ObservableProperty]
        private double _cpuVrmTdc = 8;

        [ObservableProperty]
        private bool _isCpuVrmEdcEnabled = false;

        [ObservableProperty]
        private double _cpuVrmEdc = 8;

        [ObservableProperty]
        private bool _isSocVrmTdcEnabled = false;

        [ObservableProperty]
        private double _socVrmTdc = 8;

        [ObservableProperty]
        private bool _isSocVrmEdcEnabled = false;

        [ObservableProperty]
        private double _socVrmEdc = 8;

        [ObservableProperty]
        private bool _isGfxVrmTdcEnabled = false;

        [ObservableProperty]
        private double _gfxVrmTdc = 8;

        [ObservableProperty]
        private bool _isGfxVrmEdcEnabled = false;

        [ObservableProperty]
        private double _gfxVrmEdc = 8;

        [ObservableProperty]
        private Visibility _amdApuiGPUClkVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isAPUiGPUClkEnabled = false;

        [ObservableProperty]
        private double _aPUiGPUClkMinimum = 200;

        [ObservableProperty]
        private double _aPUiGPUClk = 200;

        [ObservableProperty]
        private Visibility _amdSoftClkVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool _isSoftMiniGPUClkEnabled = false;

        [ObservableProperty]
        private double _softMiniGPUClk = 400;

        [ObservableProperty]
        private bool _isSoftMaxiGPUClkEnabled = false;

        [ObservableProperty]
        private double _softMaxiGPUClk = 400;

        [ObservableProperty]
        private bool _isSoftMinCPUClkEnabled = false;

        [ObservableProperty]
        private double _softMinCPUClk = 400;

        [ObservableProperty]
        private bool _isSoftMaxCPUClkEnabled = false;

        [ObservableProperty]
        private double _softMaxCPUClk = 400;

        [ObservableProperty]
        private bool _isSoftMinDataClkEnabled = false;

        [ObservableProperty]
        private double _softMinDataClk = 400;

        [ObservableProperty]
        private bool _isSoftMaxDataClkEnabled = false;

        [ObservableProperty]
        private double _softMaxDataClk = 400;

        [ObservableProperty]
        private bool _isSoftMinFabClkEnabled = false;

        [ObservableProperty]
        private double _softMinFabClk = 400;

        [ObservableProperty]
        private bool _isSoftMaxFabClkEnabled = false;

        [ObservableProperty]
        private double _softMaxFabClk = 400;

        [ObservableProperty]
        private bool _isSoftMinSoCClkEnabled = false;

        [ObservableProperty]
        private double _softMinSoCClk = 400;

        [ObservableProperty]
        private bool _isSoftMaxSoCClkEnabled = false;

        [ObservableProperty]
        private double _softMaxSoCClk = 400;

        [ObservableProperty]
        private bool _isSoftMinVCNClkEnabled = false;

        [ObservableProperty]
        private double _softMinVCNClk = 400;

        [ObservableProperty]
        private bool _isSoftMaxVCNClkEnabled = false;

        [ObservableProperty]
        private double _softMaxVCNClk = 400;

        [ObservableProperty]
        private bool _isUXTUSREnabled = false;

        [ObservableProperty]
        private bool _isVSyncEnabled = false;

        [ObservableProperty]
        private bool _isAutoCapEnabled = false;

        [ObservableProperty]
        private double _sharp = 5;

        [ObservableProperty]
        private int _resScaleIndex = 0;

        [ObservableProperty]
        private Visibility _aDLXVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isRadeonGraphEnabled = false;

        [ObservableProperty]
        private bool _isAntiLagEnabled = false;

        [ObservableProperty]
        private bool _isRSREnabled = false;

        [ObservableProperty]
        private double _rSR = 5;

        [ObservableProperty]
        private bool _isBoostEnabled = false;

        [ObservableProperty]
        private double _boost = 50;

        [ObservableProperty]
        private bool _isImageSharpEnabled = false;

        [ObservableProperty]
        private double _imageSharp = 10;

        [ObservableProperty]
        private bool _isSyncEnabled = false;

        [ObservableProperty]
        private Visibility _nVIDIAVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isNVEnabled = false;

        [ObservableProperty]
        private double _nVMaxCore = 400;

        [ObservableProperty]
        private double _nVCore = 0;

        [ObservableProperty]
        private double _nVMem = 0;

        [ObservableProperty]
        private Visibility _intelBalVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isIntelBalEnabled = false;

        [ObservableProperty]
        private double _intelCpuBal = 0;

        [ObservableProperty]
        private double _intelGpuBal = 0;

        [ObservableProperty]
        private Visibility _intelCoreRatioVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isIntelRatioCoreEnabled = false;

        [ObservableProperty]
        private double _intelRatioC1 = 4;

        [ObservableProperty]
        private double _intelRatioC2 = 4;

        [ObservableProperty]
        private double _intelRatioC3 = 4;

        [ObservableProperty]
        private double _intelRatioC4 = 4;

        [ObservableProperty]
        private double _intelRatioC5 = 4;

        [ObservableProperty]
        private double _intelRatioC6 = 4;

        [ObservableProperty]
        private double _intelRatioC7 = 4;

        [ObservableProperty]
        private double _intelRatioC8 = 4;

        [ObservableProperty]
        private Visibility _intelUVVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isIntelUVEnabled = false;

        [ObservableProperty]
        private double _intelCoreUV = 0;

        [ObservableProperty]
        private double _intelGfxUV = 0;

        [ObservableProperty]
        private double _intelCacheUV = 0;

        [ObservableProperty]
        private double _intelSAUV = 0;

        [ObservableProperty]
        private Visibility _amdCpuTuneVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool _isAmdOCEnabled = false;

        [ObservableProperty]
        private double _amdCpuClk = 400;

        [ObservableProperty]
        private Visibility _amdCpuClkVisibility = Visibility.Visible;

        [ObservableProperty]
        private double _amdVID = 512;

        [ObservableProperty]
        private Visibility _amdPBOVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isPBOScalerEnabled = false;

        [ObservableProperty]
        private double _pBOScaler = 1;

        [ObservableProperty]
        private Visibility _amdCOVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isAllCOEnabled = false;

        [ObservableProperty]
        private double _allCO = 0;

        [ObservableProperty]
        private bool _isGfxCOEnabled = false;

        [ObservableProperty]
        private double _gfxCO = 0;

        [ObservableProperty]
        private Visibility _amdCCD1COVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool _isCCD1Core1Enabled = false;

        [ObservableProperty]
        private double _cCD1Core1 = 0;

        [ObservableProperty]
        private bool _isCCD1Core2Enabled = false;

        [ObservableProperty]
        private double _cCD1Core2 = 0;

        [ObservableProperty]
        private bool _isCCD1Core3Enabled = false;

        [ObservableProperty]
        private double _cCD1Core3 = 0;

        [ObservableProperty]
        private bool _isCCD1Core4Enabled = false;

        [ObservableProperty]
        private double _cCD1Core4 = 0;

        [ObservableProperty]
        private bool _isCCD1Core5Enabled = false;

        [ObservableProperty]
        private double _cCD1Core5 = 0;

        [ObservableProperty]
        private bool _isCCD1Core6Enabled = false;

        [ObservableProperty]
        private double _cCD1Core6 = 0;

        [ObservableProperty]
        private bool _isCCD1Core7Enabled = false;

        [ObservableProperty]
        private double _cCD1Core7 = 0;

        [ObservableProperty]
        private bool _isCCD1Core8Enabled = false;

        [ObservableProperty]
        private double _cCD1Core8 = 0;

        [ObservableProperty]
        private Visibility _amdCCD2COVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool _isCCD2Core1Enabled = false;

        [ObservableProperty]
        private double _cCD2Core1 = 0;

        [ObservableProperty]
        private bool _isCCD2Core2Enabled = false;

        [ObservableProperty]
        private double _cCD2Core2 = 0;

        [ObservableProperty]
        private bool _isCCD2Core3Enabled = false;

        [ObservableProperty]
        private double _cCD2Core3 = 0;

        [ObservableProperty]
        private bool _isCCD2Core4Enabled = false;

        [ObservableProperty]
        private double _cCD2Core4 = 0;

        [ObservableProperty]
        private bool _isCCD2Core5Enabled = false;

        [ObservableProperty]
        private double _cCD2Core5 = 0;

        [ObservableProperty]
        private bool _isCCD2Core6Enabled = false;

        [ObservableProperty]
        private double _cCD2Core6 = 0;

        [ObservableProperty]
        private bool _isCCD2Core7Enabled = false;

        [ObservableProperty]
        private double _cCD2Core7 = 0;

        [ObservableProperty]
        private bool _isCCD2Core8Enabled = false;

        [ObservableProperty]
        private double _cCD2Core8 = 0;

        [ObservableProperty]
        private Visibility _ccdAffinityVisibility = Visibility.Visible;

        [ObservableProperty]
        private int _ccdAffinityIndex = 0;

        [ObservableProperty]
        private Visibility _amdPowerProfileVisibility = Visibility.Visible;

        [ObservableProperty]
        private int _boostIndex = 0;

        [ObservableProperty]
        private Visibility _refreshRateVisibility = Visibility.Visible;

        [ObservableProperty]
        private ObservableCollection<string> _refreshRateOptions = new();

        [ObservableProperty]
        private int _refreshRateIndex = 0;

        [ObservableProperty]
        private Visibility _powerModeVisibility = Visibility.Visible;

        [ObservableProperty]
        private int _powerModeIndex = 0;

        [ObservableProperty]
        private Visibility _asusPowerVisibility = Visibility.Visible;

        [ObservableProperty]
        private int _asusPowerIndex = 0;

        [ObservableProperty]
        private Visibility _asusUltiVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isASUSUltiEnabled = false;

        [ObservableProperty]
        private Visibility _asusEcoVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isASUSEcoEnabled = false;

        [ObservableProperty] private int _powerPresetIndex = 0;
        [ObservableProperty] private double _nVPower = 0;
        [ObservableProperty] private double _nVPowerMaximum = 0;
        [ObservableProperty] private double _nVPowerMinimum = 0;
        [ObservableProperty] private bool _isCCD1Core9Enabled = false;
        [ObservableProperty] private double _cCD1Core9 = 0;
        [ObservableProperty] private bool _isCCD1Core10Enabled = false;
        [ObservableProperty] private double _cCD1Core10 = 0;
        [ObservableProperty] private bool _isCCD1Core11Enabled = false;
        [ObservableProperty] private double _cCD1Core11 = 0;
        [ObservableProperty] private bool _isCCD1Core12Enabled = false;
        [ObservableProperty] private double _cCD1Core12 = 0;
        [ObservableProperty] private bool _isCCD2Core9Enabled = false;
        [ObservableProperty] private double _cCD2Core9 = 0;
        [ObservableProperty] private bool _isCCD2Core10Enabled = false;
        [ObservableProperty] private double _cCD2Core10 = 0;
        [ObservableProperty] private bool _isCCD2Core11Enabled = false;
        [ObservableProperty] private double _cCD2Core11 = 0;
        [ObservableProperty] private bool _isCCD2Core12Enabled = false;
        [ObservableProperty] private double _cCD2Core12 = 0;
        [ObservableProperty] private int _windowsBoostModeIndex = 0;
        [ObservableProperty] private bool _isWindowsMinStateEnabled = false;
        [ObservableProperty] private double _windowsMinState = 0;
        [ObservableProperty] private bool _isWindowsMaxStateEnabled = false;
        [ObservableProperty] private double _windowsMaxState = 1;
        [ObservableProperty] private bool _isWindowsMaxFrequencyEnabled = false;
        [ObservableProperty] private double _windowsMaxFrequency = 100;
        [ObservableProperty] private bool _isWindowsEppEnabled = false;
        [ObservableProperty] private double _windowsEpp = 0;
        [ObservableProperty] private bool _isWindowsCoreParkingEnabled = false;
        [ObservableProperty] private double _windowsCoreParking = 0;
        [ObservableProperty] private bool _isWindowsMaxUnparkedCoresEnabled = false;
        [ObservableProperty] private double _windowsMaxUnparkedCores = 0;

        private readonly IPresetApplicationService _application;
        private readonly IUserInteractionService _interaction;
        private readonly IPresetCommandBuilder _commands;
        public CustomPresetsViewModel(IPresetApplicationService application, IUserInteractionService interaction, IPresetCommandBuilder commands, GpuInventoryService gpuInventory)
        {
            _application = application;
            _interaction = interaction;
            _commands = commands;
            this.gpuInventory = gpuInventory;
        }
        private Preset DefaultAPUPreset = new Preset {
            apuTemp = 95,
            apuSkinTemp = 45,
            apuSTAPMPow = 28,
            apuSTAPMTime = 64,
            apuFastPow = 28,
            apuSlowPow = 28,
            apuSlowTime = 128,
            apuCpuTdc = 64,
            apuCpuEdc = 64,
            apuSocTdc = 64,
            apuSocEdc = 64,
            apuGfxTdc = 64,
            apuGfxEdc = 64,
            apuGfxClk = 1000,

            amdVID = 1200,
            amdClock = 3200,

            nvMaxCoreClk = 4000
        };

        private Preset DefaultAMDDtCPUPreset = new Preset {
            dtCpuTemp = 85,
            dtCpuPPT = 140,
            dtCpuEDC = 160,
            dtCpuTDC = 160,

            amdVID = 1200,
            amdClock = 3200,

            nvMaxCoreClk = 4000
        };

        private Preset DefaultIntelPreset = new Preset {
            IntelPL1 = 35,
            IntelPL2 = 65,

            IntelBalCPU = 9,
            IntelBalGPU = 13,

            nvMaxCoreClk = 4000
        };

        private PresetManager presetManager;
        private readonly GpuInventoryService gpuInventory;
        private bool deferredSetupComplete;
        private bool isUpdatingPresetValues;
        private int radeonGpuCount;
        private int nvidiaGpuCount;

        int[] clockRatio = null;
        protected override async Task InitializeAsync()
        {

            presetManager = new PresetManager(Settings.Default.Path + GetPresetFileName());

            CcdAffinityVisibility = Visibility.Collapsed;

            ADLXVisibility = Visibility.Collapsed;
            NVIDIAVisibility = Visibility.Collapsed;
            RefreshRateVisibility = Visibility.Collapsed;

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                AmdCPUVisibility = Visibility.Collapsed;
                AmdCpuThermalVisibility = Visibility.Collapsed;
                IntelCPUVisibility = Visibility.Collapsed;
                IntelUVVisibility = Visibility.Collapsed;
                IntelBalVisibility = Visibility.Collapsed;
                IntelCoreRatioVisibility = Visibility.Collapsed;

                if(Family.FAM < Family.RyzenFamily.Renoir)
                {
                    AmdCCD1COVisibility = Visibility.Collapsed;
                    AmdCCD2COVisibility = Visibility.Collapsed;
                }
                
                if (Family.FAM is not (
                    Family.RyzenFamily.StrixHalo or
                    Family.RyzenFamily.StrixPoint or
                    Family.RyzenFamily.KrackanPoint or
                    Family.RyzenFamily.PhoenixPoint or
                    Family.RyzenFamily.PhoenixPoint2 or
                    Family.RyzenFamily.Mendocino or
                    Family.RyzenFamily.Rembrandt or
                    Family.RyzenFamily.Medusa1 or
                    Family.RyzenFamily.Medusa2 or
                    Family.RyzenFamily.Lucienne or
                    Family.RyzenFamily.Renoir))
                    AmdApuiGPUClkVisibility = Visibility.Collapsed;
                if (SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery) AmdCpuTuneVisibility = Visibility.Collapsed;

                if (Family.FAM < Family.RyzenFamily.Renoir) AmdSoftClkVisibility = Visibility.Visible;
                
                AmdCOVisibility = Visibility.Visible;

                AmdCCD1COVisibility = AmdCOVisibility;

                if (Family.FAM == Family.RyzenFamily.DragonRange || Family.FAM == Family.RyzenFamily.FireRange || Family.FAM == Family.RyzenFamily.StrixHalo || Family.FAM == Family.RyzenFamily.KrackanPoint) if (Family.CPUName.Contains("Ryzen 9") || Family.CPUName.Contains("395") || Family.CPUName.Contains("390")) AmdCCD2COVisibility = AmdCOVisibility;

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = presetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    PowerPresetOptions.Add(presetName);
                }

                if (Family.FAM is Family.RyzenFamily.DragonRange or
                    Family.RyzenFamily.FireRange or
                    Family.RyzenFamily.StrixHalo or
                    Family.RyzenFamily.StrixPoint or
                    Family.RyzenFamily.KrackanPoint or
                    Family.RyzenFamily.KrackanPoint2 or
                    Family.RyzenFamily.Medusa1 or
                    Family.RyzenFamily.Medusa2)
                {
                   if((int)CpuAffinityManager.GetActiveProcessorCount(0xFFFF) > 16) CcdAffinityVisibility = Visibility.Visible;
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                AmdApuCPUVisibility = Visibility.Collapsed;
                AmdApuThermalVisibility = Visibility.Collapsed;
                AmdApuVRMVisibility = Visibility.Collapsed;
                IntelCPUVisibility = Visibility.Collapsed;
                IntelUVVisibility = Visibility.Collapsed;
                IntelBalVisibility = Visibility.Collapsed;
                IntelCoreRatioVisibility = Visibility.Collapsed;

                AmdApuiGPUClkVisibility = Visibility.Collapsed;
                AmdPowerProfileVisibility = Visibility.Collapsed;

                if (Family.FAM < Family.RyzenFamily.Vermeer) AmdCOVisibility = Visibility.Collapsed;
                AmdCCD1COVisibility = AmdCOVisibility;
                if (Family.CPUName.Contains("Ryzen 9")) AmdCCD2COVisibility = AmdCOVisibility;

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = presetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    PowerPresetOptions.Add(presetName);
                }

                if (Environment.ProcessorCount > 16) CcdAffinityVisibility = Visibility.Visible;
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                AmdCPUVisibility = Visibility.Collapsed;
                AmdCpuThermalVisibility = Visibility.Collapsed;
                AmdApuCPUVisibility = Visibility.Collapsed;
                AmdApuThermalVisibility = Visibility.Collapsed;
                AmdApuVRMVisibility = Visibility.Collapsed;
                AmdPowerProfileVisibility = Visibility.Collapsed;
                //AmdApuiGPUClkVisibility = Visibility.Collapsed;

                APUiGPUClkMinimum = 100;
                APUiGPUClkMinimum = 100;

                AmdCpuClkVisibility = Visibility.Collapsed;
                AmdPBOVisibility = Visibility.Collapsed;
                AmdCpuTuneVisibility = Visibility.Collapsed;
                AmdCOVisibility = Visibility.Collapsed;

                clockRatio = new int[8];

                IntelRatioC1 = IntelRatioC2 = IntelRatioC3 = IntelRatioC4 = IntelRatioC5 = IntelRatioC6 = IntelRatioC7 = IntelRatioC8 = 0;
                // Get the names of all the stored presets
                IEnumerable<string> presetNames = presetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    PowerPresetOptions.Add(presetName);
                }
            }


            if (!Settings.Default.isASUS)
            {
                AsusPowerVisibility = Visibility.Collapsed;
                AsusUltiVisibility = Visibility.Collapsed;
                AsusEcoVisibility = Visibility.Collapsed;
            }

            if (Family.FAM == Family.RyzenFamily.Renoir || Family.FAM == Family.RyzenFamily.Lucienne || Family.FAM == Family.RyzenFamily.Mendocino || Family.FAM == Family.RyzenFamily.Rembrandt || Family.FAM == Family.RyzenFamily.PhoenixPoint || Family.FAM == Family.RyzenFamily.PhoenixPoint2 || Family.FAM == Family.RyzenFamily.HawkPoint) AmdApuiGPUClkVisibility = Visibility.Visible;

            await InitializeHardwareAsync();
        }


        [RelayCommand]
        private async Task ApplyPresetAsync()
        {
            string commandValues = "";

            commandValues = getCommandValues();

            if (commandValues != "" && commandValues != null)
            {
                await _application.ApplyAsync(commandValues, appliedName: GetSelectedPresetName());
                _interaction.Notify("Preset Applied", $"Your custom preset settings have been applied!");
            }

            Settings.Default.CommandString = commandValues;
            Settings.Default.Save();

        }

        [RelayCommand]
        private void SavePreset()
        {
            if (!PresetNameText.Contains("PM -"))
            {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    if (PresetNameText != "" && PresetNameText != null)
                    {
                        // Save a preset
                        Preset preset = new Preset
                        {
                            apuTemp = (int)APUTemp,
                            apuSkinTemp = (int)APUSkinTemp,
                            apuSTAPMPow = (int)STAPMPow,
                            apuSTAPMTime = (int)FastTime,
                            apuFastPow = (int)FastPow,
                            apuSlowPow = (int)SlowPow,
                            apuSlowTime = (int)SlowTime,

                            apuCpuTdc = (int)CpuVrmTdc,
                            apuCpuEdc = (int)CpuVrmEdc,
                            apuSocTdc = (int)SocVrmTdc,
                            apuSocEdc = (int)SocVrmEdc,
                            apuGfxTdc = (int)GfxVrmTdc,
                            apuGfxEdc = (int)GfxVrmEdc,

                            apuGfxClk = (int)APUiGPUClk,

                            pboScalar = (int)PBOScaler,
                            coAllCore = (int)AllCO,

                            coGfx = (int)GfxCO,
                            isCoGfx = (bool)IsGfxCOEnabled,

                            boostProfile = (int)BoostIndex,

                            rsr = (int)RSR,
                            boost = (int)Boost,
                            imageSharp = (int)ImageSharp,
                            isRadeonGraphics = (bool)IsRadeonGraphEnabled,
                            isRSR = (bool)IsRSREnabled,
                            isBoost = (bool)IsBoostEnabled,
                            isAntiLag = (bool)IsAntiLagEnabled,
                            isImageSharp = (bool)IsImageSharpEnabled,
                            isSync = (bool)IsSyncEnabled,

                            ccd1Core1 = (int)CCD1Core1,
                            ccd1Core2 = (int)CCD1Core2,
                            ccd1Core3 = (int)CCD1Core3,
                            ccd1Core4 = (int)CCD1Core4,
                            ccd1Core5 = (int)CCD1Core5,
                            ccd1Core6 = (int)CCD1Core6,
                            ccd1Core7 = (int)CCD1Core7,
                            ccd1Core8 = (int)CCD1Core8,
                            ccd1Core9 = (int)CCD1Core9,
                            ccd1Core10 = (int)CCD1Core10,
                            ccd1Core11 = (int)CCD1Core11,
                            ccd1Core12 = (int)CCD1Core12,

                            ccd2Core1 = (int)CCD2Core1,
                            ccd2Core2 = (int)CCD2Core2,
                            ccd2Core3 = (int)CCD2Core3,
                            ccd2Core4 = (int)CCD2Core4,
                            ccd2Core5 = (int)CCD2Core5,
                            ccd2Core6 = (int)CCD2Core6,
                            ccd2Core7 = (int)CCD2Core7,
                            ccd2Core8 = (int)CCD2Core8,
                            ccd2Core9 = (int)CCD2Core9,
                            ccd2Core10 = (int)CCD2Core10,
                            ccd2Core11 = (int)CCD2Core11,
                            ccd2Core12 = (int)CCD2Core12,

                            commandValue = getCommandValues(),

                            isApuTemp = (bool)IsAPUTempEnabled,
                            isApuSkinTemp = (bool)IsAPUSkinTempEnabled,
                            isApuSTAPMPow = (bool)IsSTAPMPowEnabled,
                            isApuSlowPow = (bool)IsSlowPowEnabled,
                            isApuSlowTime = (bool)IsSlowTimeEnabled,
                            isApuFastPow = (bool)IsFastPowEnabled,
                            isApuSTAPMTime = (bool)IsFastTimeEnabled,

                            isApuCpuTdc = (bool)IsCpuVrmTdcEnabled,
                            isApuCpuEdc = (bool)IsCpuVrmEdcEnabled,
                            isApuSocTdc = (bool)IsSocVrmTdcEnabled,
                            isApuSocEdc = (bool)IsSocVrmEdcEnabled,
                            isApuGfxTdc = (bool)IsGfxVrmTdcEnabled,
                            isApuGfxEdc = (bool)IsGfxVrmEdcEnabled,

                            isApuGfxClk = (bool)IsAPUiGPUClkEnabled,

                            isPboScalar = (bool)IsPBOScalerEnabled,
                            isCoAllCore = (bool)IsAllCOEnabled,

                            IsCCD1Core1 = (bool)IsCCD1Core1Enabled,
                            IsCCD1Core2 = (bool)IsCCD1Core2Enabled,
                            IsCCD1Core3 = (bool)IsCCD1Core3Enabled,
                            IsCCD1Core4 = (bool)IsCCD1Core4Enabled,
                            IsCCD1Core5 = (bool)IsCCD1Core5Enabled,
                            IsCCD1Core6 = (bool)IsCCD1Core6Enabled,
                            IsCCD1Core7 = (bool)IsCCD1Core7Enabled,
                            IsCCD1Core8 = (bool)IsCCD1Core8Enabled,
                            IsCCD1Core9 = (bool)IsCCD1Core9Enabled,
                            IsCCD1Core10 = (bool)IsCCD1Core10Enabled,
                            IsCCD1Core11 = (bool)IsCCD1Core11Enabled,
                            IsCCD1Core12 = (bool)IsCCD1Core12Enabled,

                            IsCCD2Core1 = (bool)IsCCD2Core1Enabled,
                            IsCCD2Core2 = (bool)IsCCD2Core2Enabled,
                            IsCCD2Core3 = (bool)IsCCD2Core3Enabled,
                            IsCCD2Core4 = (bool)IsCCD2Core4Enabled,
                            IsCCD2Core5 = (bool)IsCCD2Core5Enabled,
                            IsCCD2Core6 = (bool)IsCCD2Core6Enabled,
                            IsCCD2Core7 = (bool)IsCCD2Core7Enabled,
                            IsCCD2Core8 = (bool)IsCCD2Core8Enabled,
                            IsCCD2Core9 = (bool)IsCCD2Core9Enabled,
                            IsCCD2Core10 = (bool)IsCCD2Core10Enabled,
                            IsCCD2Core11 = (bool)IsCCD2Core11Enabled,
                            IsCCD2Core12 = (bool)IsCCD2Core12Enabled,

                            isNVIDIA = (bool)IsNVEnabled,
                            nvMaxCoreClk = (int)NVMaxCore,
                            nvCoreClk = (int)NVCore,
                            nvMemClk = (int)NVMem,
                            nvPower = (int)NVPower,

                            IsAmdOC = (bool)IsAmdOCEnabled,
                            amdClock = (int)AmdCpuClk,
                            amdVID = (int)AmdVID,

                            softMiniGPUClk = (int)SoftMiniGPUClk,
                            softMinCPUClk = (int)SoftMinCPUClk,
                            softMinFabClk = (int)SoftMinFabClk,
                            softMinDataClk = (int)SoftMinDataClk,
                            softMinSoCClk = (int)SoftMinSoCClk,
                            softMinVCNClk = (int)SoftMinVCNClk,

                            softMaxiGPUClk = (int)SoftMaxiGPUClk,
                            softMaxCPUClk = (int)SoftMaxCPUClk,
                            softMaxFabClk = (int)SoftMaxFabClk,
                            softMaxDataClk = (int)SoftMaxDataClk,
                            softMaxSoCClk = (int)SoftMaxSoCClk,
                            softMaxVCNClk = (int)SoftMaxVCNClk,

                            isSoftMiniGPUClk = (bool)IsSoftMiniGPUClkEnabled,
                            isSoftMinCPUClk = (bool)IsSoftMinCPUClkEnabled,
                            isSoftMinFabClk = (bool)IsSoftMinFabClkEnabled,
                            isSoftMinDataClk = (bool)IsSoftMinDataClkEnabled,
                            isSoftMinSoCClk = (bool)IsSoftMinSoCClkEnabled,
                            isSoftMinVCNClk = (bool)IsSoftMinVCNClkEnabled,

                            isSoftMaxiGPUClk = (bool)IsSoftMaxiGPUClkEnabled,
                            isSoftMaxCPUClk = (bool)IsSoftMaxCPUClkEnabled,
                            isSoftMaxFabClk = (bool)IsSoftMaxFabClkEnabled,
                            isSoftMaxDataClk = (bool)IsSoftMaxDataClkEnabled,
                            isSoftMaxSoCClk = (bool)IsSoftMaxSoCClkEnabled,
                            isSoftMaxVCNClk = (bool)IsSoftMaxVCNClkEnabled,

                            asusGPUUlti = (bool)IsASUSUltiEnabled,
                            asusiGPU = (bool)IsASUSEcoEnabled,
                            asusPowerProfile = (int)AsusPowerIndex,

                            displayHz = (int)RefreshRateIndex,

                            isMag = (bool)IsUXTUSREnabled,
                            isVsync = (bool)IsVSyncEnabled,
                            isRecap = (bool)IsAutoCapEnabled,
                            Sharpness = (int)Sharp,
                            ResScaleIndex = (int)ResScaleIndex,

                            powerMode = (int)PowerModeIndex,
                            windowsBoostMode = WindowsBoostModeIndex,
                            isWindowsMinState = (bool)IsWindowsMinStateEnabled,
                            windowsMinState = (int)WindowsMinState,
                            isWindowsMaxState = (bool)IsWindowsMaxStateEnabled,
                            windowsMaxState = (int)WindowsMaxState,
                            isWindowsMaxFrequency = (bool)IsWindowsMaxFrequencyEnabled,
                            windowsMaxFrequency = (int)WindowsMaxFrequency,
                            isWindowsEpp = (bool)IsWindowsEppEnabled,
                            windowsEpp = (int)WindowsEpp,
                            isWindowsCoreParking = (bool)IsWindowsCoreParkingEnabled,
                            windowsCoreParking = (int)WindowsCoreParking,
                            isWindowsMaxUnparkedCores = (bool)IsWindowsMaxUnparkedCoresEnabled,
                            windowsMaxUnparkedCores = (int)WindowsMaxUnparkedCores,
                            ccdAffinity = (int)CcdAffinityIndex,
                        };
                        presetManager.SavePreset(PresetNameText, preset);
                        if ( !PowerPresetOptions.Contains(PresetNameText) )
                            PowerPresetOptions.Add(PresetNameText);

                        SelectedPresetName = PresetNameText;
                        Settings.Default.cstmPreset = PresetNameText;
                        Settings.Default.Save();
                        _interaction.Notify("Preset Saved", $"Your preset {PresetNameText} has been saved successfully!");

                    }
                }

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    if (PresetNameText != "" && PresetNameText != null)
                    {
                        // Save a preset
                        Preset preset = new Preset
                        {
                            dtCpuTemp = (int)CPUTemp,
                            dtCpuPPT = (int)PPT,
                            dtCpuTDC = (int)TDC,
                            dtCpuEDC = (int)EDC,
                            pboScalar = (int)PBOScaler,
                            coAllCore = (int)AllCO,

                            boostProfile = (int)BoostIndex,

                            rsr = (int)RSR,
                            boost = (int)Boost,
                            imageSharp = (int)ImageSharp,
                            isRadeonGraphics = (bool)IsRadeonGraphEnabled,
                            isRSR = (bool)IsRSREnabled,
                            isBoost = (bool)IsBoostEnabled,
                            isAntiLag = (bool)IsAntiLagEnabled,
                            isImageSharp = (bool)IsImageSharpEnabled,
                            isSync = (bool)IsSyncEnabled,

                            commandValue = getCommandValues(),


                            isDtCpuTemp = (bool)IsCPUTempEnabled,
                            isDtCpuPPT = (bool)IsPPTEnabled,
                            isDtCpuTDC = (bool)IsTDCEnabled,
                            isDtCpuEDC = (bool)IsEDCEnabled,
                            isPboScalar = (bool)IsPBOScalerEnabled,
                            isCoAllCore = (bool)IsAllCOEnabled,

                            coGfx = (int)GfxCO,
                            isCoGfx = (bool)IsGfxCOEnabled,

                            isNVIDIA = (bool)IsNVEnabled,
                            nvMaxCoreClk = (int)NVMaxCore,
                            nvCoreClk = (int)NVCore,
                            nvMemClk = (int)NVMem,
                            nvPower = (int)NVPower,

                            ccd1Core1 = (int)CCD1Core1,
                            ccd1Core2 = (int)CCD1Core2,
                            ccd1Core3 = (int)CCD1Core3,
                            ccd1Core4 = (int)CCD1Core4,
                            ccd1Core5 = (int)CCD1Core5,
                            ccd1Core6 = (int)CCD1Core6,
                            ccd1Core7 = (int)CCD1Core7,
                            ccd1Core8 = (int)CCD1Core8,
                            ccd1Core9 = (int)CCD1Core9,
                            ccd1Core10 = (int)CCD1Core10,
                            ccd1Core11 = (int)CCD1Core11,
                            ccd1Core12 = (int)CCD1Core12,

                            ccd2Core1 = (int)CCD2Core1,
                            ccd2Core2 = (int)CCD2Core2,
                            ccd2Core3 = (int)CCD2Core3,
                            ccd2Core4 = (int)CCD2Core4,
                            ccd2Core5 = (int)CCD2Core5,
                            ccd2Core6 = (int)CCD2Core6,
                            ccd2Core7 = (int)CCD2Core7,
                            ccd2Core8 = (int)CCD2Core8,
                            ccd2Core9 = (int)CCD2Core9,
                            ccd2Core10 = (int)CCD2Core10,
                            ccd2Core11 = (int)CCD2Core11,
                            ccd2Core12 = (int)CCD2Core12,

                            IsCCD1Core1 = (bool)IsCCD1Core1Enabled,
                            IsCCD1Core2 = (bool)IsCCD1Core2Enabled,
                            IsCCD1Core3 = (bool)IsCCD1Core3Enabled,
                            IsCCD1Core4 = (bool)IsCCD1Core4Enabled,
                            IsCCD1Core5 = (bool)IsCCD1Core5Enabled,
                            IsCCD1Core6 = (bool)IsCCD1Core6Enabled,
                            IsCCD1Core7 = (bool)IsCCD1Core7Enabled,
                            IsCCD1Core8 = (bool)IsCCD1Core8Enabled,
                            IsCCD1Core9 = (bool)IsCCD1Core9Enabled,
                            IsCCD1Core10 = (bool)IsCCD1Core10Enabled,
                            IsCCD1Core11 = (bool)IsCCD1Core11Enabled,
                            IsCCD1Core12 = (bool)IsCCD1Core12Enabled,

                            IsCCD2Core1 = (bool)IsCCD2Core1Enabled,
                            IsCCD2Core2 = (bool)IsCCD2Core2Enabled,
                            IsCCD2Core3 = (bool)IsCCD2Core3Enabled,
                            IsCCD2Core4 = (bool)IsCCD2Core4Enabled,
                            IsCCD2Core5 = (bool)IsCCD2Core5Enabled,
                            IsCCD2Core6 = (bool)IsCCD2Core6Enabled,
                            IsCCD2Core7 = (bool)IsCCD2Core7Enabled,
                            IsCCD2Core8 = (bool)IsCCD2Core8Enabled,
                            IsCCD2Core9 = (bool)IsCCD2Core9Enabled,
                            IsCCD2Core10 = (bool)IsCCD2Core10Enabled,
                            IsCCD2Core11 = (bool)IsCCD2Core11Enabled,
                            IsCCD2Core12 = (bool)IsCCD2Core12Enabled,

                            IsAmdOC = (bool)IsAmdOCEnabled,
                            amdClock = (int)AmdCpuClk,
                            amdVID = (int)AmdVID,

                            asusGPUUlti = (bool)IsASUSUltiEnabled,
                            asusiGPU = (bool)IsASUSEcoEnabled,
                            asusPowerProfile = (int)AsusPowerIndex,

                            displayHz = (int)RefreshRateIndex,

                            isMag = (bool)IsUXTUSREnabled,
                            isVsync = (bool)IsVSyncEnabled,
                            isRecap = (bool)IsAutoCapEnabled,
                            Sharpness = (int)Sharp,
                            ResScaleIndex = (int)ResScaleIndex,

                            powerMode = (int)PowerModeIndex,
                            windowsBoostMode = WindowsBoostModeIndex,
                            isWindowsMinState = (bool)IsWindowsMinStateEnabled,
                            windowsMinState = (int)WindowsMinState,
                            isWindowsMaxState = (bool)IsWindowsMaxStateEnabled,
                            windowsMaxState = (int)WindowsMaxState,
                            isWindowsMaxFrequency = (bool)IsWindowsMaxFrequencyEnabled,
                            windowsMaxFrequency = (int)WindowsMaxFrequency,
                            isWindowsEpp = (bool)IsWindowsEppEnabled,
                            windowsEpp = (int)WindowsEpp,
                            isWindowsCoreParking = (bool)IsWindowsCoreParkingEnabled,
                            windowsCoreParking = (int)WindowsCoreParking,
                            isWindowsMaxUnparkedCores = (bool)IsWindowsMaxUnparkedCoresEnabled,
                            windowsMaxUnparkedCores = (int)WindowsMaxUnparkedCores,
                            ccdAffinity = (int)CcdAffinityIndex,
                        };
                        presetManager.SavePreset(PresetNameText, preset);
                        if (!PowerPresetOptions.Contains(PresetNameText))
                            PowerPresetOptions.Add(PresetNameText);

                        SelectedPresetName = PresetNameText;
                        Settings.Default.cstmPreset = PresetNameText;
                        Settings.Default.Save();
                        _interaction.Notify("Preset Saved", $"Your preset {PresetNameText} has been saved successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    if (PresetNameText != "" && PresetNameText != null)
                    {
                        // Save a preset
                        Preset preset = new Preset
                        {
                            IntelPL1 = (int)IntelPL1,
                            IntelPL2 = (int)IntelPL2,
                            IntelVoltCPU = (int)IntelCoreUV,
                            IntelVoltGPU = (int)IntelGfxUV,
                            IntelVoltCache = (int)IntelCacheUV,
                            IntelVoltSA = (int)IntelSAUV,
                            IntelBalCPU = (int)IntelCpuBal,
                            IntelBalGPU = (int)IntelGpuBal,

                            isApuGfxClk = (bool)IsAPUiGPUClkEnabled,
                            apuGfxClk = (int)APUiGPUClk,

                            rsr = (int)RSR,
                            boost = (int)Boost,
                            imageSharp = (int)ImageSharp,
                            isRadeonGraphics = (bool)IsRadeonGraphEnabled,
                            isRSR = (bool)IsRSREnabled,
                            isBoost = (bool)IsBoostEnabled,
                            isAntiLag = (bool)IsAntiLagEnabled,
                            isImageSharp = (bool)IsImageSharpEnabled,
                            isSync = (bool)IsSyncEnabled,

                            commandValue = getCommandValues(),

                            isIntelPL1 = (bool)IsIntelPL1Enabled,
                            isIntelPL2 = (bool)IsIntelPL2Enabled,
                            IsIntelVolt = (bool)IsIntelUVEnabled,
                            IsIntelBal = (bool)IsIntelBalEnabled,

                            isNVIDIA = (bool)IsNVEnabled,
                            nvMaxCoreClk = (int)NVMaxCore,
                            nvCoreClk = (int)NVCore,
                            nvMemClk = (int)NVMem,
                            nvPower = (int)NVPower,

                            asusGPUUlti = (bool)IsASUSUltiEnabled,
                            asusiGPU = (bool)IsASUSEcoEnabled,
                            asusPowerProfile = (int)AsusPowerIndex,

                            displayHz = (int)RefreshRateIndex,

                            isMag = (bool)IsUXTUSREnabled,
                            isVsync = (bool)IsVSyncEnabled,
                            isRecap = (bool)IsAutoCapEnabled,
                            Sharpness = (int)Sharp,
                            ResScaleIndex = (int)ResScaleIndex,

                            powerMode = (int)PowerModeIndex,
                            windowsBoostMode = WindowsBoostModeIndex,
                            isWindowsMinState = (bool)IsWindowsMinStateEnabled,
                            windowsMinState = (int)WindowsMinState,
                            isWindowsMaxState = (bool)IsWindowsMaxStateEnabled,
                            windowsMaxState = (int)WindowsMaxState,
                            isWindowsMaxFrequency = (bool)IsWindowsMaxFrequencyEnabled,
                            windowsMaxFrequency = (int)WindowsMaxFrequency,
                            isWindowsEpp = (bool)IsWindowsEppEnabled,
                            windowsEpp = (int)WindowsEpp,
                            isWindowsCoreParking = (bool)IsWindowsCoreParkingEnabled,
                            windowsCoreParking = (int)WindowsCoreParking,
                            isWindowsMaxUnparkedCores = (bool)IsWindowsMaxUnparkedCoresEnabled,
                            windowsMaxUnparkedCores = (int)WindowsMaxUnparkedCores,
                            ccdAffinity = (int)CcdAffinityIndex,

                            isIntelClockRatio = (bool)IsIntelRatioCoreEnabled,
                            intelClockRatioC1 = (int)IntelRatioC1,
                            intelClockRatioC2 = (int)IntelRatioC2,
                            intelClockRatioC3 = (int)IntelRatioC3,
                            intelClockRatioC4 = (int)IntelRatioC4,
                            intelClockRatioC5 = (int)IntelRatioC5,
                            intelClockRatioC6 = (int)IntelRatioC6,
                            intelClockRatioC7 = (int)IntelRatioC7,
                            intelClockRatioC8 = (int)IntelRatioC8,
                        };
                        presetManager.SavePreset(PresetNameText, preset);
                        if (!PowerPresetOptions.Contains(PresetNameText))
                            PowerPresetOptions.Add(PresetNameText);

                        SelectedPresetName = PresetNameText;
                        Settings.Default.cstmPreset = PresetNameText;
                        Settings.Default.Save();
                        _interaction.Notify("Preset Saved", $"Your preset {PresetNameText} has been saved successfully!");
                    }
                }
            }
        }

        [RelayCommand]
        private void DeletePreset()
        {
            try
            {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    if (SelectedPresetName != "" && SelectedPresetName != null)
                    {
                        string deletePresetName = SelectedPresetName;
                        presetManager.DeletePreset(deletePresetName);
                        PowerPresetOptions.Remove(deletePresetName);

                        updateValues("");
                        _interaction.Notify("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    if (SelectedPresetName != "" && SelectedPresetName != null)
                    {
                        string deletePresetName = SelectedPresetName;
                        presetManager.DeletePreset(deletePresetName);

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = presetManager.GetPresetNames();
                        PowerPresetOptions.Remove(deletePresetName);

                        updateValues("");
                        _interaction.Notify("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    if (SelectedPresetName != "" && SelectedPresetName != null)
                    {
                        string deletePresetName = SelectedPresetName;
                        presetManager.DeletePreset(deletePresetName);
                        PowerPresetOptions.Remove(deletePresetName);

                        updateValues("");
                        _interaction.Notify("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to delete preset");
            }
        }

        [RelayCommand]
        private void ReloadPreset()
        {
            ReloadPresetValues(SelectedPresetName as string ?? SelectedPresetName);
        }

        private string? GetSelectedPresetName()
        {
            var name = SelectedPresetName as string ?? SelectedPresetName;
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }

        private void ReloadPresetList()
        {
            var selectedPreset = GetSelectedPresetName() ?? Settings.Default.cstmPreset;
            presetManager = new PresetManager(Settings.Default.Path + GetPresetFileName());
            var presetNames = presetManager.GetPresetNames()
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            PowerPresetOptions.Clear();
            foreach (var presetName in presetNames)
            {
                PowerPresetOptions.Add(presetName);
            }

            SelectedPresetName = selectedPreset ?? string.Empty;
        }

        private void ReloadPresetValues(string? presetName)
        {
            presetManager = new PresetManager(Settings.Default.Path + GetPresetFileName());
            updateValues(presetName);
        }

        public void updateValues(string? preset)
        {
            if (isUpdatingPresetValues)
            {
                return;
            }

            isUpdatingPresetValues = true;
            var presetName = preset ?? string.Empty;
            try
            {
                SelectedPresetName = presetName;
                Settings.Default.cstmPreset = presetName;
                Settings.Default.Save();

                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = string.IsNullOrWhiteSpace(presetName) ? DefaultAPUPreset : presetManager.GetPreset(presetName) ?? DefaultAPUPreset;

                    // Read the values from the preset
                    APUSkinTemp = myPreset.apuSkinTemp;
                    APUTemp = myPreset.apuTemp;
                    STAPMPow = myPreset.apuSTAPMPow;
                    FastPow = myPreset.apuFastPow;
                    SlowPow = myPreset.apuSlowPow;
                    SlowTime = myPreset.apuSlowTime;
                    FastTime = myPreset.apuSTAPMTime;

                    IsAPUTempEnabled = myPreset.isApuTemp;
                    IsAPUSkinTempEnabled = myPreset.isApuSkinTemp;
                    IsSTAPMPowEnabled = myPreset.isApuSTAPMPow;
                    IsSlowPowEnabled = myPreset.isApuSlowPow;
                    IsSlowTimeEnabled = myPreset.isApuSlowTime;
                    IsFastPowEnabled = myPreset.isApuFastPow;
                    IsFastTimeEnabled = myPreset.isApuSTAPMTime;

                    CpuVrmTdc = myPreset.apuCpuTdc;
                    CpuVrmEdc = myPreset.apuCpuEdc;
                    GfxVrmTdc = myPreset.apuGfxTdc;
                    GfxVrmEdc = myPreset.apuGfxEdc;
                    SocVrmTdc = myPreset.apuSocTdc;
                    SocVrmEdc = myPreset.apuSocEdc;

                    IsCpuVrmTdcEnabled = myPreset.isApuCpuTdc;
                    IsCpuVrmEdcEnabled = myPreset.isApuCpuEdc;
                    IsGfxVrmTdcEnabled = myPreset.isApuGfxTdc;
                    IsGfxVrmEdcEnabled = myPreset.isApuGfxEdc;
                    IsSocVrmTdcEnabled = myPreset.isApuSocTdc;
                    IsSocVrmEdcEnabled = myPreset.isApuSocEdc;

                    APUiGPUClk = myPreset.apuGfxClk;

                    IsAPUiGPUClkEnabled = myPreset.isApuGfxClk;

                    PBOScaler = myPreset.pboScalar;
                    AllCO = myPreset.coAllCore;
                    GfxCO = myPreset.coGfx;

                    IsPBOScalerEnabled = myPreset.isPboScalar;
                    IsAllCOEnabled = myPreset.isCoAllCore;
                    IsGfxCOEnabled = myPreset.isCoGfx;

                    IsRadeonGraphEnabled = myPreset.isRadeonGraphics;
                    IsAntiLagEnabled = myPreset.isAntiLag;
                    IsRSREnabled = myPreset.isRSR;
                    IsBoostEnabled = myPreset.isBoost;
                    IsImageSharpEnabled = myPreset.isImageSharp;
                    IsSyncEnabled = myPreset.isSync;
                    RSR = myPreset.rsr;
                    Boost = myPreset.boost;
                    ImageSharp = myPreset.imageSharp;

                    CCD1Core1 = myPreset.ccd1Core1;
                    CCD1Core2 = myPreset.ccd1Core2;
                    CCD1Core3 = myPreset.ccd1Core3;
                    CCD1Core4 = myPreset.ccd1Core4;
                    CCD1Core5 = myPreset.ccd1Core5;
                    CCD1Core6 = myPreset.ccd1Core6;
                    CCD1Core7 = myPreset.ccd1Core7;
                    CCD1Core8 = myPreset.ccd1Core8;
                    CCD1Core9 = myPreset.ccd1Core9;
                    CCD1Core10 = myPreset.ccd1Core10;
                    CCD1Core11 = myPreset.ccd1Core11;
                    CCD1Core12 = myPreset.ccd1Core12;

                    IsCCD1Core1Enabled = myPreset.IsCCD1Core1;
                    IsCCD1Core2Enabled = myPreset.IsCCD1Core2;
                    IsCCD1Core3Enabled = myPreset.IsCCD1Core3;
                    IsCCD1Core4Enabled = myPreset.IsCCD1Core4;
                    IsCCD1Core5Enabled = myPreset.IsCCD1Core5;
                    IsCCD1Core6Enabled = myPreset.IsCCD1Core6;
                    IsCCD1Core7Enabled = myPreset.IsCCD1Core7;
                    IsCCD1Core8Enabled = myPreset.IsCCD1Core8;
                    IsCCD1Core9Enabled = myPreset.IsCCD1Core9;
                    IsCCD1Core10Enabled = myPreset.IsCCD1Core10;
                    IsCCD1Core11Enabled = myPreset.IsCCD1Core11;
                    IsCCD1Core12Enabled = myPreset.IsCCD1Core12;

                    CCD2Core1 = myPreset.ccd2Core1;
                    CCD2Core2 = myPreset.ccd2Core2;
                    CCD2Core3 = myPreset.ccd2Core3;
                    CCD2Core4 = myPreset.ccd2Core4;
                    CCD2Core5 = myPreset.ccd2Core5;
                    CCD2Core6 = myPreset.ccd2Core6;
                    CCD2Core7 = myPreset.ccd2Core7;
                    CCD2Core8 = myPreset.ccd2Core8;
                    CCD2Core9 = myPreset.ccd2Core9;
                    CCD2Core10 = myPreset.ccd2Core10;
                    CCD2Core11 = myPreset.ccd2Core11;
                    CCD2Core12 = myPreset.ccd2Core12;

                    IsCCD2Core1Enabled = myPreset.IsCCD2Core1;
                    IsCCD2Core2Enabled = myPreset.IsCCD2Core2;
                    IsCCD2Core3Enabled = myPreset.IsCCD2Core3;
                    IsCCD2Core4Enabled = myPreset.IsCCD2Core4;
                    IsCCD2Core5Enabled = myPreset.IsCCD2Core5;
                    IsCCD2Core6Enabled = myPreset.IsCCD2Core6;
                    IsCCD2Core7Enabled = myPreset.IsCCD2Core7;
                    IsCCD2Core8Enabled = myPreset.IsCCD2Core8;
                    IsCCD2Core9Enabled = myPreset.IsCCD2Core9;
                    IsCCD2Core10Enabled = myPreset.IsCCD2Core10;
                    IsCCD2Core11Enabled = myPreset.IsCCD2Core11;
                    IsCCD2Core12Enabled = myPreset.IsCCD2Core12;

                    BoostIndex = myPreset.boostProfile;

                    IsNVEnabled = myPreset.isNVIDIA;
                    NVMaxCore = myPreset.nvMaxCoreClk;
                    NVCore = myPreset.nvCoreClk;
                    NVMem = myPreset.nvMemClk;
                    if(myPreset.nvPower > 0) NVPower = myPreset.nvPower;

                    IsAmdOCEnabled = myPreset.IsAmdOC;
                    AmdCpuClk = myPreset.amdClock;
                    AmdVID = myPreset.amdVID;

                    SoftMiniGPUClk = myPreset.softMiniGPUClk;
                    SoftMinCPUClk = myPreset.softMinCPUClk;
                    SoftMinFabClk = myPreset.softMinFabClk;
                    SoftMinSoCClk = myPreset.softMinSoCClk;
                    SoftMinDataClk = myPreset.softMinDataClk;

                    SoftMaxiGPUClk = myPreset.softMaxiGPUClk;
                    SoftMaxCPUClk = myPreset.softMaxCPUClk;
                    SoftMaxFabClk = myPreset.softMaxFabClk;
                    SoftMaxSoCClk = myPreset.softMaxSoCClk;
                    SoftMaxDataClk = myPreset.softMaxDataClk;

                    IsSoftMiniGPUClkEnabled = myPreset.isSoftMiniGPUClk;
                    IsSoftMinCPUClkEnabled = myPreset.isSoftMinCPUClk;
                    IsSoftMinFabClkEnabled = myPreset.isSoftMinFabClk;
                    IsSoftMinSoCClkEnabled = myPreset.isSoftMinSoCClk;
                    IsSoftMinDataClkEnabled = myPreset.isSoftMinDataClk;

                    IsSoftMaxiGPUClkEnabled = myPreset.isSoftMaxiGPUClk;
                    IsSoftMaxCPUClkEnabled = myPreset.isSoftMaxCPUClk;
                    IsSoftMaxFabClkEnabled = myPreset.isSoftMaxFabClk;
                    IsSoftMaxSoCClkEnabled = myPreset.isSoftMaxSoCClk;
                    IsSoftMaxDataClkEnabled = myPreset.isSoftMaxDataClk;

                    IsASUSUltiEnabled = myPreset.asusGPUUlti;
                    IsASUSEcoEnabled = myPreset.asusiGPU;
                    AsusPowerIndex = myPreset.asusPowerProfile;

                    if (myPreset.displayHz <= RefreshRateOptions.Count) RefreshRateIndex = myPreset.displayHz;

                    PowerModeIndex = myPreset.powerMode;
                    WindowsBoostModeIndex = myPreset.windowsBoostMode;
                    IsWindowsMinStateEnabled = myPreset.isWindowsMinState;
                    WindowsMinState = myPreset.windowsMinState;
                    IsWindowsMaxStateEnabled = myPreset.isWindowsMaxState;
                    WindowsMaxState = myPreset.windowsMaxState;
                    IsWindowsMaxFrequencyEnabled = myPreset.isWindowsMaxFrequency;
                    WindowsMaxFrequency = myPreset.windowsMaxFrequency;
                    IsWindowsEppEnabled = myPreset.isWindowsEpp;
                    WindowsEpp = myPreset.windowsEpp;
                    IsWindowsCoreParkingEnabled = myPreset.isWindowsCoreParking;
                    WindowsCoreParking = myPreset.windowsCoreParking;
                    IsWindowsMaxUnparkedCoresEnabled = myPreset.isWindowsMaxUnparkedCores;
                    WindowsMaxUnparkedCores = myPreset.windowsMaxUnparkedCores;
                    CcdAffinityIndex = myPreset.ccdAffinity;

                    IsUXTUSREnabled = myPreset.isMag;
                    IsVSyncEnabled = myPreset.isVsync;
                    IsAutoCapEnabled = myPreset.isRecap;
                    Sharp = myPreset.Sharpness;
                    ResScaleIndex = myPreset.ResScaleIndex;
                } else if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = string.IsNullOrWhiteSpace(presetName) ? DefaultAMDDtCPUPreset : presetManager.GetPreset(presetName) ?? DefaultAMDDtCPUPreset;

                    // Read the values from the preset
                    CPUTemp = myPreset.dtCpuTemp;
                    PPT = myPreset.dtCpuPPT;
                    TDC = myPreset.dtCpuTDC;
                    EDC = myPreset.dtCpuEDC;

                    IsCPUTempEnabled = myPreset.isDtCpuTemp;
                    IsPPTEnabled = myPreset.isDtCpuPPT;
                    IsTDCEnabled = myPreset.isDtCpuTDC;
                    IsEDCEnabled = myPreset.isDtCpuEDC;

                    PBOScaler = myPreset.pboScalar;
                    AllCO = myPreset.coAllCore;
                    GfxCO = myPreset.coGfx;

                    IsPBOScalerEnabled = myPreset.isPboScalar;
                    IsAllCOEnabled = myPreset.isCoAllCore;
                    IsGfxCOEnabled = myPreset.isCoGfx;

                    IsRadeonGraphEnabled = myPreset.isRadeonGraphics;
                    IsAntiLagEnabled = myPreset.isAntiLag;
                    IsRSREnabled = myPreset.isRSR;
                    IsBoostEnabled = myPreset.isBoost;
                    IsImageSharpEnabled = myPreset.isImageSharp;
                    IsSyncEnabled = myPreset.isSync;
                    RSR = myPreset.rsr;
                    Boost = myPreset.boost;
                    ImageSharp = myPreset.imageSharp;

                    CCD1Core1 = myPreset.ccd1Core1;
                    CCD1Core2 = myPreset.ccd1Core2;
                    CCD1Core3 = myPreset.ccd1Core3;
                    CCD1Core4 = myPreset.ccd1Core4;
                    CCD1Core5 = myPreset.ccd1Core5;
                    CCD1Core6 = myPreset.ccd1Core6;
                    CCD1Core7 = myPreset.ccd1Core7;
                    CCD1Core8 = myPreset.ccd1Core8;
                    CCD1Core9 = myPreset.ccd1Core9;
                    CCD1Core10 = myPreset.ccd1Core10;
                    CCD1Core11 = myPreset.ccd1Core11;
                    CCD1Core12 = myPreset.ccd1Core12;

                    IsCCD1Core1Enabled = myPreset.IsCCD1Core1;
                    IsCCD1Core2Enabled = myPreset.IsCCD1Core2;
                    IsCCD1Core3Enabled = myPreset.IsCCD1Core3;
                    IsCCD1Core4Enabled = myPreset.IsCCD1Core4;
                    IsCCD1Core5Enabled = myPreset.IsCCD1Core5;
                    IsCCD1Core6Enabled = myPreset.IsCCD1Core6;
                    IsCCD1Core7Enabled = myPreset.IsCCD1Core7;
                    IsCCD1Core8Enabled = myPreset.IsCCD1Core8;
                    IsCCD1Core9Enabled = myPreset.IsCCD1Core9;
                    IsCCD1Core10Enabled = myPreset.IsCCD1Core10;
                    IsCCD1Core11Enabled = myPreset.IsCCD1Core11;
                    IsCCD1Core12Enabled = myPreset.IsCCD1Core12;

                    CCD2Core1 = myPreset.ccd2Core1;
                    CCD2Core2 = myPreset.ccd2Core2;
                    CCD2Core3 = myPreset.ccd2Core3;
                    CCD2Core4 = myPreset.ccd2Core4;
                    CCD2Core5 = myPreset.ccd2Core5;
                    CCD2Core6 = myPreset.ccd2Core6;
                    CCD2Core7 = myPreset.ccd2Core7;
                    CCD2Core8 = myPreset.ccd2Core8;
                    CCD2Core9 = myPreset.ccd2Core9;
                    CCD2Core10 = myPreset.ccd2Core10;
                    CCD2Core11 = myPreset.ccd2Core11;
                    CCD2Core12 = myPreset.ccd2Core12;

                    IsCCD2Core1Enabled = myPreset.IsCCD2Core1;
                    IsCCD2Core2Enabled = myPreset.IsCCD2Core2;
                    IsCCD2Core3Enabled = myPreset.IsCCD2Core3;
                    IsCCD2Core4Enabled = myPreset.IsCCD2Core4;
                    IsCCD2Core5Enabled = myPreset.IsCCD2Core5;
                    IsCCD2Core6Enabled = myPreset.IsCCD2Core6;
                    IsCCD2Core7Enabled = myPreset.IsCCD2Core7;
                    IsCCD2Core8Enabled = myPreset.IsCCD2Core8;
                    IsCCD2Core9Enabled = myPreset.IsCCD2Core9;
                    IsCCD2Core10Enabled = myPreset.IsCCD2Core10;
                    IsCCD2Core11Enabled = myPreset.IsCCD2Core11;
                    IsCCD2Core12Enabled = myPreset.IsCCD2Core12;

                    IsNVEnabled = myPreset.isNVIDIA;
                    NVMaxCore = myPreset.nvMaxCoreClk;
                    NVCore = myPreset.nvCoreClk;
                    NVMem = myPreset.nvMemClk;
                    if (myPreset.nvPower > 0) NVPower = myPreset.nvPower;

                    IsAmdOCEnabled = myPreset.IsAmdOC;
                    AmdCpuClk = myPreset.amdClock;
                    AmdVID = myPreset.amdVID;

                    IsASUSUltiEnabled = myPreset.asusGPUUlti;
                    IsASUSEcoEnabled = myPreset.asusiGPU;
                    AsusPowerIndex = myPreset.asusPowerProfile;

                    if (myPreset.displayHz <= RefreshRateOptions.Count) RefreshRateIndex = myPreset.displayHz;

                    PowerModeIndex = myPreset.powerMode;
                    WindowsBoostModeIndex = myPreset.windowsBoostMode;
                    IsWindowsMinStateEnabled = myPreset.isWindowsMinState;
                    WindowsMinState = myPreset.windowsMinState;
                    IsWindowsMaxStateEnabled = myPreset.isWindowsMaxState;
                    WindowsMaxState = myPreset.windowsMaxState;
                    IsWindowsMaxFrequencyEnabled = myPreset.isWindowsMaxFrequency;
                    WindowsMaxFrequency = myPreset.windowsMaxFrequency;
                    IsWindowsEppEnabled = myPreset.isWindowsEpp;
                    WindowsEpp = myPreset.windowsEpp;
                    IsWindowsCoreParkingEnabled = myPreset.isWindowsCoreParking;
                    WindowsCoreParking = myPreset.windowsCoreParking;
                    IsWindowsMaxUnparkedCoresEnabled = myPreset.isWindowsMaxUnparkedCores;
                    WindowsMaxUnparkedCores = myPreset.windowsMaxUnparkedCores;
                    CcdAffinityIndex = myPreset.ccdAffinity;

                    IsUXTUSREnabled = myPreset.isMag;
                    IsVSyncEnabled = myPreset.isVsync;
                    IsAutoCapEnabled = myPreset.isRecap;
                    Sharp = myPreset.Sharpness;
                    ResScaleIndex = myPreset.ResScaleIndex;
                } else if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = string.IsNullOrWhiteSpace(presetName) ? DefaultIntelPreset : presetManager.GetPreset(presetName) ?? DefaultIntelPreset;

                    // Read the values from the preset
                    IntelPL1 = myPreset.IntelPL1;
                    IntelPL2 = myPreset.IntelPL2;

                    IsIntelPL1Enabled = myPreset.isIntelPL1;
                    IsIntelPL2Enabled = myPreset.isIntelPL2;

                    APUiGPUClk = myPreset.apuGfxClk;

                    IsAPUiGPUClkEnabled = myPreset.isApuGfxClk;

                    IsRadeonGraphEnabled = myPreset.isRadeonGraphics;
                    IsAntiLagEnabled = myPreset.isAntiLag;
                    IsRSREnabled = myPreset.isRSR;
                    IsBoostEnabled = myPreset.isBoost;
                    IsImageSharpEnabled = myPreset.isImageSharp;
                    IsSyncEnabled = myPreset.isSync;
                    RSR = myPreset.rsr;
                    Boost = myPreset.boost;
                    ImageSharp = myPreset.imageSharp;

                    IsNVEnabled = myPreset.isNVIDIA;
                    NVMaxCore = myPreset.nvMaxCoreClk;
                    NVCore = myPreset.nvCoreClk;
                    NVMem = myPreset.nvMemClk;
                    if (myPreset.nvPower > 0) NVPower = myPreset.nvPower;

                    IsASUSUltiEnabled = myPreset.asusGPUUlti;
                    IsASUSEcoEnabled = myPreset.asusiGPU;
                    AsusPowerIndex = myPreset.asusPowerProfile;

                    if (myPreset.displayHz <= RefreshRateOptions.Count) RefreshRateIndex = myPreset.displayHz;

                    PowerModeIndex = myPreset.powerMode;
                    WindowsBoostModeIndex = myPreset.windowsBoostMode;
                    IsWindowsMinStateEnabled = myPreset.isWindowsMinState;
                    WindowsMinState = myPreset.windowsMinState;
                    IsWindowsMaxStateEnabled = myPreset.isWindowsMaxState;
                    WindowsMaxState = myPreset.windowsMaxState;
                    IsWindowsMaxFrequencyEnabled = myPreset.isWindowsMaxFrequency;
                    WindowsMaxFrequency = myPreset.windowsMaxFrequency;
                    IsWindowsEppEnabled = myPreset.isWindowsEpp;
                    WindowsEpp = myPreset.windowsEpp;
                    IsWindowsCoreParkingEnabled = myPreset.isWindowsCoreParking;
                    WindowsCoreParking = myPreset.windowsCoreParking;
                    IsWindowsMaxUnparkedCoresEnabled = myPreset.isWindowsMaxUnparkedCores;
                    WindowsMaxUnparkedCores = myPreset.windowsMaxUnparkedCores;
                    CcdAffinityIndex = myPreset.ccdAffinity;

                    IsUXTUSREnabled = myPreset.isMag;
                    IsVSyncEnabled = myPreset.isVsync;
                    IsAutoCapEnabled = myPreset.isRecap;
                    Sharp = myPreset.Sharpness;
                    ResScaleIndex = myPreset.ResScaleIndex;

                    IsIntelUVEnabled = myPreset.IsIntelVolt;
                    IntelCoreUV = myPreset.IntelVoltCPU;
                    IntelGfxUV = myPreset.IntelVoltGPU;
                    IntelCacheUV = myPreset.IntelVoltCache;
                    IntelSAUV = myPreset.IntelVoltSA;

                    IsIntelBalEnabled = myPreset.IsIntelBal;
                    IntelCpuBal = myPreset.IntelBalCPU;
                    IntelGpuBal = myPreset.IntelBalGPU;

                    IsIntelRatioCoreEnabled = myPreset.isIntelClockRatio;
                    IntelRatioC1 = myPreset.intelClockRatioC1;
                    IntelRatioC2 = myPreset.intelClockRatioC2;
                    IntelRatioC3 = myPreset.intelClockRatioC3;
                    IntelRatioC4 = myPreset.intelClockRatioC4;
                    IntelRatioC5 = myPreset.intelClockRatioC5;
                    IntelRatioC6 = myPreset.intelClockRatioC6;
                    IntelRatioC7 = myPreset.intelClockRatioC7;
                    IntelRatioC8 = myPreset.intelClockRatioC8;
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to update preset values");
            }
            finally
            {
                isUpdatingPresetValues = false;
            }
        }

        [RelayCommand]
        private async Task UndoOverclockAsync()
        {
            IsAmdOCEnabled = false;
            await _application.ApplyAsync("--disable-oc ");
            await _application.ApplyAsync(getCommandValues(), appliedName: GetSelectedPresetName());
            Settings.Default.CommandString = getCommandValues();
            Settings.Default.Save();
            UndoVisibility = Visibility.Collapsed;
            await _application.ApplyAsync("--disable-oc ");
        }

        private async Task InitializeHardwareAsync()
        {
            ReloadPresetList();

            if (deferredSetupComplete)
            {
                ReloadPresetValues(Settings.Default.cstmPreset);
                return;
            }

            deferredSetupComplete = true;
            await App.DisplaySetupTask;

            if (Display.uniqueRefreshRates.Count > 1)
            {
                RefreshRateOptions.Clear();
                RefreshRateOptions.Add("System Controlled");
                foreach (int rate in Display.uniqueRefreshRates)
                    RefreshRateOptions.Add($"{rate} Hz");
                RefreshRateVisibility = Visibility.Visible;
            }

            GpuInventorySnapshot inventory = await gpuInventory.GetSnapshotAsync();
            radeonGpuCount = inventory.RadeonCount;
            nvidiaGpuCount = inventory.NvidiaCount;
            ADLXVisibility = radeonGpuCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            NVIDIAVisibility = nvidiaGpuCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (nvidiaGpuCount > 0)
            {
                NvTuning.GpuInfo? info = await Task.Run<NvTuning.GpuInfo?>(() =>
                    NvTuning.TryGetGpuInfo(out NvTuning.GpuInfo value) ? value : null);
                if (info.HasValue)
                {
                    NVPowerMaximum = info.Value.MaxPowerWatts;
                    NVPowerMaximum = info.Value.MaxPowerWatts;
                    NVPowerMinimum = info.Value.MinPowerWatts;
                    NVPowerMinimum = info.Value.MinPowerWatts;
                    NVPower = info.Value.CurrentPowerWatts;
                }
            }

            if (Settings.Default.isASUS)
            {
                (int mux, int eco, int performanceMode) = await Task.Run(() =>
                {
                    try
                    {
                        uint muxId = App.product.Contains("ROG") || App.product.Contains("TUF") ? ASUSWmi.GPUMux : ASUSWmi.GPUMuxVivo;
                        uint performanceId = App.product.Contains("ROG") || App.product.Contains("TUF") ? ASUSWmi.PerformanceMode : ASUSWmi.VivoBookMode;
                        return (App.wmi.DeviceGet(muxId), App.wmi.DeviceGet(ASUSWmi.GPUEco), App.wmi.DeviceGet(performanceId));
                    }
                    catch
                    {
                        return (-1, -1, -1);
                    }
                });

                if (mux > 0) IsASUSUltiEnabled = false;
                else if (mux > -1) IsASUSUltiEnabled = true;
                else AsusUltiVisibility = Visibility.Collapsed;

                if (eco is >= 0 and < 1) IsASUSEcoEnabled = false;
                else if (eco > 0) IsASUSEcoEnabled = true;
                else AsusEcoVisibility = Visibility.Collapsed;

                if (performanceMode == (int)ASUSWmi.AsusMode.Silent) AsusPowerIndex = 1;
                else if (performanceMode == (int)ASUSWmi.AsusMode.Balanced) AsusPowerIndex = 2;
                else if (performanceMode == (int)ASUSWmi.AsusMode.Turbo) AsusPowerIndex = 3;
            }

            ReloadPresetValues(Settings.Default.cstmPreset);
        }
        private static string GetPresetFileName() => Family.TYPE switch
        {
            Family.ProcessorType.Amd_Apu => "apuPresets.json",
            Family.ProcessorType.Amd_Desktop_Cpu => "amdDtCpuPresets.json",
            _ => "intelPresets.json"
        };
        public string getCommandValues() => _commands.Build(this, new PresetHardwareContext(Family.TYPE, Family.FAM, Settings.Default.isASUS, Display.uniqueRefreshRates, clockRatio?.Length ?? 0));
        partial void OnSelectedPresetNameChanged(string value) { if (!isUpdatingPresetValues && presetManager != null) updateValues(value); }
        protected override void OnActivated() => ReloadPresetList();
        public override async Task StopAsync()
        {
            var baseStop = base.StopAsync();
            await Task.WhenAll(baseStop, ApplyPresetCommand.ExecutionTask ?? Task.CompletedTask,
                UndoOverclockCommand.ExecutionTask ?? Task.CompletedTask);
        }

        partial void OnIsBoostEnabledChanged(bool value)
        {
            if (value) { IsRSREnabled = false; IsAntiLagEnabled = false; }
        }

        partial void OnIsAntiLagEnabledChanged(bool value)
        {
            if (value) IsBoostEnabled = false;
        }

        partial void OnIsRSREnabledChanged(bool value)
        {
            if (value) { IsBoostEnabled = false; IsImageSharpEnabled = false; }
        }

        partial void OnIsImageSharpEnabledChanged(bool value)
        {
            if (value) IsRSREnabled = false;
        }

        partial void OnIsAmdOCEnabledChanged(bool value)
        {
            UndoVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

}
