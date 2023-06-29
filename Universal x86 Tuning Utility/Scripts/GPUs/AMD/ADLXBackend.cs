using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.GPUs.AMD
{
    internal class ADLXBackend
    {
        public const string CppFunctionsDLL = @"PerformanceMetrics.dll";
        public const string CppFunctionsDLL1 = @"GraphSettings.dll";
        [DllImport(CppFunctionsDLL, CallingConvention = CallingConvention.Cdecl)] public static extern int GetFPSData();
        [DllImport(CppFunctionsDLL, CallingConvention = CallingConvention.Cdecl)] public static extern int GetGPUMetrics(int GPU, int Sensor);

        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetFPSLimit(int GPU, bool isEnabled, int FPS);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetAntiLag(int GPU, bool isEnabled);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetBoost(int GPU, bool isEnabled, int percent);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetRSR(bool isEnabled);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int GetRSRState();
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern bool SetRSRSharpness(int sharpness);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int GetRSRSharpness();
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetChill(int GPU, bool isEnabled, int maxFPS, int minFPS);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetImageSharpning(int GPU, bool isEnabled, int percent);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetEnhancedSync(int GPU, bool isEnabled);
    }
}
