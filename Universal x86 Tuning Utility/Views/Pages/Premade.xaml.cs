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
                PremadePresets.SetPremadePresets();
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

                    if (GetSystemInfo.Product.ToLower().Contains("laptop 16 (amd ryzen 7040") && GetSystemInfo.Manufacturer.ToLower().Contains("framework"))
                    {
                        tbxMessage.Text = "Premade Presets - Framework Laptop 16 (AMD Ryzen 7040HS Series)";
                        bdgCertified.Visibility = Visibility.Visible;
                    }
                    else if (GetSystemInfo.Product.ToLower().Contains("laptop 13 (amd ryzen 7040") && GetSystemInfo.Manufacturer.ToLower().Contains("framework"))
                    {
                        tbxMessage.Text = "Premade Presets - Framework Laptop 13 (AMD Ryzen 7040U Series)";
                        bdgCertified.Visibility = Visibility.Visible;
                    }
                    else bdgCertified.Visibility = Visibility.Collapsed;
                   

                    PremadePresets.SetPremadePresets();

                    EcoPreset = PremadePresets.EcoPreset;
                    BalPreset = PremadePresets.BalPreset;
                    PerformancePreset = PremadePresets.PerformancePreset;
                    ExtremePreset = PremadePresets.ExtremePreset;

                    var image = imgPackage;
                    var bitmap = new BitmapImage(PremadePresets.uri);
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
