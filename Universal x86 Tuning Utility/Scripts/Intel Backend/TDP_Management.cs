using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.Scripts.Intel_Backend
{

    public static class TDP_Management
    {
        private static Object objLock = new Object();
        public static string BaseDir = Settings.Default.Path;

        //Change TDP routines - Intel
        public static void changeTDPAll(int pl)
        {
            //Return Success as default value, otherwise alert calling routine to error
            try
            {
                determineCPU();
                runIntelTDPChangeMMIOKX(pl, pl);
                runIntelTDPChangeMSR(pl, pl);
            }
            catch (Exception ex)
            {

            }

        }
        public static void changePL1(int pl)
        {
            //Return Success as default value, otherwise alert calling routine to error
            try
            {
                determineCPU();
                runIntelTDPChangeMMIOKX(pl, 0);
                runIntelTDPChangeMSR(pl, 0);
            }
            catch (Exception ex)
            {

            }

        }

        public static void changePL2(int pl)
        {
            //Return Success as default value, otherwise alert calling routine to error
            try
            {
                determineCPU();
                runIntelTDPChangeMMIOKX(0, pl);
                runIntelTDPChangeMSR(0, pl);
            }
            catch (Exception ex)
            {

            }

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
                //Log_Writer.writeLog("Change TDP MMIO processKX=" + processKX + "; Hex PL1 PL2=" + hexPL1 + "," + hexPL2 );
                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    lock (objLock)
                    {
                        commandArgumentsPL1 = " /wrmem16 " + MCHBAR + "a0 0x" + hexPL1;
                        //Log_Writer.writeLog("Change TDP MMIO commandargumentPL1=" + commandArgumentsPL1);
                        if (pl1TDP > 0) Run_CLI.RunCommand(commandArgumentsPL1, true, processKX);
                        Task.Delay(500);
                        if(pl2TDP > 0) commandArgumentsPL2 = " /wrmem16 " + MCHBAR + "a4 0x" + hexPL2;
                        //Log_Writer.writeLog("Change TDP MMIO commandargumentPL2=" + commandArgumentsPL2);
                        Run_CLI.RunCommand(commandArgumentsPL2, true, processKX);
                        //Log_Writer.writeLog("Change TDP MMIO complete");
                        Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {

            }
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

                        if (pl1TDP > 0 && pl2TDP <= 0) commandArguments = " -s write 0x610 0x00dd8" + hexPL1;
                        if (pl1TDP <= 0 && pl2TDP > 0) commandArguments = " -s write 0x610 0x00438" + hexPL2;
                        if (pl1TDP > 0 && pl2TDP > 0) commandArguments = " -s write 0x610 0x00438" + hexPL2 + " 0x00dd8" + hexPL1;
                        processMSR = BaseDir + "Assets\\Intel\\MSR\\msr-cmd.exe";
                        //Log_Writer.writeLog("Change TDP MSR processMSR=" + processMSR + "; Hex PL1 PL2=" + hexPL1 + "," + hexPL2);
                        Run_CLI.RunCommand(commandArguments, false, processMSR);
                        //Log_Writer.writeLog("Change TDP MSR complete");
                        Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {


            }


        }
        //End change TDP routines

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
            //Get the processor name to determine intel vs AMD
            object processorNameRegistry = Registry.GetValue("HKEY_LOCAL_MACHINE\\hardware\\description\\system\\centralprocessor\\0", "ProcessorNameString", null);
            string processorName;

            if (processorNameRegistry != null)
            {
                determineIntelMCHBAR();
            }
        }

        static string MCHBAR = null;

        static void determineIntelMCHBAR()
        {

            //Get the processor model to determine MCHBAR, INTEL ONLY
            object processorModelRegistry = Registry.GetValue("HKEY_LOCAL_MACHINE\\hardware\\description\\system\\centralprocessor\\0", "Identifier", null);
            string processorModel = null;
            if (processorModelRegistry != null)
            {
                //If not null, convert to string and determine MCHBAR for rw.exe
                processorModel = processorModelRegistry.ToString();
                if (processorModel.IndexOf("Model 140") >= 0) { MCHBAR = "0xFEDC59"; } else { MCHBAR = "0xFED159"; };
            }

        }

        //MMIO Stuff here
        static string convertTDPToHexMMIO(int tdp)
        {
            //Convert integer TDP value to Hex for rw.exe
            //Must use formula (TDP in watt   *1000/125) +32768 and convert to hex
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
            //Convert integer TDP value to Hex for rw.exe
            //Must use formula (TDP in watt   *1000/125) +32768 and convert to hex
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
    }
}
