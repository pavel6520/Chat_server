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
        private object lockerAddMes;
        private List<string> errorRequest;

        public MySql_Client(string address, int port, string login, string pass)
        {
            connStr = $"server={address};port={port};user={login};password={pass};database=chat_db;";
            lockerReg = new object();
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
                        File.AppendAllText("mysqlLog.log", DateTime.Now + errorRequest[0] + "\r\n");
                        errorRequest.RemoveAt(0);
                    }
                }
            });
        }

        public string CheckUser(string login)
        {
            string request = $"SELECT login FROM user WHERE login = '{login}'";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                string res = null;
                MySqlCommand com = new MySqlCommand(request, conn);
                MySqlDataReader reader = com.ExecuteReader();
                if (reader.Read())
                    res = reader[0].ToString();
                conn.Close();
                return res;
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + "\n" + ex.Message);
                return null;
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
                errorRequest.Add(request + "\n" + ex.Message);
                return null;
            }
        }

        public long RegNewUser(string login, string pass, string email)
        {
            string request = $"INSERT INTO user (login, pass, email, status) VALUES ('{login}', '{pass}', '{email}', '00');";
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
                errorRequest.Add(request + "\n" + ex.Message);
                return -1;
            }
        }

        public long AddMessage(DateTime time, string sender, string message, string recepient = null)
        {
            string request;
            if (recepient != null)
                request = $"INSERT INTO message (`from`, `to`, `message`, `date`) VALUES ('{sender}', '{recepient}', '{message}', '{time.ToString("yyyy.MM.dd HH:mm:ss")}')";
            else
                request = $"INSERT INTO message (`from`, `message`, `date`) VALUES ('{sender}', '{message}', '{time.ToString("yyyy.MM.dd HH:mm:ss")}')";
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
                errorRequest.Add(request + "\n" + ex.Message);
                return -1;
            }
        }

        public ClientClass.CW.MessageToClient[] LoadMessage(DateTime day, string to, string from = null, int count = 0)
        {
            if (to == null)
                return GetMes($"SELECT m.from, m.date, m.message FROM message m WHERE m.to is null order by m.id desc" +
                    $"{(count == 0 ? "" : $" limit {count}")}", true);
            else
                return GetMes("SELECT m.from, m.to, m.date, m.message FROM message m " +
                    $"WHERE m.from = 'test1' and m.to = 'test' order by m.id desc{(count == 0 ? "" : $" limit {count}")}", false);
        }

        private ClientClass.CW.MessageToClient[] GetMes(string request, bool mode)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                MySqlCommand com = new MySqlCommand(request, conn);
                lock (lockerReg)
                {
                    List<ClientClass.CW.MessageToClient> result = new List<ClientClass.CW.MessageToClient>();
                    MySqlDataReader r = com.ExecuteReader();
                    if (mode)
                        while (r.Read())
                            result.Add(new ClientClass.CW.MessageToClient()
                                { from = r[0].ToString(), to = null, date = DateTime.Parse(r[1].ToString()), message = r[2].ToString() });
                    else
                        while (r.Read())
                            result.Add(new ClientClass.CW.MessageToClient()
                            { from = r[0].ToString(), to = r[1].ToString(), date = DateTime.Parse(r[2].ToString()), message = r[3].ToString() });
                    return result.ToArray();
                }
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + "\n" + ex.Message);
                return null;
            }
        }
    }
}
