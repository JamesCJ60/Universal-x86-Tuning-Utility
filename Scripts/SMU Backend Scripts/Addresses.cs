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
using AATUV3.Scripts;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using Stopbyte.Controls;

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

            if (FAMID == -1)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0X3B10528;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0X3B10564;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0X3B10598;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B1051C;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0X3B10568;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0X3B10590;
            }


            if (FAMID == 0 || FAMID == 1 || FAMID == 2 || FAMID == 3 || FAMID == 7)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10564;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10A20;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10A80;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10A88;
            }
            else if (FAMID == 5 || FAMID == 8 || FAMID == 9 || FAMID == 11)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10578;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10a20;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10a80;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10a88;
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
            else if (FAMID == 10)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3010508;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3010988;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3010984;

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
        public static ulong[] Args64;
        public static Smu RyzenAccess;
        public static bool EnableDebug;
        public static UInt32 PMTableVersion;
        public static string SMUVersion;
        public static uint Address;
        public static ulong Address64;
        public static uint AddressLow;
        public static uint AddressHigh;

        public static string[] SensorNames;
        public static string[] SensorOffsets;

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

        public async static void GetPMTableVersion()
        {
            await Task.Run(() =>
            {
                uint msg1 = 0x0;
                uint msg2 = 0x0;
                uint msg3 = 0x0;

                if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 8)
                {
                    msg1 = 0x6;
                    msg2 = 0x66;
                    msg3 = 0x65;
                }

                if (Families.FAMID == 5)
                {
                    msg1 = 0x14;
                    msg2 = 0x13;
                    msg3 = 0x65;
                }

                if (Families.FAMID == 4 || Families.FAMID == 6)
                {
                    msg1 = 0x8;
                    msg2 = 0x6;
                    msg3 = 0x5;
                }

                if (Families.FAMID == 10)
                {
                    msg1 = 0x3;
                    msg2 = 0x4;
                    msg3 = 0x5;
                }

                if (Families.FAMID == 8)
                {
                    Args = new uint[6];
                    Args64 = new ulong[6];
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
                    Address64 = (ulong)(Args[1] << 32 | Args[0]);

                    Args[0] = 0;
                    Thread.Sleep(100);

                    RyzenAccess.SendPsmu64(msg3, ref Args64);

                    Thread.Sleep(100);

                    RyzenAccess.Deinitialize();

                    DumpPMTableWithSensors();
                }
                else
                {
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
                }

            });
        }
        public static int PMTLoop = 0;

        public static void DumpPMTableWithSensors()
        {
            uint msg1 = 0x0;
            uint msg2 = 0x0;
            uint msg3 = 0x0;

            //set SMU message address
            if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 8)
            {
                msg1 = 0x6;
                msg2 = 0x66;
                msg3 = 0x65;
            }
            if (Families.FAMID == 5)
            {
                msg1 = 0x14;
                msg2 = 0x13;
                msg3 = 0x65;
            }
            if (Families.FAMID == 4 || Families.FAMID == 6)
            {
                msg1 = 0x8;
                msg2 = 0x6;
                msg3 = 0x5;
            }
            if (Families.FAMID == 10)
            {
                msg1 = 0x3;
                msg2 = 0x4;
                msg3 = 0x5;
            }

            Args = new uint[6];
            Args64 = new ulong[6];
            RyzenAccess = new Smu(EnableDebug);
            RyzenAccess.Initialize();

            if (RyzenAccess.SendPsmu(msg1, ref Args) == Smu.Status.OK)
            {
                PMTableVersion = Args[0];
            }

            if (RyzenAccess.SendPsmu(msg2, ref Args) == Smu.Status.OK || RyzenAccess.SendPsmu64(msg2, ref Args64) == Smu.Status.OK)
            {
                RyzenAccess.SendPsmu(msg2, ref Args);
                //Set Address
                Address = Args[0];

                Args = new uint[6];

                Args[0] = Address;
                if (Families.FAMID == 8)
                {
                    //Set Address and reset Args[]
                    RyzenAccess.SendMp1(0xE, ref Args);
                }


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

                if (Families.FAMID != 10)
                {
                    RyzenAccess.SendMp1(0x2, ref Args);
                    for (int i = 0; i < 6; i++)
                    {
                        SmuVersionArgs[i] = Args[i];
                    }
                    Thread.Sleep(100);
                }
                else Args[0] = 0;
                TableDump.Initialize();
                String SmuVersion = "";//$"{SmuVersionArgs[0]:X8}".Substring(0,2) + ".";
                SmuVersion += $"{SmuVersionArgs[0]:X8}".Substring(2, 2) + ".";
                SmuVersion += $"{SmuVersionArgs[0]:X8}".Substring(4, 2) + ".";
                SmuVersion += $"{SmuVersionArgs[0]:X8}".Substring(6, 2);
                TableDump[0] = ($"APU/CPU Name: {Settings.Default.APUName}");
                TableDump[1] = ($"SMU Version: {SmuVersionArgs[0]:X8}");
                TableDump[2] = ($"SMU Version: " + SmuVersion);
                SMUVersion = $"{SmuVersion}";
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
                try
                {
                    File.WriteAllLines(Settings.Default.Path + "\\PMTableDumpWithSensors.log", TableDump);

                    if (Settings.Default.ApplyOCAtStart) ApplyOC();
                }
                catch (Exception ex) { }
            }
        }


        private static void ApplyOC()
        {
            int i = 0;
            if ((bool)Settings.Default.isAllCoreCLK == true)
            {
                SendCommand.set_oc_clk((uint)Settings.Default.AllCoreClk);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)Settings.Default.AllCoreClk);
                SendCommand.set_enable_oc();
                SendCommand.set_oc_clk((uint)Settings.Default.AllCoreClk);
                SendCommand.set_enable_oc();
                i++;
            }

            if ((bool)Settings.Default.isVID == true)
            {
                double vid = Math.Round((double)Settings.Default.CPUVID / 1000, 2);
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_oc_volt(Convert.ToUInt32((1.55 - vid) / 0.00625));
                SendCommand.set_enable_oc();
                i++;
            }

            if ((bool)Settings.Default.isBUS == true)
            {
                RwMmioAmd MMIO = new RwMmioAmd();
                MMIO.SetBclk(Convert.ToDouble(Settings.Default.BusCLK));
                i++;
            }

            if ((bool)Settings.Default.isCPUCO == true)
            {
                if ((int)Settings.Default.COCPU >= 0)
                {
                    SendCommand.set_coall((uint)Settings.Default.COCPU);
                }
                else
                {
                    SendCommand.set_coall(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)Settings.Default.COCPU)));
                }
                i++;
            }

            if ((bool)Settings.Default.isGPUCO == true)
            {
                if ((int)Settings.Default.COiGPU >= 0)
                {
                    SendCommand.set_cogfx((uint)Settings.Default.COiGPU);
                }
                else
                {
                    SendCommand.set_cogfx(Convert.ToUInt32(0x100000 - (uint)(-1 * (int)Settings.Default.COiGPU)));
                }
                i++;
            }

            //if (cbiGPU.IsChecked == true)
            //{
            //    SendCommand.set_gfx_clk((uint)nudiGPU.Value);
            //    i++;
            //}

            string CCD1output = Settings.Default.PerCOCCD1;
            string[] CCD1 = CCD1output.Split(',');

            string CCD2output = Settings.Default.PerCOCCD2;
            string[] CCD2 = CCD2output.Split(',');

            string CCD1OC = Settings.Default.PerOCCCD1;
            string[] CCD1OCArray = CCD1OC.Split(',');

            if ((bool)Settings.Default.isPerOC == true)
            {
                int o = 0;
                do
                {
                    SendCommand.set_per_core_oc_clk(Convert.ToUInt32((o << 20) | (Convert.ToUInt32(CCD1OCArray[o]) & 1048575)));
                    SendCommand.set_enable_oc();
                    o++;
                    i++;
                }
                while (o < CCD1OCArray.Length);
            }

            if (Settings.Default.isPerCO == true)
            {
                int x = 0;
                do
                {
                    int CCD, CCX, CORE, magnitude, magnitude2;

                    CCD = 0;
                    CCX = 0;
                    CORE = x;

                    magnitude = Convert.ToInt32(CCD1[x]);
                    magnitude2 = Convert.ToInt32(CCD2[x]);

                    if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 8)
                    {
                        int value = (CORE << 20) | (magnitude & 0xFFFF);
                        SendCommand.set_coper(Convert.ToUInt32(value));
                    }
                    else if (magnitude >= 0)
                    {
                        uint CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | magnitude & 0xFFFFF);
                        SendCommand.set_coper(CO);

                        CCX = 1;
                        CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | magnitude & 0xFFFFF);
                        SendCommand.set_coper(CO);
                    }
                    else
                    {
                        magnitude = magnitude * -1;
                        uint CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | (0x100000 - magnitude) & 0xFFFFF);
                        SendCommand.set_coper(CO);

                        CCX = 1;
                        CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | (0x100000 - magnitude) & 0xFFFFF);
                        SendCommand.set_coper(CO);
                    }

                    if (magnitude2 >= 0)
                    {
                        uint CO;
                        if (Families.FAMID == 6 || Families.FAMID == 10)
                        {
                            CCD = 1;
                            CCX = 0;
                            CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | magnitude & 0xFFFFF);
                            SendCommand.set_coper(CO);

                            CCX = 1;
                            CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | magnitude & 0xFFFFF);
                            SendCommand.set_coper(CO);
                        }
                    }
                    else
                    {
                        magnitude2 = magnitude2 * -1;
                        uint CO;
                        if (Families.FAMID == 6 || Families.FAMID == 10)
                        {
                            CCX = 0;
                            CCD = 1;
                            CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | (0x100000 - magnitude2) & 0xFFFFF);
                            SendCommand.set_coper(CO);

                            CCX = 1;
                            CO = Convert.ToUInt32(((CCD << 4 | CCX % 1 & 15) << 4 | CORE % 8 & 15) << 20 | (0x100000 - magnitude2) & 0xFFFFF);
                            SendCommand.set_coper(CO);
                        }
                    }

                    x++;
                }
                while (x < 8);
                i++;
            }

            if ((bool)Settings.Default.isNV == true)
            {
                //Get RyzenAdj path
                string path = "\\bin\\oc.exe";
                //Pass settings on to be applied
                BasicExeBackend.ApplySettings(path, "0 " + Settings.Default.dGPUCLK + " " + Settings.Default.dGPUMem, true);
                BasicExeBackend.ApplySettings(path, "1 " + Settings.Default.dGPUCLK + " " + Settings.Default.dGPUMem, true);
                BasicExeBackend.ApplySettings(path, "2 " + Settings.Default.dGPUCLK + " " + Settings.Default.dGPUMem, true);
                i++;
            }

            if (i == 0)
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Error! There-are-no-settings-to-apply!", false);
            }
            else
            {
                BasicExeBackend.ApplySettings("\\bin\\Notification.exe", "1 Settings-Applied! Your-settings-have-been-applied-successfully.", false);
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
                    //if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2)
                    //{
                    //    msg3 = 0x3d;
                    //}
                    if (Families.FAMID == 3 || Families.FAMID == 7 || Families.FAMID == 8)
                    {
                        msg3 = 0x65;
                    }

                    if (Families.FAMID == 4 || Families.FAMID == 6)
                    {
                        msg3 = 0x5;
                    }
                    //if (Families.FAMID == 5)
                    //{
                    //    msg3 = 0x65;
                    //}

                    //if (Families.FAMID == 10)
                    //{
                    //    msg3 = 0x5;
                    //}

                    Args = new uint[6];
                    RyzenAccess = new Smu(EnableDebug);
                    RyzenAccess.Initialize();
                    //if (Families.FAMID == 0 || Families.FAMID == 1 || Families.FAMID == 2) Args[0] = 3;
                    //else Args[0] = 0;
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
