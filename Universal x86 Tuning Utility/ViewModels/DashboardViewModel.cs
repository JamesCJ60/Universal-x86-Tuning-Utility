﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLib.Plugin.RiotGames.Model;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.UXTU_Super_Resolution;
using Windows.System;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int _counter = 0;


        [RelayCommand]
        private void OnCounterIncrement()
        {
            Counter++;
        }

        private readonly INavigationService _navigationService;


        private ICommand _navigateCommand;

        private ICommand _openWindowCommand;

        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

        public ICommand OpenWindowCommand => _openWindowCommand ??= new RelayCommand<string>(OnOpenWindow);

        public DispatcherTimer autoAdaptive = new DispatcherTimer();
        public DashboardViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            autoAdaptive.Interval = TimeSpan.FromSeconds(1);
            autoAdaptive.Tick += AutoAdaptive_Tick;
            autoAdaptive.Start();
        }

        public void AutoAdaptive_Tick(object sender, EventArgs e)
        {
           if(Settings.Default.isStartAdpative) _navigationService.Navigate(typeof(Views.Pages.Adaptive));
            autoAdaptive.Stop();
        }

        public void OnNavigatedTo()
        {
            System.Diagnostics.Debug.WriteLine($"INFO | {typeof(DashboardViewModel)} navigated", "Wpf.Ui.Demo");
        }

        public void OnNavigatedFrom()
        {
            System.Diagnostics.Debug.WriteLine($"INFO | {typeof(DashboardViewModel)} navigated", "Wpf.Ui.Demo");
        }

        private void OnNavigate(string parameter)
        {
            switch (parameter)
            {
                case "premade":
                    _navigationService.Navigate(typeof(Views.Pages.Premade));
                    return;

                case "custom":
                    _navigationService.Navigate(typeof(Views.Pages.CustomPresets));
                    return;

                case "adaptive":
                    _navigationService.Navigate(typeof(Views.Pages.Adaptive));
                    return;

                case "auto":
                    _navigationService.Navigate(typeof(Views.Pages.Automations));
                    return;

                case "info":
                    _navigationService.Navigate(typeof(Views.Pages.SystemInfo));
                    return;

                case "help":
                    Process.Start(new ProcessStartInfo("http://www.discord.gg/3EkYMZGJwq") { UseShellExecute = true });
                    return;

                case "support":
                    Process.Start(new ProcessStartInfo("https://www.paypal.com/paypalme/JamesCJ60") { UseShellExecute = true });
                    Process.Start(new ProcessStartInfo("https://patreon.com/uxtusoftware") { UseShellExecute = true });
                    return;
                case "games":
                    _navigationService.Navigate(typeof(Views.Pages.Games));
                    return;
                default:
                    string[] parts = parameter.Split('-');
                    MessageBox.Show("Worked!");
                    if (!parts[0].Contains("Microsoft Store")) Game_Manager.LaunchApp(parts[2], parts[0], parts[1], parts[1]);
                    else Game_Manager.LaunchApp(parts[1], parts[0], parts[1], parts[1]);
                    return;
            }
        }


        private void OnOpenWindow(string parameter)
        {
            switch (parameter)
            {
                //case "open_window_store":
                //    _testWindowService.Show<Views.Windows.StoreWindow>();
                //    return;

                //case "open_window_manager":
                //    _testWindowService.Show<Views.Windows.TaskManagerWindow>();
                //    return;

                //case "open_window_editor":
                //    _testWindowService.Show<Views.Windows.EditorWindow>();
                //    return;

                //case "open_window_settings":
                //    _testWindowService.Show<Views.Windows.SettingsWindow>();
                //    return;

                //case "open_window_experimental":
                //    _testWindowService.Show<Views.Windows.ExperimentalWindow>();
                //    return;
            }
        }
    }
}
