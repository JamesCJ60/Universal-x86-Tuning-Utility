using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.ASUS
{
    public class ASUSWmi
    {
        const string FILE_NAME = @"\\.\\ATKACPI";
        const uint CONTROL_CODE = 0x0022240C;

        const uint DSTS = 0x53545344;
        const uint DEVS = 0x53564544;

        public const uint UniversalControl = 0x00100021;
        public const int KB_Light_Up = 0xc4;
        public const int KB_Light_Down = 0xc5;

        public const uint CPU_Fan = 0x00110013;
        public const uint GPU_Fan = 0x00110014;
        public const uint SYS_Fan = 0x00110031;

        public const uint PerformanceMode = 0x00120075; // Performance modes
        public const uint VivoBookMode = 0x00110019; // Vivobook performance modes

        public const uint GPUEco = 0x00090020;
        public const uint GPUMux = 0x00090016;
        public const uint eGPU = 0x00090019;
        public const uint eGPUConnected = 0x00090018;

        public const int Temp_CPU = 0x00120094;
        public const int Temp_GPU = 0x00120097;

        public const uint BatteryLimit = 0x00120057;
        public const uint ScreenOverdrive = 0x00050019;
        public const uint ScreenMultizone = 0x0005001E;

        public const uint DevsCPUFanCurve = 0x00110024;
        public const uint DevsGPUFanCurve = 0x00110025;
        public const uint DevsSYSFanCurve = 0x00110032;

        public const int PerformanceBalanced = 0;
        public const int PerformanceTurbo = 1;
        public const int PerformanceSilent = 2;

        public const int GPUModeEco = 0;
        public const int GPUModeStandard = 1;
        public const int GPUModeUltimate = 2;

        public const int PPT_TotalA0 = 0x001200A0;
        public const int PPT_EDCA1 = 0x001200A1;
        public const int PPT_TDCA2 = 0x001200A2;
        public const int PPT_APUA3 = 0x001200A3;

        public const int PPT_CPUB0 = 0x001200B0;
        public const int PPT_CPUB1 = 0x001200B1;

        public const int PPT_APUC1 = 0x001200C1;
        public const int PPT_APUC2 = 0x001200C2;

        public const int TUF_KB = 0x00100056;
        public const int TUF_KB_STATE = 0x00100057;

        public const int CPU_VOLTAGE = 0x00120079;


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;

        private IntPtr _handle;

        public ASUSWmi()
        {
            _handle = CreateFile(
                FILE_NAME,
                GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero
            );
            if (_handle == new IntPtr(-1))
            {
                throw new Exception("Can't connect to ACPI");
            }
        }

        public void Control(uint dwIoControlCode, byte[] lpInBuffer, byte[] lpOutBuffer)
        {
            uint lpBytesReturned = 0;
            bool result = DeviceIoControl(
                _handle,
                dwIoControlCode,
                lpInBuffer,
                (uint)lpInBuffer.Length,
                lpOutBuffer,
                (uint)lpOutBuffer.Length,
                ref lpBytesReturned,
                IntPtr.Zero
            );
        }

        public void Close()
        {
            CloseHandle(_handle);
        }

        private byte[] CallMethod(uint methodId, byte[] args)
        {
            byte[] acpiBuf = new byte[8 + args.Length];
            byte[] outBuffer = new byte[20];

            BitConverter.GetBytes(methodId).CopyTo(acpiBuf, 0);
            BitConverter.GetBytes((uint)args.Length).CopyTo(acpiBuf, 4);
            Array.Copy(args, 0, acpiBuf, 8, args.Length);

            Control(CONTROL_CODE, acpiBuf, outBuffer);

            return outBuffer;
        }

        public void SetFanCurve(int device, byte[] curve)
        {
            if (device == 0)
                DeviceSet(DevsCPUFanCurve, curve);
            if (device == 1)
                DeviceSet(DevsGPUFanCurve, curve);
            else
                DeviceSet(DevsSYSFanCurve, curve);
        }

        public byte[] GetFanCurve(int device, int mode = 0)
        {
            uint fan_mode;

            switch (mode)
            {
                case 1: fan_mode = 2; break;
                case 2: fan_mode = 1; break;
                default: fan_mode = 0; break;
            }

            if (device == 1)
                return DeviceGetBuffer(DevsGPUFanCurve, fan_mode);
            else
                return DeviceGetBuffer(DevsCPUFanCurve, fan_mode);
        }

        public void DeviceSet(uint deviceID, int status)
        {
            byte[] args = BitConverter.GetBytes(deviceID)
                .Concat(BitConverter.GetBytes(status))
                .ToArray();
            CallMethod(DEVS, args);
        }

        public void DeviceSet(uint deviceID, byte[] parameters)
        {
            byte[] args = BitConverter.GetBytes(deviceID)
                .Concat(parameters)
                .ToArray();
            CallMethod(DEVS, args);
        }

        public int DeviceGet(uint deviceID)
        {
            byte[] args = BitConverter.GetBytes(deviceID);
            byte[] status = CallMethod(DSTS, args);
            return BitConverter.ToInt32(status, 0) - 65536;
        }

        public int DeviceGet2(uint deviceID)
        {
            byte[] args = BitConverter.GetBytes(deviceID);
            byte[] status = CallMethod(DSTS, args);
            return BitConverter.ToInt32(status, 0);
        }

        public byte[] DeviceGetBuffer(uint deviceID, uint status = 0)
        {
            byte[] args = BitConverter.GetBytes(deviceID)
                .Concat(BitConverter.GetBytes(status))
                .ToArray();
            return CallMethod(DSTS, args);
        }

        public void TUFKeyboardRGB(int mode, Color color, int speed)
        {

            byte[] setting = new byte[12];
            setting[0] = (byte)1;
            setting[1] = (byte)mode;
            setting[2] = color.R;
            setting[3] = color.G;
            setting[4] = color.B;
            setting[5] = (byte)speed;

            DeviceSet(TUF_KB, setting);
        }

        public void SubscribeToEvents(Action<object, EventArrivedEventArgs> EventHandler)
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            watcher.EventArrived += new EventArrivedEventHandler(EventHandler);
            watcher.Scope = new ManagementScope("root\\wmi");
            watcher.Query = new WqlEventQuery("SELECT * FROM AsusAtkWmiEvent");
            watcher.Start();
        }
    }
}
