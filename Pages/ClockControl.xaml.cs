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
using UXTU.Properties;

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
            nudCoreClock.Value = Settings.Default.AllCoreClk;
            nudCoreVolt.Value = Settings.Default.CPUVID;
            nudBus.Value = (double)Settings.Default.BusCLK;
            nudCOCPU.Value = Settings.Default.COCPU;
            nudCOIGPU.Value = Settings.Default.COiGPU;
            nudiGPU.Value = Settings.Default.iGPUClk;
            nuddGPUCore.Value = Settings.Default.dGPUCLK;
            nuddGPUMem.Value = Settings.Default.dGPUMem;

            cbCoreClock.IsChecked = Settings.Default.isAllCoreCLK;
            cbCoreVolt.IsChecked = Settings.Default.isVID;
            cbBus.IsChecked = Settings.Default.isBUS;
            cbCOCPU.IsChecked = Settings.Default.isCPUCO;
            cbCOIGPU.IsChecked = Settings.Default.isGPUCO;
            cbdGPUCore.IsChecked = Settings.Default.isNV;
        }

        private void Disable_Click(object sender, RoutedEventArgs e)
        {
            SendCommand.set_disable_oc();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            Settings.Default.AllCoreClk = (int)nudCoreClock.Value;
            Settings.Default.CPUVID = (int)nudCoreVolt.Value;
            Settings.Default.BusCLK = (double)nudBus.Value;
            Settings.Default.COCPU = (int)nudCOCPU.Value;
            Settings.Default.COiGPU = (int)nudCOIGPU.Value;
            Settings.Default.iGPUClk = (int)nudiGPU.Value;
            Settings.Default.dGPUCLK = (int)nuddGPUCore.Value;
            Settings.Default.dGPUMem = (int)nuddGPUMem.Value;
            Settings.Default.Save();

            if (cbCoreClock.IsChecked == true)
            {
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)nudCoreClock.Value);
                SendCommand.set_enable_oc();
                i++;
            }

            if(cbCoreVolt.IsChecked == true)
            {
                double vid = Math.Round((double)nudCoreVolt.Value / 1000, 2);
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_enable_oc();
                i++;
            }

            if(cbBus.IsChecked == true)
            {
                RwMmioAmd MMIO = new RwMmioAmd();
                MMIO.SetBclk(Convert.ToDouble(nudBus.Value));
                i++;
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
                i++;
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
                i++;
            }

            if(cbiGPU.IsChecked == true)
            {
                SendCommand.set_gfx_clk((uint)nudiGPU.Value);
                i++;
            }

            if(cbdGPUCore.IsChecked == true)
            {
                //Get RyzenAdj path
                string path = "\\bin\\oc.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(path, "0 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                BasicExeBackend.ApplySettings(path, "1 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                BasicExeBackend.ApplySettings(path, "2 " + nuddGPUCore.Value + " " + nuddGPUMem.Value, true);
                i++;
            }

            if(i == 0)
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }
            else
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
            }

            Settings.Default.isAllCoreCLK = (bool)cbCoreClock.IsChecked;
            Settings.Default.isVID = (bool)cbCoreVolt.IsChecked;
            Settings.Default.isBUS = (bool)cbBus.IsChecked;
            Settings.Default.isCPUCO = (bool)cbCOCPU.IsChecked;
            Settings.Default.isGPUCO = (bool)cbCOIGPU.IsChecked;
            Settings.Default.isNV = (bool)cbdGPUCore.IsChecked;
            Settings.Default.Save();
        }
    }
}
