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
    class TCP_server
    {
        TcpListener listener;
        int port;
        public bool State { get; private set; }

        List<ClientClass> clients;

        public TCP_server(int port)
        {
            clients = new List<ClientClass>();
            this.port = port;
            State = false;
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            //listener = new TcpListener(IPAddress.Parse(address), 8080);
            listener.Start();

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
            
            Console.WriteLine(DateTime.Now + " [INFO] TCP-сервер запущен, порт " + port);
        }

        internal void Message(ClientClass sender, string message, string loginRecipient = null)
        {
            foreach (ClientClass cc in clients)
            {
                if (cc.isReady)
                {
                    try
                    {
                        if (loginRecipient == null)
                        {
                            cc.SendMessagePublic(sender, message);
                        }
                        else if (cc.Login == loginRecipient)
                        {
                            cc.SendMessage(sender, message);
                            break;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        Task.Factory.StartNew(() => ClientClosed(cc.Id));
                    }
                }
                //cc.Close();
            }
            //TODO: добавление сообщения в бд
        }

        internal void ClientClosed(byte[] Id)
        {
            ClientClass cc = clients.Find(delegate (ClientClass x) { return x.Id == Id; });
            cc.Close();
            Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + cc.Address + " отключен");
            clients.Remove(cc);
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
            Console.WriteLine(DateTime.Now + " [INFO] TCP-сервер остановлен");
        }
    }
}
