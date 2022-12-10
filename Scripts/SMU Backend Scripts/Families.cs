using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace RyzenSMUBackend
{
    internal class Families
    {
        public static string[] FAM = { "RAVEN", "PICASSO", "DALI", "RENOIR/LUCIENNE", "MATISSE", "VANGOGH", "VERMEER", "CEZANNE/BARCELO", "REMBRANDT", "PHOENIX", "RAPHAEL/DRAGON RANGE" };
        public static int FAMID { get; protected set; }

        public static string CPUModel = "";

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

        public static void SetFam()
        {
            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in myProcessorObject.Get())
            {
                CPUModel = obj["Caption"].ToString();
            }


            FAMID = 99999;

            if (CPUModel.Contains("Model " + Convert.ToString(1)) || CPUModel.Contains("Model " + Convert.ToString(8)))
            {
                FAMID = -1; //Zen1/+ DT
            }

            if (CPUModel.Contains("Model " + Convert.ToString(17)))
            {
                FAMID = 0; //RAVEN
            }

            if (CPUModel.Contains("Model " + Convert.ToString(24)))
            {
                FAMID = 1; //PICASSO
            }

            if (CPUModel.Contains("Model " + Convert.ToString(32)))
            {
                FAMID = 2; //DALI
            }

            if (CPUModel.Contains("Model " + Convert.ToString(33)))
            {
                FAMID = 6; //VERMEER
            }

            if (CPUModel.Contains("Model " + Convert.ToString(96)) || CPUModel.Contains("Model " + Convert.ToString(104)))
            {
                FAMID = 3; //RENOIR/LUCIENNE
            }

            if (CPUModel.Contains("Model " + Convert.ToString(144)))
            {
                FAMID = 5; //VANGOGH
            }

            if (CPUModel.Contains("Model " + Convert.ToString(80)))
            {
                FAMID = 7; //CEZANNE/BARCELO
            }

            if (CPUModel.Contains("Model " + Convert.ToString(64)) || CPUModel.Contains("Model " + Convert.ToString(68)))
            {
                FAMID = 8; //REMBRANDT
            }

            if (CPUModel.Contains("Model " + Convert.ToString(97)))
            {
                FAMID = 10; //RAPHAEL/DRAGON RANGE
            }


            Addresses.SetAddresses();
        }
    }
}
