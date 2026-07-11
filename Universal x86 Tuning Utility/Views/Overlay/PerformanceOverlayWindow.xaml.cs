using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.ViewModels;

namespace Universal_x86_Tuning_Utility.Views.Overlay
{
    public partial class PerformanceOverlayWindow : Window
    {
        private const int ExtendedStyleIndex = -20;
        private const int TransparentStyle = 0x20;
        private const int LayeredStyle = 0x80000;
        private const int ToolWindowStyle = 0x80;
        private const int NoActivateStyle = 0x08000000;

        private readonly PerformanceOverlayViewModel _viewModel;
        private bool _allowClose;
        private bool _displaySettingsSubscribed;
        private bool _sizeUpdatePending;

        public PerformanceOverlayWindow(PerformanceOverlayViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
            Closing += OnClosing;
            SizeChanged += OnSizeChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        public void ClosePermanently()
        {
            _allowClose = true;
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, ExtendedStyleIndex);
            SetWindowLong(handle, ExtendedStyleIndex, style | TransparentStyle | LayeredStyle | ToolWindowStyle | NoActivateStyle);

            if (!_displaySettingsSubscribed)
            {
                SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
                _displaySettingsSubscribed = true;
            }

            ApplyViewModelSize();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PerformanceOverlayViewModel.WindowWidth) && e.PropertyName != nameof(PerformanceOverlayViewModel.WindowHeight))
                return;

            QueueSizeUpdate();
        }

        private void QueueSizeUpdate()
        {
            if (_sizeUpdatePending)
                return;

            _sizeUpdatePending = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _sizeUpdatePending = false;
                ApplyViewModelSize();
            }));
        }

        private void ApplyViewModelSize()
        {
            double width = _viewModel.WindowWidth;
            double height = _viewModel.WindowHeight;

            if (!IsFinitePositive(width) || !IsFinitePositive(height))
                return;

            SetCurrentValue(WidthProperty, width);
            SetCurrentValue(HeightProperty, height);
            InvalidateMeasure();
            InvalidateArrange();
            UpdateLayout();
            ClampToWorkArea();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClampToWorkArea();
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ApplyViewModelSize();
                ClampToWorkArea();
            }));
        }

        private void ClampToWorkArea()
        {
            if (!IsLoaded || !IsFinitePositive(ActualWidth) || !IsFinitePositive(ActualHeight))
                return;

            Rect area = SystemParameters.WorkArea;
            double rightLimit = Math.Max(area.Left + 5, area.Right - ActualWidth - 5);
            double bottomLimit = Math.Max(area.Top + 5, area.Bottom - ActualHeight - 5);

            Left = Math.Clamp(IsFinite(Left) ? Left : area.Left + 5, area.Left + 5, rightLimit);
            Top = Math.Clamp(IsFinite(Top) ? Top : area.Top + 5, area.Top + 5, bottomLimit);
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                Hide();
                _viewModel.StopMonitoring();
                return;
            }

            Loaded -= OnLoaded;
            Closing -= OnClosing;
            SizeChanged -= OnSizeChanged;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            if (_displaySettingsSubscribed)
            {
                SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                _displaySettingsSubscribed = false;
            }

            _viewModel.Dispose();
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool IsFinitePositive(double value)
        {
            return IsFinite(value) && value > 0;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr window, int index);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr window, int index, int newStyle);
    }
}
