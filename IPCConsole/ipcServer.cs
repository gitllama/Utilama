namespace ViewModels
{
    public class IPCViewModel
    {
        /* コンストラクタ・初期化 ****************************** */
        public IPCViewModel(CisModel vm)
        {
            this.cis = vm;
            ipc = new IPCSaver("name");
            ipc.obj.PropertyChanged += (sender, e) =>
            {
                try
                {
                    if (e.PropertyName == "command")
                    {
                        var dispatcher = System.Windows.Application.Current.Dispatcher;
                        if (dispatcher.CheckAccess())
                        {
                            sendcommands(ipc.obj.command);
                        }
                        else
                        {
                            dispatcher.Invoke(() => sendcommands(ipc.obj.command));
                        }
                    }
                }
                catch(Exception ex)
                {
                    ipc.obj.echo = $"{ex}";
                }
            };
        }
        /* プロパティ・コマンド ****************************** */
        public CisModel cis;
        IPCSaver ipc { get; set; }
        #region cis変更通知プロパティ
        private string _comm;
        public string comm
        {
            get
            { return _comm; }
            set
            {
                _comm = value;
                //RaisePropertyChanged();
            }
        }
        #endregion
        public void sendcommands(string command)
        {
            var com = command.Split(':');
            var key = com[0].Trim();
            var value = com.Length > 1 ? com[1].Trim() : "";
            switch (key)
            {
                case "status":
                    ipc.obj.echo = $"{cis.Status}";
                    break;
                case "reset":
                    ipc.obj.echo = $"{cis.Reset()}";
                    break;
                case "run":
                    ipc.obj.echo = $"{cis.Run()}";
                    break;
                case "write":
                    cis.WriteReg(int.Parse(value.Split(',')[0]), int.Parse(value.Split(',')[1]));
                    ipc.obj.echo = $"write";
                    break;
                case "read":
                    var addr = int.TryParse(value, out int buf) ? buf : Convert.ToInt32(value, 16);
                    var data = cis.ReadReg(addr);
                    var str =
                      $"0x{addr:X2}({addr}) : 0x{(data & 0x00_00_00_FF):X2}({(data & 0x00_00_00_FF)})\r\n"
                    + $"0x{addr + 1:X2}({addr + 1}) : 0x{(data & 0x00_00_FF_00) >> 8:X2}({(data & 0x00_00_FF_00) >> 8})\r\n"
                    + $"0x{addr + 2:X2}({addr + 2}) : 0x{(data & 0x00_FF_00_00) >> 16:X2}({(data & 0x00_FF_00_00) >> 16})\r\n"
                    + $"0x{addr + 3:X2}({addr + 3}) : 0x{(data & 0xFF_00_00_00) >> 24:X2}({(data & 0xFF_00_00_00) >> 24})";
                    ipc.obj.echo = $"{str}";
                    break;
                case "capture":
                    var i = value.Split(',');
                    cis.CaptureStart(i[0], Convert.ToInt32(i[1]), ToObject<CisModel.CaptureEnum>(i[2].Trim()));
                    break;
                case "property":
                    string hoge = "";
                    foreach(var j in cis.GetType().GetProperties())
                    {
                        hoge += $"{j}\r\n";
                    }
                    ipc.obj.echo = hoge;
                    break;
                case "":
                    ipc.obj.echo = "null";
                    break;
                default:
                    try
                    {
                        var mypropertyinfo = cis.GetType().GetProperty(key);
                        if(mypropertyinfo.PropertyType.IsEnum)
                        {
                            Type t = mypropertyinfo.PropertyType;
                            mypropertyinfo.SetValue(cis, Enum.Parse(t, value), null);
                        }
                        else
                        {
                            mypropertyinfo.SetValue(cis, Convert.ChangeType(value, mypropertyinfo.PropertyType), null);
                        }
                        ipc.obj.echo = $"{command}";
                    }
                    catch
                    {
                        ipc.obj.echo = $"command not found";
                    }
                    break;
            }
        }
        public static T ToObject<T>(string value) => (T)Enum.ToObject(typeof(T), value);
    }
}
