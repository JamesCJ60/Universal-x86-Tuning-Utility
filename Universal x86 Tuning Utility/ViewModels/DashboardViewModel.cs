using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class DashboardViewModel :
        ObservableObject,
        INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly DispatcherTimer _autoAdaptiveTimer;

        public DashboardViewModel(
            INavigationService navigationService)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(
                    nameof(navigationService));

            _autoAdaptiveTimer = new DispatcherTimer(
                DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _autoAdaptiveTimer.Tick += AutoAdaptiveTimer_Tick;
        }

        [RelayCommand]
        private void Navigate(string? destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                Debug.WriteLine(
                    "Dashboard navigation was requested without a destination.");

                return;
            }

            switch (destination)
            {
                case "premade":
                case "custom":
                case "adaptive":
                case "games":
                case "auto":
                case "info":
                    NavigateWithinApplication(destination);
                    break;

                case "help":
                    OpenUrl("https://discord.gg/3EkYMZGJwq");
                    break;

                case "support":
                    OpenUrl(
                        "https://www.paypal.com/paypalme/JamesCJ60");

                    OpenUrl(
                        "https://patreon.com/uxtusoftware");
                    break;

                default:
                    Debug.WriteLine(
                        $"Unknown dashboard destination: {destination}");

                    break;
            }
        }

        public Task OnNavigatedToAsync()
        {
            Debug.WriteLine(
                $"INFO | {nameof(DashboardViewModel)} navigated to.");

            _autoAdaptiveTimer.Stop();

            if (Settings.Default.isStartAdpative)
            {
                _autoAdaptiveTimer.Start();
            }

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            Debug.WriteLine(
                $"INFO | {nameof(DashboardViewModel)} navigated from.");

            _autoAdaptiveTimer.Stop();

            return Task.CompletedTask;
        }

        private void AutoAdaptiveTimer_Tick(
            object? sender,
            EventArgs e)
        {
            _autoAdaptiveTimer.Stop();

            if (Settings.Default.isStartAdpative)
            {
                NavigateWithinApplication("adaptive");
            }
        }

        private void NavigateWithinApplication(
            string targetPageTag)
        {
            bool succeeded =
                _navigationService.Navigate(targetPageTag);

            if (!succeeded)
            {
                Debug.WriteLine(
                    $"Dashboard navigation failed for tag " +
                    $"'{targetPageTag}'. Ensure the matching " +
                    $"NavigationViewItem has that TargetPageTag.");
            }
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"Could not open '{url}': {exception}");
            }
        }
    }
}
