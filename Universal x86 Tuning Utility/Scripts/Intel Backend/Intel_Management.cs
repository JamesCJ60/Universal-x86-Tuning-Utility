using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.Scripts.Intel_Backend
{
    public static class Intel_Management
    {
        private static readonly object objLock = new object();
        private static readonly object initializationLock = new object();

        public static string BaseDir = Settings.Default.Path;

        private static IntelPawnIo? msrPawnIo;
        private static IntelPawnIo? mchbarPawnIo;
        private static IntelPawnIO? intelPawnIO;

        private static string? MCHBAR;

        public static void Initialise()
        {
            lock (initializationLock)
            {
                if (intelPawnIO != null && intelPawnIO.IsLoaded)
                    return;

                try
                {
                    string? msrPath = ResolveIntelModulePath("IntelMSR.bin");

                    if (msrPath == null)
                    {
                        Misc.DiagnosticLogger.LogError(
                            new FileNotFoundException("IntelMSR.bin not found."),
                            "Failed to initialise Intel PawnIO.");
                        return;
                    }

                    msrPawnIo = IntelPawnIo.LoadModuleFromFile(msrPath);

                    string? mchbarPath = ResolveIntelModulePath("IntelMCHBAR.bin");

                    if (mchbarPath != null)
                        mchbarPawnIo = IntelPawnIo.LoadModuleFromFile(mchbarPath);

                    if (msrPawnIo == null || !msrPawnIo.IsLoaded)
                    {
                        Misc.DiagnosticLogger.LogError(
                            new InvalidOperationException("IntelMSR.bin failed to load."),
                            "Failed to initialise Intel PawnIO.");
                        return;
                    }

                    intelPawnIO = new IntelPawnIO(msrPawnIo, mchbarPawnIo);
                    intelPawnIO.Open();

                    DetermineIntelMCHBAR();
                }
                catch (Exception ex)
                {
                    Misc.DiagnosticLogger.LogError(ex, "Failed to initialise Intel PawnIO.");
                }
            }
        }

        public static void Deinitialize()
        {
            lock (initializationLock)
            {
                try
                {
                    intelPawnIO?.Close();

                    msrPawnIo?.Close();
                    mchbarPawnIo?.Close();

                    intelPawnIO = null;
                    msrPawnIo = null;
                    mchbarPawnIo = null;
                    MCHBAR = null;
                }
                catch
                {
                }
            }
        }

        public static void changeTDPAll(int pl)
        {
            try
            {
                EnsureInitialised();
                runIntelTDPChangeMSR(pl, pl);
            }
            catch (Exception ex)
            {
                Misc.DiagnosticLogger.LogError(ex, "Failed to change Intel TDP MSR.");
            }
        }

        public static void changePowerBalance(int value, int cpuOrGpu)
        {
            if (value < 0 || value > 31)
                return;

            try
            {
                EnsureInitialised();

                if (cpuOrGpu == 0)
                    changePowerBalance(0x63A, value);

                if (cpuOrGpu == 1)
                    changePowerBalance(0x642, value);
            }
            catch (Exception ex)
            {
                Misc.DiagnosticLogger.LogError(ex, "Failed to change Intel power balance.");
            }
        }

        public static void changeVoltageOffset(int value, int voltagePlane)
        {
            try
            {
                EnsureInitialised();

                ulong command = voltagePlane switch
                {
                    0 => 0x80000011UL,
                    1 => 0x80000111UL,
                    2 => 0x80000211UL,
                    3 => 0x80000411UL,
                    _ => 0UL
                };

                if (command == 0)
                    return;

                ulong data = unchecked((uint)((int)Math.Round(value * 1.024) << 21));
                ulong msrValue = (command << 32) | data;

                intelPawnIO!.WriteMsr(0x150, msrValue);

                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void changeClockRatioOffset(int[] clockRatios)
        {
            try
            {
                EnsureInitialised();

                if (clockRatios == null || clockRatios.Length == 0)
                    return;

                ulong value = 0;
                int ratioCount = Math.Min(clockRatios.Length, 8);
                for (int i = 0; i < ratioCount; ++i)
                    value = (value << 8) | (byte)Math.Clamp(clockRatios[i], 0, 255);

                intelPawnIO!.WriteMsr(0x1AD, value);

                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static int[] readClockRatios()
        {
            try
            {
                EnsureInitialised();

                ulong value = intelPawnIO!.ReadMsr(0x1AD);
                int[] intParts = new int[8];

                for (int i = 0; i < 8; i++)
                    intParts[i] = (int)((value >> ((7 - i) * 8)) & 0xFF);

                return intParts;
            }
            catch
            {
                return Array.Empty<int>();
            }
        }

        public static int changeGpuClock(int value)
        {
            /*
             * Not currently supported.
             *
             */
            return -1;
        }

        private static void runIntelTDPChangeMSR(int pl1TDP, int pl2TDP)
        {
            try
            {
                EnsureInitialised();

                if (pl1TDP <= 0 || pl2TDP < 0)
                    return;

                lock (objLock)
                {
                    uint high = 0x00438000u | ((uint)(pl2TDP * 8) & 0xFFFu);
                    uint low = 0x00DD8000u | ((uint)((pl1TDP - 1) * 8) & 0xFFFu);

                    ulong value = ((ulong)high << 32) | low;

                    intelPawnIO!.WriteMsr(0x610, value);

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static void changePowerBalance(uint address, int value)
        {
            try
            {
                EnsureInitialised();

                intelPawnIO!.WriteMsr(address, unchecked((ulong)value));

                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Misc.DiagnosticLogger.LogError(ex, "Failed to change Intel power balance via MSR.");
            }
        }

        public static void checkDriverBlockRegistry()
        {
            try
            {
                RegistryKey? myKey = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Control\CI\Config",
                    true);

                if (myKey != null)
                {
                    object? value = myKey.GetValue("VulnerableDriverBlocklistEnable");

                    if (value?.ToString() == "1")
                    {
                        myKey.SetValue(
                            "VulnerableDriverBlocklistEnable",
                            "0",
                            RegistryValueKind.String);
                    }

                    myKey.Close();
                }
            }
            catch { }
        }

        public static void determineCPU()
        {
            checkDriverBlockRegistry();
            EnsureInitialised();
            DetermineIntelMCHBAR();
        }

        private static bool DetermineIntelMCHBAR()
        {
            try
            {
                EnsureInitialised();

                if (intelPawnIO == null || !intelPawnIO.HasMchbar)
                    return false;

                ulong mchbar = intelPawnIO.GetMchbarAddress();

                if (mchbar == 0)
                    return false;

                MCHBAR = "0x" + mchbar.ToString("X");

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void EnsureInitialised()
        {
            if (intelPawnIO != null && intelPawnIO.IsLoaded)
                return;

            Initialise();
        }

        private static string? ResolveIntelModulePath(string fileName)
        {
            string binName = Path.ChangeExtension(fileName, ".bin");

            string[] candidates =
            {
                Path.Combine(BaseDir, "Assets", "Intel", "PawnIO", binName),
                Path.Combine(BaseDir, "Assets", "Intel", "PawnIO", fileName),

                binName,
                fileName
            };

            return candidates.FirstOrDefault(File.Exists);
        }

    }
}
