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

        /// <summary>
        /// Retrieves and calculates all memory timings using the OpenLibSys API.
        /// The method does not change any of the PCI/HEX IDs.
        /// </summary>
        /// <returns>An instance of Mem_Timings with all values set.</returns>
        public static Mem_Timings RetrieveTimings()
        {
            Mem_Timings mem = new Mem_Timings();

            // These values must not be changed.
            bool SMUSlow = false;
            int SMUDelay = SMUSlow ? 60 : 10;

            Ols ols = new Ols();
            Ols.OlsStatus status = ols.Status;
            Ols.OlsDllStatus dllStatus = ols.DllStatus;
            if (status != Ols.OlsStatus.NO_ERROR)
                throw new ApplicationException($"OlsStatus error:\nstatus {status}\ndllStatus {dllStatus}");
            if (dllStatus != Ols.OlsDllStatus.OLS_DLL_NO_ERROR)
                throw new ApplicationException($"OlsDllStatus error:\nstatus {status}\ndllStatus {dllStatus}");

            // Get CPU information.
            uint eax = 0, ebx = 0, ecx = 0, edx = 0;
            ols.CpuidPx(0x80000001, ref eax, ref ebx, ref ecx, ref edx, (UIntPtr)0x01);
            uint CPUFMS = eax & 0xFFFF00;

            // Read SMUORG register.
            uint SMUORG = ols.ReadPciConfigDword(0x00, 0xB8);
            Thread.Sleep(SMUDelay);

            // Determine offset.
            uint someOffset = ReadDword(0x50200, ols, SMUDelay) == 0x300 ? 0x100000u : 0u;

            // Read registers (do not modify the HEX values).
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

            // Determine which timing values to use.
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

            // Calculate timing values.
            mem.BGS = BGS != 0x87654321;
            mem.BGSA = BGSA == 0x111107F1;
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

            // Restore registers.
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