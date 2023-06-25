using RyzenSmu;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.AMD_Backend;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;

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

            apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
            amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
            intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                sdAmdCPU.Visibility = Visibility.Collapsed;
                sdAmdCpuThermal.Visibility = Visibility.Collapsed;
                sdIntelCPU.Visibility = Visibility.Collapsed;
                if (Family.FAM != Family.RyzenFamily.PhoenixPoint && Family.FAM != Family.RyzenFamily.Mendocino && Family.FAM != Family.RyzenFamily.Rembrandt && Family.FAM != Family.RyzenFamily.Lucienne && Family.FAM != Family.RyzenFamily.Renoir) sdAmdApuiGPUClk.Visibility = Visibility.Collapsed;
                if (Family.FAM != Family.RyzenFamily.PhoenixPoint && Family.FAM != Family.RyzenFamily.Mendocino && Family.FAM != Family.RyzenFamily.Rembrandt && Family.FAM != Family.RyzenFamily.Lucienne && Family.FAM != Family.RyzenFamily.Renoir) sdAmdCO.Visibility = Visibility.Collapsed;
                if (Family.CPUName.Contains("U") && Family.FAM > Family.RyzenFamily.Renoir) sdAmdPBO.Visibility = Visibility.Collapsed;
                if(!Family.CPUName.Replace("with Radeon Graphics", null).Contains("G") && Family.CPUName != "AMD Custom APU 0405") if(Family.FAM != Family.RyzenFamily.Renoir && !Family.CPUName.Contains("Ryzen 9") && cbAllCO.Visibility == Visibility) sdAmdCO.Visibility = Visibility.Collapsed;

                if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery && Family.FAM >= Family.RyzenFamily.Renoir) sdAmdCO.Visibility = Visibility.Visible;
                

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
                sdAmdPowerProfile.Visibility = Visibility.Collapsed;

                if (Family.FAM < Family.RyzenFamily.Vermeer) sdAmdCO.Visibility = Visibility.Collapsed;

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
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            string commandValues = "";

            commandValues = getCommandValues();

            if (commandValues != "" && commandValues != null)
            {
                RyzenAdj_To_UXTU.Translate(commandValues);
                ToastNotification.ShowToastNotification("Preset Applied", $"Your custom preset settings have been applied!");
            }

            Settings.Default.CommandString = commandValues;
            Settings.Default.Save();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
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

                        boostProfile = (int)cbxBoost.SelectedIndex,

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
                        isCoAllCore = (bool)cbAllCO.IsChecked
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

                        commandValue = getCommandValues(),


                        isDtCpuTemp = (bool)cbCPUTemp.IsChecked,
                        isDtCpuPPT = (bool)cbPPT.IsChecked,
                        isDtCpuTDC = (bool)cbTDC.IsChecked,
                        isDtCpuEDC = (bool)cbEDC.IsChecked,
                        isPboScalar = (bool)cbPBOScaler.IsChecked,
                        isCoAllCore = (bool)cbAllCO.IsChecked
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

                        commandValue = getCommandValues(),

                        isIntelPL1 = (bool)cbIntelPL1.IsChecked,
                        isIntelPL2 = (bool)cbIntelPL2.IsChecked,
                    };
                    amdDtCpuPresetManager.SavePreset(tbxPresetName.Text, preset);

                    amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

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

                        cbPBOScaler.IsChecked = myPreset.isPboScalar;
                        cbAllCO.IsChecked = myPreset.isCoAllCore;

                        cbxBoost.SelectedIndex = myPreset.boostProfile;
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

                        cbPBOScaler.IsChecked = myPreset.isPboScalar;
                        cbAllCO.IsChecked = myPreset.isCoAllCore;
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
                    }
                }
            }
            catch (Exception ex) { }
        }

        public string getCommandValues()
        {
            string commandValues = "";
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

                if (cbxBoost.SelectedIndex > 0)
                {
                    if (cbxBoost.SelectedIndex == 1) commandValues = commandValues + $"--power-saving ";
                    if (cbxBoost.SelectedIndex == 2) commandValues = commandValues + $"--max-performance ";
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                if (cbCPUTemp.IsChecked == true) commandValues = commandValues + $"--tctl-limit={nudCPUTemp.Value} ";
                if (cbPPT.IsChecked == true) commandValues = commandValues + $"--ppt-limit={nudPPT.Value} ";
                if (cbTDC.IsChecked == true) commandValues = commandValues + $"--tdc-limit={nudTDC.Value} ";
                if (cbEDC.IsChecked == true) commandValues = commandValues + $"--edc-limit={nudEDC.Value} ";
                if (cbPBOScaler.IsChecked == true) commandValues = commandValues + $"--pbo-scalar={nudPBOScaler.Value * 100} ";
                if (cbAllCO.IsChecked == true)
                {
                    if (nudAllCO.Value >= 0) commandValues = commandValues + $"--set-coall={nudAllCO.Value} ";
                    if (nudAllCO.Value < 0) commandValues = commandValues + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudAllCO.Value))} ";
                }
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                if (cbIntelPL2.IsChecked == true) commandValues = commandValues + $"--power-limit-1={nudIntelPL1.Value} ";
                if (cbIntelPL2.IsChecked == true) commandValues = commandValues + $"--power-limit-2={nudIntelPL2.Value} ";
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
    }
}
