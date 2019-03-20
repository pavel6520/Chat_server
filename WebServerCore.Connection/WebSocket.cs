using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebServerCore.Connection {
    public sealed class WebSocket {
        private ConnectionClass cc;
        private Http.HttpContext context;

        public System.Net.CookieCollection Cookie { get { return context.Request.Cookie; } }
        public string HostName { get { return context.Request.HostName; } }
        public string Path { get { return context.Request.Uri; } }
        public string UserAddress { get { return cc.UserAddress; } }

        public WebSocket(Http.HttpContext context) {
            this.context = context;
            if (!context.Request.IsWebSocket) {
                throw new FormatException("Подключение не содержит запрос для включения WebSocket");
            }
            if (context.Request.Headers.GetValues("Sec-WebSocket-Version")[0] == "13") {
                context.Response.StatusCode = 101;
                context.Response.StatusDescription = "Switching Protocols";
                context.Response.Headers.Add("Upgrade", "websocket");
                context.Response.Headers.Add("Connection", "Upgrade");
                context.Response.Headers.Add("Sec-WebSocket-Accept", Convert.ToBase64String(System.Security.Cryptography.SHA1.Create().ComputeHash(
                    Encoding.UTF8.GetBytes($"{context.Request.Headers.GetValues("Sec-WebSocket-Key")[0]}258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))));
                context.Response.Close();
                cc = context.GetConnection();
            }
        }

        public int ReadText(ref byte[] b) {
            List<byte> readBuf = new List<byte>();
            byte opcode = 0;

            while (true) {
                Frame frame = new Frame(cc, opcode);
                ulong len = frame.Length_64;
                if (frame.Code_4 == 1 || frame.Code_4 == 2 || (frame.Code_4 == 0 && (opcode == 1 || opcode == 2))) { //текст/двоичные данные
                    while(len > 0) {
                        if (len > 8191) {
                            readBuf.AddRange(cc.Read(8191));
                            len -= 8191;
                        }
                        else {
                            readBuf.AddRange(cc.Read((int)len));
                            len = 0;
                        }
                    }
                    if (frame.Fin_1) {
                        b = readBuf.ToArray();
                        ApplyMask(ref b, frame.Mask_32);
                        return 0;
                    }
                }
                else if (frame.Code_4 == 8) { //Close
                    b = null;
                    return 1;
                }
                else if (frame.Code_4 == 9) { //PING
                }
                else if (frame.Code_4 == 10) { //PONG
                    b = null;
                    return 2;
                }
            }
        }

        public void WriteText(byte[] b) {
            Frame.Write(cc, true, 1, false, (uint)b.Length, null);
            cc.Write(b);
        }

        private void ApplyMask(ref byte[] b, byte[] mask) {
            for (long i = 0; i < b.LongLength; i++) {
                b[i] = (byte)(b[i] ^ mask[i % 4]);
            }
        }

        internal class Frame {
            public bool Fin_1;
            //public bool RSV1_1, RSV2_1, RSV3_1;
            public byte Code_4;
            public bool MaskBool;
            public byte[] Mask_32;
            public ulong Length_64;

            public Frame(ConnectionClass cc, byte codeOne = 0) {
                byte? byteBuf = cc.ReadByte();
                if (byteBuf == null) {
                    throw new NullReferenceException("Нет данных для чтения");
                }
                Fin_1 = byteBuf >> 7 == 1;
                Code_4 = Convert.ToByte(byteBuf % 16);
                byteBuf = cc.ReadByte();
                MaskBool = byteBuf >> 7 == 1;
                Length_64 = (uint)byteBuf % 128;
                byte[] buf = null;
                if (Length_64 == 126) {
                    buf = cc.Read(2);
                    Length_64 = ((uint)buf[0] << 8) + buf[1];
                }
                else if (Length_64 == 127) {
                    buf = cc.Read(8);
                    Length_64 = ((((((((((((((ulong)buf[0] << 8) + buf[1]) << 8) + buf[2]) << 8) + buf[3]) << 8) +buf[4]) << 8) +buf[5]) << 8) +buf[6]) << 8) +buf[7];
                }
                if (MaskBool) {
                    Mask_32 = cc.Read(4);
                }
                if (!Fin_1 && codeOne > 0) {
                    Code_4 = codeOne;
                }
            }

            public static void Write(ConnectionClass cc, bool Fin, byte Code, bool MaskBool, ulong Length, byte[] Mask) {
                byte b1 = (byte)((Fin ? 128 : 0) + Code);
                cc.WriteByte(b1);
                byte b2;
                byte[] b_len = null;
                if (Length >= 65536) {
                    b2 = (byte)((MaskBool ? 128 : 0) + 127);
                    b_len = new byte[4];
                    b_len[0] = (byte)((Length >> 56) & 255);
                    b_len[1] = (byte)((Length >> 48) & 255);
                    b_len[2] = (byte)((Length >> 40) & 255);
                    b_len[3] = (byte)((Length >> 32) & 255);
                    b_len[4] = (byte)((Length >> 24) & 255);
                    b_len[5] = (byte)((Length >> 16) & 255);
                    b_len[6] = (byte)((Length >> 8) & 255);
                    b_len[7] = (byte)(Length & 255);
                }
                else if(Length >= 126) {
                    b2 = (byte)((MaskBool ? 128 : 0) + 126);
                    b_len = new byte[4];
                    b_len[6] = (byte)((Length >> 8) & 255);
                    b_len[7] = (byte)(Length & 255);
                }
                else {
                    b2 = (byte)((MaskBool ? 128 : 0) + (byte)Length);
                }
                cc.WriteByte(b2);
                if (Length >= 126) {
                    cc.Write(b_len);
                }
                if (MaskBool) {
                    cc.Write(Mask);
                }
            }
        }
    }
}
