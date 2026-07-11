using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.Services;
using Universal_x86_Tuning_Utility.Services.Performance;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class PerformanceOverlayViewModel : ObservableObject, IDisposable
    {
        private const double BaseDesignWidth = 1090;
        private const double BaseDesignHeight = 435;
        private const double BaseGraphWidth = 190;
        private const double BaseGraphHeight = 56;
        private const double BaseMemoryBarWidth = 96;
        private const int MaximumGraphSamples = 60;

        private readonly IHardwareMonitoringService _hardware;
        private readonly IFpsMonitoringService _fps;
        private readonly DispatcherTimer _timer;
        private readonly Queue<double> _frameTimes = new();
        private readonly Queue<double> _onePercentFrameTimes = new();
        private readonly Queue<double> _pointOnePercentFrameTimes = new();
        private readonly Queue<double> _gpuPower = new();
        private readonly Queue<double> _cpuPower = new();
        private IDisposable? _hardwareLease;
        private IDisposable? _fpsLease;
        private OverlayOptions _options = new();
        private int _isUpdating;
        private bool _disposed;

        [ObservableProperty] private Visibility _contentVisibility = Visibility.Visible;
        [ObservableProperty] private double _windowWidth = BaseDesignWidth;
        [ObservableProperty] private double _windowHeight = BaseDesignHeight;
        [ObservableProperty] private double _overlayOpacity = 0.95;
        [ObservableProperty] private bool _showFps = true;
        [ObservableProperty] private bool _showFrameTimes = true;
        [ObservableProperty] private bool _showGpu = true;
        [ObservableProperty] private bool _showCpu = true;
        [ObservableProperty] private bool _showMemory = true;
        [ObservableProperty] private bool _showBattery = true;
        [ObservableProperty] private bool _showGraphs = true;
        [ObservableProperty] private int _currentFps;
        [ObservableProperty] private int _onePercentLowFps;
        [ObservableProperty] private int _pointOnePercentLowFps;
        [ObservableProperty] private double _frameTimeMs;
        [ObservableProperty] private double _onePercentFrameTimeMs;
        [ObservableProperty] private double _pointOnePercentFrameTimeMs;
        [ObservableProperty] private string _graphicsApi = "N/A";
        [ObservableProperty] private int _gpuTemperature;
        [ObservableProperty] private int _gpuUsage;
        [ObservableProperty] private int _gpuPowerWatts;
        [ObservableProperty] private int _gpuClockMhz;
        [ObservableProperty] private double _gpuMemoryUsedGb;
        [ObservableProperty] private double _gpuMemoryTotalGb;
        [ObservableProperty] private int _cpuTemperature;
        [ObservableProperty] private int _cpuUsage;
        [ObservableProperty] private int _cpuPowerWatts;
        [ObservableProperty] private int _cpuClockMhz;
        [ObservableProperty] private double _systemMemoryUsedGb;
        [ObservableProperty] private double _systemMemoryTotalGb;
        [ObservableProperty] private bool _hasBattery;
        [ObservableProperty] private int _batteryPercent;
        [ObservableProperty] private bool _isBatteryCharging;
        [ObservableProperty] private double _batteryPowerWatts;
        [ObservableProperty] private int _batteryTimeRemainingMinutes;
        [ObservableProperty] private PointCollection _frameTimePoints = new();
        [ObservableProperty] private PointCollection _onePercentFrameTimePoints = new();
        [ObservableProperty] private PointCollection _pointOnePercentFrameTimePoints = new();
        [ObservableProperty] private PointCollection _gpuPowerPoints = new();
        [ObservableProperty] private PointCollection _cpuPowerPoints = new();
        [ObservableProperty] private string _frameTimeAxisMaxText = "10";
        [ObservableProperty] private string _frameTimeAxisMidText = "5";
        [ObservableProperty] private string _frameTimeAxisMinText = "0";
        [ObservableProperty] private string _powerAxisMaxText = "50";
        [ObservableProperty] private string _powerAxisMidText = "25";
        [ObservableProperty] private string _powerAxisMinText = "0";

        public double DesignWidth => BaseDesignWidth;
        public double DesignHeight => BaseDesignHeight;
        public FontFamily OverlayFontFamily { get; } = new("Cascadia Mono");
        public Brush PrimaryTextBrush { get; } = CreateBrush("#F5F7F7");
        public Brush MutedTextBrush { get; } = CreateBrush("#D2D8D9");
        public Brush PanelBackgroundBrush { get; } = CreateBrush("#D20A1114");
        public Brush PanelBorderBrush { get; } = CreateBrush("#704A5C61");
        public Brush SeparatorBrush { get; } = CreateBrush("#806A797D");
        public Brush GraphGridLineBrush { get; } = CreateBrush("#4A607176");
        public Brush GpuBrush { get; } = CreateBrush("#00F5FF");
        public Brush CpuBrush { get; } = CreateBrush("#F13CFF");
        public Brush FrameTimeBrush { get; } = CreateBrush("#F5F7F7");
        public Brush OnePercentBrush { get; } = CreateBrush("#00E5FF");
        public Brush ZeroPointOnePercentBrush { get; } = CreateBrush("#FF2CFF");
        public Brush BatteryBrush { get; } = CreateBrush("#39FF14");
        public Brush BatteryRingBackgroundBrush { get; } = CreateBrush("#7054666A");
        public Brush MemoryBarBackgroundBrush { get; } = CreateBrush("#5054666A");
        public Thickness PanelBorderThickness => new(1);
        public CornerRadius PanelCornerRadius => new(6);
        public CornerRadius MemoryBarCornerRadius => new(4);
        public double LabelFontSize => 16;
        public double ValueFontSize => 17;
        public double LegendFontSize => 14;
        public double FpsFontSize => 60;
        public double FpsUnitFontSize => 23;
        public double HardwareLabelFontSize => 21;
        public double GraphTitleFontSize => 16;
        public double AxisFontSize => 13;
        public double GraphColumnWidth => BaseGraphWidth;
        public double GraphWidth => BaseGraphWidth;
        public double GraphHeight => BaseGraphHeight;
        public double GraphMidY => BaseGraphHeight / 2;
        public double GraphGridLineThickness => 1;
        public double GraphStrokeThickness => 1.5;
        public double MemoryBarWidth => BaseMemoryBarWidth;
        public double MemoryBarHeight => 8;
        public int ZeroPointOnePercentLowFps => PointOnePercentLowFps;
        public double ZeroPointOnePercentFrameTimeMs => PointOnePercentFrameTimeMs;
        public PointCollection ZeroPointOnePercentFrameTimePoints => PointOnePercentFrameTimePoints;
        public string GpuMemoryText => $"{GpuMemoryUsedGb:0.0} / {GpuMemoryTotalGb:0.0} GB";
        public string SystemMemoryText => $"{SystemMemoryUsedGb:0.0} / {SystemMemoryTotalGb:0.0} GB";
        public string BatteryText => HasBattery ? $"{BatteryPercent}%{(IsBatteryCharging ? " charging" : string.Empty)}" : "No battery";
        public string BatteryTimeText => BatteryTimeRemainingMinutes > 0 ? $"{BatteryTimeRemainingMinutes / 60}h {BatteryTimeRemainingMinutes % 60:00}m" : "N/A";
        public string BatteryPercentText => HasBattery ? $"{Math.Clamp(BatteryPercent, 0, 100)}%" : "N/A";
        public string BatteryStatusText => !HasBattery ? "No battery" : IsBatteryCharging ? "Charging" : "Discharging";
        public string BatteryTimeRemainingText => BatteryTimeText;
        public string BatteryPowerText => HasBattery ? $"{BatteryPowerWatts:0.0} W" : "N/A";
        public double GpuMemoryUsageBarWidth => CalculateUsageWidth(GpuMemoryUsedGb, GpuMemoryTotalGb);
        public double SystemMemoryUsageBarWidth => CalculateUsageWidth(SystemMemoryUsedGb, SystemMemoryTotalGb);
        public Geometry BatteryRingGeometry => CreateBatteryRingGeometry(BatteryPercent);
        public Geometry BatteryStateIconGeometry => IsBatteryCharging ? ChargingGeometry : BatteryGeometry;
        public Visibility LeftPanelVisibility => ToVisibility(ShowFps || ShowFrameTimes);
        public Visibility FpsBlockVisibility => ToVisibility(ShowFps);
        public Visibility LowFpsBlockVisibility => ToVisibility(ShowFps);
        public Visibility FrameTimeStatsVisibility => ToVisibility(ShowFrameTimes);
        public Visibility ApiBlockVisibility => ToVisibility(ShowFps);
        public Visibility BatteryTileVisibility => ToVisibility(ShowBattery && HasBattery);
        public Visibility GpuRowVisibility => ToVisibility(ShowGpu);
        public Visibility CpuRowVisibility => ToVisibility(ShowCpu || ShowMemory);
        public Visibility GpuSparklineVisibility => ToVisibility(ShowGpu && GpuMemoryTotalGb > 0);
        public Visibility CpuSparklineVisibility => ToVisibility((ShowCpu || ShowMemory) && SystemMemoryTotalGb > 0);
        public Visibility BottomGraphsVisibility => ToVisibility(ShowGraphs && (ShowFrameTimes || ShowGpu || ShowCpu));
        public Visibility FrameTimeGraphVisibility => ToVisibility(ShowGraphs && ShowFrameTimes);
        public Visibility PowerGraphVisibility => ToVisibility(ShowGraphs && (ShowGpu || ShowCpu));
        public Visibility FrameTimeLineVisibility => ToVisibility(ShowGraphs && ShowFrameTimes);
        public Visibility OnePercentFrameTimeLineVisibility => ToVisibility(ShowGraphs && ShowFrameTimes);
        public Visibility ZeroPointOnePercentFrameTimeLineVisibility => ToVisibility(ShowGraphs && ShowFrameTimes);
        public Visibility GpuPowerLineVisibility => ToVisibility(ShowGraphs && ShowGpu);
        public Visibility CpuPowerLineVisibility => ToVisibility(ShowGraphs && ShowCpu);

        private static Geometry ChargingGeometry { get; } = CreateFrozenGeometry("M9,0 L2,10 H7 L5,18 L14,7 H9 Z");
        private static Geometry BatteryGeometry { get; } = CreateFrozenGeometry("M0,3 H14 V15 H0 Z M14,7 H17 V11 H14 Z");

        public PerformanceOverlayViewModel(IHardwareMonitoringService hardware, IFpsMonitoringService fps)
        {
            _hardware = hardware ?? throw new ArgumentNullException(nameof(hardware));
            _fps = fps ?? throw new ArgumentNullException(nameof(fps));
            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Tick += UpdateMetrics;
        }

        public void ApplyOptions(OverlayOptions options)
        {
            ThrowIfDisposed();

            bool monitoringWasActive = _timer.IsEnabled;
            if (monitoringWasActive)
                StopMonitoring();

            double scale = IsFinite(options.Scale) && options.Scale > 0 ? Math.Clamp(options.Scale, 0.3, 3.0) : 1.0;
            double opacity = IsFinite(options.Opacity) ? Math.Clamp(options.Opacity, 0.0, 1.0) : 0.95;
            int refreshInterval = Math.Max(16, options.RefreshIntervalMs);

            _options = options with
            {
                Scale = scale,
                Opacity = opacity,
                RefreshIntervalMs = refreshInterval
            };

            WindowWidth = BaseDesignWidth * scale;
            WindowHeight = BaseDesignHeight * scale;
            OverlayOpacity = opacity;
            ShowFps = options.ShowFps;
            ShowFrameTimes = options.ShowFrameTimes;
            ShowGpu = options.ShowGpu;
            ShowCpu = options.ShowCpu;
            ShowMemory = options.ShowMemory;
            ShowBattery = options.ShowBattery;
            ShowGraphs = options.ShowGraphs;
            _timer.Interval = TimeSpan.FromMilliseconds(refreshInterval);
            NotifyVisibilityProperties();

            if (monitoringWasActive)
                StartMonitoring();
        }

        public void StartMonitoring()
        {
            ThrowIfDisposed();

            if (_timer.IsEnabled)
                return;

            HardwareMonitoringCategory categories = HardwareMonitoringCategory.None;
            if (_options.ShowCpu)
                categories |= HardwareMonitoringCategory.Cpu;
            if (_options.ShowMemory)
                categories |= HardwareMonitoringCategory.Memory;
            if (_options.ShowGpu)
                categories |= HardwareMonitoringCategory.Gpu;
            if (_options.ShowBattery)
                categories |= HardwareMonitoringCategory.Battery;

            _hardwareLease = _hardware.Acquire(categories);
            _fpsLease = _fps.Acquire();
            UpdateMetrics(this, EventArgs.Empty);
            _timer.Start();
        }

        public void StopMonitoring()
        {
            _timer.Stop();
            _hardwareLease?.Dispose();
            _hardwareLease = null;
            _fpsLease?.Dispose();
            _fpsLease = null;
        }

        public void SetContentVisible(bool visible)
        {
            if (_disposed)
                return;

            ContentVisibility = visible ? Visibility.Visible : Visibility.Hidden;

            if (visible)
                StartMonitoring();
            else
                StopMonitoring();
        }

        private async void UpdateMetrics(object? sender, EventArgs e)
        {
            if (_disposed || Interlocked.Exchange(ref _isUpdating, 1) != 0)
                return;

            try
            {
                HardwareMetricsSnapshot hardware = await Task.Run(_hardware.ReadSnapshot);
                FpsMetricsSnapshot fps = _fps.Latest;

                CurrentFps = fps.Fps;
                OnePercentLowFps = fps.OnePercentLowFps;
                PointOnePercentLowFps = fps.PointOnePercentLowFps;
                FrameTimeMs = fps.FrameTimeMs;
                OnePercentFrameTimeMs = fps.OnePercentFrameTimeMs;
                PointOnePercentFrameTimeMs = fps.PointOnePercentFrameTimeMs;
                GraphicsApi = fps.GraphicsApi;
                GpuTemperature = hardware.GpuTemperature;
                GpuUsage = hardware.GpuUsage;
                GpuPowerWatts = hardware.GpuPowerWatts;
                GpuClockMhz = hardware.GpuClockMhz;
                GpuMemoryUsedGb = hardware.GpuMemoryUsedGb;
                GpuMemoryTotalGb = hardware.GpuMemoryTotalGb;
                CpuTemperature = hardware.CpuTemperature;
                CpuUsage = hardware.CpuUsage;
                CpuPowerWatts = hardware.CpuPowerWatts;
                CpuClockMhz = hardware.CpuClockMhz;
                SystemMemoryUsedGb = hardware.SystemMemoryUsedGb;
                SystemMemoryTotalGb = hardware.SystemMemoryTotalGb;
                HasBattery = hardware.HasBattery;
                BatteryPercent = hardware.BatteryPercent;
                IsBatteryCharging = hardware.IsBatteryCharging;
                BatteryPowerWatts = hardware.BatteryPowerWatts;
                BatteryTimeRemainingMinutes = hardware.BatteryTimeRemainingMinutes;

                NotifyMetricProperties();

                Add(_frameTimes, FrameTimeMs);
                Add(_onePercentFrameTimes, OnePercentFrameTimeMs);
                Add(_pointOnePercentFrameTimes, PointOnePercentFrameTimeMs);
                Add(_gpuPower, GpuPowerWatts);
                Add(_cpuPower, CpuPowerWatts);

                UpdateFrameTimeGraph();
                UpdatePowerGraph();
            }
            catch
            {
            }
            finally
            {
                Interlocked.Exchange(ref _isUpdating, 0);
            }
        }

        private void UpdateFrameTimeGraph()
        {
            double[] frameTimeSamples = _frameTimes.ToArray();
            double[] onePercentSamples = _onePercentFrameTimes.ToArray();
            double[] pointOnePercentSamples = _pointOnePercentFrameTimes.ToArray();
            double axisMaximum = CalculateAxisMaximum(10, frameTimeSamples, onePercentSamples, pointOnePercentSamples);

            FrameTimePoints = BuildPoints(frameTimeSamples, BaseGraphWidth, BaseGraphHeight, axisMaximum);
            OnePercentFrameTimePoints = BuildPoints(onePercentSamples, BaseGraphWidth, BaseGraphHeight, axisMaximum);
            PointOnePercentFrameTimePoints = BuildPoints(pointOnePercentSamples, BaseGraphWidth, BaseGraphHeight, axisMaximum);
            FrameTimeAxisMaxText = FormatAxisValue(axisMaximum, true);
            FrameTimeAxisMidText = FormatAxisValue(axisMaximum / 2.0, true);
            FrameTimeAxisMinText = "0";
            OnPropertyChanged(nameof(ZeroPointOnePercentFrameTimePoints));
        }

        private void UpdatePowerGraph()
        {
            double[] gpuSamples = _gpuPower.ToArray();
            double[] cpuSamples = _cpuPower.ToArray();
            double axisMaximum = CalculateAxisMaximum(50, gpuSamples, cpuSamples);

            GpuPowerPoints = BuildPoints(gpuSamples, BaseGraphWidth, BaseGraphHeight, axisMaximum);
            CpuPowerPoints = BuildPoints(cpuSamples, BaseGraphWidth, BaseGraphHeight, axisMaximum);
            PowerAxisMaxText = FormatAxisValue(axisMaximum, false);
            PowerAxisMidText = FormatAxisValue(axisMaximum / 2.0, false);
            PowerAxisMinText = "0";
        }

        private void NotifyMetricProperties()
        {
            OnPropertyChanged(nameof(ZeroPointOnePercentLowFps));
            OnPropertyChanged(nameof(ZeroPointOnePercentFrameTimeMs));
            OnPropertyChanged(nameof(GpuMemoryText));
            OnPropertyChanged(nameof(SystemMemoryText));
            OnPropertyChanged(nameof(GpuMemoryUsageBarWidth));
            OnPropertyChanged(nameof(SystemMemoryUsageBarWidth));
            OnPropertyChanged(nameof(BatteryText));
            OnPropertyChanged(nameof(BatteryTimeText));
            OnPropertyChanged(nameof(BatteryPercentText));
            OnPropertyChanged(nameof(BatteryStatusText));
            OnPropertyChanged(nameof(BatteryTimeRemainingText));
            OnPropertyChanged(nameof(BatteryPowerText));
            OnPropertyChanged(nameof(BatteryRingGeometry));
            OnPropertyChanged(nameof(BatteryStateIconGeometry));
            OnPropertyChanged(nameof(BatteryTileVisibility));
            OnPropertyChanged(nameof(GpuSparklineVisibility));
            OnPropertyChanged(nameof(CpuSparklineVisibility));
        }

        private void NotifyVisibilityProperties()
        {
            OnPropertyChanged(nameof(LeftPanelVisibility));
            OnPropertyChanged(nameof(FpsBlockVisibility));
            OnPropertyChanged(nameof(LowFpsBlockVisibility));
            OnPropertyChanged(nameof(FrameTimeStatsVisibility));
            OnPropertyChanged(nameof(ApiBlockVisibility));
            OnPropertyChanged(nameof(BatteryTileVisibility));
            OnPropertyChanged(nameof(GpuRowVisibility));
            OnPropertyChanged(nameof(CpuRowVisibility));
            OnPropertyChanged(nameof(GpuSparklineVisibility));
            OnPropertyChanged(nameof(CpuSparklineVisibility));
            OnPropertyChanged(nameof(BottomGraphsVisibility));
            OnPropertyChanged(nameof(FrameTimeGraphVisibility));
            OnPropertyChanged(nameof(PowerGraphVisibility));
            OnPropertyChanged(nameof(FrameTimeLineVisibility));
            OnPropertyChanged(nameof(OnePercentFrameTimeLineVisibility));
            OnPropertyChanged(nameof(ZeroPointOnePercentFrameTimeLineVisibility));
            OnPropertyChanged(nameof(GpuPowerLineVisibility));
            OnPropertyChanged(nameof(CpuPowerLineVisibility));
        }

        private static void Add(Queue<double> values, double value)
        {
            double safeValue = IsFinite(value) ? Math.Max(0, value) : 0;
            values.Enqueue(safeValue);

            while (values.Count > MaximumGraphSamples)
                values.Dequeue();
        }

        private static PointCollection BuildPoints(double[] samples, double width, double height, double axisMaximum)
        {
            var points = new PointCollection(samples.Length);

            if (samples.Length == 0)
                return points;

            double maximum = Math.Max(1, axisMaximum);
            double drawableHeight = Math.Max(1, height - 1);

            for (int index = 0; index < samples.Length; index++)
            {
                double x = samples.Length == 1 ? 0 : index * width / (samples.Length - 1);
                double y = drawableHeight - Math.Clamp(samples[index] / maximum, 0, 1) * drawableHeight;
                points.Add(new Point(x, y));
            }

            points.Freeze();
            return points;
        }

        private static double CalculateAxisMaximum(double minimum, params double[][] series)
        {
            double maximum = Math.Max(0, minimum);

            foreach (double[] samples in series)
            {
                if (samples.Length > 0)
                    maximum = Math.Max(maximum, samples.Max());
            }

            if (!IsFinite(maximum) || maximum <= 0)
                return Math.Max(1, minimum);

            double paddedMaximum = maximum * 1.1;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(paddedMaximum)));
            double step = magnitude / 2.0;

            if (!IsFinite(step) || step <= 0)
                step = 1;

            return Math.Max(Math.Max(1, minimum), Math.Ceiling(paddedMaximum / step) * step);
        }

        private static string FormatAxisValue(double value, bool allowDecimal)
        {
            if (!IsFinite(value))
                return "0";

            return allowDecimal && value < 10 ? value.ToString("0.#") : value.ToString("0");
        }

        private static double CalculateUsageWidth(double used, double total)
        {
            if (!IsFinite(used) || !IsFinite(total) || total <= 0)
                return 0;

            return Math.Clamp(used / total, 0, 1) * BaseMemoryBarWidth;
        }

        private static Geometry CreateBatteryRingGeometry(int percentage)
        {
            double progress = Math.Clamp(percentage, 0, 100) / 100.0;
            if (progress <= 0)
                return Geometry.Empty;

            const double centre = 36;
            const double radius = 33;
            double endAngle = -90 + 359.999 * progress;
            double radians = endAngle * Math.PI / 180.0;
            Point start = new(centre, centre - radius);
            Point end = new(centre + radius * Math.Cos(radians), centre + radius * Math.Sin(radians));
            var figure = new PathFigure
            {
                StartPoint = start,
                IsClosed = false,
                IsFilled = false
            };
            figure.Segments.Add(new ArcSegment
            {
                Point = end,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = progress > 0.5
            });
            var geometry = new PathGeometry(new[] { figure });
            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateFrozenGeometry(string data)
        {
            Geometry geometry = Geometry.Parse(data);
            geometry.Freeze();
            return geometry;
        }

        private static Brush CreateBrush(string colour)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(colour)!;
            brush.Freeze();
            return brush;
        }

        private static Visibility ToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PerformanceOverlayViewModel));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _timer.Tick -= UpdateMetrics;
            StopMonitoring();
            GC.SuppressFinalize(this);
        }
    }
}
