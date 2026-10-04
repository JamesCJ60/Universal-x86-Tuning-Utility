using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class PremadePresetsViewModel : PageViewModel
{
    private readonly IPresetCatalogService _catalog;
    private readonly IPresetApplicationService _application;
    private readonly IUserInteractionService _interaction;
    private static readonly string[] Names = { "Eco", "Balanced", "Performance", "Extreme" };
    private static readonly string[] Descriptions =
    {
        "This preset is designed to prioritize energy efficiency over performance. It sets power limits to conservative levels to reduce power consumption and heat generation, making it ideal for prolonged use in situations where maximizing battery life or minimizing energy usage is critical.",
        "This preset aims to find a balance between performance and power consumption, providing a stable and efficient experience. This preset sets the power limits of the system to a level that balances performance and power usage, without sacrificing too much of either.",
        "This preset is optimized for maximum performance by increasing the power limits of the APU/CPU, which allows it to run at higher clock speeds for longer periods of time. This can result in improved system responsiveness and faster load times in applications that require high levels of processing power.",
        "This preset aims to push the power limits of the system to their maximum, allowing for the highest possible performance. This preset is designed for users who demand the most from their hardware and are willing to tolerate higher power consumption and potentially increased noise levels."
    };
    [ObservableProperty] private string _title = "Premade Presets";
    [ObservableProperty] private string _presetName = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _commands = "";
    [ObservableProperty] private ImageSource? _packageImage;
    [ObservableProperty] private Visibility _certifiedVisibility = Visibility.Collapsed;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEco), nameof(IsBalanced), nameof(IsPerformance), nameof(IsExtreme))]
    private int _selectedIndex = -1;
    public bool IsEco => SelectedIndex == 0;
    public bool IsBalanced => SelectedIndex == 1;
    public bool IsPerformance => SelectedIndex == 2;
    public bool IsExtreme => SelectedIndex == 3;

    public PremadePresetsViewModel(IPresetCatalogService catalog, IPresetApplicationService application, IUserInteractionService interaction)
    {
        _catalog = catalog;
        _application = application;
        _interaction = interaction;
    }

    protected override async Task InitializeAsync()
    {
        PremadePresets.SetPremadePresets();
        PackageImage = new BitmapImage(PremadePresets.uri);
        Title = LocalizationService.Get(Family.TYPE == Family.ProcessorType.Intel ? "Generic Intel presets based on processor power tier" : "Premade Presets");
        if (GetSystemInfo.Manufacturer.Contains("framework", StringComparison.OrdinalIgnoreCase))
        {
            if (GetSystemInfo.Product.Contains("laptop 16 (amd ryzen 7040", StringComparison.OrdinalIgnoreCase))
                Title = "Premade Presets - Framework Laptop 16 (AMD Ryzen 7040HS Series)";
            else if (GetSystemInfo.Product.Contains("laptop 13 (amd ryzen 7040", StringComparison.OrdinalIgnoreCase))
                Title = "Premade Presets - Framework Laptop 13 (AMD Ryzen 7040U Series)";
            CertifiedVisibility = Title == "Premade Presets" ? Visibility.Collapsed : Visibility.Visible;
        }
        if (Settings.Default.premadePreset is >= 0 and <= 3)
            await ApplyAsync(Settings.Default.premadePreset.ToString());
    }

    [RelayCommand]
    private Task ApplyAsync(string? index) => RunCommandAsync(async () =>
    {
        if (!int.TryParse(index, out var selected) || selected < 0 || selected >= Names.Length) return;
        var name = Names[selected];
        var commands = _catalog.GetCommands($"PM - {name} Preset");
        await _application.ApplyAsync(commands, appliedName: name + " Preset", localizeAppliedName: true);
        SelectedIndex = selected;
        PresetName = LocalizationService.Get(name + " Preset");
        Description = LocalizationService.Get(Descriptions[selected]);
        Commands = commands;
        Settings.Default.CommandString = commands;
        Settings.Default.premadePreset = selected;
        Settings.Default.Save();
        _interaction.Notify(name + " Preset Applied!", $"The {name.ToLowerInvariant()} premade power preset has been applied!");
    });
}
