using Microsoft.Extensions.Logging;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using NvAPIWrapper.Native.Interfaces.GPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NvAPIWrapper.Native.GPU.Structures.PerformanceStates20InfoV1;

namespace Universal_x86_Tuning_Utility.Scripts.GPUs.NVIDIA
{
    internal class NvTuning
    {
        public const int MinCoreOffset = -900, MinMemoryOffset = -900, MaxCoreOffset = 2000, MaxMemoryOffset = 2000, MinClockLimit = 400, MaxClockLimit = 4000;

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

        public static int SetMaxGPUClock(int clock)
        {

            if (clock < MinClockLimit || clock >= MaxClockLimit) clock = 0;

            int _clockLimit = GetMaxGPUCLock();

            if (_clockLimit != clock)
            {
                if (clock > 0) RunPowershellCommand($"nvidia-smi -lgc 0,{clock}");
                else RunPowershellCommand($"nvidia-smi -rgc");
                return 1;
            }
            else
            {
                return 0;
            }


        }
        public static int GetMaxGPUCLock()
        {
            PhysicalGPU internalGpu = PhysicalGPU.GetPhysicalGPUs().First();
            try
            {
                PrivateClockBoostLockV2 data = GPUApi.GetClockBoostLock(internalGpu.Handle);
                int limit = (int)data.ClockBoostLocks[0].VoltageInMicroV / 1000;
                return limit;
            }
            catch (Exception ex)
            {
                return -1;

            }
        }
        private static bool RunPowershellCommand(string script)
        {
            try
            {
                RunCMD("powershell", script);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void RunCMD(string name, string args)
        {
            var cmd = new Process();
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.FileName = name;
            cmd.StartInfo.Arguments = args;
            cmd.Start();

            string result = cmd.StandardOutput.ReadToEnd().Replace(Environment.NewLine, " ").Trim(' ');

            cmd.WaitForExit();
        }
    }
}
