using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class PerfCounters
    {
        public static float? BatteryDischarge { get; private set; }
        public static void ReadSensors()
        {
            try
            {
                using (var cb = new PerformanceCounter("Power Meter", "Power", "Power Meter (0)", true))
                    BatteryDischarge = cb.NextValue() / 1000;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed reading sensors: {ex.Message}");
            }
        }

    }
}
