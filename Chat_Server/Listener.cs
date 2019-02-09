using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chat_server
{
    class Listener
    {
        int _port;
        //TcpListener _listener;
        Socket _listener;
        int _portSSL;
        X509Certificate certificate;
        TcpListener _listenerSSL;

        public Listener(int port, int portSSL)
        {
            _port = port;
            _portSSL = portSSL;
        }

        public int Start()
        {
            certificate = new X509Certificate("123.pfx", "gps6520");
            Task.Factory.StartNew(() =>
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(new IPEndPoint(IPAddress.Any, _port));

                while (true)
                {
                    //TcpClient client;
                    Socket client;
                    try
                    {
                        //client = _listener.AcceptTcpClient();
                        client = _listener.Accept();
                        Task.Factory.StartNew(() => clientStart(client));
                    }
                    catch (Exception e)
                    { }
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                _listenerSSL = new TcpListener(IPAddress.Any, _portSSL);
                _listenerSSL.Start();

                while (true)
                {
                    TcpClient client;
                    try
                    {
                        client = _listenerSSL.AcceptTcpClient();
                    }
                    catch (Exception e)
                    { }
                }
            }, TaskCreationOptions.LongRunning);
            return 0;
        }

        private void clientStart(Socket client)
        {
            string strRequest;
            {
                byte[] buffer = new byte[10240];
                int receivedBCount = client.Receive(buffer);
                strRequest = Encoding.UTF8.GetString(buffer, 0, receivedBCount);

                buffer = null;
            }
            // Парсим запрос
            string httpMethod = strRequest.Substring(0, strRequest.IndexOf(" "));

            int start = strRequest.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strRequest.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strRequest.Substring(start, length);
        }
    }
}
