using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddOrder : Form
    {
        BD sql_BD = new BD();
        int clientID;

        public AddOrder(int id)
        {
            InitializeComponent();
            clientID = id;

            // Открываем соединение и заполняем первый выпадающий список
            sql_BD.openConnect();
            try
            {
                comboBox1.Items.Clear();

                // Запрос на получение типов техники
                sql_BD.command.CommandText = "SELECT techTypeName FROM HomeTechTypes";
                using (SqlDataReader reader = sql_BD.command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader.GetString(reader.GetOrdinal("techTypeName")));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Добавление нового запроса
            try
            {
                if (string.IsNullOrWhiteSpace(comboBox1.Text) || string.IsNullOrWhiteSpace(comboBox2.Text) || string.IsNullOrWhiteSpace(richTextBox1.Text))
                {
                    throw new Exception("Все поля должны быть заполнены.");
                }

                // Добавление записи в БД через метод `AddOrder`
                sql_BD.AddOrder(comboBox1.Text, comboBox2.Text, richTextBox1.Text, clientID);
                MessageBox.Show("Запрос успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Заполнение второго выпадающего списка (моделей техники) на основе выбранного типа техники
            try
            {
                comboBox2.Items.Clear();

                sql_BD.command.CommandText = @"
                    SELECT modelName 
                    FROM HomeTechModels 
                    WHERE techTypeID = (
                        SELECT techTypeID 
                        FROM HomeTechTypes 
                        WHERE techTypeName = @techTypeName
                    )";

                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@techTypeName", comboBox1.Text);

                using (SqlDataReader reader = sql_BD.command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox2.Items.Add(reader.GetString(reader.GetOrdinal("modelName")));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моделей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddOrder_Load(object sender, EventArgs e)
        {
            // Метод вызывается при загрузке формы, если требуется дополнительная инициализация
        }
    }
}
