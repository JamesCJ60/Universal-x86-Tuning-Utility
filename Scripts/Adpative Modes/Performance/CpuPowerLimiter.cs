using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSmu;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UXTU.Properties;

namespace UXTU.Scripts.Adpative_Modes.Performance
{
    public class CpuPowerLimiter
    {
        private static int MinCurveOptimiser = 0; // CO
        private const int PowerLimitIncrement = 2; // watts
        private const int CurveOptimiserIncrement = 1; // CO

        private static int _newPowerLimit; // watts
        private static int _currentPowerLimit; // watts
        private static int _newCO; // CO
        private static int _lastPowerLimit = 1000; // watts
        private static int _lastCO = 0; // CO


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
        public static void GetCurrentPowerLimit(int MaxPowerLimit, int MinPowerLimit, int MaxTemperature)
        {
            if (_newPowerLimit == 0) _newPowerLimit = MaxPowerLimit;

            // Get current fast PPT from APU
            if (Families.FAMID == 3 || Families.FAMID == 7)
            {
                _currentPowerLimit = (int)GetSensor.getSensorValve("PPT_LIMIT_FAST");

                if (Settings.Default.CPUName.Contains("G")) MinPowerLimit = 10;
                else MinPowerLimit = 5;
            }
            // Get Current PPT from CPU
            else if(Families.FAMID == 4 || Families.FAMID == 6)
            {
                _currentPowerLimit = (int)GetSensor.getSensorValve("PPT_LIMIT");
                MinPowerLimit = 30;
            }

            if (_newPowerLimit == 0) _newPowerLimit = _currentPowerLimit;
        }

        public static void UpdatePowerLimit(int temperature, int cpuLoad, int MaxPowerLimit, int MinPowerLimit, int MaxTemperature)
        {
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

            // Apply new power limit if power limit has changed
            if (_newPowerLimit != _lastPowerLimit || _currentPowerLimit != _newPowerLimit)
            {

                // Turn power limit into mW
                uint _TDP = Convert.ToUInt32(_newPowerLimit * 1000);

                // Detect if AMD CPU or APU
                if (Families.FAMID == 3 || Families.FAMID == 7)
                {
                    // Apply new power and temp limit
                    SendCommand.set_tctl_temp((uint)MaxTemperature);
                    SendCommand.set_apu_skin_temp_limit((uint)MaxTemperature);
                    SendCommand.set_fast_limit(_TDP);
                    SendCommand.set_slow_limit(_TDP);
                    SendCommand.set_stapm_limit(_TDP);
                    SendCommand.set_slow_time(64);
                    SendCommand.set_stapm_time(1024);
                    SendCommand.set_vrm_current((uint)(_TDP * 1.33));
                    SendCommand.set_vrmmax_current((uint)(_TDP * 1.33));
                }
                else if (Families.FAMID == 4 || Families.FAMID == 6)
                {
                    // Apply new power and temp limit
                    SendCommand.set_tctl_temp((uint)MaxTemperature);
                    SendCommand.set_ppt((uint)_TDP);
                    SendCommand.set_edc((uint)(_TDP * 1.33));
                    SendCommand.set_tdc((uint)(_TDP * 1.33));
                }

                // Save new TDP to avoid unnecessary reapplies
                _lastPowerLimit = _newPowerLimit;
            }
        }

        private static int prevCpuLoad = -1;
        public static void CurveOptimiserLimit(int cpuLoad, int MaxCurveOptimiser)
        {
            int newMaxCO = MaxCurveOptimiser;


            // Change max CO limit based on CPU usage
            if(cpuLoad < 10) newMaxCO = MaxCurveOptimiser;
            else if (cpuLoad >= 10 && cpuLoad < 80) newMaxCO = MaxCurveOptimiser - CurveOptimiserIncrement * 2;
            else if (cpuLoad >= 80) newMaxCO = MaxCurveOptimiser - CurveOptimiserIncrement * 3;

            if (_lastCO == 0 && prevCpuLoad <= 0) _lastCO = newMaxCO;
            if (prevCpuLoad < 0) prevCpuLoad = 100;

            // Decrease the number by 10 if the CPU load is increased by 10
            if (cpuLoad > prevCpuLoad + 10)
            {
                _newCO = _lastCO - CurveOptimiserIncrement;

                // Store the current CPU load for the next iteration
                prevCpuLoad = prevCpuLoad + 10;
            }
            // Increase the number by 10 if the CPU load is decreased by 10
            else if (cpuLoad < prevCpuLoad - 10)
            {
                _newCO = _lastCO + CurveOptimiserIncrement;

                // Store the current CPU load for the next iteration
                prevCpuLoad = prevCpuLoad - 10;
            }

            // Make sure min and max CO is not exceeded
            if(_newCO <= MinCurveOptimiser) _newCO = MinCurveOptimiser;
            if (_newCO >= newMaxCO) _newCO = newMaxCO;

            // Make sure CO is within CO max limit + 5
            if(_newCO > 35) _newCO = 35;

            // Apply new CO
            if (_newCO != _lastCO) UpdateCO(_newCO);
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
