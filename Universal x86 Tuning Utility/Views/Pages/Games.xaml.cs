using Octokit;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui.Common.Interfaces;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;

namespace Universal_x86_Tuning_Utility.Views.Pages
{
    /// <summary>
    /// Interaction logic for Automations.xaml
    /// </summary>
    public partial class Games : INavigableView<ViewModels.GamesViewModel>
    {
        public ViewModels.GamesViewModel ViewModel
        {
            get;
        }

        public class GameLauncherItem
        {
            public string gameID { get; set; }
            public string gameName { get; set; }
            public string appType { get; set; }
            public string launchCommand { get; set; }
            public string path { get; set; }
            public string exe { get; set; }
            public string imageLocation { get; set; } = "";
            public string iconPath { get; set; } = "";
        }

        public static List<GameLauncherItem> GameList = null;
        public Games(ViewModels.GamesViewModel viewModel)
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;
            ViewModel = viewModel;
            setUp();
            Garbage.Garbage_Collect();
        }

        public async void setUp()
        {
            lbGames.ItemsSource = null;
            ccLoading.Visibility = Visibility.Visible;
            await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library());
            GameList = new List<GameLauncherItem>();
            await Task.WhenAll(Game_Manager.installedGames.Select(async game =>
            {
                GameLauncherItem launcherItem = new GameLauncherItem
                {
                    gameName = game.gameName,
                    appType = game.appType,
                    path = game.path,
                    launchCommand = game.launchCommand,
                    exe = game.exe,
                    gameID = game.gameID,
                    iconPath = await GetImages.GetIconImageUrl(game.gameName)
                };

                lock (GameList)
                {
                    GameList.Add(launcherItem);
                }
            }));

            GameList = GameList.OrderBy(item => item.gameName).ToList();
            GameList = GameList.Distinct().ToList();
            ccLoading.Visibility = Visibility.Collapsed;
            lbGames.ItemsSource = GameList;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            setUp();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                if (button.Tag is string parameter)
                {
                    string[] parts = parameter.Split('-');
                    if (!parts[0].Contains("Microsoft Store"))
                    {
                        Game_Manager.LaunchApp(parts[2], parts[0], parts[1], parts[1]);
                        ToastNotification.ShowToastNotification($"Launching {parts[3]}", $"This should only take a few moments!");
                    }

                    else
                    {
                        Game_Manager.LaunchApp(parts[1], parts[0], parts[1], parts[1]);
                        ToastNotification.ShowToastNotification($"Launching {parts[2]}", $"This should only take a few moments!");
                    }
                }
            }
        }
    }
}
