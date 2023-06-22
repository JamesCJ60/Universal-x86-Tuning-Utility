using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{

    internal class RyzenAdj_To_UXTU
    {
        //Translate RyzenAdj like cli arguments to UXTU
        public static void Translate(string _ryzenAdjString)
        {
            try
            {
                //Remove last space off cli arguments 
                _ryzenAdjString = _ryzenAdjString.Substring(0, _ryzenAdjString.Length - 1);
                //Split cli arguments into array
                string[] ryzenAdjCommands = _ryzenAdjString.Split(' ');
                List<Task> tasks = new List<Task>();
                int i = 0;
                do
                {
                    //Run through array
                    foreach (string ryzenAdjCommand in ryzenAdjCommands)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                // Extract the command string before the "=" sign
                                string ryzenAdjCommandString = ryzenAdjCommand.Split('=')[0].Replace("=", null).Replace("--", null);

                                //Convert value of select cli argument to int
                                string ryzenAdjCommandValueString = ryzenAdjCommand.Substring(ryzenAdjCommand.IndexOf('=') + 1);
                                int ryzenAdjCommandValue = Convert.ToInt32(ryzenAdjCommandValueString);

                                SMUCommands.applySettings(ryzenAdjCommandString, (uint)ryzenAdjCommandValue);
                                await Task.Delay(2);
                            }
                            catch (Exception ex) { }
                        }));

                        Task.WaitAll(tasks.ToArray());

                        i++;
                    }
                } while (i < 2);

            }
            catch (Exception ex) { }
        }
    }
}
