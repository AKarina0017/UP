using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using MessagingToolkit.QRCode.Codec;

namespace WindowsFormsApp1
{
    public partial class Client : Form
    {
        BD sql_BD = new BD();
        Login mainForm;
        int ID;

        public Client(Login login, int id)
        {
            InitializeComponent();

            // Генерация QR-кода
            string qrtext = "https://docs.google.com/forms/d/e/1FAIpQLScTuA2CpxkwOLQPSpE91aALP2YUG9g-E8VpQnFKkztG0NjC5A/viewform";
            QRCodeEncoder encoder = new QRCodeEncoder();
            Bitmap qrcode = encoder.Encode(qrtext);
            pictureBox1.Image = qrcode as Image;

            mainForm = login;
            string fio;
            string role;
            sql_BD.GetFioRole(id, out fio, out role);
            toolStripTextBox1.Text = fio;
            toolStripTextBox2.Text = role;
            ID = id;

            fullTable();
        }

        public void fullTable()
        {
            dataGridView1.Rows.Clear();
            sql_BD.openConnect();
            sql_BD.command.CommandText = $@"
                SELECT 
                    r.requestID, 
                    r.startDate, 
                    ht.techTypeName, 
                    hm.modelName, 
                    p.description AS problemDescription, 
                    rs.statusName AS requestStatus, 
                    r.completionDate, 
                    r.repairParts, 
                    c.message 
                FROM Requests r
                LEFT JOIN HomeTechModels hm ON r.modelID = hm.modelID
                LEFT JOIN HomeTechTypes ht ON hm.techTypeID = ht.techTypeID
                LEFT JOIN Problems p ON r.problemDescriptionID = p.problemID
                LEFT JOIN RequestStatuses rs ON r.requestStatusID = rs.statusID
                LEFT JOIN Comments c ON r.requestID = c.requestID
                WHERE r.clientID = {ID}";

            SqlDataReader reader = sql_BD.command.ExecuteReader();
            List<string[]> data = new List<string[]>();

            while (reader.Read())
            {
                string[] row = new string[9];
                row[0] = reader["requestID"].ToString();
                row[1] = reader["startDate"] != DBNull.Value ? Convert.ToDateTime(reader["startDate"]).ToString("yyyy-MM-dd") : "";
                row[2] = reader["techTypeName"].ToString();
                row[3] = reader["modelName"].ToString();
                row[4] = reader["problemDescription"].ToString();
                row[5] = reader["requestStatus"].ToString();
                row[6] = reader["completionDate"] != DBNull.Value ? Convert.ToDateTime(reader["completionDate"]).ToString("yyyy-MM-dd") : "";
                row[7] = reader["repairParts"].ToString();
                row[8] = reader["message"].ToString();

                data.Add(row);
            }
            sql_BD.closeConnect();

            foreach (var row in data)
            {
                dataGridView1.Rows.Add(row);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddOrder addOrder = new AddOrder(ID);
            if (addOrder.ShowDialog() == DialogResult.Cancel)
            {
                fullTable();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                RedactOrder redactOrder = new RedactOrder(orderID);
                if (redactOrder.ShowDialog() == DialogResult.Cancel)
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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (linkLabel1.Text == "Оставить отзыв")
            {
                pictureBox1.Visible = true;
                linkLabel1.Text = "Закрыть";
            }
            else
            {
                pictureBox1.Visible = false;
                linkLabel1.Text = "Оставить отзыв";
            }
        }

        private void Client_Load(object sender, EventArgs e)
        {
        }
    }
}
