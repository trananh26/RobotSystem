using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL.RobotSystem
{

    public class DLAGV
    {
        private static string ConnectionString = ConfigurationSettings.AppSettings["DatabaseConnection"];
        private static SqlConnection conn = new SqlConnection(ConnectionString);
        private static SqlCommand cmd = new SqlCommand();
        private static SqlDataAdapter da;


        public static DataTable GetDataTable(string stored)
        {
            DataTable dt = new DataTable();
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                da = new SqlDataAdapter(stored, conn);
                da.Fill(dt);
                conn.Close();
            }
            catch (Exception ee)
            {

            }
            return dt;
        }

        /// <summary>
        /// Cap nhat Alarm
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ID"></param>
        /// <param name="alarm"></param>
        public static void UpdateAGVAlarm(string query, string ID, string alarm)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Alarm", alarm);
                command.Parameters.AddWithValue("@ID", ID);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        /// <summary>
        /// Cập nhật lại dest AGV
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ID"></param>
        /// <param name="dest"></param>
        public static void UpdateAGVDest(string query, string ID, string dest)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Dest", dest);
                command.Parameters.AddWithValue("@ID", ID);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        /// <summary>
        /// Cập nhật vị trí AGV
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ID"></param>
        /// <param name="location"></param>
        public static void UpdateAGVLocation(string query, string ID, string location)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Location", location);
                command.Parameters.AddWithValue("@ID", ID);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái chạy
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ID"></param>
        /// <param name="runStop"></param>
        /// <param name="fullEmpty"></param>
        public static void UpdateAGVState(string query, string ID, string runStop, string fullEmpty)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RunState", runStop);
                command.Parameters.AddWithValue("@FullState", fullEmpty);
                command.Parameters.AddWithValue("@ID", ID);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        /// <summary>
        /// Cập nhật lại lệnh line out
        /// </summary>
        /// <param name="query"></param>
        /// <param name="State"></param>
        public static void UpdateOutputState(string query, string State)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@State", State);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        /// <summary>
        /// Update path mới cho AGV
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ID"></param>
        /// <param name="Path"></param>
        public static void UpdateAGVPath(string query, string ID, string Path)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Path", Path);
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.ExecuteNonQuery();
                conn.Close();

            }
            catch (Exception)
            {

            }
        }
    }
}
