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

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class FSR : UserControl
    {
        public FSR()
        {
            InitializeComponent();

            try
            {
                int i = 0;

                foreach (var process in Process.GetProcessesByName("magpie")) i++;
                foreach (var process in Process.GetProcessesByName("Magpie")) i++;

                if (Settings.Default.isMagpie == false || i <= 0)
                {
                    MessageBox.Show("You must enable Magpie FSR in the settings menu to use Magpie FSR.", "Magpie is not open!");
                }

                if (File.Exists(Settings.Default.Path + "\\bin\\magpie\\config.mp"))
                {

                    var lines = File.ReadAllLines(Settings.Default.Path + "\\bin\\magpie\\config.mp");

                    int captureMode = Convert.ToInt32(lines[1]);
                    bool vsync = Convert.ToBoolean(lines[4]);
                    cbVsync.IsChecked = vsync;
                    bool dGame = Convert.ToBoolean(lines[7]);
                    cb3DGame.IsChecked = dGame;
                    int interpolationMode = Convert.ToInt32(lines[10]);
                    double sharpness = Convert.ToDouble(lines[13]);

                    nudSharpness.Value = Convert.ToInt32(sharpness * 100);

                    if (captureMode == 0) rbGC.IsChecked = true;
                    else rbDC.IsChecked = true;

                    if (interpolationMode == 0) rbN.IsChecked = true;
                    else rbB.IsChecked = true;
                }
            }catch
            {

            }
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                string path = Settings.Default.Path + "\\bin\\magpie\\config.mp";
                int captureMode = 0;
                int interpolationMode = 0;

                if (rbGC.IsChecked == true) captureMode = 0;
                else captureMode = 1;

                if (rbN.IsChecked == true) interpolationMode = 0;
                else interpolationMode = 1;

                double sharpness = (double)nudSharpness.Value / 100;

                string finalOutput = $"[Capture Mode]\n{captureMode.ToString()}\n\n[VSync]\n{cbVsync.IsChecked.ToString()}\n\n[Is3DMode]\n{cb3DGame.IsChecked.ToString()}\n\n[Interpolation]\n{interpolationMode.ToString()}\n\n[FSR Sharpness]\n{sharpness.ToString()}";
                File.WriteAllText(path, finalOutput);

                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Saved! FSR-settings-have-been-saved-successfully.", false);
            }
            catch
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error There-was-an-error-while-trying-to-save-settings", false);
            }
        }
    }
}
