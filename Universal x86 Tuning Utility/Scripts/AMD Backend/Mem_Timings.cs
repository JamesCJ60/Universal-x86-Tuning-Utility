using OpenLibSys_Mem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{
    /// <summary>
    /// The Mem_Timings class holds each memory timing value and contains
    /// a reusable method to retrieve all timings.
    /// </summary>
    public class Mem_Timings
    {
        // Properties for memory timing values.
        public bool BGS { get; set; }
        public bool BGSA { get; set; }
        public bool Preamble2T { get; set; }
        public bool GDM { get; set; }
        public bool Cmd2T { get; set; }
        public uint MEMCLK { get; set; }
        public uint tRCDWR { get; set; }
        public uint tRCDRD { get; set; }
        public uint tRAS { get; set; }
        public uint tCL { get; set; }
        public uint tRPPB { get; set; }
        public uint tRP { get; set; }
        public uint tRCPB { get; set; }
        public uint tRC { get; set; }
        public uint tRTP { get; set; }
        public uint tRRDDLR { get; set; }
        public uint tRRDL { get; set; }
        public uint tRRDS { get; set; }
        public uint tFAWDLR { get; set; }
        public uint tFAWSLR { get; set; }
        public uint tFAW { get; set; }
        public uint tWTRL { get; set; }
        public uint tWTRS { get; set; }
        public uint tCWL { get; set; }
        public uint tWR { get; set; }
        public uint tRCPage { get; set; }
        public uint tRDRDBAN { get; set; }
        public uint tRDRDSCL { get; set; }
        public uint tRDRDSCDLR { get; set; }
        public uint tRDRDSC { get; set; }
        public uint tRDRDSD { get; set; }
        public uint tRDRDDD { get; set; }
        public uint tWRWRBAN { get; set; }
        public uint tWRWRSCL { get; set; }
        public uint tWRWRSCDLR { get; set; }
        public uint tWRWRSC { get; set; }
        public uint tWRWRSD { get; set; }
        public uint tWRWRDD { get; set; }
        public uint tWRRDSCDLR { get; set; }
        public uint tRDWR { get; set; }
        public uint tWRRD { get; set; }
        public uint tREF { get; set; }
        public uint tREFCT { get; set; }
        public uint tMODPDA { get; set; }
        public uint tMRDPDA { get; set; }
        public uint tMOD { get; set; }
        public uint tMRD { get; set; }
        public uint tSTAG { get; set; }
        public uint tCKE { get; set; }
        public uint tPHYWRD { get; set; }
        public uint tPHYRDLAT { get; set; }
        public uint tPHYWRLAT { get; set; }
        public uint tRDDATA { get; set; }
        public uint tRFC4 { get; set; }
        public uint tRFC4CT { get; set; }
        public uint tRFC2 { get; set; }
        public uint tRFC2CT { get; set; }
        public uint tRFC { get; set; }
        public uint tRFCCT { get; set; }
        public uint tSTAG4LR { get; set; }
        public uint tSTAG2LR { get; set; }
        public uint tSTAGLR { get; set; }
        public uint tWRMPR { get; set; }
        public string ProcODT { get; set; }
        public string RttNom { get; set; }
        public string RttWr { get; set; }
        public string RttPark { get; set; }
        public string AddrCmdSetup { get; set; }
        public string CsOdtSetup { get; set; }
        public string CkeSetup { get; set; }
        public string ClkDrvStrength { get; set; }
        public string AddrCmdDrvStrength { get; set; }
        public string CsOdtDrvStrength { get; set; }
        public string CkeDrvStrength { get; set; }

        /// <summary>
        /// Retrieves and calculates all memory timings using the OpenLibSys API.
        /// The method does not change any of the PCI/HEX IDs.
        /// </summary>
        /// <returns>An instance of Mem_Timings with all values set.</returns>
        public static Mem_Timings RetrieveTimings()
        {
            Mem_Timings mem = new Mem_Timings();
            bool SMUSlow = false;
            int SMUDelay = SMUSlow ? 60 : 10;

            Ols ols = new Ols();
            if (ols.Status != Ols.OlsStatus.NO_ERROR || ols.DllStatus != Ols.OlsDllStatus.OLS_DLL_NO_ERROR)
                throw new ApplicationException("Ols initialization error.");

            uint eax = 0, ebx = 0, ecx = 0, edx = 0;
            ols.CpuidPx(0x80000001, ref eax, ref ebx, ref ecx, ref edx, (UIntPtr)0x01);
            uint CPUFMS = eax & 0xFFFF00;

            uint SMUORG = ols.ReadPciConfigDword(0x00, 0xB8);
            Thread.Sleep(SMUDelay);

            uint someOffset = ReadDword(0x50200, ols, SMUDelay) == 0x300 ? 0x100000u : 0u;

            uint BGS = ReadDword(0x00050058 + someOffset, ols, SMUDelay);
            uint BGSA = ReadDword(0x000500D0 + someOffset, ols, SMUDelay);
            uint DramConfiguration = ReadDword(0x00050200 + someOffset, ols, SMUDelay);

            uint DramTiming1 = ReadDword(0x00050204 + someOffset, ols, SMUDelay);
            uint DramTiming2 = ReadDword(0x00050208 + someOffset, ols, SMUDelay);
            uint DramTiming3 = ReadDword(0x0005020C + someOffset, ols, SMUDelay);
            uint DramTiming4 = ReadDword(0x00050210 + someOffset, ols, SMUDelay);
            uint DramTiming5 = ReadDword(0x00050214 + someOffset, ols, SMUDelay);
            uint DramTiming6 = ReadDword(0x00050218 + someOffset, ols, SMUDelay);
            uint DramTiming7 = ReadDword(0x0005021C + someOffset, ols, SMUDelay);
            uint DramTiming8 = ReadDword(0x00050220 + someOffset, ols, SMUDelay);
            uint DramTiming9 = ReadDword(0x00050224 + someOffset, ols, SMUDelay);
            uint DramTiming10 = ReadDword(0x00050228 + someOffset, ols, SMUDelay);
            uint DramTiming12 = ReadDword(0x00050230 + someOffset, ols, SMUDelay);
            uint DramTiming13 = ReadDword(0x00050234 + someOffset, ols, SMUDelay);
            uint DramTiming20 = ReadDword(0x00050250 + someOffset, ols, SMUDelay);
            uint DramTiming21 = ReadDword(0x00050254 + someOffset, ols, SMUDelay);
            uint DramTiming22 = ReadDword(0x00050258 + someOffset, ols, SMUDelay);

            uint tRFCTiming0 = ReadDword(0x00050260 + someOffset, ols, SMUDelay);
            uint tRFCTiming1 = ReadDword(0x00050264 + someOffset, ols, SMUDelay);
            uint tSTAGTiming0 = ReadDword(0x00050270 + someOffset, ols, SMUDelay);
            uint tSTAGTiming1 = ReadDword(0x00050274 + someOffset, ols, SMUDelay);
            uint DramTiming35 = ReadDword(0x0005028C + someOffset, ols, SMUDelay);

            uint tRFCTiming, tSTAGTiming;
            if (tRFCTiming0 == tRFCTiming1)
            {
                tRFCTiming = tRFCTiming0;
                tSTAGTiming = tSTAGTiming0;
            }
            else if (tRFCTiming0 == 0x21060138)
            {
                tRFCTiming = tRFCTiming1;
                tSTAGTiming = tSTAGTiming1;
            }
            else
            {
                tRFCTiming = tRFCTiming0;
                tSTAGTiming = tSTAGTiming0;
            }

            mem.BGS = (BGS != 0x87654321);
            mem.BGSA = (BGSA == 0x111107F1);
            mem.Preamble2T = ((DramConfiguration & 0x1000) >> 12) != 0;
            mem.GDM = ((DramConfiguration & 0x800) >> 11) != 0;
            mem.Cmd2T = ((DramConfiguration & 0x400) >> 10) != 0;
            mem.MEMCLK = DramConfiguration & 0x7F;
            float MEMCLKTRxx = mem.MEMCLK / 3.0f * 100;
            mem.MEMCLK = (uint)(mem.MEMCLK / 3.0f * 200);

            mem.tRCDWR = (DramTiming1 & 0x3F000000) >> 24;
            mem.tRCDRD = (DramTiming1 & 0x3F0000) >> 16;
            mem.tRAS = (DramTiming1 & 0x7F00) >> 8;
            mem.tCL = DramTiming1 & 0x3F;

            mem.tRPPB = (DramTiming2 & 0x3F000000) >> 24;
            mem.tRP = (DramTiming2 & 0x3F0000) >> 16;
            mem.tRCPB = (DramTiming2 & 0xFF00) >> 8;
            mem.tRC = DramTiming2 & 0xFF;

            mem.tRTP = (DramTiming3 & 0x1F000000) >> 24;
            mem.tRRDDLR = (DramTiming3 & 0x1F0000) >> 16;
            mem.tRRDL = (DramTiming3 & 0x1F00) >> 8;
            mem.tRRDS = DramTiming3 & 0x1F;

            mem.tFAWDLR = (DramTiming4 & 0x7E000000) >> 25;
            mem.tFAWSLR = (DramTiming4 & 0xFC0000) >> 18;
            mem.tFAW = DramTiming4 & 0x7F;

            mem.tWTRL = (DramTiming5 & 0x7F0000) >> 16;
            mem.tWTRS = (DramTiming5 & 0x1F00) >> 8;
            mem.tCWL = DramTiming5 & 0x3F;

            mem.tWR = DramTiming6 & 0x7F;

            mem.tRCPage = (DramTiming7 & 0xFFF00000) >> 20;

            mem.tRDRDBAN = (DramTiming8 & 0xC0000000) >> 30;
            mem.tRDRDSCL = (DramTiming8 & 0x3F000000) >> 24;
            mem.tRDRDSCDLR = (DramTiming8 & 0xF00000) >> 20;
            mem.tRDRDSC = (DramTiming8 & 0xF0000) >> 16;
            mem.tRDRDSD = (DramTiming8 & 0xF00) >> 8;
            mem.tRDRDDD = DramTiming8 & 0xF;

            mem.tWRWRBAN = (DramTiming9 & 0xC0000000) >> 30;
            mem.tWRWRSCL = (DramTiming9 & 0x3F000000) >> 24;
            mem.tWRWRSCDLR = (DramTiming9 & 0xF00000) >> 20;
            mem.tWRWRSC = (DramTiming9 & 0xF0000) >> 16;
            mem.tWRWRSD = (DramTiming9 & 0xF00) >> 8;
            mem.tWRWRDD = DramTiming9 & 0xF;

            mem.tWRRDSCDLR = (DramTiming10 & 0x1F0000) >> 16;
            mem.tRDWR = (DramTiming10 & 0x1F00) >> 8;
            mem.tWRRD = DramTiming10 & 0xF;

            mem.tREF = DramTiming12 & 0xFFFF;
            mem.tREFCT = (uint)(1000 / MEMCLKTRxx * mem.tREF);

            mem.tMODPDA = (DramTiming13 & 0x3F000000) >> 24;
            mem.tMRDPDA = (DramTiming13 & 0x3F0000) >> 16;
            mem.tMOD = (DramTiming13 & 0x3F00) >> 8;
            mem.tMRD = DramTiming13 & 0x3F;

            mem.tSTAG = (DramTiming20 & 0xFF0000) >> 16;

            mem.tCKE = (DramTiming21 & 0x1F000000) >> 24;

            mem.tPHYWRD = (DramTiming22 & 0x7000000) >> 24;
            mem.tPHYRDLAT = (DramTiming22 & 0x3F0000) >> 16;
            mem.tPHYWRLAT = (DramTiming22 & 0x1F00) >> 8;
            mem.tRDDATA = DramTiming22 & 0x7F;

            mem.tRFC4 = (tRFCTiming & 0xFFC00000) >> 22;
            mem.tRFC4CT = (uint)(1000 / MEMCLKTRxx * mem.tRFC4);

            mem.tRFC2 = (tRFCTiming & 0x3FF800) >> 11;
            mem.tRFC2CT = (uint)(1000 / MEMCLKTRxx * mem.tRFC2);

            mem.tRFC = tRFCTiming & 0x7FF;
            mem.tRFCCT = (uint)(1000 / MEMCLKTRxx * mem.tRFC);

            mem.tSTAG4LR = (tSTAGTiming & 0x1FF00000) >> 20;
            mem.tSTAG2LR = (tSTAGTiming & 0x7FC00) >> 10;
            mem.tSTAGLR = tSTAGTiming & 0x1FF;

            mem.tWRMPR = (DramTiming35 & 0x3F000000) >> 24;

            uint eax2 = 0, ebx2 = 0, ecx2 = 0, edx2 = 0;
            ols.CpuidPx(0x80000001, ref eax2, ref ebx2, ref ecx2, ref edx2, (UIntPtr)0x01);
            eax2 &= 0xFFFF00;
            ebx2 = (ebx2 & 0xF0000000) >> 28;
            uint someOffset2 = 0;
            if (ebx2 == 7)
                someOffset2 = 0x2180;
            else if (ebx2 == 2)
                someOffset2 = 0x100;
            else
                someOffset2 = 0x00;

            if (eax2 == 0x810F00 || (ebx2 == 1 || ebx2 == 3 || ebx2 == 4))
            {
                mem.ProcODT = "N/A";
                mem.RttNom = "N/A";
                mem.RttWr = "N/A";
                mem.RttPark = "N/A";
                mem.AddrCmdSetup = "N/A";
                mem.CsOdtSetup = "N/A";
                mem.CkeSetup = "N/A";
                mem.ClkDrvStrength = "N/A";
                mem.AddrCmdDrvStrength = "N/A";
                mem.CsOdtDrvStrength = "N/A";
                mem.CkeDrvStrength = "N/A";
            }
            else
            {
                ols.WritePciConfigDword(0x00, 0xB8, 0x3B10528);
                ols.WritePciConfigDword(0x00, 0xBC, 0x2C);
                ols.WritePciConfigDword(0x00, 0xB8, 0x3B1059C);
                uint x = ols.ReadPciConfigDword(0, 0xBC);
                ulong num26 = x - someOffset2;
                Ols.IsInpOutDriverOpen2();
                uint num27 = 0xB1;
                uint physLong1 = ols.GetPhysLong(new UIntPtr(num26 + num27));
                uint num28 = 0xB5;
                uint physLong2 = ols.GetPhysLong(new UIntPtr(num26 + num28));
                uint num29 = 0xBA;
                uint physLong3 = ols.GetPhysLong(new UIntPtr(num26 + num29));

                uint addrCmdSetup = physLong1 & 0xFF;
                switch (addrCmdSetup)
                {
                    case 0: mem.AddrCmdSetup = "0/0"; break;
                    case 1: mem.AddrCmdSetup = "0/1"; break;
                    case 2: mem.AddrCmdSetup = "0/2"; break;
                    case 3: mem.AddrCmdSetup = "0/3"; break;
                    case 4: mem.AddrCmdSetup = "0/4"; break;
                    case 5: mem.AddrCmdSetup = "0/5"; break;
                    case 6: mem.AddrCmdSetup = "0/6"; break;
                    case 7: mem.AddrCmdSetup = "0/7"; break;
                    case 8: mem.AddrCmdSetup = "0/8"; break;
                    case 9: mem.AddrCmdSetup = "0/9"; break;
                    case 10: mem.AddrCmdSetup = "0/10"; break;
                    case 11: mem.AddrCmdSetup = "0/11"; break;
                    case 12: mem.AddrCmdSetup = "0/12"; break;
                    case 13: mem.AddrCmdSetup = "0/13"; break;
                    case 14: mem.AddrCmdSetup = "0/14"; break;
                    case 15: mem.AddrCmdSetup = "0/15"; break;
                    case 16: mem.AddrCmdSetup = "0/16"; break;
                    case 17: mem.AddrCmdSetup = "0/17"; break;
                    case 18: mem.AddrCmdSetup = "0/18"; break;
                    case 19: mem.AddrCmdSetup = "0/19"; break;
                    case 20: mem.AddrCmdSetup = "0/20"; break;
                    case 21: mem.AddrCmdSetup = "0/21"; break;
                    case 22: mem.AddrCmdSetup = "0/22"; break;
                    case 23: mem.AddrCmdSetup = "0/23"; break;
                    case 24: mem.AddrCmdSetup = "0/24"; break;
                    case 25: mem.AddrCmdSetup = "0/25"; break;
                    case 26: mem.AddrCmdSetup = "0/26"; break;
                    case 27: mem.AddrCmdSetup = "0/27"; break;
                    case 28: mem.AddrCmdSetup = "0/28"; break;
                    case 29: mem.AddrCmdSetup = "0/29"; break;
                    case 30: mem.AddrCmdSetup = "0/30"; break;
                    case 31: mem.AddrCmdSetup = "0/31"; break;
                    case 32: mem.AddrCmdSetup = "1/0"; break;
                    case 33: mem.AddrCmdSetup = "1/1"; break;
                    case 34: mem.AddrCmdSetup = "1/2"; break;
                    case 35: mem.AddrCmdSetup = "1/3"; break;
                    case 36: mem.AddrCmdSetup = "1/4"; break;
                    case 37: mem.AddrCmdSetup = "1/5"; break;
                    case 38: mem.AddrCmdSetup = "1/6"; break;
                    case 39: mem.AddrCmdSetup = "1/7"; break;
                    case 40: mem.AddrCmdSetup = "1/8"; break;
                    case 41: mem.AddrCmdSetup = "1/9"; break;
                    case 42: mem.AddrCmdSetup = "1/10"; break;
                    case 43: mem.AddrCmdSetup = "1/11"; break;
                    case 44: mem.AddrCmdSetup = "1/12"; break;
                    case 45: mem.AddrCmdSetup = "1/13"; break;
                    case 46: mem.AddrCmdSetup = "1/14"; break;
                    case 47: mem.AddrCmdSetup = "1/15"; break;
                    case 48: mem.AddrCmdSetup = "1/16"; break;
                    case 49: mem.AddrCmdSetup = "1/17"; break;
                    case 50: mem.AddrCmdSetup = "1/18"; break;
                    case 51: mem.AddrCmdSetup = "1/19"; break;
                    case 52: mem.AddrCmdSetup = "1/20"; break;
                    case 53: mem.AddrCmdSetup = "1/21"; break;
                    case 54: mem.AddrCmdSetup = "1/22"; break;
                    case 55: mem.AddrCmdSetup = "1/23"; break;
                    case 56: mem.AddrCmdSetup = "1/24"; break;
                    case 57: mem.AddrCmdSetup = "1/25"; break;
                    case 58: mem.AddrCmdSetup = "1/26"; break;
                    case 59: mem.AddrCmdSetup = "1/27"; break;
                    case 60: mem.AddrCmdSetup = "1/28"; break;
                    case 61: mem.AddrCmdSetup = "1/29"; break;
                    case 62: mem.AddrCmdSetup = "1/30"; break;
                    case 63: mem.AddrCmdSetup = "1/31"; break;
                }

                uint csOdtSetup = (physLong1 & 0xFF00) >> 8;
                switch (csOdtSetup)
                {
                    case 0: mem.CsOdtSetup = "0/0"; break;
                    case 1: mem.CsOdtSetup = "0/1"; break;
                    case 2: mem.CsOdtSetup = "0/2"; break;
                    case 3: mem.CsOdtSetup = "0/3"; break;
                    case 4: mem.CsOdtSetup = "0/4"; break;
                    case 5: mem.CsOdtSetup = "0/5"; break;
                    case 6: mem.CsOdtSetup = "0/6"; break;
                    case 7: mem.CsOdtSetup = "0/7"; break;
                    case 8: mem.CsOdtSetup = "0/8"; break;
                    case 9: mem.CsOdtSetup = "0/9"; break;
                    case 10: mem.CsOdtSetup = "0/10"; break;
                    case 11: mem.CsOdtSetup = "0/11"; break;
                    case 12: mem.CsOdtSetup = "0/12"; break;
                    case 13: mem.CsOdtSetup = "0/13"; break;
                    case 14: mem.CsOdtSetup = "0/14"; break;
                    case 15: mem.CsOdtSetup = "0/15"; break;
                    case 16: mem.CsOdtSetup = "0/16"; break;
                    case 17: mem.CsOdtSetup = "0/17"; break;
                    case 18: mem.CsOdtSetup = "0/18"; break;
                    case 19: mem.CsOdtSetup = "0/19"; break;
                    case 20: mem.CsOdtSetup = "0/20"; break;
                    case 21: mem.CsOdtSetup = "0/21"; break;
                    case 22: mem.CsOdtSetup = "0/22"; break;
                    case 23: mem.CsOdtSetup = "0/23"; break;
                    case 24: mem.CsOdtSetup = "0/24"; break;
                    case 25: mem.CsOdtSetup = "0/25"; break;
                    case 26: mem.CsOdtSetup = "0/26"; break;
                    case 27: mem.CsOdtSetup = "0/27"; break;
                    case 28: mem.CsOdtSetup = "0/28"; break;
                    case 29: mem.CsOdtSetup = "0/29"; break;
                    case 30: mem.CsOdtSetup = "0/30"; break;
                    case 31: mem.CsOdtSetup = "0/31"; break;
                    case 32: mem.CsOdtSetup = "1/0"; break;
                    case 33: mem.CsOdtSetup = "1/1"; break;
                    case 34: mem.CsOdtSetup = "1/2"; break;
                    case 35: mem.CsOdtSetup = "1/3"; break;
                    case 36: mem.CsOdtSetup = "1/4"; break;
                    case 37: mem.CsOdtSetup = "1/5"; break;
                    case 38: mem.CsOdtSetup = "1/6"; break;
                    case 39: mem.CsOdtSetup = "1/7"; break;
                    case 40: mem.CsOdtSetup = "1/8"; break;
                    case 41: mem.CsOdtSetup = "1/9"; break;
                    case 42: mem.CsOdtSetup = "1/10"; break;
                    case 43: mem.CsOdtSetup = "1/11"; break;
                    case 44: mem.CsOdtSetup = "1/12"; break;
                    case 45: mem.CsOdtSetup = "1/13"; break;
                    case 46: mem.CsOdtSetup = "1/14"; break;
                    case 47: mem.CsOdtSetup = "1/15"; break;
                    case 48: mem.CsOdtSetup = "1/16"; break;
                    case 49: mem.CsOdtSetup = "1/17"; break;
                    case 50: mem.CsOdtSetup = "1/18"; break;
                    case 51: mem.CsOdtSetup = "1/19"; break;
                    case 52: mem.CsOdtSetup = "1/20"; break;
                    case 53: mem.CsOdtSetup = "1/21"; break;
                    case 54: mem.CsOdtSetup = "1/22"; break;
                    case 55: mem.CsOdtSetup = "1/23"; break;
                    case 56: mem.CsOdtSetup = "1/24"; break;
                    case 57: mem.CsOdtSetup = "1/25"; break;
                    case 58: mem.CsOdtSetup = "1/26"; break;
                    case 59: mem.CsOdtSetup = "1/27"; break;
                    case 60: mem.CsOdtSetup = "1/28"; break;
                    case 61: mem.CsOdtSetup = "1/29"; break;
                    case 62: mem.CsOdtSetup = "1/30"; break;
                    case 63: mem.CsOdtSetup = "1/31"; break;
                }

                uint ckeSetup = (physLong1 & 0xFF0000) >> 16;
                switch (ckeSetup)
                {
                    case 0: mem.CkeSetup = "0/0"; break;
                    case 1: mem.CkeSetup = "0/1"; break;
                    case 2: mem.CkeSetup = "0/2"; break;
                    case 3: mem.CkeSetup = "0/3"; break;
                    case 4: mem.CkeSetup = "0/4"; break;
                    case 5: mem.CkeSetup = "0/5"; break;
                    case 6: mem.CkeSetup = "0/6"; break;
                    case 7: mem.CkeSetup = "0/7"; break;
                    case 8: mem.CkeSetup = "0/8"; break;
                    case 9: mem.CkeSetup = "0/9"; break;
                    case 10: mem.CkeSetup = "0/10"; break;
                    case 11: mem.CkeSetup = "0/11"; break;
                    case 12: mem.CkeSetup = "0/12"; break;
                    case 13: mem.CkeSetup = "0/13"; break;
                    case 14: mem.CkeSetup = "0/14"; break;
                    case 15: mem.CkeSetup = "0/15"; break;
                    case 16: mem.CkeSetup = "0/16"; break;
                    case 17: mem.CkeSetup = "0/17"; break;
                    case 18: mem.CkeSetup = "0/18"; break;
                    case 19: mem.CkeSetup = "0/19"; break;
                    case 20: mem.CkeSetup = "0/20"; break;
                    case 21: mem.CkeSetup = "0/21"; break;
                    case 22: mem.CkeSetup = "0/22"; break;
                    case 23: mem.CkeSetup = "0/23"; break;
                    case 24: mem.CkeSetup = "0/24"; break;
                    case 25: mem.CkeSetup = "0/25"; break;
                    case 26: mem.CkeSetup = "0/26"; break;
                    case 27: mem.CkeSetup = "0/27"; break;
                    case 28: mem.CkeSetup = "0/28"; break;
                    case 29: mem.CkeSetup = "0/29"; break;
                    case 30: mem.CkeSetup = "0/30"; break;
                    case 31: mem.CkeSetup = "0/31"; break;
                    case 32: mem.CkeSetup = "1/0"; break;
                    case 33: mem.CkeSetup = "1/1"; break;
                    case 34: mem.CkeSetup = "1/2"; break;
                    case 35: mem.CkeSetup = "1/3"; break;
                    case 36: mem.CkeSetup = "1/4"; break;
                    case 37: mem.CkeSetup = "1/5"; break;
                    case 38: mem.CkeSetup = "1/6"; break;
                    case 39: mem.CkeSetup = "1/7"; break;
                    case 40: mem.CkeSetup = "1/8"; break;
                    case 41: mem.CkeSetup = "1/9"; break;
                    case 42: mem.CkeSetup = "1/10"; break;
                    case 43: mem.CkeSetup = "1/11"; break;
                    case 44: mem.CkeSetup = "1/12"; break;
                    case 45: mem.CkeSetup = "1/13"; break;
                    case 46: mem.CkeSetup = "1/14"; break;
                    case 47: mem.CkeSetup = "1/15"; break;
                    case 48: mem.CkeSetup = "1/16"; break;
                    case 49: mem.CkeSetup = "1/17"; break;
                    case 50: mem.CkeSetup = "1/18"; break;
                    case 51: mem.CkeSetup = "1/19"; break;
                    case 52: mem.CkeSetup = "1/20"; break;
                    case 53: mem.CkeSetup = "1/21"; break;
                    case 54: mem.CkeSetup = "1/22"; break;
                    case 55: mem.CkeSetup = "1/23"; break;
                    case 56: mem.CkeSetup = "1/24"; break;
                    case 57: mem.CkeSetup = "1/25"; break;
                    case 58: mem.CkeSetup = "1/26"; break;
                    case 59: mem.CkeSetup = "1/27"; break;
                    case 60: mem.CkeSetup = "1/28"; break;
                    case 61: mem.CkeSetup = "1/29"; break;
                    case 62: mem.CkeSetup = "1/30"; break;
                    case 63: mem.CkeSetup = "1/31"; break;
                }

                uint clkDrvStrength = (physLong1 & 0xFF000000) >> 24;
                if (clkDrvStrength <= 7)
                {
                    switch (clkDrvStrength)
                    {
                        case 0: mem.ClkDrvStrength = "120.0Ω"; break;
                        case 1: mem.ClkDrvStrength = "60.0Ω"; break;
                        case 3: mem.ClkDrvStrength = "40.0Ω"; break;
                        case 7: mem.ClkDrvStrength = "30.0Ω"; break;
                    }
                }
                else if (clkDrvStrength != 15)
                {
                    if (clkDrvStrength == 31)
                        mem.ClkDrvStrength = "20.0Ω";
                }
                else
                    mem.ClkDrvStrength = "24.0Ω";

                uint addrCmdDrvStrength = physLong2 & 0xFF;
                if (addrCmdDrvStrength <= 7)
                {
                    switch (addrCmdDrvStrength)
                    {
                        case 0: mem.AddrCmdDrvStrength = "120.0Ω"; break;
                        case 1: mem.AddrCmdDrvStrength = "60.0Ω"; break;
                        case 3: mem.AddrCmdDrvStrength = "40.0Ω"; break;
                        case 7: mem.AddrCmdDrvStrength = "30.0Ω"; break;
                    }
                }
                else if (addrCmdDrvStrength != 15)
                {
                    if (addrCmdDrvStrength == 31)
                        mem.AddrCmdDrvStrength = "20.0Ω";
                }
                else
                    mem.AddrCmdDrvStrength = "24.0Ω";

                uint csOdtDrvStrength = (physLong2 & 0xFF00) >> 8;
                if (csOdtDrvStrength <= 7)
                {
                    switch (csOdtDrvStrength)
                    {
                        case 0: mem.CsOdtDrvStrength = "120.0Ω"; break;
                        case 1: mem.CsOdtDrvStrength = "60.0Ω"; break;
                        case 3: mem.CsOdtDrvStrength = "40.0Ω"; break;
                        case 7: mem.CsOdtDrvStrength = "30.0Ω"; break;
                    }
                }
                else if (csOdtDrvStrength != 15)
                {
                    if (csOdtDrvStrength == 31)
                        mem.CsOdtDrvStrength = "20.0Ω";
                }
                else
                    mem.CsOdtDrvStrength = "24.0Ω";

                uint ckeDrvStrength = (physLong2 & 0xFF0000) >> 16;
                if (ckeDrvStrength <= 7)
                {
                    switch (ckeDrvStrength)
                    {
                        case 0: mem.CkeDrvStrength = "120.0Ω"; break;
                        case 1: mem.CkeDrvStrength = "60.0Ω"; break;
                        case 3: mem.CkeDrvStrength = "40.0Ω"; break;
                        case 7: mem.CkeDrvStrength = "30.0Ω"; break;
                    }
                }
                else if (ckeDrvStrength != 15)
                {
                    if (ckeDrvStrength == 31)
                        mem.CkeDrvStrength = "20.0Ω";
                }
                else
                    mem.CkeDrvStrength = "24.0Ω";

                uint rttNom = physLong3 & 0xFF;
                switch (rttNom)
                {
                    case 0: mem.RttNom = "Disabled"; break;
                    case 1: mem.RttNom = "60.0Ω"; break;
                    case 2: mem.RttNom = "120.0Ω"; break;
                    case 3: mem.RttNom = "40.0Ω"; break;
                    case 4: mem.RttNom = "240.0Ω"; break;
                    case 5: mem.RttNom = "48.0Ω"; break;
                    case 6: mem.RttNom = "80.0Ω"; break;
                    case 7: mem.RttNom = "34.3Ω"; break;
                }
                uint rttWr = (physLong3 & 0xFF00) >> 8;
                switch (rttWr)
                {
                    case 0: mem.RttWr = "Disabled"; break;
                    case 1: mem.RttWr = "120.0Ω"; break;
                    case 2: mem.RttWr = "240.0Ω"; break;
                    case 3: mem.RttWr = "Hi-Z"; break;
                    case 4: mem.RttWr = "80.0Ω"; break;
                }
                uint rttPark = (physLong3 & 0xFF0000) >> 16;
                switch (rttPark)
                {
                    case 0: mem.RttPark = "Disabled"; break;
                    case 1: mem.RttPark = "60.0Ω"; break;
                    case 2: mem.RttPark = "120.0Ω"; break;
                    case 3: mem.RttPark = "40.0Ω"; break;
                    case 4: mem.RttPark = "240.0Ω"; break;
                    case 5: mem.RttPark = "48.0Ω"; break;
                    case 6: mem.RttPark = "80.0Ω"; break;
                    case 7: mem.RttPark = "34.3Ω"; break;
                }
                uint procODT = (physLong3 & 0xFF000000) >> 24;
                if (procODT == 8) mem.ProcODT = "120.0Ω";
                else if (procODT == 9) mem.ProcODT = "96.0Ω";
                else if (procODT == 10) mem.ProcODT = "80.0Ω";
                else if (procODT == 11) mem.ProcODT = "68.6Ω";
                else if (procODT == 24) mem.ProcODT = "60.0Ω";
                else if (procODT == 25) mem.ProcODT = "53.3Ω";
                else if (procODT == 26) mem.ProcODT = "48.0Ω";
                else if (procODT == 27) mem.ProcODT = "43.6Ω";
                else if (procODT == 56) mem.ProcODT = "40.0Ω";
                else if (procODT == 57) mem.ProcODT = "36.9Ω";
                else if (procODT == 58) mem.ProcODT = "34.3Ω";
                else if (procODT == 59) mem.ProcODT = "32.0Ω";
                else if (procODT == 62) mem.ProcODT = "30.0Ω";
                else if (procODT == 63) mem.ProcODT = "28.2Ω";
                else mem.ProcODT = "N/A";
            }

            ols.WritePciConfigDword(0x0, 0xB8, 0x3B10528);
            ols.WritePciConfigDword(0x0, 0xBC, 0x02);
            ols.WritePciConfigDword(0x00, 0xB8, SMUORG);
            Thread.Sleep(SMUDelay);

            ols.Dispose();
            return mem;
        }

        /// <summary>
        /// Helper method to write a PCI config dword and then read it back.
        /// </summary>
        private static uint ReadDword(uint value, Ols ols, int delay)
        {
            ols.WritePciConfigDword(0x00, 0xB8, value);
            Thread.Sleep(delay);
            return ols.ReadPciConfigDword(0x00, 0xBC);
        }
    }
}