using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for Automations.xaml
    /// </summary>
    public partial class Automations : Page
    {
        private PresetManager apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
        private PresetManager amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json"); 
        private PresetManager intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
        bool setup = false;
        public Automations()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;
            PremadePresets.SetPremadePresets();
            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                // Get the names of all the stored presets
                IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                cbxCharge.Items.Add("PM - Eco Preset");
                cbxDischarge.Items.Add("PM - Eco Preset");
                cbxResume.Items.Add("PM - Eco Preset");

                cbxCharge.Items.Add("PM - Balanced Preset");
                cbxDischarge.Items.Add("PM - Balanced Preset");
                cbxResume.Items.Add("PM - Balanced Preset");

                cbxCharge.Items.Add("PM - Performance Preset");
                cbxDischarge.Items.Add("PM - Performance Preset");
                cbxResume.Items.Add("PM - Performance Preset");

                cbxCharge.Items.Add("PM - Extreme Preset");
                cbxDischarge.Items.Add("PM - Extreme Preset");
                cbxResume.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxCharge.Items.Add(presetName);
                    cbxDischarge.Items.Add(presetName);
                    cbxResume.Items.Add(presetName);
                }

                getAcPreset(Settings.Default.acPreset);
                getDcPreset(Settings.Default.dcPreset);
                getResumePreset(Settings.Default.resumePreset);
            }
            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                // Get the names of all the stored presets
                IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                cbxCharge.Items.Add("PM - Eco Preset");
                cbxDischarge.Items.Add("PM - Eco Preset");
                cbxResume.Items.Add("PM - Eco Preset");

                cbxCharge.Items.Add("PM - Balanced Preset");
                cbxDischarge.Items.Add("PM - Balanced Preset");
                cbxResume.Items.Add("PM - Balanced Preset");

                cbxCharge.Items.Add("PM - Performance Preset");
                cbxDischarge.Items.Add("PM - Performance Preset");
                cbxResume.Items.Add("PM - Performance Preset");

                cbxCharge.Items.Add("PM - Extreme Preset");
                cbxDischarge.Items.Add("PM - Extreme Preset");
                cbxResume.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxCharge.Items.Add(presetName);
                    cbxDischarge.Items.Add(presetName);
                    cbxResume.Items.Add(presetName);
                }

                getAcPreset(Settings.Default.acPreset);
                getDcPreset(Settings.Default.dcPreset);
                getResumePreset(Settings.Default.resumePreset);
            }
            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                // Get the names of all the stored presets
                IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxCharge.Items.Add(presetName);
                    cbxDischarge.Items.Add(presetName);
                    cbxResume.Items.Add(presetName);
                }

                getAcPreset(Settings.Default.acPreset);
                getDcPreset(Settings.Default.dcPreset);
                getResumePreset(Settings.Default.resumePreset);
            }

            Garbage.Garbage_Collect();
        }

        private void getAcPreset(string searchName)
        {
            int selectedIndex = 0; // index to select if the search fails

            foreach (var item in cbxCharge.Items)
            {
                if (item.ToString() == searchName)
                {
                    cbxCharge.SelectedItem = item;
                    setup = true;
                    return;
                }
            }

            setup = true;
            cbxCharge.SelectedIndex = selectedIndex;
        }

        private void getDcPreset(string searchName)
        {
            int selectedIndex = 0; // index to select if the search fails

            foreach (var item in cbxDischarge.Items)
            {
                if (item.ToString() == searchName)
                {
                    cbxDischarge.SelectedItem = item;
                    setup = true;
                    return;
                }
            }

            setup = true;
            cbxDischarge.SelectedIndex = selectedIndex;
        }

        private void getResumePreset(string searchName)
        {
            int selectedIndex = 0; // index to select if the search fails

            foreach (var item in cbxResume.Items)
            {
                if (item.ToString() == searchName)
                {
                    cbxResume.SelectedItem = item;
                    setup = true;
                    return;
                }
            }

            setup = true;
            cbxDischarge.SelectedIndex = selectedIndex;
        }

        private void btnChargeLoad_Click(object sender, RoutedEventArgs e)
        {
            setup = false;
            string oldPreset = Settings.Default.acPreset;
            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                cbxCharge.Items.Clear();
                cbxCharge.Items.Add("None");

                cbxCharge.Items.Add("PM - Eco Preset");

                cbxCharge.Items.Add("PM - Balanced Preset");

                cbxCharge.Items.Add("PM - Performance Preset");

                cbxCharge.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxCharge.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                cbxCharge.Items.Clear();
                cbxCharge.Items.Add("None");

                cbxCharge.Items.Add("PM - Eco Preset");

                cbxCharge.Items.Add("PM - Balanced Preset");

                cbxCharge.Items.Add("PM - Performance Preset");

                cbxCharge.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxCharge.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                cbxCharge.Items.Clear();
                cbxCharge.Items.Add("None");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxCharge.Items.Add(presetName);
                }
            }


            getAcPreset(oldPreset);
        }

        private void btnDischargeLoad_Click(object sender, RoutedEventArgs e)
        {
            setup = false;
            string oldPreset = Settings.Default.dcPreset;
            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                cbxDischarge.Items.Clear();
                cbxDischarge.Items.Add("None");


                cbxDischarge.Items.Add("PM - Eco Preset");

                cbxDischarge.Items.Add("PM - Balanced Preset");

                cbxDischarge.Items.Add("PM - Performance Preset");

                cbxDischarge.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxDischarge.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                cbxDischarge.Items.Clear();
                cbxDischarge.Items.Add("None");

                cbxDischarge.Items.Add("PM - Eco Preset");

                cbxDischarge.Items.Add("PM - Balanced Preset");

                cbxDischarge.Items.Add("PM - Performance Preset");

                cbxDischarge.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxDischarge.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                cbxDischarge.Items.Clear();
                cbxDischarge.Items.Add("None");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxDischarge.Items.Add(presetName);
                }
            }

            getDcPreset(oldPreset);
        }

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            setup = false;
            string oldPreset = Settings.Default.resumePreset;
            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                cbxResume.Items.Clear();
                cbxResume.Items.Add("None");

                cbxResume.Items.Add("PM - Eco Preset");

                cbxResume.Items.Add("PM - Balanced Preset");

                cbxResume.Items.Add("PM - Performance Preset");

                cbxResume.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxResume.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
            {
                amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                cbxResume.Items.Clear();
                cbxResume.Items.Add("None");

                cbxResume.Items.Add("PM - Eco Preset");

                cbxResume.Items.Add("PM - Balanced Preset");

                cbxResume.Items.Add("PM - Performance Preset");

                cbxResume.Items.Add("PM - Extreme Preset");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxResume.Items.Add(presetName);
                }
            }

            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

                // Get the names of all the stored presets
                IEnumerable<string> presetNames = intelPresetManager.GetPresetNames();

                cbxResume.Items.Clear();
                cbxResume.Items.Add("None");

                // Populate a combo box with the preset names
                foreach (string presetName in presetNames)
                {
                    cbxResume.Items.Add(presetName);
                }
            }


            getResumePreset(oldPreset);
        }

        private void cbxCharge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                if (setup == true)
                {
                    string presetName = (sender as ComboBox).SelectedItem as string;

                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                        {
                            if(presetName.Contains("PM - Eco"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.EcoPreset;
                            }
                            else if (presetName.Contains("PM - Bal"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.BalPreset;
                            }
                            else if (presetName.Contains("PM - Perf"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.PerformancePreset;
                            }
                            else if (presetName.Contains("PM - Ext"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.ExtremePreset;
                            }
                            else
                            {
                                apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
                                Preset myPreset = apuPresetManager.GetPreset(presetName);
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = myPreset.commandValue;
                            }
                        }
                        else
                        {
                            Settings.Default.acPreset = presetName;
                            Settings.Default.acCommandString = "";
                        }
                    }
                    if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                    {
                        amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
                        if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                        {
                            if (presetName.Contains("PM - Eco"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.EcoPreset;
                            }
                            else if (presetName.Contains("PM - Bal"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.BalPreset;
                            }
                            else if (presetName.Contains("PM - Perf"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.PerformancePreset;
                            }
                            else if (presetName.Contains("PM - Ext"))
                            {
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = PremadePresets.ExtremePreset;
                            }
                            else
                            {
                                Preset myPreset = amdDtCpuPresetManager.GetPreset(presetName);
                                Settings.Default.acPreset = presetName;
                                Settings.Default.acCommandString = myPreset.commandValue;
                            }
                        }
                        else
                        {
                            Settings.Default.acPreset = presetName;
                            Settings.Default.acCommandString = "";
                        }
                    }

                    if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
                        if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                        {
                            Preset myPreset = intelPresetManager.GetPreset(presetName);
                            Settings.Default.acPreset = presetName;
                            Settings.Default.acCommandString = myPreset.commandValue;
                        }
                        else
                        {
                            Settings.Default.acPreset = presetName;
                            Settings.Default.acCommandString = "";
                        }
                    }

                    Settings.Default.Save();
                }        
            } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void cbxDischarge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (setup == true)
            {
                string presetName = (sender as ComboBox).SelectedItem as string;

                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
                    if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                    {
                        if (presetName.Contains("PM - Eco"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.EcoPreset;
                        }
                        else if (presetName.Contains("PM - Bal"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.BalPreset;
                        }
                        else if (presetName.Contains("PM - Perf"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.PerformancePreset;
                        }
                        else if (presetName.Contains("PM - Ext"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.ExtremePreset;
                        }
                        else
                        {
                            Preset myPreset = apuPresetManager.GetPreset(presetName);
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = myPreset.commandValue;
                        }
                    }
                    else
                    {
                        Settings.Default.dcPreset = presetName;
                        Settings.Default.dcCommandString = "";
                    }
                }
                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
                    if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                    {
                        if (presetName.Contains("PM - Eco"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.EcoPreset;
                        }
                        else if (presetName.Contains("PM - Bal"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.BalPreset;
                        }
                        else if (presetName.Contains("PM - Perf"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.PerformancePreset;
                        }
                        else if (presetName.Contains("PM - Ext"))
                        {
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = PremadePresets.ExtremePreset;
                        }
                        else
                        {
                            Preset myPreset = amdDtCpuPresetManager.GetPreset(presetName);
                            Settings.Default.dcPreset = presetName;
                            Settings.Default.dcCommandString = myPreset.commandValue;
                        }
                    }
                    else
                    {
                        Settings.Default.dcPreset = presetName;
                        Settings.Default.dcCommandString = "";
                    }
                }
                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
                    if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                    {
                        Preset myPreset = intelPresetManager.GetPreset(presetName);
                        Settings.Default.dcPreset = presetName;
                        Settings.Default.dcCommandString = myPreset.commandValue;
                    }
                    else
                    {
                        Settings.Default.dcPreset = presetName;
                        Settings.Default.dcCommandString = "";
                    }
                }

                Settings.Default.Save();
            }
        }

        private void cbxResume_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (setup == true)
                {
                    string presetName = (sender as ComboBox).SelectedItem as string;

                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                        {
                            if (presetName.Contains("PM - Eco"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.EcoPreset;
                            }
                            else if (presetName.Contains("PM - Bal"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.BalPreset;
                            }
                            else if (presetName.Contains("PM - Perf"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.PerformancePreset;
                            }
                            else if (presetName.Contains("PM - Ext"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.ExtremePreset;
                            }
                            else
                            {
                                apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
                                Preset myPreset = apuPresetManager.GetPreset(presetName);
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = myPreset.commandValue;
                            }
                        }
                        else
                        {
                            Settings.Default.resumePreset = presetName;
                            Settings.Default.resumeCommandString = "";
                        }
                    }
                    if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                    {
                        amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
                        if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                        {
                            if (presetName.Contains("PM - Eco"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.EcoPreset;
                            }
                            else if (presetName.Contains("PM - Bal"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.BalPreset;
                            }
                            else if (presetName.Contains("PM - Perf"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.PerformancePreset;
                            }
                            else if (presetName.Contains("PM - Ext"))
                            {
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = PremadePresets.ExtremePreset;
                            }
                            else
                            {
                                Preset myPreset = amdDtCpuPresetManager.GetPreset(presetName);
                                Settings.Default.resumePreset = presetName;
                                Settings.Default.resumeCommandString = myPreset.commandValue;
                            }
                        }
                        else
                        {
                            Settings.Default.resumePreset = presetName;
                            Settings.Default.resumeCommandString = "";
                        }
                    }

                    if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
                        if ((sender as ComboBox).SelectedItem != (sender as ComboBox).Items[0])
                        {
                            Preset myPreset = intelPresetManager.GetPreset(presetName);
                            Settings.Default.resumePreset = presetName;
                            Settings.Default.resumeCommandString = myPreset.commandValue;
                        }
                        else
                        {
                            Settings.Default.resumePreset = presetName;
                            Settings.Default.resumeCommandString = "";
                        }
                    }

                    Settings.Default.Save();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
