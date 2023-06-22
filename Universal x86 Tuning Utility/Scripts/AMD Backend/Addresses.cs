using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.AMD_Backend
{
    class Addresses
    {
        public static void setAddresses()
        {
            RyzenSmu.Smu.SMU_PCI_ADDR = 0x00000000;
            RyzenSmu.Smu.SMU_OFFSET_ADDR = 0xB8;
            RyzenSmu.Smu.SMU_OFFSET_DATA = 0xBC;

            if (Family.FAM == Family.RyzenFamily.SummitRidge || Family.FAM == Family.RyzenFamily.PinnacleRidge) Socket_AM4_V1();
            if (Family.FAM == Family.RyzenFamily.RavenRidge || Family.FAM == Family.RyzenFamily.Picasso || Family.FAM == Family.RyzenFamily.Dali || Family.FAM == Family.RyzenFamily.Pollock || Family.FAM == Family.RyzenFamily.FireFlight) Socket_FT5_FP5_AM4();
            if (Family.FAM == Family.RyzenFamily.Matisse || Family.FAM == Family.RyzenFamily.Vermeer) Socket_AM4_V2();
            if (Family.FAM == Family.RyzenFamily.Renoir || Family.FAM == Family.RyzenFamily.Lucienne || Family.FAM == Family.RyzenFamily.Cezanne_Barcelo) Socket_FP6_AM4();
            if (Family.FAM == Family.RyzenFamily.VanGogh) Socket_FF3();
            if (Family.FAM == Family.RyzenFamily.Mendocino || Family.FAM == Family.RyzenFamily.Rembrandt || Family.FAM == Family.RyzenFamily.PhoenixPoint) Socket_FT6_FP7_FP8();
            if (Family.FAM == Family.RyzenFamily.DragonRange) Socket_AM5_V1();
        }

        private static void Socket_FT5_FP5_AM4()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10564;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10A20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10A80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10A88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true, 0x1a), // Use MP1 address
                ("stapm-time",true , 0x1e), // Use RSMU address
                ("fast-limit",true , 0x1b),
                ("slow-limit",true , 0x1c),
                ("slow-time",true , 0x1d),
                ("tctl-temp",true , 0x1f),
                ("cHTC-temp",false , 0x56),
                ("vrm-current",true , 0x20),
                ("vrmmax-current",true , 0x22),
                ("vrmsoc-current",true , 0x21),
                ("vrmsocmax-current",true , 0x23),
                ("prochot-deassertion-ramp",true , 0x25),
                ("pbo-scalar",false , 0x68),
                ("power-saving",true , 0x19),
                ("max-performance",true , 0x18),
                ("oc-clk",false , 0x7d),
                ("per-core-oc-clk",false , 0x7e),
                ("oc-volt",false , 0x7f),
                ("enable-oc",false , 0x69),
                ("disable-oc",false , 0x6a),
            };
        }

        private static void Socket_FP6_AM4()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10564;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10A20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10A80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10A88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true , 0x14), // Use MP1 address
                ("stapm-limit",false , 0x31), // Use RSMU address
                ("ppt-limit",false , 0x33),
                ("stapm-time",true , 0x18),
                ("fast-limit",true , 0x15),
                ("slow-limit",true , 0x16),
                ("slow-time",true , 0x17),
                ("tctl-temp",true , 0x19),
                ("cHTC-temp",false , 0x37),
                ("apu-skin-temp",true , 0x38),
                ("vrm-current",true , 0x1a),
                ("vrmmax-current",true , 0x1c),
                ("vrmsoc-current",true , 0x1b),
                ("vrmsocmax-current",true , 0x1d),
                ("prochot-deassertion-ramp",true , 0x20),
                ("gfx-clk",false , 0x89),
                ("dgpu-skin-temp",true , 0x37),
                ("power-saving",true , 0x12),
                ("max-performance",true , 0x11),
                ("pbo-scalar",false , 0x3F),
                ("oc-clk",false , 0x19),
                ("oc-clk",true , 0x31),
                ("per-core-oc-clk",false , 0x1a),
                ("per-core-oc-clk",true , 0x32),
                ("oc-volt",false , 0x1b),
                ("oc-volt",true , 0x33),
                ("set-coall",true , 0x55),
                ("set-coall",false , 0xB1),
                ("set-coper",true , 0x54),
                ("set-cogfx",true , 0x64),
                ("set-cogfx",false , 0x57),
                ("enable-oc",false , 0x1d),
                ("enable-oc",true , 0x2f),
                ("disable-oc",false , 0x1d),
                ("disable-oc",true , 0x30)
            };
        }

        private static void Socket_FT6_FP7_FP8()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10578;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10a20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10a80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10a88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit", true, 0x14), // Use MP1 address
                ("stapm-limit", false, 0x31), // Use RSMU address
                ("stapm-time", true, 0x18),
                ("fast-limit", true, 0x15),
                ("fast-limit", false, 0x32),
                ("slow-limit", true, 0x16),
                ("slow-limit", false, 0x33),
                ("slow-limit", false, 0x34),
                ("slow-time", true, 0x17),
                ("tctl-temp", true, 0x19),
                ("cHTC-temp", false, 0x37),
                ("apu-skin-temp", true, 0x33),
                ("vrm-current", true, 0x1a),
                ("vrmmax-current", true, 0x1c),
                ("vrmsoc-current", true, 0x1b),
                ("vrmsocmax-current", true ,0x1d),
                ("prochot-deassertion-ramp", true, 0x1f),
                ("gfx-clk", false, 0x89),
                ("dgpu-skin-temp", true, 0x32),
                ("power-saving", true, 0x12),
                ("max-performance", true, 0x11),
                ("pbo-scalar", false, 0x3E),
                ("oc-clk",  false, 0x19),
                ("per-core-oc-clk", false, 0x1a),
                ("set-coall",   true, 0x4c),
                ("set-coall",   false, 0x5d),
                ("set-coper",   true, 0x4b),
                ("set-cogfx",   false, 0xb7),
                ("enable-oc",   false, 0x17),
                ("disable-oc",  false, 0x18)
            };
        }

        private static void Socket_FF3()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10578;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10a20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10a80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10a88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true, 0x14), // Use MP1 address
                ("stapm-limit",false , 0x31), // Use RSMU address
                ("stapm-time",true , 0x18),
                ("fast-limit",true , 0x15),
                ("slow-limit",true , 0x16),
                ("slow-time",true , 0x17),
                ("tctl-temp",true , 0x19),
                ("cHTC-temp",false , 0x37),
                ("apu-skin-temp",true , 0x33),
                ("vrm-current",true , 0x1a),
                ("vrmmax-current",true , 0x1e),
                ("vrmsoc-current",true , 0x1b),
                ("vrmsocmax-current",true , 0x1d),
                ("vrmgfx-current",true , 0x1c),
                ("vrmgfxmax-current",true , 0x1f),
                ("prochot-deassertion-ramp",true , 0x22),
                ("gfx-clk",false , 0x89),
                ("power-saving",true , 0x12),
                ("max-performance",true , 0x11),
                ("set-coall",true , 0x4c),
                ("set-coall",false , 0x5d),
                ("set-coper",true , 0x4b),
                ("set-cogfx",false , 0xb7)
            };
        }

        private static void Socket_AM4_V1()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0X3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0X3B10564;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0X3B10598;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B1051C;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0X3B10568;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0X3B10590;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("ppt-limit",false, 0x64), // Use RSMU address
                ("tdc-limit",false , 0x65),
                ("edc-limit",false , 0x66),
                ("tctl-temp",false , 0x68),
                ("pbo-scalar",false , 0x6a),
                ("oc-clk", false, 0x6c),
                ("per-core-oc-clk",false , 0x6d),
                ("oc-volt", false, 0x6e),
                ("enable-oc",true , 0x23),
                ("enable-oc",false , 0x6b),
                ("disable-oc",true , 0x24),
            };
        }

        private static void Socket_AM4_V2()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10530;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B1057C;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B109C4;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10524;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10570;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10A40;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("ppt-limit",true, 0x3D), // Use MP1 address
                ("ppt-limit",false, 0x53), // Use RSMU address
                ("tdc-limit",true , 0x3B),
                ("tdc-limit",false , 0x54),
                ("edc-limit",true , 0x3c),
                ("edc-limit",false , 0x55),
                ("tctl-temp",true , 0x23),
                ("tctl-temp",false , 0x56),
                ("pbo-scalar",false , 0x58),
                ("oc-clk", true, 0x26),
                ("oc-clk", false, 0x5c),
                ("per-core-oc-clk",true , 0x27),
                ("per-core-oc-clk",false , 0x5d),
                ("oc-volt", true, 0x28),
                ("oc-volt", false, 0x61),
                ("set-coall", true, 0x36),
                ("set-coall", false, 0xb),
                ("set-coper", true, 0x35),
                ("enable-oc",true , 0x24),
                ("enable-oc",false , 0x5a),
                ("disable-oc",true , 0x25),
                ("disable-oc",false , 0x5b),
            };
        }

        private static void Socket_AM5_V1()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3010508;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3010988;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3010984;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x3B10524;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x3B10570;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x3B10A40;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("ppt-limit",true, 0x3e), // Use MP1 address
                ("ppt-limit",false, 0x56), // Use RSMU address
                ("tdc-limit",true , 0x3c),
                ("tdc-limit",false , 0x57),
                ("edc-limit",true , 0x3d),
                ("edc-limit",false , 0x58),
                ("tctl-temp",true , 0x3f),
                ("tctl-temp",false , 0x59),
                ("pbo-scalar",false , 0x5b),
                ("oc-clk", false, 0x5f),
                ("per-core-oc-clk",false , 0x60),
                ("oc-volt", false, 0x61),
                ("set-coall", false, 0x7),
                ("set-coper", false, 0x6),
                ("enable-oc",false , 0x5d),
                ("disable-oc",false , 0x5e),
            };
        }
    }
}
