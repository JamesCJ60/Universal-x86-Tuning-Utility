using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace Universal_x86_Tuning_Utility.ViewModels;

/// <summary>Owns initialization and polling independently of the lifetime of a WPF page.</summary>
public abstract class PageViewModel : ObservableObject, IDisposable
{
    private static readonly HashSet<PageViewModel> ActiveModels = new();
    private Task? _initialization;
    private readonly List<DispatcherTimer> _timers = new();
    private readonly HashSet<Task> _commands = new();
    private bool _disposed;
    private int _activation;
    protected bool IsActive { get; private set; }
    protected bool IsDisposed => _disposed;

    public async Task ActivateAsync()
    {
        if (_disposed) return;
        ActiveModels.Add(this);
        var activation = ++_activation;
        IsActive = true;
        try
        {
            await (_initialization ??= InitializeAsync());
            if (IsActive && !_disposed && activation == _activation) OnActivated();
        }
        catch (Exception error)
        {
            Log.Error(error, "Failed to initialize {ViewModel}", GetType().Name);
            _initialization = null;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        OnDeactivated();
    }

    protected virtual Task InitializeAsync() => Task.CompletedTask;
    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }

    public virtual async Task StopAsync()
    {
        Dispose();
        var pending = new List<Task>(_commands);
        if (_initialization != null) pending.Add(_initialization);
        await Task.WhenAll(pending);
    }

    public static async Task StopAllAsync()
    {
        var models = new List<PageViewModel>(ActiveModels);
        // Stop all producers before awaiting any in-flight operation.
        var stops = new List<Task>();
        foreach (var model in models)
        {
            stops.Add(StopModelAsync(model));
        }
        await Task.WhenAll(stops);

        static async Task StopModelAsync(PageViewModel model)
        {
            try { await model.StopAsync(); }
            catch (Exception error) { Log.Error(error, "Failed to stop {ViewModel}", model.GetType().Name); }
        }
    }

    protected DispatcherTimer OwnTimer(DispatcherTimer timer)
    {
        _timers.Add(timer);
        return timer;
    }

    protected async Task RunCommandAsync(Func<Task> action)
    {
        if (_disposed) return;
        Task? task = null;
        try
        {
            task = action();
            _commands.Add(task);
            await task;
        }
        catch (Exception error) { Log.Error(error, "Command failed in {ViewModel}", GetType().Name); }
        finally { if (task != null) _commands.Remove(task); }
    }

    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ActiveModels.Remove(this);
        Deactivate();
        foreach (var timer in _timers) timer.Stop();
        _timers.Clear();
    }
}
