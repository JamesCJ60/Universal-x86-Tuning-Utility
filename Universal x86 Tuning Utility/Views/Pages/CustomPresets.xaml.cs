using RyzenSmu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.ASUS;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for CustomPresets.xaml
    /// </summary>
    public partial class CustomPresets : INavigableView<ViewModels.CustomPresetsViewModel>
    {
        public ViewModels.CustomPresetsViewModel ViewModel
        {
            get;
        }

        private PresetManager apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
        private PresetManager amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
        private PresetManager intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
        public CustomPresets()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            nudAPUSkinTemp.Value = 45;
            nudAPUTemp.Value = 95;
            nudSTAPMPow.Value = 28;
            nudFastPow.Value = 28;
            nudSlowPow.Value = 28;
            nudSlowTime.Value = 128;
            nudFastTime.Value = 64;
            nudCpuVrmTdc.Value = 64;
            nudCpuVrmEdc.Value = 64;
            nudGfxVrmTdc.Value = 64;
            nudGfxVrmEdc.Value = 64;
            nudSocVrmTdc.Value = 64;
            nudSocVrmEdc.Value = 64;
            nudAPUiGPUClk.Value = 1500;
            nudCPUTemp.Value = 85;
            nudPPT.Value = 140;
            nudEDC.Value = 160;
            nudTDC.Value = 160;
            nudIntelPL1.Value = 35;
            nudIntelPL2.Value = 65;
            nudAmdVID.Value = 1200;
            nudAmdCpuClk.Value = 3200;
            nudNVMaxCore.Value = 4000;

            apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
            amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
            intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

            if (GetRadeonGPUCount() < 1) sdADLX.Visibility = Visibility.Collapsed;
            if (GetNVIDIAGPUCount() < 1) sdNVIDIA.Visibility = Visibility.Collapsed;

            try
            {
                if (Display.uniqueRefreshRates.Count > 1)
                {
                    cbxRefreshRate.Items.Add($"System Controlled");
                    foreach (var rate in Display.uniqueRefreshRates)
                    {
                        cbxRefreshRate.Items.Add($"{rate} Hz");
                    }
                }
                else sdRefreshRate.Visibility = Visibility.Collapsed;
            } catch 
            {
                sdRefreshRate.Visibility = Visibility.Collapsed;
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                sdAmdCPU.Visibility = Visibility.Collapsed;
                sdAmdCpuThermal.Visibility = Visibility.Collapsed;
                sdIntelCPU.Visibility = Visibility.Collapsed;
                if (Family.FAM != Family.RyzenFamily.PhoenixPoint || Family.FAM != Family.RyzenFamily.PhoenixPoint2 && Family.FAM != Family.RyzenFamily.Mendocino && Family.FAM != Family.RyzenFamily.Rembrandt && Family.FAM != Family.RyzenFamily.Lucienne && Family.FAM != Family.RyzenFamily.Renoir) sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;
                if (Family.CPUName.Contains("U") && Family.FAM > Family.RyzenFamily.Renoir) sdAmdPBO.Visibility = Visibility.Collapsed;
                if(SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery) sdAmdCpuTune.Visibility = Visibility.Collapsed;

                if(Family.FAM < Family.RyzenFamily.Renoir) sdAmdSoftClk.Visibility = Visibility.Visible;

                if (Family.FAM < Family.RyzenFamily.Renoir || Family.FAM == Family.RyzenFamily.Mendocino) sdAmdCO.Visibility = Visibility.Collapsed;
                else sdAmdCO.Visibility = Visibility.Visible;

                sdAmdCCD1CO.Visibility = sdAmdCO.Visibility;

                if (Family.FAM < Family.RyzenFamily.DragonRange)
                {
                    sdAmdPowerProfile.Visibility = Visibility.Collapsed;
                    sdAmdCO.Visibility = Visibility.Collapsed;
                    if (Family.CPUName.Contains("Ryzen 9")) sdAmdCCD2CO.Visibility = sdAmdCO.Visibility;
                }

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxPowerPreset.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                sdAmdApuCPU.Visibility = Visibility.Collapsed;
                sdAmdApuThermal.Visibility = Visibility.Collapsed;
                sdAmdApuVRM.Visibility = Visibility.Collapsed;
                sdIntelCPU.Visibility = Visibility.Collapsed;
                sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;
                sdAmdPowerProfile.Visibility = Visibility.Collapsed;

                if (Family.FAM < Family.RyzenFamily.Vermeer) sdAmdCO.Visibility = Visibility.Collapsed;
                sdAmdCCD1CO.Visibility = sdAmdCO.Visibility;
                if (Family.CPUName.Contains("Ryzen 9")) sdAmdCCD2CO.Visibility = sdAmdCO.Visibility;

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxPowerPreset.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                sdAmdCPU.Visibility = Visibility.Collapsed;
                sdAmdCpuThermal.Visibility = Visibility.Collapsed;
                sdAmdApuCPU.Visibility = Visibility.Collapsed;
                sdAmdApuThermal.Visibility = Visibility.Collapsed;
                sdAmdApuVRM.Visibility = Visibility.Collapsed;
                sdAmdPowerProfile.Visibility = Visibility.Collapsed;
                sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;
                sdAmdCpuClk.Visibility = Visibility.Collapsed;
                sdAmdPBO.Visibility = Visibility.Collapsed;
                sdAmdCpuTune.Visibility = Visibility.Collapsed;
                sdAmdCO.Visibility = Visibility.Collapsed;

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxPowerPreset.Items.Add(presetName);
                }
            }

            if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(15, 0, 0, 0);
            else mainCon.Margin = new Thickness(15, 0, -12, 0);

            if (Settings.Default.isASUS)
            {
                uint id = 0;
                if (App.product.Contains("ROG") || App.product.Contains("TUF")) id = ASUSWmi.GPUMux;
                else id = ASUSWmi.GPUMuxVivo;
                int mux = App.wmi.DeviceGet(id);

                if (mux > 0) tsASUSUlti.IsChecked = false;
                else if (mux > -1) tsASUSUlti.IsChecked = true;
                else sdAsusUlti.Visibility = Visibility.Collapsed;

                id = ASUSWmi.GPUEco;
                int eco = App.wmi.DeviceGet(id);

                if (eco > -1 && eco < 1) tsASUSEco.IsChecked = false;
                else if (eco > 0) tsASUSEco.IsChecked = true;
                else sdAsusEco.Visibility = Visibility.Collapsed;

                if (App.product.Contains("ROG") || App.product.Contains("TUF")) id = ASUSWmi.PerformanceMode;
                else id = ASUSWmi.VivoBookMode;
                int perfMode = App.wmi.DeviceGet(id);
                if (perfMode == (int)ASUSWmi.AsusMode.Silent) cbxAsusPower.SelectedIndex = 1;
                else if (perfMode == (int)ASUSWmi.AsusMode.Balanced) cbxAsusPower.SelectedIndex = 2;
                else if (perfMode == (int)ASUSWmi.AsusMode.Turbo) cbxAsusPower.SelectedIndex = 3;
            }
            else
            {
                sdAsusPower.Visibility = Visibility.Collapsed;
                sdAsusUlti.Visibility = Visibility.Collapsed;
                sdAsusEco.Visibility = Visibility.Collapsed;
            }

            if (Family.FAM == Family.RyzenFamily.Renoir || Family.FAM == Family.RyzenFamily.Lucienne || Family.FAM == Family.RyzenFamily.Mendocino || Family.FAM == Family.RyzenFamily.Rembrandt || Family.FAM == Family.RyzenFamily.PhoenixPoint || Family.FAM == Family.RyzenFamily.PhoenixPoint2 || Family.FAM == Family.RyzenFamily.HawkPoint) sdAmdApuiGPUClk.Visibility = Visibility.Visible;

            Garbage.Garbage_Collect();
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
                await Task.Run(() => RyzenAdj_To_UXTU.Translate(commandValues));
                ToastNotification.ShowToastNotification("Preset Applied", $"Your custom preset settings have been applied!");
            }

            Settings.Default.CommandString = commandValues;
            Settings.Default.Save();

            if (GetRadeonGPUCount() < 1) sdADLX.Visibility = Visibility.Collapsed;
            else sdADLX.Visibility = Visibility.Visible;
            if (GetNVIDIAGPUCount() < 1) sdNVIDIA.Visibility = Visibility.Collapsed;
            else sdNVIDIA.Visibility = Visibility.Visible;
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

                        };
                        apuPresetManager.SavePreset(tbxPresetName.Text, preset);

                        apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                        cbxPowerPreset.Items.Clear();

                        // Populate a combo box with the preset names
                        foreach (string presetName in presetNames)
                        {
                            cbxPowerPreset.Items.Add(presetName);
                        }

                        ToastNotification.ShowToastNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");

                        if (GetRadeonGPUCount() < 1) sdADLX.Visibility = Visibility.Collapsed;
                        else sdADLX.Visibility = Visibility.Visible;
                        if (GetNVIDIAGPUCount() < 1) sdNVIDIA.Visibility = Visibility.Collapsed;
                        else sdNVIDIA.Visibility = Visibility.Visible;
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

                            coGfx= (int)nudGfxCO.Value,
                            isCoGfx = (bool)cbGfxCO.IsChecked,

                            isNVIDIA = (bool)tsNV.IsChecked,
                            nvMaxCoreClk = (int)nudNVMaxCore.Value,
                            nvCoreClk = (int)nudNVCore.Value,
                            nvMemClk = (int)nudNVMem.Value,

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
                        };
                        amdDtCpuPresetManager.SavePreset(tbxPresetName.Text, preset);

                        amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                        cbxPowerPreset.Items.Clear();

                        // Populate a combo box with the preset names
                        foreach (string presetName in presetNames)
                        {
                            cbxPowerPreset.Items.Add(presetName);
                        }

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

                            isNVIDIA = (bool)tsNV.IsChecked,
                            nvMaxCoreClk = (int)nudNVMaxCore.Value,
                            nvCoreClk = (int)nudNVCore.Value,
                            nvMemClk = (int)nudNVMem.Value,

                            asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                            asusiGPU = (bool)tsASUSEco.IsChecked,
                            asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                            displayHz = (int)cbxRefreshRate.SelectedIndex,

                            isMag = (bool)tsUXTUSR.IsChecked,
                            isVsync = (bool)cbVSync.IsChecked,
                            isRecap = (bool)cbAutoCap.IsChecked,
                            Sharpness = (int)nudSharp.Value,
                            ResScaleIndex = (int)cbxResScale.SelectedIndex,
                        };
                        intelPresetManager.SavePreset(tbxPresetName.Text, preset);

                        intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                        cbxPowerPreset.Items.Clear();

                        // Populate a combo box with the preset names
                        foreach (string presetName in presetNames)
                        {
                            cbxPowerPreset.Items.Add(presetName);
                        }

                        ToastNotification.ShowToastNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");
                    }
                }
            }
        }

        private void cbxPowerPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateValues((sender as System.Windows.Controls.ComboBox).SelectedItem as string);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    if (cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null)
                    {
                        string deletePresetName = cbxPowerPreset.Text;
                        apuPresetManager.DeletePreset(deletePresetName);

                        apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                        cbxPowerPreset.Items.Clear();

                        // Populate a combo box with the preset names
                        foreach (string presetName in presetNames)
                        {
                            cbxPowerPreset.Items.Add(presetName);
                        }

                        ToastNotification.ShowToastNotification("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    if (cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null)
                    {
                        string deletePresetName = cbxPowerPreset.Text;
                        amdDtCpuPresetManager.DeletePreset(deletePresetName);

                        amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                        cbxPowerPreset.Items.Clear();

                        // Populate a combo box with the preset names
                        foreach (string presetName in presetNames)
                        {
                            cbxPowerPreset.Items.Add(presetName);
                        }

                        ToastNotification.ShowToastNotification("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    if (cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null)
                    {
                        string deletePresetName = cbxPowerPreset.Text;
                        intelPresetManager.DeletePreset(deletePresetName);

                        intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

                        // Get the names of all the stored presets
                        IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                        cbxPowerPreset.Items.Clear();

                        // Populate a combo box with the preset names
                        foreach (string presetName in presetNames)
                        {
                            cbxPowerPreset.Items.Add(presetName);
                        }

                        ToastNotification.ShowToastNotification("Preset Deleted", $"Your preset {deletePresetName} has been deleted successfully!");
                    }
                }
            } catch (Exception ex) {  }
        }


        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
            amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
            intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
            if(cbxPowerPreset.Text != null && cbxPowerPreset.Text != "") updateValues(cbxPowerPreset.SelectedItem.ToString());
        }

        public void updateValues(string preset)
        {
            try
            {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = apuPresetManager.GetPreset(preset);

                    if (myPreset != null)
                    {
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

                        nudCCD2Core1.Value = myPreset.ccd1Core1;
                        nudCCD2Core2.Value = myPreset.ccd1Core2;
                        nudCCD2Core3.Value = myPreset.ccd1Core3;
                        nudCCD2Core4.Value = myPreset.ccd1Core4;
                        nudCCD2Core5.Value = myPreset.ccd1Core5;
                        nudCCD2Core6.Value = myPreset.ccd1Core6;
                        nudCCD2Core7.Value = myPreset.ccd1Core7;
                        nudCCD2Core8.Value = myPreset.ccd1Core8;

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

                        if(myPreset.displayHz <= cbxRefreshRate.Items.Count) cbxRefreshRate.SelectedIndex = myPreset.displayHz;

                        tsUXTUSR.IsChecked = myPreset.isMag;
                        cbVSync.IsChecked = myPreset.isVsync;
                        cbAutoCap.IsChecked = myPreset.isRecap;
                        nudSharp.Value = myPreset.Sharpness;
                        cbxResScale.SelectedIndex = myPreset.ResScaleIndex;
                    }
                }

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = amdDtCpuPresetManager.GetPreset(preset);

                    if (myPreset != null)
                    {
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

                        tsAmdOC.IsChecked = myPreset.IsAmdOC;
                        nudAmdCpuClk.Value = myPreset.amdClock;
                        nudAmdVID.Value = myPreset.amdVID;

                        tsASUSUlti.IsChecked = myPreset.asusGPUUlti;
                        tsASUSEco.IsChecked = myPreset.asusiGPU;
                        cbxAsusPower.SelectedIndex = myPreset.asusPowerProfile;

                        if (myPreset.displayHz <= cbxRefreshRate.Items.Count) cbxRefreshRate.SelectedIndex = myPreset.displayHz;

                        tsUXTUSR.IsChecked = myPreset.isMag;
                        cbVSync.IsChecked = myPreset.isVsync;
                        cbAutoCap.IsChecked = myPreset.isRecap;
                        nudSharp.Value = myPreset.Sharpness;
                        cbxResScale.SelectedIndex = myPreset.ResScaleIndex;
                    }
                }
                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    // Get the "myPreset" preset
                    Preset myPreset = intelPresetManager.GetPreset(preset);

                    if (myPreset != null)
                    {
                        // Read the values from the preset
                        nudIntelPL1.Value = myPreset.IntelPL1;
                        nudIntelPL2.Value = myPreset.IntelPL2;

                        cbIntelPL1.IsChecked = myPreset.isIntelPL1;
                        cbIntelPL2.IsChecked = myPreset.isIntelPL2;

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

                        tsASUSUlti.IsChecked = myPreset.asusGPUUlti;
                        tsASUSEco.IsChecked = myPreset.asusiGPU;
                        cbxAsusPower.SelectedIndex = myPreset.asusPowerProfile;

                        if (myPreset.displayHz <= cbxRefreshRate.Items.Count) cbxRefreshRate.SelectedIndex = myPreset.displayHz;

                        tsUXTUSR.IsChecked = myPreset.isMag;
                        cbVSync.IsChecked = myPreset.isVsync;
                        cbAutoCap.IsChecked = myPreset.isRecap;
                        nudSharp.Value = myPreset.Sharpness;
                        cbxResScale.SelectedIndex = myPreset.ResScaleIndex;
                    }
                }
                Garbage.Garbage_Collect();
            }
            catch (Exception ex) { }
        }

        public string getCommandValues()
        {
            string commandValues = "";

            commandValues = commandValues + $"--UXTUSR={tsUXTUSR.IsChecked}-{cbVSync.IsChecked}-{nudSharp.Value / 100}-{cbxResScale.SelectedIndex}-{cbAutoCap.IsChecked} ";

            if (Settings.Default.isASUS)
            {
               if(cbxAsusPower.SelectedIndex > 0) commandValues = commandValues + $"--ASUS-Power={cbxAsusPower.SelectedIndex} ";
               if(sdAsusEco.Visibility == Visibility.Visible) commandValues = commandValues + $"--ASUS-Eco={tsASUSEco.IsChecked} ";
               if(sdAsusUlti.Visibility == Visibility.Visible) commandValues = commandValues + $"--ASUS-MUX={tsASUSUlti.IsChecked} ";
            }

            if (sdRefreshRate.Visibility == Visibility.Visible && cbxRefreshRate.SelectedIndex > 0) 
            {
                commandValues = commandValues + $"--Refresh-Rate={Display.uniqueRefreshRates[cbxRefreshRate.SelectedIndex - 1]} ";
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                if (cbAPUTemp.IsChecked == true) commandValues = commandValues + $"--tctl-temp={nudAPUTemp.Value} --cHTC-temp={nudAPUTemp.Value} ";
                if (cbAPUSkinTemp.IsChecked == true) commandValues = commandValues + $"--apu-skin-temp={nudAPUSkinTemp.Value} ";
                if (cbSTAPMPow.IsChecked == true) commandValues = commandValues + $"--stapm-limit={nudSTAPMPow.Value * 1000}  ";
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
                    if (nudAllCO.Value >= 0) commandValues = commandValues + $"--set-coall={nudAllCO.Value} ";
                    if (nudAllCO.Value < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudAllCO.Value))} ";
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

                if (Family.FAM == Family.RyzenFamily.DragonRange)
                {
                    if (cbCCD1Core1.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 0 % 8 & 15) << 20 | ((int)nudCCD1Core1.Value & 0xFFFF)} ";
                    if (cbCCD1Core2.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 1 % 8 & 15) << 20 | ((int)nudCCD1Core2.Value & 0xFFFF)} ";
                    if (cbCCD1Core3.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 2 % 8 & 15) << 20 | ((int)nudCCD1Core3.Value & 0xFFFF)} ";
                    if (cbCCD1Core4.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 3 % 8 & 15) << 20 | ((int)nudCCD1Core4.Value & 0xFFFF)} ";
                    if (cbCCD1Core5.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 4 % 8 & 15) << 20 | ((int)nudCCD1Core5.Value & 0xFFFF)} ";
                    if (cbCCD1Core6.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 5 % 8 & 15) << 20 | ((int)nudCCD1Core6.Value & 0xFFFF)} ";
                    if (cbCCD1Core7.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 6 % 8 & 15) << 20 | ((int)nudCCD1Core7.Value & 0xFFFF)} ";
                    if (cbCCD1Core8.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 7 % 8 & 15) << 20 | ((int)nudCCD1Core8.Value & 0xFFFF)} ";

                    if (cbCCD2Core1.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 0 % 8 & 15) << 20 | ((int)nudCCD2Core1.Value & 0xFFFF)} ";
                    if (cbCCD2Core2.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 1 % 8 & 15) << 20 | ((int)nudCCD2Core2.Value & 0xFFFF)} ";
                    if (cbCCD2Core3.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 2 % 8 & 15) << 20 | ((int)nudCCD2Core3.Value & 0xFFFF)} ";
                    if (cbCCD2Core4.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 3 % 8 & 15) << 20 | ((int)nudCCD2Core4.Value & 0xFFFF)} ";
                    if (cbCCD2Core5.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 4 % 8 & 15) << 20 | ((int)nudCCD2Core5.Value & 0xFFFF)} ";
                    if (cbCCD2Core6.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 5 % 8 & 15) << 20 | ((int)nudCCD2Core6.Value & 0xFFFF)} ";
                    if (cbCCD2Core7.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 6 % 8 & 15) << 20 | ((int)nudCCD2Core7.Value & 0xFFFF)} ";
                    if (cbCCD2Core8.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 7 % 8 & 15) << 20 | ((int)nudCCD2Core8.Value & 0xFFFF)} ";
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

                Garbage.Garbage_Collect();
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

                if (cbCCD1Core1.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 0 % 8 & 15) << 20 | ((int)nudCCD1Core1.Value & 0xFFFF)} ";
                if (cbCCD1Core2.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 1 % 8 & 15) << 20 | ((int)nudCCD1Core2.Value & 0xFFFF)} ";
                if (cbCCD1Core3.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 2 % 8 & 15) << 20 | ((int)nudCCD1Core3.Value & 0xFFFF)} ";
                if (cbCCD1Core4.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 3 % 8 & 15) << 20 | ((int)nudCCD1Core4.Value & 0xFFFF)} ";
                if (cbCCD1Core5.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 4 % 8 & 15) << 20 | ((int)nudCCD1Core5.Value & 0xFFFF)} ";
                if (cbCCD1Core6.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 5 % 8 & 15) << 20 | ((int)nudCCD1Core6.Value & 0xFFFF)} ";
                if (cbCCD1Core7.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 6 % 8 & 15) << 20 | ((int)nudCCD1Core7.Value & 0xFFFF)} ";
                if (cbCCD1Core8.IsChecked == true) commandValues = commandValues + $"--set-coper={((0 << 4 | 0 % 1 & 15) << 4 | 7 % 8 & 15) << 20 | ((int)nudCCD1Core8.Value & 0xFFFF)} ";

                if (cbCCD2Core1.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 0 % 8 & 15) << 20 | ((int)nudCCD2Core1.Value & 0xFFFF)} ";
                if (cbCCD2Core2.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 1 % 8 & 15) << 20 | ((int)nudCCD2Core2.Value & 0xFFFF)} ";
                if (cbCCD2Core3.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 2 % 8 & 15) << 20 | ((int)nudCCD2Core3.Value & 0xFFFF)} ";
                if (cbCCD2Core4.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 3 % 8 & 15) << 20 | ((int)nudCCD2Core4.Value & 0xFFFF)} ";
                if (cbCCD2Core5.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 4 % 8 & 15) << 20 | ((int)nudCCD2Core5.Value & 0xFFFF)} ";
                if (cbCCD2Core6.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 5 % 8 & 15) << 20 | ((int)nudCCD2Core6.Value & 0xFFFF)} ";
                if (cbCCD2Core7.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 6 % 8 & 15) << 20 | ((int)nudCCD2Core7.Value & 0xFFFF)} ";
                if (cbCCD2Core8.IsChecked == true) commandValues = commandValues + $"--set-coper={((1 << 4 | 0 % 1 & 15) << 4 | 7 % 8 & 15) << 20 | ((int)nudCCD2Core8.Value & 0xFFFF)} ";

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
                if (cbIntelPL1.IsChecked == true) commandValues = commandValues + $"--intel-pl={nudIntelPL1.Value} ";
                if (cbIntelPL2.IsChecked == true) commandValues = commandValues + $"--power-limit-2={nudIntelPL2.Value} ";
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

            if(tsNV.IsChecked == true)
            {
                commandValues = commandValues + $"--NVIDIA-Clocks={nudNVMaxCore.Value}-{nudNVCore.Value}-{nudNVMem.Value} ";
            }

            return commandValues;
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

            if(checkBox == cbImageSharp) cbRSR.IsChecked = false;

            Garbage.Garbage_Collect();
        }

        public static int GetRadeonGPUCount()
        {
            int count = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] as string;
                    if (name != null && name.Contains("Radeon"))
                    {
                        count++;
                    }
                }

                Garbage.Garbage_Collect();
            }
            return count;
        }

        public static int GetNVIDIAGPUCount()
        {
            int count = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] as string;
                    if (name != null && name.Contains("NVIDIA"))
                    {
                        count++;
                    }
                }

                Garbage.Garbage_Collect();
            }
            return count;
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            tsAmdOC.IsChecked = false;
            RyzenAdj_To_UXTU.Translate("--disable-oc ");
            RyzenAdj_To_UXTU.Translate(getCommandValues());
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Garbage.Garbage_Collect();
        }
    }
}
