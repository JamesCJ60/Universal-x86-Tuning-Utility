using UXTU.Properties;
using AATUV3.Scripts;
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

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class ProjectSnowdrop : UserControl
    {
        string path = Settings.Default["Path"].ToString();
        string apuname = (string)Settings.Default["APUName"];
        string APUPresetToLoad = "";
        string preset1 = "";
        string preset2 = "";
        string preset3 = "";
        string preset4 = "";

        public ProjectSnowdrop()
        {
            InitializeComponent();
            try
            {
                string apuPreset = path + "\\project-snowdrop\\APU";
                string devicePath = path + "\\project-snowdrop\\Device";

                if (File.Exists(devicePath + "\\config.txt"))
                {
                    imgDevice.Source = new BitmapImage(new Uri(devicePath + "\\config.png"));

                    var lines = File.ReadAllLines(devicePath + $"\\config.txt");

                    lblPresetName.Text = lines[1];
                    lblCPU.Text = lines[4];
                    lblGPU.Text = lines[5];
                    lblRAM.Text = lines[6];
                    lblStore.Text = lines[7];
                    lblDisplay.Text = lines[8];

                    if (lines[11].Contains("N/A"))
                    {
                        btnPreset1.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnPreset1.Content = lines[11];
                        preset1 = lines[12];
                    }
                    if (lines[15].Contains("N/A"))
                    {
                        btnPreset2.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnPreset2.Content = lines[15];
                        preset2 = lines[16];

                    }
                    if (lines[19].Contains("N/A"))
                    {
                        btnPreset3.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnPreset3.Content = lines[19];
                        preset3 = lines[20];
                    }
                    if (lines[23].Contains("N/A"))
                    {
                        btnPreset4.Visibility = Visibility.Collapsed;
                    }
                    else 
                    {
                        btnPreset4.Content = lines[23];
                        preset4 = lines[24];
                    }

                    lblPresetsHeader.Text = "Device Specific Power Presets:";
                }
                else
                {
                    imgDevice.Source = new BitmapImage(new Uri(apuPreset + "\\config.png"));

                    lblPresetName.Text = "APU Preset: " + apuname;
                    lblCPU.Text = "CPU: " + apuname;
                    lblGPU.Text = "GPU: Radeon™ Graphics";
                    lblRAM.Text = "RAM: N/A";
                    lblStore.Text = "Storage: N/A";
                    lblDisplay.Text = "Display: N/A";

                    if (apuname.Contains("U"))
                    {
                        APUPresetToLoad = "U";
                    }
                    if (apuname.Contains("HX"))
                    {
                        APUPresetToLoad = "HX";
                    }
                    if (apuname.Contains("H") || apuname.Contains("Z"))
                    {
                        APUPresetToLoad = "H-HS";
                    }

                    var lines = File.ReadAllLines(apuPreset + $"\\{APUPresetToLoad}\\config.txt");

                    if (lines[11].Contains("N/A"))
                    {
                        btnPreset1.Content = "No Preset Available";
                    }
                    else
                    {
                        btnPreset1.Content = lines[11];
                        preset1 = lines[12];
                    }
                    if (lines[15].Contains("N/A"))
                    {
                        btnPreset2.Content = "No Preset Available";
                    }
                    else
                    {
                        btnPreset2.Content = lines[15];
                        preset2 = lines[16];

                    }
                    if (lines[19].Contains("N/A"))
                    {
                        btnPreset3.Content = "No Preset Available";
                    }
                    else
                    {
                        btnPreset3.Content = lines[19];
                        preset3 = lines[20];
                    }
                    if (lines[23].Contains("N/A"))
                    {
                        btnPreset4.Content = "Find Device Presets";
                    }
                    else 
                    {
                        btnPreset4.Content = lines[23];
                        preset4 = lines[24];
                    }

                    lblPresetsHeader.Text = "APU Specific Power Presets:";
                }

            } catch (Exception ex)
            {
                MessageBox.Show("An error occured loading presets. Error: " + ex.Message);
            }
        }

        private void btnPreset1_Click(object sender, RoutedEventArgs e)
        {
            if (preset1 == null || preset1 == "")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }

            else
            {
                //Get RyzenAdj path
                string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(pathRyzenAdj, preset1, true);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default["RyzenAdjArguments"] = preset1;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }

        private void btnPreset2_Click(object sender, RoutedEventArgs e)
        {
            if (preset2 == null || preset2 == "")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }

            else
            {
                //Get RyzenAdj path
                string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(pathRyzenAdj, preset2, true);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default["RyzenAdjArguments"] = preset2;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }

        private void btnPreset3_Click(object sender, RoutedEventArgs e)
        {
            if (preset3 == null || preset3 == "")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }

            else
            {
                //Get RyzenAdj path
                string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(pathRyzenAdj, preset3, true);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default["RyzenAdjArguments"] = preset3;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }

        private void btnPreset4_Click(object sender, RoutedEventArgs e)
        {
            if (preset4 == null || preset4 == "" && btnPreset4.Content != "Find Device Presets")
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }
            if (btnPreset4.Content == "Find Device Presets")
            {
                System.Diagnostics.Process.Start("https://www.dropbox.com/sh/5yll57epokkcqgs/AABVsb_cjkdmMoss5JsMK4fia?dl=0");
            }

            else
            {
                //Get RyzenAdj path
                string pathRyzenAdj = "\\bin\\ryzenadj\\ryzenadj.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(pathRyzenAdj, preset4, true);
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
                Settings.Default["RyzenAdjArguments"] = preset4;
                Settings.Default.Save();
                //System.Windows.MessageBox.Show(ryzenadj);
            }
        }
    }
}
