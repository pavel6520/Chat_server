using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore {
    static class DBClient
    {
        private static MySqlConnection connO;
        public static bool Connection { get; private set; }

        public static void Create()
        {
            //string StrConnect = $"server={Config.mysql.host};port={Config.mysql.port};user={Config.mysql.login};password={Config.mysql.pass};database={Config.mysql.db};";
            //connO = new MySqlConnection(StrConnect);
        }

        public static bool Check()
        {
            MySqlConnection conn = (MySqlConnection)connO.Clone();
            try
            {
                conn.Open();
                Log.Write(LogType.INFO, "DBClient", $"Connect successful! MySql Server {conn.ServerVersion}");

                //MySqlCommand command = new MySqlCommand("select * from test where tet = @p", conn);
                //command.Parameters.AddWithValue("@p", "test");
                //command.Prepare();
                //command.ExecuteReader();
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(LogType.FATAL, "DBClient", $"Failed to connect with an error: {ex.Message}");
                return false;
            }
        }

        public static MySqlDataReader ExecuteReader(MySqlCommand comm)
        {
            if (comm != null)
            {
                MySqlConnection conn = (MySqlConnection)connO.Clone();
                conn.Open();
                comm.Connection = conn;
                if (!comm.IsPrepared) comm.Prepare();
                return comm.ExecuteReader();
            }
            else
            {
                return null;
            }
        }

        public static int ExecuteNonQuery(MySqlCommand comm)
        {
            if (comm != null)
            {
                MySqlConnection conn = (MySqlConnection)connO.Clone();
                conn.Open();
                comm.Connection = conn;
                if (!comm.IsPrepared) comm.Prepare();
                return comm.ExecuteNonQuery();
            }
            else
            {
                return -1;
            }
        }

        public static object ExecuteScalar(MySqlCommand comm)
        {
            if (comm != null)
            {
                MySqlConnection conn = (MySqlConnection)connO.Clone();
                conn.Open();
                comm.Connection = conn;
                if (!comm.IsPrepared) comm.Prepare();
                return comm.ExecuteScalar();
            }
            else
            {
                return null;
            }
        }
    }
}
