using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSmu;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UXTU.Scripts.Adpative_Modes.Performance
{
    public class CpuPowerLimiter
    {
        private static int MaxPowerLimit = 125; // watts
        private static int MinPowerLimit = 5; // watts
        private static int MaxCurveOptimiser = 30; // CO
        private static int MinCurveOptimiser = 0; // CO
        private static int MaxTemperature = 105; // degrees Celsius
        private const int PowerLimitIncrement = 1; // watts
        private const int CurveOptimiserIncrement = 1; // CO

        private static int _newPowerLimit;
        private static int _currentPowerLimit;
        private static int _newCO;
        private static int _lastPowerLimit;
        private static int _lastCO;

        public static void UpdateLimits(int maxPower, int minPower, int maxTemp, int maxCO)
        {
            // Update max and min power limti and max temp limit
            MaxPowerLimit = maxPower;
            MinPowerLimit = minPower;
            MaxTemperature = maxTemp;
            MaxCurveOptimiser = maxCO;
        }

        //Zen1/+ - -1
        //RAVEN - 0
        //PICASSO - 1
        //DALI - 2
        //RENOIR/LUCIENNE - 3
        //MATISSE - 4
        //VANGOGH - 5
        //VERMEER - 6
        //CEZANNE/BARCELO - 7
        //REMBRANDT - 8
        //PHEONIX - 9
        //RAPHAEL/DRAGON RANGE - 10
        public static void GetCurrentPowerLimit()
        {
            // Get current fast PPT from APU
            if (Families.FAMID == 3 || Families.FAMID == 7)
            {
                _currentPowerLimit = (int)GetSensor.getSensorValve("PPT_LIMIT_FAST");
            }
            // Get Current PPT from CPU
            else if(Families.FAMID == 4 || Families.FAMID == 6)
            {
                _currentPowerLimit = (int)GetSensor.getSensorValve("PPT_LIMIT");
            }
        }

        public static void UpdatePowerLimit(int temperature, int cpuLoad)
        {
            if (temperature > MaxTemperature)
            {
                // Reduce power limit if temperature is too high
                _newPowerLimit = Math.Max(MinPowerLimit, _newPowerLimit - PowerLimitIncrement);
            }
            else if (cpuLoad > 10 && temperature <= (MaxTemperature - 5))
            {
                // Increase power limit if temperature allows and CPU load is high
                _newPowerLimit = Math.Min(MaxPowerLimit, _newPowerLimit + PowerLimitIncrement);
            }

            // Apply new power limit if power limit has changed
            if(_newPowerLimit != _lastPowerLimit / 1000 || _currentPowerLimit != _newPowerLimit) UpdateTDP(_newPowerLimit);
        }

        private static int prevCpuLoad = 0;
        public static void CurveOptimiserLimit(int cpuLoad)
        {
            int newMaxCO = MaxCurveOptimiser;

            // Change max CO limit based on CPU usage
            if(cpuLoad < 10) newMaxCO = MaxCurveOptimiser;
            else if(cpuLoad > 10 && cpuLoad < 40) newMaxCO = MaxCurveOptimiser - CurveOptimiserIncrement * 4;
            else if (cpuLoad >= 40 && cpuLoad < 80) newMaxCO = MaxCurveOptimiser - CurveOptimiserIncrement * 6;
            else if (cpuLoad >= 80) newMaxCO = MaxCurveOptimiser - CurveOptimiserIncrement * 8;

            // Decrease the number by 10 if the CPU load is increased by 10
            if (cpuLoad > prevCpuLoad + 10)
            {
                _newCO -= CurveOptimiserIncrement;
            }
            // Increase the number by 10 if the CPU load is decreased by 10
            else if (cpuLoad < prevCpuLoad - 10)
            {
                _newCO += CurveOptimiserIncrement;
            }

            // Make sure min and max CO is not exceeded
            if(_newCO <= MinCurveOptimiser) _newCO = MinCurveOptimiser;
            if (_newCO >= newMaxCO) _newCO = newMaxCO;

            // Make sure CO is within CO max limit
            if(_newCO > 30) _newCO = 30;

            // Store the current CPU load for the next iteration
            prevCpuLoad = cpuLoad;

            // Apply new CO
            if (_newCO != _lastCO) UpdateCO(_newCO);
        }

        private static void UpdateTDP(int _newPowerLimit)
        {
            // Turn power limit into mW
            _newPowerLimit = _newPowerLimit * 1000;

            // Detect if AMD CPU or APU
            if (Families.FAMID == 3 || Families.FAMID == 7)
            {
                // Apply new power and temp limit
                SendCommand.set_tctl_temp((uint)MaxTemperature);
                SendCommand.set_apu_skin_temp_limit((uint)MaxTemperature);
                SendCommand.set_fast_limit((uint)_newPowerLimit);
                SendCommand.set_slow_limit((uint)_newPowerLimit);
                SendCommand.set_stapm_limit((uint)_newPowerLimit);
                SendCommand.set_slow_time(64);
                SendCommand.set_fast_limit(1024);
                SendCommand.set_vrm_current((uint)(_newPowerLimit * 1.33));
                SendCommand.set_vrmmax_current((uint)(_newPowerLimit * 1.33));
            }
            else if (Families.FAMID == 4 || Families.FAMID == 6)
            {
                // Apply new power and temp limit
                SendCommand.set_tctl_temp((uint)MaxTemperature);
                SendCommand.set_ppt((uint)_newPowerLimit);
                SendCommand.set_edc((uint)(_newPowerLimit * 1.33));
                SendCommand.set_tdc((uint)(_newPowerLimit * 1.33));
            }

            // Save new TDP to avoid unnecessary reapplies
            _lastPowerLimit = _newPowerLimit;
        }

        private static void UpdateCO(int _newCO)
        {
            // Apply new CO
            SendCommand.set_coall(Convert.ToUInt32(0x100000 - (uint)(_newCO)));

            // Save new CO to avoid unnecessary reapplies
            _lastCO = _newCO;
        }
    }
}
