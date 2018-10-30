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

            //thread = new Thread(new ParameterizedThreadStart(Listener));
            //thread.Start(listener);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (State)
                    {
                        TcpClient tcpClient = listener.AcceptTcpClient();

                        ShowInfo(tcpClient);

                        ClientClass client = new ClientClass(tcpClient, this);
                        clients.Add(client);
                        client.Start();
                    }
                }
                catch (SocketException) { }
            });
            
            Console.WriteLine(DateTime.Now + " [INFO] TCP-сервер запущен, порт " + port);
        }

        private void ShowInfo(TcpClient client)
        {
            Console.WriteLine(DateTime.Now + " [DEBUG][TCP] Подключение: " + client.Client.RemoteEndPoint);
        }

        internal void Message(ClientClass sender, string message, string loginRecipient = null)
        {
            foreach (ClientClass cc in clients)
            {
                if (cc.isReady)
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
                //cc.Close();
            }
            //TODO: добавление сообщения в бд
        }

        internal void ClientClosed(byte[] Id)
        {
            foreach (ClientClass cc in clients)
            {
                if (cc.Id == Id)
                {
                    cc.Close();
                    clients.Remove(cc);
                    return;
                }
            }
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
