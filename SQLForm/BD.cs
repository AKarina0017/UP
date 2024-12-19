using System;
using System.Data.SqlClient;

namespace SQLForm
{
    public class BD
    {
        public SqlConnection connection;
        public SqlCommand command;
        public BD()
        {
            connection = new SqlConnection("Data Source=ADCLG1;Initial Catalog=Thang;Integrated Security=True;");
            command = new SqlCommand("", connection);
        }
        public void openConnect()
        {
            try
            {
                connection.Close();
                connection.Open();
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка подключения к базе данных. Исключение:{ex}");
            }
        }
        public void closeConnect()
        {
            connection.Close();
        }
        public bool RequestIsExist(int ID)
        {
            this.openConnect();
            command.CommandText = $"SELECT * FROM [request] WHERE [requestID] = {ID}";
            int count = Convert.ToInt32(command.ExecuteScalar());
            this.closeConnect();
            if (count != 0) return true;
            else return false;
        }
        public bool UserIsExist(string login, string password)
        {
            this.openConnect();
            command.CommandText = $"SELECT [userTypeID] FROM [users] WHERE [login] = '{login}' AND [password] = '{password}'";
            int count = Convert.ToInt32(command.ExecuteScalar());
            this.closeConnect();
            if (count != 0) return true;
            else return false;
        }
        public int RequestFindResultCount(string searchTerm)
        {
            openConnect();
            command.CommandText = "SELECT COUNT(*) AS TotalCount " +
                "FROM [request] r " +
                "LEFT JOIN techModel t ON r.techModelID = t.computerTechID " +
                "LEFT JOIN requestStatus s ON r.requestStatus = s.requestStatusID " +
                "LEFT JOIN comment c ON r.requestID = c.requestID " +
                "LEFT JOIN users u_master ON r.masterID = u_master.userID " +
                "LEFT JOIN users u_client ON r.clientID = u_client.userID " +
                "WHERE (r.requestID LIKE '%' + @searchTerm + '%' OR " +
                "t.computerTechType LIKE '%' + @searchTerm + '%' OR " +
                "t.computerTechModel LIKE '%' + @searchTerm + '%' OR " +
                "r.problemDescryption LIKE '%' + @searchTerm + '%' OR " +
                "u_master.fio LIKE '%' + @searchTerm + '%' OR " +
                "u_client.fio LIKE '%' + @searchTerm + '%' OR " +
                "c.message LIKE '%' + @searchTerm + '%')";
            command.Parameters.AddWithValue("@searchTerm", searchTerm.Replace("'", "''"));
            return Convert.ToInt32(command.ExecuteScalar());
        }
        
        public void GetFioRole(int ID, out string fio, out string role)
        {
            openConnect();
            command.CommandText = $"SELECT * FROM [users] WHERE [userID] = {ID}";
            int userTypeID;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                reader.Read();
                fio = reader.GetString(reader.GetOrdinal("fio"));
                userTypeID = reader.GetInt32(reader.GetOrdinal("userTypeID"));
            }
            command.CommandText = $"SELECT [userType] FROM [usersType] WHERE [userTypeID] = {userTypeID}";
            role = command.ExecuteScalar().ToString();
        }
        public int GetUserTypeID(int ID)
        {
            openConnect();
            command.CommandText = $"SELECT [userTypeID] FROM [users] WHERE [userID] ={ID}";
             return Convert.ToInt32(command.ExecuteScalar());
        }
    }

}
