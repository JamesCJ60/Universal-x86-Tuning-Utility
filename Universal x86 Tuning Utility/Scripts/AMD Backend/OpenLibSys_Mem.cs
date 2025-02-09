//-----------------------------------------------------------------------------
//     Author : hiyohiyo
//       Mail : hiyohiyo@crystalmark.info
//        Web : http://openlibsys.org/
//    License : The modified BSD license
//
//                     Copyright 2007-2009 OpenLibSys.org. All rights reserved.
//-----------------------------------------------------------------------------
// This is support library for WinRing0 1.3.x.

#define _PHYSICAL_MEMORY_SUPPORT

using System;
using System.Runtime.InteropServices;

namespace OpenLibSys_Mem
{
    public class Ols : IDisposable
    {
        const string dllNameX64 = "WinRing0x64_Mem.dll";
        const string dllName = "WinRing0_Mem.dll";

        // for this support library
        public enum OlsStatus
        {
            NO_ERROR = 0,
            DLL_NOT_FOUND = 1,
            DLL_INCORRECT_VERSION = 2,
            DLL_INITIALIZE_ERROR = 3,
        }

        // for WinRing0
        public enum OlsDllStatus
        {
            OLS_DLL_NO_ERROR = 0,
            OLS_DLL_UNSUPPORTED_PLATFORM = 1,
            OLS_DLL_DRIVER_NOT_LOADED = 2,
            OLS_DLL_DRIVER_NOT_FOUND = 3,
            OLS_DLL_DRIVER_UNLOADED = 4,
            OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK = 5,
            OLS_DLL_UNKNOWN_ERROR = 9
        }

        // for WinRing0
        public enum OlsDriverType
        {
            OLS_DRIVER_TYPE_UNKNOWN = 0,
            OLS_DRIVER_TYPE_WIN_9X = 1,
            OLS_DRIVER_TYPE_WIN_NT = 2,
            OLS_DRIVER_TYPE_WIN_NT4 = 3,    // Obsolete
            OLS_DRIVER_TYPE_WIN_NT_X64 = 4,
            OLS_DRIVER_TYPE_WIN_NT_IA64 = 5
        }

        // for WinRing0
        public enum OlsErrorPci : uint
        {
            OLS_ERROR_PCI_BUS_NOT_EXIST = 0xE0000001,
            OLS_ERROR_PCI_NO_DEVICE = 0xE0000002,
            OLS_ERROR_PCI_WRITE_CONFIG = 0xE0000003,
            OLS_ERROR_PCI_READ_CONFIG = 0xE0000004
        }

        // Bus Number, Device Number and Function Number to PCI Device Address
        public uint PciBusDevFunc(uint bus, uint dev, uint func)
        {
            return ((bus & 0xFF) << 8) | ((dev & 0x1F) << 3) | (func & 7);
        }

        // PCI Device Address to Bus Number
        public uint PciGetBus(uint address)
        {
            return ((address >> 8) & 0xFF);
        }

        // PCI Device Address to Device Number
        public uint PciGetDev(uint address)
        {
            return ((address >> 3) & 0x1F);
        }

        // PCI Device Address to Function Number
        public uint PciGetFunc(uint address)
        {
            return (address & 7);
        }

        [DllImport("kernel32")]
        public extern static IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        private IntPtr module = IntPtr.Zero;
        private OlsStatus status = OlsStatus.NO_ERROR;

        public Ols()
        {
            string fileName;

            if (IntPtr.Size == 8)
            {
                fileName = dllNameX64;
            }
            else
            {
                fileName = dllName;
            }

            module = LoadLibrary(fileName);
            if (module == IntPtr.Zero)
            {
                status = OlsStatus.DLL_NOT_FOUND;
            }
            else
            {
                GetDllStatus = GetDelegate<_GetDllStatus>("GetDllStatus");
                GetDllVersion = GetDelegate<_GetDllVersion>("GetDllVersion");
                GetDriverVersion = GetDelegate<_GetDriverVersion>("GetDriverVersion");
                GetDriverType = GetDelegate<_GetDriverType>("GetDriverType");

                InitializeOls = GetDelegate<_InitializeOls>("InitializeOls");
                DeinitializeOls = GetDelegate<_DeinitializeOls>("DeinitializeOls");

                IsCpuid = GetDelegate<_IsCpuid>("IsCpuid");
                IsMsr = GetDelegate<_IsMsr>("IsMsr");
                IsTsc = GetDelegate<_IsTsc>("IsTsc");
                Hlt = GetDelegate<_Hlt>("Hlt");
                HltTx = GetDelegate<_HltTx>("HltTx");
                HltPx = GetDelegate<_HltPx>("HltPx");
                Rdmsr = GetDelegate<_Rdmsr>("Rdmsr");
                RdmsrTx = GetDelegate<_RdmsrTx>("RdmsrTx");
                RdmsrPx = GetDelegate<_RdmsrPx>("RdmsrPx");
                Wrmsr = GetDelegate<_Wrmsr>("Wrmsr");
                WrmsrTx = GetDelegate<_WrmsrTx>("WrmsrTx");
                WrmsrPx = GetDelegate<_WrmsrPx>("WrmsrPx");
                Rdpmc = GetDelegate<_Rdpmc>("Rdpmc");
                RdpmcTx = GetDelegate<_RdpmcTx>("RdpmcTx");
                RdpmcPx = GetDelegate<_RdpmcPx>("RdpmcPx");
                Cpuid = GetDelegate<_Cpuid>("Cpuid");
                CpuidTx = GetDelegate<_CpuidTx>("CpuidTx");
                CpuidPx = GetDelegate<_CpuidPx>("CpuidPx");
                Rdtsc = GetDelegate<_Rdtsc>("Rdtsc");
                RdtscTx = GetDelegate<_RdtscTx>("RdtscTx");
                RdtscPx = GetDelegate<_RdtscPx>("RdtscPx");

                ReadIoPortByte = GetDelegate<_ReadIoPortByte>("ReadIoPortByte");
                ReadIoPortWord = GetDelegate<_ReadIoPortWord>("ReadIoPortWord");
                ReadIoPortDword = GetDelegate<_ReadIoPortDword>("ReadIoPortDword");
                ReadIoPortByteEx = GetDelegate<_ReadIoPortByteEx>("ReadIoPortByteEx");
                ReadIoPortWordEx = GetDelegate<_ReadIoPortWordEx>("ReadIoPortWordEx");
                ReadIoPortDwordEx = GetDelegate<_ReadIoPortDwordEx>("ReadIoPortDwordEx");

                WriteIoPortByte = GetDelegate<_WriteIoPortByte>("WriteIoPortByte");
                WriteIoPortWord = GetDelegate<_WriteIoPortWord>("WriteIoPortWord");
                WriteIoPortDword = GetDelegate<_WriteIoPortDword>("WriteIoPortDword");
                WriteIoPortByteEx = GetDelegate<_WriteIoPortByteEx>("WriteIoPortByteEx");
                WriteIoPortWordEx = GetDelegate<_WriteIoPortWordEx>("WriteIoPortWordEx");
                WriteIoPortDwordEx = GetDelegate<_WriteIoPortDwordEx>("WriteIoPortDwordEx");

                SetPciMaxBusIndex = GetDelegate<_SetPciMaxBusIndex>("SetPciMaxBusIndex");
                ReadPciConfigByte = GetDelegate<_ReadPciConfigByte>("ReadPciConfigByte");
                ReadPciConfigWord = GetDelegate<_ReadPciConfigWord>("ReadPciConfigWord");
                ReadPciConfigDword = GetDelegate<_ReadPciConfigDword>("ReadPciConfigDword");
                ReadPciConfigByteEx = GetDelegate<_ReadPciConfigByteEx>("ReadPciConfigByteEx");
                ReadPciConfigWordEx = GetDelegate<_ReadPciConfigWordEx>("ReadPciConfigWordEx");
                ReadPciConfigDwordEx = GetDelegate<_ReadPciConfigDwordEx>("ReadPciConfigDwordEx");
                WritePciConfigByte = GetDelegate<_WritePciConfigByte>("WritePciConfigByte");
                WritePciConfigWord = GetDelegate<_WritePciConfigWord>("WritePciConfigWord");
                WritePciConfigDword = GetDelegate<_WritePciConfigDword>("WritePciConfigDword");
                WritePciConfigByteEx = GetDelegate<_WritePciConfigByteEx>("WritePciConfigByteEx");
                WritePciConfigWordEx = GetDelegate<_WritePciConfigWordEx>("WritePciConfigWordEx");
                WritePciConfigDwordEx = GetDelegate<_WritePciConfigDwordEx>("WritePciConfigDwordEx");
                FindPciDeviceById = GetDelegate<_FindPciDeviceById>("FindPciDeviceById");
                FindPciDeviceByClass = GetDelegate<_FindPciDeviceByClass>("FindPciDeviceByClass");

#if _PHYSICAL_MEMORY_SUPPORT
                ReadDmiMemory = GetDelegate<_ReadDmiMemory>("ReadDmiMemory");
                ReadPhysicalMemory = GetDelegate<_ReadPhysicalMemory>("ReadPhysicalMemory");
                WritePhysicalMemory = GetDelegate<_WritePhysicalMemory>("WritePhysicalMemory");
#endif
                if (!(
                   GetDllStatus != null
                && GetDllVersion != null
                && GetDriverVersion != null
                && GetDriverType != null
                && InitializeOls != null
                && DeinitializeOls != null
                && IsCpuid != null
                && IsMsr != null
                && IsTsc != null
                && Hlt != null
                && HltTx != null
                && HltPx != null
                && Rdmsr != null
                && RdmsrTx != null
                && RdmsrPx != null
                && Wrmsr != null
                && WrmsrTx != null
                && WrmsrPx != null
                && Rdpmc != null
                && RdpmcTx != null
                && RdpmcPx != null
                && Cpuid != null
                && CpuidTx != null
                && CpuidPx != null
                && Rdtsc != null
                && RdtscTx != null
                && RdtscPx != null
                && ReadIoPortByte != null
                && ReadIoPortWord != null
                && ReadIoPortDword != null
                && ReadIoPortByteEx != null
                && ReadIoPortWordEx != null
                && ReadIoPortDwordEx != null
                && WriteIoPortByte != null
                && WriteIoPortWord != null
                && WriteIoPortDword != null
                && WriteIoPortByteEx != null
                && WriteIoPortWordEx != null
                && WriteIoPortDwordEx != null
                && SetPciMaxBusIndex != null
                && ReadPciConfigByte != null
                && ReadPciConfigWord != null
                && ReadPciConfigDword != null
                && ReadPciConfigByteEx != null
                && ReadPciConfigWordEx != null
                && ReadPciConfigDwordEx != null
                && WritePciConfigByte != null
                && WritePciConfigWord != null
                && WritePciConfigDword != null
                && WritePciConfigByteEx != null
                && WritePciConfigWordEx != null
                && WritePciConfigDwordEx != null
                && FindPciDeviceById != null
                && FindPciDeviceByClass != null
#if _PHYSICAL_MEMORY_SUPPORT
                && ReadDmiMemory != null
                && ReadPhysicalMemory != null
                && WritePhysicalMemory != null
#endif
))
                {
                    status = OlsStatus.DLL_INCORRECT_VERSION;
                }

                if (InitializeOls() == 0)
                {
                    status = OlsStatus.DLL_INITIALIZE_ERROR;
                }
            }
        }

        public OlsStatus Status => status;

        public OlsDllStatus DllStatus => (OlsDllStatus)GetDllStatus();

        public void Dispose()
        {
            if (module != IntPtr.Zero)
            {
                DeinitializeOls();
                FreeLibrary(module);
                module = IntPtr.Zero;
            }
        }

        public T GetDelegate<T>(string procName)
        {
            IntPtr ptr = GetProcAddress(module, procName);

            if (ptr != IntPtr.Zero)
                return Marshal.GetDelegateForFunctionPointer<T>(ptr);

            int result = Marshal.GetHRForLastWin32Error();
            throw Marshal.GetExceptionForHR(result);
        }

        //-----------------------------------------------------------------------------
        // DLL Information
        //-----------------------------------------------------------------------------
        public delegate uint _GetDllStatus();
        public delegate uint _GetDllVersion(ref byte major, ref byte minor, ref byte revision, ref byte release);
        public delegate uint _GetDriverVersion(ref byte major, ref byte minor, ref byte revision, ref byte release);
        public delegate uint _GetDriverType();

        public delegate int _InitializeOls();
        public delegate void _DeinitializeOls();

        public _GetDllStatus GetDllStatus = null;
        public _GetDllVersion GetDllVersion = null;
        public _GetDriverVersion GetDriverVersion = null;
        public _GetDriverType GetDriverType = null;

        public _InitializeOls InitializeOls = null;
        public _DeinitializeOls DeinitializeOls = null;

        //-----------------------------------------------------------------------------
        // CPU
        //-----------------------------------------------------------------------------
        public delegate int _IsCpuid();
        public delegate int _IsMsr();
        public delegate int _IsTsc();
        public delegate int _Hlt();
        public delegate int _HltTx(UIntPtr threadAffinityMask);
        public delegate int _HltPx(UIntPtr processAffinityMask);
        public delegate int _Rdmsr(uint index, ref uint eax, ref uint edx);
        public delegate int _RdmsrTx(uint index, ref uint eax, ref uint edx, UIntPtr threadAffinityMask);
        public delegate int _RdmsrPx(uint index, ref uint eax, ref uint edx, UIntPtr processAffinityMask);
        public delegate int _Wrmsr(uint index, uint eax, uint edx);
        public delegate int _WrmsrTx(uint index, uint eax, uint edx, UIntPtr threadAffinityMask);
        public delegate int _WrmsrPx(uint index, uint eax, uint edx, UIntPtr processAffinityMask);
        public delegate int _Rdpmc(uint index, ref uint eax, ref uint edx);
        public delegate int _RdpmcTx(uint index, ref uint eax, ref uint edx, UIntPtr threadAffinityMask);
        public delegate int _RdpmcPx(uint index, ref uint eax, ref uint edx, UIntPtr processAffinityMask);
        public delegate int _Cpuid(uint index, ref uint eax, ref uint ebx, ref uint ecx, ref uint edx);
        public delegate int _CpuidTx(uint index, ref uint eax, ref uint ebx, ref uint ecx, ref uint edx, UIntPtr threadAffinityMask);
        public delegate int _CpuidPx(uint index, ref uint eax, ref uint ebx, ref uint ecx, ref uint edx, UIntPtr processAffinityMask);
        public delegate int _Rdtsc(ref uint eax, ref uint edx);
        public delegate int _RdtscTx(ref uint eax, ref uint edx, UIntPtr threadAffinityMask);
        public delegate int _RdtscPx(ref uint eax, ref uint edx, UIntPtr processAffinityMask);

        public _IsCpuid IsCpuid = null;
        public _IsMsr IsMsr = null;
        public _IsTsc IsTsc = null;
        public _Hlt Hlt = null;
        public _HltTx HltTx = null;
        public _HltPx HltPx = null;
        public _Rdmsr Rdmsr = null;
        public _RdmsrTx RdmsrTx = null;
        public _RdmsrPx RdmsrPx = null;
        public _Wrmsr Wrmsr = null;
        public _WrmsrTx WrmsrTx = null;
        public _WrmsrPx WrmsrPx = null;
        public _Rdpmc Rdpmc = null;
        public _RdpmcTx RdpmcTx = null;
        public _RdpmcPx RdpmcPx = null;
        public _Cpuid Cpuid = null;
        public _CpuidTx CpuidTx = null;
        public _CpuidPx CpuidPx = null;
        public _Rdtsc Rdtsc = null;
        public _RdtscTx RdtscTx = null;
        public _RdtscPx RdtscPx = null;

        //-----------------------------------------------------------------------------
        // I/O
        //-----------------------------------------------------------------------------
        public delegate byte _ReadIoPortByte(ushort port);
        public delegate ushort _ReadIoPortWord(ushort port);
        public delegate uint _ReadIoPortDword(ushort port);
        public _ReadIoPortByte ReadIoPortByte;
        public _ReadIoPortWord ReadIoPortWord;
        public _ReadIoPortDword ReadIoPortDword;

        public delegate int _ReadIoPortByteEx(ushort port, ref byte value);
        public delegate int _ReadIoPortWordEx(ushort port, ref ushort value);
        public delegate int _ReadIoPortDwordEx(ushort port, ref uint value);
        public _ReadIoPortByteEx ReadIoPortByteEx;
        public _ReadIoPortWordEx ReadIoPortWordEx;
        public _ReadIoPortDwordEx ReadIoPortDwordEx;

        public delegate void _WriteIoPortByte(ushort port, byte value);
        public delegate void _WriteIoPortWord(ushort port, ushort value);
        public delegate void _WriteIoPortDword(ushort port, uint value);
        public _WriteIoPortByte WriteIoPortByte;
        public _WriteIoPortWord WriteIoPortWord;
        public _WriteIoPortDword WriteIoPortDword;

        public delegate int _WriteIoPortByteEx(ushort port, byte value);
        public delegate int _WriteIoPortWordEx(ushort port, ushort value);
        public delegate int _WriteIoPortDwordEx(ushort port, uint value);
        public _WriteIoPortByteEx WriteIoPortByteEx;
        public _WriteIoPortWordEx WriteIoPortWordEx;
        public _WriteIoPortDwordEx WriteIoPortDwordEx;

        //-----------------------------------------------------------------------------
        // PCI
        //-----------------------------------------------------------------------------
        public delegate void _SetPciMaxBusIndex(byte max);
        public _SetPciMaxBusIndex SetPciMaxBusIndex;

        public delegate byte _ReadPciConfigByte(uint pciAddress, byte regAddress);
        public delegate ushort _ReadPciConfigWord(uint pciAddress, byte regAddress);
        public delegate uint _ReadPciConfigDword(uint pciAddress, byte regAddress);
        public _ReadPciConfigByte ReadPciConfigByte;
        public _ReadPciConfigWord ReadPciConfigWord;
        public _ReadPciConfigDword ReadPciConfigDword;

        public delegate int _ReadPciConfigByteEx(uint pciAddress, uint regAddress, ref byte value);
        public delegate int _ReadPciConfigWordEx(uint pciAddress, uint regAddress, ref ushort value);
        public delegate int _ReadPciConfigDwordEx(uint pciAddress, uint regAddress, ref uint value);
        public _ReadPciConfigByteEx ReadPciConfigByteEx;
        public _ReadPciConfigWordEx ReadPciConfigWordEx;
        public _ReadPciConfigDwordEx ReadPciConfigDwordEx;

        public delegate void _WritePciConfigByte(uint pciAddress, byte regAddress, byte value);
        public delegate void _WritePciConfigWord(uint pciAddress, byte regAddress, ushort value);
        public delegate void _WritePciConfigDword(uint pciAddress, byte regAddress, uint value);
        public _WritePciConfigByte WritePciConfigByte;
        public _WritePciConfigWord WritePciConfigWord;
        public _WritePciConfigDword WritePciConfigDword;

        public delegate int _WritePciConfigByteEx(uint pciAddress, uint regAddress, byte value);
        public delegate int _WritePciConfigWordEx(uint pciAddress, uint regAddress, ushort value);
        public delegate int _WritePciConfigDwordEx(uint pciAddress, uint regAddress, uint value);
        public _WritePciConfigByteEx WritePciConfigByteEx;
        public _WritePciConfigWordEx WritePciConfigWordEx;
        public _WritePciConfigDwordEx WritePciConfigDwordEx;

        public delegate uint _FindPciDeviceById(ushort vendorId, ushort deviceId, byte index);
        public delegate uint _FindPciDeviceByClass(byte baseClass, byte subClass, byte programIf, byte index);
        public _FindPciDeviceById FindPciDeviceById;
        public _FindPciDeviceByClass FindPciDeviceByClass;

        //-----------------------------------------------------------------------------
        // Physical Memory (unsafe)
        //-----------------------------------------------------------------------------
#if _PHYSICAL_MEMORY_SUPPORT
        public unsafe delegate uint _ReadDmiMemory(byte* buffer, uint count, uint unitSize);
        public _ReadDmiMemory ReadDmiMemory;

        public unsafe delegate uint _ReadPhysicalMemory(UIntPtr address, byte* buffer, uint count, uint unitSize);
        public unsafe delegate uint _WritePhysicalMemory(UIntPtr address, byte* buffer, uint count, uint unitSize);

        public _ReadPhysicalMemory ReadPhysicalMemory;
        public _WritePhysicalMemory WritePhysicalMemory;

        public uint GetPhysLong(UIntPtr address)
        {
            unsafe
            {
                byte* buf = stackalloc byte[4];

                //Console.ReadKey();

                uint res2 = 0;

                var res = ReadPhysicalMemory(address, (byte*)&res2, 1, 4);
                //var res = ReadPhysicalMemory(address, buf, 1, 4);

                int error = Marshal.GetLastWin32Error();

                return res2;
                //return *(uint*)buf;
            }
        }
#endif
    }
}
