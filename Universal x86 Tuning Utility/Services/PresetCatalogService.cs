using System.Collections.Generic;
using System.Linq;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts;

namespace Universal_x86_Tuning_Utility.Services;

public interface IPresetCatalogService
{
    IReadOnlyList<string> GetNames();
    string GetCommands(string name);
}

/// <summary>Uses the existing Windows preset files without changing their JSON format.</summary>
public sealed class PresetCatalogService : IPresetCatalogService
{
    private PresetManager CreateManager() => new(Settings.Default.Path + (Family.TYPE switch
    {
        Family.ProcessorType.Amd_Apu => "apuPresets.json",
        Family.ProcessorType.Amd_Desktop_Cpu => "amdDtCpuPresets.json",
        _ => "intelPresets.json"
    }));

    public IReadOnlyList<string> GetNames()
    {
        var names = new List<string> { "None" };
            names.AddRange(new[] { "PM - Eco Preset", "PM - Balanced Preset", "PM - Performance Preset", "PM - Extreme Preset" });
        names.AddRange(CreateManager().GetPresetNames());
        return names.Distinct().ToList();
    }

    public string GetCommands(string name)
    {
        if (string.IsNullOrEmpty(name) || name == "None") return string.Empty;
        if (!name.StartsWith("PM -")) return CreateManager().GetPreset(name)?.commandValue ?? string.Empty;
        PremadePresets.SetPremadePresets();
        return name switch
        {
            "PM - Eco Preset" => PremadePresets.EcoPreset,
            "PM - Balanced Preset" => PremadePresets.BalPreset,
            "PM - Performance Preset" => PremadePresets.PerformancePreset,
            "PM - Extreme Preset" => PremadePresets.ExtremePreset,
            _ => string.Empty
        };
    }
}
