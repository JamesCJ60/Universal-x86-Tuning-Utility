using System;
using System.Management;

namespace Universal_x86_Tuning_Utility.Services;

public interface IGraphicsHardwareService
{
    int CountRadeonGpus();
    int CountNvidiaGpus();
    void CheckOriginality();
}

public sealed class GraphicsHardwareService : IGraphicsHardwareService
{
    public int CountRadeonGpus() => Count("Radeon");
    public int CountNvidiaGpus() => Count("NVIDIA");
    public void CheckOriginality() => Scripts.GPUs.NVIDIA.NvHwCheck.CheckROPCount();

    private static int Count(string manufacturer)
    {
        using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
        using var controllers = searcher.Get();
        var count = 0;
        foreach (ManagementObject controller in controllers)
        {
            using (controller)
                if (controller["Name"] is string name && name.Contains(manufacturer, StringComparison.OrdinalIgnoreCase)) count++;
        }
        return count;
    }
}
