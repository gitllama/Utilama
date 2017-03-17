namespace IPC
{
    public class IpcRemoteObject : MarshalByRefObject, INotifyPropertyChanged
    {
        private string _command;
        public string command
        {
            get => _command;
            set
            {
                isReady = false;
                _echo = "";
                this.SetProperty(ref this._command, value);
            }
        }

        private string _echo;
        public string echo
        {
            get => _echo;
            set
            {
                this.SetProperty(ref this._echo, value);
                _command = "";
                isReady = true;
            }
        }

        private bool _isReady = true;
        public bool isReady
        {
            get => _isReady;
            private set => this.SetProperty(ref this._isReady, value);
        }

        public override object InitializeLifetimeService()
        {
            //一定時間アクセスしない場合exceptionがthowされるのでその対策
            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, value)) { return false; }
            field = value;
            var h = this.PropertyChanged;
            if (h != null) { h(this, new PropertyChangedEventArgs(propertyName)); }
            return true;
        }
    }

    public class IPCSaver
    {
        public IpcRemoteObject obj { get; set; }

        public IPCSaver(string s)
        {
            IpcServerChannel channel = new IpcServerChannel(s);
            ChannelServices.RegisterChannel(channel, true);
            obj = new IpcRemoteObject();
            //RemoteObject.cis = cis;
            RemotingServices.Marshal(obj, "handler", typeof(IpcRemoteObject));
        }
    }

    public class IPCClient
    {
        public IpcRemoteObject obj { get; set; }
        public string addr;

        public IPCClient()
        {
            var channel = new IpcClientChannel();
            ChannelServices.RegisterChannel(channel, true);

            /*
            IpcChannel channel = new IpcChannel();
            ChannelServices.RegisterChannel(channel, true);
            obj = new IpcRemoteObject();

            obj = Activator.GetObject(typeof(IpcRemoteObject), $"ipc://{s}/handler") as IpcRemoteObject;
            addr = $"ipc://{s}/handler";
            */
        }

        public void ObjSet(string s)
        {
            obj = new IpcRemoteObject();
            obj = Activator.GetObject(typeof(IpcRemoteObject), $"ipc://{s}/handler") as IpcRemoteObject;
            addr = $"ipc://{s}/handler";
        }

    }
}

namespace IPCConsole
{  
    class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<string, BTIPC.IPCClient> ipc = new Dictionary<string, BTIPC.IPCClient>();
            var ipc = new BTIPC.IPCClient();

            for (;;)
            {
                Console.Write($"{ipc?.addr ?? ""} > ");
                //Console.Write($" > ");
                var rl = Console.ReadLine();
                var hoge = rl.Split(':');
                var key = hoge[0].Trim();
                var value = hoge.Length > 1 ? hoge[1].Trim() : "";

                switch (key)
                {
                    case "process":
                        try
                        {
                            System.Diagnostics.Process.Start(value);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Process Start err");
                            Console.WriteLine(e);
                        }
                        break;
                    case "connect":
                        try
                        {
                            ipc.ObjSet(value);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("connect err");
                            Console.WriteLine(e);
                        }
                        break;
                    case "disconnect":
                        try
                        {
                            //ipc.obj[] = null;
                            ipc = null;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("disconnect err");
                            Console.WriteLine(e);
                        }
                        break;
                    default:
                        try
                        {
                            if (key.Split('.').Length > 1)
                            {
                                ipc.ObjSet(key.Split('.')[0].Trim());
                                ipc.obj.command = rl.Split('.')[1] + "\r\n";
                            } 
                            else
                            {
                                ipc.obj.command = rl + "\r\n";
                            }

                            while (!ipc.obj.isReady) Task.Delay(100);

                            Console.WriteLine(ipc.obj.echo);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("command err");
                            Console.WriteLine(e);
                        }
                        break;

                }


            }

        }
    }
}
