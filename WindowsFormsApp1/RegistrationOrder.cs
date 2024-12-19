using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class RegistrationOrder : Form
    {
        BD sql_BD = new BD();
        int orderID;

        public RegistrationOrder(int id)
        {
            InitializeComponent();
            orderID = id;
            sql_BD.openConnect();

            // Запрос данных для заполнения формы
            sql_BD.command.CommandText = @"
                SELECT 
                    r.startDate, 
                    ht.techTypeName, 
                    hm.modelName, 
                    p.description AS problemDescription, 
                    rs.statusName AS requestStatus, 
                    r.completionDate, 
                    r.repairParts, 
                    u_master.fio AS masterFIO, 
                    u_client.fio AS clientFIO, 
                    c.message 
                FROM 
                    Requests r 
                LEFT JOIN 
                    HomeTechModels hm ON r.modelID = hm.modelID 
                LEFT JOIN 
                    HomeTechTypes ht ON hm.techTypeID = ht.techTypeID 
                LEFT JOIN 
                    Problems p ON r.problemDescriptionID = p.problemID 
                LEFT JOIN 
                    RequestStatuses rs ON r.requestStatusID = rs.statusID 
                LEFT JOIN 
                    Comments c ON r.requestID = c.requestID 
                LEFT JOIN 
                    Users u_master ON r.masterID = u_master.userID 
                LEFT JOIN 
                    Users u_client ON r.clientID = u_client.userID 
                WHERE 
                    r.requestID = @requestID";

            sql_BD.command.Parameters.Clear();
            sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                if (reader.Read())
                {
                    dateTimePicker1.Value = reader.GetDateTime(reader.GetOrdinal("startDate"));
                    comboBox1.Text = reader.GetString(reader.GetOrdinal("techTypeName"));
                    comboBox2.Text = reader.GetString(reader.GetOrdinal("modelName"));
                    richTextBox1.Text = reader.GetString(reader.GetOrdinal("problemDescription"));
                    comboBox3.Text = reader.GetString(reader.GetOrdinal("requestStatus"));

                    if (!reader.IsDBNull(reader.GetOrdinal("completionDate")))
                    {
                        checkBox1.Checked = true;
                        dateTimePicker2.Value = reader.GetDateTime(reader.GetOrdinal("completionDate"));
                    }
                    else
                    {
                        checkBox1.Checked = false;
                        dateTimePicker2.CustomFormat = " ";
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("repairParts")))
                        richTextBox2.Text = reader.GetString(reader.GetOrdinal("repairParts"));

                    if (!reader.IsDBNull(reader.GetOrdinal("message")))
                        richTextBox3.Text = reader.GetString(reader.GetOrdinal("message"));

                    if (!reader.IsDBNull(reader.GetOrdinal("masterFIO")))
                        comboBox4.Text = reader.GetString(reader.GetOrdinal("masterFIO"));

                    comboBox5.Text = reader.GetString(reader.GetOrdinal("clientFIO"));
                }
            }

            // Заполнение значений в comboBox
            FillComboBoxes();
        }

        private void FillComboBoxes()
        {
            // Заполнение типов техники
            comboBox1.Items.Clear();
            sql_BD.command.CommandText = "SELECT techTypeName FROM HomeTechTypes";
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                    comboBox1.Items.Add(reader.GetString(0));
            }

            // Заполнение статусов заказа
            comboBox3.Items.Clear();
            sql_BD.command.CommandText = "SELECT statusName FROM RequestStatuses";
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                    comboBox3.Items.Add(reader.GetString(0));
            }

            // Заполнение списка мастеров
            comboBox4.Items.Clear();
            sql_BD.command.CommandText = "SELECT fio FROM Users WHERE typeID = 2";
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                    comboBox4.Items.Add(reader.GetString(0));
            }

            // Заполнение списка клиентов
            comboBox5.Items.Clear();
            sql_BD.command.CommandText = "SELECT fio FROM Users WHERE typeID = 1";
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                    comboBox5.Items.Add(reader.GetString(0));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sql_BD.openConnect();

                // Переменные для хранения данных
                int modelID = 0;
                int statusID = 0;
                int problemDescriptionID = 0;
                object masterID = null; // Мастер может быть null
                int clientID = 0;

                // Получение ID модели
                sql_BD.command.CommandText = @"
                    SELECT TOP 1 modelID 
                    FROM HomeTechModels 
                    WHERE modelName = @modelName";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@modelName", comboBox2.Text);
                modelID = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                // Получение ID статуса
                sql_BD.command.CommandText = @"
                    SELECT TOP 1 statusID 
                    FROM RequestStatuses 
                    WHERE statusName = @statusName";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@statusName", comboBox3.Text);
                statusID = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                // Получение ID мастера (может быть null)
                sql_BD.command.CommandText = @"
                    SELECT TOP 1 userID 
                    FROM Users 
                    WHERE fio = @fio";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@fio", comboBox4.Text); // Мастер
                masterID = sql_BD.command.ExecuteScalar();

                // Получение ID клиента
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@fio", comboBox5.Text); // Клиент
                clientID = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                // Получение ID проблемы
                sql_BD.command.CommandText = @"
                    SELECT TOP 1 problemID 
                    FROM Problems 
                    WHERE description = @description";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@description", richTextBox1.Text);
                problemDescriptionID = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                // Форматирование необязательных полей
                string repairParts = string.IsNullOrEmpty(richTextBox2.Text) ? "NULL" : $"'{richTextBox2.Text}'";
                string completionDate = checkBox1.Checked ? dateTimePicker2.Value.ToString("yyyy-MM-dd") : "NULL";

                // Обновление заявки
                sql_BD.command.CommandText = @"
                    UPDATE Requests 
                    SET 
                        startDate = @startDate, 
                        modelID = @modelID, 
                        problemDescriptionID = @problemDescriptionID, 
                        requestStatusID = @requestStatusID, 
                        completionDate = @completionDate, 
                        repairParts = @repairParts, 
                        masterID = @masterID, 
                        clientID = @clientID 
                    WHERE 
                        requestID = @requestID";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@startDate", dateTimePicker1.Value);
                sql_BD.command.Parameters.AddWithValue("@modelID", modelID);
                sql_BD.command.Parameters.AddWithValue("@problemDescriptionID", problemDescriptionID);
                sql_BD.command.Parameters.AddWithValue("@requestStatusID", statusID);
                sql_BD.command.Parameters.AddWithValue("@completionDate", checkBox1.Checked ? (object)dateTimePicker2.Value : DBNull.Value);
                sql_BD.command.Parameters.AddWithValue("@repairParts", string.IsNullOrEmpty(richTextBox2.Text) ? DBNull.Value : (object)richTextBox2.Text);
                sql_BD.command.Parameters.AddWithValue("@masterID", masterID ?? DBNull.Value);
                sql_BD.command.Parameters.AddWithValue("@clientID", clientID);
                sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

                sql_BD.command.ExecuteNonQuery();

                // Обновление комментариев
                sql_BD.command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM Comments 
                    WHERE requestID = @requestID";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@requestID", orderID);
                int commentCount = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                if (commentCount > 0)
                {
                    if (string.IsNullOrEmpty(richTextBox3.Text))
                    {
                        sql_BD.command.CommandText = @"
                            DELETE FROM Comments 
                            WHERE requestID = @requestID";
                    }
                    else
                    {
                        sql_BD.command.CommandText = @"
                            UPDATE Comments 
                            SET message = @message 
                            WHERE requestID = @requestID";
                        sql_BD.command.Parameters.AddWithValue("@message", richTextBox3.Text);
                    }
                }
                else if (!string.IsNullOrEmpty(richTextBox3.Text))
                {
                    sql_BD.command.CommandText = @"
                        INSERT INTO Comments (message, masterID, requestID) 
                        VALUES (@message, @masterID, @requestID)";
                    sql_BD.command.Parameters.AddWithValue("@message", richTextBox3.Text);
                    sql_BD.command.Parameters.AddWithValue("@masterID", masterID ?? DBNull.Value);
                }

                sql_BD.command.ExecuteNonQuery();

                MessageBox.Show("Запись зарегистрирована", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                sql_BD.closeConnect();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных. Проверьте ввод.\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sql_BD.openConnect();
            sql_BD.command.CommandText = @"
                SELECT modelName 
                FROM HomeTechModels 
                WHERE techTypeID = 
                    (SELECT TOP 1 techTypeID FROM HomeTechTypes WHERE techTypeName = @techTypeName)";
            sql_BD.command.Parameters.Clear();
            sql_BD.command.Parameters.AddWithValue("@techTypeName", comboBox1.Text);

            comboBox2.Items.Clear();
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                    comboBox2.Items.Add(reader.GetString(0));
            }
            sql_BD.closeConnect();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker2.Enabled = checkBox1.Checked;
            dateTimePicker2.CustomFormat = checkBox1.Checked ? "yyyy-MM-dd" : " ";
        }

        private void RegistrationOrder_Load(object sender, EventArgs e)
        {
        }
    }
}
