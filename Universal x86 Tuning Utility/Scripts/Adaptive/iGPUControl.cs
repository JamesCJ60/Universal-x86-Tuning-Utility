using HidSharp.Utility;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility.Scripts.Adaptive
{
    internal class iGPUControl
    {
        private static int _lastClock = 0; // mhz
        private static int _lastGpuUsage = 50; // %
        public static int _currentPowerLimit = 28;
        public static string commmand = "";
        private const int LastWindowSize = 8; // Number of samples in the sliding window
        private static Queue<int> gpuLastLoadSamples = new Queue<int>();
        private static double averageLastGpuLoad = 0.0;
        private const int WindowSize = 2; // Number of samples in the sliding window
        private static Queue<int> gpuLoadSamples = new Queue<int>();
        private static double averageGpuLoad = 0.0;
        public static async void UpdateiGPUClock(int maxClock, int minClock, int MaxTemperature, int _powerdraw, int _temperature, int _currentClock, int _gpuLoad, int memClock, int cpuClocks, int minCPUClock, double fps = 0, int fpsLimit = 0)
        {
            try
            {
                if (_lastClock <= 0) _lastClock = (int)(maxClock / 1.6);
                int newClock = _currentClock;

                if (_currentClock <= 0) _currentClock = _lastClock;
                if (averageLastGpuLoad <= 0) averageLastGpuLoad = _gpuLoad;
                if (averageGpuLoad <= 0) averageGpuLoad = _gpuLoad;

                gpuLoadSamples.Enqueue(_gpuLoad);

                // Remove oldest sample if the window is full
                if (gpuLoadSamples.Count > WindowSize)
                {
                    int oldestSample = gpuLastLoadSamples.Dequeue();
                    averageGpuLoad = ((averageGpuLoad * WindowSize) - oldestSample + _gpuLoad) / WindowSize;
                }
                else averageGpuLoad = ((averageGpuLoad * (gpuLoadSamples.Count - 1)) + _gpuLoad) / gpuLoadSamples.Count;

                _gpuLoad = (int)averageGpuLoad;

                if (_gpuLoad >= 87 && _gpuLoad <= 92 && _temperature <= MaxTemperature && memClock >= 550 && cpuClocks > minCPUClock) newClock = _lastClock;
                else
                {
                    // Remove oldest sample if the window is full
                    if (gpuLastLoadSamples.Count > LastWindowSize)
                    {
                        int oldestSample = gpuLastLoadSamples.Dequeue();
                        averageLastGpuLoad = ((averageLastGpuLoad * LastWindowSize) - oldestSample + _gpuLoad) / LastWindowSize;
                    }
                    else averageLastGpuLoad = ((averageLastGpuLoad * (gpuLastLoadSamples.Count - 1)) + _gpuLoad) / gpuLastLoadSamples.Count;

                    if ((int)averageLastGpuLoad <= 40 && _gpuLoad > 60 && _currentClock < 650 && cpuClocks >= minCPUClock && memClock > 550) newClock = (int)(maxClock / 1.6);

                    if (fps > 0 && fpsLimit > 0)
                    {
                        if (_gpuLoad > 92 && _temperature <= MaxTemperature && memClock >= 550 && cpuClocks > minCPUClock || fps < fpsLimit)
                        {
                            if (_currentClock < maxClock / 4) newClock = _currentClock + 75;
                            else if (_currentClock < maxClock / 3) newClock = _currentClock + 50;
                            else if (_currentClock < maxClock / 2) newClock = _currentClock + 35;
                            else if (_currentClock < maxClock / 1.33) newClock = _currentClock + 25;
                            else newClock = _currentClock + 25;
                        }
                        else if (_temperature > MaxTemperature || _gpuLoad < 87 || memClock < 550 || cpuClocks < minCPUClock || fps > fpsLimit)
                        {
                            if (_currentClock > minClock)
                            {
                                if (_currentClock > minClock && _gpuLoad > 50) newClock = _currentClock - 25;
                                else if (_currentClock > minClock && _gpuLoad < 20) newClock = _currentClock - 50;
                            }
                        }
                    }
                    else
                    {
                        if (_gpuLoad > 92 && _temperature <= MaxTemperature && memClock >= 550 && cpuClocks > minCPUClock)
                        {
                            if (_currentClock < maxClock / 4) newClock = _currentClock + 75;
                            else if (_currentClock < maxClock / 3) newClock = _currentClock + 50;
                            else if (_currentClock < maxClock / 2) newClock = _currentClock + 35;
                            else if (_currentClock < maxClock / 1.33) newClock = _currentClock + 25;
                            else newClock = _currentClock + 25;
                        }

                        else if (_temperature > MaxTemperature || _gpuLoad < 87 || memClock < 550 || cpuClocks < minCPUClock)
                        {
                            if (_currentClock > minClock)
                            {
                                if (_currentClock > minClock && _gpuLoad > 50) newClock = _currentClock - 25;
                                else if (_currentClock > minClock && _gpuLoad < 20) newClock = _currentClock - 50;
                            }
                        }
                    }

                }

                if (_currentClock > maxClock) newClock = maxClock - 10;
                if (_currentClock < minClock) newClock = minClock + 10;

                if (newClock <= (_lastClock - 15) && newClock > 0 || newClock >= (_lastClock + 15) && newClock > 0)
                {
                    commmand = $"--gfx-clk={newClock} ";
                    _lastClock = newClock;
                }

                gpuLastLoadSamples.Enqueue(_gpuLoad);
                _lastGpuUsage = (int)_gpuLoad;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

        }
    }
}

