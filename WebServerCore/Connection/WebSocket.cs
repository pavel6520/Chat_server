using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebServerCore.Connection {
    class WebSocket {
        private Connection cc;

        internal WebSocket(Http.HttpContext context) {
            if (!context.Request.isWebSocket) {
                throw new Exception("Подключение не содержит запрос для включения WebSocket");//TODO
            }
            if (context.Request.Headers.GetValues("Sec-WebSocket-Version")[0] == "13") {
                context.Response.StatusCode = 101;
                context.Response.StatusDescription = "Switching Protocols";
                context.Response.Headers.Add("Connection", "Upgrade");
                context.Response.Headers.Add("Upgrade", "websocket");
                context.Response.Headers.Add("Sec-WebSocket-Accept", Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(
                    Encoding.UTF8.GetBytes($"{context.Request.Headers.GetValues("Sec-WebSocket-Key")[0]}258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))));
                context.Response.Close();
                cc = context.GetConnection();
            }
        }

        public byte[] Read(ref int code) {
            List<byte> readBuf = new List<byte>();
            int opcode = 0;

            while (true) {
                bool FIN = false;
                byte? byteBuf = cc.ReadByte();
                if (byteBuf == null) {
                    throw new NullReferenceException("Нет данных для чтения");
                }
                FIN = byteBuf >> 7 == 1;
                int opcodeBuf = (int)byteBuf % 16;
                byteBuf = cc.ReadByte();
                bool mask = byteBuf >> 7 == 1;
                ulong len = (uint)byteBuf % 128;
                byte[] buf = null;
                if(len == 126) {
                    buf = cc.Read(2);
                    len = ((uint)buf[0] << 8) + buf[1];
                }
                else if (len == 127) {
                    buf = cc.Read(4);
                    len = ((((((ulong)buf[0] << 8) + buf[1]) << 8) + buf[2]) << 8) + buf[3];
                }
                if (!FIN && opcodeBuf > 0) {
                    opcode = opcodeBuf;
                }

                if (opcodeBuf == 1 || opcodeBuf == 2) { //текст/двоичные данные
                    readBuf.AddRange(cc.Read());
                    code = 0;
                    if (FIN) {
                        return readBuf.ToArray();
                    }
                }
                else if (opcodeBuf == 8) { //Close
                    code = 1;
                    return null;
                }
                else if (opcodeBuf == 9) { //PING

                }
                else if (opcodeBuf == 10) { //PONG
                    code = 2;
                    return null;
                }
                else if (opcodeBuf == 0) {
                    if (opcode == 1 || opcode == 2) {
                        readBuf.AddRange(cc.Read());
                    }
                    if (FIN) {
                        return readBuf.ToArray();
                    }
                }
            }

            //{
            //    byte[] buf = new byte[10000];
            //    int readed = tcpClient.GetStream().Read(buf, 0, buf.Length);
            //    byte[] inputData = new byte[readed];
            //    for (long i = 0; i < readed; i++)
            //        inputData[i] = buf[i];
            //    byte secondByte = inputData[1];
            //    int dataLength = secondByte & 127;
            //    int indexFirstMask = 2;
            //    if (dataLength == 126)
            //        indexFirstMask = 4;
            //    else if (dataLength == 127)
            //        indexFirstMask = 10;

            //    IEnumerable<byte> keys = inputData.Skip(indexFirstMask).Take(4);
            //    int indexFirstDataByte = indexFirstMask + 4;

            //    byte[] decoded = new byte[inputData.Length - indexFirstDataByte];
            //    for (int i = indexFirstDataByte, j = 0; i < inputData.Length; i++, j++)
            //        decoded[j] = (byte)(inputData[i] ^ keys.ElementAt(j % 4));

            //    return Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            //}
        }

        public void Write(byte[] b) {



            //byte[] outputData;
            //byte[] bytesRaw = Encoding.UTF8.GetBytes(message);
            //byte[] frame = new byte[10];
            //int indexStartRawData = -1;
            //int length = bytesRaw.Length;
            //frame[0] = (byte)129;
            //if (length <= 125) {
            //    frame[1] = (byte)length;
            //    indexStartRawData = 2;
            //}
            //else if (length >= 126 && length <= 65535) {
            //    frame[1] = (byte)126;
            //    frame[2] = (byte)((length >> 8) & 255);
            //    frame[3] = (byte)(length & 255);
            //    indexStartRawData = 4;
            //}
            //else {
            //    frame[1] = (byte)127;
            //    frame[2] = (byte)((length >> 56) & 255);
            //    frame[3] = (byte)((length >> 48) & 255);
            //    frame[4] = (byte)((length >> 40) & 255);
            //    frame[5] = (byte)((length >> 32) & 255);
            //    frame[6] = (byte)((length >> 24) & 255);
            //    frame[7] = (byte)((length >> 16) & 255);
            //    frame[8] = (byte)((length >> 8) & 255);
            //    frame[9] = (byte)(length & 255);
            //    indexStartRawData = 10;
            //}
            //outputData = new byte[indexStartRawData + length];
            //int reponseIdx = 0;
            ////Add the frame bytes to the reponse
            //for (int i = 0; i < indexStartRawData; i++, reponseIdx++)
            //    outputData[reponseIdx] = frame[i];
            ////Add the data bytes to the response
            //for (int i = 0; i < length; i++, reponseIdx++)
            //    outputData[reponseIdx] = bytesRaw[i];
            //lock (locker)
            //    tcpClient.GetStream().Write(outputData, 0, outputData.Length);
        }
    }
}
