using EasySave_GUI.Libs;
using EasySave_GUI.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace EasySave_GUI.Models
{
    class Message
    {
        public static readonly Message OK_MESSAGE = new Message { Error=false,CMD="OK_Message"};
        public bool Error;
        public string CMD;
        public object obj;
    }
    class ConnectMessage : Message
    {
        public string password;
    }
    class GetTasks : RelayCommand
    {
        public GetTasks(Action<object> executemethod, Func<object, bool> canexecutemethod) : base(executemethod, canexecutemethod) { }

        public override string ToString()=>"GetTasks";
    }
    class PauseTask : RelayCommand
    {
        public PauseTask(Action<object> executemethod, Func<object, bool> canexecutemethod) : base(executemethod, canexecutemethod)
        {
        }
        public override string ToString() => "PauseTask";
    }
    internal class Serveur
    {
        Socket Server;
        Socket Client;
        public GetTasks GetTasks;
        public Dictionary<string, RelayCommand> cmds = new Dictionary<string, RelayCommand>();
        private string password;
        public Serveur(string p)
        {
            password = p;
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void SeConnecter(string ip, int port)
        {
            IPAddress ipAddress2 = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddress2, port);
            Server.Bind(endPoint);
            Server.Listen(1);
        }
        public void AccepterConnection( )
        {
            Client = Server.Accept();
            ConnectMessage msg = EcouterReseau<ConnectMessage>();
            if (msg.password != password)
            {
                envoiData(new Message { Error = true });
                AccepterConnection();
                return;
            }
            else
            {
                envoiData(new Message { Error = false });
            }

        }
        public T EcouterReseau<T>()
        {
            byte[] data = new Byte[1024];
            string a = "";
            try
            {
                int bytesRec = Client.Receive(data);
                a = Encoding.UTF8.GetString(data, 0, bytesRec);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return JsonConvert.DeserializeObject<T>(a);
        }
        public void envoiData(object obj)
        {
            var data = (Message)obj;
            byte[] dataSend = new Byte[1024];
            var ss = "";
            //Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CurrentCulture;
            dataSend = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            Client.Send(dataSend, dataSend.Length, SocketFlags.None);
        }
        public static void Deconnecter(Socket client)
        {
            client.Close();
        }
        public void LaunchServer(Preferences p)
        {
            
            SeConnecter("127.0.0.1", 2906);
            Thread t = new Thread(() =>
            {
                Message cmd ;
                while (true)
                {
                    if (Client == null || !Client.Connected)
                    {
                        AccepterConnection();
                    }
                    cmd = EcouterReseau<Message>();
                    Debug.WriteLine(cmd);
                    ExecuteCmd(cmd);
                }
            });
            t.Start();
        }
        private void ExecuteCmd(Message cmd)
        {
            if(cmd == null|| cmd == Message.OK_MESSAGE)
            {
                return;
            }
            if (cmd.Error) MessageBox.Show("An error occured with the distant Client");
            else
            {
                cmds[cmd.CMD].Execute(cmd.obj);
            }
        }
    }
}
