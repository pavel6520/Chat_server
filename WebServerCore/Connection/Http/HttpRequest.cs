﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Connection.Http {
    public sealed class HttpRequest {
        private readonly Connection cc;

        public string[] Accept { get; private set; }
        public string[] AcceptEncoding { get; private set; }
        public string Connection { get; private set; }
        public System.Net.CookieCollection Cookie { get; private set; }
        public DateTime Date { get; private set; }
        public string HostName { get; private set; }
        public string Method { get; private set; }
        public string Protocol { get; private set; }
        public string ProtocolVersion { get; private set; }
        public string Path { get; private set; }
        public string Upgrade { get; private set; }
        public string UserAddress { get; private set; }
        public string UserAgent { get; private set; }
        public string[] UserLanguage { get; private set; }

        public System.Collections.Specialized.NameValueCollection Headers { get; private set; }

        public bool isWebSocket { get; private set; }

        internal HttpRequest(Connection cc) {
            this.cc = cc;
            UserAddress = cc.UserAddress;
            string sBuf = cc.ReadLine();
            string[] buf = sBuf.Split(Const.SPLIT_SPACE, StringSplitOptions.RemoveEmptyEntries);
            if (buf.Length != 3)
                throw new FormatException($"Ожидался формат METHOD PATH PROTOCOL. Получена строка {sBuf}");
            Method = buf[0];
            Path = buf[1];
            buf = buf[2].Split(Const.SPLIT_SLASH, StringSplitOptions.RemoveEmptyEntries);
            Protocol = buf[0];
            ProtocolVersion = buf[1];
            Headers = new System.Collections.Specialized.NameValueCollection(1);
            Cookie = new System.Net.CookieCollection();
            while (true) {
                string read2 = cc.ReadLine();
                if (read2.Length <= 4)
                    break;
                int pos = read2.IndexOf(':');
                string read1 = read2.Substring(0, pos).ToLower();
                read2 = read2.Remove(0, pos + 2);
#if DEBUG
                Headers.Add(read1, read2);
#endif
                switch (read1) {
                    case "Accept":
                        Accept = read2.Split(Const.SPLIT_DOT, StringSplitOptions.RemoveEmptyEntries);
                        break;
                    case "Accept-Encoding":
                        AcceptEncoding = read2.Split(Const.SPLIT_DOTSPACE, StringSplitOptions.RemoveEmptyEntries);
                        break;
                    case "Accept-Language":
                        UserLanguage = read2.Split(Const.SPLIT_DOT, StringSplitOptions.RemoveEmptyEntries);
                        break;
                    case "Connection":
                        Connection = read2;
                        break;
                    case "Cookie":
                        buf = read2.Split(Const.SPLIT_SEMICOLON, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in buf) {
                            string[] itemSplit = item.Split(Const.SPLIT_EQUAL);
                            Cookie.Add(new System.Net.Cookie(itemSplit[0], itemSplit[1]));
                        }
                        break;
                    case "Date":
                        Date = DateTime.Parse(read2);
                        break;
                    case "Host":
                        HostName = read2;
                        break;
                    case "Upgrade":
                        Upgrade = read2;
                        break;
                    case "User-Agent":
                        UserAgent = read2;
                        break;
                    default:
#if DEBUG
#else
                        Headers.Add(read1, read2);
#endif
                        break;
                }
            }
            isWebSocket = Connection == "Upgrade" && Upgrade == "websocket";
        }

        public ConnectionRead GetReadStream() {
            return new ConnectionRead(cc);
        }
    }
}