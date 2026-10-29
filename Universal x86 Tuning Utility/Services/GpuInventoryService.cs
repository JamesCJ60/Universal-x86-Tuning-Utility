using System;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Services
{
    public readonly record struct GpuInventorySnapshot(int RadeonCount, int NvidiaCount);

    public sealed class GpuInventoryService
    {
        private readonly SemaphoreSlim _sync = new(1, 1);
        private GpuInventorySnapshot? _snapshot;

        public async Task<GpuInventorySnapshot> GetSnapshotAsync()
        {
            if (_snapshot.HasValue)
                return _snapshot.Value;

            await _sync.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_snapshot.HasValue)
                    return _snapshot.Value;

                _snapshot = await Task.Run(ReadSnapshot).ConfigureAwait(false);
                return _snapshot.Value;
            }
            finally
            {
                _sync.Release();
            }
        }

        private static GpuInventorySnapshot ReadSnapshot()
        {
            try
            {
                int radeonCount = 0;
                int nvidiaCount = 0;
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (ManagementObject adapter in searcher.Get())
                {
                    string name = adapter["Name"]?.ToString() ?? string.Empty;
                    if (name.Contains("Radeon", StringComparison.OrdinalIgnoreCase)) radeonCount++;
                    if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) nvidiaCount++;
                }

                return new GpuInventorySnapshot(radeonCount, nvidiaCount);
            }
            catch
            {
                return default;
            }
        }
    }
}
