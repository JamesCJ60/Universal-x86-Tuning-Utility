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
                SendCommand.set_ppt((uint)nudPPT.Value * 1000);
                SendCommand.set_edc((uint)nudEDC.Value * 1000);
                SendCommand.set_tdc((uint)nudTDC.Value * 1000);
            }
        }
    }
}
