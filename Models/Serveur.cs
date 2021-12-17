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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EasySave_GUI.Models
{
    class Message
    {
        public static readonly string  OK_MESSAGE = "OK";
        public bool Error;
        public string CMD;
        public object obj;
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
        public Serveur()
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void SeConnecter(string ip, int port)
        {
            IPAddress ipAddress2 = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddress2, port);
            Server.Bind(endPoint);
            Server.Listen(1);
        }
        public void AccepterConnection()
        {
            Client = Server.Accept();
        }
        public string EcouterReseau()
        {
            byte[] data = new Byte[1024];
            string a = "";
            try
            {
                int bytesRec = Client.Receive(data);
                a = Encoding.ASCII.GetString(data, 0, bytesRec);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return a;
        }
        public void envoiData(string data)
        {
            byte[] dataSend = new Byte[1024];
            dataSend = Encoding.ASCII.GetBytes(data);
            Debug.WriteLine("test" + data);
            Client.Send(dataSend, dataSend.Length, SocketFlags.None);
        }
        public static void Deconnecter(Socket client)
        {
            client.Close();
        }
        public void LaunchServer(Preferences p)
        {
            
            SeConnecter("127.0.0.1", 2906);
            AccepterConnection();
            MessageBox.Show(p.language == "FR" ? "Client Connecté" : "Client Connected");
            string cmd = "";
            while (true)
            {
                cmd = EcouterReseau();
                Debug.WriteLine(cmd);
                ExecuteCmd(cmd);
            }
        }
        private void ExecuteCmd(string cmd)
        {
            if(cmd == Message.OK_MESSAGE)
            {
                return;
            }
            Message msg = JsonConvert.DeserializeObject<Message>(cmd);
            if (msg == null) return;
            if (msg.Error) MessageBox.Show("An error occured with the distant Client");
            else
            {
                cmds[msg.CMD].Execute(msg.obj);
            }
        }
    }
}
