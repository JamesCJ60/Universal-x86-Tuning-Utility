using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class GamesViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;


        private ICommand _navigateCommand;
        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

        public DispatcherTimer autoAdaptive = new DispatcherTimer();
        public GamesViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            autoAdaptive.Interval = TimeSpan.FromSeconds(1);
            autoAdaptive.Tick += AutoAdaptive_Tick;
            autoAdaptive.Start();
        }

        public void AutoAdaptive_Tick(object sender, EventArgs e)
        {
            if (Settings.Default.isStartAdpative) _navigationService.Navigate(typeof(Views.Pages.Adaptive));
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
            string[] parts = parameter.Split('-');
            MessageBox.Show("Worked!");
            if (!parts[0].Contains("Microsoft Store")) Game_Manager.LaunchApp(parts[2], parts[0], parts[1], parts[1]);
            else Game_Manager.LaunchApp(parts[1], parts[0], parts[1], parts[1]);
        }
    }
}
