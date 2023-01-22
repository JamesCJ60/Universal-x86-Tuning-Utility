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
using System.Diagnostics;

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

        public static decimal getSensorValueRAdj(string SensorName)
        {
            using (Process process = new Process())
            {
                int i = 0;

                string path = Settings.Default.Path + "\\bin\\ryzenadj\\ryzenadj.exe";
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "-i";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();


                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines != null || lines.Length != 0)
                {
                    do { i++; } while (!lines[i].Contains(SensorName));

                    if (lines[i].Contains(SensorName))
                    {
                        lines[i] = lines[i].Substring(25);
                        lines[i] = lines[i].Remove(lines[i].Length - 21);
                        lines[i] = lines[i].Replace("|", null);
                        lines[i] = lines[i].Replace(" ", null);

                        return Convert.ToDecimal(lines[i].ToString());
                    }
                    else
                    {
                        return 0;
                    }
                }
                process.WaitForExit();
            }

            return 0;
        }
    }
}
