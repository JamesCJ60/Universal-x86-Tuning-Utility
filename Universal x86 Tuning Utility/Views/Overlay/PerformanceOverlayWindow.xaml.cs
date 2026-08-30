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
        private const uint NoSize = 0x0001;
        private const uint NoZOrder = 0x0004;
        private const uint NoActivate = 0x0010;
        private const uint NoOwnerZOrder = 0x0200;

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
            if (e.PropertyName == nameof(PerformanceOverlayViewModel.TargetProcessId))
            {
                MoveToTargetDisplay(_viewModel.TargetProcessId);
                return;
            }

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

            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero || !GetWindowRect(handle, out NativeRect bounds))
                return;

            System.Drawing.Rectangle area = System.Windows.Forms.Screen.FromHandle(handle).WorkingArea;
            int width = bounds.Right - bounds.Left;
            int height = bounds.Bottom - bounds.Top;
            int left = Math.Clamp(bounds.Left, area.Left + 5, Math.Max(area.Left + 5, area.Right - width - 5));
            int top = Math.Clamp(bounds.Top, area.Top + 5, Math.Max(area.Top + 5, area.Bottom - height - 5));

            if (left != bounds.Left || top != bounds.Top)
                SetWindowPos(handle, IntPtr.Zero, left, top, 0, 0, NoSize | NoZOrder | NoActivate | NoOwnerZOrder);
        }

        private void MoveToTargetDisplay(int processId)
        {
            if (!IsLoaded || processId <= 0)
                return;

            IntPtr targetWindow = FindProcessWindow(processId);
            IntPtr overlayWindow = new WindowInteropHelper(this).Handle;
            if (targetWindow == IntPtr.Zero || overlayWindow == IntPtr.Zero || !GetWindowRect(overlayWindow, out NativeRect bounds))
                return;

            System.Windows.Forms.Screen targetScreen = System.Windows.Forms.Screen.FromHandle(targetWindow);
            System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(overlayWindow);
            if (string.Equals(targetScreen.DeviceName, currentScreen.DeviceName, StringComparison.OrdinalIgnoreCase))
                return;

            System.Drawing.Rectangle currentArea = currentScreen.WorkingArea;
            System.Drawing.Rectangle targetArea = targetScreen.WorkingArea;
            int width = bounds.Right - bounds.Left;
            int height = bounds.Bottom - bounds.Top;
            int relativeLeft = Math.Max(5, bounds.Left - currentArea.Left);
            int relativeTop = Math.Max(5, bounds.Top - currentArea.Top);
            int left = targetArea.Left + Math.Min(relativeLeft, Math.Max(5, targetArea.Width - width - 5));
            int top = targetArea.Top + Math.Min(relativeTop, Math.Max(5, targetArea.Height - height - 5));
            SetWindowPos(overlayWindow, IntPtr.Zero, left, top, 0, 0, NoSize | NoZOrder | NoActivate | NoOwnerZOrder);
        }

        private static IntPtr FindProcessWindow(int processId)
        {
            IntPtr result = IntPtr.Zero;
            EnumWindows((window, _) =>
            {
                GetWindowThreadProcessId(window, out uint windowProcessId);
                if (windowProcessId != processId || !IsWindowVisible(window))
                    return true;

                result = window;
                return false;
            }, IntPtr.Zero);
            return result;
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

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr window, out NativeRect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        private delegate bool EnumWindowsCallback(IntPtr window, IntPtr parameter);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
