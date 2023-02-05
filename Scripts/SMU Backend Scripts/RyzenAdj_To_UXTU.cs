using AATUV3;
using RyzenSMUBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UXTU.Scripts.SMU_Backend_Scripts
{
    internal class RyzenAdj_To_UXTU
    {
        //Translate RyzenAdj cli arguments to UXTU
        public static void Translate(string _ryzenAdjString)
        {
            try
            {
                //Remove last space off cli arguments 
                _ryzenAdjString = _ryzenAdjString.Substring(0, _ryzenAdjString.Length - 1);
                //Split cli arguments into array
                string[] ryzenAdjCommands = _ryzenAdjString.Split(' ');
                int i = 0;
                //Run through array
                do
                {
                    //Convert value of select cli argument to int
                    string ryzenAdjCommandValueString = ryzenAdjCommands[i].Substring(ryzenAdjCommands[i].IndexOf('=') + 1);
                    int ryzenAdjCommandValue = Convert.ToInt32(ryzenAdjCommandValueString);
                    //Find which cli argument is stored in current array index and apply settings
                    if (ryzenAdjCommands[i].Contains("--tctl-temp=")) { SendCommand.set_tctl_temp((uint)ryzenAdjCommandValue); SendCommand.set_cHTC_temp((uint)ryzenAdjCommandValue); }
                    else if (ryzenAdjCommands[i].Contains("--apu-skin-temp=")) SendCommand.set_apu_skin_temp_limit((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--stapm-limit=")) { SendCommand.set_stapm_limit((uint)ryzenAdjCommandValue); SendCommand.set_stapm2_limit((uint)ryzenAdjCommandValue); }
                    else if (ryzenAdjCommands[i].Contains("--slow-limit=")) SendCommand.set_slow_limit((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--fast-limit=")) SendCommand.set_fast_limit((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--slow-time=")) SendCommand.set_slow_time((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--stapm-time=")) SendCommand.set_stapm_time((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--prochot-deassertion-ramp=")) SendCommand.set_prochot_deassertion_ramp((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrm-current=")) SendCommand.set_vrm_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrmmax-current=")) SendCommand.set_vrmmax_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrmsoc-current=")) SendCommand.set_vrmsoc_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrmsocmax-current=")) SendCommand.set_vrmsocmax_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrmgfx-current=")) SendCommand.set_vrmgfx_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrmgfxmax-current=")) SendCommand.set_vrmgfxmax_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--vrmcvip-current=")) SendCommand.set_vrmcvip_current((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--max-fclk-frequency=")) SendCommand.set_max_fclk_freq((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--min-fclk-frequency=")) SendCommand.set_min_fclk_freq((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--max-gfxclk=")) SendCommand.set_max_gfxclk_freq((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--min-gfxclk=")) SendCommand.set_min_gfxclk_freq((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--max-lclk=")) SendCommand.set_max_lclk((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--min-lclk=")) SendCommand.set_min_lclk((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--max-vcn=")) SendCommand.set_max_vcn_freq((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--min-vcn=")) SendCommand.set_min_vcn_freq((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--dgpu-skin-temp=")) SendCommand.set_dGPU_skin((uint)ryzenAdjCommandValue);
                    else if (ryzenAdjCommands[i].Contains("--gfx-clk=")) SendCommand.set_gfx_clk((uint)ryzenAdjCommandValue);
                    i++;
                } while (i < ryzenAdjCommands.Length);
            } catch(Exception ex) { }
        }
    }
}
