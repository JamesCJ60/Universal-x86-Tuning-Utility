using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for Automations.xaml
    /// </summary>
    public partial class Games : Page
    {
        public Games()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;
            setUp();
            Garbage.Garbage_Collect();
        }

        public async void setUp()
        {
            await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library());

            lbGames.ItemsSource = Game_Manager.installedGames;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
