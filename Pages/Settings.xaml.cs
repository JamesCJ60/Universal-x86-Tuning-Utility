using UXTU.Properties;
using AATUV3.Scripts;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Xceed.Wpf.Toolkit;

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class SettingsMenu : UserControl
    {
        public SettingsMenu()
        {
            InitializeComponent();
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default["AutoReapply"] = (bool)cbAutoReapply.IsChecked;
            Settings.Default["AutoReapplyTime"] = (int)nudAutoReapply.Value;
            Settings.Default["StartMinimised"] = (bool)cbStartMinimised.IsChecked;
            Settings.Default["StartOnBoot"] = (bool)cbStartOnBoot.IsChecked;
            Settings.Default["SensorOverlay"] = (bool)cbSensorOverlay.IsChecked;
            Settings.Default.ApplyOCAtStart = (bool)cbApplyOC.IsChecked;
            Settings.Default.Save();

            if(cbStartOnBoot.IsChecked == true)
            {
                var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                key.SetValue("MyApplication", System.Reflection.Assembly.GetExecutingAssembly().Location.ToString());
            } 
            else
            {
                var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                key.DeleteValue("MyApplication", false);
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            cbAutoReapply.IsChecked = Convert.ToBoolean(Settings.Default["AutoReapply"]);
            nudAutoReapply.Value = Convert.ToInt32(Settings.Default["AutoReapplyTime"]);
            cbStartMinimised.IsChecked = Convert.ToBoolean(Settings.Default["StartMinimised"]);
            cbStartOnBoot.IsChecked = Convert.ToBoolean(Settings.Default["StartOnBoot"]);
            cbSensorOverlay.IsChecked = Convert.ToBoolean(Settings.Default["SensorOverlay"]);
            cbApplyOC.IsChecked = Settings.Default.ApplyOCAtStart;
        }
    }
}
