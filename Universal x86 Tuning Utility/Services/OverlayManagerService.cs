using Gma.System.MouseKeyHook;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.ViewModels;
using Universal_x86_Tuning_Utility.Views.Overlay;

namespace Universal_x86_Tuning_Utility.Services
{
    public sealed class OverlayManagerService :
        IHostedService,
        IDisposable
    {
        private readonly OverlaySettingsStore _settingsStore;
        private readonly IHardwareMonitoringService _hardware;
        private readonly Performance.IFpsMonitoringService _fps;

        private readonly object _settingsSaveLock = new();

        private readonly object _hookLock = new();

        private IKeyboardMouseEvents? _hook;
        private PerformanceOverlayWindow? _window;
        private PerformanceOverlayViewModel? _viewModel;

        private DateTime _lastHotkeyUtc = DateTime.MinValue;
        private bool _contentsVisible = true;
        private bool _disposed;

        public OverlayOptions CurrentOptions =>
            _settingsStore.Current;

        public OverlayManagerService(
            OverlaySettingsStore settingsStore,
            IHardwareMonitoringService hardware,
            Performance.IFpsMonitoringService fps)
        {
            _settingsStore =
                settingsStore ??
                throw new ArgumentNullException(
                    nameof(settingsStore));

            _hardware =
                hardware ??
                throw new ArgumentNullException(
                    nameof(hardware));

            _fps =
                fps ??
                throw new ArgumentNullException(
                    nameof(fps));
        }

        public Task StartAsync(
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            cancellationToken.ThrowIfCancellationRequested();

            OverlayOptions options = _settingsStore.Current;

            if (options.Enabled)
                EnsureHook();

            if (options.Enabled && options.ShowOnStartup)
                ShowOverlay();

            return Task.CompletedTask;
        }

        public Task StopAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RemoveHook();

            RunOnUiThread(() =>
            {
                try
                {
                    SavePosition();
                }
                finally
                {
                    CloseOverlayWindow();
                }
            });

            return Task.CompletedTask;
        }

        public void ApplyOptions(OverlayOptions options)
        {
            ThrowIfDisposed();

            OverlayOptions normalisedOptions =
                NormaliseForStorage(options);

            SaveOptionsIfChanged(normalisedOptions);

            if (normalisedOptions.Enabled)
                EnsureHook();
            else
                RemoveHook();

            RunOnUiThread(() =>
            {
                _viewModel?.ApplyOptions(normalisedOptions);

                if (!normalisedOptions.Enabled)
                {
                    StopMonitoring();
                    _window?.Hide();
                    return;
                }

                if (_window?.IsVisible == true &&
                    _contentsVisible)
                {
                    _viewModel?.StartMonitoring();
                }
                else
                {
                    _viewModel?.StopMonitoring();
                }
            });
        }

        public void ShowOverlay()
        {
            ThrowIfDisposed();

            RunOnUiThread(() =>
            {
                OverlayOptions options =
                    _settingsStore.Current;

                if (!options.Enabled)
                    return;

                EnsureWindow();

                _contentsVisible = true;

                _viewModel!.SetContentVisible(true);
                _viewModel.StartMonitoring();

                if (_window!.IsVisible)
                {
                    _window.Activate();
                    return;
                }

                _window.Show();
            });
        }

        public void HideOverlay()
        {
            if (_disposed)
                return;

            RunOnUiThread(() =>
            {
                StopMonitoring();
                _window?.Hide();
            });
        }

        public void ToggleOverlay()
        {
            ThrowIfDisposed();

            RunOnUiThread(() =>
            {
                if (_window?.IsVisible == true)
                    HideOverlay();
                else
                    ShowOverlay();
            });
        }

        public void ToggleContents()
        {
            ThrowIfDisposed();

            RunOnUiThread(() =>
            {
                OverlayOptions options =
                    _settingsStore.Current;

                if (!options.Enabled)
                    return;

                EnsureWindow();

                if (_window?.IsVisible != true)
                    _window!.Show();

                _contentsVisible = !_contentsVisible;

                _viewModel!.SetContentVisible(
                    _contentsVisible);

                if (_contentsVisible)
                    _viewModel.StartMonitoring();
                else
                    _viewModel.StopMonitoring();
            });
        }

        public static bool IsValidHotkey(string? text)
        {
            return Hotkey.TryParse(text, out _);
        }

        private void EnsureWindow()
        {
            if (_window is not null)
                return;

            OverlayOptions options =
                NormaliseForStorage(
                    _settingsStore.Current);

            _viewModel =
                new PerformanceOverlayViewModel(
                    _hardware,
                    _fps);

            _viewModel.ApplyOptions(options);

            _window =
                new PerformanceOverlayWindow(
                    _viewModel);

            _window.Closed += OnWindowClosed;

            _window.Left = options.Left;
            _window.Top = options.Top;
        }

        private void OnWindowClosed(
            object? sender,
            EventArgs e)
        {
            if (!ReferenceEquals(sender, _window))
                return;

            _viewModel?.StopMonitoring();
            _viewModel?.Dispose();

            _viewModel = null;
            _window = null;
        }

        private void EnsureHook()
        {
            ThrowIfDisposed();

            lock (_hookLock)
            {
                if (_hook is not null)
                    return;

                _hook = Hook.GlobalEvents();
                _hook.KeyDown += OnGlobalKeyDown;
            }
        }

        private void RemoveHook()
        {
            lock (_hookLock)
            {
                if (_hook is null)
                    return;

                _hook.KeyDown -= OnGlobalKeyDown;
                _hook.Dispose();
                _hook = null;
            }
        }

        private void OnGlobalKeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (_disposed)
                return;

            DateTime now = DateTime.UtcNow;

            if (now - _lastHotkeyUtc <
                TimeSpan.FromMilliseconds(350))
            {
                return;
            }

            OverlayOptions options =
                _settingsStore.Current;

            if (Hotkey.TryParse(
                    options.ToggleOverlayHotkey,
                    out Hotkey toggleOverlay) &&
                toggleOverlay.Matches(e))
            {
                _lastHotkeyUtc = now;
                ToggleOverlay();
                return;
            }

            if (Hotkey.TryParse(
                    options.ToggleContentsHotkey,
                    out Hotkey toggleContents) &&
                toggleContents.Matches(e))
            {
                _lastHotkeyUtc = now;
                ToggleContents();
            }
        }

        private OverlayOptions NormaliseForStorage(
            OverlayOptions options)
        {
            return RunOnUiThread(() =>
            {
                Rect workArea = SystemParameters.WorkArea;

                double fallbackLeft =
                    workArea.Left + 5;

                double fallbackTop =
                    workArea.Top + 5;

                double left = options.Left;
                double top = options.Top;

                if (!IsFinite(left))
                {
                    left =
                        _window is not null &&
                        IsFinite(_window.Left)
                            ? _window.Left
                            : fallbackLeft;
                }

                if (!IsFinite(top))
                {
                    top =
                        _window is not null &&
                        IsFinite(_window.Top)
                            ? _window.Top
                            : fallbackTop;
                }

                double scale =
                    IsFinite(options.Scale) &&
                    options.Scale > 0
                        ? options.Scale
                        : 1.0;

                double opacity =
                    IsFinite(options.Opacity)
                        ? Math.Clamp(
                            options.Opacity,
                            0.0,
                            1.0)
                        : 1.0;

                int refreshInterval =
                    options.RefreshIntervalMs > 0
                        ? options.RefreshIntervalMs
                        : 500;

                return options with
                {
                    Left = left,
                    Top = top,
                    Scale = scale,
                    Opacity = opacity,
                    RefreshIntervalMs = refreshInterval,

                    ToggleOverlayHotkey =
                        options.ToggleOverlayHotkey?
                            .Trim() ??
                        string.Empty,

                    ToggleContentsHotkey =
                        options.ToggleContentsHotkey?
                            .Trim() ??
                        string.Empty
                };
            });
        }

        private void SaveOptionsIfChanged(
            OverlayOptions options)
        {
            lock (_settingsSaveLock)
            {
                OverlayOptions current =
                    _settingsStore.Current;

                if (Equals(current, options))
                    return;

                _settingsStore.Save(options);
            }
        }

        private void SavePosition()
        {
            if (_window is null)
                return;

            double left = _window.Left;
            double top = _window.Top;

            if (!IsFinite(left) ||
                !IsFinite(top))
            {
                return;
            }

            OverlayOptions options =
                NormaliseForStorage(
                    _settingsStore.Current with
                    {
                        Left = left,
                        Top = top
                    });

            SaveOptionsIfChanged(options);
        }

        private void StopMonitoring()
        {
            _viewModel?.StopMonitoring();
        }

        private void CloseOverlayWindow()
        {
            PerformanceOverlayWindow? window =
                _window;

            PerformanceOverlayViewModel? viewModel =
                _viewModel;

            _window = null;
            _viewModel = null;

            if (window is not null)
            {
                window.Closed -= OnWindowClosed;
                window.ClosePermanently();
            }

            viewModel?.StopMonitoring();
            viewModel?.Dispose();
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private static void RunOnUiThread(
            Action action)
        {
            System.Windows.Application? application =
                System.Windows.Application.Current;

            if (application?.Dispatcher is null ||
                application.Dispatcher.CheckAccess())
            {
                action();
                return;
            }

            application.Dispatcher.Invoke(action);
        }

        private static T RunOnUiThread<T>(
            Func<T> action)
        {
            System.Windows.Application? application =
                System.Windows.Application.Current;

            if (application?.Dispatcher is null ||
                application.Dispatcher.CheckAccess())
            {
                return action();
            }

            return application.Dispatcher.Invoke(action);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(OverlayManagerService));
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            RemoveHook();

            RunOnUiThread(CloseOverlayWindow);

            GC.SuppressFinalize(this);
        }

        private readonly record struct Hotkey(
            bool Control,
            bool Alt,
            bool Shift,
            Keys Key)
        {
            public bool Matches(KeyEventArgs e)
            {
                return e.Control == Control &&
                       e.Alt == Alt &&
                       e.Shift == Shift &&
                       e.KeyCode == Key;
            }

            public static bool TryParse(
                string? text,
                out Hotkey hotkey)
            {
                hotkey = default;

                if (string.IsNullOrWhiteSpace(text))
                    return false;

                bool control = false;
                bool alt = false;
                bool shift = false;
                bool hasPrimaryKey = false;

                Keys key = Keys.None;

                string[] tokens = text.Split(
                    '+',
                    StringSplitOptions.TrimEntries |
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string token in tokens)
                {
                    if (token.Equals(
                            "Ctrl",
                            StringComparison.OrdinalIgnoreCase) ||
                        token.Equals(
                            "Control",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        control = true;
                        continue;
                    }

                    if (token.Equals(
                            "Alt",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        alt = true;
                        continue;
                    }

                    if (token.Equals(
                            "Shift",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        shift = true;
                        continue;
                    }

                    if (hasPrimaryKey)
                        return false;

                    if (!Enum.TryParse(
                            token,
                            true,
                            out key))
                    {
                        return false;
                    }

                    hasPrimaryKey = true;
                }

                if (!hasPrimaryKey ||
                    key == Keys.None ||
                    key is
                        Keys.ControlKey or
                        Keys.LControlKey or
                        Keys.RControlKey or
                        Keys.Menu or
                        Keys.LMenu or
                        Keys.RMenu or
                        Keys.ShiftKey or
                        Keys.LShiftKey or
                        Keys.RShiftKey)
                {
                    return false;
                }

                hotkey = new Hotkey(
                    control,
                    alt,
                    shift,
                    key);

                return true;
            }
        }
    }
}