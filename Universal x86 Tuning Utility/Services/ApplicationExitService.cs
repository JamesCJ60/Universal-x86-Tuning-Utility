using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Universal_x86_Tuning_Utility.Services;

/// <summary>Routes explicit exits through the window's asynchronous shutdown pipeline.</summary>
public sealed class ApplicationExitService
{
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    private readonly Queue<Action> _afterStop = new();
    public event Action? ExitRequested;

    public void RequestExit(Action afterStop)
    {
        _afterStop.Enqueue(afterStop);
        // Let the initiating command finish before shutdown waits for page commands.
        _dispatcher.BeginInvoke(new Action(() => ExitRequested?.Invoke()));
    }

    public void CompleteExit()
    {
        while (_afterStop.TryDequeue(out var action)) action();
    }
}
