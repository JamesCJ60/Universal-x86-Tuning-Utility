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

namespace AATUV3.Pages
{
    /// <summary>
    /// Interaction logic for HomeMenu.xaml
    /// </summary>
    public partial class HomeMenu : UserControl
    {
        public HomeMenu()
        {
            InitializeComponent();
        }

        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            //Open discord invite link
            System.Diagnostics.Process.Start("https://discord.gg/ampBxnyKaz");
        }

        private void Reddit_Click(object sender, RoutedEventArgs e)
        {
            //Open browser to subreddit 
            System.Diagnostics.Process.Start("https://www.reddit.com/r/RyzenShine/");
        }

        private void SupportUs_Click(object sender, RoutedEventArgs e)
        {
            //Open browser to Patreon page  
            System.Diagnostics.Process.Start("https://www.patreon.com/aatusoftware");
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            //Open browser to RyzenAdj guide 
            System.Diagnostics.Process.Start("https://github.com/FlyGoat/RyzenAdj/wiki/Supported-Models");
        }

        private void Releases_Click(object sender, RoutedEventArgs e)
        {
            //Open browser to AATU releases page 
            System.Diagnostics.Process.Start("https://github.com/JamesCJ60/AMD-APU-Tuning-Utility/releases");
        }
    }
}
