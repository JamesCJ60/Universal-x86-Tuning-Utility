using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RyzenSmu;
using RyzenSMUBackend;
using AATUV3.Scripts.SMU_Backend_Scripts;
using System.Windows.Forms;

namespace AATUV3.Scripts.SMU_Backend_Scripts
{
    internal class GetSensor
    {
        public static float getSensorValve(string SensorName)
        {           
            int i = -1;
            string[] SensorNames = { };
            uint[] SensorOffset = { };

            if (Addresses.PMTableVersion == 400005)
            {
                SensorNames = pmtables.PMT_Sensor_400005;
                SensorOffset = pmtables.PMT_Offset_400005;
            }

            do{i++; if (i >= SensorNames.Length && SensorNames[i] != SensorName) return 0; } while (SensorNames[i] != SensorName);

            if (SensorNames[i] == SensorName)
            {
                return Smu.ReadFloat(Addresses.Address, SensorOffset[i]);
            }

            return 0;
        }
    }
}
