using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class AutomationsViewModel : PageViewModel
{
    private readonly IPresetCatalogService _presets;
    private readonly IAutomationSettings _settings;
    private bool _loading;
    [ObservableProperty] private bool _isCpuUndervoltEnabled = Settings.Default.isAutoUvCPU;
    [ObservableProperty] private bool _isGpuUndervoltEnabled = Settings.Default.isAutoUviGPU;
    partial void OnIsCpuUndervoltEnabledChanged(bool value) { Settings.Default.isAutoUvCPU = value; Settings.Default.Save(); }
    partial void OnIsGpuUndervoltEnabledChanged(bool value) { Settings.Default.isAutoUviGPU = value; Settings.Default.Save(); }
    public ObservableCollection<string> Presets { get; } = new();
    [ObservableProperty] private string _selectedAcPreset = "None";
    [ObservableProperty] private string _selectedDcPreset = "None";
    [ObservableProperty] private string _selectedResumePreset = "None";

    public AutomationsViewModel(IPresetCatalogService presets, IAutomationSettings settings)
    {
        _presets = presets;
        _settings = settings;
    }

    protected override Task InitializeAsync() { ReloadPresets(); return Task.CompletedTask; }
    protected override void OnActivated() => ReloadPresets();

    [RelayCommand]
    private void ReloadPresets()
    {
        _loading = true;
        try
        {
            Presets.Clear();
            foreach (var name in _presets.GetNames()) Presets.Add(name);
            SelectedAcPreset = Presets.Contains(_settings.acPreset) ? _settings.acPreset : "None";
            SelectedDcPreset = Presets.Contains(_settings.dcPreset) ? _settings.dcPreset : "None";
            SelectedResumePreset = Presets.Contains(_settings.resumePreset) ? _settings.resumePreset : "None";
        }
        finally { _loading = false; }
        SaveSelections();
    }

    partial void OnSelectedAcPresetChanged(string value) => SaveSelections();
    partial void OnSelectedDcPresetChanged(string value) => SaveSelections();
    partial void OnSelectedResumePresetChanged(string value) => SaveSelections();

    private void SaveSelections()
    {
        if (_loading) return;
        _settings.acPreset = SelectedAcPreset;
        _settings.acCommandString = _presets.GetCommands(SelectedAcPreset);
        _settings.dcPreset = SelectedDcPreset;
        _settings.dcCommandString = _presets.GetCommands(SelectedDcPreset);
        _settings.resumePreset = SelectedResumePreset;
        _settings.resumeCommandString = _presets.GetCommands(SelectedResumePreset);
        _settings.Save();
    }
}
