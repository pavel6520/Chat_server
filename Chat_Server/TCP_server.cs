using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Chat_server
{
    class TCP_Server
    {
        TcpListener listener;
        int port;
        public bool State { get; private set; }

        List<ClientClass> clients;
        List<string> online;

        public TCP_Server(int port)
        {
            clients = new List<ClientClass>();
            this.port = port;
            State = false;
            listener = new TcpListener(IPAddress.Any, port);
        }

        public bool Start()
        {
            //listener = new TcpListener(IPAddress.Parse(address), 8080);
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " [ERROR][TCP] Не удалось запустить. " + ex.Message);
                return false;
            }

            State = true;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (State)
                    {
                        TcpClient tcpClient = listener.AcceptTcpClient();

                        ClientClass client = new ClientClass(tcpClient, this);
                        clients.Add(client);
                        client.Start();
                    }
                }
                catch (SocketException) { }
            }, TaskCreationOptions.LongRunning);
            
            Console.WriteLine(DateTime.Now + " [INFO][TCP] Сервер запущен, порт " + port);
            return true;
        }

        internal void Message(ClientClass sender, string message, string loginRecipient = null)
        {
            DateTime date = DateTime.Now;
            //TODO: добавление сообщения в бд
            foreach (ClientClass cc in clients)
            {
                if (cc.Status == ClientClass.ClientStatus.Ready)
                {
                    try
                    {
                        if (loginRecipient == null)
                        {
                            cc.SendMessagePublic(sender, message, date);
                        }
                        else if (cc.Login == loginRecipient)
                        {
                            cc.SendMessage(sender, message);
                            break;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        Task.Factory.StartNew(() => ClientClosed(cc));
                    }
                }
                //cc.Close();
            }
        }

        internal void ClientClosed(ClientClass forDel)
        {
            //ClientClass cc = clients.Find(delegate (ClientClass x) { return x.Id == Id; });
            forDel.Close();
            if (Program.DEBUG)
                Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + forDel.Address + " " + forDel.Login + " отключен");

            clients.FindAll(delegate (ClientClass x) { return x.Login == forDel.Login; });
            clients.Remove(forDel);
            return;
        }

        public void Stop()
        {
            listener.Stop();
            
            foreach (ClientClass cc in clients)
            {
                cc.Close();
            }
            clients.Clear();
            Console.WriteLine(DateTime.Now + " [INFO][TCP] Сервер остановлен");
        }
    }
}
