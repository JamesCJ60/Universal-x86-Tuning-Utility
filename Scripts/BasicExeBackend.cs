using UXTU.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AATUV3.Scripts
{
    public class BasicExeBackend
    {
        /// <summary>
        /// Open third party program within folder/sub-folders
        /// </summary>
        /// <param name="program">Program executable</param>
        /// <param name="input">Program arguments</param>
        /// <param name="isHidden">Is program intended to be hidden when opened</param>
        public static async void ApplySettings(string program, string input, bool isHidden)
        {
            try
            {
                await Task.Run(() =>
                {
                    //Get current path and join it with program executable
                    string path = (string)Settings.Default["Path"] + program;
                    //Create new process
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    //Hide program if required
                    if (isHidden == true)
                    {
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    }
                    //Pass on path and arguments
                    startInfo.FileName = path;
                    startInfo.Arguments = input;
                    process.StartInfo = startInfo;
                    //Start program
                    process.Start();

                    Thread.Sleep(10000);
                    process.Close();
                    return;
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }


        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);
        public static async Task Garbage_Collect()
        {
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);

            Thread.Sleep(1);

            long usedMemory = GC.GetTotalMemory(true);
        }
    }
}
