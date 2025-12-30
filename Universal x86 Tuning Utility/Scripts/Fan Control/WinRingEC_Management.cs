//using OpenLibSys_Fan;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Universal_x86_Tuning_Utility.Scripts.Fan_Control
//{
//    internal class WinRingEC_Management
//    {
//        public static ushort reg_addr = 0x0;
//        public static ushort reg_data = 0x0;
//        static Ols ols = new Ols();
//        static Object lockObject = new object();
//        public static void InitECWin4()

//        {

//            if (ols == null)

//                OlsInit();

//            if (ols == null)

//                return;

//            try

//            {

//                byte EC_Chip_ID1 = ECRamReadWin4(0x2000);

//                if (EC_Chip_ID1 == 0x55)

//                {

//                    byte EC_Chip_Ver = ECRamReadWin4(0x1060);

//                    EC_Chip_Ver = (Byte)(EC_Chip_Ver | 0x80);

//                    ECRamWriteWin4(0x1060, EC_Chip_Ver);


//                }

//                //if (ols != null)

//                //    OlsFree();

//            }

//            catch

//            {

//                OlsFree();

//                return;

//            }

//        }



//        public static byte ECRamReadWin4(ushort address)

//        {

//            if (ols == null)

//                OlsInit();

//            if (ols == null)

//                return 0;

//            byte data = 0;

//            byte high_byte = (byte)((address >> 8) & 0xFF);

//            byte low_byte = (byte)(address & 0xFF);
//            try
//            {

//                lock (lockObject)

//                {

//                    reg_addr = 0x2E;

//                    reg_data = 0x2F;

//                    ols.WriteIoPortByte(reg_addr, 0x2E);

//                    ols.WriteIoPortByte(reg_data, 0x11);

//                    ols.WriteIoPortByte(reg_addr, 0x2F);

//                    ols.WriteIoPortByte(reg_data, high_byte);



//                    ols.WriteIoPortByte(reg_addr, 0x2E);

//                    ols.WriteIoPortByte(reg_data, 0x10);

//                    ols.WriteIoPortByte(reg_addr, 0x2F);

//                    ols.WriteIoPortByte(reg_data, low_byte);



//                    ols.WriteIoPortByte(reg_addr, 0x2E);

//                    ols.WriteIoPortByte(reg_data, 0x12);

//                    ols.WriteIoPortByte(reg_addr, 0x2F);

//                    data = ols.ReadIoPortByte(reg_data);

//                }

//                //if (ols != null)

//                //   OlsFree();

//            }

//            catch

//            {

//                OlsFree();

//                return 0;

//            }

//            return data;

//        }



//        public static void ECRamWriteWin4(ushort address, byte data)

//        {

//            if (ols == null)

//                OlsInit();

//            if (ols == null)

//                return;



//            byte high_byte = (byte)((address >> 8) & 0xFF);

//            byte low_byte = (byte)(address & 0xFF);

//            try
//            {

//                lock (lockObject)

//                {

//                    reg_addr = 0x2E;

//                    reg_data = 0x2F;

//                    ols.WriteIoPortByte(reg_addr, 0x2E);

//                    ols.WriteIoPortByte(reg_data, 0x11);

//                    ols.WriteIoPortByte(reg_addr, 0x2F);

//                    ols.WriteIoPortByte(reg_data, high_byte);



//                    ols.WriteIoPortByte(reg_addr, 0x2E);

//                    ols.WriteIoPortByte(reg_data, 0x10);

//                    ols.WriteIoPortByte(reg_addr, 0x2F);

//                    ols.WriteIoPortByte(reg_data, low_byte);



//                    ols.WriteIoPortByte(reg_addr, 0x2E);

//                    ols.WriteIoPortByte(reg_data, 0x12);

//                    ols.WriteIoPortByte(reg_addr, 0x2F);

//                    ols.WriteIoPortByte(reg_data, data);

//                }

//                //if (ols != null)

//                //    OlsFree();

//            }

//            catch

//            {

//                OlsFree();

//                return;

//            }



//        }
//        public static void ECRamWrite(ushort address, byte data)
//        {
//            if (ols == null)
//                OlsInit();
//            if (ols == null)
//                return;
//            byte high_byte = (byte)((address >> 8) & 0xFF);
//            byte low_byte = (byte)(address & 0xFF);
//            try
//            {
//                lock (lockObject)
//                {
//                    ols.WriteIoPortByte(reg_addr, 0x2E);
//                    ols.WriteIoPortByte(reg_data, 0x11);
//                    ols.WriteIoPortByte(reg_addr, 0x2F);
//                    ols.WriteIoPortByte(reg_data, high_byte);

//                    ols.WriteIoPortByte(reg_addr, 0x2E);
//                    ols.WriteIoPortByte(reg_data, 0x10);
//                    ols.WriteIoPortByte(reg_addr, 0x2F);
//                    ols.WriteIoPortByte(reg_data, low_byte);

//                    ols.WriteIoPortByte(reg_addr, 0x2E);
//                    ols.WriteIoPortByte(reg_data, 0x12);
//                    ols.WriteIoPortByte(reg_addr, 0x2F);
//                    ols.WriteIoPortByte(reg_data, data);


//                }
//            }
//            catch
//            {
//                ols = null;
//                return;
//            }

//        }

//        public static byte ECRamRead(ushort address)
//        {

//            if (ols == null)
//                OlsInit();
//            if (ols == null)
//                return 0;
//            byte data = 0;
//            byte high_byte = (byte)((address >> 8) & 0xFF);
//            byte low_byte = (byte)(address & 0xFF);
//            try
//            {
//                lock (lockObject)
//                {
//                    ols.WriteIoPortByte(reg_addr, 0x2E);
//                    ols.WriteIoPortByte(reg_data, 0x11);
//                    ols.WriteIoPortByte(reg_addr, 0x2F);
//                    ols.WriteIoPortByte(reg_data, high_byte);

//                    ols.WriteIoPortByte(reg_addr, 0x2E);
//                    ols.WriteIoPortByte(reg_data, 0x10);
//                    ols.WriteIoPortByte(reg_addr, 0x2F);
//                    ols.WriteIoPortByte(reg_data, low_byte);

//                    ols.WriteIoPortByte(reg_addr, 0x2E);
//                    ols.WriteIoPortByte(reg_data, 0x12);
//                    ols.WriteIoPortByte(reg_addr, 0x2F);
//                    data = ols.ReadIoPortByte(reg_data);
//                }

//            }
//            catch
//            {
//                ols = null;
//                return 0;
//            }

//            return data;
//        }

//        unsafe public static void OlsInit()
//        {
//            //-----------------------------------------------------------------------------
//            // Initialize
//            //-----------------------------------------------------------------------------
//            ols = new Ols();

//            // Check support library sutatus
//            switch (ols.GetStatus())
//            {
//                case (uint)Ols.Status.NO_ERROR:
//                    break;
//                case (uint)Ols.Status.DLL_NOT_FOUND:
//                    ols = null;
//                    // MessageBox.Show("WingRing0 Status Error!! DLL_NOT_FOUND");
//                    break;
//                case (uint)Ols.Status.DLL_INCORRECT_VERSION:
//                    ols = null;
//                    //  MessageBox.Show("WingRing0 Status Error!! DLL_INCORRECT_VERSION");
//                    break;
//                case (uint)Ols.Status.DLL_INITIALIZE_ERROR:
//                    ols = null;
//                    //  MessageBox.Show("WingRing0 Status Error!! DLL_INITIALIZE_ERROR");
//                    break;
//            }
//            if (ols == null)
//            {
//                //RaiseOlsInitFailedEvent();
//                return;
//            }

//            // Check WinRing0 status
//            switch (ols.GetDllStatus())
//            {
//                case (uint)Ols.OlsDllStatus.OLS_DLL_NO_ERROR:
//                    break;
//                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED:
//                    //  MessageBox.Show("WingRing0 DLL Status Error!! OLS_DRIVER_NOT_LOADED");
//                    ols = null;
//                    break;
//                case (uint)Ols.OlsDllStatus.OLS_DLL_UNSUPPORTED_PLATFORM:
//                    // MessageBox.Show("WingRing0 DLL Status Error!! OLS_UNSUPPORTED_PLATFORM");
//                    ols = null;
//                    break;
//                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_FOUND:
//                    //  MessageBox.Show("WingRing0 DLL Status Error!! OLS_DLL_DRIVER_NOT_FOUND");
//                    ols = null;
//                    break;
//                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_UNLOADED:
//                    // MessageBox.Show("WingRing0 DLL Status Error!! OLS_DLL_DRIVER_UNLOADED");
//                    ols = null;
//                    break;
//                case (uint)Ols.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK:
//                    //  MessageBox.Show("WingRing0 DLL Status Error!! DRIVER_NOT_LOADED_ON_NETWORK");
//                    ols = null;
//                    break;
//                case (uint)Ols.OlsDllStatus.OLS_DLL_UNKNOWN_ERROR:
//                    //  MessageBox.Show("WingRing0 DLL Status Error!! OLS_DLL_UNKNOWN_ERROR");
//                    ols = null;
//                    break;
//            }
//            if (ols == null)
//            {
//                //RaiseOlsInitFailedEvent();
//                return;
//            }
//        }

//        public static void OlsFree()
//        {
//            if (ols != null)
//                ols.DeinitializeOls();
//        }

//    }
//}