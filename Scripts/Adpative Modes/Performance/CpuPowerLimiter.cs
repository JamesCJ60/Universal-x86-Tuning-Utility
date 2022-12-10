using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSmu;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UXTU.Scripts.Adpative_Modes.Performance
{
    public class CpuPowerLimiter
    {
        private int MaxPowerLimit = 125; // watts
        private int MinPowerLimit = 5; // watts
        private int MaxTemperature = 105; // degrees Celsius
        private const int PowerLimitIncrement = 1; // watts

        private int _newPowerLimit;
        private int _currentPowerLimit;
        private int _lastPowerLimit;

        public void UpdateLimits(int maxPower, int minPower, int maxTemp)
        {
            // Update max and min power limti and max temp limit
            MaxPowerLimit = maxPower;
            MinPowerLimit = minPower;
            MaxTemperature = maxTemp;
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
        public void GetCurrentPowerLimit()
        {
            if (Families.FAMID == 3 || Families.FAMID == 7)
            {
                _currentPowerLimit = (int)GetSensor.getSensorValve("PPT_LIMIT_FAST");
            }
            else if(Families.FAMID == 4 || Families.FAMID == 6)
            {
                _currentPowerLimit = (int)GetSensor.getSensorValve("PPT_LIMIT");
            }
        }

        public void UpdatePowerLimit(int temperature, int cpuLoad)
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

        private void UpdateTDP(int _newPowerLimit)
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
    }
}
