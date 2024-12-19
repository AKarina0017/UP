using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Operator : Form
    {
        BD sql_BD = new BD();
        Login mainForm;
        int ID;

        public Operator(Login login, int id)
        {
            InitializeComponent();
            mainForm = login;
            ID = id;
            string fio;
            string role;
            sql_BD.GetFioRole(ID, out fio, out role);
            toolStripTextBox1.Text = fio;
            toolStripTextBox2.Text = role;
            FullTable();
        }

        public List<string[]> readerData(SqlDataReader reader)
        {
            List<string[]> data = new List<string[]>();
            while (reader.Read())
            {
                string[] row = new string[11];
                row[0] = reader["requestID"].ToString();
                row[1] = reader["startDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["startDate"]).ToString("yyyy-MM-dd") : "";
                row[2] = reader["techTypeName"].ToString();
                row[3] = reader["modelName"].ToString();
                row[4] = reader["problemDescription"].ToString();
                row[5] = reader["statusName"].ToString();
                row[6] = reader["completionDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["completionDate"]).ToString("yyyy-MM-dd") : "";
                row[7] = reader["repairParts"].ToString();
                row[8] = reader["message"].ToString();
                row[9] = reader["masterFIO"].ToString();
                row[10] = reader["clientFIO"].ToString();
                data.Add(row);
            }
            return data;
        }

        public void FullTable()
        {
            sql_BD.openConnect();
            dataGridView1.Rows.Clear();
            sql_BD.command.CommandText = @"
                SELECT r.requestID, r.startDate, t.techTypeName, m.modelName, 
                       p.description AS problemDescription, s.statusName, r.completionDate, 
                       r.repairParts, u_master.fio AS masterFIO, u_client.fio AS clientFIO, 
                       c.message
                FROM Requests r
                LEFT JOIN HomeTechModels m ON r.modelID = m.modelID
                LEFT JOIN HomeTechTypes t ON m.techTypeID = t.techTypeID
                LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID
                LEFT JOIN RequestStatuses s ON r.requestStatusID = s.statusID
                LEFT JOIN Comments c ON r.requestID = c.requestID
                LEFT JOIN Users u_master ON r.masterID = u_master.userID
                LEFT JOIN Users u_client ON r.clientID = u_client.userID
                WHERE r.requestStatusID = 1";
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                List<string[]> data = readerData(reader);
                foreach (var row in data)
                {
                    dataGridView1.Rows.Add(row);
                }
            }
            dataGridView2.Rows.Clear();
            sql_BD.command.CommandText = @"
                SELECT r.requestID, r.startDate, t.techTypeName, m.modelName, 
                       p.description AS problemDescription, s.statusName, r.completionDate, 
                       r.repairParts, u_master.fio AS masterFIO, u_client.fio AS clientFIO, 
                       c.message
                FROM Requests r
                LEFT JOIN HomeTechModels m ON r.modelID = m.modelID
                LEFT JOIN HomeTechTypes t ON m.techTypeID = t.techTypeID
                LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID
                LEFT JOIN RequestStatuses s ON r.requestStatusID = s.statusID
                LEFT JOIN Comments c ON r.requestID = c.requestID
                LEFT JOIN Users u_master ON r.masterID = u_master.userID
                LEFT JOIN Users u_client ON r.clientID = u_client.userID";
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                List<string[]> data = readerData(reader);
                foreach (var row in data)
                {
                    dataGridView2.Rows.Add(row);
                }
            }
        }

        private void FoundTable(string searchTerm)
        {
            sql_BD.openConnect();
            sql_BD.command.CommandText = @"
                SELECT r.requestID, r.startDate, t.techTypeName, m.modelName, 
                       p.description AS problemDescription, s.statusName, r.completionDate, 
                       r.repairParts, u_master.fio AS masterFIO, u_client.fio AS clientFIO, 
                       c.message
                FROM Requests r
                LEFT JOIN HomeTechModels m ON r.modelID = m.modelID
                LEFT JOIN HomeTechTypes t ON m.techTypeID = t.techTypeID
                LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID
                LEFT JOIN RequestStatuses s ON r.requestStatusID = s.statusID
                LEFT JOIN Comments c ON r.requestID = c.requestID
                LEFT JOIN Users u_master ON r.masterID = u_master.userID
                LEFT JOIN Users u_client ON r.clientID = u_client.userID
                WHERE (CAST(r.requestID AS NVARCHAR) LIKE '%' + @searchTerm + '%' OR
                       t.techTypeName LIKE '%' + @searchTerm + '%' OR
                       m.modelName LIKE '%' + @searchTerm + '%' OR
                       p.description LIKE '%' + @searchTerm + '%' OR
                       u_master.fio LIKE '%' + @searchTerm + '%' OR
                       u_client.fio LIKE '%' + @searchTerm + '%' OR
                       c.message LIKE '%' + @searchTerm + '%')";
            sql_BD.command.Parameters.AddWithValue("@searchTerm", searchTerm.Replace("'", "''"));
            dataGridView2.Rows.Clear();
            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                List<string[]> data = readerData(reader);
                foreach (var row in data)
                {
                    dataGridView2.Rows.Add(row);
                }
            }
            int Records = dataGridView2.Rows.Count - 1;
            sql_BD.command.CommandText = "SELECT COUNT(*) FROM Requests";
            int Records1 = Convert.ToInt32(sql_BD.command.ExecuteScalar());
            label3.Text = "Записей: " + Records + " из " + Records1;
            sql_BD.command.Parameters.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddOrder addOrder = new AddOrder(ID);
            if (addOrder.ShowDialog() == DialogResult.Cancel)
                FullTable();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                RegistrationOrder registrationOrder = new RegistrationOrder(orderID);
                if (registrationOrder.ShowDialog() == DialogResult.Cancel)
                    FullTable();
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице", "Недостаточно данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                int orderID = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells[0].Value.ToString());
                DialogResult result = MessageBox.Show("Вы точно хотите удалить заявку?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        sql_BD.openConnect();
                        sql_BD.command.CommandText = $"DELETE FROM Requests WHERE [requestID] = {orderID}";
                        sql_BD.command.ExecuteNonQuery();
                        sql_BD.closeConnect();
                        MessageBox.Show("Запрос успешно удален.", "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        FullTable();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Удаление отменено.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице", "Недостаточно данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void button3_Click_1(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                int orderID = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells[0].Value.ToString());
                RegistrationOrder registrationOrder = new RegistrationOrder(orderID);
                if (registrationOrder.ShowDialog() == DialogResult.Cancel)
                    FullTable();
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице.", "Недостаточно данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                label3.Text = string.Empty;
                FullTable();
            }
            else
            {
                dataGridView2.Rows.Clear();
                FoundTable(textBox1.Text);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mainForm.Show();
            this.Close();
        }

        private void Operator_Load(object sender, EventArgs e)
        {

        }
    }
}
