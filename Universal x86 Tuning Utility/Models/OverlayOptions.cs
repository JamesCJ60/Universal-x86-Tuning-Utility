namespace Universal_x86_Tuning_Utility.Models
{
    public sealed record OverlayOptions
    {
        public bool Enabled { get; init; }
        public bool ShowOnStartup { get; init; }
        public string ToggleOverlayHotkey { get; init; } = "Ctrl+Shift+O";
        public string ToggleContentsHotkey { get; init; } = "Ctrl+Shift+I";
        public int RefreshIntervalMs { get; init; } = 500;
        public double Scale { get; init; } = 1;
        public double Opacity { get; init; } = 0.95;
        public bool ShowFps { get; init; } = true;
        public bool ShowFrameTimes { get; init; } = true;
        public bool ShowGpu { get; init; } = true;
        public bool ShowCpu { get; init; } = true;
        public bool ShowMemory { get; init; } = true;
        public bool ShowBattery { get; init; } = true;
        public bool ShowGraphs { get; init; } = true;
        public double Left { get; init; } = double.NaN;
        public double Top { get; init; } = double.NaN;
    }
}
