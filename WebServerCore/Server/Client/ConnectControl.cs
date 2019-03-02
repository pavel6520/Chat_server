using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Server.Client {
    class ConnectControl {
        public ConnectType? Type { get => type; }

        private bool crypt;
        private ConnectType? type;
        private Socket client;
        private SslStream sslStream;
        private NetworkStream stream;

        public ConnectControl(Socket client, X509Certificate crypt = null) {
            this.client = client;
            if (crypt != null) {
                this.crypt = true;
                sslStream = new SslStream(new NetworkStream(client));
                sslStream.AuthenticateAsServer(crypt, false, System.Security.Authentication.SslProtocols.Tls12, false);
            }
            else {
                this.crypt = false;
                stream = new NetworkStream(client);
            }
        }

        private byte? _ReadByte() {
            int readed;
            if (crypt)
                readed = sslStream.ReadByte();
            else
                readed = stream.ReadByte();
            if (readed == -1)
                return null;
            else
                return (byte)readed;
        }

        private byte[] _Read(int count = 10000) {
            byte[] read = null;
            if (crypt)
                sslStream.Read(read, 0, count);
            else
                stream.Read(read, 0, count);
            return read;
        }

        private string _ReadLine() {
            bool r = false;
            string buf = "";
            byte? readB = _ReadByte();
            string read;

            while (readB != null) {
                read = Encoding.ASCII.GetString(new byte[] { (byte)readB });

                if (read == "\r")
                    r = true;
                else if (read == "\n" && r)
                    break;
                else
                    buf += read;
                readB = _ReadByte();
            }
            return buf;
        }

        private void _WriteByte(byte b) {
            if (crypt)
                sslStream.WriteByte(b);
            else
                stream.WriteByte(b);
        }

        private void _Write(byte[] b) {
            if (crypt)
                sslStream.Write(b);
            else
                stream.Write(b, 0, b.Length);
        }

        public void GetMethod() {
            if (type == null) {
                string connect = _ReadLine();
            }
            else throw new Exception("Метод соединения уже определен");
        }

        public byte[] Read(uint? count = null) {



            return null;
        }

        public string ReadLine() {

            return null;
        }

        public enum ConnectType {
            TCP = 0,
            HTTP = 1,
            WEBSOCKET = 2
        }
    }
}