using System;
using System.Collections.Generic;
using System.Linq;
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
using RyzenSMUBackend;
using AATUV3.Scripts.SMU_Backend_Scripts;
using AATUV3.Scripts;
using UXTU.Scripts.Intel;
using System.IO;
using UXTU.Properties;

namespace AATUV3.Pages.Intel
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class IntelCustomPresets : UserControl
    {
        public IntelCustomPresets()
        {
            InitializeComponent();

            if (File.Exists(Settings.Default.Path + "\\I-LastUsedSettings.txt"))
            {
                var lines = File.ReadAllLines(Settings.Default.Path + "\\I-LastUsedSettings.txt");

                bool powerlimits = Convert.ToBoolean(lines[1]);
                cbPower.IsChecked = powerlimits;
                int pl0, pl1, pl2, pl4;

                pl0 = Convert.ToInt32(lines[2]);
                pl1 = Convert.ToInt32(lines[3]);
                pl2 = Convert.ToInt32(lines[4]);
                pl4 = Convert.ToInt32(lines[5]);

                nudPL1.Value = pl1;
                nudPL2.Value = pl2;

                bool voltages = Convert.ToBoolean(lines[8]);
                cbVoltage.IsChecked = voltages;

                int core, cache, igpuC, igpuU, agent;
                core = Convert.ToInt32(lines[9]);
                cache = Convert.ToInt32(lines[10]);
                igpuC = Convert.ToInt32(lines[11]);
                igpuU = Convert.ToInt32(lines[12]);
                agent = Convert.ToInt32(lines[13]);

                nudCore.Value = core;
                nudCache.Value = cache;
                nudGPU.Value = igpuC;
                nudGPUSlice.Value = igpuU;
                nudAgent.Value = agent;
            }
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int pl1 = Convert.ToInt32(nudPL1.Value);
                int pl2 = Convert.ToInt32(nudPL2.Value);

                if (cbPower.IsChecked == true) await Task.Run(() => ChangeTDP.changeTDP(pl1, pl2));

                if (cbVoltage.IsChecked == true) { string voltageOffset = $"set --allow-overvolt --commit 0 {nudCore.Value.ToString()} 1 {nudGPU.Value.ToString()} 2 {nudCache.Value.ToString()} 3 {nudAgent.Value.ToString()} 4 {nudGPUSlice.Value.ToString()}"; BasicExeBackend.ApplySettings("//bin//intel//IntelVoltageControl.exe", voltageOffset, true); }

                string LastUsed = $"[Power Limits]\n{cbPower.IsChecked.ToString()}\n0\n{nudPL1.Value.ToString()}\n{nudPL2.Value.ToString()}\n0\n\n[Voltages]\n{cbVoltage.IsChecked.ToString()}\n{nudCore.Value.ToString()}\n{nudCache.Value.ToString()}\n{nudGPU.Value.ToString()}\n{nudGPUSlice.Value.ToString()}\n{nudAgent.Value.ToString()}";

                File.WriteAllText(Settings.Default.Path + "\\I-LastUsedSettings.txt", LastUsed);

                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
            }
            catch (Exception ex)
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-was-an-issue-applying-settings!", false);
            }
        }
    }
}
