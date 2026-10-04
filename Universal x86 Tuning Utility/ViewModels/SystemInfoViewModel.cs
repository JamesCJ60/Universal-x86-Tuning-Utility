using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class SystemInfoViewModel : PageViewModel
{
    private readonly ISystemInformationService _service;
    private readonly DispatcherTimer _timer;
    private bool _reading;
    [ObservableProperty] private SystemInformationState _information = new();
    public object MemoryTimings => _service.MemoryTimings;

    public SystemInfoViewModel(ISystemInformationService service)
    {
        _service = service;
        _timer = OwnTimer(new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) });
        _timer.Tick += RefreshBattery;
    }
    protected override async Task InitializeAsync() => Information = await _service.ReadAsync();
    protected override void OnActivated() => _timer.Start();
    protected override void OnDeactivated() => _timer.Stop();
    private async void RefreshBattery(object? sender, EventArgs e)
    {
        if (_reading || !IsActive || IsDisposed || Information.BatteryVisibility != System.Windows.Visibility.Visible) return;
        _reading = true;
        await RunCommandAsync(async () =>
        {
            try { Information.ChargeRateText = await _service.ReadBatteryRateAsync(); }
            catch (Exception error) { Serilog.Log.Error(error, "Failed to read battery rate"); }
            finally { _reading = false; }
        });
    }
}
