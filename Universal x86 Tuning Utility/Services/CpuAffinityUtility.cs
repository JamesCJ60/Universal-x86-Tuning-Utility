using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CpuAffinityUtility
{
    /// <summary>
    /// CCD Affinity Manager
    /// Mode 0 = all CCDs, 1 = primary CCD, 2 = secondary CCD.
    /// Will be updated for big.LITTLE APUs/CPUs in the future.
    /// </summary>
    public static class CpuAffinityManager
    {
        private static readonly object _syncRoot = new();

        private static ManagementEventWatcher? _watcher;
        private static ulong _mask;
        private static int _currentMode = -1;

        public static void SetGlobalAffinity(int mode)
        {
            if (mode == _currentMode) return;

            var newMask = BuildMask(mode);

            lock (_syncRoot)
            {
                if (mode == _currentMode) return;

                _currentMode = mode;
                _mask = newMask;

                foreach (var p in Process.GetProcesses())
                    TrySetAffinity(p, _mask);

                if (_watcher == null)
                {
                    _watcher = new ManagementEventWatcher(
                        new WqlEventQuery("SELECT ProcessID FROM Win32_ProcessStartTrace"));
                    _watcher.EventArrived += OnProcessStarted;
                    _watcher.Start();
                }
            }
        }

        public static void Stop()
        {
            lock (_syncRoot)
            {
                _watcher?.Stop();
                _watcher?.Dispose();
                _watcher = null;
                _currentMode = -1;
            }
        }

        private static void OnProcessStarted(object? _, EventArrivedEventArgs e)
        {
            if (e.NewEvent?.Properties["ProcessID"]?.Value is int pid)
            {
                try
                {
                    using var np = Process.GetProcessById(pid);
                    TrySetAffinity(np, _mask);
                }
                catch { /* permission or race; ignore */ }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TrySetAffinity(Process proc, ulong mask)
        {
            try { proc.ProcessorAffinity = (IntPtr)mask; }
            catch { /* safe to ignore */ }
        }

        private static ulong BuildMask(int mode)
        {
            int logical = (int)GetActiveProcessorCount(ALL_GROUPS);

            if (logical < 2) throw new NotSupportedException("Needs more than one logical processor.");
            if (logical > 64) throw new NotSupportedException("Only one processor group supported.");

            int half = logical / 2;

            return mode switch
            {
                0 => (1UL << logical) - 1,                               // all cores
                1 => (1UL << half) - 1,                                  // lower half
                2 => ((1UL << logical) - 1) ^ ((1UL << half) - 1),       // upper half
                _ => throw new ArgumentOutOfRangeException(nameof(mode), "Mode must be 0, 1, or 2.")
            };
        }

        private const uint ALL_GROUPS = 0xFFFF;
        [DllImport("kernel32.dll")] public static extern uint GetActiveProcessorCount(uint groupNumber);
    }
}
