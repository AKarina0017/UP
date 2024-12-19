using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class BuyRepairParts : Form
    {
        BD sql_BD = new BD();
        int orderID;

        public BuyRepairParts(int id)
        {
            InitializeComponent();
            orderID = id;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sql_BD.openConnect();

                // Обновление поля repairParts в таблице Requests
                sql_BD.command.CommandText = $@"
                    UPDATE Requests 
                    SET repairParts = @repairParts 
                    WHERE requestID = @orderID";

                // Параметры для предотвращения SQL-инъекций
                sql_BD.command.Parameters.Clear();
                sql_BD.command.Parameters.AddWithValue("@repairParts", richTextBox1.Text);
                sql_BD.command.Parameters.AddWithValue("@orderID", orderID);

                sql_BD.command.ExecuteNonQuery();
                MessageBox.Show("Запись успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sql_BD.closeConnect();
            }
        }

        private void BuyRepairParts_Load(object sender, EventArgs e)
        {
            // Здесь можно добавить дополнительную логику, если требуется
        }
    }
}
