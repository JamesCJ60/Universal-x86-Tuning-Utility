using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{
    /// <summary>
    /// Adapted from ZenStates's PawnIo.cs
    /// https://github.com/irusanov/ZenStates-Core/tree/master/PawnIo
    /// 
    /// Independent implementation of the PawnIO device call format:
    /// - IOCTL payload begins with a 32-byte ASCII function name (NUL-terminated if shorter)
    /// - Followed by N 64-bit signed integers (little-endian)
    /// </summary>
    public sealed class PawnIo
    {
        private const int FunctionNameBytes = 32;

        private const string DevicePath = @"\\.\PawnIO";
        private const uint ShareReadWrite = 0x00000003;

        // Device-specific control codes
        private const uint DeviceType = 41394u << 16;
        private const uint IoctlExecuteFn = 0x841 << 2;
        private const uint IoctlLoadBinary = 0x821 << 2;

        // HRESULT for invalid handle
        private const int E_HANDLE = unchecked((int)0x80070006);

        private readonly SafeFileHandle? _device;

        private static readonly Version? _installedVersion = ReadInstalledVersion();

        private PawnIo(SafeFileHandle? deviceHandle)
        {
            _device = deviceHandle;
        }

        /// <summary>True when the uninstall registry entry exists and a version can be read.</summary>
        public static bool IsInstalled => Version != null;

        /// <summary>PawnIO version if available, otherwise null.</summary>
        public static Version? Version => _installedVersion;

        /// <summary>True when the device handle exists and is usable.</summary>
        public bool IsLoaded => _device != null && !_device.IsInvalid && !_device.IsClosed;

        // --------------------------------------------------------------------
        // Module loading
        // --------------------------------------------------------------------
        public static PawnIo LoadModuleFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("PawnIO module not found.", filePath);

            byte[] moduleBytes = File.ReadAllBytes(filePath);
            return LoadModule(moduleBytes);
        }

        public static PawnIo LoadModuleFromResource(Assembly assembly, string resourceName)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name must not be empty.", nameof(resourceName));

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return new PawnIo(null);

            byte[] moduleBytes = ReadAllBytes(stream);
            return LoadModule(moduleBytes);
        }

        private static PawnIo LoadModule(byte[] moduleBytes)
        {
            IntPtr raw = CreateFile(
                DevicePath,
                FileAccess.GENERIC_READ | FileAccess.GENERIC_WRITE,
                ShareReadWrite,
                IntPtr.Zero,
                CreationDisposition.OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (raw == IntPtr.Zero || raw.ToInt64() == -1)
                return new PawnIo(null);

            try
            {
                bool ok = DeviceIoControl(
                    raw,
                    ControlCode.LoadBinary,
                    moduleBytes,
                    (uint)moduleBytes.Length,
                    null,
                    0,
                    out _,
                    IntPtr.Zero);

                if (!ok)
                {
                    CloseHandle(raw);
                    return new PawnIo(null);
                }

                return new PawnIo(new SafeFileHandle(raw, ownsHandle: true));
            }
            catch
            {
                try { CloseHandle(raw); } catch { }
                return new PawnIo(null);
            }
        }

        public void Close()
        {
            if (IsLoaded)
                _device!.Close();
        }

        // --------------------------------------------------------------------
        // Execution
        // --------------------------------------------------------------------

        /// <summary>
        /// Executes a function and returns its outputs.
        /// </summary>
        public long[] Execute(string name, long[] input, int outLength)
        {
            if (outLength < 0)
                throw new ArgumentOutOfRangeException(nameof(outLength));

            input ??= Array.Empty<long>();

            long[] outputs = new long[outLength];
            int hr = ExecuteHr(name, input, (uint)input.Length, outputs, (uint)outLength, out uint returned);

            // If it failed, return the default-sized array (matches typical “best-effort” behaviour).
            if (hr != 0 || returned == 0)
                return outputs;

            // Trim to returned length (in longs) if smaller than requested.
            if (returned < (uint)outLength)
            {
                long[] trimmed = new long[returned];
                Array.Copy(outputs, trimmed, (int)returned);
                return trimmed;
            }

            return outputs;
        }

        /// <summary>
        /// Executes a function and returns an HRESULT (0 = S_OK).
        /// </summary>
        public int ExecuteHr(string name, long[] inBuffer, uint inSize, long[] outBuffer, uint outSize, out uint returnSize)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            inBuffer ??= Array.Empty<long>();
            outBuffer ??= Array.Empty<long>();

            if (inBuffer.Length < inSize)
                throw new ArgumentOutOfRangeException(nameof(inSize));

            if (outBuffer.Length < outSize)
                throw new ArgumentOutOfRangeException(nameof(outSize));

            if (!IsLoaded)
            {
                returnSize = 0;
                return E_HANDLE;
            }

            byte[] request = BuildRequest(name, inBuffer, inSize);
            byte[] response = new byte[outSize * 8];

            bool ok = DeviceIoControl(
                _device!,
                ControlCode.Execute,
                request,
                (uint)request.Length,
                response,
                (uint)response.Length,
                out uint bytesReturned,
                IntPtr.Zero);

            if (!ok)
            {
                returnSize = 0;
                return Marshal.GetHRForLastWin32Error();
            }

            int copyBytes = Math.Min((int)bytesReturned, outBuffer.Length * 8);
            Buffer.BlockCopy(response, 0, outBuffer, 0, copyBytes);

            returnSize = bytesReturned / 8;
            return 0;
        }

        private static byte[] BuildRequest(string functionName, long[] args, uint argCount)
        {
            // Protocol layout: 32 bytes name + N * 8 bytes args.
            byte[] buffer = new byte[FunctionNameBytes + (argCount * 8)];

            // Function name section (ASCII, NUL-terminated if shorter)
            byte[] nameBytes = Encoding.ASCII.GetBytes(functionName);
            int nameCopy = Math.Min(FunctionNameBytes - 1, nameBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, buffer, 0, nameCopy);

            // Arguments section
            if (argCount > 0)
                Buffer.BlockCopy(args, 0, buffer, FunctionNameBytes, (int)argCount * 8);

            return buffer;
        }

        // --------------------------------------------------------------------
        // Version detection
        // --------------------------------------------------------------------

        private static Version? ReadInstalledVersion()
        {
            try
            {
                using RegistryKey? key =
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO");

                object? raw = key?.GetValue("DisplayVersion");
                return TryParseVersion(raw, out Version? parsed) ? parsed : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryParseVersion(object? val, out Version? version)
        {
            version = null;

            if (val is not string s || string.IsNullOrWhiteSpace(s))
                return false;

            try
            {
                version = new Version(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ReadAllBytes(Stream src)
        {
            using var ms = new MemoryStream();
            src.CopyTo(ms);
            return ms.ToArray();
        }

        // --------------------------------------------------------------------
        // Native interop
        // --------------------------------------------------------------------

        private enum ControlCode : uint
        {
            LoadBinary = DeviceType | IoctlLoadBinary,
            Execute = DeviceType | IoctlExecuteFn
        }

        private enum FileAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        private enum CreationDisposition : uint
        {
            OPEN_EXISTING = 3
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern bool DeviceIoControl(
            SafeFileHandle device,
            ControlCode ioControlCode,
            [In] byte[] inBuffer,
            uint inBufferSize,
            [Out] byte[] outBuffer,
            uint nOutBufferSize,
            out uint bytesReturned,
            IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            ControlCode dwIoControlCode,
            byte[] lpInBuffer,
            uint nInBufferSize,
            byte[]? lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}