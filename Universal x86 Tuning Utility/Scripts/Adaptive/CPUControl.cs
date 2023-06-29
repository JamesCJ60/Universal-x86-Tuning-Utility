using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Universal_x86_Tuning_Utility.Scripts.Adaptive
{
    internal class CPUControl
    {
        private static int MinCurveOptimiser = 0; // CO
        private const int PowerLimitIncrement = 2; // watts
        private const int CurveOptimiserIncrement = 1; // CO

        private static int _newPowerLimit; // watts
        public static int _currentPowerLimit; // watts
        private static int _newCO; // CO
        private static int _lastPowerLimit = 1000; // watts
        private static int _lastCO = 0; // CO
        public static int _lastUsage = 0;


        public static string cpuCommand = "";
        public static string coCommand = "";
        public static async void UpdatePowerLimit(int temperature, int cpuLoad, int MaxPowerLimit, int MinPowerLimit, int MaxTemperature)
        {
            try { 
            if (temperature >= MaxTemperature - 2)
            {
                // Reduce power limit if temperature is too high
                _newPowerLimit = Math.Max(MinPowerLimit, _newPowerLimit - PowerLimitIncrement);
            }
            else if (cpuLoad > 10 && temperature <= (MaxTemperature - 5))
            {
                // Increase power limit if temperature allows and CPU load is high
                _newPowerLimit = Math.Min(MaxPowerLimit, _newPowerLimit + PowerLimitIncrement);
            }

            if (_newPowerLimit < MinPowerLimit) _newPowerLimit = MinPowerLimit;
            if (_newPowerLimit > MaxPowerLimit) _newPowerLimit = MaxPowerLimit;

                // Apply new power limit if power limit has changed
                if (_newPowerLimit <= _lastPowerLimit - 1 || _newPowerLimit >= _lastPowerLimit + 1)
                {
                    int _TDP = _newPowerLimit;

                    // Detect if AMD CPU or APU
                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        _TDP = _newPowerLimit * 1000;

                        if (_TDP >= 5000)
                        {
                            // Apply new power and temp limit
                            cpuCommand = $"--tctl-temp={MaxTemperature} --cHTC-temp={MaxTemperature} --apu-skin-temp={MaxTemperature} --stapm-limit={_TDP}  --fast-limit={_TDP} --stapm-time=64 --slow-limit={_TDP} --slow-time=128 --vrm-current=300000 --vrmmax-current=300000 --vrmsoc-current=300000 --vrmsocmax-current=300000 ";
                            // Save new TDP to avoid unnecessary reapplies
                            _lastPowerLimit = _newPowerLimit;
                            iGPUControl._currentPowerLimit = _newPowerLimit;
                        }

                    }

                    else if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                    {
                        _TDP = _newPowerLimit;
                        // Apply new power and temp limit

                        cpuCommand = $"--tctl-temp={MaxTemperature} --ppt-limit={_TDP} --edc-limit={(_TDP * 1.33)} --tdc-limit={(_TDP * 1.33)} ";
                        _lastPowerLimit = _newPowerLimit;
                        iGPUControl._currentPowerLimit = _newPowerLimit;
                    }

                    else if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        _TDP = _newPowerLimit;
                        // Apply new power and temp limit

                        cpuCommand = $"--intel-pl={_newPowerLimit}";
                        _lastPowerLimit = _newPowerLimit;
                    }
                } 
            } catch { }


            _lastUsage = cpuLoad;
        }

        private static int prevCpuLoad = -1;
        public static void CurveOptimiserLimit(int cpuLoad, int MaxCurveOptimiser)
        {
            try
            {
                int newMaxCO = MaxCurveOptimiser;

                // Change max CO limit based on CPU usage
                if (cpuLoad < 10) newMaxCO = MaxCurveOptimiser;
                else if (cpuLoad >= 10 && cpuLoad < 80) newMaxCO = MaxCurveOptimiser - CurveOptimiserIncrement * 2;
                else if (cpuLoad >= 80) newMaxCO = MaxCurveOptimiser;

                if (_lastCO == 0 && prevCpuLoad <= 0) _lastCO = newMaxCO;
                if (prevCpuLoad < 0) prevCpuLoad = 100;

                // Increase CO if the CPU load is increased by 10
                if (cpuLoad > prevCpuLoad + 10)
                {
                    _newCO = _lastCO + CurveOptimiserIncrement;

                    // Store the current CPU load for the next iteration
                    prevCpuLoad = prevCpuLoad + 10;
                }
                // Decrease CO if the CPU load is decreased by 10
                else if (cpuLoad < prevCpuLoad - 10)
                {
                    _newCO = _lastCO - CurveOptimiserIncrement;

                    // Store the current CPU load for the next iteration
                    prevCpuLoad = prevCpuLoad - 10;
                }

                // Make sure min and max CO is not exceeded
                if (_newCO <= MinCurveOptimiser) _newCO = MinCurveOptimiser;
                if (_newCO >= newMaxCO) _newCO = newMaxCO;

                // Make sure CO is within CO max limit + 5
                if (_newCO > 55) _newCO = 55;

                if (cpuLoad < 5) _newCO = 0;

                if (cpuLoad > 80) _newCO = MaxCurveOptimiser;

                // Apply new CO
                if (_newCO != _lastCO) UpdateCO(_newCO);
            } catch { }
        }

        private static async void UpdateCO(int _newCO)
        {
            // Apply new CO
            if (_newCO > 0) coCommand = $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(_newCO))} ";
            else coCommand = $"--set-coall={0} ";

            // Save new CO to avoid unnecessary reapplies
            _lastCO = _newCO;
        }
    }
}
