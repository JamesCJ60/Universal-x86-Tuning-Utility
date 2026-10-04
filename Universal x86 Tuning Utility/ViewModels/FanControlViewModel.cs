using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class FanControlViewModel : PageViewModel
{
    private readonly IFanControlService _fan;
    private readonly IUserInteractionService _interaction;
    private readonly DispatcherTimer _timer;
    private bool _reading;
    [ObservableProperty] private string _configurationName = "";
    [ObservableProperty] private string _status = "Disabled";
    [ObservableProperty] private double _speed = 50;

    public FanControlViewModel(IFanControlService fan, IUserInteractionService interaction)
    {
        _fan = fan;
        _interaction = interaction;
        _timer = OwnTimer(new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) });
        _timer.Tick += OnTick;
    }

    protected override Task InitializeAsync() { Reload(); return Task.CompletedTask; }
    [RelayCommand] private void Reload() { _fan.Reload(); ConfigurationName = _fan.ConfigurationName; }
    [RelayCommand] private void Enable() => _fan.Enable();
    [RelayCommand] private void Disable() { _timer.Stop(); Status = "Disabled"; _fan.Disable(); }
    [RelayCommand] private void ApplySpeed() => _fan.SetSpeed((int)Speed);
    [RelayCommand] private void CopyConfigurationName() => _interaction.CopyText(ConfigurationName);
    [RelayCommand]
    private void ToggleCurve()
    {
        if (_timer.IsEnabled) { _timer.Stop(); Status = "Disabled"; }
        else _timer.Start();
    }

    private async void OnTick(object? sender, EventArgs e)
    {
        if (_reading || IsDisposed) return;
        _reading = true;
        await RunCommandAsync(async () =>
        {
            try
            {
                var temperature = await Task.Run(_fan.ReadCpuTemperature);
                if (!_timer.IsEnabled || IsDisposed) return;
                var speed = Interpolate(temperature);
                if (_fan.IsEnabled) _fan.SetSpeed(speed);
                Status = $"Enabled - {speed}% - {temperature}°C";
            }
            catch (Exception error) { _timer.Stop(); Status = error.Message; }
            finally { _reading = false; }
        });
    }

    public static int Interpolate(int temperature)
    {
        int[] temperatures = { 25, 35, 45, 55, 65, 75, 85, 95 };
        int[] speeds = { 0, 5, 15, 25, 40, 55, 70, 100 };
        if (temperature <= temperatures[0]) return speeds[0];
        for (var i = 1; i < temperatures.Length; i++)
            if (temperature <= temperatures[i])
                return speeds[i - 1] + (speeds[i] - speeds[i - 1]) * (temperature - temperatures[i - 1]) / (temperatures[i] - temperatures[i - 1]);
        return speeds[^1];
    }
}
