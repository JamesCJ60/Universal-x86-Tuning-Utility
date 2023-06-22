using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{
    internal class ADLXBackend
    {
        public const string CppFunctionsDLL = @"PerformanceMetrics.dll";
        public const string CppFunctionsDLL1 = @"GraphSettings.dll";
        [DllImport(CppFunctionsDLL, CallingConvention = CallingConvention.Cdecl)] public static extern int GetFPSData();
        [DllImport(CppFunctionsDLL, CallingConvention = CallingConvention.Cdecl)] public static extern int GetGPUMetrics(int GPU, int Sensor);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetFPSLimit(int GPU, bool isEnabled, int FPS);
    }
}
