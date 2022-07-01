using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RyzenSmu;
using RyzenSMUBackend;
using AATUV3.Scripts.SMU_Backend_Scripts;
using System.Windows.Forms;
using System.IO;
using UXTU.Properties;

namespace AATUV3.Scripts.SMU_Backend_Scripts
{
    internal class GetSensor
    {

        public static float getSensorValve(string SensorName)
        {
            try
            {
                string[] SensorNames = pmtables.PMT_Sensors;
                uint[] SensorOffset = pmtables.PMT_Offset;

                if(SensorNames.Length == 0 || SensorNames == null)
                {
                    if (File.Exists(Settings.Default["Path"].ToString() + $"\\bin\\pmtables\\0x00{string.Format("{0:x}", Addresses.PMTableVersion)}-sensors.txt"))
                    {
                        pmtables.PMT_Sensors = File.ReadAllLines(Settings.Default["Path"].ToString() + $"\\bin\\pmtables\\0x00{string.Format("{0:x}", Addresses.PMTableVersion)}-sensors.txt");
                    }
                }

                int i = -1;

                if (SensorNames != null || SensorNames.Length != 0)
                {
                    do { i++; } while (SensorNames[i] != SensorName && i <= SensorNames.Length);

                    if (SensorNames[i] == SensorName)
                    {
                        return Smu.ReadFloat(Addresses.Address, SensorOffset[i]);
                    }
                }
            }
            catch (Exception ex) { }
            return 0;
        }
    }
}
