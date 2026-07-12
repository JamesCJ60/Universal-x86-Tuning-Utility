using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = string.Empty;

        [ObservableProperty]
        private ObservableCollection<object> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        [ObservableProperty]
        private string _downloads = "Downloads: ";

        [ObservableProperty]
        private bool _isDownloads;

        private ICommand? _navigateCommand;

        public MainWindowViewModel(INavigationService navigationService)
        {
            InitializeViewModel();
            LocalizationService.CultureChanged += OnCultureChanged;
        }

        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

        private void InitializeViewModel()
        {
            ApplicationTitle = "Universal x86 Tuning Utility";

            NavigationItems = new ObservableCollection<object>
            {
                CreateNavigationItem("Home", "dashboard", SymbolRegular.Home24, typeof(Views.Pages.DashboardPage))
            };

            if (Family.TYPE != Family.ProcessorType.Intel)
                NavigationItems.Add(CreateNavigationItem("Premade", "premade", SymbolRegular.Predictions24, typeof(Views.Pages.Premade)));

            NavigationItems.Add(CreateNavigationItem("Custom", "custom", SymbolRegular.Book24, typeof(Views.Pages.CustomPresets)));
            NavigationItems.Add(CreateNavigationItem("Adaptive", "adaptive", SymbolRegular.Radar20, typeof(Views.Pages.Adaptive)));
            NavigationItems.Add(CreateNavigationItem("Games", "games", SymbolRegular.Games24, typeof(Views.Pages.Games)));
            NavigationItems.Add(CreateNavigationItem("Overlay", "overlay", SymbolRegular.DesktopPulse24, typeof(Views.Pages.OverlaySettingsPage)));
            NavigationItems.Add(CreateNavigationItem("Auto", "auto", SymbolRegular.Transmission24, typeof(Views.Pages.Automations)));
            NavigationItems.Add(CreateNavigationItem("Info", "info", SymbolRegular.Info24, typeof(Views.Pages.SystemInfo)));

            NavigationFooter = new ObservableCollection<object>
            {
                CreateNavigationItem("Settings", "settings", SymbolRegular.Settings24, typeof(Views.Pages.SettingsPage))
            };

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new() { Header = "Home", Tag = "tray_home" }
            };
        }

        private static NavigationViewItem CreateNavigationItem(string content, string tag, SymbolRegular icon, Type pageType) =>
            new(content, icon, pageType) { TargetPageTag = tag };

        private void OnCultureChanged(object? sender, EventArgs e)
        {
            foreach (var item in NavigationItems.Concat(NavigationFooter).OfType<NavigationViewItem>())
            {
                var symbol = item.TargetPageTag switch
                {
                    "dashboard" => SymbolRegular.Home24,
                    "premade" => SymbolRegular.Predictions24,
                    "custom" => SymbolRegular.Book24,
                    "adaptive" => SymbolRegular.Radar20,
                    "games" => SymbolRegular.Games24,
                    "overlay" => SymbolRegular.DesktopPulse24,
                    "auto" => SymbolRegular.Transmission24,
                    "info" => SymbolRegular.Info24,
                    "settings" => SymbolRegular.Settings24,
                    _ => SymbolRegular.Empty
                };

                if (symbol != SymbolRegular.Empty)
                {
                    item.Icon = new SymbolIcon { Symbol = symbol };
                }
            }
        }

        private void OnNavigate(string? parameter)
        {
            switch (parameter)
            {
                case "download":
                    OpenUrl("https://github.com/JamesCJ60/Universal-x86-Tuning-Utility/releases");
                    break;
                case "discord":
                    OpenUrl("http://www.discord.gg/3EkYMZGJwq");
                    break;
                case "support":
                    OpenUrl("https://www.paypal.com/paypalme/JamesCJ60");
                    OpenUrl("https://patreon.com/uxtusoftware");
                    break;
            }
        }

        private static void OpenUrl(string url) =>
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
