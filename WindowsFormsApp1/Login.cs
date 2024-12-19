using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;

namespace WindowsFormsApp1
{
    public partial class Login : Form
    {
        BD sql_BD = new BD();
        private int loginAttempts = 0; // Общий счётчик попыток входа
        private int captchaAttempts = 0; // Счётчик попыток после включения капчи
        private bool isLocked = false; // Флаг блокировки входа
        private System.Timers.Timer lockTimer; // Таймер блокировки
        private const int lockDuration = 3 * 60 * 1000; // 3 минуты в миллисекундах
        private bool permanentLock = false; // Флаг блокировки до перезапуска

        public Login()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            textBoxPassword.PasswordChar = checkBox.Checked ? '\0' : '*';
        }

        private string text = String.Empty;
        PictureBox captcha = new PictureBox();
        Button updateCaptcha = new Button();
        TextBox textBoxCaptcha = new TextBox();

        private Bitmap CreateImage(int Width, int Height)
        {
            Random rnd = new Random();
            Bitmap result = new Bitmap(Width, Height);
            int Xpos = rnd.Next(0, Width - 50);
            int Ypos = rnd.Next(15, Height - 15);
            Brush[] colors = { Brushes.Black, Brushes.Red, Brushes.RoyalBlue, Brushes.Green };
            Graphics g = Graphics.FromImage(result);
            g.Clear(Color.Gray);

            text = String.Empty;
            string ALF = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
            for (int i = 0; i < 5; ++i)
                text += ALF[rnd.Next(ALF.Length)];

            g.DrawString(text, new Font("Arial", 15), colors[rnd.Next(colors.Length)], new PointF(Xpos, Ypos));
            g.DrawLine(Pens.Black, new Point(0, 0), new Point(Width - 1, Height - 1));
            g.DrawLine(Pens.Black, new Point(0, Height - 1), new Point(Width - 1, 0));

            for (int i = 0; i < Width; ++i)
                for (int j = 0; j < Height; ++j)
                    if (rnd.Next() % 20 == 0)
                        result.SetPixel(i, j, Color.White);

            return result;
        }

        private void ToEnter(int userID, int typeID)
        {
            this.Hide();

            switch (typeID)
            {
                case 4: // Клиент
                    Client client = new Client(this, userID);
                    client.Show();
                    break;
                case 2: // Мастер
                    Master master = new Master(this, userID);
                    master.Show();
                    break;
                case 3: // Оператор
                    Operator operation = new Operator(this, userID);
                    operation.Show();
                    break;
                case 1: // Менеджер
                    Manager manager = new Manager(this, userID);
                    manager.Show();
                    break;
                default:
                    MessageBox.Show("Неизвестный тип пользователя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Show();
                    break;
            }
        }

        private void buttonCaptcha_Click(object sender, EventArgs e)
        {
            captcha.Image = this.CreateImage(captcha.Width, captcha.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (permanentLock)
            {
                MessageBox.Show("Доступ заблокирован до перезапуска приложения.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isLocked)
            {
                MessageBox.Show("Попробуйте снова через 3 минуты.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                sql_BD.openConnect();

                string login = textBoxLogin.Text.Trim();
                string password = textBoxPassword.Text.Trim();

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Логин и пароль не могут быть пустыми.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверяем капчу, если она активна
                if (loginAttempts >= 2 && captcha.Visible)
                {
                    if (string.IsNullOrWhiteSpace(textBoxCaptcha.Text) || textBoxCaptcha.Text != text)
                    {
                        MessageBox.Show("Неверная капча. Попробуйте снова.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        captchaAttempts++;
                        HandleFailedLogin();
                        return;
                    }
                }

                string query = @"SELECT [userID], [typeID] FROM [Users] WHERE [login] = @login AND [password] = @password";

                using (SqlCommand command = new SqlCommand(query, sql_BD.connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Успешный вход
                            loginAttempts = 0;
                            captchaAttempts = 0;
                            isLocked = false;
                            textBoxCaptcha.Clear();
                            textBoxCaptcha.Visible = false;
                            captcha.Visible = false;
                            updateCaptcha.Visible = false;

                            ToEnter(reader.GetInt32(reader.GetOrdinal("userID")), reader.GetInt32(reader.GetOrdinal("typeID")));
                        }
                        else
                        {
                            // Неверный логин или пароль
                            MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            HandleFailedLogin();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sql_BD.closeConnect();
            }
        }

        private void HandleFailedLogin()
        {
            loginAttempts++;

            if (loginAttempts == 2)
            {
                if (!captcha.Visible)
                {
                    captcha.Image = CreateImage(200, 50);
                    captcha.Location = new Point(10, 220); // Позиция капчи
                    captcha.Size = new Size(200, 50);
                    this.Controls.Add(captcha);
                    captcha.Visible = true;

                    updateCaptcha.Text = "Обновить капчу";
                    updateCaptcha.Location = new Point(220, 220);
                    updateCaptcha.Click += buttonCaptcha_Click;
                    this.Controls.Add(updateCaptcha);
                    updateCaptcha.Visible = true;

                    textBoxCaptcha.Location = new Point(10, 280);
                    textBoxCaptcha.Size = new Size(200, 30);
                    this.Controls.Add(textBoxCaptcha);
                    textBoxCaptcha.Visible = true;

                    MessageBox.Show("Введите капчу, чтобы продолжить.", "Капча", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            if (captchaAttempts > 2)
            {
                isLocked = true;
                lockTimer = new System.Timers.Timer(lockDuration);
                lockTimer.Elapsed += (sender, args) =>
                {
                    isLocked = false;
                    lockTimer.Stop();
                    lockTimer.Dispose();
                };
                lockTimer.Start();
                MessageBox.Show("Вход заблокирован на 3 минуты.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (captchaAttempts > 3)
            {
                permanentLock = true;
                MessageBox.Show("Доступ заблокирован до перезапуска приложения.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Login_Load(object sender, EventArgs e)
        {
            this.Height = 300;
        }
    }
}
