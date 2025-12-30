using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using RyzenSmu;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
//using Universal_x86_Tuning_Utility.Scripts.AMD_Backend;
using Universal_x86_Tuning_Utility.Scripts.ASUS;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;
using Universal_x86_Tuning_Utility.Views.Pages;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Universal_x86_Tuning_Utility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static IHost _host;

        public static ASUSWmi wmi;
        public static XgMobileConnectionService xgMobileConnectionService;
        private static ILogger<App>? _logger;
        //public static Mem_Timings memTimings = new Mem_Timings();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        public static string version = "26.0.0";
        private Mutex mutex;
        private const string MutexName = "UniversalX86TuningUtility";

        public static readonly string SCALE_MODELS_JSON_PATH = ".\\ScaleModels.json";
        public static readonly string LOGS_FOLDER = ".\\logs\\";

        public static string product = "";

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true)
                .WriteTo.File(LOGS_FOLDER + "uxtu_log.txt",
                    fileSizeLimitBytes: 8 * 1024 * 1024, // 8MB
                    rollingInterval: RollingInterval.Day
                )
                .CreateLogger();

            if (!IsVcRedistInstalled("x64") && !IsVcRedistInstalled("x86"))
            {
                var downloadVcRedist = MessageBox.Show("Microsoft Visual C++ Redistributable is not installed.\n" +
                                                       "Would you like to download it now?",
                    "Missing VC Redistributable",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (downloadVcRedist == MessageBoxResult.Yes)
                {
                    Process.Start(
                        new ProcessStartInfo("https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist")
                            { UseShellExecute = true });
                }

                Shutdown();
                return;
            }

            try
            {
                try
                {
                    await Task.Run(() => product = GetSystemInfo.Product);
                    Display.setUpLists();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Failed to setup product and display refresh rates");
                }

                _host = Host
                    .CreateDefaultBuilder()
                    .ConfigureAppConfiguration(c =>
                    {
                        c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // App Host
                        services.AddHostedService<ApplicationHostService>();

                        // Page resolver service
                        services.AddSingleton<IPageService, PageService>();

                        try
                        {
                            Settings.Default.isASUS = false;
                            Settings.Default.Save();

                            if (product.Contains("ROG") || product.Contains("TUF") || product.Contains("Ally") ||
                                product.Contains("Flow") || product.ToLower().Contains("vivobook") ||
                                product.ToLower().Contains("zenbook"))
                            {
                                wmi = new ASUSWmi();

                                services.AddSingleton(wmi);
                                services.AddSingleton<XgMobileConnectionService>();

                                Settings.Default.isASUS = true;
                                Settings.Default.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error(ex, "Failed to setup ASUS WMI services");
                        }

                        // Theme manipulation
                        services.AddSingleton<IThemeService, ThemeService>();

                        // TaskBar manipulation
                        services.AddSingleton<ITaskBarService, TaskBarService>();

                        // Service containing navigation, same as INavigationWindow... but without window
                        services.AddSingleton<INavigationService, NavigationService>();

                        // Main window with navigation
                        services.AddScoped<INavigationWindow, Views.Windows.MainWindow>();
                        services.AddScoped<ViewModels.MainWindowViewModel>();

                        // Views and ViewModels
                        services.AddScoped<Views.Pages.DashboardPage>();
                        services.AddScoped<ViewModels.DashboardViewModel>();
                        services.AddScoped<Views.Pages.CustomPresets>();
                        services.AddScoped<Views.Pages.Premade>();
                        services.AddScoped<Views.Pages.Adaptive>();
                        services.AddScoped<Views.Pages.Automations>();
                        services.AddScoped<Views.Pages.FanControl>();
                        services.AddScoped<Views.Pages.SystemInfo>();
                        services.AddScoped<ViewModels.GamesViewModel>();
                        services.AddScoped<Views.Pages.Games>();
                        services.AddScoped<ViewModels.CustomPresetsViewModel>();
                        services.AddScoped<Views.Pages.DataPage>();
                        services.AddScoped<ViewModels.DataViewModel>();
                        services.AddScoped<Views.Pages.SettingsPage>();
                        services.AddScoped<ViewModels.SettingsViewModel>();

                        // Configuration
                        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
                    }).Build();

                _logger = _host.Services.GetRequiredService<ILogger<App>>();

                _ = Tablet.TabletDevices;
                bool firstBoot = false;
                try
                {
                    if (Settings.Default.SettingsUpgradeRequired)
                    {
                        try
                        {
                            Settings.Default.Upgrade();
                            Settings.Default.SettingsUpgradeRequired = false;
                            Settings.Default.Save();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update settings on startup");
                        }
                    }

                    firstBoot = Settings.Default.FirstBoot;
                }
                catch (ConfigurationErrorsException ex)
                {
                    string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;
                    File.Delete(filename);
                    Settings.Default.Reload();
                }

                bool createdNew;
                mutex = new Mutex(true, MutexName, out createdNew);

                if (!createdNew)
                {
                    _logger.LogWarning("Failed to start app, as there is already running uxtu instance. Shutting down");
                    MessageBox.Show("An instance of Universal x86 Tuning Utility is already open!",
                        "Error starting Universal x86 Tuning Utility");
                    // Close the new instance
                    Shutdown();
                    return;
                }

                if (File.Exists("C:\\Universal.x86.Tuning.Utility.V2.msi"))
                    File.Delete("C:\\Universal.x86.Tuning.Utility.V2.msi");

                Family.setCpuFamily();

                string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                path = path.Replace("Universal x86 Tuning Utility.dll", null);

                if (firstBoot || Settings.Default.Path != path)
                {
                    Settings.Default.Path = path;
                    Settings.Default.FirstBoot = false;
                    Settings.Default.Save();

                    //PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFAUTONOMOUS", 1, true);
                    //PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFAUTONOMOUS", 1, false);
                    //PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP", 50, true);
                    //PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP", 50, false);
                    //PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", 50, true);
                    //PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", 50, false);

                    try
                    {
                        Task.Run(() => UnblockFilesInDirectory(path));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to unblock files in {dir} directory", path);
                    }
                }

                try
                {
                    if (IsInternetAvailable())
                        if (Settings.Default.UpdateCheck)
                            CheckForUpdate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to check for updates");
                }

                await _host.StartAsync();

                try
                {
                    await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync game library");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Failed to build and start a host");
                MessageBox.Show(ex.Message);
            }
        }

        public static async void CheckForUpdate()
        {
            var updateManager = new UpdateManager("JamesCJ60", "Universal-x86-Tuning-Utility", App.version, "C:\\");

            var isUpdateAvailable = await updateManager.IsUpdateAvailable();

            if (isUpdateAvailable)
            {
                if (updateManager._newVersion.StartsWith("3."))
                    ToastNotification.ShowToastNotification("Phantom Control Centre Now Available!",
                        $"Head to the Phantom Control Centre GitHub releases page to easily download the latest build!");
                else
                    ToastNotification.ShowToastNotification("New Update Available!",
                        $"Head to the settings menu to easily download the new Universal x86 Tuning Utility update!");
            }
        }

        public static bool IsInternetAvailable()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var result = ping.Send("8.8.8.8", 2000); // ping Google DNS server
                    return result.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool IsVcRedistInstalled(string arch)
        {
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(
                $@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\{arch}");
            if (key == null) return false;

            return key.GetValue("Installed") is int i && i == 1;
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            if (Family.TYPE != Family.ProcessorType.Intel) SMUCommands.RyzenAccess.Deinitialize();

            await _host.StopAsync();

            _host.Dispose();

            if (Games.CustomGameIconsDirectoryPath != null) //will be refactored in the future
            {
                Directory.Delete(Games.CustomGameIconsDirectoryPath, true);
            }
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
            _logger?.LogCritical(e.Exception, "Unhandled dispatcher exception");
        }

        static void UnblockFilesInDirectory(string directoryPath)
        {
            // Unblock files in the current directory
            Parallel.ForEach(Directory.GetFiles(directoryPath), filePath => { UnblockFile(filePath); });
        }

        static void UnblockFile(string filePath)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "powershell.exe";
            processInfo.Arguments = $"Unblock-File -Path \"{filePath}\"";
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;

            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo = processInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
    }
}