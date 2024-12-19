using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddComment : Form
    {
        BD sql_BD = new BD();
        int orderID;
        int masterID;

        public AddComment(int idO, int idM)
        {
            InitializeComponent();
            orderID = idO;
            masterID = idM;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sql_BD.openConnect();
            try
            {
                // Проверка, существует ли уже комментарий для данного запроса
                sql_BD.command.CommandText = "SELECT COUNT(*) FROM Comments WHERE requestID = @requestID";
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@requestID", orderID);

                int commentCount = Convert.ToInt32(sql_BD.command.ExecuteScalar());

                // Если поле пустое
                if (string.IsNullOrWhiteSpace(richTextBox1.Text))
                {
                    var result = MessageBox.Show("Вы точно хотите удалить комментарий?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        sql_BD.command.CommandText = "DELETE FROM Comments WHERE requestID = @requestID";
                        sql_BD.command.ExecuteNonQuery();
                        MessageBox.Show("Комментарий успешно удалён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Удаление отменено.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (commentCount > 0)
                {
                    // Если комментарий уже существует
                    var result = MessageBox.Show("Вы уверены, что хотите обновить комментарий?", "Подтверждение обновления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        sql_BD.command.CommandText = "UPDATE Comments SET message = @message WHERE requestID = @requestID";
                        sql_BD.command.Parameters.AddWithValue("@message", richTextBox1.Text);
                        sql_BD.command.ExecuteNonQuery();
                        MessageBox.Show("Комментарий успешно обновлён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Обновление отменено.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // Если комментария ещё нет, добавляем новый
                    sql_BD.command.CommandText = "INSERT INTO Comments (message, masterID, requestID) VALUES (@message, @masterID, @requestID)";
                    sql_BD.command.Parameters.AddWithValue("@message", richTextBox1.Text);
                    sql_BD.command.Parameters.AddWithValue("@masterID", masterID);
                    sql_BD.command.ExecuteNonQuery();
                    MessageBox.Show("Комментарий успешно добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с комментариями: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sql_BD.closeConnect();
                this.Close();
            }
        }

        private void AddComment_Load(object sender, EventArgs e)
        {
            // Этот метод можно использовать для начальной инициализации формы
        }

        private void AddComment_Load_1(object sender, EventArgs e)
        {

        }
    }
}
