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
using UXTU.Scripts;
using UXTU.Properties;
using System.IO;
using System.Diagnostics;
using UXTU.Scripts.Adpative_Modes.Performance;
using UXTU.Scripts.ADLX;
using System.Windows.Threading;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    /// 

    public partial class APM : UserControl
    {
        public DispatcherTimer reapplyAPM = new DispatcherTimer();

        public APM()
        {
            InitializeComponent();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() == "Start Mode")
            {
                btnStart.Content = "Stop Mode";

                GlobalVariables.AdaptivePerf = true;
                if (cbCPU.IsChecked == true)
                {
                    int powermode = 0;

                    if (rbPower.IsChecked == true) powermode = 0;
                    if (rbBal.IsChecked == true) powermode = 1;
                    if (rbMax.IsChecked == true) powermode = 2;

                    if (cbTDP.IsChecked == true)
                    {
                        OverlayWindow.tdpOverride = (bool)cbTDP.IsChecked;
                        OverlayWindow.maxPowerOverride = (int)nudTDP.Value;
                    }

                    if (cbTemp.IsChecked == true)
                    {
                        OverlayWindow.tempOverride = (bool)cbTemp.IsChecked;
                        OverlayWindow.maxTempOverride = (int)nudTemp.Value;
                    }

                    if (cbTDP.IsChecked == true)
                    {
                        OverlayWindow.uvOverride = (bool)cbCO.IsChecked;
                        OverlayWindow.maxCOOverride = (int)nudCO.Value;
                    }

                    OverlayWindow.tboCPU = (bool)cbCPU.IsChecked;
                    OverlayWindow.tboiGPU = (bool)cbiGPU.IsChecked;

                    if (cbiGPUTemp.IsChecked == true) { OverlayWindow.tboiGPUTempOverride = true; OverlayWindow.tboiGPUMaxTemp = (int)nudiGPUTemp.Value; }
                    else OverlayWindow.tboiGPUTempOverride = false;

                    if (cbiGPUClockOffset.IsChecked == true) { OverlayWindow.tboiGPUOffset = (int)nudiGPUClockOffset.Value; }
                    else OverlayWindow.tboiGPUOffset = 0;

                    bool isEnabled = false;
                    if (cbsFPS.IsChecked == true) isEnabled = true;
                    int fpsLimit = (int)nudsFPS.Value;

                    ADLXBackend.SetFPSLimit(0, isEnabled, fpsLimit);

                    OverlayWindow.powermode = powermode;
                }
                else
                {
                    btnStart.Content = "Start Mode";
                    GlobalVariables.AdaptivePerf = false;
                    if (cbCPU.IsChecked == true)
                    {
                        CpuPowerLimiter.updateBattery(1, 1, 1);
                    }
                }
            }
        }
    }
}
