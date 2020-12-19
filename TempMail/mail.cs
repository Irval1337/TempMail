using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace TempMail
{
    public partial class mail : Form
    {
        string Key, Mail;
        IWebProxy Proxy;
        int index = 0;
        int count;
        bool first = true;
        Form1 main;

        public mail(string key, string mail, IWebProxy proxy, Form1 form1)
        {
            InitializeComponent();
            this.Text = mail;
            Key = key;
            Mail = mail;
            Proxy = proxy;
            main = form1;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            label1.Focus();
        }

        void Update()
        {
            try
            {
                main.GetLimit();
                string response = GET("https://post-shift.ru/api.php", $"action=getlist&key={Key}&hash={Properties.Settings.Default.hash}", Proxy);
                if (!response.StartsWith("["))
                {
                    Letters letters = JsonConvert.DeserializeObject<Letters>(response);
                    if (letters.error.ToLower().StartsWith("key_not_found") || letters.error.ToLower().StartsWith("the_list_is_empty"))
                        button1.Enabled = button2.Enabled = false;
                    DialogResult result = MessageBox.Show("Ошибка получения ответа сервера: " + letters.error, "Ошибка", MessageBoxButtons.OK);
                    if (result == DialogResult.OK)
                        this.Close();
                }
                else
                {
                    response = "{'letters':" + response + "}";
                    Letters letters = JsonConvert.DeserializeObject<Letters>(response);
                    count = letters.letters.Length;

                    ChangeMail(letters.letters, index);
                }
                main.GetLimit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка во время выполнения кода\n" + ex.ToString());
            }
        }

        void ChangeMail (Letter[] data, int index)
        {
            textBox1.Text = data[index].id.ToString();
            textBox2.Text = data[index].from;
            textBox3.Text = data[index].subject;
            textBox5.Text = data[index].date;
            try
            {
                main.GetLimit();
                string response = GET("https://post-shift.ru/api.php", $"action=getmail&key={Key}&id={count - index}&hash={Properties.Settings.Default.hash}", Proxy);
                Body message = JsonConvert.DeserializeObject<Body>(response);
                if (message.message != null)
                    richTextBox1.Text = message.message;
                else
                    MessageBox.Show("Ошибка получения ответа сервера: " + message.error, "Ошибка");
                main.GetLimit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка во время работы программы\n" + ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            index++;
            button2.Enabled = count > index + 1;
            button1.Enabled = index > 0;  
            Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            index--;
            button2.Enabled = count > index + 1;
            button1.Enabled = index > 0;
            Update();
        }

        private void mail_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Update();
            button2.Enabled = count != 1;
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private void textBox2_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private void textBox3_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox3.Text); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private void textBox5_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox5.Text); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private void richTextBox1_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private static string GET(string Url, string Data, IWebProxy proxy = null)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(Url + "?" + Data);
            req.Proxy = proxy;
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }
    }
}
