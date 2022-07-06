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
    public partial class ASUSAC : UserControl
    {
        string path = Settings.Default["Path"].ToString();
        string apuname = (string)Settings.Default["APUName"];
        string APUPresetToLoad = "";
        string preset1 = "";
        string preset2 = "";
        string preset3 = "";
        string preset4 = "";
        string ACImages = "";

        public ASUSAC()
        {
            InitializeComponent();
            try
            {
                ACImages = path + "\\images\\";


                GetModeImage(Settings.Default.ACMode.ToString());

            } catch (Exception ex)
            {
                MessageBox.Show("An error occured loading AC. Error: " + ex.Message);
            }

        }

        void GetModeImage(string mode)
        {
            if(File.Exists(ACImages + $"\\{mode}.png"))
            {
                imgMode.Source = new BitmapImage(new Uri(ACImages + $"\\{mode}.png"));
            }

            btnPreset1.FontWeight = FontWeights.Normal;
            btnPreset2.FontWeight = FontWeights.Normal;
            btnPreset3.FontWeight = FontWeights.Normal;
            btnPreset4.FontWeight = FontWeights.Normal;

            if (mode.Contains("Silent"))
            {
                btnPreset1.FontWeight = FontWeights.Bold;
            }
            if (mode.Contains("Bal"))
            {
                btnPreset2.FontWeight = FontWeights.Bold;
            }
            if (mode.Contains("Turbo"))
            {
                btnPreset3.FontWeight = FontWeights.Bold;
            }
            if (mode.Contains("Manual"))
            {
                btnPreset4.FontWeight = FontWeights.Bold;
            }
        }

        private void btnPreset1_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = "Silent";
            Settings.Default.Save();
            GetModeImage(Settings.Default.ACMode.ToString());
        }

        private void btnPreset2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = "Bal";
            Settings.Default.Save();
            GetModeImage(Settings.Default.ACMode.ToString());
        }

        private void btnPreset3_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ACMode = "Turbo";
            Settings.Default.Save();
            GetModeImage(Settings.Default.ACMode.ToString());
        }

        private void btnPreset4_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
