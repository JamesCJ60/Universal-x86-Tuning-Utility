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
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            if(cbPower.IsChecked == true) await Task.Run(() => ChangeTDP.changeTDP((int)nudPL1.Value, (int)nudPL2.Value));

            if (cbVoltage.IsChecked == true) { string voltageOffset = $"set --allow-overvolt --commit 0 {nudCore.Value.ToString()} 1 {nudGPU.Value.ToString()} 2 {nudCache.Value.ToString()} 3 {nudAgent.Value.ToString()} 4 {nudGPUSlice.Value.ToString()}"; BasicExeBackend.ApplySettings("bin//intel//IntelVoltageControl.exe", voltageOffset, true); }
        }
    }
}
