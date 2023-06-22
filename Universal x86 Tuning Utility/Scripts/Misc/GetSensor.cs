using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class GetSensor
    {
        public static void openSensor()
        {
            thisPC.Open();
            cpu = thisPC.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            amdGPU = thisPC.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd);
        }

        public static void updateSensor()
        {
            var hardware = thisPC.Hardware;
            
        }

        public static void closeSensor()
        {
            var cpu = thisPC.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            cpu.Update();
            var gpu = thisPC.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd);
            gpu.Update();
        }

        public static Computer thisPC = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
        };
        public static bool updateCPU = true;
        public static bool updateAMDGPU = true;

        private static IHardware cpu;
        public static float getCPUInfo(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                //Computer computer = new Computer
                //{
                //    IsCpuEnabled = true,
                //};
                //computer.Open();
                if (updateCPU)
                {
                    cpu.Update();
                    updateCPU = false;
                }
                var sensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == sensorType && s.Name.Contains(sensorName));
                if (sensor != null)
                {
                    value = (int)sensor.Value;
                    //computer.Close();
                    return value;
                }
                else
                {
                    //computer.Close();
                    return value;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return 0;
            }
        }
        private static IHardware amdGPU;
        public static float getAMDGPU(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                if (updateAMDGPU)
                {
                    amdGPU.Update();
                    updateAMDGPU = false;
                }
                var sensor = amdGPU.Sensors.FirstOrDefault(s => s.SensorType == sensorType && s.Name.Contains(sensorName));
                if (sensor != null)
                {
                    value = (int)sensor.Value;
                    //computer.Close();
                    return value;
                }
                else
                {
                    //computer.Close();
                    return value;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return 0;
            }
        }

        public static float getNVGPU(SensorType sensorType, string sensorName)
        {
            float value = 0;
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.GpuNvidia)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == sensorType && sensor.Name.Contains(sensorName))
                            {
                                value = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                }
                return value;
            }
            catch (Exception ex)
            {
                return value;
            }
        }
    }
}
