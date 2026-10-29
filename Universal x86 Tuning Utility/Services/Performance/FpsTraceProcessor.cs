using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Universal_x86_Tuning_Utility.Services.Performance
{
    internal sealed class FpsTraceProcessor : IDisposable
    {
        private static readonly Guid DxgiProvider = Guid.Parse("{CA11C036-0102-4A2D-A6AD-F03CFED5D3C9}");
        private static readonly Guid DxgKrnlProvider = Guid.Parse("{802EC45A-1E99-4B83-9920-87C98277BA9D}");
        private const string Win32kProvider = "Microsoft-Windows-Win32k";
        private const ulong DxgKrnlPresentKeyword = 0x0000040000000000UL;
        private const ulong Win32kGraphicsKeyword = 0x0000000000080000UL;
        private readonly ConcurrentDictionary<int, FrameTimestampBuffer> _frames = new();
        private readonly ConcurrentDictionary<int, double> _lastTimestamp = new();
        private TraceEventSession? _session;
        private Thread? _traceThread;
        private int _targetProcessId;

        public double ElapsedMilliseconds => _session?.Source?.SessionStartTime is null
            ? 0
            : (DateTime.UtcNow - _session.Source.SessionStartTime.ToUniversalTime()).TotalMilliseconds;

        public int TargetProcessId
        {
            get => Volatile.Read(ref _targetProcessId);
            set => Volatile.Write(ref _targetProcessId, value);
        }

        public void Start()
        {
            if (_session is not null)
                return;

            _session = new TraceEventSession($"UXTU-FPS-{Environment.ProcessId}") { StopOnDispose = true };
            _session.EnableProvider(DxgiProvider, TraceEventLevel.Informational, ulong.MaxValue);
            _session.EnableProvider(DxgKrnlProvider, TraceEventLevel.Informational, DxgKrnlPresentKeyword);
            try
            {
                _session.EnableProvider(Win32kProvider, TraceEventLevel.Verbose, Win32kGraphicsKeyword);
            }
            catch
            {
            }
            _session.Source.AllEvents += OnEvent;
            _traceThread = new Thread(() => _session.Source.Process())
            {
                IsBackground = true,
                Name = "UXTU FPS trace"
            };
            _traceThread.Start();
        }

        public FrameStatistics GetStatistics(int processId)
        {
            if (processId <= 0 || !_frames.TryGetValue(processId, out FrameTimestampBuffer? buffer))
                return default;

            return buffer.Calculate(ElapsedMilliseconds);
        }

        public void ClearProcess(int processId)
        {
            _frames.TryRemove(processId, out _);
            _lastTimestamp.TryRemove(processId, out _);
        }

        private void OnEvent(TraceEvent data)
        {
            int targetPid = TargetProcessId;
            if (targetPid <= 0 || data.ProcessID != targetPid)
                return;

            bool isDxgi = data.ProviderGuid == DxgiProvider && (int)data.ID == 42;
            bool isKernelPresent = data.ProviderGuid == DxgKrnlProvider &&
                (data.TaskName?.Contains("Present", StringComparison.OrdinalIgnoreCase) == true ||
                 data.TaskName?.Contains("Flip", StringComparison.OrdinalIgnoreCase) == true ||
                 data.EventName?.Contains("Present", StringComparison.OrdinalIgnoreCase) == true);
            bool isOpenGlPresent = data.ProviderName?.Contains("Win32k", StringComparison.OrdinalIgnoreCase) == true &&
                (data.TaskName?.Contains("Swap", StringComparison.OrdinalIgnoreCase) == true ||
                 data.TaskName?.Contains("Present", StringComparison.OrdinalIgnoreCase) == true ||
                 data.EventName?.Contains("Swap", StringComparison.OrdinalIgnoreCase) == true);

            if (!isDxgi && !isKernelPresent && !isOpenGlPresent)
                return;

            FrameTimestampBuffer buffer = _frames.GetOrAdd(targetPid, _ => new FrameTimestampBuffer());
            if (isDxgi)
                buffer.HasDxgiEvents = true;
            else if (buffer.HasDxgiEvents)
                return;

            double timestamp = data.TimeStampRelativeMSec;
            if (_lastTimestamp.TryGetValue(targetPid, out double last) && timestamp - last < 0.2)
                return;

            _lastTimestamp[targetPid] = timestamp;
            buffer.Add(timestamp);
        }

        public void Stop()
        {
            TraceEventSession? session = Interlocked.Exchange(ref _session, null);
            if (session is null)
                return;

            session.Source.AllEvents -= OnEvent;
            session.Dispose();
            Thread? traceThread = Interlocked.Exchange(ref _traceThread, null);
            if (traceThread is not null && traceThread != Thread.CurrentThread)
                traceThread.Join(TimeSpan.FromSeconds(1));
            _frames.Clear();
            _lastTimestamp.Clear();
            TargetProcessId = 0;
        }

        public void Dispose() => Stop();
    }
}
