using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts.Misc;

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
                        if (GetSystemInfo.Product.ToLower().Contains("laptop 16 (amd ryzen 7040") && GetSystemInfo.Manufacturer.ToLower().Contains("framework"))
                        {
                            uri = new Uri("pack://application:,,,/Assets/Laptops/Framework/framework-laptop-16.png");
                            bool has7700S = GetSystemInfo.IsGPUPresent("AMD Radeon(TM) RX 7700S");

                            if (has7700S)
                            {
                                EcoPreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=45 --stapm-limit=30000 --fast-limit=35000 --slow-limit=30000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=0 ";
                                BalPreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=95000 --fast-limit=95000 --slow-limit=95000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=1 ";
                                PerformancePreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=100000 --fast-limit=100000 --slow-limit=120000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=2 ";
                                ExtremePreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=120000 --fast-limit=140000 --slow-limit=120000 --vrm-current=200000 --vrmmax-current=200000 --vrmsoc-current=200000 --vrmsocmax-current=200000 --vrmgfx-current=200000 --Win-Power=2 ";
                            }
                            else
                            {
                                EcoPreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=45 --stapm-limit=6000 --fast-limit=8000 --slow-limit=6000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=0 ";
                                BalPreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=35000 --fast-limit=45000 --slow-limit=38000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=1 ";
                                PerformancePreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=45000 --fast-limit=55000 --slow-limit=50000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=2 ";
                                ExtremePreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=55000 --fast-limit=70000 --slow-limit=65000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=2 ";
                            }
                        }
                        else if (GetSystemInfo.Product.ToLower().Contains("laptop 13 (amd ryzen 7040") && GetSystemInfo.Manufacturer.ToLower().Contains("framework") || GetSystemInfo.Product.ToLower().Contains("laptop 13 (amd ryzen ai 300") && GetSystemInfo.Manufacturer.ToLower().Contains("framework"))
                        {
                            uri = new Uri("pack://application:,,,/Assets/Laptops/Framework/framework-laptop-13.png");

                            EcoPreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=45 --stapm-limit=8000 --fast-limit=10000 --slow-limit=8000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=0 ";
                            BalPreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=15000 --fast-limit=18000 --slow-limit=15000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=1 ";
                            PerformancePreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=28000 --fast-limit=42000 --slow-limit=28000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=2 ";
                            ExtremePreset = "--tctl-temp=100 --cHTC-temp=100 --apu-skin-temp=50 --stapm-limit=35000 --fast-limit=60000 --slow-limit=35000 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 --Win-Power=2 ";
                        }
                        else {
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

                                if (cpuName.Contains("U") || cpuName.Contains("AI") && !cpuName.Contains("HX"))
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

                                if (Family.FAM == Family.RyzenFamily.StrixHalo)
                                {
                                    EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=18000  --fast-limit=25000 --stapm-time=64 --slow-limit=18000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    BalPreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=55000  --fast-limit=65000 --stapm-time=64 --slow-limit=55000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=100000  --fast-limit=120000 --stapm-time=64 --slow-limit=100000 --slow-time=128 --vrm-current=240000 --vrmmax-current=240000 --vrmsoc-current=240000 --vrmsocmax-current=240000 --vrmgfx-current=240000 ";
                                    ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=145000  --fast-limit=165000 --stapm-time=64 --slow-limit=145000 --slow-time=128 --vrm-current=240000 --vrmmax-current=240000 --vrmsoc-current=240000 --vrmsocmax-current=240000 --vrmgfx-current=240000 ";
                                }

                                if (Family.FAM == Family.RyzenFamily.DragonRange || Family.FAM == Family.RyzenFamily.FireRange)
                                {
                                    EcoPreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=35000  --fast-limit=45000 --stapm-time=64 --slow-limit=35000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    BalPreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=65000  --fast-limit=75000 --stapm-time=64 --slow-limit=65000 --slow-time=128 --vrm-current=180000 --vrmmax-current=180000 --vrmsoc-current=180000 --vrmsocmax-current=180000 --vrmgfx-current=180000 ";
                                    PerformancePreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=100000  --fast-limit=120000 --stapm-time=64 --slow-limit=100000 --slow-time=128 --vrm-current=240000 --vrmmax-current=240000 --vrmsoc-current=240000 --vrmsocmax-current=240000 --vrmgfx-current=240000 ";
                                    ExtremePreset = "--tctl-temp=95 --cHTC-temp=95 --stapm-limit=125000  --fast-limit=145000 --stapm-time=64 --slow-limit=125000 --slow-time=128 --vrm-current=240000 --vrmmax-current=240000 --vrmsoc-current=240000 --vrmsocmax-current=240000 --vrmgfx-current=240000 ";
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
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=45000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=95000 --edc-limit=122000 --tdc-limit=122000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                }
                                else if (cpuName.Contains("X3D"))
                                {
                                    EcoPreset = "--tctl-temp=85 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=85 --ppt-limit=85000 --edc-limit=120000 --tdc-limit=120000 ";
                                    PerformancePreset = "--tctl-temp=85 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                    ExtremePreset = "--tctl-temp=85 --ppt-limit=140000 --edc-limit=190000 --tdc-limit=190000 ";
                                }
                                else if (cpuName.Contains("X") && CpuName[2].Contains("9"))
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=95000 --edc-limit=130000 --tdc-limit=130000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=125000 --edc-limit=142000 --tdc-limit=142000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=170000 --edc-limit=230000 --tdc-limit=230000 ";
                                }
                                else if (cpuName.Contains("X"))
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=88000 --edc-limit=125000 --tdc-limit=125000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=140000 --edc-limit=190000 --tdc-limit=190000 ";
                                }
                                else
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=45000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=88000 --edc-limit=125000 --tdc-limit=125000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                }
                            }
                            else
                            {
                                uri = new Uri("pack://application:,,,/Assets/config-DT-AM5.png");

                                if (cpuName.Contains("E"))
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=45000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=95000 --edc-limit=122000 --tdc-limit=122000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                }
                                else if (cpuName.Contains("X3D"))
                                {
                                    EcoPreset = "--tctl-temp=85 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=85 --ppt-limit=85000 --edc-limit=120000 --tdc-limit=120000 ";
                                    PerformancePreset = "--tctl-temp=85 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                    ExtremePreset = "--tctl-temp=85 --ppt-limit=140000 --edc-limit=190000 --tdc-limit=190000 ";
                                }
                                else if (cpuName.Contains("X") && CpuName[2].Contains("9"))
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=145000 --tdc-limit=145000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=145000 --edc-limit=210000 --tdc-limit=210000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=230000 --edc-limit=310000 --tdc-limit=310000 ";
                                }
                                else if (cpuName.Contains("X"))
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=88000 --edc-limit=125000 --tdc-limit=125000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=140000 --edc-limit=190000 --tdc-limit=190000 ";
                                }
                                else
                                {
                                    EcoPreset = "--tctl-temp=95 --ppt-limit=45000 --edc-limit=90000 --tdc-limit=90000 ";
                                    BalPreset = "--tctl-temp=95 --ppt-limit=65000 --edc-limit=90000 --tdc-limit=90000 ";
                                    PerformancePreset = "--tctl-temp=95 --ppt-limit=88000 --edc-limit=125000 --tdc-limit=125000 ";
                                    ExtremePreset = "--tctl-temp=95 --ppt-limit=105000 --edc-limit=142000 --tdc-limit=142000 ";
                                }
                            }
                        }
                    }
                }
                else if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    SetIntelPremadePresets();
                }
            }
            catch (Exception ex)
            {
                Misc.DiagnosticLogger.LogError(ex, "Failed to set premade presets");
            }
        }

        private static void SetIntelPremadePresets()
        {
            uri = new Uri("pack://application:,,,/Assets/config.png");
            var limits = GetIntelPowerLimits(Family.CPUName);
            var ecoPl1 = Math.Max(4, (int)Math.Round(limits.Pl1 * 0.55));
            var ecoPl2 = Math.Max(ecoPl1 + 2, (int)Math.Round(limits.Pl1 * 0.8));
            var balancedPl1 = Math.Max(4, (int)Math.Round(limits.Pl1 * 0.8));
            var balancedPl2 = Math.Max(balancedPl1 + 2, (int)Math.Round(limits.Pl2 * 0.5));
            var performancePl1 = limits.Pl1;
            var performancePl2 = Math.Max(performancePl1 + 2, (int)Math.Round(limits.Pl2 * 0.75));

            EcoPreset = BuildIntelPreset(ecoPl1, ecoPl2, 3, 75, 0);
            BalPreset = BuildIntelPreset(balancedPl1, balancedPl2, 1, 45, 1);
            PerformancePreset = BuildIntelPreset(performancePl1, performancePl2, 2, 20, 2);
            ExtremePreset = BuildIntelPreset(limits.Pl1, limits.Pl2, 2, 0, 2);
        }

        private static string BuildIntelPreset(int pl1, int pl2, int boostMode, int energyPreference, int powerMode)
        {
            pl2 = Math.Max(pl1 + 2, pl2);
            return $"--intel-pl={pl1},{pl2} --Win-CPU={boostMode},-1,-1,{energyPreference} --Win-Power={powerMode} ";
        }

        private static (int Pl1, int Pl2) GetIntelPowerLimits(string processorName)
        {
            var name = processorName.ToUpperInvariant();
            var match = Regex.Match(name, @"(?:I[3579]-|ULTRA\s+[3579]\s+)(?<model>\d{3,5})(?<suffix>[A-Z0-9]*)");
            var model = match.Success ? match.Groups["model"].Value : string.Empty;
            var suffix = match.Success ? match.Groups["suffix"].Value : string.Empty;
            var generation = 0;
            if (model.Length == 5 && int.TryParse(model[..2], out var twoDigitGeneration)) generation = twoDigitGeneration;
            else if (model.Length == 4 && (model.StartsWith("10") || model.StartsWith("11")) && int.TryParse(model[..2], out var laterFourDigitGeneration)) generation = laterFourDigitGeneration;
            else if (model.Length >= 4 && int.TryParse(model[..1], out var oneDigitGeneration)) generation = oneDigitGeneration;

            if (name.Contains("ULTRA"))
            {
                if (suffix.StartsWith("H")) return (28, 115);
                if (suffix.StartsWith("U")) return (15, 57);
                if (suffix.StartsWith("V")) return (17, 37);
            }

            if (suffix.Contains("HX")) return (55, 157);
            if (suffix.StartsWith("H") || suffix.Contains("HK") || suffix.Contains("HQ")) return generation >= 12 ? (45, 115) : (45, 90);
            if (suffix.StartsWith("P")) return (28, 64);
            if (suffix.StartsWith("Y")) return (7, 15);
            if (suffix.StartsWith("U") || suffix.Contains("G")) return generation >= 11 ? (15, 55) : (15, 25);
            if (suffix.StartsWith("T")) return (35, 65);
            if (suffix.Contains("K") || suffix.Contains("X")) return generation >= 12 ? (125, 241) : (95, 119);
            if (generation >= 12)
            {
                if (name.Contains("I3-")) return (60, 89);
                if (name.Contains("I5-")) return (65, 117);
                if (name.Contains("I7-")) return (65, 180);
                return (65, 219);
            }

            if (name.Contains("I3-")) return (65, 75);
            if (name.Contains("I5-")) return (65, 95);
            if (name.Contains("I7-")) return (65, 125);
            if (name.Contains("I9-")) return (65, 165);
            return (35, 65);
        }
    }
}
