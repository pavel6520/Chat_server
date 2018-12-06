using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Chat_server
{
    class MySql_Client
    {
        private string connStr;
        private object lockerReg;
        private object lockerDial;
        private object lockerAddMes;
        private List<string> errorRequest;

        public MySql_Client(string address, int port, string login, string pass)
        {
            connStr = $"server={address};port={port};user={login};password={pass};database=chat_db;";
            lockerReg = new object();
            lockerDial = new object();
            lockerAddMes = new object();
            errorRequest = new List<string>();
        }

        public bool CheckConnect()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                conn.Close();
            }
            catch { return false; }
            return true;
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    while (errorRequest.Count > 0)
                    {
                        File.AppendAllText("mysqlLog.log", DateTime.Now + " " + errorRequest[0] + "\r\n");
                        errorRequest.RemoveAt(0);
                    }
                }
            });
        }

        public long UserExist(string login, string email = null)
        {
            string request = $"SELECT id FROM user WHERE login = '{login}'{(email == null ? "" : $" and email = '{email}'")}";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                MySqlCommand com = new MySqlCommand(request, conn);
                try
                {
                    return Convert.ToInt64(com.ExecuteScalar());
                }
                catch { return 0; }
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + " " + ex.Message);
                return -1;
            }
        }

        public string[] CheckUser(string login, string pass)
        {
            string request = $"SELECT id, login FROM user WHERE login = '{login}' and binary pass = '{pass}'";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                string[] res = null;
                MySqlCommand com = new MySqlCommand(request, conn);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.Read())
                    res = new string[] { reader[0].ToString(), reader[1].ToString() };
                conn.Close();
                return res;
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + " " + ex.Message);
                return null;
            }
        }

        public long RegNewUser(string login, string pass, string email)
        {
            email = email.ToLower();
            if (UserExist(login, email) == 0)
            {
                string request = $"INSERT INTO user (login, pass, email) VALUES ('{login}', '{pass}', '{email}');";
                try
                {
                    MySqlConnection conn = new MySqlConnection(connStr);
                    conn.Open();
                    MySqlCommand com = new MySqlCommand(request, conn);
                    lock (lockerReg) com.ExecuteScalar();
                    return com.LastInsertedId;
                }
                catch (MySqlException ex)
                {
                    errorRequest.Add(request + " " + ex.Message);
                    return -1;
                }
            }
            else return 0;
        }

        public long AddMessage(DateTime time, long senderID, string message, string recipient)
        {
            string request = "";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                MySqlCommand com;

                long recipientId = UserExist(recipient);

                if (recipientId <= 0) return -2;
                request = $"SELECT d.id from dialog d WHERE (d.user_from = '{senderID}' AND d.user_to = '{recipientId}')";
                com = new MySqlCommand(request, conn);
                long dialogId = Convert.ToInt64(com.ExecuteScalar());
                if (dialogId <= 0)
                {
                    request = $"INSERT INTO dialog (dialog.user_from, dialog.user_to) VALUES ('{senderID}', '{recipientId}')";
                    com = new MySqlCommand(request, conn);
                    com.ExecuteNonQuery();
                    dialogId = com.LastInsertedId;
                }

                request = "INSERT INTO message (message.dialog_id, message.date, message.message)" +
                    $"VALUES ('{dialogId}', '{time.ToString("yyyy.MM.dd HH:mm:ss")}', '{message}')";
                com = new MySqlCommand(request, conn);
                lock (lockerReg) com.ExecuteScalar();
                return com.LastInsertedId;
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + " " + ex.Message);
                return -1;
            }
        }

        public long AddMessageP(DateTime time, long senderID, string message)
        {
            string request = "";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                request = "INSERT INTO message_public (user_id, message, date)" +
                    $"VALUES ('{senderID}', '{message}', '{time.ToString("yyyy.MM.dd HH:mm:ss")}')";
                MySqlCommand com = new MySqlCommand(request, conn);
                lock (lockerAddMes) com.ExecuteScalar();
                return com.LastInsertedId;
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + " " + ex.Message);
                return -1;
            }
        }

        public ClientClass.CW.MessageToClient[] LoadMessagePublic(long userID, int count = 50)
        {
            string request = "";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                MySqlCommand com;
                List<ClientClass.CW.MessageToClient> result = new List<ClientClass.CW.MessageToClient>();

                request = $"SELECT m.id, u.login, m.date, m.message FROM message_public m, user u WHERE u.id = m.user_id ORDER BY m.id DESC LIMIT {count}";
                com = new MySqlCommand(request, conn);
                MySqlDataReader r = com.ExecuteReader();
                while (r.Read()) result.Add(new ClientClass.CW.MessageToClient()
                { id = Convert.ToInt64(r[0]), from = r[1].ToString(), to = "public", date = DateTime.Parse(r[2].ToString()), message = r[3].ToString() });
                r.Close();

                result.Sort();
                return result.ToArray();
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + " " + ex.Message);
                return null;
            }
        }

        public ClientClass.CW.MessageToClient[] LoadMessagePrivate(long userID, string recipient, int count = 50)
        {
            string request = "";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                
                long recipientId = UserExist(recipient);
                List<long> dialogsID = new List<long>();

                request = $"SELECT d.id FROM dialog d WHERE (d.user_from = '{userID}' AND d.user_to = '{recipientId}') " +
                    $"OR d.user_from = '{recipientId}' AND d.user_to = '{userID}'";

                MySqlCommand com = new MySqlCommand(request, conn);
                MySqlDataReader r = com.ExecuteReader();
                
                while (r.Read()) dialogsID.Add(Convert.ToInt64(r[0]));

                List<ClientClass.CW.MessageToClient> result = new List<ClientClass.CW.MessageToClient>();

                for (int i = 0; i < dialogsID.Count; i++)
                {
                    r.Close();
                    request = $"SELECT m.id, u1.login u_from, u2.login u_to, m.date, m.message FROM message m, dialog d, user u1, user u2 " +
                        $"where (d.user_from = u1.id and d.user_to = u2.id) and m.dialog_id = d.id and d.id = '{dialogsID[i]}' ORDER BY m.id DESC LIMIT {count}";
                    com = new MySqlCommand(request, conn);
                    r = com.ExecuteReader();
                    while (r.Read())
                        result.Add(new ClientClass.CW.MessageToClient()
                        { id = Convert.ToInt64(r[0]), from = r[1].ToString(), to = r[2].ToString(), date = DateTime.Parse(r[3].ToString()), message = r[4].ToString() });
                }
                result.Sort();
                return result.ToArray();
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + " " + ex.Message);
                return null;
            }
        }
    }
}
