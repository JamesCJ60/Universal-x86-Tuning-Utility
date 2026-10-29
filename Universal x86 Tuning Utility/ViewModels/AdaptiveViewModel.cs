using GameLib.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Services;
using static Universal_x86_Tuning_Utility.Scripts.Game_Manager;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class AdaptiveViewModel : PageViewModel, IAdaptiveControlState
    {
        [ObservableProperty]
        private ObservableCollection<string> _powerPresetOptions = new();

        [ObservableProperty]
        private int _powerPresetIndex = -1;

        [ObservableProperty]
        private string _selectedPresetName = "";

        [ObservableProperty]
        private double _polling = 2;

        [ObservableProperty]
        private bool _isAutoSwitchEnabled = false;

        [ObservableProperty]
        private bool _canSave = false;

        [ObservableProperty]
        private bool _canStart = false;

        [ObservableProperty]
        private Wpf.Ui.Controls.SymbolRegular _startIconSymbol = Wpf.Ui.Controls.SymbolRegular.Play20;

        [ObservableProperty]
        private string _startTextText = "Start Adaptive Mode";

        [ObservableProperty]
        private bool _isPresetAutoSwitchEnabled = false;

        [ObservableProperty]
        private double _temp = 10;

        [ObservableProperty]
        private double _powerLimit = 8;

        [ObservableProperty]
        private Visibility _cOVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isCurveEnabled = false;

        [ObservableProperty]
        private double _curve = 0;

        [ObservableProperty]
        private Visibility _tBOiGPUVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isTBOiGPUEnabled = false;

        [ObservableProperty]
        private double _maxGfxClk = 200;

        [ObservableProperty]
        private double _minGfxClk = 200;

        [ObservableProperty]
        private double _minCpuClk = 1000;

        [ObservableProperty]
        private bool _isUXTUSREnabled = false;

        [ObservableProperty]
        private bool _isVSyncEnabled = false;

        [ObservableProperty]
        private bool _isAutoCapEnabled = false;

        [ObservableProperty]
        private double _sharp = 5;

        [ObservableProperty]
        private int _resScaleIndex = 0;

        [ObservableProperty]
        private Visibility _aDLXVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isRadeonGraphEnabled = false;

        [ObservableProperty]
        private bool _isAntiLagEnabled = false;

        [ObservableProperty]
        private bool _isRSREnabled = false;

        [ObservableProperty]
        private double _rSR = 100;

        [ObservableProperty]
        private bool _isBoostEnabled = false;

        [ObservableProperty]
        private double _boost = 50;

        [ObservableProperty]
        private bool _isImageSharpEnabled = false;

        [ObservableProperty]
        private double _imageSharp = 10;

        [ObservableProperty]
        private bool _isSyncEnabled = false;

        [ObservableProperty]
        private Visibility _nVIDIAVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool _isNVEnabled = false;

        [ObservableProperty]
        private double _nVMaxCore = 400;

        [ObservableProperty]
        private double _nVCore = 0;

        [ObservableProperty]
        private double _nVMem = 0;

        [ObservableProperty]
        private Visibility _asusPowerVisibility = Visibility.Visible;

        [ObservableProperty]
        private int _asusPowerIndex = 0;

        [ObservableProperty]
        private bool _isRTSSEnabled = false;

        [ObservableProperty]
        private double _frameRateLimit = 0;

        [ObservableProperty] private int _windowsBoostModeIndex = 0;
        [ObservableProperty] private bool _isWindowsMinStateEnabled = false;
        [ObservableProperty] private double _windowsMinState = 5;
        [ObservableProperty] private bool _isWindowsMaxStateEnabled = false;
        [ObservableProperty] private double _windowsMaxState = 100;
        [ObservableProperty] private bool _isWindowsMaxFrequencyEnabled = false;
        [ObservableProperty] private double _windowsMaxFrequency = 5000;
        [ObservableProperty] private bool _isWindowsEppEnabled = false;
        [ObservableProperty] private double _windowsEpp = 50;
        [ObservableProperty] private bool _isWindowsCoreParkingEnabled = false;
        [ObservableProperty] private double _windowsCoreParking = 100;
        [ObservableProperty] private bool _isWindowsMaxUnparkedCoresEnabled = false;
        [ObservableProperty] private double _windowsMaxUnparkedCores = 100;
        private readonly IGraphicsHardwareService _graphics;
        private readonly IAdaptiveSensorService _sensorService;
        private readonly IAdaptiveControlService _control;
        private AdaptiveReadings _readings = new(0, 0, 0, 0, 0, 0, 0);
        private readonly IPresetApplicationService _application;
        private readonly IUserInteractionService _interaction;
        private Task _updateTask = Task.CompletedTask;
        private async void adaptive_Tick(object sender, EventArgs e)
        {
            if (!_updateTask.IsCompleted || IsDisposed) return;
            _updateTask = RunAdaptiveTickAsync();
            try { await _updateTask; }
            catch (Exception error) { Serilog.Log.Error(error, "Adaptive polling failed"); }
        }
        private Task _sensorTask = Task.CompletedTask;
        private async void sensors_Tick(object sender, EventArgs e)
        {
            if (!_sensorTask.IsCompleted || IsDisposed) return;
            _sensorTask = ReadSensorsAsync();
            try { await _sensorTask; }
            catch (Exception error) { Serilog.Log.Error(error, "Adaptive polling failed"); }
        }
        private bool _updating;
        public AdaptiveViewModel(IPresetApplicationService application, IUserInteractionService interaction, IGraphicsHardwareService graphics, IAdaptiveSensorService sensorService, IAdaptiveControlService control)
        {
            _application = application;
            _graphics = graphics;
            _sensorService = sensorService;
            _control = control;
            _interaction = interaction;
            OwnTimer(adaptiveMode);
            OwnTimer(sensors);
        }

        protected override async Task InitializeAsync()
        {


            await setUp();
            if (IsDisposed) return;

            adaptiveMode.Interval = TimeSpan.FromSeconds(2);
            adaptiveMode.Tick += new EventHandler(adaptive_Tick);
            adaptiveMode.Start();

            sensors.Interval = TimeSpan.FromSeconds(2);
            sensors.Tick += new EventHandler(sensors_Tick);
            sensors.Start();

            Polling = Math.Max(0.1, Settings.Default.polling);

            IsAutoSwitchEnabled = Settings.Default.autoSwitch;

            if (!Settings.Default.isASUS) AsusPowerVisibility = Visibility.Collapsed;

            await Task.CompletedTask;
        }

        System.Windows.Threading.DispatcherTimer adaptiveMode = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer sensors = new System.Windows.Threading.DispatcherTimer();

        private static AdaptivePresetManager adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
        private async Task setUp()
        {
            try
            {
                if (_graphics.CountRadeonGpus() <= 0)
                {
                    TBOiGPUVisibility = Visibility.Collapsed;
                    ADLXVisibility = Visibility.Collapsed;
                }

                if (_graphics.CountNvidiaGpus() < 1) NVIDIAVisibility = Visibility.Collapsed;

                if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu || Family.FAM == Family.RyzenFamily.DragonRange) PowerLimit = 86;
                else PowerLimit = 28;
                MaxGfxClk = 1900;
                MinGfxClk = 400;
                Temp = 95;
                MinCpuClk = 1500;
                NVMaxCore = 4000;
                IsPresetAutoSwitchEnabled = true;

                await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library(true));

                PowerPresetOptions.Add("Default");
                foreach (GameLauncherItem item in Game_Manager.installedGames) PowerPresetOptions.Add(item.gameName);

                SelectedPresetName = "Default";

                IEnumerable<string> presetNames = adaptivePresetManager.GetPresetNames();

                foreach (GameLauncherItem item in Game_Manager.installedGames)
                {
                    bool containsName = false;

                    foreach (string names in presetNames)
                    {
                        if (names.Contains(item.gameName)) containsName = true;
                    }

                    if (containsName == false)
                    {
                        AdaptivePreset preset = new AdaptivePreset
                        {
                            Temp = (int)Temp,
                            Power = (int)PowerLimit,
                            CO = (int)Curve,
                            minGFX = (int)MinGfxClk,
                            MaxGFX = (int)MaxGfxClk,
                            minCPU = (int)MinCpuClk,
                            isCO = (bool)IsCurveEnabled,
                            isGFX = (bool)IsTBOiGPUEnabled,
                            rsr = (int)RSR,
                            boost = (int)Boost,
                            imageSharp = (int)ImageSharp,
                            isRadeonGraphics = (bool)IsRadeonGraphEnabled,
                            isRSR = (bool)IsRSREnabled,
                            isBoost = (bool)IsBoostEnabled,
                            isAntiLag = (bool)IsAntiLagEnabled,
                            isImageSharp = (bool)IsImageSharpEnabled,
                            isSync = (bool)IsSyncEnabled,
                            isNVIDIA = (bool)IsNVEnabled,
                            nvMaxCoreClk = (int)NVMaxCore,
                            nvCoreClk = (int)NVCore,
                            nvMemClk = (int)NVMem,
                            asusPowerProfile = (int)AsusPowerIndex,
                    windowsBoostMode = WindowsBoostModeIndex,
                    isWindowsMinState = IsWindowsMinStateEnabled,
                    windowsMinState = (int)WindowsMinState,
                    isWindowsMaxState = IsWindowsMaxStateEnabled,
                    windowsMaxState = (int)WindowsMaxState,
                    isWindowsMaxFrequency = IsWindowsMaxFrequencyEnabled,
                    windowsMaxFrequency = (int)WindowsMaxFrequency,
                    isWindowsEpp = IsWindowsEppEnabled,
                    windowsEpp = (int)WindowsEpp,
                    isWindowsCoreParking = IsWindowsCoreParkingEnabled,
                    windowsCoreParking = (int)WindowsCoreParking,
                    isWindowsMaxUnparkedCores = IsWindowsMaxUnparkedCoresEnabled,
                    windowsMaxUnparkedCores = (int)WindowsMaxUnparkedCores,
                            isMag = (bool)IsUXTUSREnabled,
                            isVsync = (bool)IsVSyncEnabled,
                            isRecap = (bool)IsAutoCapEnabled,
                            Sharpness = (int)Sharp,
                            ResScaleIndex = (int)ResScaleIndex,
                            isAutoSwitch = (bool)IsPresetAutoSwitchEnabled
                        };
                        adaptivePresetManager.SavePreset(item.gameName, preset);
                    }

                    if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        COVisibility = Visibility.Collapsed;
                        TBOiGPUVisibility = Visibility.Collapsed;
                    }

                }


                CanStart = true;
                CanSave = true;

                if (Settings.Default.isStartAdpative) await ToggleAdaptiveMode();
            }
            catch (Exception ex) { Serilog.Log.Error(ex, "Operation failed in AdaptiveViewModel"); }
        }

        bool start = false;
        [RelayCommand]
        private async Task ToggleAdaptiveAsync()
        {
            await ToggleAdaptiveMode();
        }

        private async Task ToggleAdaptiveMode()
        {
            if (IsDisposed) return;
            try
            {
                if (start)
                {
                    start = false;
                    StartIconSymbol = Wpf.Ui.Controls.SymbolRegular.Play20;
                    StartTextText = "Start Adaptive Mode";
                    try { await Task.WhenAll(_sensorTask, _updateTask); }
                    finally { _sensorService.Close(); }
                    Settings.Default.isAdaptiveModeRunning = false;
                    Settings.Default.Save();

                }
                else
                {
                    start = false;
                    StartIconSymbol = Wpf.Ui.Controls.SymbolRegular.Stop20;
                    StartTextText = "Stop Adaptive Mode";
                    await _sensorService.OpenAsync();
                    _readings = await _sensorService.ReadAsync();
                    _control.Reset();
                    if (IsDisposed) { _sensorService.Close(); return; }
                    start = true;
                    Settings.Default.isAdaptiveModeRunning = true;
                    Settings.Default.Save();
                }
            }
            catch (Exception error)
            {
                start = false;
                _sensorService.Close();
                Settings.Default.isAdaptiveModeRunning = false;
                Settings.Default.Save();
                StartIconSymbol = Wpf.Ui.Controls.SymbolRegular.Play20;
                StartTextText = "Start Adaptive Mode";
                _interaction.ShowError(error.Message);
            }
        }


        private async Task RunAdaptiveTickAsync()
        {
            if (_updating || IsDisposed) return;
            _updating = true;
            try
            {
                if (start == true)
                {
                    await update();
                }
                if (IsDisposed) return;
                if (Settings.Default.polling != Polling)
                {
                    Settings.Default.polling = (double)Polling;
                    Settings.Default.Save();
                }

                if (adaptiveMode.Interval != TimeSpan.FromSeconds(Math.Max(0.1, Polling)))
                {
                    adaptiveMode.Stop();
                    adaptiveMode.Interval = TimeSpan.FromSeconds(Math.Max(0.1, Polling));
                    adaptiveMode.Start();
                }
                if (sensors.Interval != TimeSpan.FromSeconds(Math.Max(0.1, Polling)))
                {
                    sensors.Stop();
                    sensors.Interval = TimeSpan.FromSeconds(Math.Max(0.1, Polling));
                    sensors.Start();
                }
            }
            finally { _updating = false; }
        }


        private void loadPreset(string presetName)
        {
            try
            {
                adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
                AdaptivePreset myPreset = adaptivePresetManager.GetPreset(presetName);

                if (myPreset != null)
                {
                    IsPresetAutoSwitchEnabled = myPreset.isAutoSwitch;

                    Temp = myPreset.Temp;
                    PowerLimit = myPreset.Power;
                    Curve = myPreset.CO;
                    MaxGfxClk = myPreset.MaxGFX;
                    MinGfxClk = myPreset.minGFX;
                    MinCpuClk = myPreset.minCPU;

                    IsCurveEnabled = myPreset.isCO;
                    IsTBOiGPUEnabled = myPreset.isGFX;

                    IsRadeonGraphEnabled = myPreset.isRadeonGraphics;
                    IsAntiLagEnabled = myPreset.isAntiLag;
                    IsRSREnabled = myPreset.isRSR;
                    IsBoostEnabled = myPreset.isBoost;
                    IsImageSharpEnabled = myPreset.isImageSharp;
                    IsSyncEnabled = myPreset.isSync;
                    RSR = myPreset.rsr;
                    Boost = myPreset.boost;
                    ImageSharp = myPreset.imageSharp;

                    IsNVEnabled = myPreset.isNVIDIA;
                    NVMaxCore = myPreset.nvMaxCoreClk;
                    NVCore = myPreset.nvCoreClk;
                    NVMem = myPreset.nvMemClk;

                    AsusPowerIndex = myPreset.asusPowerProfile;
                    WindowsBoostModeIndex = myPreset.windowsBoostMode;
                    IsWindowsMinStateEnabled = myPreset.isWindowsMinState;
                    WindowsMinState = myPreset.windowsMinState;
                    IsWindowsMaxStateEnabled = myPreset.isWindowsMaxState;
                    WindowsMaxState = myPreset.windowsMaxState;
                    IsWindowsMaxFrequencyEnabled = myPreset.isWindowsMaxFrequency;
                    WindowsMaxFrequency = myPreset.windowsMaxFrequency;
                    IsWindowsEppEnabled = myPreset.isWindowsEpp;
                    WindowsEpp = myPreset.windowsEpp;
                    IsWindowsCoreParkingEnabled = myPreset.isWindowsCoreParking;
                    WindowsCoreParking = myPreset.windowsCoreParking;
                    IsWindowsMaxUnparkedCoresEnabled = myPreset.isWindowsMaxUnparkedCores;
                    WindowsMaxUnparkedCores = myPreset.windowsMaxUnparkedCores;

                    IsUXTUSREnabled = myPreset.isMag;
                    IsVSyncEnabled = myPreset.isVsync;
                    IsAutoCapEnabled = myPreset.isRecap;
                    Sharp = myPreset.Sharpness;
                    ResScaleIndex = myPreset.ResScaleIndex;
                }
            }
            catch (Exception ex) { Serilog.Log.Error(ex, "Operation failed in AdaptiveViewModel"); }
        }

        private void savePreset(string presetName)
        {
            try
            {
                AdaptivePreset preset = new AdaptivePreset
                {
                    Temp = (int)Temp,
                    Power = (int)PowerLimit,
                    CO = (int)Curve,
                    minGFX = (int)MinGfxClk,
                    MaxGFX = (int)MaxGfxClk,
                    minCPU = (int)MinCpuClk,
                    isCO = (bool)IsCurveEnabled,
                    isGFX = (bool)IsTBOiGPUEnabled,
                    rsr = (int)RSR,
                    boost = (int)Boost,
                    imageSharp = (int)ImageSharp,
                    isRadeonGraphics = (bool)IsRadeonGraphEnabled,
                    isRSR = (bool)IsRSREnabled,
                    isBoost = (bool)IsBoostEnabled,
                    isAntiLag = (bool)IsAntiLagEnabled,
                    isImageSharp = (bool)IsImageSharpEnabled,
                    isSync = (bool)IsSyncEnabled,
                    isNVIDIA = (bool)IsNVEnabled,
                    nvMaxCoreClk = (int)NVMaxCore,
                    nvCoreClk = (int)NVCore,
                    nvMemClk = (int)NVMem,
                    asusPowerProfile = (int)AsusPowerIndex,
                    windowsBoostMode = WindowsBoostModeIndex,
                    isWindowsMinState = IsWindowsMinStateEnabled,
                    windowsMinState = (int)WindowsMinState,
                    isWindowsMaxState = IsWindowsMaxStateEnabled,
                    windowsMaxState = (int)WindowsMaxState,
                    isWindowsMaxFrequency = IsWindowsMaxFrequencyEnabled,
                    windowsMaxFrequency = (int)WindowsMaxFrequency,
                    isWindowsEpp = IsWindowsEppEnabled,
                    windowsEpp = (int)WindowsEpp,
                    isWindowsCoreParking = IsWindowsCoreParkingEnabled,
                    windowsCoreParking = (int)WindowsCoreParking,
                    isWindowsMaxUnparkedCores = IsWindowsMaxUnparkedCoresEnabled,
                    windowsMaxUnparkedCores = (int)WindowsMaxUnparkedCores,
                    isMag = (bool)IsUXTUSREnabled,
                    isVsync = (bool)IsVSyncEnabled,
                    isRecap = (bool)IsAutoCapEnabled,
                    Sharpness = (int)Sharp,
                    ResScaleIndex = (int)ResScaleIndex,
                    isAutoSwitch = (bool)IsPresetAutoSwitchEnabled
                };
                adaptivePresetManager.SavePreset(presetName, preset);
            }
            catch (Exception ex) { Serilog.Log.Error(ex, "Operation failed in AdaptiveViewModel"); }
        }


        [RelayCommand]
        private async Task ReloadGamesAsync()
        {
            try
            {

                await Task.Run(() => Game_Manager.installedGames = Game_Manager.syncGame_Library(true));
                PowerPresetOptions.Clear();
                PowerPresetOptions.Add("Default");
                foreach (GameLauncherItem item in Game_Manager.installedGames) PowerPresetOptions.Add(item.gameName);
                SelectedPresetName = "Default";
            }
            catch (Exception ex) { Serilog.Log.Error(ex, "Operation failed in AdaptiveViewModel"); }
        }

        [RelayCommand]
        private void SavePreset()
        {
            if (!string.IsNullOrWhiteSpace(SelectedPresetName)) savePreset(SelectedPresetName);
        }

        private async Task ReadSensorsAsync()
        {
            if (!start || IsDisposed) return;
            _readings = await _sensorService.ReadAsync();
            if (!start || IsDisposed) return;
            if (IsAutoSwitchEnabled)
            {
                var game = await _sensorService.FindRunningGameAsync();
                if (!IsDisposed && PowerPresetOptions.Contains(game) && SelectedPresetName != game)
                    SelectedPresetName = game;
            }
        }


        private Task update() => _control.UpdateAsync(this, _readings, Settings.Default.isASUS);


        [RelayCommand]
        private void SaveAutoSwitch()
        {
            Settings.Default.autoSwitch = (bool)IsAutoSwitchEnabled;
            Settings.Default.Save();
        }


        partial void OnSelectedPresetNameChanged(string value)
        {

            string presetName = value;
            loadPreset(presetName);

        }

        partial void OnIsBoostEnabledChanged(bool value)
        {
            if (value) { IsRSREnabled = false; IsAntiLagEnabled = false; }
        }

        partial void OnIsAntiLagEnabledChanged(bool value)
        {
            if (value) IsBoostEnabled = false;
        }

        partial void OnIsRSREnabledChanged(bool value)
        {
            if (value) { IsBoostEnabled = false; IsImageSharpEnabled = false; }
        }

        partial void OnIsImageSharpEnabledChanged(bool value)
        {
            if (value) IsRSREnabled = false;
        }
        public override async Task StopAsync()
        {
            var baseStop = base.StopAsync();
            try
            {
                await Task.WhenAll(baseStop, _sensorTask, _updateTask,
                    ToggleAdaptiveCommand.ExecutionTask ?? Task.CompletedTask,
                    ReloadGamesCommand.ExecutionTask ?? Task.CompletedTask);
            }
            finally { _sensorService.Close(); }
        }

        public override void Dispose()
        {
            base.Dispose();
            start = false;
            Settings.Default.isAdaptiveModeRunning = false;
        }
    }
}
