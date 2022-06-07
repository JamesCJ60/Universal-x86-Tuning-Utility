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

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class ClockControl : UserControl
    {
        public ClockControl()
        {
            InitializeComponent();
        }

        private void Disable_Click(object sender, RoutedEventArgs e)
        {
            SendCommand.set_disable_oc();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (cbCoreClock.IsChecked == true)
            {
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
            }

            if(cbCoreVolt.IsChecked == true)
            {
                double vid = Math.Round((double)nudCoreVolt.Value / 1000, 2);
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_enable_oc();
            }

            if(cbBus.IsChecked == true)
            {
                RwMmioAmd MMIO = new RwMmioAmd();
                MMIO.SetBclk(Convert.ToDouble(nudBus.Value));
            }

            if(cbCOCPU.IsChecked == true)
            {
                if(nudCOCPU.Value >= 0)
                {
                    SendCommand.set_coall((uint)nudCOCPU.Value);
                }else
                {
                    SendCommand.set_coall(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudCOCPU.Value)));
                }
            }

            if (cbCOIGPU.IsChecked == true)
            {
                if (nudCOCPU.Value >= 0)
                {
                    SendCommand.set_cogfx((uint)nudCOIGPU.Value);
                }
                else
                {
                    SendCommand.set_cogfx(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudCOIGPU.Value)));
                }
            }

            if(cbiGPU.IsChecked == true)
            {
                SendCommand.set_gfx_clk((uint)nudiGPU.Value);
            }

            if(cbdGPUCore.IsChecked == true)
            {
                //Get RyzenAdj path
                string path = "\\bin\\oc.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(path, "0 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                BasicExeBackend.ApplySettings(path, "1 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                BasicExeBackend.ApplySettings(path, "2 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
            }
        }
    }
}
