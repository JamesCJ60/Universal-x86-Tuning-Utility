using System;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    class Garbage
    {
        private const long CollectionThreshold = 512L * 1024L * 1024L;

        public static Task Garbage_Collect()
        {
            try
            {
                if (GC.GetTotalMemory(false) < CollectionThreshold)
                    return Task.CompletedTask;

                GC.Collect(2, GCCollectionMode.Optimized, false, false);
            }
            catch (Exception ex)
            {
                DiagnosticLogger.LogError(ex, "Failed to collect garbage");
            }

            return Task.CompletedTask;
        }
    }
}
