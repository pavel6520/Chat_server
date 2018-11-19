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
        private object locker;
        private List<string> errorRequest;

        public MySql_Client(string address, int port, string login, string pass)
        {
            connStr = $"server={address};port={port};user={login};password={pass};database=chat_db;";
            locker = new object();
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
                FileStream f = File.Open("mysqlLog.log", FileMode.OpenOrCreate, FileAccess.Write);
                byte[] buf = Encoding.UTF8.GetBytes("\n" + DateTime.Now + "Mysql Start\n");
                f.Write(buf, 0, buf.Length);
                while (true)
                {
                    Thread.Sleep(1000);
                    while (errorRequest.Count > 0)
                    {
                        buf = Encoding.UTF8.GetBytes(errorRequest[0]);
                        f.Write(buf, 0, buf.Length);
                    }
                }
            });
        }

        public bool CheckUser(string login, string pass)
        {
            string request = $"SELECT EXISTS (SELECT * FROM `user` WHERE binary login = '{login}' and binary pass = '{pass}')";
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                bool result;
                MySqlCommand com = new MySqlCommand(request, conn);
                if ((long)com.ExecuteScalar() == 1) result = true;
                else result = false;
                conn.Close();
                return result;
            }
            catch (MySqlException ex)
            {
                errorRequest.Add(request + "\n" + ex.Message);
                return false;
            }
        }

        public bool RegNewUser(string login, string pass, string email)
        {
            lock (locker)
            {
                string request = $"INSERT INTO user (login, pass, email, status) VALUES ('{login}', '{pass}', '{email}', '00')";
                try
                {
                    MySqlConnection conn = new MySqlConnection(connStr);
                    conn.Open();
                    bool result;
                    MySqlCommand com = new MySqlCommand(request, conn);
                    result = ((long)com.ExecuteNonQuery() == 1);
                    conn.Close();
                    return result;
                }
                catch (MySqlException ex)
                {
                    errorRequest.Add(request + "\n" + ex.Message);
                    return false;
                }
            }
        }
    }
}
