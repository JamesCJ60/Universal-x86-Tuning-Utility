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
using UXTU.Properties;
using RyzenSmu;
using UXTU;
using AATUV3.Scripts.SMU_Backend_Scripts;
using System.Windows.Interop;

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
            else if(FAMID == 10)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3010508;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3010988;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3010984;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0;
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

        public async static void GetPMTableVersion()
        {
            await Task.Run(() =>
            {
                uint msg1 = 0x0;
                uint msg2 = 0x0;
                uint msg3 = 0x0;

                //set SMU message address
                if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2)
                {
                    msg1 = 0xc;
                    msg2 = 0xb;
                    msg3 = 0x3d;
                }

                
                if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 5 || Families.FAMID == 8)
                {
                    msg1 = 0x6;
                    msg2 = 0x66;
                    msg3 = 0x65;
                }

                if (Families.FAMID == 4 || Families.FAMID == 6)
                {
                    msg1 = 0x8;
                    msg2 = 0x6;
                    msg3 = 0x5;
                }

                Args = new uint[6];
                RyzenAccess = new Smu(EnableDebug);
                RyzenAccess.Initialize();

                //Get PMTable version
                RyzenAccess.SendPsmu(msg1, ref Args);
                PMTableVersion = Args[0];
                string pmt = string.Format("{0:x}", PMTableVersion);
                PMTableVersion = uint.Parse(pmt);

                Args[0] = 0;
                Thread.Sleep(250);

                //Set Address and reset Args[]
                RyzenAccess.SendPsmu(msg2, ref Args);
                Address = Args[0];

                Args[0] = 0;
                Thread.Sleep(100);

                RyzenAccess.SendPsmu(msg3, ref Args);

                Thread.Sleep(100);

                RyzenAccess.Deinitialize();

                DumpPMTableWithSensors();
            });
        }

        public static void DumpPMTableWithSensors()
        {
            uint msg1 = 0x0;
            uint msg2 = 0x0;
            uint msg3 = 0x0;

            //set SMU message address
            if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2)
            {
                msg1 = 0xc;
                msg2 = 0xb;
                msg3 = 0x3d;
            }
            if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 5 || Families.FAMID == 8)
            {
                msg1 = 0x6;
                msg2 = 0x66;
                msg3 = 0x65;
            }
            if (Families.FAMID == 4 ||  Families.FAMID == 6)
            {
                msg1 = 0x8;
                msg2 = 0x6;
                msg3 = 0x5;
            }

            Args = new uint[6];
            RyzenAccess = new Smu(EnableDebug);
            RyzenAccess.Initialize();

            if (RyzenAccess.SendPsmu(msg1, ref Args) == Smu.Status.OK)
            {
                PMTableVersion = Args[0];
            }

            if (RyzenAccess.SendPsmu(msg2, ref Args) == Smu.Status.OK)
            {
                //Set Address and reset Args[]
                Address = Args[0];
                if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2) Args[0] = 3;
                else Args[0] = 0;
                //Dump the Power Monitoring Table
                RyzenAccess.SendPsmu(msg3, ref Args);
                //Sleep so that the SMU has time to dump the PM Table properly.
                Thread.Sleep(100);

                for (int i = 0; i <= 1000 && Smu.ReadFloat(Address, 0) == 0f; i++)
                {
                    Thread.Sleep(1);
                }

                UInt32[] SmuVersionArgs = new UInt32[6];
                string[] TableDump = new string[706];
                RyzenAccess.SendMp1(0x2, ref Args);
                for (int i = 0; i < 6; i++)
                {
                    SmuVersionArgs[i] = Args[i];
                }
                Thread.Sleep(100);


                TableDump.Initialize();
                String SmuVersion = "";//$"{SmuVersionArgs[0]:X8}".Substring(0,2) + ".";
                SmuVersion += $"{SmuVersionArgs[0]:X8}".Substring(2, 2) + ".";
                SmuVersion += $"{SmuVersionArgs[0]:X8}".Substring(4, 2) + ".";
                SmuVersion += $"{SmuVersionArgs[0]:X8}".Substring(6, 2);
                TableDump[0] = ($"APU/CPU Name: {Settings.Default.APUName}");
                TableDump[1] = ($"SMU Version: {SmuVersionArgs[0]:X8}");
                TableDump[2] = ($"SMU Version: " + SmuVersion);
                TableDump[3] = ($"PMTableBaseAddress: 0x{Address:X8}");
                TableDump[4] = ($"PMTableVersion: 0x00{string.Format("{0:x}", PMTableVersion)}");
                float CurrentValue = 0.0F;
                bool OnlyZero = true;
                for (UInt32 i = 0; i <= 700; i++)
                {
                    CurrentValue = Smu.ReadFloat(Address, i);
                    if (OnlyZero && CurrentValue != 0.0F)
                    {
                        OnlyZero = false;
                    }
                    TableDump[5 + i] = $"0x{i:X4}\t{CurrentValue:F4}";
                }
                File.WriteAllLines("PMTableDumpWithSensors.log", TableDump);
            }
        }

        public async static void UpdateTable()
        {
            await Task.Run(() =>
            {
                try
                {
                    uint msg3 = 0x0;

                    //set SMU message address
                    if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2)
                    {
                        msg3 = 0x3d;
                    }
                    if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 5 || Families.FAMID == 8)
                    {
                        msg3 = 0x65;
                    }

                    Args = new uint[6];
                    RyzenAccess = new Smu(EnableDebug);
                    RyzenAccess.Initialize();
                    if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2) Args[0] = 3;
                    else Args[0] = 0;
                    RyzenAccess.SendPsmu(msg3, ref Args);
                    Thread.Sleep(500);
                    RyzenAccess.Deinitialize();
                }
                catch (Exception ex)
                {

                }
            });
        }

    }
}
