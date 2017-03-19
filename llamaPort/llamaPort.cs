using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using llamaPorts.Expressions;

namespace llamaPorts
{
    public class llamaPort
    {
        public SerialPort port = new SerialPort();

        public struct Commands
        {
            public string transmit { get; set; }
            public string receive { get; set; }
        }

        /*プロパティ*/


        public Dictionary<string, Commands> Command { get; set; } = new Dictionary<string, Commands>();

        public string PortName { get => port.PortName; set => port.PortName = value; }

        public int BaudRate { get => port.BaudRate; set => port.BaudRate = value; }

        public int DataBits { get => port.DataBits; set => port.DataBits = value; }

        private bool NewLineBool = true;
        public string NewLine
        {
            get => port.NewLine
                .Replace("\r", @"\r")
                .Replace("\n", @"\n");
            set
            {
                if (value != "")
                {
                    port.NewLine = value
                    .Replace(@"\r", "\r")
                    .Replace(@"\n", "\n");
                    NewLineBool = true;
                }

                else
                    NewLineBool = false;
            }
        }

        public string StopBits
        {
            get => port.StopBits.ToString();
            set => port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), value, true);
        }

        public string Parity
        {
            get => port.Parity.ToString();
            set => port.Parity = (Parity)Enum.Parse(typeof(Parity), value, true);
        }

        public int ReadTimeout { get => port.ReadTimeout; set => port.ReadTimeout = value; }
        public int WriteTimeout { get => port.WriteTimeout; set => port.WriteTimeout = value; }

        public int Wait { get; set; } = 500;

        /*インデクサ*/

        //public string this[string s] => SendLine(Command[s]);
        //public string this[string s, double v] => SendLine(string.Format(Command[s], v));
        //public string this[string s, int v] => SendLine(string.Format(Command[s], v));


        /*コンストラクタ*/

        public llamaPort() { }


        /*メソッド*/

        public void Open()
        {
            try
            {
                port.Open();
            }
            catch(Exception e)
            {
                throw new ArgumentNullException( $"{e}\r\nOpen port is{GetPortNames()}");
            }
        }


        public void Close() => port.Close();

        public string GetPortNames() => string.Join(",", SerialPort.GetPortNames());

        public string GetCommand(string src, object value = null)
        {
            if (NewLineBool)
            {
                string com = "";
                switch (value)
                {
                    case null:
                        com = Command[src].transmit.Encode();
                        break;
                    case int v:
                        com = Command[src].transmit.Encode(v);
                        break;
                    case double v:
                        com = Command[src].transmit.Encode(v);
                        break;
                    case string v:
                        com = Command[src].transmit.Encode(v);
                        break;
                }
                return com;
            }
            else
            {
                return Command[src].transmit.Encode();
            }
        }
        public string SendCommand(string src, object value = null)
        {
            port.WriteLine(GetCommand(src, value));
            Thread.Sleep(Wait);

            if (NewLineBool)
            {   
                return port.ReadLine().Decode(Command[src].receive);
            }
            else
            {
                byte[] a = new byte[port.BytesToRead];
                port.Read(a, 0, a.Length);
                int read = 0; 
                while (read < rbyte) 
                { 
                    int length = port.Read(a, read, a.Length - read); 
                    read += length; 
                } 
                return System.Text.Encoding.ASCII.GetString(a).Decode(Command[src].receive);
            }
        }

        public string SendLine(string command)
        {
            port.WriteLine(command);

            Thread.Sleep(Wait);

            return port.ReadLine();
        }
        public void WriteLine(string command) => port.WriteLine(command);
        public string ReadLine() => port.ReadLine();

    }
}
