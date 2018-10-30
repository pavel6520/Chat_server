using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chat_server
{
    class ClientClass
    {
        public static int Count { get; private set; }

        private TcpClient tcpClient;
        private TCP_server tcpServer;
        public byte[] Id { get; private set; }
        public string Login { get; private set; }
        public bool isAuth { get; private set; }
        public bool isReady { get; private set; }

        public ClientClass(TcpClient tcpClient, TCP_server tcpServer)
        {
            this.tcpClient = tcpClient;
            this.tcpServer = tcpServer;
            Id = Guid.NewGuid().ToByteArray();
            isAuth = false;
            Login = null;
        }

        public void Start()
        {
            Task.Factory.StartNew(Process);
        }

        public void Close()
        {
            tcpClient.Close();
        }

        private void Process()
        {
            if (tcpClient.Connected)
            {
                if (isAuth = Authorization())
                {
                    Count++;

                    //отправка истории
                    //
                    //
                    //отправка закончена

                    isReady = true;
                    while (true)
                    {
                        try
                        {
                            string inputData = DecodeMessage(Read());
                            Console.WriteLine(DateTime.Now + " [DEBUG][TCP] Mes: " + tcpClient.Client.RemoteEndPoint + " " + inputData);
                            //чтение из сокета и действия

                            tcpServer.Message(this, inputData);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Count--;
                            tcpServer.ClientClosed(Id);
                            return;
                        }
                    }
                }
            }
            
        }

        public void SendMessage(ClientClass sender, string message)
        {
            //отправка клиенту
        }

        public void SendMessagePublic(ClientClass sender, string message)
        {
            Send(EncodeMessageToSend(message));
        }

        private bool Authorization()//добавить авторизацию по логину-паролю
        {
            string inputData = Encoding.UTF8.GetString(Read());
            if (inputData.Substring(0, 14) == "GET /websocket"
                && Regex.IsMatch(inputData, "Connection: Upgrade")
                && Regex.IsMatch(inputData, "Upgrade: websocket"))
            {
                Send(
                    Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                        + "Connection: Upgrade" + Environment.NewLine
                        + "Upgrade: websocket" + Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            System.Security.Cryptography.SHA1.Create().ComputeHash(
                                        Encoding.UTF8.GetBytes(
                                            new Regex("Sec-WebSocket-Key: (.*)").Match(inputData).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")))
                                        + Environment.NewLine + Environment.NewLine));
                return true;
            }
            return false;
        }

        private byte[] Read()
        {
            byte[] buf = new byte[10000000];
            int readed = tcpClient.GetStream().Read(buf, 0, buf.Length);
            byte[] inputData = new byte[readed];
            for (long i = 0; i < readed; i++)
                inputData[i] = buf[i];
            return inputData;
        }

        private void Send(byte[] outputData)
        {
            tcpClient.GetStream().Write(outputData, 0, outputData.Length);
        }

        private static string DecodeMessage(byte[] bytes)
        {
            string incomingData = null;
            byte secondByte = bytes[1];
            int dataLength = secondByte & 127;
            int indexFirstMask = 2;
            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;

            IEnumerable<byte> keys = bytes.Skip(indexFirstMask).Take(4);
            int indexFirstDataByte = indexFirstMask + 4;

            byte[] decoded = new byte[bytes.Length - indexFirstDataByte];
            for (int i = indexFirstDataByte, j = 0; i < bytes.Length; i++, j++)
            {
                decoded[j] = (byte)(bytes[i] ^ keys.ElementAt(j % 4));
            }

            return incomingData = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
        }

        private static byte[] EncodeMessageToSend(string message)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.UTF8.GetBytes(message);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)129;
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }
    }
}
