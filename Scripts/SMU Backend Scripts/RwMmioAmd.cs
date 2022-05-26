using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AATUV3;

namespace AATUV3.Scripts.SMU_Backend_Scripts
{
    internal class RwMmioAmd
    {
        readonly int[] bclk_table = { 100, 101, 102, 103, 96, 97, 98, 99, 108, 109, 110, 111, 104, 105, 106, 107, 116, 117, 118, 119,
        112, 113, 114, 115, 124, 125, 126, 127, 120, 121, 122, 123, 132, 133, 134, 135, 128, 129, 130, 131, 140,
        141, 142, 143, 136, 137, 138, 139, 148, 149, 150, 151, 144, 145, 146, 147 };
        const uint MISC_baseAddress = 0xfed80E00;
        //const uint SMUIO_baseAddress = 0x0005A000;
        //const uint SMBUS_baseAddress = 0xFED80A00;
        //const uint UMC0_baseAddress = 0x00050000;
        const uint MiscClkCntl1 = 0x40;
        const uint CGPLLCONFIG1 = 0x8;
        const uint CGPllCfg3 = 0x10;

        uint RdFchMisc(uint regadd, uint startbit, uint endbit)
        {
            UIntPtr p = (UIntPtr)(MISC_baseAddress + regadd);
            uint pdwPhysVal = 0;
            AATUV3.MainWindow.GetPhysLong(p, ref pdwPhysVal);

            uint value = 0;
            value = ((uint)((int)pdwPhysVal >> (int)startbit)) & (max_valuefrombits(endbit - startbit + 1));
            return value;
        }

        //done
        void WrFchMisc(uint regadd, uint Value, uint startbit, uint endbit)
        {
            UIntPtr p = (UIntPtr)(MISC_baseAddress + regadd);
            uint pdwPhysVal = 0;
            AATUV3.MainWindow.GetPhysLong(p, ref pdwPhysVal);
            pdwPhysVal = setbit2(pdwPhysVal, Value, startbit, endbit);
            AATUV3.MainWindow.SetPhysLong(p, pdwPhysVal);
            //Cg1CfgUpdateReq();
        }
        uint max_valuefrombits(uint value)
        {
            return (uint)(((ulong)1 << (int)value) - 1);
        }

        private uint setbit2(uint pdwPhysVal, uint value, uint startbit, uint endbit)
        {
            uint result = pdwPhysVal;
            if (endbit >= startbit)
            {
                int shift = (int)(endbit - startbit + 1);
                uint mask = (uint)((((ulong)1 << shift) - 1) << ((int)startbit));
                result &= ~mask;

                value &= (uint)(((ulong)1 << shift) - 1);
                result |= value << (int)(startbit);
            }
            return result;
        }

        void Cg1CfgUpdateReq()
        {
            UIntPtr p = (UIntPtr)(MISC_baseAddress + MiscClkCntl1);
            uint pdwPhysVal = 0;
            AATUV3.MainWindow.GetPhysLong(p, ref pdwPhysVal);
            pdwPhysVal = setbit2(pdwPhysVal, 1, 30, 30);
            AATUV3.MainWindow.SetPhysLong(p, pdwPhysVal);
        }

        public double GetBclk()
        {
            uint bclk = RdFchMisc(CGPllCfg3, 4, 12);
            uint bclk_decimal = RdFchMisc(CGPllCfg3, 25, 28);
            double bclk_decimal_value = (double)(bclk_decimal) * 0.0625;
            if (bclk >= bclk_table.Length)
            {
                return 151 + bclk_decimal_value;
            }
            int i;
            for (i = 0; i < bclk_table.Length; i++)
            {
                if (bclk == i)
                    break;
            }
            double bclk2 = bclk_table[i];
            bclk2 += bclk_decimal_value;
            return bclk2;
        }
        public void SetBclk(double BCLK)
        {
            DisableSsc();
            Cg1FbdivLoadEn();
            CG1PllFcw0IntOverride(BCLK);
            Cg1CfgUpdateReq();
        }

        void CG1PllFcw0IntOverride(double BCLK)
        {
            WrFchMisc(CGPllCfg3, calculate_bclk((int)BCLK), 4, 12);
            int int_portion = (int)BCLK;
            double remainder = BCLK - int_portion;
            if (remainder > 0.0)
            {
                int number_of_units = (int)(remainder / 0.0625);
                WrFchMisc(CGPllCfg3, (uint)number_of_units, 25, 28);
            }
            else
            {
                WrFchMisc(CGPllCfg3, 0, 23, 28);
            }
        }

        //done
        uint calculate_bclk(int bclk)
        {
            if (bclk < 96)
                bclk = 96;
            if (bclk > 151)
                bclk = 151;
            uint i = 0;
            for (i = 0; i < bclk_table.Length; i++)
            {
                if (bclk == bclk_table[i])
                {
                    return i;
                }
            }
            return 0;
        }

        //done
        void DisableSsc()
        {
            WrFchMisc(CGPLLCONFIG1, 0, 0, 0);
        }

        void Cg1FbdivLoadEn()
        {
            WrFchMisc(MiscClkCntl1, 1, 25, 25);
        }

        public void SetBclk2(double BCLK)
        {
            // disable ssc
            uint pdwPhysVal = 0;
            AATUV3.MainWindow.GetPhysLong((UIntPtr)0xFED80E08, ref pdwPhysVal); // cpu.io.GetPhysLong((UIntPtr)0xFED80E08, out uint value);
            pdwPhysVal = setbit2(pdwPhysVal, 0, 0, 0);
            AATUV3.MainWindow.SetPhysLong((UIntPtr)0xFED80E08, pdwPhysVal); // cpu.io.SetPhysLong((UIntPtr)0xFED80E08, ZenStates.Core.Utils.SetBits(value, 0, 0, 0));

            // CG1_FBDIV_LoadEn
            AATUV3.MainWindow.GetPhysLong((UIntPtr)0xFED80E40, ref pdwPhysVal); // cpu.io.GetPhysLong((UIntPtr)0xFED80E40, out value);
            pdwPhysVal = setbit2(pdwPhysVal, 1, 25, 25);
            AATUV3.MainWindow.SetPhysLong((UIntPtr)0xFED80E40, pdwPhysVal); // cpu.io.SetPhysLong((UIntPtr)0xFED80E40, ZenStates.Core.Utils.SetBits(value, 25, 1, 1));

            // Set bclk
            AATUV3.MainWindow.GetPhysLong((UIntPtr)0xFED80E10, ref pdwPhysVal); //cpu.io.GetPhysLong((UIntPtr)0xFED80E10, out value);
            double targetBclk = BCLK; // double.Parse(numericUpDownBclk.Text);
            uint fraction = (uint)Math.Round((targetBclk - Math.Floor(targetBclk)) / 0.0625);
            if (fraction > 15)
                fraction = 15;

            uint index = calculate_bclk((int)targetBclk);// (uint)Array.IndexOf(bclkTable, Convert.ToInt32(Math.Floor(targetBclk)));
            pdwPhysVal = setbit2(pdwPhysVal, index, 4, 12); // value = ZenStates.Core.Utils.SetBits(value, 4, 9, index);
            pdwPhysVal = setbit2(pdwPhysVal, fraction, 25, 28);  //value = ZenStates.Core.Utils.SetBits(value, 23, 6, fraction);
            AATUV3.MainWindow.SetPhysLong((UIntPtr)0xFED80E10, pdwPhysVal); // cpu.io.SetPhysLong((UIntPtr)0xFED80E10, value);

            AATUV3.MainWindow.GetPhysLong((UIntPtr)0xFED80E40, ref pdwPhysVal); //cpu.io.GetPhysLong((UIntPtr)0xFED80E40, out value);
            pdwPhysVal = setbit2(pdwPhysVal, 1, 30, 30);
            AATUV3.MainWindow.SetPhysLong((UIntPtr)0xFED80E40, pdwPhysVal); //cpu.io.SetPhysLong((UIntPtr)0xFED80E40, ZenStates.Core.Utils.SetBits(value, 30, 1, 1));
        }

        public void testBclk(double BCLK)
        {
            SetBclk2(BCLK);

            // test output:
            Console.WriteLine("SetBclk2(" + BCLK + ")=" + GetBclk());
        }
    }
}
