using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Client {
    public class Connection {
        public string UserAddress { get { return client.RemoteEndPoint.ToString(); } }
        public bool Connected { get { return client.Connected; } }
        public int Available { get { return client.Available; } }

        protected readonly bool crypt;
        protected readonly Socket client;
        protected readonly SslStream sslStream;
        protected readonly NetworkStream stream;

        internal Connection(Socket client, X509Certificate crypt = null) {
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

        protected Connection(Connection cc) {
            crypt = cc.crypt;
            client = cc.client;
            sslStream = cc.sslStream;
            stream = cc.stream;
        }

        public byte? ReadByte() {
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

        public byte[] Read(int count = 10000) {
            byte[] read = null;
            if (crypt)
                sslStream.Read(read, 0, count);
            else
                stream.Read(read, 0, count);
            return read;
        }

        public string ReadLine() {
            bool r = false;
            string buf = "";
            byte? readB = ReadByte();
            string read;

            while (readB != null) {
                read = Encoding.ASCII.GetString(new byte[] { (byte)readB });

                if (read == "\r")
                    r = true;
                else if (read == "\n" && r)
                    break;
                else
                    buf += read;
                readB = ReadByte();
            }
            return buf;
        }

        public void WriteByte(byte b) {
            if (crypt)
                sslStream.WriteByte(b);
            else
                stream.WriteByte(b);
        }

        public void Write(byte[] b) {
            if (crypt)
                sslStream.Write(b);
            else
                stream.Write(b, 0, b.Length);
        }

        public void WriteLine(string s) {
            Write(Encoding.UTF8.GetBytes($"{s}\r\n"));
        }
    }
}