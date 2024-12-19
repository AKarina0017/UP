using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Master : Form
    {
        SqlCommand command;
        BD sql_BD = new BD();
        Login mainForm;
        int ID;

        public Master(Login login, int id)
        {
            InitializeComponent();
            mainForm = login;
            ID = id;
            string fio;
            string role;
            sql_BD.GetFioRole(ID, out fio, out role);
            toolStripTextBox1.Text = fio;
            toolStripTextBox2.Text = role;
            fullTable();
        }

        public void fullTable()
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            sql_BD.openConnect();

            // Незавершенные заказы
            command = new SqlCommand("SELECT r.requestID, r.startDate, t.modelName AS computerTechModel, " +
                                      "p.description AS problemDescription, s.statusName AS requestStatus, " +
                                      "r.completionDate, r.repairParts, c.[message] " +
                                      "FROM Requests r " +
                                      "LEFT JOIN HomeTechModels t ON r.modelID = t.modelID " +
                                      "LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID " +
                                      "LEFT JOIN RequestStatuses s ON r.requestStatusID = s.statusID " +
                                      "LEFT JOIN Comments c ON r.requestID = c.requestID " +
                                      $"WHERE r.masterID = {ID} AND r.requestStatusID <> 2", sql_BD.connection);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    dataGridView1.Rows.Add(new object[]
                    {
                        reader["requestID"],
                        reader["startDate"] != DBNull.Value ? Convert.ToDateTime(reader["startDate"]).ToString("yyyy-MM-dd") : "",
                        reader["computerTechModel"],
                        reader["problemDescription"],
                        reader["requestStatus"],
                        reader["completionDate"] != DBNull.Value ? Convert.ToDateTime(reader["completionDate"]).ToString("yyyy-MM-dd") : "",
                        reader["repairParts"],
                        reader["message"]
                    });
                }
            }

            // Завершенные заказы
            command = new SqlCommand("SELECT r.requestID, r.startDate, t.modelName AS computerTechModel, " +
                                      "p.description AS problemDescription, s.statusName AS requestStatus, " +
                                      "r.completionDate, r.repairParts, c.[message] " +
                                      "FROM Requests r " +
                                      "LEFT JOIN HomeTechModels t ON r.modelID = t.modelID " +
                                      "LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID " +
                                      "LEFT JOIN RequestStatuses s ON r.requestStatusID = s.statusID " +
                                      "LEFT JOIN Comments c ON r.requestID = c.requestID " +
                                      $"WHERE r.masterID = {ID} AND r.requestStatusID = 2", sql_BD.connection);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    dataGridView2.Rows.Add(new object[]
                    {
                        reader["requestID"],
                        reader["startDate"] != DBNull.Value ? Convert.ToDateTime(reader["startDate"]).ToString("yyyy-MM-dd") : "",
                        reader["computerTechModel"],
                        reader["problemDescription"],
                        reader["requestStatus"],
                        reader["completionDate"] != DBNull.Value ? Convert.ToDateTime(reader["completionDate"]).ToString("yyyy-MM-dd") : "",
                        reader["repairParts"],
                        reader["message"]
                    });
                }
            }

            sql_BD.closeConnect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].Cells[0].Value != null)
            {
                int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                BuyRepairParts buyRepairParts = new BuyRepairParts(orderID);
                if (buyRepairParts.ShowDialog() == DialogResult.Cancel)
                {
                    fullTable();
                }
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице.", "Недостаточно данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].Cells[0].Value != null)
            {
                int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                AddComment addComment = new AddComment(orderID, ID);
                if (addComment.ShowDialog() == DialogResult.Cancel)
                {
                    fullTable();
                }
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице.", "Недостаточно данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mainForm.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].Cells[0].Value != null)
            {
                var result = MessageBox.Show("Вы точно хотите завершить заказ?", "Подтверждение изменения статуса", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    int requestID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                    sql_BD.openConnect();

                    SqlCommand command = new SqlCommand($"UPDATE Requests SET requestStatusID = 2 WHERE requestID = {requestID}", sql_BD.connection);
                    command.ExecuteNonQuery();

                    sql_BD.closeConnect();
                    fullTable();
                }
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице.", "Недостаточно данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Master_Load(object sender, EventArgs e)
        {

        }
    }
}
