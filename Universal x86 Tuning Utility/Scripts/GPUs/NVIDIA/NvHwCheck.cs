using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility.Services;

namespace Universal_x86_Tuning_Utility.Scripts.GPUs.NVIDIA
{
    internal class NvHwCheck
    {
        public static void CheckROPCount()
        {
            PhysicalGPU[] gpus = PhysicalGPU.GetPhysicalGPUs();

            foreach (PhysicalGPU gpu in gpus)
            {
                int ropCheck = GetROPCount(gpu.ArchitectInformation.PhysicalGPU.ToString());
                
                // If the GPU has less ROPs than the expected amount
                if (ropCheck > 0 && ropCheck > gpu.ArchitectInformation.NumberOfROPs) ToastNotification.ShowToastNotification(LocalizationService.Get("NVIDIA GPU Warning"), LocalizationService.Format("ROP count is lower than expected on {0} ({1} ROPs out of {2} ROPs)", gpu.ArchitectInformation.PhysicalGPU, gpu.ArchitectInformation.NumberOfROPs, ropCheck));
            }
        }

        private static int GetROPCount(string gpuName)
        {
            if (gpuName.Contains("Laptop"))
            {
                // Blackwell
                if (gpuName.Contains("5090")) return 112;
                else if (gpuName.Contains("5080")) return 96;
                else if (gpuName.Contains("5070 Ti")) return 64;
                else if (gpuName.Contains("5070")) return 48;
                // Ada
                else if (gpuName.Contains("4090")) return 112;
                else if (gpuName.Contains("4080")) return 80;
                else if (gpuName.Contains("4070")) return 48;
                else if (gpuName.Contains("4060")) return 48;
                else if (gpuName.Contains("4050")) return 48;
            }
            else
            {
                // Blackwell
                if (gpuName.Contains("5090")) return 176;
                else if (gpuName.Contains("5080")) return 112;
                else if (gpuName.Contains("5070 Ti")) return 96;
                else if (gpuName.Contains("5070")) return 64;
                // Ada
                else if (gpuName.Contains("4090")) return 176;
                else if (gpuName.Contains("4080")) return 112;
                else if (gpuName.Contains("4070 Ti Super")) return 96;
                else if (gpuName.Contains("4070 Ti")) return 80;
                else if (gpuName.Contains("4070 Super")) return 80;
                else if (gpuName.Contains("4070")) return 64;
                else if (gpuName.Contains("4060 Ti")) return 48;
                else if (gpuName.Contains("4060")) return 48;
            }

            return -1;
        }
    }
}
