using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Manager : Form
    {
        BD sql_BD = new BD();
        Login mainForm;
        int ID;

        public Manager(Login login, int id)
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

        // Метод для чтения данных из SqlDataReader
        public List<string[]> readerData(SqlDataReader reader)
        {
            List<string[]> data = new List<string[]>();
            while (reader.Read())
            {
                string[] row = new string[10];
                row[0] = reader["requestID"].ToString();
                row[1] = reader["startDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["startDate"]).ToString("yyyy-MM-dd") : "";
                row[2] = reader["techTypeName"].ToString();
                row[3] = reader["modelName"].ToString();
                row[4] = reader["description"].ToString();
                row[5] = reader["statusName"].ToString();
                row[6] = reader["completionDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["completionDate"]).ToString("yyyy-MM-dd") : "";
                row[7] = reader["repairParts"].ToString();
                row[8] = reader["masterFIO"].ToString();
                row[9] = reader["clientFIO"].ToString();
                data.Add(row);
            }
            return data;
        }

        // Заполнение таблицы данными
        public void fullTable()
        {
            sql_BD.openConnect();
            dataGridView1.Rows.Clear();
            sql_BD.command.CommandText = @"
                SELECT 
                    r.requestID,
                    r.startDate,
                    ht.techTypeName,
                    hm.modelName,
                    p.description,
                    rs.statusName,
                    r.completionDate,
                    r.repairParts,
                    u_master.fio AS masterFIO,
                    u_client.fio AS clientFIO
                FROM Requests r
                LEFT JOIN HomeTechModels hm ON r.modelID = hm.modelID
                LEFT JOIN HomeTechTypes ht ON hm.techTypeID = ht.techTypeID
                LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID
                LEFT JOIN RequestStatuses rs ON r.requestStatusID = rs.statusID
                LEFT JOIN Users u_master ON r.masterID = u_master.userID
                LEFT JOIN Users u_client ON r.clientID = u_client.userID";

            using (SqlDataReader reader = sql_BD.command.ExecuteReader())
            {
                List<string[]> data = readerData(reader);
                foreach (var row in data)
                {
                    dataGridView1.Rows.Add(row);
                }
            }
            sql_BD.closeConnect();
        }

        // Обработчик нажатия на кнопку для редактирования заказа
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                RedactOrderManager redactOrderManager = new RedactOrderManager(orderID);
                if (redactOrderManager.ShowDialog() == DialogResult.Cancel)
                {
                    fullTable();
                }
            }
            else
            {
                MessageBox.Show("Выберите строку в таблице", "Недостаточно данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Manager_Load(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mainForm.Show();
            this.Close();
        }
    }
}
