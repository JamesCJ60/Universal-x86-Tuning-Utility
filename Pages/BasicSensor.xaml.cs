using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSMUBackend;
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
using System.Windows.Threading;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class BasicSensor : UserControl
    {
        public BasicSensor()
        {
            InitializeComponent();
            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(1);
            sensor.Tick += SensorUpdate_Tick;
            sensor.Start();
        }

        void SensorUpdate_Tick(object sender, EventArgs e)
        {
            int highestPL = 0;

            CoreLat.Visibility = Visibility.Collapsed;

            if (string.Format("{0:x}", Addresses.PMTableVersion).Contains("400005") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("400004") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("400001") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("370000") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("370003") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("370004") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("370005"))
            {
                if ((int)GetSensor.getSensorValve("STAPM_LIMIT") > highestPL) highestPL = (int)GetSensor.getSensorValve("STAPM_LIMIT");
                if ((int)GetSensor.getSensorValve("PPT_LIMIT_FAST") > highestPL) highestPL = (int)GetSensor.getSensorValve("PPT_LIMIT_FAST");
                if ((int)GetSensor.getSensorValve("PPT_LIMIT_SLOW") > highestPL) highestPL = (int)GetSensor.getSensorValve("PPT_LIMIT_SLOW");

                lblAPUTemp.Text = $"{(int)GetSensor.getSensorValve("THM_VALUE_CORE")}°C/{(int)GetSensor.getSensorValve("THM_LIMIT_CORE")}°C";
                if((int)GetSensor.getSensorValve("STT_VALUE_APU") > 0)
                {
                    Skin.Visibility = Visibility.Visible;
                    lblSkinTemp.Text = $"{(int)GetSensor.getSensorValve("STT_VALUE_APU")}°C/{(int)GetSensor.getSensorValve("STT_LIMIT_APU")}°C";
                }else
                {
                    Skin.Visibility = Visibility.Collapsed;
                }
                
                lbliGPUTemp.Text = $"{(int)GetSensor.getSensorValve("GFX_TEMP")}°C/{(int)GetSensor.getSensorValve("THM_LIMIT_CORE")}°C";

                lblSTAPM.Text = $"{(int)GetSensor.getSensorValve("SOCKET_POWER")}W/{(int)GetSensor.getSensorValve("STAPM_LIMIT")}W";
                lblShortBoost.Text = $"{(int)GetSensor.getSensorValve("SOCKET_POWER")}W/{(int)GetSensor.getSensorValve("PPT_LIMIT_FAST")}W";
                lblLongBoost.Text = $"{(int)GetSensor.getSensorValve("SOCKET_POWER")}W/{(int)GetSensor.getSensorValve("PPT_LIMIT_SLOW")}W";
                lblSocket.Text = $"{(int)GetSensor.getSensorValve("SOCKET_POWER")}W/{highestPL}W";
                lblMem.Text = $"{GetSensor.getSensorValve("VDDIO_MEM_POWER").ToString("0.00")}W";

                lbliGPUClk.Text = $"{(int)GetSensor.getSensorValve("GFX_FREQEFF")}MHz";
                lblSoCClk.Text = $"{(int)GetSensor.getSensorValve("SOCCLK_FREQ")}MHz";
                lblMemClk.Text = $"{(int)GetSensor.getSensorValve("MEMCLK_FREQ")}MHz";
                lblFabricClk.Text = $"{(int)GetSensor.getSensorValve("FCLK_FREQ")}MHz";
                lblUncoreClk.Text = $"{(int)GetSensor.getSensorValve("UCLK_FREQ")}MHz";



                lblCore1Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_0")).ToString("0.00")}GHz";
                lblCore2Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_1")).ToString("0.00")}GHz";
                lblCore3Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_2")).ToString("0.00")}GHz";
                lblCore4Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_3")).ToString("0.00")}GHz";
                lblCore5Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_4")).ToString("0.00")}GHz";
                lblCore6Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_5")).ToString("0.00")}GHz";
                lblCore7Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_6")).ToString("0.00")}GHz";
                lblCore8Clk.Text = $"{(GetSensor.getSensorValve("CORE_FREQ_7")).ToString("0.00")}GHz";
            }

            if (string.Format("{0:x}", Addresses.PMTableVersion).Contains("400005") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("400004") || string.Format("{0:x}", Addresses.PMTableVersion).Contains("400001"))
            {
                CoreLat.Visibility = Visibility.Visible;
                lblCore1Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_0")}ns";
                lblCore2Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_1")}ns";
                lblCore3Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_2")}ns";
                lblCore4Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_3")}ns";
                lblCore5Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_4")}ns";
                lblCore6Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_5")}ns";
                lblCore7Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_6")}ns";
                lblCore8Lat.Text = $"{(int)GetSensor.getSensorValve("CORE_MEM_LATENCY_7")}ns";
            }
        }
    }
}
