using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class RedactOrder : Form
    {
        int orderID;
        BD sql_BD = new BD();

        public RedactOrder(int id)
        {
            InitializeComponent();
            orderID = id;
            sql_BD.openConnect();

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            // Получение данных заказа
            sql_BD.command.CommandText = $"SELECT ht.techTypeName, htm.modelName, p.description " +
                $"FROM Requests r " +
                $"INNER JOIN HomeTechModels htm ON r.modelID = htm.modelID " +
                $"INNER JOIN HomeTechTypes ht ON htm.techTypeID = ht.techTypeID " +
                $"INNER JOIN Problems p ON r.problemDescriptionID = p.problemID " +
                $"WHERE r.requestID = @requestID";
            sql_BD.command.Parameters.Clear();
            sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBox1.Text = reader.GetString(reader.GetOrdinal("techTypeName"));
                    comboBox2.Text = reader.GetString(reader.GetOrdinal("modelName"));
                    richTextBox1.Text = reader.GetString(reader.GetOrdinal("description"));
                }
            }

            // Заполнение типов техники
            sql_BD.command.CommandText = "SELECT techTypeName FROM HomeTechTypes";
            sql_BD.command.Parameters.Clear();

            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                while (reader.Read())
                {
                    comboBox1.Items.Add(reader.GetString(reader.GetOrdinal("techTypeName")));
                }
            }

            sql_BD.closeConnect();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sql_BD.openConnect();

            // Загрузка моделей техники на основе выбранного типа
            sql_BD.command.CommandText = $"SELECT htm.modelName " +
                $"FROM HomeTechModels htm " +
                $"INNER JOIN HomeTechTypes ht ON htm.techTypeID = ht.techTypeID " +
                $"WHERE ht.techTypeName = @techTypeName";
            sql_BD.command.Parameters.Clear();
            sql_BD.command.Parameters.AddWithValue("@techTypeName", comboBox1.Text);

            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                comboBox2.Text = string.Empty;
                comboBox2.Items.Clear();
                while (reader.Read())
                {
                    comboBox2.Items.Add(reader.GetString(reader.GetOrdinal("modelName")));
                }
            }

            sql_BD.closeConnect();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(comboBox1.Text) || string.IsNullOrWhiteSpace(comboBox2.Text))
                {
                    throw new Exception("Тип и модель техники должны быть выбраны.");
                }

                if (string.IsNullOrWhiteSpace(richTextBox1.Text))
                {
                    richTextBox1.Text = "Без описания"; // Устанавливаем описание по умолчанию
                }

                sql_BD.openConnect();

                // Получение modelID для выбранной модели
                sql_BD.command.CommandText = $"SELECT htm.modelID " +
                    $"FROM HomeTechModels htm " +
                    $"INNER JOIN HomeTechTypes ht ON htm.techTypeID = ht.techTypeID " +
                    $"WHERE ht.techTypeName = @techTypeName AND htm.modelName = @modelName";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@techTypeName", comboBox1.Text);
                sql_BD.command.Parameters.AddWithValue("@modelName", comboBox2.Text);

                int modelID = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                // Проверяем, существует ли problemID
                sql_BD.command.CommandText = $"SELECT problemID FROM Problems WHERE description = @description";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@description", richTextBox1.Text);

                object problemIDObj = sql_BD.command.ExecuteScalar();
                int problemID;

                if (problemIDObj == null) // Если записи нет, добавляем новое описание
                {
                    sql_BD.command.CommandText = "INSERT INTO Problems (description) OUTPUT INSERTED.problemID VALUES (@description)";
                    sql_BD.command.Parameters.Clear();
                    sql_BD.command.Parameters.AddWithValue("@description", richTextBox1.Text);
                    problemID = Convert.ToInt32(sql_BD.command.ExecuteScalar());
                }
                else
                {
                    problemID = Convert.ToInt32(problemIDObj);
                }

                // Обновление заказа
                sql_BD.command.CommandText = $"UPDATE Requests " +
                    $"SET modelID = @modelID, problemDescriptionID = @problemID " +
                    $"WHERE requestID = @requestID";

                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@modelID", modelID);
                sql_BD.command.Parameters.AddWithValue("@problemID", problemID);
                sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

                var result = MessageBox.Show("Вы точно хотите изменить запись?", "Подтверждение изменения", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    sql_BD.command.ExecuteNonQuery();
                    MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Изменение записи отменено.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sql_BD.closeConnect();
            }
        }

        private void RedactOrder_Load(object sender, EventArgs e)
        {
            // Логика при загрузке формы, если требуется
        }
    }
}
