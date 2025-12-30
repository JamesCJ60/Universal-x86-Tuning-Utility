using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.AMD_Backend;
using Universal_x86_Tuning_Utility.Scripts.GPUs.AMD;
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;
using Windows.Storage;
using static RyzenSmu.RyzenSMU;

[assembly: CLSCompliant(false)]


namespace RyzenSmu
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
            if (Family.FAM == Family.RyzenFamily.Mendocino || Family.FAM == Family.RyzenFamily.Rembrandt || Family.FAM == Family.RyzenFamily.PhoenixPoint || Family.FAM == Family.RyzenFamily.PhoenixPoint2 || Family.FAM == Family.RyzenFamily.HawkPoint || Family.FAM == Family.RyzenFamily.HawkPoint2 || Family.FAM == Family.RyzenFamily.StrixPoint || Family.FAM == Family.RyzenFamily.KrackanPoint || Family.FAM == Family.RyzenFamily.KrackanPoint2 || Family.FAM == Family.RyzenFamily.StrixHalo) Socket_FT6_FP7_FP8();
            if (Family.FAM == Family.RyzenFamily.Raphael || Family.FAM == Family.RyzenFamily.DragonRange || Family.FAM == Family.RyzenFamily.GraniteRidge || Family.FAM == Family.RyzenFamily.FireRange) Socket_AM5_V1();

            SMUCommands.RyzenAccess.Initialise();
        }

        private static void Socket_FT5_FP5_AM4()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10564;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10A20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10A80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10A88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true, 0x1a), // Use MP1 address
                ("stapm-time",true , 0x1e), 
                ("fast-limit",true , 0x1b),
                ("slow-limit",true , 0x1c),
                ("slow-time",true , 0x1d),
                ("tctl-temp",true , 0x1f),
                ("cHTC-temp",false , 0x56), // Use RSMU address
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
                ("max-cpuclk",true, 0x44),
                ("min-cpuclk",true, 0x45),
                ("max-gfxclk",true, 0x46),
                ("min-gfxclk",true, 0x47),
                ("max-socclk-frequency",true, 0x48),
                ("min-socclk-frequency",true, 0x49),
                ("max-fclk-frequency",true, 0x4a),
                ("min-fclk-frequency",true, 0x4b),
                ("max-vcn",true, 0x4c),
                ("min-vcn",true, 0x4d),
                ("max-lclk",true, 0x4e),
                ("min-lclk",true, 0x4f),
            };
        }

        private static void Socket_FP6_AM4()
        {
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10564;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10A20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10A80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10A88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true , 0x14), // Use MP1 address
                //("stapm-limit",false , 0x31), // Use RSMU address
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
                ("enable-oc",false , 0x17),
                ("enable-oc",true , 0x2f),
                ("disable-oc",false , 0x18),
                ("disable-oc",true , 0x30)
            };
        }

        private static void Socket_FT6_FP7_FP8()
        {
            if(Family.FAM == Family.RyzenFamily.StrixPoint || Family.FAM == Family.RyzenFamily.KrackanPoint || Family.FAM == Family.RyzenFamily.KrackanPoint2 || Family.FAM == Family.RyzenFamily.StrixHalo)
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3b10928;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3b10978;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3b10998;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10a20;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10a80;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10a88;
            }
            else 
            {
                RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10528;
                RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B10578;
                RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B10998;

                RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10a20;
                RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10a80;
                RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10a88;
            }

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit", true, 0x14), // Use MP1 address
                ("stapm-limit", false, 0x31),// Use RSMU address
                ("stapm-time", true, 0x18),
                ("fast-limit", true, 0x15),
                ("fast-limit", false, 0x32),
                ("slow-limit", true, 0x16),
                ("slow-limit", false, 0x33),
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

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10a20;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10a80;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10a88;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true, 0x14), // Use MP1 address
                //("stapm-limit",false , 0x31), // Use RSMU address
                ("stapm-time",true , 0x18),
                ("fast-limit",true , 0x15),
                ("slow-limit",true , 0x16),
                ("slow-time",true , 0x17),
                ("tctl-temp",true , 0x19),
                ("cHTC-temp",false , 0x37),
                ("apu-skin-temp",true , 0x33),
                ("vrm-current",true , 0x1a),
                ("vrmmax-current",true , 0x1c),
                ("vrmsoc-current",true , 0x1b),
                ("vrmsocmax-current",true , 0x1d),
                ("vrmgfx-current",true , 0x1e),
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

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B1051C;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0X03B10568;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0X03B10590;

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

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10524;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10570;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10A40;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("ppt-limit",true, 0x3D), // Use MP1 address
                ("ppt-limit",false, 0x53), // Use RSMU address
                ("tdc-limit",true , 0x3B),
                ("tdc-limit",false , 0x54),
                ("edc-limit",true , 0x3c),
                ("edc-limit",false , 0x55),
                ("tctl-temp",true , 0x3E),
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
            RyzenSmu.Smu.MP1_ADDR_MSG = 0x3B10530;
            RyzenSmu.Smu.MP1_ADDR_RSP = 0x3B1057C;
            RyzenSmu.Smu.MP1_ADDR_ARG = 0x3B109C4;

            RyzenSmu.Smu.PSMU_ADDR_MSG = 0x03B10524;
            RyzenSmu.Smu.PSMU_ADDR_RSP = 0x03B10570;
            RyzenSmu.Smu.PSMU_ADDR_ARG = 0x03B10A40;

            SMUCommands.commands = new List<(string, bool, uint)>
            {
                // Store the commands
                ("stapm-limit",true, 0x4f),
                ("stapm-time",true , 0x53),
                ("fast-limit",true , 0x3e),
                ("slow-limit",true , 0x5f),
                ("slow-time",true , 0x60),
                ("vrm-current",true , 0x3c),
                ("vrm-current",false , 0x57),
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

    static class SMUCommands
    {
        public static List<(string, bool, uint)> commands;

        public static Smu RyzenAccess = new Smu();

        public static void applySettings(string commandName, uint value)
        {
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
        }
    }

    class Smu
    {
        public static RyzenSMU ryzenSMU;
        public static PawnIo pawnIo;

        public void Initialise()
        {
            pawnIo = PawnIo.LoadModuleFromFile(@"RyzenSMU.bin");
            if (!pawnIo.IsLoaded)
            {
                Console.WriteLine("PawnIo failed to load.");
                return;
            }
            else
            {
                ryzenSMU = new RyzenSMU(pawnIo);
                ryzenSMU.Open();
            }
        }

        public void Deinitialize()
        {
            if (pawnIo.IsLoaded)
            {
                ryzenSMU.Close();
                pawnIo.Close();
            }
        }

        public static uint SMU_PCI_ADDR { get; set; }
        public static uint SMU_OFFSET_ADDR { get; set; }
        public static uint SMU_OFFSET_DATA { get; set; }

        public static uint MP1_ADDR_MSG { get; set; }
        public static uint MP1_ADDR_RSP { get; set; }
        public static uint MP1_ADDR_ARG { get; set; }

        public static uint PSMU_ADDR_MSG { get; set; }
        public static uint PSMU_ADDR_RSP { get; set; }
        public static uint PSMU_ADDR_ARG { get; set; }


        public Status SendMp1(uint message, ref uint[] arguments)
        {
            var mp1 = ryzenSMU.RegisterMailbox(
                 name: "MP1",
                 msgAddr: MP1_ADDR_MSG,
                 rspAddr: MP1_ADDR_RSP,
                 argAddr: MP1_ADDR_ARG,
                 maxArgs: 6
             );

            return ryzenSMU.SendSmuCommand(mp1, message, ref arguments);
        }

        public Status SendRsmu(uint message, ref uint[] arguments)
        {
            var rsmu = ryzenSMU.RegisterMailbox(
                  name: "RSMU",
                  msgAddr: PSMU_ADDR_MSG,
                  rspAddr: PSMU_ADDR_RSP,
                  argAddr: PSMU_ADDR_ARG,
                  maxArgs: 6
              );

            return ryzenSMU.SendSmuCommand(rsmu, message, ref arguments);
        }

    }
    /// <summary>
    /// High-level interface for sending commands to the AMD SMU via PawnIO.
    /// Handles mutex co-ordination, mailbox selection, and argument flow.
    /// </summary>
    internal sealed class RyzenSMU : IDisposable
    {
        // PawnIO function names used for register access
        private const string IOCTL_READ_SMU_REGISTER = "ioctl_read_smu_register";
        private const string IOCTL_WRITE_SMU_REGISTER = "ioctl_write_smu_register";

        // Maximum number of polls whilst waiting for the SMU to respond
        private const ushort POLL_LIMIT = 8192;

        // Global mutex names.
        private const string ISA_MUTEX_NAME = "Global\\Access_ISABUS.HTP.Method";
        private const string PCI_MUTEX_NAME = "Global\\Access_PCI";

        private readonly PawnIo _pawnIo;

        // Mutex handles owned by this instance
        private Mutex? _isaMutex;
        private Mutex? _pciMutex;

        private bool _disposed;

        // --------------------------------------------------------------------
        // Mailbox handling
        // --------------------------------------------------------------------

        /// <summary>
        /// Represents a single SMU mailbox (for example, MP1 or RSMU).
        /// Addresses are fully configurable and may be changed at runtime.
        /// </summary>
        public sealed class Mailbox
        {
            public Mailbox(string name, uint maxArgs = 6)
            {
                Name = string.IsNullOrWhiteSpace(name) ? "Unnamed" : name.Trim();
                MAX_ARGS = maxArgs;
            }

            /// <summary>
            /// Friendly name used for debugging/logging only.
            /// </summary>
            public string Name { get; }

            public uint MAX_ARGS { get; set; }

            public uint SMU_ADDR_MSG { get; set; }
            public uint SMU_ADDR_RSP { get; set; }
            public uint SMU_ADDR_ARG { get; set; }

            public bool IsValid =>
                MAX_ARGS > 0 &&
                SMU_ADDR_MSG != 0 &&
                SMU_ADDR_RSP != 0 &&
                SMU_ADDR_ARG != 0;
        }

        // Registered mailboxes by name (MP1, RSMU, etc.)
        private readonly Dictionary<string, Mailbox> _mailboxes =
            new(StringComparer.OrdinalIgnoreCase);

        // Last command metadata (handy when diagnosing failures)
        public string? LastMailboxName { get; private set; }
        public uint LastCommand { get; private set; }
        public Status LastStatus { get; private set; } = Status.FAILED;

        public RyzenSMU(PawnIo pawnIo)
        {
            _pawnIo = pawnIo ?? throw new ArgumentNullException(nameof(pawnIo));
        }

        /// <summary>
        /// SMU-visible status codes only.
        /// Transport or mutex failures are reported as FAILED.
        /// </summary>
        public enum Status : byte
        {
            OK = 0x01,
            FAILED = 0xFF,
            UNKNOWN_CMD = 0xFE,
            CMD_REJECTED_PREREQ = 0xFD,
            CMD_REJECTED_BUSY = 0xFC
        }

        // --------------------------------------------------------------------
        // Lifecycle
        // --------------------------------------------------------------------

        /// <summary>
        /// Opens the global mutexes required for SMU access.
        /// Safe to call multiple times.
        /// </summary>
        public void Open()
        {
            ThrowIfDisposed();

            _isaMutex ??= CreateOrOpenMutex(ISA_MUTEX_NAME);
            _pciMutex ??= CreateOrOpenMutex(PCI_MUTEX_NAME);
        }

        /// <summary>
        /// Releases mutex handles owned by this instance.
        /// </summary>
        public void Close()
        {
            DisposeMutex(ref _isaMutex);
            DisposeMutex(ref _pciMutex);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Close();
            _disposed = true;
        }

        // --------------------------------------------------------------------
        // Mailbox registration
        // --------------------------------------------------------------------

        /// <summary>
        /// Registers (or replaces) a mailbox with a friendly name.
        /// Useful for keeping a known set such as "MP1" and "RSMU".
        /// </summary>
        public Mailbox RegisterMailbox(
            string name,
            uint msgAddr,
            uint rspAddr,
            uint argAddr,
            uint maxArgs = 6)
        {
            var mailbox = new Mailbox(name, maxArgs)
            {
                SMU_ADDR_MSG = msgAddr,
                SMU_ADDR_RSP = rspAddr,
                SMU_ADDR_ARG = argAddr
            };

            _mailboxes[mailbox.Name] = mailbox;
            return mailbox;
        }

        // --------------------------------------------------------------------
        // Public SMU command entry point
        // --------------------------------------------------------------------

        /// <summary>
        /// Sends a command to the SMU using the specified mailbox.
        /// On success, the SMU may overwrite values in the args array.
        /// </summary>
        public Status SendSmuCommand(Mailbox mailbox, uint message, ref uint[] args)
        {
            ThrowIfDisposed();

            if (mailbox == null || !mailbox.IsValid || message == 0)
            {
                RememberDebug(mailbox, message, Status.UNKNOWN_CMD);
                return Status.UNKNOWN_CMD;
            }

            // Lazily open mutexes if the caller forgot
            if (_pciMutex == null)
                Open();

            if (_pciMutex == null || !WaitForMutex(_pciMutex, 10))
            {
                RememberDebug(mailbox, message, Status.FAILED);
                return Status.FAILED;
            }

            try
            {
                var status = ExecuteMailboxFlow(mailbox, message, ref args);
                RememberDebug(mailbox, message, status);
                return status;
            }
            catch
            {
                RememberDebug(mailbox, message, Status.FAILED);
                return Status.FAILED;
            }
            finally
            {
                SafeReleaseMutex(_pciMutex);
            }
        }

        // --------------------------------------------------------------------
        // Core mailbox protocol
        // --------------------------------------------------------------------

        private Status ExecuteMailboxFlow(Mailbox mb, uint msg, ref uint[] args)
        {
            // Ensure the mailbox is idle before starting
            if (!WaitForResponse(mb.SMU_ADDR_RSP, out _))
                return Status.FAILED;

            // Clear response register
            if (!Write32(mb.SMU_ADDR_RSP, 0))
                return Status.FAILED;

            // Write input arguments
            if (!WriteArguments(mb, args))
                return Status.FAILED;

            // Send command
            if (!Write32(mb.SMU_ADDR_MSG, msg))
                return Status.FAILED;

            // Wait for completion
            if (!WaitForResponse(mb.SMU_ADDR_RSP, out uint rsp))
                return Status.FAILED;

            // The response register should contain a byte-sized status code
            if (rsp > byte.MaxValue)
                return Status.FAILED;

            Status status = unchecked((Status)rsp);

            // Read back arguments only on success
            if (status == Status.OK && args != null && args.Length > 0)
            {
                if (!ReadArguments(mb, ref args))
                    return Status.FAILED;
            }

            return status;
        }

        private bool WaitForResponse(uint rspReg, out uint value)
        {
            value = 0;

            for (ushort i = 0; i < POLL_LIMIT; i++)
            {
                if (Read32(rspReg, out value) && value != 0)
                    return true;
            }

            return false;
        }

        private bool WriteArguments(Mailbox mb, uint[] args)
        {
            uint[] payload = PrepareArguments(args, mb.MAX_ARGS);

            // Guard against overflow when calculating base + (index * 4)
            uint maxSafe = uint.MaxValue - (mb.MAX_ARGS * 4);

            for (int i = 0; i < payload.Length; i++)
            {
                uint reg = mb.SMU_ADDR_ARG + (uint)(i * 4);
                if (reg > maxSafe)
                    continue;

                if (!Write32(reg, payload[i]))
                    return false;
            }

            return true;
        }

        private bool ReadArguments(Mailbox mb, ref uint[] args)
        {
            int count = Math.Min(args.Length, (int)mb.MAX_ARGS);

            // Same overflow guard as above
            uint maxSafe = uint.MaxValue - (mb.MAX_ARGS * 4);

            for (int i = 0; i < count; i++)
            {
                uint reg = mb.SMU_ADDR_ARG + (uint)(i * 4);
                if (reg > maxSafe)
                    continue;

                if (!Read32(reg, out uint value))
                    return false;

                args[i] = value;
            }

            return true;
        }

        // --------------------------------------------------------------------
        // Low-level register access
        // --------------------------------------------------------------------

        private bool Read32(uint reg, out uint value)
        {
            value = 0;

            long[] inBuf = { unchecked((long)reg) };
            long[] outBuf = new long[1];

            int hr = _pawnIo.ExecuteHr(IOCTL_READ_SMU_REGISTER, inBuf, 1, outBuf, 1, out _);
            if (hr != 0)
                return false;

            value = unchecked((uint)outBuf[0]);
            return true;
        }

        private bool Write32(uint reg, uint value)
        {
            long[] inBuf = { unchecked((long)reg), unchecked((long)value) };

            int hr = _pawnIo.ExecuteHr(
                IOCTL_WRITE_SMU_REGISTER,
                inBuf,
                2,
                Array.Empty<long>(),
                0,
                out _);

            return hr == 0;
        }

        private static uint[] PrepareArguments(uint[] args, uint maxArgs)
        {
            uint[] result = new uint[maxArgs];

            if (args == null || args.Length == 0)
                return result;

            int copy = Math.Min(args.Length, (int)maxArgs);
            for (int i = 0; i < copy; i++)
                result[i] = args[i];

            return result;
        }

        // --------------------------------------------------------------------
        // Mutex helpers
        // --------------------------------------------------------------------

        private static Mutex? CreateOrOpenMutex(string name)
        {
            try
            {
                var security = BuildWorldWritableSecurity();
#if NETFRAMEWORK
                return new Mutex(false, name, out _, security);
#else
                return MutexAcl.Create(false, name, out _, security);
#endif
            }
            catch (UnauthorizedAccessException)
            {
                try { return Mutex.OpenExisting(name); }
                catch { return null; }
            }
            catch
            {
                return null;
            }
        }

        private static MutexSecurity BuildWorldWritableSecurity()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var rule = new MutexAccessRule(
                sid,
                MutexRights.FullControl,
                AccessControlType.Allow);

            var security = new MutexSecurity();
            security.AddAccessRule(rule);
            return security;
        }

        private static bool WaitForMutex(Mutex mutex, int timeoutMs)
        {
            try { return mutex.WaitOne(timeoutMs, false); }
            catch (AbandonedMutexException) { return true; }
            catch { return false; }
        }

        private static void SafeReleaseMutex(Mutex mutex)
        {
            try { mutex.ReleaseMutex(); }
            catch { }
        }

        private static void DisposeMutex(ref Mutex? mutex)
        {
            try { mutex?.Close(); }
            catch { }
            finally { mutex = null; }
        }

        private void RememberDebug(Mailbox? mb, uint cmd, Status status)
        {
            LastMailboxName = mb?.Name;
            LastCommand = cmd;
            LastStatus = status;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RyzenSMU));
        }
    }
}
