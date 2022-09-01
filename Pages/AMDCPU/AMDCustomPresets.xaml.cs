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

namespace AATUV3.Pages.AMDCPU
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class AMDCustomPresets : UserControl
    {
        public AMDCustomPresets()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if(cbPower.IsChecked == true)
            {
                uint TJ = (uint)nudTJ.Value;
                uint PPT = (uint)nudPPT.Value * 1000;
                uint EDC = (uint)nudEDC.Value * 1000;
                uint TDC = (uint)nudTDC.Value * 1000;
                uint Scaler = (uint)nudpboScaler.Value;

                SendCommand.set_tctl_temp(TJ);
                SendCommand.set_ppt(PPT);
                SendCommand.set_edc(EDC);
                SendCommand.set_tdc(TDC);
                SendCommand.set_scaler(Scaler);
            }
        }
    }
}
