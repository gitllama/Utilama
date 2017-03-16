using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace llamaPorts.Expressions
{
    public static class RS232CExtensions
    {
        const char STX = (char)0x02;
        const char ETX = (char)0x03;
        const char ACK = (char)0x06;
        const char NAK = (char)0x15;
        const char EM = (char)0x19;
        const char ENQ = (char)0x05;


        public static string Encode(this string src) => src.ConvE().SetBCC();
   
        public static string Encode(this string src, dynamic value) => String.Format(src, value).Encode();

        public static string Decode(this string src, string anchor, string key = "result")
        {
            if (anchor == null || anchor == "") return src.ConvD();

            var _anchor = anchor.ConvE()
                .Replace("<BCCa>", "(?<bcc>.*)")
                .Replace("<BCCd>", "(?<bcc>.*)");

            Match match = (new Regex(_anchor, RegexOptions.IgnoreCase | RegexOptions.Singleline)).Match(src);
            if (!match.Success) return "Regex not Match";

            if (anchor.Contains("<BCCa>"))
            {
                if (match.Groups["bcc"].Value != src.GetCode().ToBCCascii()) return "bcc not Match";
            }
            else if(anchor.Contains("<BCCd>"))
            {
                if (match.Groups["bcc"].Value != src.GetCode().ToBCCascii()) return "bcc not Match";
            }

            return match.Groups[key].Value;
        }


        private static string ConvE(this string src)
        {
            return src
                .Replace("<STX>", STX.ToString())
                .Replace("<ETX>", ETX.ToString())
                .Replace("<ACK>", ACK.ToString())
                .Replace("<NAK>", NAK.ToString())
                .Replace("<EM>", EM.ToString())
                .Replace("<ENQ>", ENQ.ToString());
        }
        private static string ConvD(this string src)
        {
            return src
                .Replace(STX.ToString(), "<STX>")
                .Replace(ETX.ToString(), "<ETX>")
                .Replace(ACK.ToString(), "<ACK>")
                .Replace(NAK.ToString(), "<NAK>")
                .Replace(EM.ToString(), "<EM>")
                .Replace(ENQ.ToString(), "<ENQ>");
        }

        private static string SetBCC(this string src)
        {
            return src
                .Replace("<BCCa>", src.GetCode().ToBCCascii())
                .Replace("<BCCd>", src.GetCode().ToBCCdec());
        }
        private static string GetCode(this string src)
        {
            var _bcc = $"{STX}(?<bcc>.*){ETX}";
            Match matchbcc = (new Regex(_bcc, RegexOptions.IgnoreCase | RegexOptions.Singleline)).Match(src);
            return matchbcc.Success ? $"{matchbcc.Groups["bcc"].Value}{ETX}" : "";
        }

        private static string ToBCCdec(this string src) => BitConverter.ToString(new byte[] {src.ToBytes().Aggregate((n, next) => n ^= next) });
        private static string ToBCCascii(this string src) => Encoding.ASCII.GetString(new byte[] { src.ToBytes().Aggregate((n, next) => n ^= next) });
        private static byte[] ToBytes(this string src) => Encoding.ASCII.GetBytes(src);


    }
}
