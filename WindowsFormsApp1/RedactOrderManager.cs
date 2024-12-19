using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class RedactOrderManager : Form
    {
        BD sql_BD = new BD();
        int orderID;

        public RedactOrderManager(int id)
        {
            InitializeComponent();
            orderID = id;
            sql_BD.openConnect();

            // Запрос данных для редактирования
            sql_BD.command.CommandText =
                @"SELECT r.startDate, ht.techTypeName, htm.modelName,
                         r.problemDescriptionID, s.statusName AS requestStatus,
                         r.completionDate, r.repairParts,
                         u_master.fio AS masterFIO, u_client.fio AS clientFIO,
                         c.message
                  FROM Requests r
                  LEFT JOIN HomeTechModels htm ON r.modelID = htm.modelID
                  LEFT JOIN HomeTechTypes ht ON htm.techTypeID = ht.techTypeID
                  LEFT JOIN RequestStatuses s ON r.requestStatusID = s.statusID
                  LEFT JOIN Comments c ON r.requestID = c.requestID
                  LEFT JOIN Users u_master ON r.masterID = u_master.userID
                  LEFT JOIN Users u_client ON r.clientID = u_client.userID
                  WHERE r.requestID = @requestID";
            sql_BD.command.Parameters.Clear();
            sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                if (reader.Read())
                {
                    dateTimePicker1.Value = reader.GetDateTime(reader.GetOrdinal("startDate"));
                    comboBox1.Text = reader.IsDBNull(reader.GetOrdinal("techTypeName"))
                        ? "" : reader.GetString(reader.GetOrdinal("techTypeName"));
                    comboBox2.Text = reader.IsDBNull(reader.GetOrdinal("modelName"))
                        ? "" : reader.GetString(reader.GetOrdinal("modelName"));

                    // Заполняем problemDescriptionID
                    richTextBox1.Text = reader.IsDBNull(reader.GetOrdinal("problemDescriptionID"))
                        ? "" : reader.GetValue(reader.GetOrdinal("problemDescriptionID")).ToString();

                    comboBox3.Text = reader.IsDBNull(reader.GetOrdinal("requestStatus"))
                        ? "" : reader.GetString(reader.GetOrdinal("requestStatus"));
                    checkBox1.Checked = !reader.IsDBNull(reader.GetOrdinal("completionDate"));
                    if (checkBox1.Checked)
                        dateTimePicker2.Value = reader.GetDateTime(reader.GetOrdinal("completionDate"));
                    else
                        dateTimePicker2.CustomFormat = " ";

                    richTextBox2.Text = reader.IsDBNull(reader.GetOrdinal("repairParts"))
                        ? "" : reader.GetString(reader.GetOrdinal("repairParts"));
                    richTextBox3.Text = reader.IsDBNull(reader.GetOrdinal("message"))
                        ? "" : reader.GetString(reader.GetOrdinal("message"));
                    comboBox4.Text = reader.IsDBNull(reader.GetOrdinal("masterFIO"))
                        ? "" : reader.GetString(reader.GetOrdinal("masterFIO"));
                    comboBox5.Text = reader.IsDBNull(reader.GetOrdinal("clientFIO"))
                        ? "" : reader.GetString(reader.GetOrdinal("clientFIO"));
                }
            }

            // Заполнение типов техники
            FillComboBox(comboBox1, "SELECT techTypeName FROM HomeTechTypes");

            // Заполнение статусов
            FillComboBox(comboBox3, "SELECT statusName FROM RequestStatuses");

            // Заполнение мастеров
            FillComboBox(comboBox4, "SELECT fio FROM Users WHERE typeID = 2");

            // Заполнение клиентов
            FillComboBox(comboBox5, "SELECT fio FROM Users WHERE typeID = 1");

            sql_BD.closeConnect();
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "yyyy-MM-dd";
        }

        private void FillComboBox(ComboBox comboBox, string query)
        {
            comboBox.Items.Clear();
            sql_BD.command.CommandText = query;
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBox.Items.Add(reader.GetString(0));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sql_BD.openConnect();

                // Получаем userID клиента по его ФИО
                sql_BD.command.Parameters.Clear();
                sql_BD.command.CommandText = "SELECT userID FROM Users WHERE fio = @fio";
                sql_BD.command.Parameters.AddWithValue("@fio", comboBox5.Text);
                object clientIDObj = sql_BD.command.ExecuteScalar();
                int clientID = clientIDObj == null ? 0 : Convert.ToInt32(clientIDObj);

                // Получаем userID мастера по его ФИО
                sql_BD.command.Parameters.Clear();
                sql_BD.command.CommandText = "SELECT userID FROM Users WHERE fio = @fio";
                sql_BD.command.Parameters.AddWithValue("@fio", comboBox4.Text);
                object masterIDObj = sql_BD.command.ExecuteScalar();
                int masterID = masterIDObj == null ? 0 : Convert.ToInt32(masterIDObj);

                // Проверка значения поля "Дата завершения"
                DateTime? completionDate = checkBox1.Checked ? dateTimePicker2.Value : (DateTime?)null;

                // Обработка значения problemDescriptionID
                string problemDescriptionID = string.IsNullOrWhiteSpace(richTextBox1.Text)
                    ? "0" // Значение по умолчанию
                    : richTextBox1.Text;

                // Проверка типа данных для problemDescriptionID
                if (!int.TryParse(problemDescriptionID, out int parsedProblemDescriptionID))
                {
                    MessageBox.Show("Значение для problemDescriptionID должно быть числом.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Выполняем обновление данных
                sql_BD.command.Parameters.Clear();
                sql_BD.command.CommandText =
                    @"UPDATE Requests 
              SET completionDate = @completionDate, 
                  masterID = @masterID, 
                  problemDescriptionID = @problemDescriptionID
              WHERE requestID = @requestID";

                sql_BD.command.Parameters.AddWithValue("@completionDate", (object)completionDate ?? DBNull.Value);
                sql_BD.command.Parameters.AddWithValue("@masterID", masterID);
                sql_BD.command.Parameters.AddWithValue("@problemDescriptionID", parsedProblemDescriptionID);
                sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

                sql_BD.command.ExecuteNonQuery();

                MessageBox.Show("Запись изменена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                sql_BD.closeConnect();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sql_BD.openConnect();

            sql_BD.command.CommandText =
                @"SELECT modelName FROM HomeTechModels 
                  INNER JOIN HomeTechTypes ON HomeTechModels.techTypeID = HomeTechTypes.techTypeID 
                  WHERE techTypeName = @techTypeName";
            sql_BD.command.Parameters.Clear();
            sql_BD.command.Parameters.AddWithValue("@techTypeName", comboBox1.Text);

            comboBox2.Items.Clear();
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBox2.Items.Add(reader.GetString(0));
                }
            }

            sql_BD.closeConnect();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker2.Enabled = checkBox1.Checked;
            dateTimePicker2.CustomFormat = checkBox1.Checked ? "yyyy-MM-dd" : " ";
        }

        private void RedactOrderManager_Load(object sender, EventArgs e)
        {
            // Если необходима дополнительная логика загрузки формы
        }
    }
}
