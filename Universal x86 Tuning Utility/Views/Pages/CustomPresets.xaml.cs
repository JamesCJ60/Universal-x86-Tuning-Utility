using CpuAffinityUtility;
using RyzenSmu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.ASUS;
using Universal_x86_Tuning_Utility.Scripts.GPUs.NVIDIA;
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    public partial class CustomPresets : INavigableView<ViewModels.CustomPresetsViewModel>
    {
        public ViewModels.CustomPresetsViewModel ViewModel
        {
            get;
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
        NumberBox[] intelRatioControls = null;
        public CustomPresets(GpuInventoryService gpuInventory)
        {
            this.gpuInventory = gpuInventory;
            InitializeComponent();
            _ = Tablet.TabletDevices;

            presetManager = new PresetManager(Settings.Default.Path + GetPresetFileName());

            sdCcdAffinity.Visibility = Visibility.Collapsed;

            sdADLX.Visibility = Visibility.Collapsed;
            sdNVIDIA.Visibility = Visibility.Collapsed;
            sdRefreshRate.Visibility = Visibility.Collapsed;

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                sdAmdCPU.Visibility = Visibility.Collapsed;
                sdAmdCpuThermal.Visibility = Visibility.Collapsed;
                sdIntelCPU.Visibility = Visibility.Collapsed;
                sdIntelUV.Visibility = Visibility.Collapsed;
                sdIntelBal.Visibility = Visibility.Collapsed;
                sdIntelCoreRatio.Visibility = Visibility.Collapsed;

                if(Family.FAM < Family.RyzenFamily.Renoir)
                {
                    sdAmdCCD1CO.Visibility = Visibility.Collapsed;
                    sdAmdCCD2CO.Visibility = Visibility.Collapsed;
                }
                
                if (Family.FAM is not (
                    Family.RyzenFamily.StrixHalo or
                    Family.RyzenFamily.StrixPoint or
                    Family.RyzenFamily.KrackanPoint or
                    Family.RyzenFamily.PhoenixPoint or
                    Family.RyzenFamily.PhoenixPoint2 or
                    Family.RyzenFamily.Mendocino or
                    Family.RyzenFamily.Rembrandt or
                    Family.RyzenFamily.Lucienne or
                    Family.RyzenFamily.Renoir))
                    sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;
                if (SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery) sdAmdCpuTune.Visibility = Visibility.Collapsed;

                if (Family.FAM < Family.RyzenFamily.Renoir) sdAmdSoftClk.Visibility = Visibility.Visible;
                
                sdAmdCO.Visibility = Visibility.Visible;

                sdAmdCCD1CO.Visibility = sdAmdCO.Visibility;

                if (Family.FAM == Family.RyzenFamily.DragonRange || Family.FAM == Family.RyzenFamily.FireRange || Family.FAM == Family.RyzenFamily.StrixHalo || Family.FAM == Family.RyzenFamily.KrackanPoint) if (Family.CPUName.Contains("Ryzen 9") || Family.CPUName.Contains("395") || Family.CPUName.Contains("390")) sdAmdCCD2CO.Visibility = sdAmdCO.Visibility;

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = presetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxPowerPreset.Items.Add(presetName);
                }

                if (Family.FAM == Family.RyzenFamily.DragonRange || Family.FAM == Family.RyzenFamily.FireRange || Family.FAM == Family.RyzenFamily.StrixHalo)
                {
                   if((int)CpuAffinityManager.GetActiveProcessorCount(0xFFFF) > 16) sdCcdAffinity.Visibility = Visibility.Visible;
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                sdAmdApuCPU.Visibility = Visibility.Collapsed;
                sdAmdApuThermal.Visibility = Visibility.Collapsed;
                sdAmdApuVRM.Visibility = Visibility.Collapsed;
                sdIntelCPU.Visibility = Visibility.Collapsed;
                sdIntelUV.Visibility = Visibility.Collapsed;
                sdIntelBal.Visibility = Visibility.Collapsed;
                sdIntelCoreRatio.Visibility = Visibility.Collapsed;

                sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;
                sdAmdPowerProfile.Visibility = Visibility.Collapsed;

                if (Family.FAM < Family.RyzenFamily.Vermeer) sdAmdCO.Visibility = Visibility.Collapsed;
                sdAmdCCD1CO.Visibility = sdAmdCO.Visibility;
                if (Family.CPUName.Contains("Ryzen 9")) sdAmdCCD2CO.Visibility = sdAmdCO.Visibility;

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = presetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxPowerPreset.Items.Add(presetName);
                }

                if (Environment.ProcessorCount > 16) sdCcdAffinity.Visibility = Visibility.Visible;
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                sdAmdCPU.Visibility = Visibility.Collapsed;
                sdAmdCpuThermal.Visibility = Visibility.Collapsed;
                sdAmdApuCPU.Visibility = Visibility.Collapsed;
                sdAmdApuThermal.Visibility = Visibility.Collapsed;
                sdAmdApuVRM.Visibility = Visibility.Collapsed;
                sdAmdPowerProfile.Visibility = Visibility.Collapsed;
                //sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;

                nudAPUiGPUClk.Minimum = 100;
                sdAPUiGPUClk.Minimum = 100;

                sdAmdCpuClk.Visibility = Visibility.Collapsed;
                sdAmdPBO.Visibility = Visibility.Collapsed;
                sdAmdCpuTune.Visibility = Visibility.Collapsed;
                sdAmdCO.Visibility = Visibility.Collapsed;

                clockRatio = new int[8];

                intelRatioControls = new NumberBox[]
                {
                    nudIntelRatioC1,
                    nudIntelRatioC2,
                    nudIntelRatioC3,
                    nudIntelRatioC4,
                    nudIntelRatioC5,
                    nudIntelRatioC6,
                    nudIntelRatioC7,
                    nudIntelRatioC8
                };

                if (clockRatio != null)
                {
                    int core = 0;
                    foreach (var clock in clockRatio)
                    {
                        if (core < intelRatioControls.Length) intelRatioControls[core].Value = clock;
                        
                        core++;
                    }
                }
                // Get the names of all the stored presets
                IEnumerable<string> presetNames = presetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxPowerPreset.Items.Add(presetName);
                }
            }

            if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(15, 0, 0, 0);
            else mainCon.Margin = new Thickness(15, 0, -12, 0);

            if (!Settings.Default.isASUS)
            {
                sdAsusPower.Visibility = Visibility.Collapsed;
                sdAsusUlti.Visibility = Visibility.Collapsed;
                sdAsusEco.Visibility = Visibility.Collapsed;
            }

            if (Family.FAM == Family.RyzenFamily.Renoir || Family.FAM == Family.RyzenFamily.Lucienne || Family.FAM == Family.RyzenFamily.Mendocino || Family.FAM == Family.RyzenFamily.Rembrandt || Family.FAM == Family.RyzenFamily.PhoenixPoint || Family.FAM == Family.RyzenFamily.PhoenixPoint2 || Family.FAM == Family.RyzenFamily.HawkPoint) sdAmdApuiGPUClk.Visibility = Visibility.Visible;

            updateValues(Settings.Default.cstmPreset);
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }

        private async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            string commandValues = "";

            commandValues = getCommandValues();

            if (commandValues != "" && commandValues != null)
            {
                await RyzenAdj_To_UXTU.TranslateAsync(commandValues, appliedName: GetSelectedPresetName());
                ToastNotification.ShowToastNotification("Preset Applied", $"Your custom preset settings have been applied!");
            }

            Settings.Default.CommandString = commandValues;
            Settings.Default.Save();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!tbxPresetName.Text.Contains("PM -"))
            {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    if (tbxPresetName.Text != "" && tbxPresetName.Text != null)
                    {
                        // Save a preset
                        Preset preset = new Preset
                        {
                            apuTemp = (int)nudAPUTemp.Value,
                            apuSkinTemp = (int)nudAPUSkinTemp.Value,
                            apuSTAPMPow = (int)nudSTAPMPow.Value,
                            apuSTAPMTime = (int)nudFastTime.Value,
                            apuFastPow = (int)nudFastPow.Value,
                            apuSlowPow = (int)nudSlowPow.Value,
                            apuSlowTime = (int)nudSlowTime.Value,

                            apuCpuTdc = (int)nudCpuVrmTdc.Value,
                            apuCpuEdc = (int)nudCpuVrmEdc.Value,
                            apuSocTdc = (int)nudSocVrmTdc.Value,
                            apuSocEdc = (int)nudSocVrmEdc.Value,
                            apuGfxTdc = (int)nudGfxVrmTdc.Value,
                            apuGfxEdc = (int)nudGfxVrmEdc.Value,

                            apuGfxClk = (int)nudAPUiGPUClk.Value,

                            pboScalar = (int)nudPBOScaler.Value,
                            coAllCore = (int)nudAllCO.Value,

                            coGfx = (int)nudGfxCO.Value,
                            isCoGfx = (bool)cbGfxCO.IsChecked,

                            boostProfile = (int)cbxBoost.SelectedIndex,

                            rsr = (int)nudRSR.Value,
                            boost = (int)nudBoost.Value,
                            imageSharp = (int)nudImageSharp.Value,
                            isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                            isRSR = (bool)cbRSR.IsChecked,
                            isBoost = (bool)cbBoost.IsChecked,
                            isAntiLag = (bool)cbAntiLag.IsChecked,
                            isImageSharp = (bool)cbImageSharp.IsChecked,
                            isSync = (bool)cbSync.IsChecked,

                            ccd1Core1 = (int)nudCCD1Core1.Value,
                            ccd1Core2 = (int)nudCCD1Core2.Value,
                            ccd1Core3 = (int)nudCCD1Core3.Value,
                            ccd1Core4 = (int)nudCCD1Core4.Value,
                            ccd1Core5 = (int)nudCCD1Core5.Value,
                            ccd1Core6 = (int)nudCCD1Core6.Value,
                            ccd1Core7 = (int)nudCCD1Core7.Value,
                            ccd1Core8 = (int)nudCCD1Core8.Value,

                            ccd2Core1 = (int)nudCCD2Core1.Value,
                            ccd2Core2 = (int)nudCCD2Core2.Value,
                            ccd2Core3 = (int)nudCCD2Core3.Value,
                            ccd2Core4 = (int)nudCCD2Core4.Value,
                            ccd2Core5 = (int)nudCCD2Core5.Value,
                            ccd2Core6 = (int)nudCCD2Core6.Value,
                            ccd2Core7 = (int)nudCCD2Core7.Value,
                            ccd2Core8 = (int)nudCCD2Core8.Value,

                            commandValue = getCommandValues(),

                            isApuTemp = (bool)cbAPUTemp.IsChecked,
                            isApuSkinTemp = (bool)cbAPUSkinTemp.IsChecked,
                            isApuSTAPMPow = (bool)cbSTAPMPow.IsChecked,
                            isApuSlowPow = (bool)cbSlowPow.IsChecked,
                            isApuSlowTime = (bool)cbSlowTime.IsChecked,
                            isApuFastPow = (bool)cbFastPow.IsChecked,
                            isApuSTAPMTime = (bool)cbFastTime.IsChecked,

                            isApuCpuTdc = (bool)cbCpuVrmTdc.IsChecked,
                            isApuCpuEdc = (bool)cbCpuVrmEdc.IsChecked,
                            isApuSocTdc = (bool)cbSocVrmTdc.IsChecked,
                            isApuSocEdc = (bool)cbSocVrmEdc.IsChecked,
                            isApuGfxTdc = (bool)cbGfxVrmTdc.IsChecked,
                            isApuGfxEdc = (bool)cbGfxVrmEdc.IsChecked,

                            isApuGfxClk = (bool)cbAPUiGPUClk.IsChecked,

                            isPboScalar = (bool)cbPBOScaler.IsChecked,
                            isCoAllCore = (bool)cbAllCO.IsChecked,

                            IsCCD1Core1 = (bool)cbCCD1Core1.IsChecked,
                            IsCCD1Core2 = (bool)cbCCD1Core2.IsChecked,
                            IsCCD1Core3 = (bool)cbCCD1Core3.IsChecked,
                            IsCCD1Core4 = (bool)cbCCD1Core4.IsChecked,
                            IsCCD1Core5 = (bool)cbCCD1Core5.IsChecked,
                            IsCCD1Core6 = (bool)cbCCD1Core6.IsChecked,
                            IsCCD1Core7 = (bool)cbCCD1Core7.IsChecked,
                            IsCCD1Core8 = (bool)cbCCD1Core8.IsChecked,

                            IsCCD2Core1 = (bool)cbCCD2Core1.IsChecked,
                            IsCCD2Core2 = (bool)cbCCD2Core2.IsChecked,
                            IsCCD2Core3 = (bool)cbCCD2Core3.IsChecked,
                            IsCCD2Core4 = (bool)cbCCD2Core4.IsChecked,
                            IsCCD2Core5 = (bool)cbCCD2Core5.IsChecked,
                            IsCCD2Core6 = (bool)cbCCD2Core6.IsChecked,
                            IsCCD2Core7 = (bool)cbCCD2Core7.IsChecked,
                            IsCCD2Core8 = (bool)cbCCD2Core8.IsChecked,

                            isNVIDIA = (bool)tsNV.IsChecked,
                            nvMaxCoreClk = (int)nudNVMaxCore.Value,
                            nvCoreClk = (int)nudNVCore.Value,
                            nvMemClk = (int)nudNVMem.Value,
                            nvPower = (int)nudNVPower.Value,

                            IsAmdOC = (bool)tsAmdOC.IsChecked,
                            amdClock = (int)nudAmdCpuClk.Value,
                            amdVID = (int)nudAmdVID.Value,

                            softMiniGPUClk = (int)nudSoftMiniGPUClk.Value,
                            softMinCPUClk = (int)nudSoftMinCPUClk.Value,
                            softMinFabClk = (int)nudSoftMinFabClk.Value,
                            softMinDataClk = (int)nudSoftMinDataClk.Value,
                            softMinSoCClk = (int)nudSoftMinSoCClk.Value,
                            softMinVCNClk = (int)nudSoftMinVCNClk.Value,

                            softMaxiGPUClk = (int)nudSoftMaxiGPUClk.Value,
                            softMaxCPUClk = (int)nudSoftMaxCPUClk.Value,
                            softMaxFabClk = (int)nudSoftMaxFabClk.Value,
                            softMaxDataClk = (int)nudSoftMaxDataClk.Value,
                            softMaxSoCClk = (int)nudSoftMaxSoCClk.Value,
                            softMaxVCNClk = (int)nudSoftMaxVCNClk.Value,

                            isSoftMiniGPUClk = (bool)cbSoftMiniGPUClk.IsChecked,
                            isSoftMinCPUClk = (bool)cbSoftMinCPUClk.IsChecked,
                            isSoftMinFabClk = (bool)cbSoftMinFabClk.IsChecked,
                            isSoftMinDataClk = (bool)cbSoftMinDataClk.IsChecked,
                            isSoftMinSoCClk = (bool)cbSoftMinSoCClk.IsChecked,
                            isSoftMinVCNClk = (bool)cbSoftMinVCNClk.IsChecked,

                            isSoftMaxiGPUClk = (bool)cbSoftMaxiGPUClk.IsChecked,
                            isSoftMaxCPUClk = (bool)cbSoftMaxCPUClk.IsChecked,
                            isSoftMaxFabClk = (bool)cbSoftMaxFabClk.IsChecked,
                            isSoftMaxDataClk = (bool)cbSoftMaxDataClk.IsChecked,
                            isSoftMaxSoCClk = (bool)cbSoftMaxSoCClk.IsChecked,
                            isSoftMaxVCNClk = (bool)cbSoftMaxVCNClk.IsChecked,

                            asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                            asusiGPU = (bool)tsASUSEco.IsChecked,
                            asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                            displayHz = (int)cbxRefreshRate.SelectedIndex,

                            isMag = (bool)tsUXTUSR.IsChecked,
                            isVsync = (bool)cbVSync.IsChecked,
                            isRecap = (bool)cbAutoCap.IsChecked,
                            Sharpness = (int)nudSharp.Value,
                            ResScaleIndex = (int)cbxResScale.SelectedIndex,

                            powerMode = (int)cbxPowerMode.SelectedIndex,
                            windowsBoostMode = cbxWindowsBoostMode.SelectedIndex,
                            isWindowsMinState = (bool)cbWindowsMinState.IsChecked,
                            windowsMinState = (int)nudWindowsMinState.Value,
                            isWindowsMaxState = (bool)cbWindowsMaxState.IsChecked,
                            windowsMaxState = (int)nudWindowsMaxState.Value,
                            isWindowsMaxFrequency = (bool)cbWindowsMaxFrequency.IsChecked,
                            windowsMaxFrequency = (int)nudWindowsMaxFrequency.Value,
                            isWindowsEpp = (bool)cbWindowsEpp.IsChecked,
                            windowsEpp = (int)nudWindowsEpp.Value,
                            isWindowsCoreParking = (bool)cbWindowsCoreParking.IsChecked,
                            windowsCoreParking = (int)nudWindowsCoreParking.Value,
                            isWindowsMaxUnparkedCores = (bool)cbWindowsMaxUnparkedCores.IsChecked,
                            windowsMaxUnparkedCores = (int)nudWindowsMaxUnparkedCores.Value,
                            ccdAffinity = (int)cbxCcdAffinity.SelectedIndex,
                        };
                        presetManager.SavePreset(tbxPresetName.Text, preset);
                        if ( !cbxPowerPreset.Items.Contains(tbxPresetName.Text) )
                            cbxPowerPreset.Items.Add(tbxPresetName.Text);

                        cbxPowerPreset.Text = tbxPresetName.Text;
                        Settings.Default.cstmPreset = tbxPresetName.Text;
                        Settings.Default.Save();
                        ToastNotification.ShowToastNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");

                    }
                }

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    if (tbxPresetName.Text != "" && tbxPresetName.Text != null)
                    {
                        // Save a preset
                        Preset preset = new Preset
                        {
                            dtCpuTemp = (int)nudCPUTemp.Value,
                            dtCpuPPT = (int)nudPPT.Value,
                            dtCpuTDC = (int)nudTDC.Value,
                            dtCpuEDC = (int)nudEDC.Value,
                            pboScalar = (int)nudPBOScaler.Value,
                            coAllCore = (int)nudAllCO.Value,

                            boostProfile = (int)cbxBoost.SelectedIndex,

                            rsr = (int)nudRSR.Value,
                            boost = (int)nudBoost.Value,
                            imageSharp = (int)nudImageSharp.Value,
                            isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                            isRSR = (bool)cbRSR.IsChecked,
                            isBoost = (bool)cbBoost.IsChecked,
                            isAntiLag = (bool)cbAntiLag.IsChecked,
                            isImageSharp = (bool)cbImageSharp.IsChecked,
                            isSync = (bool)cbSync.IsChecked,

                            commandValue = getCommandValues(),


                            isDtCpuTemp = (bool)cbCPUTemp.IsChecked,
                            isDtCpuPPT = (bool)cbPPT.IsChecked,
                            isDtCpuTDC = (bool)cbTDC.IsChecked,
                            isDtCpuEDC = (bool)cbEDC.IsChecked,
                            isPboScalar = (bool)cbPBOScaler.IsChecked,
                            isCoAllCore = (bool)cbAllCO.IsChecked,

                            coGfx = (int)nudGfxCO.Value,
                            isCoGfx = (bool)cbGfxCO.IsChecked,

                            isNVIDIA = (bool)tsNV.IsChecked,
                            nvMaxCoreClk = (int)nudNVMaxCore.Value,
                            nvCoreClk = (int)nudNVCore.Value,
                            nvMemClk = (int)nudNVMem.Value,
                            nvPower = (int)nudNVPower.Value,

                            ccd1Core1 = (int)nudCCD1Core1.Value,
                            ccd1Core2 = (int)nudCCD1Core2.Value,
                            ccd1Core3 = (int)nudCCD1Core3.Value,
                            ccd1Core4 = (int)nudCCD1Core4.Value,
                            ccd1Core5 = (int)nudCCD1Core5.Value,
                            ccd1Core6 = (int)nudCCD1Core6.Value,
                            ccd1Core7 = (int)nudCCD1Core7.Value,
                            ccd1Core8 = (int)nudCCD1Core8.Value,

                            ccd2Core1 = (int)nudCCD2Core1.Value,
                            ccd2Core2 = (int)nudCCD2Core2.Value,
                            ccd2Core3 = (int)nudCCD2Core3.Value,
                            ccd2Core4 = (int)nudCCD2Core4.Value,
                            ccd2Core5 = (int)nudCCD2Core5.Value,
                            ccd2Core6 = (int)nudCCD2Core6.Value,
                            ccd2Core7 = (int)nudCCD2Core7.Value,
                            ccd2Core8 = (int)nudCCD2Core8.Value,

                            IsCCD1Core1 = (bool)cbCCD1Core1.IsChecked,
                            IsCCD1Core2 = (bool)cbCCD1Core2.IsChecked,
                            IsCCD1Core3 = (bool)cbCCD1Core3.IsChecked,
                            IsCCD1Core4 = (bool)cbCCD1Core4.IsChecked,
                            IsCCD1Core5 = (bool)cbCCD1Core5.IsChecked,
                            IsCCD1Core6 = (bool)cbCCD1Core6.IsChecked,
                            IsCCD1Core7 = (bool)cbCCD1Core7.IsChecked,
                            IsCCD1Core8 = (bool)cbCCD1Core8.IsChecked,

                            IsCCD2Core1 = (bool)cbCCD2Core1.IsChecked,
                            IsCCD2Core2 = (bool)cbCCD2Core2.IsChecked,
                            IsCCD2Core3 = (bool)cbCCD2Core3.IsChecked,
                            IsCCD2Core4 = (bool)cbCCD2Core4.IsChecked,
                            IsCCD2Core5 = (bool)cbCCD2Core5.IsChecked,
                            IsCCD2Core6 = (bool)cbCCD2Core6.IsChecked,
                            IsCCD2Core7 = (bool)cbCCD2Core7.IsChecked,
                            IsCCD2Core8 = (bool)cbCCD2Core8.IsChecked,

                            IsAmdOC = (bool)tsAmdOC.IsChecked,
                            amdClock = (int)nudAmdCpuClk.Value,
                            amdVID = (int)nudAmdVID.Value,

                            asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                            asusiGPU = (bool)tsASUSEco.IsChecked,
                            asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                            displayHz = (int)cbxRefreshRate.SelectedIndex,

                            isMag = (bool)tsUXTUSR.IsChecked,
                            isVsync = (bool)cbVSync.IsChecked,
                            isRecap = (bool)cbAutoCap.IsChecked,
                            Sharpness = (int)nudSharp.Value,
                            ResScaleIndex = (int)cbxResScale.SelectedIndex,

                            powerMode = (int)cbxPowerMode.SelectedIndex,
                            windowsBoostMode = cbxWindowsBoostMode.SelectedIndex,
                            isWindowsMinState = (bool)cbWindowsMinState.IsChecked,
                            windowsMinState = (int)nudWindowsMinState.Value,
                            isWindowsMaxState = (bool)cbWindowsMaxState.IsChecked,
                            windowsMaxState = (int)nudWindowsMaxState.Value,
                            isWindowsMaxFrequency = (bool)cbWindowsMaxFrequency.IsChecked,
                            windowsMaxFrequency = (int)nudWindowsMaxFrequency.Value,
                            isWindowsEpp = (bool)cbWindowsEpp.IsChecked,
                            windowsEpp = (int)nudWindowsEpp.Value,
                            isWindowsCoreParking = (bool)cbWindowsCoreParking.IsChecked,
                            windowsCoreParking = (int)nudWindowsCoreParking.Value,
                            isWindowsMaxUnparkedCores = (bool)cbWindowsMaxUnparkedCores.IsChecked,
                            windowsMaxUnparkedCores = (int)nudWindowsMaxUnparkedCores.Value,
                            ccdAffinity = (int)cbxCcdAffinity.SelectedIndex,
                        };
                        presetManager.SavePreset(tbxPresetName.Text, preset);
                        if (!cbxPowerPreset.Items.Contains(tbxPresetName.Text))
                            cbxPowerPreset.Items.Add(tbxPresetName.Text);

                        cbxPowerPreset.Text = tbxPresetName.Text;
                        Settings.Default.cstmPreset = tbxPresetName.Text;
                        Settings.Default.Save();
                        ToastNotification.ShowToastNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    if (tbxPresetName.Text != "" && tbxPresetName.Text != null)
                    {
                        // Save a preset
                        Preset preset = new Preset
                        {
                            IntelPL1 = (int)nudIntelPL1.Value,
                            IntelPL2 = (int)nudIntelPL2.Value,
                            IntelVoltCPU = (int)nudIntelCoreUV.Value,
                            IntelVoltGPU = (int)nudIntelGfxUV.Value,
                            IntelVoltCache = (int)nudIntelCacheUV.Value,
                            IntelVoltSA = (int)nudIntelSAUV.Value,
                            IntelBalCPU = (int)nudIntelCpuBal.Value,
                            IntelBalGPU = (int)nudIntelGpuBal.Value,

                            isApuGfxClk = (bool)cbAPUiGPUClk.IsChecked,
                            apuGfxClk = (int)nudAPUiGPUClk.Value,

                            rsr = (int)nudRSR.Value,
                            boost = (int)nudBoost.Value,
                            imageSharp = (int)nudImageSharp.Value,
                            isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                            isRSR = (bool)cbRSR.IsChecked,
                            isBoost = (bool)cbBoost.IsChecked,
                            isAntiLag = (bool)cbAntiLag.IsChecked,
                            isImageSharp = (bool)cbImageSharp.IsChecked,
                            isSync = (bool)cbSync.IsChecked,

                            commandValue = getCommandValues(),

                            isIntelPL1 = (bool)cbIntelPL1.IsChecked,
                            isIntelPL2 = (bool)cbIntelPL2.IsChecked,
                            IsIntelVolt = (bool)tsIntelUV.IsChecked,
                            IsIntelBal = (bool)tsIntelBal.IsChecked,

                            isNVIDIA = (bool)tsNV.IsChecked,
                            nvMaxCoreClk = (int)nudNVMaxCore.Value,
                            nvCoreClk = (int)nudNVCore.Value,
                            nvMemClk = (int)nudNVMem.Value,
                            nvPower = (int)nudNVPower.Value,

                            asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                            asusiGPU = (bool)tsASUSEco.IsChecked,
                            asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                            displayHz = (int)cbxRefreshRate.SelectedIndex,

                            isMag = (bool)tsUXTUSR.IsChecked,
                            isVsync = (bool)cbVSync.IsChecked,
                            isRecap = (bool)cbAutoCap.IsChecked,
                            Sharpness = (int)nudSharp.Value,
                            ResScaleIndex = (int)cbxResScale.SelectedIndex,

                            powerMode = (int)cbxPowerMode.SelectedIndex,
                            windowsBoostMode = cbxWindowsBoostMode.SelectedIndex,
                            isWindowsMinState = (bool)cbWindowsMinState.IsChecked,
                            windowsMinState = (int)nudWindowsMinState.Value,
                            isWindowsMaxState = (bool)cbWindowsMaxState.IsChecked,
                            windowsMaxState = (int)nudWindowsMaxState.Value,
                            isWindowsMaxFrequency = (bool)cbWindowsMaxFrequency.IsChecked,
                            windowsMaxFrequency = (int)nudWindowsMaxFrequency.Value,
                            isWindowsEpp = (bool)cbWindowsEpp.IsChecked,
                            windowsEpp = (int)nudWindowsEpp.Value,
                            isWindowsCoreParking = (bool)cbWindowsCoreParking.IsChecked,
                            windowsCoreParking = (int)nudWindowsCoreParking.Value,
                            isWindowsMaxUnparkedCores = (bool)cbWindowsMaxUnparkedCores.IsChecked,
                            windowsMaxUnparkedCores = (int)nudWindowsMaxUnparkedCores.Value,
                            ccdAffinity = (int)cbxCcdAffinity.SelectedIndex,

                            isIntelClockRatio = (bool)tsIntelRatioCore.IsChecked,
                            intelClockRatioC1 = (int)nudIntelRatioC1.Value,
                            intelClockRatioC2 = (int)nudIntelRatioC2.Value,
                            intelClockRatioC3 = (int)nudIntelRatioC3.Value,
                            intelClockRatioC4 = (int)nudIntelRatioC4.Value,
                            intelClockRatioC5 = (int)nudIntelRatioC5.Value,
                            intelClockRatioC6 = (int)nudIntelRatioC6.Value,
                            intelClockRatioC7 = (int)nudIntelRatioC7.Value,
                            intelClockRatioC8 = (int)nudIntelRatioC8.Value,
                        };
                        presetManager.SavePreset(tbxPresetName.Text, preset);
                        if (!cbxPowerPreset.Items.Contains(tbxPresetName.Text))
                            cbxPowerPreset.Items.Add(tbxPresetName.Text);

                        cbxPowerPreset.Text = tbxPresetName.Text;
                        Settings.Default.cstmPreset = tbxPresetName.Text;
                        Settings.Default.Save();
                        ToastNotification.ShowToastNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");
                    }
                }
            }
        }

        private void cbxPowerPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateValues(((System.Windows.Controls.ComboBox)sender).SelectedItem as string);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    if (cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null)
                    {
                        string deletePresetName = cbxPowerPreset.Text;
                        presetManager.DeletePreset(deletePresetName);
                        cbxPowerPreset.Items.Remove(deletePresetName);

                        updateValues("");
                        ToastNotification.ShowToastNotification("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    if (cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null)
                    {
                        string deletePresetName = cbxPowerPreset.Text;
                        presetManager.DeletePreset(deletePresetName);

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = presetManager.GetPresetNames();
                        cbxPowerPreset.Items.Remove(deletePresetName);

                        updateValues("");
                        ToastNotification.ShowToastNotification("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    if (cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null)
                    {
                        string deletePresetName = cbxPowerPreset.Text;
                        presetManager.DeletePreset(deletePresetName);
                        cbxPowerPreset.Items.Remove(deletePresetName);

                        updateValues("");
                        ToastNotification.ShowToastNotification("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to delete preset");
            }
        }


        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            ReloadPresetValues(cbxPowerPreset.SelectedItem as string ?? cbxPowerPreset.Text);
        }

        private string? GetSelectedPresetName()
        {
            var name = cbxPowerPreset.SelectedItem as string ?? cbxPowerPreset.Text;
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }

        private static string GetPresetFileName() => Family.TYPE switch
        {
            Family.ProcessorType.Amd_Apu => "apuPresets.json",
            Family.ProcessorType.Amd_Desktop_Cpu => "amdDtCpuPresets.json",
            Family.ProcessorType.Intel => "intelPresets.json",
            _ => "apuPresets.json"
        };

        private void ReloadPresetList()
        {
            var selectedPreset = GetSelectedPresetName() ?? Settings.Default.cstmPreset;
            presetManager = new PresetManager(Settings.Default.Path + GetPresetFileName());
            var presetNames = presetManager.GetPresetNames()
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            cbxPowerPreset.Items.Clear();
            foreach (var presetName in presetNames)
            {
                cbxPowerPreset.Items.Add(presetName);
            }

            cbxPowerPreset.Text = selectedPreset ?? string.Empty;
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
                cbxPowerPreset.Text = presetName;
                Settings.Default.cstmPreset = presetName;
                Settings.Default.Save();

                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = string.IsNullOrWhiteSpace(presetName) ? DefaultAPUPreset : presetManager.GetPreset(presetName) ?? DefaultAPUPreset;

                    // Read the values from the preset
                    nudAPUSkinTemp.Value = myPreset.apuSkinTemp;
                    nudAPUTemp.Value = myPreset.apuTemp;
                    nudSTAPMPow.Value = myPreset.apuSTAPMPow;
                    nudFastPow.Value = myPreset.apuFastPow;
                    nudSlowPow.Value = myPreset.apuSlowPow;
                    nudSlowTime.Value = myPreset.apuSlowTime;
                    nudFastTime.Value = myPreset.apuSTAPMTime;

                    cbAPUTemp.IsChecked = myPreset.isApuTemp;
                    cbAPUSkinTemp.IsChecked = myPreset.isApuSkinTemp;
                    cbSTAPMPow.IsChecked = myPreset.isApuSTAPMPow;
                    cbSlowPow.IsChecked = myPreset.isApuSlowPow;
                    cbSlowTime.IsChecked = myPreset.isApuSlowTime;
                    cbFastPow.IsChecked = myPreset.isApuFastPow;
                    cbFastTime.IsChecked = myPreset.isApuSTAPMTime;

                    nudCpuVrmTdc.Value = myPreset.apuCpuTdc;
                    nudCpuVrmEdc.Value = myPreset.apuCpuEdc;
                    nudGfxVrmTdc.Value = myPreset.apuGfxTdc;
                    nudGfxVrmEdc.Value = myPreset.apuGfxEdc;
                    nudSocVrmTdc.Value = myPreset.apuSocTdc;
                    nudSocVrmEdc.Value = myPreset.apuSocEdc;

                    cbCpuVrmTdc.IsChecked = myPreset.isApuCpuTdc;
                    cbCpuVrmEdc.IsChecked = myPreset.isApuCpuEdc;
                    cbGfxVrmTdc.IsChecked = myPreset.isApuGfxTdc;
                    cbGfxVrmEdc.IsChecked = myPreset.isApuGfxEdc;
                    cbSocVrmTdc.IsChecked = myPreset.isApuSocTdc;
                    cbSocVrmEdc.IsChecked = myPreset.isApuSocEdc;

                    nudAPUiGPUClk.Value = myPreset.apuGfxClk;

                    cbAPUiGPUClk.IsChecked = myPreset.isApuGfxClk;

                    nudPBOScaler.Value = myPreset.pboScalar;
                    nudAllCO.Value = myPreset.coAllCore;
                    nudGfxCO.Value = myPreset.coGfx;

                    cbPBOScaler.IsChecked = myPreset.isPboScalar;
                    cbAllCO.IsChecked = myPreset.isCoAllCore;
                    cbGfxCO.IsChecked = myPreset.isCoGfx;

                    tsRadeonGraph.IsChecked = myPreset.isRadeonGraphics;
                    cbAntiLag.IsChecked = myPreset.isAntiLag;
                    cbRSR.IsChecked = myPreset.isRSR;
                    cbBoost.IsChecked = myPreset.isBoost;
                    cbImageSharp.IsChecked = myPreset.isImageSharp;
                    cbSync.IsChecked = myPreset.isSync;
                    nudRSR.Value = myPreset.rsr;
                    nudBoost.Value = myPreset.boost;
                    nudImageSharp.Value = myPreset.imageSharp;

                    nudCCD1Core1.Value = myPreset.ccd1Core1;
                    nudCCD1Core2.Value = myPreset.ccd1Core2;
                    nudCCD1Core3.Value = myPreset.ccd1Core3;
                    nudCCD1Core4.Value = myPreset.ccd1Core4;
                    nudCCD1Core5.Value = myPreset.ccd1Core5;
                    nudCCD1Core6.Value = myPreset.ccd1Core6;
                    nudCCD1Core7.Value = myPreset.ccd1Core7;
                    nudCCD1Core8.Value = myPreset.ccd1Core8;

                    cbCCD1Core1.IsChecked = myPreset.IsCCD1Core1;
                    cbCCD1Core2.IsChecked = myPreset.IsCCD1Core2;
                    cbCCD1Core3.IsChecked = myPreset.IsCCD1Core3;
                    cbCCD1Core4.IsChecked = myPreset.IsCCD1Core4;
                    cbCCD1Core5.IsChecked = myPreset.IsCCD1Core5;
                    cbCCD1Core6.IsChecked = myPreset.IsCCD1Core6;
                    cbCCD1Core7.IsChecked = myPreset.IsCCD1Core7;
                    cbCCD1Core8.IsChecked = myPreset.IsCCD1Core8;

                    nudCCD2Core1.Value = myPreset.ccd2Core1;
                    nudCCD2Core2.Value = myPreset.ccd2Core2;
                    nudCCD2Core3.Value = myPreset.ccd2Core3;
                    nudCCD2Core4.Value = myPreset.ccd2Core4;
                    nudCCD2Core5.Value = myPreset.ccd2Core5;
                    nudCCD2Core6.Value = myPreset.ccd2Core6;
                    nudCCD2Core7.Value = myPreset.ccd2Core7;
                    nudCCD2Core8.Value = myPreset.ccd2Core8;

                    cbCCD2Core1.IsChecked = myPreset.IsCCD2Core1;
                    cbCCD2Core2.IsChecked = myPreset.IsCCD2Core2;
                    cbCCD2Core3.IsChecked = myPreset.IsCCD2Core3;
                    cbCCD2Core4.IsChecked = myPreset.IsCCD2Core4;
                    cbCCD2Core5.IsChecked = myPreset.IsCCD2Core5;
                    cbCCD2Core6.IsChecked = myPreset.IsCCD2Core6;
                    cbCCD2Core7.IsChecked = myPreset.IsCCD2Core7;
                    cbCCD2Core8.IsChecked = myPreset.IsCCD2Core8;

                    cbxBoost.SelectedIndex = myPreset.boostProfile;

                    tsNV.IsChecked = myPreset.isNVIDIA;
                    nudNVMaxCore.Value = myPreset.nvMaxCoreClk;
                    nudNVCore.Value = myPreset.nvCoreClk;
                    nudNVMem.Value = myPreset.nvMemClk;
                    if(myPreset.nvPower > 0) nudNVPower.Value = myPreset.nvPower;

                    tsAmdOC.IsChecked = myPreset.IsAmdOC;
                    nudAmdCpuClk.Value = myPreset.amdClock;
                    nudAmdVID.Value = myPreset.amdVID;

                    nudSoftMiniGPUClk.Value = myPreset.softMiniGPUClk;
                    nudSoftMinCPUClk.Value = myPreset.softMinCPUClk;
                    nudSoftMinFabClk.Value = myPreset.softMinFabClk;
                    nudSoftMinSoCClk.Value = myPreset.softMinSoCClk;
                    nudSoftMinDataClk.Value = myPreset.softMinDataClk;

                    nudSoftMaxiGPUClk.Value = myPreset.softMaxiGPUClk;
                    nudSoftMaxCPUClk.Value = myPreset.softMaxCPUClk;
                    nudSoftMaxFabClk.Value = myPreset.softMaxFabClk;
                    nudSoftMaxSoCClk.Value = myPreset.softMaxSoCClk;
                    nudSoftMaxDataClk.Value = myPreset.softMaxDataClk;

                    cbSoftMiniGPUClk.IsChecked = myPreset.isSoftMiniGPUClk;
                    cbSoftMinCPUClk.IsChecked = myPreset.isSoftMinCPUClk;
                    cbSoftMinFabClk.IsChecked = myPreset.isSoftMinFabClk;
                    cbSoftMinSoCClk.IsChecked = myPreset.isSoftMinSoCClk;
                    cbSoftMinDataClk.IsChecked = myPreset.isSoftMinDataClk;

                    cbSoftMaxiGPUClk.IsChecked = myPreset.isSoftMaxiGPUClk;
                    cbSoftMaxCPUClk.IsChecked = myPreset.isSoftMaxCPUClk;
                    cbSoftMaxFabClk.IsChecked = myPreset.isSoftMaxFabClk;
                    cbSoftMaxSoCClk.IsChecked = myPreset.isSoftMaxSoCClk;
                    cbSoftMaxDataClk.IsChecked = myPreset.isSoftMaxDataClk;

                    tsASUSUlti.IsChecked = myPreset.asusGPUUlti;
                    tsASUSEco.IsChecked = myPreset.asusiGPU;
                    cbxAsusPower.SelectedIndex = myPreset.asusPowerProfile;

                    if (myPreset.displayHz <= cbxRefreshRate.Items.Count) cbxRefreshRate.SelectedIndex = myPreset.displayHz;

                    cbxPowerMode.SelectedIndex = myPreset.powerMode;
                    cbxWindowsBoostMode.SelectedIndex = myPreset.windowsBoostMode;
                    cbWindowsMinState.IsChecked = myPreset.isWindowsMinState;
                    nudWindowsMinState.Value = myPreset.windowsMinState;
                    cbWindowsMaxState.IsChecked = myPreset.isWindowsMaxState;
                    nudWindowsMaxState.Value = myPreset.windowsMaxState;
                    cbWindowsMaxFrequency.IsChecked = myPreset.isWindowsMaxFrequency;
                    nudWindowsMaxFrequency.Value = myPreset.windowsMaxFrequency;
                    cbWindowsEpp.IsChecked = myPreset.isWindowsEpp;
                    nudWindowsEpp.Value = myPreset.windowsEpp;
                    cbWindowsCoreParking.IsChecked = myPreset.isWindowsCoreParking;
                    nudWindowsCoreParking.Value = myPreset.windowsCoreParking;
                    cbWindowsMaxUnparkedCores.IsChecked = myPreset.isWindowsMaxUnparkedCores;
                    nudWindowsMaxUnparkedCores.Value = myPreset.windowsMaxUnparkedCores;
                    cbxCcdAffinity.SelectedIndex = myPreset.ccdAffinity;

                    tsUXTUSR.IsChecked = myPreset.isMag;
                    cbVSync.IsChecked = myPreset.isVsync;
                    cbAutoCap.IsChecked = myPreset.isRecap;
                    nudSharp.Value = myPreset.Sharpness;
                    cbxResScale.SelectedIndex = myPreset.ResScaleIndex;
                } else if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = string.IsNullOrWhiteSpace(presetName) ? DefaultAMDDtCPUPreset : presetManager.GetPreset(presetName) ?? DefaultAMDDtCPUPreset;

                    // Read the values from the preset
                    nudCPUTemp.Value = myPreset.dtCpuTemp;
                    nudPPT.Value = myPreset.dtCpuPPT;
                    nudTDC.Value = myPreset.dtCpuTDC;
                    nudEDC.Value = myPreset.dtCpuEDC;

                    cbCPUTemp.IsChecked = myPreset.isDtCpuTemp;
                    cbPPT.IsChecked = myPreset.isDtCpuPPT;
                    cbTDC.IsChecked = myPreset.isDtCpuTDC;
                    cbEDC.IsChecked = myPreset.isDtCpuEDC;

                    nudPBOScaler.Value = myPreset.pboScalar;
                    nudAllCO.Value = myPreset.coAllCore;
                    nudGfxCO.Value = myPreset.coGfx;

                    cbPBOScaler.IsChecked = myPreset.isPboScalar;
                    cbAllCO.IsChecked = myPreset.isCoAllCore;
                    cbGfxCO.IsChecked = myPreset.isCoGfx;

                    tsRadeonGraph.IsChecked = myPreset.isRadeonGraphics;
                    cbAntiLag.IsChecked = myPreset.isAntiLag;
                    cbRSR.IsChecked = myPreset.isRSR;
                    cbBoost.IsChecked = myPreset.isBoost;
                    cbImageSharp.IsChecked = myPreset.isImageSharp;
                    cbSync.IsChecked = myPreset.isSync;
                    nudRSR.Value = myPreset.rsr;
                    nudBoost.Value = myPreset.boost;
                    nudImageSharp.Value = myPreset.imageSharp;

                    nudCCD1Core1.Value = myPreset.ccd1Core1;
                    nudCCD1Core2.Value = myPreset.ccd1Core2;
                    nudCCD1Core3.Value = myPreset.ccd1Core3;
                    nudCCD1Core4.Value = myPreset.ccd1Core4;
                    nudCCD1Core5.Value = myPreset.ccd1Core5;
                    nudCCD1Core6.Value = myPreset.ccd1Core6;
                    nudCCD1Core7.Value = myPreset.ccd1Core7;
                    nudCCD1Core8.Value = myPreset.ccd1Core8;

                    cbCCD1Core1.IsChecked = myPreset.IsCCD1Core1;
                    cbCCD1Core2.IsChecked = myPreset.IsCCD1Core2;
                    cbCCD1Core3.IsChecked = myPreset.IsCCD1Core3;
                    cbCCD1Core4.IsChecked = myPreset.IsCCD1Core4;
                    cbCCD1Core5.IsChecked = myPreset.IsCCD1Core5;
                    cbCCD1Core6.IsChecked = myPreset.IsCCD1Core6;
                    cbCCD1Core7.IsChecked = myPreset.IsCCD1Core7;
                    cbCCD1Core8.IsChecked = myPreset.IsCCD1Core8;

                    nudCCD2Core1.Value = myPreset.ccd2Core1;
                    nudCCD2Core2.Value = myPreset.ccd2Core2;
                    nudCCD2Core3.Value = myPreset.ccd2Core3;
                    nudCCD2Core4.Value = myPreset.ccd2Core4;
                    nudCCD2Core5.Value = myPreset.ccd2Core5;
                    nudCCD2Core6.Value = myPreset.ccd2Core6;
                    nudCCD2Core7.Value = myPreset.ccd2Core7;
                    nudCCD2Core8.Value = myPreset.ccd2Core8;

                    cbCCD2Core1.IsChecked = myPreset.IsCCD2Core1;
                    cbCCD2Core2.IsChecked = myPreset.IsCCD2Core2;
                    cbCCD2Core3.IsChecked = myPreset.IsCCD2Core3;
                    cbCCD2Core4.IsChecked = myPreset.IsCCD2Core4;
                    cbCCD2Core5.IsChecked = myPreset.IsCCD2Core5;
                    cbCCD2Core6.IsChecked = myPreset.IsCCD2Core6;
                    cbCCD2Core7.IsChecked = myPreset.IsCCD2Core7;
                    cbCCD2Core8.IsChecked = myPreset.IsCCD2Core8;

                    tsNV.IsChecked = myPreset.isNVIDIA;
                    nudNVMaxCore.Value = myPreset.nvMaxCoreClk;
                    nudNVCore.Value = myPreset.nvCoreClk;
                    nudNVMem.Value = myPreset.nvMemClk;
                    if (myPreset.nvPower > 0) nudNVPower.Value = myPreset.nvPower;

                    tsAmdOC.IsChecked = myPreset.IsAmdOC;
                    nudAmdCpuClk.Value = myPreset.amdClock;
                    nudAmdVID.Value = myPreset.amdVID;

                    tsASUSUlti.IsChecked = myPreset.asusGPUUlti;
                    tsASUSEco.IsChecked = myPreset.asusiGPU;
                    cbxAsusPower.SelectedIndex = myPreset.asusPowerProfile;

                    if (myPreset.displayHz <= cbxRefreshRate.Items.Count) cbxRefreshRate.SelectedIndex = myPreset.displayHz;

                    cbxPowerMode.SelectedIndex = myPreset.powerMode;
                    cbxWindowsBoostMode.SelectedIndex = myPreset.windowsBoostMode;
                    cbWindowsMinState.IsChecked = myPreset.isWindowsMinState;
                    nudWindowsMinState.Value = myPreset.windowsMinState;
                    cbWindowsMaxState.IsChecked = myPreset.isWindowsMaxState;
                    nudWindowsMaxState.Value = myPreset.windowsMaxState;
                    cbWindowsMaxFrequency.IsChecked = myPreset.isWindowsMaxFrequency;
                    nudWindowsMaxFrequency.Value = myPreset.windowsMaxFrequency;
                    cbWindowsEpp.IsChecked = myPreset.isWindowsEpp;
                    nudWindowsEpp.Value = myPreset.windowsEpp;
                    cbWindowsCoreParking.IsChecked = myPreset.isWindowsCoreParking;
                    nudWindowsCoreParking.Value = myPreset.windowsCoreParking;
                    cbWindowsMaxUnparkedCores.IsChecked = myPreset.isWindowsMaxUnparkedCores;
                    nudWindowsMaxUnparkedCores.Value = myPreset.windowsMaxUnparkedCores;
                    cbxCcdAffinity.SelectedIndex = myPreset.ccdAffinity;

                    tsUXTUSR.IsChecked = myPreset.isMag;
                    cbVSync.IsChecked = myPreset.isVsync;
                    cbAutoCap.IsChecked = myPreset.isRecap;
                    nudSharp.Value = myPreset.Sharpness;
                    cbxResScale.SelectedIndex = myPreset.ResScaleIndex;
                } else if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = string.IsNullOrWhiteSpace(presetName) ? DefaultIntelPreset : presetManager.GetPreset(presetName) ?? DefaultIntelPreset;

                    // Read the values from the preset
                    nudIntelPL1.Value = myPreset.IntelPL1;
                    nudIntelPL2.Value = myPreset.IntelPL2;

                    cbIntelPL1.IsChecked = myPreset.isIntelPL1;
                    cbIntelPL2.IsChecked = myPreset.isIntelPL2;

                    nudAPUiGPUClk.Value = myPreset.apuGfxClk;

                    cbAPUiGPUClk.IsChecked = myPreset.isApuGfxClk;

                    tsRadeonGraph.IsChecked = myPreset.isRadeonGraphics;
                    cbAntiLag.IsChecked = myPreset.isAntiLag;
                    cbRSR.IsChecked = myPreset.isRSR;
                    cbBoost.IsChecked = myPreset.isBoost;
                    cbImageSharp.IsChecked = myPreset.isImageSharp;
                    cbSync.IsChecked = myPreset.isSync;
                    nudRSR.Value = myPreset.rsr;
                    nudBoost.Value = myPreset.boost;
                    nudImageSharp.Value = myPreset.imageSharp;

                    tsNV.IsChecked = myPreset.isNVIDIA;
                    nudNVMaxCore.Value = myPreset.nvMaxCoreClk;
                    nudNVCore.Value = myPreset.nvCoreClk;
                    nudNVMem.Value = myPreset.nvMemClk;
                    if (myPreset.nvPower > 0) nudNVPower.Value = myPreset.nvPower;

                    tsASUSUlti.IsChecked = myPreset.asusGPUUlti;
                    tsASUSEco.IsChecked = myPreset.asusiGPU;
                    cbxAsusPower.SelectedIndex = myPreset.asusPowerProfile;

                    if (myPreset.displayHz <= cbxRefreshRate.Items.Count) cbxRefreshRate.SelectedIndex = myPreset.displayHz;

                    cbxPowerMode.SelectedIndex = myPreset.powerMode;
                    cbxWindowsBoostMode.SelectedIndex = myPreset.windowsBoostMode;
                    cbWindowsMinState.IsChecked = myPreset.isWindowsMinState;
                    nudWindowsMinState.Value = myPreset.windowsMinState;
                    cbWindowsMaxState.IsChecked = myPreset.isWindowsMaxState;
                    nudWindowsMaxState.Value = myPreset.windowsMaxState;
                    cbWindowsMaxFrequency.IsChecked = myPreset.isWindowsMaxFrequency;
                    nudWindowsMaxFrequency.Value = myPreset.windowsMaxFrequency;
                    cbWindowsEpp.IsChecked = myPreset.isWindowsEpp;
                    nudWindowsEpp.Value = myPreset.windowsEpp;
                    cbWindowsCoreParking.IsChecked = myPreset.isWindowsCoreParking;
                    nudWindowsCoreParking.Value = myPreset.windowsCoreParking;
                    cbWindowsMaxUnparkedCores.IsChecked = myPreset.isWindowsMaxUnparkedCores;
                    nudWindowsMaxUnparkedCores.Value = myPreset.windowsMaxUnparkedCores;
                    cbxCcdAffinity.SelectedIndex = myPreset.ccdAffinity;

                    tsUXTUSR.IsChecked = myPreset.isMag;
                    cbVSync.IsChecked = myPreset.isVsync;
                    cbAutoCap.IsChecked = myPreset.isRecap;
                    nudSharp.Value = myPreset.Sharpness;
                    cbxResScale.SelectedIndex = myPreset.ResScaleIndex;

                    tsIntelUV.IsChecked = myPreset.IsIntelVolt;
                    nudIntelCoreUV.Value = myPreset.IntelVoltCPU;
                    nudIntelGfxUV.Value = myPreset.IntelVoltGPU;
                    nudIntelCacheUV.Value = myPreset.IntelVoltCache;
                    nudIntelSAUV.Value = myPreset.IntelVoltSA;

                    tsIntelBal.IsChecked = myPreset.IsIntelBal;
                    nudIntelCpuBal.Value = myPreset.IntelBalCPU;
                    nudIntelGpuBal.Value = myPreset.IntelBalGPU;

                    tsIntelRatioCore.IsChecked = myPreset.isIntelClockRatio;
                    nudIntelRatioC1.Value = myPreset.intelClockRatioC1;
                    nudIntelRatioC2.Value = myPreset.intelClockRatioC2;
                    nudIntelRatioC3.Value = myPreset.intelClockRatioC3;
                    nudIntelRatioC4.Value = myPreset.intelClockRatioC4;
                    nudIntelRatioC5.Value = myPreset.intelClockRatioC5;
                    nudIntelRatioC6.Value = myPreset.intelClockRatioC6;
                    nudIntelRatioC7.Value = myPreset.intelClockRatioC7;
                    nudIntelRatioC8.Value = myPreset.intelClockRatioC8;
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

        public string getCommandValues()
        {
            string commandValues = "";

            commandValues = commandValues + $"--UXTUSR={tsUXTUSR.IsChecked}-{cbVSync.IsChecked}-{nudSharp.Value / 100}-{cbxResScale.SelectedIndex}-{cbAutoCap.IsChecked} ";

            if (Settings.Default.isASUS)
            {
                if (cbxAsusPower.SelectedIndex > 0) commandValues = commandValues + $"--ASUS-Power={cbxAsusPower.SelectedIndex} ";
                if (sdAsusEco.Visibility == Visibility.Visible) commandValues = commandValues + $"--ASUS-Eco={tsASUSEco.IsChecked} ";
                if (sdAsusUlti.Visibility == Visibility.Visible) commandValues = commandValues + $"--ASUS-MUX={tsASUSUlti.IsChecked} ";
            }

            if (sdRefreshRate.Visibility == Visibility.Visible && cbxRefreshRate.SelectedIndex > 0) commandValues = commandValues + $"--Refresh-Rate={Display.uniqueRefreshRates[cbxRefreshRate.SelectedIndex - 1]} ";

            if (sdPowerMode.Visibility == Visibility.Visible && cbxPowerMode.SelectedIndex > 0) commandValues = commandValues + $"--Win-Power={cbxPowerMode.SelectedIndex - 1} ";

            var windowsBoostMode = cbxWindowsBoostMode.SelectedIndex > 0 ? cbxWindowsBoostMode.SelectedIndex - 1 : -1;
            var windowsMinState = cbWindowsMinState.IsChecked == true ? (int)nudWindowsMinState.Value : -1;
            var windowsMaxState = cbWindowsMaxState.IsChecked == true ? (int)nudWindowsMaxState.Value : -1;
            var windowsMaxFrequency = cbWindowsMaxFrequency.IsChecked == true ? (int)nudWindowsMaxFrequency.Value : -1;
            var windowsEpp = cbWindowsEpp.IsChecked == true ? (int)nudWindowsEpp.Value : -1;
            var windowsCoreParking = cbWindowsCoreParking.IsChecked == true ? (int)nudWindowsCoreParking.Value : -1;
            var windowsMaxUnparkedCores = cbWindowsMaxUnparkedCores.IsChecked == true ? (int)nudWindowsMaxUnparkedCores.Value : -1;
            if (windowsBoostMode >= 0 || windowsMinState >= 0 || windowsMaxState >= 0 || windowsMaxFrequency >= 0 || windowsEpp >= 0 || windowsCoreParking >= 0 || windowsMaxUnparkedCores >= 0)
                commandValues = commandValues + $"--Win-CPU={windowsBoostMode},{windowsMaxState},{windowsMaxFrequency},{windowsEpp},{windowsMinState},{windowsCoreParking},{windowsMaxUnparkedCores} ";

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                if (cbAPUTemp.IsChecked == true) commandValues = commandValues + $"--tctl-temp={nudAPUTemp.Value} --cHTC-temp={nudAPUTemp.Value} ";
                if (cbAPUSkinTemp.IsChecked == true) commandValues = commandValues + $"--apu-skin-temp={nudAPUSkinTemp.Value} ";
                if (cbSTAPMPow.IsChecked == true) commandValues = commandValues + $"--stapm-limit={nudSTAPMPow.Value * 1000} ";
                if (cbFastPow.IsChecked == true) commandValues = commandValues + $"--fast-limit={nudFastPow.Value * 1000} ";
                if (cbFastTime.IsChecked == true) commandValues = commandValues + $"--stapm-time={nudFastTime.Value} ";
                if (cbSlowPow.IsChecked == true) commandValues = commandValues + $"--slow-limit={nudSlowPow.Value * 1000} ";
                if (cbSlowTime.IsChecked == true) commandValues = commandValues + $"--slow-time={nudSlowTime.Value} ";
                if (cbCpuVrmTdc.IsChecked == true) commandValues = commandValues + $"--vrm-current={nudCpuVrmTdc.Value * 1000} ";
                if (cbCpuVrmEdc.IsChecked == true) commandValues = commandValues + $"--vrmmax-current={nudCpuVrmEdc.Value * 1000} ";
                if (cbSocVrmTdc.IsChecked == true) commandValues = commandValues + $"--vrmsoc-current={nudSocVrmTdc.Value * 1000} ";
                if (cbSocVrmEdc.IsChecked == true) commandValues = commandValues + $"--vrmsocmax-current={nudSocVrmEdc.Value * 1000} ";
                if (cbGfxVrmTdc.IsChecked == true) commandValues = commandValues + $"--vrmgfx-current={nudGfxVrmTdc.Value * 1000} ";
                if (cbGfxVrmEdc.IsChecked == true) commandValues = commandValues + $"--vrmgfxmax-current={nudGfxVrmEdc.Value * 1000} ";
                if (cbAPUiGPUClk.IsChecked == true) commandValues = commandValues + $"--gfx-clk={nudAPUiGPUClk.Value} ";
                if (cbPBOScaler.IsChecked == true) commandValues = commandValues + $"--pbo-scalar={nudPBOScaler.Value * 100} ";

                if (cbAllCO.IsChecked == true)
                {

                    if(Family.FAM < Family.RyzenFamily.Renoir) commandValues = commandValues + $"--set-coper={(0 << 20) | ((int)nudAllCO.Value & 0xFFFF)} ";
                    else
                    {
                        if (nudAllCO.Value >= 0) commandValues = commandValues + $"--set-coall={nudAllCO.Value} ";
                        if (nudAllCO.Value < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudAllCO.Value))} ";
                    }
                }

                if (cbGfxCO.IsChecked == true)
                {
                    if (nudGfxCO.Value >= 0) commandValues = commandValues + $"--set-cogfx={nudGfxCO.Value} ";
                    if (nudGfxCO.Value < 0) commandValues = commandValues + $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudGfxCO.Value))} ";
                }

                if (cbSoftMiniGPUClk.IsChecked == true) commandValues = commandValues + $"--min-gfxclk={nudSoftMiniGPUClk.Value} ";
                if (cbSoftMaxiGPUClk.IsChecked == true) commandValues = commandValues + $"--max-gfxclk={nudSoftMaxiGPUClk.Value} ";

                if (cbSoftMinCPUClk.IsChecked == true) commandValues = commandValues + $"--min-cpuclk={nudSoftMinCPUClk.Value} ";
                if (cbSoftMaxCPUClk.IsChecked == true) commandValues = commandValues + $"--max-cpuclk={nudSoftMaxCPUClk.Value} ";

                if (cbSoftMinDataClk.IsChecked == true) commandValues = commandValues + $"--min-lclk={nudSoftMinDataClk.Value} ";
                if (cbSoftMaxDataClk.IsChecked == true) commandValues = commandValues + $"--max-lclk={nudSoftMaxDataClk.Value} ";

                if (cbSoftMinVCNClk.IsChecked == true) commandValues = commandValues + $"--min-vcn={nudSoftMinVCNClk.Value} ";
                if (cbSoftMaxVCNClk.IsChecked == true) commandValues = commandValues + $"--max-vcn={nudSoftMaxVCNClk.Value} ";

                if (cbSoftMinFabClk.IsChecked == true) commandValues = commandValues + $"--min-fclk-frequency={nudSoftMinFabClk.Value} ";
                if (cbSoftMaxFabClk.IsChecked == true) commandValues = commandValues + $"--max-fclk-frequency={nudSoftMaxFabClk.Value} ";

                if (cbSoftMinSoCClk.IsChecked == true) commandValues = commandValues + $"--min-socclk-frequency={nudSoftMinSoCClk.Value} ";
                if (cbSoftMaxSoCClk.IsChecked == true) commandValues = commandValues + $"--max-socclk-frequency={nudSoftMaxSoCClk.Value} ";

                if (cbxBoost.SelectedIndex > 0)
                {
                    if (cbxBoost.SelectedIndex == 1) commandValues = commandValues + $"--power-saving ";
                    if (cbxBoost.SelectedIndex == 2) commandValues = commandValues + $"--max-performance ";
                }

                if (Family.FAM == Family.RyzenFamily.DragonRange || Family.FAM == Family.RyzenFamily.FireRange || Family.FAM == Family.RyzenFamily.StrixHalo)
                {
                    if (cbCCD1Core1.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 0, (int)nudCCD1Core1.Value)} ";
                    if (cbCCD1Core2.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 1, (int)nudCCD1Core2.Value)} ";
                    if (cbCCD1Core3.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 2, (int)nudCCD1Core3.Value)} ";
                    if (cbCCD1Core4.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 3, (int)nudCCD1Core4.Value)} ";
                    if (cbCCD1Core5.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 4, (int)nudCCD1Core5.Value)} ";
                    if (cbCCD1Core6.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 5, (int)nudCCD1Core6.Value)} ";
                    if (cbCCD1Core7.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 6, (int)nudCCD1Core7.Value)} ";
                    if (cbCCD1Core8.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 7, (int)nudCCD1Core8.Value)} ";
                    if (cbCCD1Core9.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 8, (int)nudCCD1Core9.Value)} ";
                    if (cbCCD1Core10.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 9, (int)nudCCD1Core10.Value)} ";
                    if (cbCCD1Core11.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 10, (int)nudCCD1Core11.Value)} ";
                    if (cbCCD1Core12.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 11, (int)nudCCD1Core12.Value)} ";

                    if (cbCCD2Core1.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 0, (int)nudCCD2Core1.Value)} ";
                    if (cbCCD2Core2.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 1, (int)nudCCD2Core2.Value)} ";
                    if (cbCCD2Core3.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 2, (int)nudCCD2Core3.Value)} ";
                    if (cbCCD2Core4.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 3, (int)nudCCD2Core4.Value)} ";
                    if (cbCCD2Core5.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 4, (int)nudCCD2Core5.Value)} ";
                    if (cbCCD2Core6.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 5, (int)nudCCD2Core6.Value)} ";
                    if (cbCCD2Core7.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 6, (int)nudCCD2Core7.Value)} ";
                    if (cbCCD2Core8.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 7, (int)nudCCD2Core8.Value)} ";
                    if (cbCCD2Core9.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 8, (int)nudCCD2Core9.Value)} ";
                    if (cbCCD2Core10.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 9, (int)nudCCD2Core10.Value)} ";
                    if (cbCCD2Core11.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 10, (int)nudCCD2Core11.Value)} ";
                    if (cbCCD2Core12.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 11, (int)nudCCD2Core12.Value)} ";
                }
                else
                {
                    if (cbCCD1Core1.IsChecked == true) commandValues = commandValues + $"--set-coper={(0 << 20) | ((int)nudCCD1Core1.Value & 0xFFFF)} ";
                    if (cbCCD1Core2.IsChecked == true) commandValues = commandValues + $"--set-coper={(1 << 20) | ((int)nudCCD1Core2.Value & 0xFFFF)} ";
                    if (cbCCD1Core3.IsChecked == true) commandValues = commandValues + $"--set-coper={(2 << 20) | ((int)nudCCD1Core3.Value & 0xFFFF)} ";
                    if (cbCCD1Core4.IsChecked == true) commandValues = commandValues + $"--set-coper={(3 << 20) | ((int)nudCCD1Core4.Value & 0xFFFF)} ";
                    if (cbCCD1Core5.IsChecked == true) commandValues = commandValues + $"--set-coper={(4 << 20) | ((int)nudCCD1Core5.Value & 0xFFFF)} ";
                    if (cbCCD1Core6.IsChecked == true) commandValues = commandValues + $"--set-coper={(5 << 20) | ((int)nudCCD1Core6.Value & 0xFFFF)} ";
                    if (cbCCD1Core7.IsChecked == true) commandValues = commandValues + $"--set-coper={(6 << 20) | ((int)nudCCD1Core7.Value & 0xFFFF)} ";
                    if (cbCCD1Core8.IsChecked == true) commandValues = commandValues + $"--set-coper={(7 << 20) | ((int)nudCCD1Core8.Value & 0xFFFF)} ";
                    if (cbCCD1Core9.IsChecked == true) commandValues = commandValues + $"--set-coper={(7 << 20) | ((int)nudCCD1Core9.Value & 0xFFFF)} ";
                    if (cbCCD1Core10.IsChecked == true) commandValues = commandValues + $"--set-coper={(7 << 20) | ((int)nudCCD1Core10.Value & 0xFFFF)} ";
                    if (cbCCD1Core11.IsChecked == true) commandValues = commandValues + $"--set-coper={(7 << 20) | ((int)nudCCD1Core11.Value & 0xFFFF)} ";
                    if (cbCCD1Core12.IsChecked == true) commandValues = commandValues + $"--set-coper={(7 << 20) | ((int)nudCCD1Core12.Value & 0xFFFF)} ";
                }

                if (tsAmdOC.IsChecked == true)
                {
                    double vid = 0;

                    vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                    commandValues = commandValues + $"--oc-clk={(int)nudAmdCpuClk.Value} --oc-clk={(int)nudAmdCpuClk.Value} ";

                    if (Family.FAM >= Family.RyzenFamily.Rembrandt)
                    {
                        vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                        commandValues = commandValues + $"--oc-volt={vid} --oc-volt={vid} ";
                    }
                    else
                    {
                        vid = Math.Round((double)nudAmdVID.Value / 1000, 2);
                        commandValues = commandValues + $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} ";
                    }

                    commandValues = commandValues + $"--enable-oc --enable-oc ";
                }

            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                if (cbCPUTemp.IsChecked == true) commandValues = commandValues + $"--tctl-limit={nudCPUTemp.Value * 1000} ";
                if (cbPPT.IsChecked == true) commandValues = commandValues + $"--ppt-limit={nudPPT.Value * 1000} ";
                if (cbTDC.IsChecked == true) commandValues = commandValues + $"--tdc-limit={nudTDC.Value * 1000} ";
                if (cbEDC.IsChecked == true) commandValues = commandValues + $"--edc-limit={nudEDC.Value * 1000} ";
                if (cbPBOScaler.IsChecked == true) commandValues = commandValues + $"--pbo-scalar={nudPBOScaler.Value * 100} ";

                if (cbAllCO.IsChecked == true)
                {
                    if (nudAllCO.Value >= 0) commandValues = commandValues + $"--set-coall={nudAllCO.Value} ";
                    if (nudAllCO.Value < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudAllCO.Value))} ";
                }

                if (cbGfxCO.IsChecked == true)
                {
                    if (nudGfxCO.Value >= 0) commandValues = commandValues + $"--set-cogfx={nudGfxCO.Value} ";
                    if (nudGfxCO.Value < 0) commandValues = commandValues + $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudGfxCO.Value))} ";
                }

                if (cbCCD1Core1.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 0, (int)nudCCD1Core1.Value)} ";
                if (cbCCD1Core2.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 1, (int)nudCCD1Core2.Value)} ";
                if (cbCCD1Core3.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 2, (int)nudCCD1Core3.Value)} ";
                if (cbCCD1Core4.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 3, (int)nudCCD1Core4.Value)} ";
                if (cbCCD1Core5.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 4, (int)nudCCD1Core5.Value)} ";
                if (cbCCD1Core6.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 5, (int)nudCCD1Core6.Value)} ";
                if (cbCCD1Core7.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 6, (int)nudCCD1Core7.Value)} ";
                if (cbCCD1Core8.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 7, (int)nudCCD1Core8.Value)} ";
                if (cbCCD1Core9.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 8, (int)nudCCD1Core9.Value)} ";
                if (cbCCD1Core10.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 9, (int)nudCCD1Core10.Value)} ";
                if (cbCCD1Core11.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 10, (int)nudCCD1Core11.Value)} ";
                if (cbCCD1Core12.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(0, 11, (int)nudCCD1Core12.Value)} ";

                if (cbCCD2Core1.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 0, (int)nudCCD2Core1.Value)} ";
                if (cbCCD2Core2.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 1, (int)nudCCD2Core2.Value)} ";
                if (cbCCD2Core3.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 2, (int)nudCCD2Core3.Value)} ";
                if (cbCCD2Core4.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 3, (int)nudCCD2Core4.Value)} ";
                if (cbCCD2Core5.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 4, (int)nudCCD2Core5.Value)} ";
                if (cbCCD2Core6.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 5, (int)nudCCD2Core6.Value)} ";
                if (cbCCD2Core7.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 6, (int)nudCCD2Core7.Value)} ";
                if (cbCCD2Core8.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 7, (int)nudCCD2Core8.Value)} ";
                if (cbCCD2Core9.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 8, (int)nudCCD2Core9.Value)} ";
                if (cbCCD2Core10.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 9, (int)nudCCD2Core10.Value)} ";
                if (cbCCD2Core11.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 10, (int)nudCCD2Core11.Value)} ";
                if (cbCCD2Core12.IsChecked == true) commandValues += $"--set-coper={BuildCoperArg(1, 11, (int)nudCCD2Core12.Value)} ";

                if (tsAmdOC.IsChecked == true)
                {
                    double vid = 0;

                    vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                    commandValues = commandValues + $"--oc-clk={(int)nudAmdCpuClk.Value} --oc-clk={(int)nudAmdCpuClk.Value} ";

                    if (Family.FAM >= Family.RyzenFamily.Rembrandt)
                    {
                        vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                        commandValues = commandValues + $"--oc-volt={vid} --oc-volt={vid} ";
                    }
                    else
                    {
                        vid = Math.Round((double)nudAmdVID.Value / 1000, 2);
                        commandValues = commandValues + $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} ";
                    }

                    commandValues = commandValues + $"--enable-oc --enable-oc ";
                }
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                if (tsIntelRatioCore.IsChecked == true)
                {
                    commandValues = commandValues + $"--intel-ratio=";
                    int core = 0;
                    foreach(int clock in clockRatio)
                    {
                        if (core < intelRatioControls.Length)
                        {
                            if (core == clockRatio.Length -1) commandValues = commandValues + $"{intelRatioControls[core].Value} ";
                            else commandValues = commandValues + $"{intelRatioControls[core].Value}-";
                        }
                        core++;
                    }
                }
                if (cbIntelPL1.IsChecked == true || cbIntelPL2.IsChecked == true)
                    commandValues = commandValues + $"--intel-pl={(int)nudIntelPL1.Value},{Math.Max((int)nudIntelPL1.Value + 2, (int)nudIntelPL2.Value)} ";
                if (tsIntelUV.IsChecked == true) commandValues = commandValues + $"--intel-volt-cpu={nudIntelCoreUV.Value} --intel-volt-gpu={nudIntelGfxUV.Value} --intel-volt-cache={nudIntelCacheUV.Value} --intel-volt-sa={nudIntelSAUV.Value} ";
                if (tsIntelBal.IsChecked == true) commandValues = commandValues + $"--intel-bal-cpu={nudIntelCpuBal.Value} --intel-bal-gpu={nudIntelGpuBal.Value} ";
                if (cbAPUiGPUClk.IsChecked == true) commandValues = commandValues + $"--intel-gpu={nudAPUiGPUClk.Value} ";

            }

            if (tsRadeonGraph.IsChecked == true)
            {
                if (cbAntiLag.IsChecked == true) commandValues = commandValues + $"--ADLX-Lag=0-true --ADLX-Lag=1-true ";
                else commandValues = commandValues + $"--ADLX-Lag=0-false --ADLX-Lag=1-false ";

                if (cbRSR.IsChecked == true) commandValues = commandValues + $"--ADLX-RSR=true-{(int)nudRSR.Value} ";
                else commandValues = commandValues + $"--ADLX-RSR=false-{(int)nudRSR.Value} ";

                if (cbBoost.IsChecked == true) commandValues = commandValues + $"--ADLX-Boost=0-true-{(int)nudBoost.Value} --ADLX-Boost=1-true-{(int)nudBoost.Value} ";
                else commandValues = commandValues + $"--ADLX-Boost=0-false-{(int)nudBoost.Value} --ADLX-Boost=1-false-{(int)nudBoost.Value} ";

                if (cbImageSharp.IsChecked == true) commandValues = commandValues + $"--ADLX-ImageSharp=0-true-{(int)nudImageSharp.Value} --ADLX-ImageSharp=1-true-{(int)nudImageSharp.Value} ";
                else commandValues = commandValues + $"--ADLX-ImageSharp=0-false-{(int)nudImageSharp.Value} --ADLX-ImageSharp=1-false-{(int)nudImageSharp.Value} ";

                if (cbSync.IsChecked == true) commandValues = commandValues + $"--ADLX-Sync=0-true --ADLX-Sync=1-true ";
                else commandValues = commandValues + $"--ADLX-Sync=0-false --ADLX-Sync=1-false ";
            }

            if (tsNV.IsChecked == true) commandValues = commandValues + $"--NVIDIA-Clocks={nudNVMaxCore.Value}-{nudNVCore.Value}-{nudNVMem.Value}-{nudNVPower.Value} ";

            if (sdCcdAffinity.Visibility == Visibility.Visible) commandValues = commandValues + $"--CCD-Affinity={cbxCcdAffinity.SelectedIndex} ";

            return commandValues;
        }

        private static uint BuildCoperArg(int ccd, int core, int offset)
        {
            int magnitude = Math.Min(Math.Abs(offset), 0xFFFFF);

            uint encoded20 =
                offset < 0
                    ? (uint)((0x100000 - magnitude) & 0xFFFFF)
                    : (uint)(magnitude & 0xFFFFF);

            uint prefix = (uint)((((ccd << 4) | (0 % 1 & 15)) << 4 | (core % 8 & 15)) << 20);
            return prefix | encoded20;
        }


        public bool IsScrollBarVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null) throw new ArgumentNullException(nameof(scrollViewer));

            return scrollViewer.ExtentHeight > scrollViewer.ViewportHeight;
        }

        private void mainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(15, 0, -12, 0);
            else mainCon.Margin = new Thickness(15, 0, 0, 0);
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
            if (checkBox == cbBoost)
            {
                cbRSR.IsChecked = false;
                cbAntiLag.IsChecked = false;
            }

            if (checkBox == cbAntiLag)
            {
                cbBoost.IsChecked = false;
            }

            if (checkBox == cbRSR)
            {
                cbBoost.IsChecked = false;
                cbImageSharp.IsChecked = false;
            }

            if (checkBox == cbImageSharp) cbRSR.IsChecked = false;

        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            tsAmdOC.IsChecked = false;
            RyzenAdj_To_UXTU.Translate("--disable-oc ");
            RyzenAdj_To_UXTU.Translate(getCommandValues(), appliedName: GetSelectedPresetName());
            Settings.Default.CommandString = getCommandValues();
            Settings.Default.Save();
            btnUndo.Visibility = Visibility.Collapsed;
            RyzenAdj_To_UXTU.Translate("--disable-oc ");
        }

        private void tsAmdOC_Checked(object sender, RoutedEventArgs e)
        {
            btnUndo.Visibility = Visibility.Visible;
        }

        private void tsAmdOC_Unchecked(object sender, RoutedEventArgs e)
        {
            btnUndo.Visibility = Visibility.Collapsed;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
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
                cbxRefreshRate.Items.Clear();
                cbxRefreshRate.Items.Add("System Controlled");
                foreach (int rate in Display.uniqueRefreshRates)
                    cbxRefreshRate.Items.Add($"{rate} Hz");
                sdRefreshRate.Visibility = Visibility.Visible;
            }

            GpuInventorySnapshot inventory = await gpuInventory.GetSnapshotAsync();
            radeonGpuCount = inventory.RadeonCount;
            nvidiaGpuCount = inventory.NvidiaCount;
            sdADLX.Visibility = radeonGpuCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            sdNVIDIA.Visibility = nvidiaGpuCount > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (nvidiaGpuCount > 0)
            {
                NvTuning.GpuInfo? info = await Task.Run<NvTuning.GpuInfo?>(() =>
                    NvTuning.TryGetGpuInfo(out NvTuning.GpuInfo value) ? value : null);
                if (info.HasValue)
                {
                    sdNVPower.Maximum = info.Value.MaxPowerWatts;
                    nudNVPower.Maximum = info.Value.MaxPowerWatts;
                    sdNVPower.Minimum = info.Value.MinPowerWatts;
                    nudNVPower.Minimum = info.Value.MinPowerWatts;
                    sdNVPower.Value = info.Value.CurrentPowerWatts;
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

                if (mux > 0) tsASUSUlti.IsChecked = false;
                else if (mux > -1) tsASUSUlti.IsChecked = true;
                else sdAsusUlti.Visibility = Visibility.Collapsed;

                if (eco is >= 0 and < 1) tsASUSEco.IsChecked = false;
                else if (eco > 0) tsASUSEco.IsChecked = true;
                else sdAsusEco.Visibility = Visibility.Collapsed;

                if (performanceMode == (int)ASUSWmi.AsusMode.Silent) cbxAsusPower.SelectedIndex = 1;
                else if (performanceMode == (int)ASUSWmi.AsusMode.Balanced) cbxAsusPower.SelectedIndex = 2;
                else if (performanceMode == (int)ASUSWmi.AsusMode.Turbo) cbxAsusPower.SelectedIndex = 3;
            }

            ReloadPresetValues(Settings.Default.cstmPreset);
        }
    }
}
