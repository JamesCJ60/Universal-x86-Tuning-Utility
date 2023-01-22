using AATUV3.Scripts.SMU_Backend_Scripts;
using Microsoft.Win32;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using UXTU.Scripts.ADLX;

namespace UXTU.Scripts.Adpative_Modes.Performance
{
    internal class iGPUClockAdjust
    {
        private const int ClockIncrement = 50; // mhz

        private static int _newClock = 0; // mhz
        private static int _currentClock = 0; // mhz
        private static int _lastClock = 1000; // mhz
        private static int _lastGpuUsage = 0; // %
        private static int _gpuFPS = 0;
        private static int _currentPowerUsage = 0; // watt
        private static int _gpuLoad = 100; // %
        private static int _temperature = 0; // °C

        private static async void GetGPULoad()
        {
            await Task.Run(() =>
            {
                try
                {
                    _gpuLoad = (int)ADLXBackend.GetGPUMetrics(0, 7);
                    _gpuFPS = (int)ADLXBackend.GetFPSData();
                    _currentClock = (int)ADLXBackend.GetGPUMetrics(0, 0);
                    _temperature = (int)ADLXBackend.GetGPUMetrics(0, 3);
                }
                catch (Exception ex)
                {

                }
            });
        }

        private static void GetPower()
        {
            if (Families.FAMID == 3 || Families.FAMID == 7)
            {
                _currentPowerUsage = (int)GetSensor.getSensorValve("SOCKET_POWER");
            }
            else
            {
                decimal STAPM = GetSensor.getSensorValueRAdj("STAPM VALUE");
                decimal Slow = GetSensor.getSensorValueRAdj("PPT VALUE SLOW");
                decimal Fast = GetSensor.getSensorValueRAdj("PPT VALUE FAST");

                _currentPowerUsage = Convert.ToInt32(Math.Max(Math.Max(STAPM, Slow), Fast));
            }
        }

        public static void UpdateiGPUClock(int maxClock, int minClock, int MaxTemperature)
        {
            GetGPULoad();
            GetPower();

            if (maxClock < 2000 && _gpuLoad > (_lastGpuUsage + 35) && _currentClock < (maxClock / 1.35)) _newClock = (int)(maxClock / 1.35);
            if (maxClock >= 2000 && _gpuLoad > (_lastGpuUsage + 35) && _currentClock < (maxClock / 1.55)) _newClock = (int)(maxClock / 1.55);

            if (_temperature >= MaxTemperature - 2 || _gpuLoad < 85 || _currentPowerUsage > (CpuPowerLimiter._currentPowerLimit - 2))
            {
                // Reduce clock if temperature is too high or usage is too low or power is too high
                _newClock = Math.Max(minClock, _newClock - ClockIncrement);
            }
            else if (_gpuLoad > 90 && _temperature <= (MaxTemperature - 5))
            {
                // Increase clock if temperature allows and GPU load is high
                _newClock = Math.Min(maxClock, minClock + ClockIncrement);
            }

            // Apply new clock if clock has changed
            if (_newClock != _lastClock + 50 || _newClock != _lastClock - 50)
            {
                // Detect if AMD CPU or APU
                if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 8)
                {
                    //Apply new clock
                    SendCommand.set_gfx_clk((uint)_newClock);
                }
                else if (Families.FAMID == 0 || Families.FAMID == 1)
                {
                    //Apply new clock limits
                    SendCommand.set_max_gfxclk_freq((uint)_newClock);
                    SendCommand.set_min_gfxclk_freq((uint)_newClock - 200);
                }

                _lastClock = _newClock;
                _lastGpuUsage = _gpuLoad;
            }
        }
    }
}
