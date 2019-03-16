using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Connection {
    public class Connection {
        public string UserAddress { get { return client.RemoteEndPoint.ToString(); } }
        public bool Connected { get { return client.Connected; } }
        public int Available { get { return client.Available; } }
        public readonly bool Crypt;
        public int ReceiveTimeout { get { return client.ReceiveTimeout; } set { client.ReceiveTimeout = value; } }

        protected readonly Socket client;
        protected readonly SslStream sslStream;
        protected readonly NetworkStream stream;

        internal Connection(Socket client, X509Certificate crypt = null) {
            this.client = client;
            if (crypt != null) {
                this.Crypt = true;
                sslStream = new SslStream(new NetworkStream(client));
                sslStream.AuthenticateAsServer(crypt);
            }
            else {
                this.Crypt = false;
                stream = new NetworkStream(client);
            }
            Log.Write(LogType.INFO, "Connection", $"Подключение от {client.RemoteEndPoint} {(crypt == null ? "без шифрования" : "с шифрованием")}");
        }

        internal protected Connection(Connection cc) {
            Crypt = cc.Crypt;
            client = cc.client;
            sslStream = cc.sslStream;
            stream = cc.stream;
        }

        internal void Close() {
            if (Crypt) {
                if (sslStream != null) {
                    sslStream.Close();
                }
            }
            else {
                if (stream != null) {
                    stream.Close();
                }
            }
        }

        public byte? ReadByte() {
            int readed;
            try {
                if (Crypt)
                    readed = sslStream.ReadByte();
                else
                    readed = stream.ReadByte();
            }
            catch (System.IO.IOException e) {
                throw new ConnectionCloseException("Удаленный хост разорвал соединение", e);
            }
            if (readed == -1)
                return null;
            else
                return (byte)readed;
        }

        public byte[] Read(int count = 10000) {
            byte[] read = new byte[count];
            try {
                if (Crypt)
                    sslStream.Read(read, 0, count);
                else
                    stream.Read(read, 0, count);
            }
            catch (System.IO.IOException e) {
                throw new ConnectionCloseException("Удаленный хост разорвал соединение", e);
            }
            return read;
        }

        /// <summary>
        /// Читает из взодящего потока строку в кодировке ASCII до спец. символов \r\n (\r\n не возвращаются)
        /// </summary>
        /// <returns></returns>
        public string ReadLine() {
            bool r = false;
            string buf = "";
            byte? readB = ReadByte();
            string read;

            while (readB != null) {
                read = Encoding.UTF8.GetString(new byte[] { (byte)readB });

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
            try {
                if (Crypt)
                    sslStream.WriteByte(b);
                else
                    stream.WriteByte(b);
            }
            catch (System.IO.IOException e) {
                throw new ConnectionCloseException("Удаленный хост разорвал соединение", e);
            }
        }

        public void Write(byte[] b) {
            try {
                if (Crypt)
                    sslStream.Write(b);
                else
                    stream.Write(b, 0, b.Length);
            }
            catch (System.IO.IOException e) {
                throw new ConnectionCloseException("Удаленный хост разорвал соединение", e);
            }
        }

        /// <summary>
        /// Записывает в исходящий поток строку в кодировке ASCII, добавляя спец. символы \r\n
        /// </summary>
        /// <param name="s"></param>
        public void WriteLine(string s = "") {
            Write(Encoding.ASCII.GetBytes($"{s}{Environment.NewLine}"));
        }
    }
}