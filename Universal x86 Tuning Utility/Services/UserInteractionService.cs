using System.Windows;
using Microsoft.Win32;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Services;

public sealed class UserInteractionService : IUserInteractionService
{
    public void Notify(string title, string message) => ToastNotification.ShowToastNotification(title, message);
    public void ShowError(string message) => MessageBox.Show(message, "Universal x86 Tuning Utility", MessageBoxButton.OK, MessageBoxImage.Error);
    public void CopyText(string text) => Clipboard.SetText(text);
    public string? SelectGameExecutable()
    {
        var dialog = new OpenFileDialog { Title = "Select game", Filter = "Executable (*.exe)|*.exe" };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
