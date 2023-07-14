using ABI.System;
using RyzenSmu;
using System;
using System.Collections.Generic;
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
using Uri = System.Uri;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for Premade.xaml
    /// </summary>
    public partial class Premade : Page
    {
        private string ExtremePreset = "", PerformancePreset = "", BalPreset = "", EcoPreset = "";

        private void tbPerf_Click(object sender, RoutedEventArgs e)
        {
            update();
            perfPreset();
        }

        private void tbBal_Click(object sender, RoutedEventArgs e)
        {
            update();
            balPreset();
        }

        private void tbEx_Click(object sender, RoutedEventArgs e)
        {
            update();
            exPreset();
        }

        private void tbEco_Click(object sender, RoutedEventArgs e)
        {
            update();
            ecoPreset();
        }

        private string cpuName = "";
        public Premade()
        {
            try
            {
                InitializeComponent();
                _ = Tablet.TabletDevices;
                update();
            }
            catch { }
        }

        private void update()
        {
            try {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu || Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    cpuName = Family.CPUName.Replace("AMD", null).Replace("with", null).Replace("Mobile", null).Replace("Ryzen", null).Replace("Radeon", null).Replace("Graphics", null).Replace("Vega", null).Replace("Gfx", null);

                    var image = imgPackage;
                    var uri = new Uri("pack://application:,,,/Assets/config.png");

                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        if (Family.FAM < Family.RyzenFamily.Matisse)
                        {
                            if (cpuName.Contains("U") || cpuName.Contains("e") || cpuName.Contains("Ce"))
                            {
                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=15000  --fast-limit=18000 --stapm-time=64 --slow-limit=16000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=18000  --fast-limit=20000 --stapm-time=64 --slow-limit=19000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=28000  --fast-limit=28000 --stapm-time=64 --slow-limit=28000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("H"))
                            {
                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=30000  --fast-limit=35000 --stapm-time=64 --slow-limit=33000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=35000  --fast-limit=42000 --stapm-time=64 --slow-limit=40000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=56000  --fast-limit=56000 --stapm-time=64 --slow-limit=56000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("GE"))
                            {
                                uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");

                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=15000  --fast-limit=15000 --stapm-time=64 --slow-limit=18000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=45000  --fast-limit=55000 --stapm-time=64 --slow-limit=48000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=55000  --fast-limit=65000 --stapm-time=64 --slow-limit=60000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=65000  --fast-limit=80000 --stapm-time=64 --slow-limit=75000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("G"))
                            {
                                uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");

                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=15000  --fast-limit=18000 --stapm-time=64 --slow-limit=18000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=65000  --fast-limit=75000 --stapm-time=64 --slow-limit=65000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=80000  --fast-limit=75000 --stapm-time=64 --slow-limit=75000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=85000  --fast-limit=95000 --stapm-time=64 --slow-limit=90000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                        }

                        if (Family.FAM > Family.RyzenFamily.Matisse)
                        {

                            if (cpuName.Contains("U"))
                            {
                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=22000  --fast-limit=24000 --stapm-time=64 --slow-limit=22000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=28000  --fast-limit=28000 --stapm-time=64 --slow-limit=28000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=30000  --fast-limit=34000 --stapm-time=64 --slow-limit=32000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("HX"))
                            {
                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=55000  --fast-limit=65000 --stapm-time=64 --slow-limit=55000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=78000  --fast-limit=70000 --stapm-time=64 --slow-limit=70000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=85000  --fast-limit=95000 --stapm-time=64 --slow-limit=90000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("HS"))
                            {
                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=35000  --fast-limit=45000 --stapm-time=64 --slow-limit=38000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=45000  --fast-limit=55000 --stapm-time=64 --slow-limit=50000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=55000  --fast-limit=70000 --stapm-time=64 --slow-limit=65000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("H"))
                            {
                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=45000  --fast-limit=55000 --stapm-time=64 --slow-limit=48000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=55000  --fast-limit=65000 --stapm-time=64 --slow-limit=60000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=65000  --fast-limit=80000 --stapm-time=64 --slow-limit=75000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("GE"))
                            {
                                uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");

                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=15000  --fast-limit=15000 --stapm-time=64 --slow-limit=18000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=45000  --fast-limit=55000 --stapm-time=64 --slow-limit=48000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=55000  --fast-limit=65000 --stapm-time=64 --slow-limit=60000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=65000  --fast-limit=80000 --stapm-time=64 --slow-limit=75000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }
                            else if (cpuName.Contains("G"))
                            {
                                uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");

                                EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=15000  --fast-limit=18000 --stapm-time=64 --slow-limit=18000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=65000  --fast-limit=75000 --stapm-time=64 --slow-limit=65000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=80000  --fast-limit=75000 --stapm-time=64 --slow-limit=75000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=85000  --fast-limit=95000 --stapm-time=64 --slow-limit=90000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                            }

                            if (Family.FAM == Family.RyzenFamily.Mendocino)
                            {
                                if (cpuName.Contains("U"))
                                {
                                    EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=6000  --fast-limit=8000 --stapm-time=64 --slow-limit=6000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    BalPreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=45 --stapm-limit=15000  --fast-limit=18000 --stapm-time=64 --slow-limit=16000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=18000  --fast-limit=20000 --stapm-time=64 --slow-limit=19000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --apu-skin-temp=95 --stapm-limit=28000  --fast-limit=28000 --stapm-time=64 --slow-limit=28000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                }
                            }
                        }
                    }

                    if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                    {
                        var CpuName = cpuName.Split(" ");

                        uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");

                        cpuName = CpuName[3];
                        if (Family.FAM < Family.RyzenFamily.Raphael)
                        {
                            if (cpuName.Contains("E"))
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=45 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=95 --edc-limit=122 --tdc-limit=122 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                            }
                            else if (cpuName.Contains("X3D"))
                            {
                                EcoPreset = "--tctl-temp=85 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=85 --ppt-limit=85 --edc-limit=120 --tdc-limit=120 ";
                                PerformancePreset = "--tctl-temp=85 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                                ExtremePreset = "--tctl-temp=85 --ppt-limit=140 --edc-limit=190 --tdc-limit=190 ";
                            }
                            else if (cpuName.Contains("X") && CpuName[2].Contains("9"))
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=95 --edc-limit=130 --tdc-limit=130 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=125 --edc-limit=142 --tdc-limit=142 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=170 --edc-limit=230 --tdc-limit=230 ";
                            }
                            else if (cpuName.Contains("X"))
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=88 --edc-limit=125 --tdc-limit=125 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=140 --edc-limit=190 --tdc-limit=190 ";
                            }
                            else
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=45 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=88 --edc-limit=125 --tdc-limit=125 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                            }
                        }
                        else
                        {
                            uri = new Uri("pack://application:,,,/Assets/config-DT-AM5.png");

                            if (cpuName.Contains("E"))
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=45 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=95 --edc-limit=122 --tdc-limit=122 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                            }
                            else if (cpuName.Contains("X3D"))
                            {
                                EcoPreset = "--tctl-temp=85 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=85 --ppt-limit=85 --edc-limit=120 --tdc-limit=120 ";
                                PerformancePreset = "--tctl-temp=85 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                                ExtremePreset = "--tctl-temp=85 --ppt-limit=140 --edc-limit=190 --tdc-limit=190 ";
                            }
                            else if (cpuName.Contains("X") && CpuName[2].Contains("9"))
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=145 --tdc-limit=145 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=145 --edc-limit=210 --tdc-limit=210 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=230 --edc-limit=310 --tdc-limit=310 ";
                            }
                            else if (cpuName.Contains("X"))
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=88 --edc-limit=125 --tdc-limit=125 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=140 --edc-limit=190 --tdc-limit=190 ";
                            }
                            else
                            {
                                EcoPreset = "--tctl-temp=95 --ppt-limit=45 --edc-limit=90 --tdc-limit=90 ";
                                BalPreset = "--tctl-temp=95 --ppt-limit=65 --edc-limit=90 --tdc-limit=90 ";
                                PerformancePreset = "--tctl-temp=95 --ppt-limit=88 --edc-limit=125 --tdc-limit=125 ";
                                ExtremePreset = "--tctl-temp=95 --ppt-limit=105 --edc-limit=142 --tdc-limit=142 ";
                            }
                        }
                    }

                    var bitmap = new BitmapImage(uri);
                    image.Source = bitmap;

                    int selectedPreset = Settings.Default.premadePreset;
                    if (selectedPreset == 0) ecoPreset();
                    if (selectedPreset == 1) balPreset();
                    if (selectedPreset == 2) perfPreset();
                    if (selectedPreset == 3) exPreset();
                }
            }
            catch { }
        }

        private async void perfPreset()
        {
            try
            {
                tbPerf.IsChecked = false;
                tbBal.IsChecked = false;
                tbEco.IsChecked = false;
                tbEx.IsChecked = false;
                tbPerf.IsChecked = true;

                tbPresetName.Text = "Performance Preset";
                tbPresetDesc.Text = "This preset is optimized for maximum performance by increasing the power limits of the APU/CPU, which allows it to run at higher clock speeds for longer periods of time. This can result in improved system responsiveness and faster load times in applications that require high levels of processing power.";
                tbUXTUPreset.Text = PerformancePreset;

                RyzenAdj_To_UXTU.Translate(PerformancePreset);

                ToastNotification.ShowToastNotification("Performance Preset Applied!", $"The performance premade power preset has been applied!");

                Settings.Default.CommandString = PerformancePreset;
                Settings.Default.premadePreset = 2;
                Settings.Default.Save();
            } catch
            {

            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Garbage.Garbage_Collect();
        }

        private async void exPreset()
        {
            try
            {
                tbPerf.IsChecked = false;
                tbBal.IsChecked = false;
                tbEco.IsChecked = false;
                tbEx.IsChecked = false;
                tbEx.IsChecked = true;

                tbPresetName.Text = "Extreme Preset";
                tbPresetDesc.Text = "This preset aims to push the power limits of the system to their maximum, allowing for the highest possible performance. This preset is designed for users who demand the most from their hardware and are willing to tolerate higher power consumption and potentially increased noise levels.";
                tbUXTUPreset.Text = ExtremePreset;
                Settings.Default.CommandString = ExtremePreset;

                RyzenAdj_To_UXTU.Translate(ExtremePreset);

                ToastNotification.ShowToastNotification("Extreme Preset Applied!", $"The extreme premade power preset has been applied!");

                Settings.Default.premadePreset = 3;
                Settings.Default.Save();
            } catch { }
        }

        private async void ecoPreset()
        {
            try
            {
                tbPerf.IsChecked = false;
                tbBal.IsChecked = false;
                tbEco.IsChecked = false;
                tbEx.IsChecked = false;
                tbEco.IsChecked = true;

                tbPresetName.Text = "Eco Preset";
                tbPresetDesc.Text = "This preset is designed to prioritize energy efficiency over performance. It sets power limits to conservative levels to reduce power consumption and heat generation, making it ideal for prolonged use in situations where maximizing battery life or minimizing energy usage is critical.";
                tbUXTUPreset.Text = EcoPreset;

                RyzenAdj_To_UXTU.Translate(EcoPreset);

                ToastNotification.ShowToastNotification("Eco Preset Applied!", $"The eco premade power preset has been applied!");

                Settings.Default.CommandString = EcoPreset;
                Settings.Default.premadePreset = 0;
                Settings.Default.Save();
            } catch { }
        }

        private async void balPreset()
        {
            try
            {
                tbPerf.IsChecked = false;
                tbBal.IsChecked = false;
                tbEco.IsChecked = false;
                tbEx.IsChecked = false;
                tbBal.IsChecked = true;

                tbPresetName.Text = "Balanced Preset";
                tbPresetDesc.Text = "This preset aims to find a balance between performance and power consumption, providing a stable and efficient experience. This preset sets the power limits to a level that balances performance and power usage, without sacrificing too much of either.";
                tbUXTUPreset.Text = BalPreset;

                RyzenAdj_To_UXTU.Translate(BalPreset);

                ToastNotification.ShowToastNotification("Balanced Preset Applied!", $"The balanced premade power preset has been applied!");

                Settings.Default.CommandString = BalPreset;
                Settings.Default.premadePreset = 1;
                Settings.Default.Save();
            } catch
            {

            }
        }
    }
}
