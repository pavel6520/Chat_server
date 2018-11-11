using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Chat_server
{
    class MySql_Client
    {
        private string connStr;

        public MySql_Client(string address, int port, string login, string pass)
        {
            connStr = $"server={address};port={port};user={login};password={pass};database=chat_db;";
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

        public bool CheckUser(string login, string pass)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                bool result;
                MySqlCommand com = new MySqlCommand($"SELECT EXISTS (SELECT * FROM `user` WHERE binary login = '{login}' and binary pass = '{pass}')", conn);
                if ((long)com.ExecuteScalar() == 1) result = true;
                else result = false;
                conn.Close();
                return result;
            }
            catch (MySqlException)
            {
                return false;
            }
        }

        public bool RegNewUser(string login, string pass, string email)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                bool result;
                MySqlCommand com = new MySqlCommand($"INSERT INTO user (login, pass, email) VALUES ('{login}', '{pass}', '{email}')", conn);
                result = ((long)com.ExecuteNonQuery() == 1);
                conn.Close();
                return result;
            }
            catch (MySqlException)
            {
                return false;
            }
        }
    }
}
