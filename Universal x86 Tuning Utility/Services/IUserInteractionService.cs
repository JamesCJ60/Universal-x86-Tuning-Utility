namespace Universal_x86_Tuning_Utility.Services;

public interface IUserInteractionService
{
    void Notify(string title, string message);
    void ShowError(string message);
    void CopyText(string text);
    string? SelectGameExecutable();
}
