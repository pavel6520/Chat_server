using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Chat_server
{
    class ClientClass
    {
        public static int Count { get; private set; }

        private TcpClient tcpClient;
        private TCP_Server tcpServer;
        public byte[] UniqueId { get; private set; }
        public string Login { get; private set; }
        public long Id { get; private set; }
        public ClientStatus Status { get; private set; }
        private object locker;

        //DEBUG
        public string Address { get; private set; }


        public ClientClass(TcpClient tcpClient, TCP_Server tcpServer)
        {
            this.tcpClient = tcpClient;
            this.tcpServer = tcpServer;
            UniqueId = Guid.NewGuid().ToByteArray();
            Login = null;
            Status = ClientStatus.Created;
            locker = new object();

            //DEBUG
            Address = tcpClient.Client.RemoteEndPoint.ToString();
        }

        public void Start()
        {
            Status = ClientStatus.Start;
            Task.Factory.StartNew(Process, TaskCreationOptions.LongRunning);
        }

        public void Close()
        {
            tcpClient.Close();
        }

        private void Process()
        {
            if (tcpClient.Connected)
            {
                if (Program.DEBUG) Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + tcpClient.Client.RemoteEndPoint + " подключился");
                //АВТОРИЗАЦИЯ
                Status = ClientStatus.Authorization;
                try
                {
                    if (Authorization()) Status = ClientStatus.LoggedIn;
                }
                catch (JsonReaderException)
                {
                    tcpServer.ClientClosed(this);
                    return;
                }
                if (Status == ClientStatus.LoggedIn)
                {
                    //if (Program.DEBUG)
                        //Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + tcpClient.Client.RemoteEndPoint + " авторизовался как " + Login);

                    Status = ClientStatus.LoadData;
                    //TODO: отправка данных
                    {
                        SendW(JsonConvert.SerializeObject(new CW("dat", JsonConvert.SerializeObject(new CW.Data
                        {
                            //id = Id,
                            login = Login,
                            online = tcpServer.Online.ToArray(),
                            history = Program.mySql.LoadMessage(DateTime.Now, null, null, 20)
                        }))));
                    }


                    Status = ClientStatus.Ready;

                    Count++;
                    Task.Factory.StartNew(() => tcpServer.ClientLogged(this));

                    while (true)
                    {
                        try
                        {
                            string inputData = ReadW();
                            if (inputData.Length == 2 && inputData[0] == 3 && inputData[1] == 65533)
                            {
                                Count--;
                                Status = ClientStatus.Stopped;
                                tcpServer.ClientClosed(this);
                                return;
                            }
                            else if (inputData.Length > 1500)
                            {
                                continue;
                                /*string json = JsonConvert.SerializeObject(new CW("err", "LEN"));*/
                            }
                            else
                            {
                                CW wrap = JsonConvert.DeserializeObject<CW>(inputData);
                                if (wrap.mestype == "mes")
                                {
                                    CW.MessageFromClient json = JsonConvert.DeserializeObject<CW.MessageFromClient>(wrap.body);
                                    if (json.message.Length > 0)
                                        Task.Factory.StartNew(() => tcpServer.Message(this, json.to, json.message));
                                    //if (Program.DEBUG)
                                        //Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + Login + ": " + json.message.Replace());
                                }
                            }
                        }
                        catch (System.IO.IOException)
                        {
                            Count--;
                            tcpServer.ClientClosed(this);
                            return;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Count--;
                            tcpServer.ClientClosed(this);
                            return;
                        }
                    }
                }
                else
                {
                    tcpServer.ClientClosed(this);
                }
            }
            else
            {
                tcpServer.ClientClosed(this);
            }

        }

        public void SendMessage(ClientClass sender, DateTime date, string message, string recipient = null)
        {
            string json = JsonConvert.SerializeObject(
                new CW("mes", JsonConvert.SerializeObject(
                    new CW.MessageToClient()
                    {
                        date = date,
                        from = sender.Login,
                        to = recipient,
                        message = message
                    })));
            SendW(json);
        }

        private bool Authorization()
        {
            string inputData = ReadHttp();
            if (inputData.Length > 15 && inputData.Substring(0, 14) == "GET /websocket"
                && Regex.IsMatch(inputData, "Connection: Upgrade")
                && Regex.IsMatch(inputData, "Upgrade: websocket"))
            {
                SendHttp("HTTP/1.1 101 Switching Protocols" + Environment.NewLine + "Connection: Upgrade" + Environment.NewLine
                    + "Upgrade: websocket" + Environment.NewLine + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(inputData).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")))
                                + Environment.NewLine + Environment.NewLine);
                while (true)
                {
                    inputData = ReadW();
                    CW wrap = JsonConvert.DeserializeObject<CW>(inputData);
                    if (wrap.mestype != null && wrap.body != null)
                    {
                        if (wrap.mestype == "log")
                        {
                            CW.Login loginJSON = JsonConvert.DeserializeObject<CW.Login>(wrap.body);
                            if (loginJSON.login != null && loginJSON.pass != null && loginJSON.login.Length >= 4 && loginJSON.pass.Length >= 4)
                            {
                                if (loginJSON.withpass)
                                {
                                    string[] res = Program.mySql.CheckUser(loginJSON.login, loginJSON.pass);
                                    if (res != null)
                                    {
                                        Id = Convert.ToInt64(res[0]);
                                        Login = res[1];
                                        SendW("LOGINED-SUCCSESS-ENTER-CHAT");
                                        return true;
                                    }
                                    else SendW("ERROR-LOGIN-PASSWORD");
                                }
                                else
                                {
                                    SendW("NOT-WORK-USE-WITHPASS");
                                }
                            }
                            else
                            {
                                SendW("ERROR-DATA-CONNECTION-CLOSING");
                                return false;
                            }
                        }
                        else if (wrap.mestype == "reg")
                        {
                            CW.Registration regJSON = JsonConvert.DeserializeObject<CW.Registration>(wrap.body);
                            if (regJSON.login != null && regJSON.pass != null && regJSON.email != null)
                                if (Regular.CheckLogin(regJSON.login))
                                    if (Regular.CheckPass(regJSON.pass))
                                        if (Regular.CheckEmail(regJSON.email))
                                        {
                                            Id = Program.mySql.RegNewUser(regJSON.login, regJSON.pass, regJSON.email);
                                            if (Id > 0)
                                            {
                                                Login = regJSON.login;
                                                SendW("REGISTR-SUCCSESS-ENTER-CHAT");
                                                return true;
                                            }
                                            else SendW("ERR-REGEXIST");
                                        }
                                        else SendW("ERR-REGEMAIL");
                                    else SendW("ERR-REGPASS");
                                else SendW("ERR-REGLOGIN");
                            else
                            {
                                SendW("ERROR-DATA-CONNECTION-CLOSING");
                                return false;
                            }
                        }
                        else
                        {
                            SendW("ERROR-DATA-CONNECTION-CLOSING");
                            return false;
                        }
                    }
                    else
                    {
                        SendW("ERROR-DATA-CONNECTION-CLOSING");
                        return false;
                    }
                }
            }
            else
            {
                //Console.WriteLine(DateTime.Now + " [DEBUG][TCP] " + tcpClient.Client.RemoteEndPoint + " прислал ЭТО " + inputData);
                SendHttp("ERROR-REQUEST expected /websocket");
            }
            return false;
        }

        private string ReadW()
        {
            byte[] buf = new byte[10000000];
            int readed = tcpClient.GetStream().Read(buf, 0, buf.Length);
            byte[] inputData = new byte[readed];
            for (long i = 0; i < readed; i++)
                inputData[i] = buf[i];
            byte secondByte = inputData[1];
            int dataLength = secondByte & 127;
            int indexFirstMask = 2;
            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;

            IEnumerable<byte> keys = inputData.Skip(indexFirstMask).Take(4);
            int indexFirstDataByte = indexFirstMask + 4;

            byte[] decoded = new byte[inputData.Length - indexFirstDataByte];
            for (int i = indexFirstDataByte, j = 0; i < inputData.Length; i++, j++)
            {
                decoded[j] = (byte)(inputData[i] ^ keys.ElementAt(j % 4));
            }

            return Encoding.UTF8.GetString(decoded, 0, decoded.Length);
        }
        private void SendW(string message)
        {
            byte[] outputData;
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
            outputData = new byte[indexStartRawData + length];
            int i, reponseIdx = 0;
            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                outputData[reponseIdx] = frame[i];
                reponseIdx++;
            }
            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                outputData[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }
            lock (locker)
                tcpClient.GetStream().Write(outputData, 0, outputData.Length);
        }

        private string ReadHttp()
        {
            byte[] buf = new byte[10000000];
            int readed = tcpClient.GetStream().Read(buf, 0, buf.Length);
            byte[] inputData = new byte[readed];
            for (long i = 0; i < readed; i++)
                inputData[i] = buf[i];
            return Encoding.UTF8.GetString(inputData);
        }
        private void SendHttp(string outputData)
        {
            byte[] outputB = Encoding.UTF8.GetBytes(outputData);
            tcpClient.GetStream().Write(outputB, 0, outputB.Length);
        }

        public enum ClientStatus
        {
            Created,
            Start,
            Authorization,
            LoggedIn,
            LoadData,
            Ready,
            Stopped
        }

        public class CW
        {
            public string mestype { get; set; }
            public string body { get; set; }

            public CW(string mestype, string body)
            {
                this.mestype = mestype;
                this.body = body;
            }

            public class Login
            {
                public string login { get; set; }
                public bool withpass { get; set; }
                public string pass { get; set; }
                public string key { get; set; }
            }

            public class Registration
            {
                public string login { get; set; }
                public string pass { get; set; }
                public string email { get; set; }
            }

            public class MessageFromClient
            {
                public string to { get; set; }
                public string message { get; set; }
            }

            public class MessageToClient
            {
                public DateTime date { get; set; }
                public string from { get; set; }
                public string to { get; set; }
                public string message { get; set; }
            }

            public class Data
            {
                //public long id { get; set; }
                public string login { get; set; }
                public string[] online { get; set; }
                public MessageToClient[] history { get; set; }
            }
        }
    }
}
