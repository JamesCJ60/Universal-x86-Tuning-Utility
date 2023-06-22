using RyzenSmu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{
    class SMUCommands
    {
        public static List<(string, bool, uint)> commands;

        public static Smu RyzenAccess = new Smu(false);

        public static void applySettings(string commandName, uint value)
        {
            RyzenAccess.Initialize();
            uint[] Args = new uint[6];
            Args[0] = value;

            // Find the command by name
            var matchingCommands = commands.Where(c => c.Item1 == commandName);
            if (matchingCommands.Count() > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (var command in matchingCommands)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        // Apply the command based on its address
                        if (command.Item2 == true) RyzenAccess.SendMp1(command.Item3, ref Args);
                        else RyzenAccess.SendRsmu(command.Item3, ref Args);
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
            else throw new ArgumentException($"Command '{commandName}' not found");

            RyzenAccess.Deinitialize();
        }
    }
}
