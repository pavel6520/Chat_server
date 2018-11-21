using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            online = new List<string>();
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
                    //try
                    //{
                        if (loginRecipient == null)
                        {
                            cc.SendMessagePublic(sender, message, date);
                        }
                        else if (cc.Login == loginRecipient)
                        {
                            cc.SendMessage(sender, message);
                            break;
                        }
                    /*}
                    catch (InvalidOperationException)
                    {
                        Task.Factory.StartNew(() => ClientClosed(cc));
                    }*/
                }
                //cc.Close();
            }
        }

        internal void ClientLogged(ClientClass client)
        {
            if (!online.Contains(client.Login))
            {
                online.Add(client.Login);
                //TODO LOGIN
                Message(client, "Вошел в чат");
            }
        }

        internal void ClientClosed(ClientClass forDel)
        {
            //ClientClass cc = clients.Find(delegate (ClientClass x) { return x.Id == Id; });
            forDel.Close();
            if (Program.DEBUG)
                Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + forDel.Address + " " + forDel.Login + " отключен");
            clients.Remove(forDel);
            if (!clients.Exists(delegate (ClientClass x) { return x.Login == forDel.Login; }))
            {
                online.Remove(forDel.Login);
                //TODO LOGOUT
                if (forDel.Status == ClientClass.ClientStatus.Ready || forDel.Status == ClientClass.ClientStatus.Stopped)
                Message(forDel, "Покинул чат");
                foreach (ClientClass cc in clients)
                {
                    if (cc.Status == ClientClass.ClientStatus.Ready)
                    {
                        //try
                        //{
                            //cc.ClientOffline(sender, message, date);
                        /*}
                        catch (InvalidOperationException)
                        {
                            Task.Factory.StartNew(() => ClientClosed(cc));
                        }*/
                    }
                }
            }
            //Console.WriteLine("Online: " + clients.Count + " " + ClientClass.Count);
            return;
        }

        internal ReadOnlyCollection<string> Online { get { return online.AsReadOnly(); } }

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
