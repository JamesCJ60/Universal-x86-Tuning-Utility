using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.Scripts
{
    internal class PremadePresets
    {
        public static string ExtremePreset = "", PerformancePreset = "", BalPreset = "", EcoPreset = "";
        private static string cpuName = "";
        public static Uri uri = null;
        public static void SetPremadePresets()
        {
            try
            {
                if (Family.TYPE == Family.ProcessorType.Amd_Apu || Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                {
                    cpuName = Family.CPUName.Replace("AMD", null).Replace("with", null).Replace("Mobile", null).Replace("Ryzen", null).Replace("Radeon", null).Replace("Graphics", null).Replace("Vega", null).Replace("Gfx", null);

                    uri = new Uri("pack://application:,,,/Assets/config.png");

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
                }
            }
            catch { }
        }
    }
}
