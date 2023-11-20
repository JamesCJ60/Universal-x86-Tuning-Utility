using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using NvAPIWrapper.Native.Interfaces.GPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NvAPIWrapper.Native.GPU.Structures.PerformanceStates20InfoV1;

namespace Universal_x86_Tuning_Utility.Scripts.GPUs.NVIDIA
{
    internal class NvTuning
    {
        public const int MinCoreOffset = -900, MinMemoryOffset = -900, MaxCoreOffset = 2000, MaxMemoryOffset = 2000;

        private static PhysicalGPU? _internalGpu;

        public static int SetClocks(int core, int memory, int voltage = 0)
        {
            if (core < MinCoreOffset || core > MaxCoreOffset || memory < MinMemoryOffset || memory > MaxMemoryOffset) return 0;

            PhysicalGPU internalGpu = PhysicalGPU.GetPhysicalGPUs().First();

            if (internalGpu == null) return -1;

            try
            {
                var coreClock = new PerformanceStates20ClockEntryV1(PublicClockDomain.Graphics, new PerformanceStates20ParameterDelta(core * 1000));
                var memoryClock = new PerformanceStates20ClockEntryV1(PublicClockDomain.Memory, new PerformanceStates20ParameterDelta(memory * 1000));
                //var voltageEntry = new PerformanceStates20BaseVoltageEntryV1(PerformanceVoltageDomain.Core, new PerformanceStates20ParameterDelta(voltage));
                
                PerformanceStates20ClockEntryV1[] clocks = { coreClock, memoryClock };
                PerformanceStates20BaseVoltageEntryV1[] voltages = { };

                PerformanceState20[] performanceStates = { new PerformanceState20(PerformanceStateId.P0_3DPerformance, clocks, voltages) };

                var overclock = new PerformanceStates20InfoV1(performanceStates, 2, 0);

                GPUApi.SetPerformanceStates20(internalGpu.Handle, overclock);
            }
            catch (Exception ex)
            {
                return -1;
            }

            return 1;
        }

    }
}
