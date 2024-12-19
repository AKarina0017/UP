using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal class BD
    {
        public SqlConnection connection;
        public SqlCommand command;

        public BD()
        {
            // Настройка подключения к вашей базе данных
            connection = new SqlConnection("Data Source=DESKTOP-DBK809R\\SQLEXPRESS;Initial Catalog=UpTech;Integrated Security=True;");
            command = new SqlCommand("", connection);
        }

        public void openConnect()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка доступа к базе данных. Исключение: {ex.Message}");
            }
        }

        public void closeConnect()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public bool UserIsExist(string login, string password)
        {
            openConnect();
            command.CommandText = @"
                SELECT COUNT(1) 
                FROM Users 
                WHERE login = @login AND password = @password";

            // Использование параметров для предотвращения SQL-инъекций
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);

            int count = Convert.ToInt32(command.ExecuteScalar());
            closeConnect();

            return count > 0;
        }

        public void AddOrder(string techTypeName, string modelName, string problemDescription, int clientID)
        {
            try
            {
                openConnect();

                command.CommandText = "INSERT INTO Problems (description) OUTPUT INSERTED.problemID VALUES (@description)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@description", problemDescription);
                int problemID = (int)command.ExecuteScalar();

                command.CommandText = @"
            SELECT modelID 
            FROM HomeTechModels 
            WHERE modelName = @modelName 
              AND techTypeID = (SELECT techTypeID FROM HomeTechTypes WHERE techTypeName = @techTypeName)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@modelName", modelName);
                command.Parameters.AddWithValue("@techTypeName", techTypeName);

                object modelIDObj = command.ExecuteScalar();
                if (modelIDObj == null)
                    throw new Exception("Указанная модель техники не найдена.");
                int modelID = (int)modelIDObj;

                command.CommandText = @"
            INSERT INTO Requests (startDate, modelID, problemDescriptionID, requestStatusID, clientID)
            VALUES (@startDate, @modelID, @problemID, @statusID, @clientID)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@startDate", DateTime.Now);
                command.Parameters.AddWithValue("@modelID", modelID);
                command.Parameters.AddWithValue("@problemID", problemID);
                command.Parameters.AddWithValue("@statusID", 1); 
                command.Parameters.AddWithValue("@clientID", clientID);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления заказа: {ex.Message}");
            }
            finally
            {
                closeConnect();
            }
        }


        public void GetFioRole(int ID, out string fio, out string role)
        {
            openConnect();

            try
            {
                // Получение данных пользователя
                command.CommandText = @"
                    SELECT fio, typeID 
                    FROM Users 
                    WHERE userID = @ID";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@ID", ID);

                int typeID;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        fio = reader.GetString(reader.GetOrdinal("fio"));
                        typeID = reader.GetInt32(reader.GetOrdinal("typeID"));
                    }
                    else
                    {
                        throw new Exception("Пользователь не найден.");
                    }
                }

                // Получение роли пользователя
                command.CommandText = @"
                    SELECT typeName 
                    FROM UserTypes 
                    WHERE typeID = @typeID";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@typeID", typeID);

                role = command.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения данных: {ex.Message}");
            }
            finally
            {
                closeConnect();
            }
        }
    }
}
