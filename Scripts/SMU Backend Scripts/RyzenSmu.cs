using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenLibSys;
using System.Reflection;
using RyzenSMUBackend;
using UXTU.Properties;
using System.Windows.Forms;

[assembly: CLSCompliant(false)]


namespace RyzenSmu
{
    class Smu
    {
        [DllImport("inpoutx64.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysLong(UIntPtr memAddress, out uint Data);
        public static float ReadFloat(uint Address, uint Offset)
        {
            uint Data = 0;
            try
            {
               GetPhysLong((UIntPtr)(Address + Offset * 4), out Data);
            }
            catch(Exception e)
            {
                String ExeptionMSG = $"Error Reading Address 0x{Address:X8} + 0x{Offset:X4}";
                //MessageBox.Show(ExeptionMSG);
            }

            byte[] bytes = new byte[4];
            bytes = BitConverter.GetBytes(Data);

            float PmData = BitConverter.ToSingle(bytes, 0);
            //Console.WriteLine($"0x{Address + Offset * 4,8:X8} | {PmData:F}");
            return PmData;
        }

        public static float ReadFloat64(ulong Address, uint Offset)
        {
            uint Data = 0;
            try
            {
                GetPhysLong((UIntPtr)(Address + Offset * 4), out Data);
            }
            catch (Exception e)
            {
                String ExeptionMSG = $"Error Reading Address 0x{Address:X8} + 0x{Offset:X4} \n{e}";
                MessageBox.Show(ExeptionMSG);
            }

            byte[] bytes = new byte[4];
            bytes = BitConverter.GetBytes(Data);

            float PmData = BitConverter.ToSingle(bytes, 0);
            Console.WriteLine($"0x{Address + Offset * 4,8:X8} | {PmData:F}");
            return PmData;
        }

        public enum Status : int
        {
            BAD = 0x0,
            OK = 0x1,
            FAILED = 0xFF,
            UNKNOWN_CMD = 0xFE,
            CMD_REJECTED_PREREQ = 0xFD,
            CMD_REJECTED_BUSY = 0xFC
        }

        private static readonly Dictionary<Smu.Status, String> status = new Dictionary<Smu.Status, string>()
        {
            { Smu.Status.BAD, "BAD" },
            { Smu.Status.OK, "OK" },
            { Smu.Status.FAILED, "Failed" },
            { Smu.Status.UNKNOWN_CMD, "Unknown Command" },
            { Smu.Status.CMD_REJECTED_PREREQ, "CMD Rejected Prereq" },
            { Smu.Status.CMD_REJECTED_BUSY, "CMD Rejected Busy" }
        };


        
        Ols RyzenNbAccesss;
        

        public Smu(bool EnableDebug)
        {
            ShowDebug = EnableDebug;
            RyzenNbAccesss = new Ols();

            // Check WinRing0 status
            switch (RyzenNbAccesss.GetDllStatus())
            {
                case (uint)Ols.OlsDllStatus.OLS_DLL_NO_ERROR:
                    if (ShowDebug)
                    {
                        //MessageBox.Show("Ols Dll is OK.", "Ols.OlsDllStatus:");
                    }
                    break;
                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED:
                    //MessageBox.Show("WinRing OLS_DRIVER_NOT_LOADED", "Ols.OlsDllStatus:");
                    throw new ApplicationException("WinRing OLS_DRIVER_NOT_LOADED");

                case (uint)Ols.OlsDllStatus.OLS_DLL_UNSUPPORTED_PLATFORM:
                    //MessageBox.Show("WinRing OLS_UNSUPPORTED_PLATFORM", "Ols.OlsDllStatus:");
                    throw new ApplicationException("WinRing OLS_UNSUPPORTED_PLATFORM");

                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_FOUND:
                    //MessageBox.Show("WinRing OLS_DLL_DRIVER_NOT_FOUND", "Ols.OlsDllStatus:");
                    throw new ApplicationException("WinRing OLS_DLL_DRIVER_NOT_FOUND");

                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_UNLOADED:
                    //MessageBox.Show("WinRing OLS_DLL_DRIVER_UNLOADED", "Ols.OlsDllStatus:");
                    throw new ApplicationException("WinRing OLS_DLL_DRIVER_UNLOADED");

                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK:
                    //MessageBox.Show("WinRing DRIVER_NOT_LOADED_ON_NETWORK", "Ols.OlsDllStatus:");
                    throw new ApplicationException("WinRing DRIVER_NOT_LOADED_ON_NETWORK");

                case (uint)Ols.OlsDllStatus.OLS_DLL_UNKNOWN_ERROR:
                    //MessageBox.Show("WinRing OLS_DLL_UNKNOWN_ERROR", "Ols.OlsDllStatus:");
                    throw new ApplicationException("WinRing OLS_DLL_UNKNOWN_ERROR");
            }
            
        }

        public void Initialize()
        {
            amdSmuMutex = new Mutex();
            RyzenNbAccesss.InitializeOls();

            // Check WinRing0 status
            switch (RyzenNbAccesss.GetStatus())
            {
                case (uint)Ols.Status.NO_ERROR:
                    if (ShowDebug)
                    {
                        //MessageBox.Show("Ols is OK.", "Ols.Status:");
                        ShowDebug = false;
                    }
                    break;
                case (uint)Ols.Status.DLL_NOT_FOUND:
                    //MessageBox.Show("WinRing Status: DLL_NOT_FOUND", "Ols.Status:");
                    throw new ApplicationException("WinRing DLL_NOT_FOUND");
                    break;
                case (uint)Ols.Status.DLL_INCORRECT_VERSION:
                    //MessageBox.Show("WinRing Status: DLL_INCORRECT_VERSION", "Ols.Status:");
                    throw new ApplicationException("WinRing DLL_INCORRECT_VERSION");
                    break;
                case (uint)Ols.Status.DLL_INITIALIZE_ERROR:
                    //MessageBox.Show("WinRing Status: DLL_INITIALIZE_ERROR", "Ols.Status:");
                    throw new ApplicationException("WinRing DLL_INITIALIZE_ERROR");
                    break;
                default:
                    break;
            }
        }
        public uint GetCpuId()
        {
            uint eax = 0, ebx = 0, ecx = 0, edx = 0;
            if (RyzenNbAccesss.Cpuid(0x00000001, ref eax, ref ebx, ref ecx, ref edx) == 1)
            {
                return eax;
            }
            return 0;
        }

        public void Deinitialize()
        {
            RyzenNbAccesss.DeinitializeOls();
        }

        public static uint SMU_PCI_ADDR { get; set; }
        public static uint SMU_OFFSET_ADDR { get; set; }
        public static uint SMU_OFFSET_DATA { get; set; }

        public static uint MP1_ADDR_MSG { get; set; }
        public static uint MP1_ADDR_RSP { get; set; }
        public static uint MP1_ADDR_ARG { get; set; }

        public static uint PSMU_ADDR_MSG { get; set; }
        public static uint PSMU_ADDR_RSP { get; set; }
        public static uint PSMU_ADDR_ARG { get; set; }
        public static uint[] args { get; set; }

        public bool ShowDebug { get; set; }

        private static Mutex amdSmuMutex;
        private const ushort SMU_TIMEOUT = 8192;

        public Status SendMp1(uint message, ref uint[] arguments)
        {
            return SendMsg(MP1_ADDR_MSG, MP1_ADDR_RSP, MP1_ADDR_ARG, message, ref arguments);
        }

        public Status SendPsmu(uint message, ref uint[] arguments)
        {
            return SendMsg(PSMU_ADDR_MSG, PSMU_ADDR_RSP, PSMU_ADDR_ARG, message, ref arguments);
        }

        public Status SendMp164(uint message, ref ulong[] arguments)
        {
            return SendMsg64(MP1_ADDR_MSG, MP1_ADDR_RSP, MP1_ADDR_ARG, message, ref arguments);
        }

        public Status SendPsmu64(uint message, ref ulong[] arguments)
        {
            return SendMsg64(PSMU_ADDR_MSG, PSMU_ADDR_RSP, PSMU_ADDR_ARG, message, ref arguments);
        }

        public bool SendSmuCommand(uint SMU_ADDR_MSG, uint SMU_ADDR_RSP, uint SMU_ADDR_ARG, uint msg, ref uint[] args)
        {
            return (SendMsg(SMU_ADDR_MSG, SMU_ADDR_RSP, SMU_ADDR_ARG, msg, ref args) == Smu.Status.OK);
        }

        public Status SendMsg(uint SMU_ADDR_MSG, uint SMU_ADDR_RSP, uint SMU_ADDR_ARG, uint msg, ref uint[] args)
        {
            ushort timeout = SMU_TIMEOUT;
            uint[] cmdArgs = new uint[6];
            int argsLength = args.Length;
            uint status = 0;

            if (argsLength > cmdArgs.Length)
                argsLength = cmdArgs.Length;

            for (int i = 0; i < argsLength; ++i)
                cmdArgs[i] = args[i];

            if (amdSmuMutex.WaitOne(5000))
            {
                // Clear response register
                bool temp;
                do
                    temp = SmuWriteReg(SMU_ADDR_RSP, 0);
                while ((!temp) && --timeout > 0);

                if (timeout == 0)
                {
                    amdSmuMutex.ReleaseMutex();
                    SmuReadReg(SMU_ADDR_RSP, ref status);
                    return (Status)status;
                }

                // Write data
                for (int i = 0; i < cmdArgs.Length; ++i)
                    SmuWriteReg(SMU_ADDR_ARG + (uint)(i * 4), cmdArgs[i]);

                // Send message
                SmuWriteReg(SMU_ADDR_MSG, msg);

                // Wait done
                if (!SmuWaitDone(SMU_ADDR_RSP))
                {
                    amdSmuMutex.ReleaseMutex();
                    SmuReadReg(SMU_ADDR_RSP, ref status);
                    return (Status)status;
                }

                // Read back args
                for (int i = 0; i < args.Length; ++i)
                    SmuReadReg(SMU_ADDR_ARG + (uint)(i * 4), ref args[i]);
            }

            amdSmuMutex.ReleaseMutex();
            SmuReadReg(SMU_ADDR_RSP, ref status);

            return (Status)status;
        }

        public Status SendMsg64(uint SMU_ADDR_MSG, uint SMU_ADDR_RSP, uint SMU_ADDR_ARG, uint msg, ref ulong[] args)
        {
            ushort timeout = SMU_TIMEOUT;
            ulong[] cmdArgs = new ulong[6];
            int argsLength = args.Length;
            uint status = 0;

            if (argsLength > cmdArgs.Length)
                argsLength = cmdArgs.Length;

            for (int i = 0; i < argsLength; ++i)
                cmdArgs[i] = args[i];

            if (amdSmuMutex.WaitOne(5000))
            {
                // Clear response register
                bool temp;
                do
                    temp = SmuWriteReg(SMU_ADDR_RSP, 0);
                while ((!temp) && --timeout > 0);

                if (timeout == 0)
                {
                    amdSmuMutex.ReleaseMutex();
                    SmuReadReg(SMU_ADDR_RSP, ref status);
                    return (Status)status;
                }

                // Write data
                for (int i = 0; i < cmdArgs.Length; ++i)
                    SmuWriteReg64(SMU_ADDR_ARG + (uint)(i * 4), cmdArgs[i]);

                // Send message
                SmuWriteReg64(SMU_ADDR_MSG, msg);

                // Wait done
                if (!SmuWaitDone(SMU_ADDR_RSP))
                {
                    amdSmuMutex.ReleaseMutex();
                    SmuReadReg(SMU_ADDR_RSP, ref status);
                    return (Status)status;
                }

                // Read back args
                for (int i = 0; i < args.Length; ++i)
                    SmuReadReg64(SMU_ADDR_ARG + (uint)(i * 4), ref args[i]);
            }

            amdSmuMutex.ReleaseMutex();
            SmuReadReg(SMU_ADDR_RSP, ref status);

            return (Status)status;
        }

        public bool SmuWaitDone(uint SMU_ADDR_RSP)
        {
            bool res;
            ushort timeout = SMU_TIMEOUT;
            uint data = 0;

            do
                res = SmuReadReg(SMU_ADDR_RSP, ref data);
            while ((!res || data != 1) && --timeout > 0);

            if (timeout == 0 || data != 1) res = false;

            return res;
        }


        private bool SmuWriteReg(uint addr, uint data)
        {
            if (RyzenNbAccesss.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_ADDR, addr) == 1)
            {
                return RyzenNbAccesss.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_DATA, data) == 1;
            }
            return false;
        }

        private bool SmuReadReg(uint addr, ref uint data)
        {
            if (RyzenNbAccesss.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_ADDR, addr) == 1)
            {
                return RyzenNbAccesss.ReadPciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_DATA, ref data) == 1;
            }
            return false;
        }

        private bool SmuWriteReg64(uint addr, ulong data)
        {
            if (RyzenNbAccesss.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_ADDR, addr) == 1)
            {
                return RyzenNbAccesss.WritePciConfigDwordEx64(SMU_PCI_ADDR, SMU_OFFSET_DATA, data) == 1;
            }
            return false;
        }

        private bool SmuReadReg64(uint addr, ref ulong data)
        {
            if (RyzenNbAccesss.WritePciConfigDwordEx(SMU_PCI_ADDR, SMU_OFFSET_ADDR, addr) == 1)
            {
                return RyzenNbAccesss.ReadPciConfigDwordEx64(SMU_PCI_ADDR, SMU_OFFSET_DATA, ref data) == 1;
            }
            return false;
        }
        private uint ReadDword(uint value)
        {
            RyzenNbAccesss.WritePciConfigDword(SMU_PCI_ADDR, (byte)SMU_OFFSET_ADDR, value);
            return RyzenNbAccesss.ReadPciConfigDword(SMU_PCI_ADDR, (byte)SMU_OFFSET_DATA);
        }


        private bool Wait4Rsp(uint SMU_ADDR_RSP)
        {
            bool res = false;
            ushort timeout = 1000;
            uint data = 0;
            while ((!res || data == 0) && --timeout > 0)
            {
                res = SmuReadReg(SMU_ADDR_RSP, ref data);
                Thread.Sleep(1);
            }

            //Console.WriteLine($"Time{(timeout):D}: 0x{data,8:X8}");


            if (timeout == 0 || data != 1) res = false;
            //Console.WriteLine($"Res{(res):D}");


            return res;
        }


        //Code by I.nfraR.ed to get the CpuID
        private string GetStringPart(uint val)
        {
            return val != 0 ? Convert.ToChar(val).ToString() : "";
        }

        private string IntToStr(uint val)
        {
            uint part1 = val & 0xff;
            uint part2 = val >> 8 & 0xff;
            uint part3 = val >> 16 & 0xff;
            uint part4 = val >> 24 & 0xff;

            return string.Format("{0}{1}{2}{3}", GetStringPart(part1), GetStringPart(part2), GetStringPart(part3), GetStringPart(part4));
        }

        public string GetCpuName()
        {
            string model = "";
            uint eax = 0, ebx = 0, ecx = 0, edx = 0;

            if (RyzenNbAccesss.Cpuid(0x80000002, ref eax, ref ebx, ref ecx, ref edx) == 1)
                model = model + IntToStr(eax) + IntToStr(ebx) + IntToStr(ecx) + IntToStr(edx);

            if (RyzenNbAccesss.Cpuid(0x80000003, ref eax, ref ebx, ref ecx, ref edx) == 1)
                model = model + IntToStr(eax) + IntToStr(ebx) + IntToStr(ecx) + IntToStr(edx);

            if (RyzenNbAccesss.Cpuid(0x80000004, ref eax, ref ebx, ref ecx, ref edx) == 1)
                model = model + IntToStr(eax) + IntToStr(ebx) + IntToStr(ecx) + IntToStr(edx);

            return model.Trim();
        }
    }
}
