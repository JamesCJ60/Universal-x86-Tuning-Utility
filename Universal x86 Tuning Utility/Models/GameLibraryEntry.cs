using CommunityToolkit.Mvvm.ComponentModel;

namespace Universal_x86_Tuning_Utility.Models;

public partial class GameLibraryEntry : ObservableObject
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Launcher { get; init; } = "";
    public string Path { get; init; } = "";
    public string Executable { get; init; } = "";
    public string LaunchCommand { get; init; } = "";
    public string IconPath { get; init; } = "";
    [ObservableProperty] private string _framesPerSecond = "No Data";
    [ObservableProperty] private string _frameTime = "No Data";
}
