using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Services;
using Wpf.Ui.Controls;

namespace Universal_x86_Tuning_Utility.Views.Windows
{
    public partial class InstallpawnIo : FluentWindow
    {
        private const string PawnIoInstallerUrl = "https://github.com/namazso/PawnIO.Setup/releases/latest/download/PawnIO_setup.exe";

        private readonly string _installerPath = Path.Combine(
            Path.GetTempPath(),
            "Universal_x86_Tuning_Utility",
            "PawnIO_setup.exe");

        private readonly DispatcherTimer _progressBarTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        private bool _installFinished;

        public InstallpawnIo()
        {
            InitializeComponent();

            ToggleButton.Content = LocalizationService.Get("Install Driver");
            CloseButton.Content = LocalizationService.Get("Cancel");

            tbxInfo.Text = LocalizationService.Get(
            "This application requires the PawnIO driver to access supported low-level hardware features.\n\n" +
            "The driver will be downloaded and installed silently in the background.\n\n" +
            "PawnIO installer notice:\n\n" +
            "PawnIO is provided 'as is' without warranty of any kind, either express or implied. Use at your own risk. " +
            "The authors are not liable for any damages arising from the use of this software.\n\n" +
            "All rights reserved.\n" +
            "This installer can be redistributed unmodified.\n" +
            "Copyright © 2026 namazso <admin@namazso.eu>");
        }

        protected override void OnClosed(EventArgs e)
        {
            _progressBarTimer.Stop();
            _progressBarTimer.Tick -= ProgressBarTimer_Tick;

            base.OnClosed(e);
        }

        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            await InstallPawnIoAsync();
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task InstallPawnIoAsync()
        {
            try
            {
                ToggleButton.Visibility = Visibility.Collapsed;
                CloseButton.IsEnabled = false;

                pbStatus.Value = 0;
                pbStatus.Visibility = Visibility.Visible;

                _installFinished = false;
                _progressBarTimer.Tick -= ProgressBarTimer_Tick;
                _progressBarTimer.Tick += ProgressBarTimer_Tick;
                _progressBarTimer.Start();

                if (!IsRunningAsAdministrator())
                {
                    tbxInfo.Text = LocalizationService.Get(
                        "Administrator permission is required to install the PawnIO driver.\n\n" +
                        "Please approve the Windows security prompt to continue.");
                }
                else
                {
                    tbxInfo.Text = LocalizationService.Get("Downloading the PawnIO driver installer...");
                }

                await DownloadPawnIoInstallerAsync();

                tbxInfo.Text = LocalizationService.Get("Installing the PawnIO driver silently. Please wait...");

                int exitCode = await RunPawnIoInstallerSilentAsync();

                _installFinished = true;

                if (exitCode == 0)
                {
                    FinishProgress(LocalizationService.Get(
                        "PawnIO driver installation completed successfully.\n\n" +
                        "You may need to restart the application, or Windows, before all hardware sensors and controls become available."));
                }
                else
                {
                    FinishProgress(LocalizationService.Format(
                        "PawnIO installer exited with code {0}.\n\nThe driver may not have installed correctly. Please try again, or run the installer manually as administrator.", exitCode));
                }
            }
            catch (Exception ex)
            {
                _installFinished = true;

                FinishProgress(LocalizationService.Format("PawnIO driver installation failed.\n\n{0}", ex.Message));
            }
            finally
            {
                DeletePawnIoInstaller();
            }
        }

        private async Task DownloadPawnIoInstallerAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_installerPath)!);

            using HttpClient client = new()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            using HttpResponseMessage response = await client.GetAsync(PawnIoInstallerUrl);
            response.EnsureSuccessStatusCode();

            await using FileStream fileStream = new(
                _installerPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

            await response.Content.CopyToAsync(fileStream);
        }

        private void DeletePawnIoInstaller()
        {
            try
            {
                if (File.Exists(_installerPath))
                {
                    File.Delete(_installerPath);
                }

                string? installerDirectory = Path.GetDirectoryName(_installerPath);

                if (!string.IsNullOrWhiteSpace(installerDirectory) &&
                    Directory.Exists(installerDirectory) &&
                    Directory.GetFiles(installerDirectory).Length == 0 &&
                    Directory.GetDirectories(installerDirectory).Length == 0)
                {
                    Directory.Delete(installerDirectory);
                }
            }
            catch
            {

            }
        }

        private Task<int> RunPawnIoInstallerSilentAsync()
        {
            TaskCompletionSource<int> taskCompletionSource = new();

            ProcessStartInfo startInfo = new()
            {
                FileName = _installerPath,
                Arguments = "-install -silent",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process process = new()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (_, _) =>
            {
                try
                {
                    taskCompletionSource.TrySetResult(process.ExitCode);
                }
                finally
                {
                    process.Dispose();
                }
            };

            try
            {
                process.Start();
            }
            catch
            {
                process.Dispose();
                throw;
            }

            return taskCompletionSource.Task;
        }

        private void ProgressBarTimer_Tick(object? sender, EventArgs e)
        {
            if (!_installFinished)
            {
                pbStatus.Value = pbStatus.Value < 88
                    ? pbStatus.Value + 1
                    : 88;

                return;
            }

            pbStatus.Value += 5;

            if (pbStatus.Value >= 100)
            {
                pbStatus.Value = 100;
                _progressBarTimer.Stop();
            }
        }

        private void FinishProgress(string message)
        {
            Dispatcher.Invoke(() =>
            {
                pbStatus.Value = 100;
                _progressBarTimer.Stop();

                tbxInfo.Text = message;

                CloseButton.Content = LocalizationService.Get("Close");
                CloseButton.IsEnabled = true;
            });
        }

        private static bool IsRunningAsAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
