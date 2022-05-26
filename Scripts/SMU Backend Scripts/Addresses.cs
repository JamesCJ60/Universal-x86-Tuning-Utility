using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AATUV3.Properties;
using RyzenSmu;

namespace RyzenSMUBackend
{
    internal class Addresses
    {


        public static void SetAddresses()
        {
            int FAMID = Families.FAMID;

            RyzenSmu.Smu.SMU_PCI_ADDR = 0x00000000;
            RyzenSmu.Smu.SMU_OFFSET_ADDR = 0xB8;
            RyzenSmu.Smu.SMU_OFFSET_DATA = 0xBC;


            if (FAMID == 0 || FAMID == 1 || FAMID == 2 || FAMID == 3 || FAMID == 7)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x03B10528;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x03B10564;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x03B10998;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10A20;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10A80;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10A88;
            }
            else if (FAMID == 5 || FAMID == 8)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x03B10528;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x03B10564;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x03B10998;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10A20;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10A80;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10A88;
            }
            else if (FAMID == 4 || FAMID == 6)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10530;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B1057C;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B109C4;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10524;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10570;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10A40;
            }
            else
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0;
            }

            GetPMTableVersion();
        }

        public static uint[] Args;
        public static Smu RyzenAccess;
        public static bool EnableDebug;
        public static UInt32 PMTableVersion;
        public static uint Address;

        public static string[] SensorNames;
        public static string[] SensorOffsets;

        public async static void GetPMTableVersion()
        {
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

            

            await Task.Run(() =>
            {
                uint msg1 = 0x0;
                uint msg2 = 0x0;
                uint msg3 = 0x0;

                //set SMU message address
                if (Families.FAMID == 3 || Families.FAMID == 7)
                {
                    msg1 = 0x6;
                    msg2 = 0x66;
                    msg3 = 0x65;
                }

                Args = new uint[6];
                RyzenAccess = new Smu(EnableDebug);
                RyzenAccess.Initialize();

                //Get PMTable version
                RyzenAccess.SendPsmu(msg1, ref Args);
                PMTableVersion = Args[0];
                string pmt = string.Format("{0:x}", PMTableVersion);
                PMTableVersion = uint.Parse(pmt);
                //MessageBox.Show($"PMTable Version: {PMTableVersion}");
                Args[0] = 0;
                Thread.Sleep(500);

                //Set Address and reset Args[]
                RyzenAccess.SendPsmu(msg2, ref Args);
                Address = Args[0];
                //string addressString = "0x" + string.Format("{0:x}", Address);
                //Address = uint.Parse(addressString);
                //MessageBox.Show($"Address: {addressString}");
                Args[0] = 0;
                Thread.Sleep(500);

                RyzenAccess.SendPsmu(msg3, ref Args);

                Thread.Sleep(500);


                //Set path to PMTable
                //string path = (string)Settings.Default["Path"] + "\\bin\\pmtables\\" + Families.FAMID + "\\0x00" + PMTableVersion;

                //string offsets = "";
                //string sensors = "";


                //Load PMTbale data
                //offsets = path + "\\0x00" + PMTableVersion + "-offsets.txt";
                //sensors = path + "\\0x00" + PMTableVersion + "-sensors.txt";

                //Check if PMTbale exists 
                //if (File.Exists(offsets) && File.Exists(sensors))
                //{
                //    //Load sensor details
                //    SensorNames = File.ReadAllLines(sensors);
                //    SensorOffsets = File.ReadAllLines(offsets);

                //    string output = "";
                //    string outputNames = "";
                //    int i = 0;

                //    foreach(string line in SensorOffsets)
                //    {
                //        int lineConvert = Convert.ToInt32(line);
                //        output = output + "0x" + string.Format("{0:x}", lineConvert) + ",\n";
                //        i++;
                //    }
                //    foreach (string line in SensorNames)
                //    {
                //        outputNames =  outputNames + $"\"{line}\",\n";
                //    }

                //    if (File.Exists(offsets))
                //    {
                //        File.Delete(offsets);
                //    }

                //    if (File.Exists(sensors))
                //    {
                //        File.Delete(sensors);
                //    }

                //    File.WriteAllText(offsets, output);
                //    File.WriteAllText(sensors, outputNames);
                //}

                RyzenAccess.Deinitialize();
            });
        }
    }
}
