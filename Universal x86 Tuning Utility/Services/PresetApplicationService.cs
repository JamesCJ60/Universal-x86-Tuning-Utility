using System.Threading;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Scripts;

namespace Universal_x86_Tuning_Utility.Services;

public sealed class PresetApplicationService : IPresetApplicationService
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task ApplyAsync(string commands, bool isAutoReapply = false, string? appliedName = null,
        bool localizeAppliedName = false)
    {
        if (string.IsNullOrWhiteSpace(commands)) return;
        await _gate.WaitAsync();
        try
        {
            await RyzenAdj_To_UXTU.TranslateAsync(commands, isAutoReapply, appliedName: appliedName, localizeAppliedName: localizeAppliedName);
        }
        finally
        {
            _gate.Release();
        }
    }
}
