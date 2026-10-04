using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Services;

public interface IPresetApplicationService
{
    Task ApplyAsync(string commands, bool isAutoReapply = false, string? appliedName = null, bool localizeAppliedName = false);
}
