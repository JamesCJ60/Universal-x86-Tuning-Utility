using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Properties;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Universal_x86_Tuning_Utility.Scripts.Intel_Backend
{

    public static class Intel_Management
    {
        private static System.Object objLock = new System.Object();
        public static string BaseDir = Settings.Default.Path;

        public static void changeTDPAll(int pl)
        {
            try
            {
                runIntelTDPChangeMMIOKX(pl, pl);
                runIntelTDPChangeMSR(pl, pl);
            }
            catch (Exception ex) { }
        }

        public static void changePowerBalance(int value, int cpuOrGpu)
        {
            if (cpuOrGpu == 0) if (value >= 0 && value <= 31) changePowerBalance("0x0000063a 0x00000000", value);
            if (cpuOrGpu == 1) if (value >= 0 && value <= 31) changePowerBalance("0x00000642 0x00000000", value);
        }

        public static void changeVoltageOffset(int value, int voltagePlane)
        {
            string processMSR = "";
            string commandArguments = "";
            try
            {
                if (voltagePlane == 0) commandArguments += $"-s write 0x150 0x80000011 0x{convertVoltageToHexMSR(value)};"; // CPU
                if (voltagePlane == 1) commandArguments += $"-s write 0x150 0x80000111 0x{convertVoltageToHexMSR(value)};"; // iGPU
                if (voltagePlane == 2) commandArguments += $"-s write 0x150 0x80000211 0x{convertVoltageToHexMSR(value)};"; // Cache
                if (voltagePlane == 3) commandArguments += $"-s write 0x150 0x80000411 0x{convertVoltageToHexMSR(value)};"; // SA

                processMSR = BaseDir + "Assets\\Intel\\MSR\\msr-cmd.exe";

                Run_CLI.RunCommand(commandArguments, false, processMSR);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            Task.Delay(100);
        }

        public static void changeClockRatioOffset(int[] clockRatios)
        {
            string processMSR = "";
            string commandArguments = "";
            try
            {
                string hexValue = "";
                for (int i = 0; i < clockRatios.Length; ++i) hexValue += clockRatios[i].ToString("X2");
                commandArguments += $"-s write 0x1AD 0x0 0x{hexValue};";
                processMSR = BaseDir + "Assets\\Intel\\MSR\\msr-cmd.exe";
                Run_CLI.RunCommand(commandArguments, false, processMSR);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            Task.Delay(100);
        }

        public static int[] readClockRatios()
        {
            string processMSR = "";
            string commandArguments = "";
            string output = "";

            try
            {
                commandArguments += $" read 0x1AD;";
                processMSR = BaseDir + "Assets\\Intel\\MSR\\msr-cmd.exe";
                output = Run_CLI.RunCommand(commandArguments, true, processMSR);
            }
            catch (Exception ex)
            {
                return new int[0];
            }

            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2) return new int[0];
            
            string secondLine = lines[1];
            string[] parts = secondLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2) return new int[0];
            
            string hexValue = parts[parts.Length - 1];
            hexValue = hexValue.Substring(2);

            int numberOfParts = hexValue.Length / 2;
            string[] hexParts = new string[numberOfParts];
            int[] intParts = new int[numberOfParts];

            for (int i = 0; i < numberOfParts; i++)
            {
                hexParts[i] = hexValue.Substring(i * 2, 2);
                intParts[i] = Convert.ToInt32(hexParts[i], 16);
            }

            return intParts;
        }



        public static int changeGpuClock(int value)
        {
            string processKX = "";
            string commandArguments = "";
            string clockHex = "";

            try
            {
                processKX = BaseDir + "Assets\\Intel\\KX\\KX.exe";
                clockHex = convertClockToHexMMIO(value);

                commandArguments = "/wrmem8 " + MCHBAR + "5994 " + clockHex;

                Run_CLI.RunCommand(commandArguments, true, processKX);
                Task.Delay(100);
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }

            return 0;
        }

        static void runIntelTDPChangeMMIOKX(int pl1TDP, int pl2TDP)
        {
            string processKX = "";
            string hexPL1 = "";
            string hexPL2 = "";
            string commandArgumentsPL1 = "";
            string commandArgumentsPL2 = "";
            try
            {

                processKX = BaseDir + "Assets\\Intel\\KX\\KX.exe";
                hexPL1 = convertTDPToHexMMIO(pl1TDP);
                hexPL2 = convertTDPToHexMMIO(pl2TDP);
                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    lock (objLock)
                    {
                        commandArgumentsPL1 = " /wrmem16 " + MCHBAR + "a0 0x" + hexPL1;
                        if (pl1TDP > 0) Run_CLI.RunCommand(commandArgumentsPL1, true, processKX);
                        Task.Delay(500);
                        if (pl2TDP > 0) commandArgumentsPL2 = " /wrmem16 " + MCHBAR + "a4 0x" + hexPL2;
                        Run_CLI.RunCommand(commandArgumentsPL2, true, processKX);
                        Task.Delay(100);
                    }
                }
            }
            catch (Exception ex) { }
        }


        static void runIntelTDPChangeMSR(int pl1TDP, int pl2TDP)
        {

            string processMSR = "";
            string hexPL1 = "";
            string hexPL2 = "";
            string commandArguments = "";
            try
            {

                hexPL1 = convertTDPToHexMSR(pl1TDP);
                hexPL2 = convertTDPToHexMSR(pl2TDP);

                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    lock (objLock)
                    {
                        if (hexPL1.Length < 3)
                        {
                            if (hexPL1.Length == 1) { hexPL1 = "00" + hexPL1; }
                            if (hexPL1.Length == 2) { hexPL1 = "0" + hexPL1; }
                        }
                        if (hexPL2.Length < 3)
                        {
                            if (hexPL2.Length == 1) { hexPL2 = "00" + hexPL2; }
                            if (hexPL2.Length == 2) { hexPL2 = "0" + hexPL2; }
                        }

                        commandArguments = " -s write 0x610 0x00438" + hexPL2 + " 0x00DD8" + hexPL1;
                        processMSR = BaseDir + "Assets\\Intel\\MSR\\msr-cmd.exe";
                        Run_CLI.RunCommand(commandArguments, false, processMSR);

                        Task.Delay(100);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }


        static void changePowerBalance(string address, int value)
        {

            string processMSR = "";
            string commandArguments = "";
            try
            {
                string hexvalue;
                hexvalue = "0x" + value.ToString("X");

                commandArguments = " -s write " + address + " " + hexvalue;
                processMSR = BaseDir + "Assets\\Intel\\MSR\\msr-cmd.exe";

                Run_CLI.RunCommand(commandArguments, false, processMSR);
                Task.Delay(100);
            }
            catch (Exception ex) { }
        }

        public static void checkDriverBlockRegistry()
        {

            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\CI\\Config", true);
            if (myKey != null)
            {
                if (myKey.GetValue("VulnerableDriverBlocklistEnable") == "1")
                {
                    myKey.SetValue("VulnerableDriverBlocklistEnable", "0", RegistryValueKind.String);
                }
                myKey.Close();
            }
        }

        public static void determineCPU()
        {
            DetermineIntelMCHBAR();
        }

        static string MCHBAR = null;

        static bool DetermineIntelMCHBAR()
        {
            string processKX = BaseDir + "Assets\\Intel\\KX\\KX.exe";

            if (!File.Exists(processKX))
                return false;

            ProcessStartInfo startInfo = new ProcessStartInfo(processKX)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = "/RdPci32 0 0 0 0x48"
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string output = process.StandardOutput.ReadLine();
                        if (output.Contains("Return"))
                        {
                            string mchbarValue = Between(output, "Return ");
                            MCHBAR = "0x" + long.Parse(mchbarValue).ToString("X2").Substring(0, 4);
                            process.Close();
                            return true;
                        }
                    }
                    process.Close();
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private static string Between(string str, string firstString, string lastString = null, bool keepBorders = false)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            int startIndex = str.IndexOf(firstString) + firstString.Length;
            int endIndex = str.Length;

            if (lastString != null)
                endIndex = str.IndexOf(lastString, startIndex);

            string result = str.Substring(startIndex, endIndex - startIndex);

            return keepBorders ? firstString + result + lastString : result;
        }


        static string convertTDPToHexMMIO(int tdp)
        {
            try
            {
                int newTDP = (tdp * 1000 / 125) + 32768;
                return newTDP.ToString("X");

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        static string convertTDPToHexMSR(int tdp)
        {
            try
            {
                int newTDP = (tdp * 8);
                return newTDP.ToString("X");

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        static string convertVoltageToHexMSR(int volt)
        {
            double hex = volt * 1.024;
            volt = (int)Math.Round(hex) << 21;
            return hex.ToString("X");
        }
        
        static string convertClockToHexMMIO(int value)
        {
            value /= 50;
            return "0x" + value.ToString("X2");
        }
    }
}
