using GameLib.Plugin.RiotGames.Model;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
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
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Universal_x86_Tuning_Utility.Views.Windows;
using Windows.Gaming.Preview.GamesEnumeration;
using Microsoft.Extensions.Logging;
using Wpf.Ui.Common.Interfaces;
using YamlDotNet.Core;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

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

        public class GameLauncherItem : INotifyPropertyChanged
        {
            private string _fpsData;
            private string _msData;

            public string gameID { get; set; }
            public string gameName { get; set; }
            public string appType { get; set; }
            public string launchCommand { get; set; }
            public string path { get; set; }
            public string exe { get; set; }
            public string imageLocation { get; set; } = "";

            public string fpsData
            {
                get { return _fpsData; }
                set
                {
                    if (_fpsData != value)
                    {
                        _fpsData = value;
                        OnPropertyChanged(nameof(fpsData));
                    }
                }
            }

            public string msData
            {
                get { return _msData; }
                set
                {
                    if (_msData != value)
                    {
                        _msData = value;
                        OnPropertyChanged(nameof(msData));
                    }
                }
            }

            public string iconPath { get; set; } = "";

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly ILogger<Games> _logger;

        public static List<GameLauncherItem> GameList = null;
        DispatcherTimer updateFPS = new DispatcherTimer();
        public Games(ViewModels.GamesViewModel viewModel, ILogger<Games> logger)
        {
            _logger = logger;
            InitializeComponent();
            _ = Tablet.TabletDevices;
            ViewModel = viewModel;
            setUp();
            Garbage.Garbage_Collect();
            updateFPS.Interval = TimeSpan.FromSeconds(2);
            updateFPS.Tick += UpdateFPS_Tick;
            updateFPS.Start();
        }

        private void UpdateFPS_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!MainWindow.isMini && Family.TYPE == Family.ProcessorType.Intel && MainWindow._mainWindowNav.SelectedPageIndex == 3 || Family.TYPE != Family.ProcessorType.Intel && MainWindow._mainWindowNav.SelectedPageIndex == 4)
                {
                    gameDataManager = new GameDataManager(Settings.Default.Path + "gameData.json");
                    IEnumerable<string> presetNames = gameDataManager.GetPresetNames();
                    foreach (string name in presetNames)
                    {
                        GameLauncherItem itemToUpdate = GameList.FirstOrDefault(item => item.gameName == name);
                        if (itemToUpdate != null)
                        {
                            GameData gameData = gameDataManager.GetPreset(name);
                            string fps = "", ms = "";
                            if (gameData?.fpsData != "No Data") fps = $"{gameData?.fpsData} FPS";
                            else fps = $"{gameData?.fpsData}";
                            itemToUpdate.fpsData = fps;
                            if (gameData?.msData != "No Data") ms = $"{gameData?.msData} ms";
                            else ms = $"{gameData?.msData}";
                            itemToUpdate.msData = ms;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update fps");
            }
        }

        private static GameDataManager gameDataManager = new GameDataManager(Settings.Default.Path + "gameData.json");
        public async void setUp()
        {
            lbGames.ItemsSource = null;
            ccLoading.Visibility = Visibility.Visible;
            await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library());
            GameList = new List<GameLauncherItem>();

            await Task.WhenAll(Game_Manager.installedGames.Select(async game =>
            {
                IEnumerable<string> presetNames = gameDataManager.GetPresetNames();

                bool containsName = false;

                foreach (string names in presetNames)
                {
                    if (names.Contains(game.gameName)) containsName = true;
                }

                if (containsName == false)
                {
                    GameData preset = new GameData
                    {
                        fpsData = "No Data"
                    };
                    gameDataManager.SavePreset(game.gameName, preset);
                }

                gameDataManager = new GameDataManager(Settings.Default.Path + "gameData.json");
                GameData gameData = gameDataManager.GetPreset(game.gameName);
                string fps = "";
                if (gameData?.fpsData != "No Data") fps = $"{gameData?.fpsData} FPS";
                else fps = $"{gameData?.fpsData}";

                GameLauncherItem launcherItem = new GameLauncherItem
                {
                    gameName = game.gameName,
                    appType = game.appType,
                    path = game.path,
                    launchCommand = game.launchCommand,
                    exe = game.exe,
                    gameID = game.gameID,
                    fpsData = fps,
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
