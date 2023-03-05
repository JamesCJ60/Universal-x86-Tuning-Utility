using AATUV3.Scripts.SMU_Backend_Scripts;
using RyzenSmu;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UXTU.Properties;
using UXTU.Scripts.ADLX;

namespace UXTU.Scripts.Adpative_Modes.Performance
{
    public class CpuPowerLimiter
    {
        private static int MinCurveOptimiser = 0; // CO
        private const int PowerLimitIncrement = 2; // watts
        private const int CurveOptimiserIncrement = 1; // CO

        private static int _newPowerLimit; // watts
        public static int _currentPowerLimit; // watts
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
            else
            {
                decimal STAPM = GetSensor.getSensorValueRAdj("STAPM LIMIT");
                decimal Slow = GetSensor.getSensorValueRAdj("PPT LIMIT FAST");
                decimal Fast = GetSensor.getSensorValueRAdj("PPT LIMIT SLOW");

                _currentPowerLimit = Convert.ToInt32((Fast + Slow + STAPM) / 3);
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
            if (_newPowerLimit <= _lastPowerLimit - 1 && _newPowerLimit >= _lastPowerLimit + 1 || _currentPowerLimit != _newPowerLimit)
            {

                // Turn power limit into mW
                uint _TDP = Convert.ToUInt32(_newPowerLimit * 1000);

                // Detect if AMD CPU or APU
                if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 8)
                {
                    if(_TDP >= 5000)
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

        private static int time = 200;
        public static async void updateBattery(int _powerMode, int _cpuLoad, int _coresInUse)
        {
            await Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow = true;

                    int i = 0;
                    if (_powerMode == 0)
                    {
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive 961CC777-2547-4F9D-8174-7D86181b8A7A";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor PERFBOOSTMODE 3";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor PERFBOOSTMODE 3";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor CPMINCORES 25";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor CPMINCORES 25";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor DISTRIBUTEUTIL 1";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor DISTRIBUTEUTIL 1";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        i++;
                    }
                    else if (_powerMode == 1)
                    {
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive 381B4222-F694-41F0-9685-FF5BB260DF2E";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor PERFBOOSTMODE 1";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor PERFBOOSTMODE 1";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor CPMINCORES 75";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor CPMINCORES 75";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor DISTRIBUTEUTIL 1";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor DISTRIBUTEUTIL 1";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        i++;
                    }
                    else if (_powerMode == 3)
                    {
                        SendCommand.set_max_performance(0);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive DED574B5-45A0-4F42-8737-46345C09C238";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor PERFBOOSTMODE 4";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setdcvalueindex scheme_current sub_processor PERFBOOSTMODE 4";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setacvalueindex scheme_current sub_processor CPMINCORES 100";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "-setdcvalueindex scheme_current sub_processor CPMINCORES 100";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "-setacvalueindex scheme_current sub_processor DISTRIBUTEUTIL 0";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(time);

                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "-setdcvalueindex scheme_current sub_processor DISTRIBUTEUTIL 0";
                        process.StartInfo = startInfo;
                        process.Start();

                        System.Threading.Thread.Sleep(time);
                        startInfo.FileName = "powercfg";
                        startInfo.Arguments = "/setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        i++;

                        updateClock(_powerMode, _cpuLoad, _coresInUse);
                    }
                } catch(Exception ex) { MessageBox.Show(ex.Message); }
            });
        }
       public static int lastClock = 100;
        public static async void updateClock(int _powerMode, int _cpuLoad, int _coresInUse)
        {
            await Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow = true;
                    int gpuLoad = (int)ADLXBackend.GetGPUMetrics(0, 7);
                    int gpuFPS = (int)ADLXBackend.GetFPSData();
                    int gpuClock =(int)ADLXBackend.GetGPUMetrics(0, 0);
                    int maxBaseClock = 0;
                    int maxClock = 100;
                    int minClock = 20;
                    int minState = 0;
                    int newClock = 100;
                    int cpuIncrement = 1;

                    try
                    {
                        ManagementObjectSearcher searcher =
                            new ManagementObjectSearcher("root\\CIMV2",
                            "SELECT * FROM Win32_Processor");

                        foreach (ManagementObject queryObj in searcher.Get())
                        {
                            maxBaseClock = Convert.ToInt32(queryObj["MaxClockSpeed"]);
                        }
                    }
                    catch (ManagementException e)
                    {

                    }


                    if (_powerMode == 0)
                    {
                        maxClock = 99;
                        minState = 0;

                        if (_coresInUse == 1 && _cpuLoad < 65 || _coresInUse == 2 && _cpuLoad < 65)
                        {
                            newClock = 99;
                        }
                        else if (_coresInUse > 2 | _coresInUse < 1)
                        {
                            if (gpuFPS > 0 && gpuLoad > 95 && gpuClock < 1000 && _cpuLoad < 90)
                            {
                                newClock = lastClock - cpuIncrement;
                            }
                            else if (gpuFPS > 0 &&  gpuLoad > 95 && _cpuLoad < 90)
                            {
                                newClock = lastClock - cpuIncrement;
                            }
                            else if (_cpuLoad < 90)
                            {
                                newClock = lastClock - cpuIncrement;
                            }

                            if (gpuFPS > 0 && gpuLoad < 90 && _cpuLoad >= 95)
                            {
                                newClock = lastClock + cpuIncrement;
                            }
                            else if (_cpuLoad > 95)
                            {
                                newClock = lastClock + cpuIncrement;
                            }

                            if (newClock > maxClock) newClock = maxClock;
                            if (newClock < minClock) newClock = minClock;

                            lastClock = newClock;
                        }
                    }
                    else if (_powerMode == 1)
                    {
                        newClock = 100;
                        minState = 0;
                    }
                    else if (_powerMode == 2)
                    {
                        newClock = 100;
                        minState = 100;
                    }

                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "-setacvalueindex scheme_current sub_processor PROCTHROTTLEMAX " + newClock;
                    process.StartInfo = startInfo;
                    process.Start();

                    System.Threading.Thread.Sleep(time);
                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "/setactive scheme_current";
                    process.StartInfo = startInfo;
                    process.Start();
                    System.Threading.Thread.Sleep(time);


                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "-setacvalueindex scheme_current sub_processor PROCTHROTTLEMIN " + minState;
                    process.StartInfo = startInfo;
                    process.Start();

                    System.Threading.Thread.Sleep(time);
                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "/setactive scheme_current";
                    process.StartInfo = startInfo;
                    process.Start();
                    System.Threading.Thread.Sleep(time);


                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "-setdcvalueindex scheme_current sub_processor PROCTHROTTLEMAX " + newClock;
                    process.StartInfo = startInfo;
                    process.Start();

                    System.Threading.Thread.Sleep(time);
                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "/setactive scheme_current";
                    process.StartInfo = startInfo;
                    process.Start();
                    System.Threading.Thread.Sleep(time);


                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "-setdcvalueindex scheme_current sub_processor PROCTHROTTLEMIN " + minState;
                    process.StartInfo = startInfo;
                    process.Start();

                    System.Threading.Thread.Sleep(time);
                    startInfo.FileName = "powercfg";
                    startInfo.Arguments = "/setactive scheme_current";
                    process.StartInfo = startInfo;
                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
    }
}
