using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ylamls.Numeric
{
    public static class NumConverter
    {
        public static (UInt32 value, int bitNum) Parse(string value)
        {
            uint dst = 0;
            int bitNum = 0;
            value = value
                .Replace("_", "")
                .Replace(",", "")
                .Trim();

            if (value.StartsWith("0x"))
            {
                //16進数
                dst = Convert.ToUInt32(value, 16);
                bitNum = (value.Length - 2) * 8;
            }
            else if (value.StartsWith("0b"))
            {
                //2進数
                dst = Convert.ToUInt32(value.Replace("0b", ""), 2);
                bitNum = (value.Length - 2);
            }
            else
            {
                //10進数
                dst = Convert.ToUInt32(value, 10);
                bitNum = (int)Math.Log(Math.Pow(10, value.Length) - 1, 2);
            }

            return (dst, bitNum);



        }

        public static (uint value, uint mask) ParseWithMask(string value)
        {
            value = value.Replace("_", "").Replace(",", "").Trim();
            value = value.Replace("0b", "");

            var dst = value
                .Replace(".", "0")
                .Replace("x", "0");
            var mask = value
                .Replace("0", "1")
                .Replace("1", "0")
                .Replace(".", "1")
                .Replace("x", "1");

            return (Convert.ToUInt32(dst, 2), Convert.ToUInt32(mask, 2));
        }

        public static uint And(this uint src1, string src2)
        {
            var i = ParseWithMask(src2);
            return (src1 & i.mask) | i.value;
        }

    }


    //throw new FormatException();

    //uint a =
    //uint.TryParse(i, out var dst_10) ? dst_10 :
    //uint.TryParse(i, System.Globalization.NumberStyles.HexNumber, null, out var dst_16) ? dst_16 :
    //Convert.ToUInt32(i, 2);
}
