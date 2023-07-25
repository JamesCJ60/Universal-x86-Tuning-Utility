using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

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

        public DashboardViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
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
