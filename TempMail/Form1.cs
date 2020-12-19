using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace TempMail
{
    public partial class Form1 : Form
    {
        string Email, Key;
        IWebProxy proxy;
        int timer;
        private static StringBuilder builder = new StringBuilder();
        public Form1()
        {
            InitializeComponent();
        }

        string GenRandomString(string Alphabet, int Length)
        {
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder(Length - 1);
            int Position = 0;
            for (int i = 0; i < Length; i++)
            {
                Position = rnd.Next(0, Alphabet.Length - 1);
                sb.Append(Alphabet[Position]);
            }
            return sb.ToString();
        }

        public static void ParseProxy(Uri uri)
        {
            string pattern = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,6}";
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                var regex = new Regex(pattern);
                Match match = regex.Match(wc.DownloadString(uri));
                while (match.Success)
                {
                    builder = new StringBuilder();
                    builder.Append(match.Value);
                    break;
                }
            }
        }

        public void ChangeHash()
        {
            try
            {
                string name = "YG_" + GenRandomString("QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm", 10);
                WebProxy proxy = null;
                if (Properties.Settings.Default.UseProxy)
                {
                    ParseProxy(new Uri(Properties.Settings.Default.server));
                    while (!TestProxy(builder.ToString())) { ParseProxy(new Uri(Properties.Settings.Default.server)); }
                    var spl = builder.ToString().Split(':');
                    proxy = new WebProxy(new Uri($"http://{spl[0]}:{spl[1]}/"), true);
                }

                string response = GET("https://post-shift.ru/api.php", $"action=reg&email={name}", proxy);
                User user = JsonConvert.DeserializeObject<User>(response);
                if (user.hash != null)
                {
                    Properties.Settings.Default.hash = user.hash;
                    Properties.Settings.Default.Save();
                    this.Text = $"TempMail [Остаток: 100]";
                }
            }
            catch { }
        }

        public int? GetLimit()
        {
            repeat:
            try
            {
                WebProxy proxy = null;
                if (Properties.Settings.Default.UseProxy)
                {
                    ParseProxy(new Uri(Properties.Settings.Default.server));
                    while (!TestProxy(builder.ToString())) { ParseProxy(new Uri(Properties.Settings.Default.server)); }
                    var spl = builder.ToString().Split(':');
                    proxy = new WebProxy(new Uri($"http://{spl[0]}:{spl[1]}/"), true);
                }

                string response = GET("https://post-shift.ru/api.php", $"action=balance&hash={Properties.Settings.Default.hash}", proxy);
                Limit user = JsonConvert.DeserializeObject<Limit>(response);
                if (user.limit == 0 || user.error != null)
                {
                    ChangeHash();
                    goto repeat;
                }
                else
                {
                    this.Text = $"TempMail [Остаток: {user.limit}]";
                    return user.limit;
                }
            }
            catch { return null; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Адрес электронной почты: ";
            label2.Text = "Секретный ключ: ";
            button2.Visible = false;
            timer1.Stop();
            label4.Visible = false;
            button3.Visible = false;
            try
            {
                GetLimit();
                string name = "YG_" + GenRandomString("QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm", 7);
                WebProxy proxy = null;
                if (Properties.Settings.Default.UseProxy)
                {
                    ParseProxy(new Uri(Properties.Settings.Default.server));
                    while (!TestProxy(builder.ToString())) { ParseProxy(new Uri(Properties.Settings.Default.server)); }
                    var spl = builder.ToString().Split(':');
                    proxy = new WebProxy(new Uri($"http://{spl[0]}:{spl[1]}/"), true);
                }
                string response = GET("https://post-shift.ru/api.php", $"action=new&name={name}&hash={Properties.Settings.Default.hash}", proxy);
                Email mail = JsonConvert.DeserializeObject<Email>(response);
                if (mail.email != null && mail.key != null)
                {
                    Email = mail.email;
                    Key = mail.key;
                    label1.Text += Email;
                    label3.Visible = true;
                    button2.Visible = true;
                    button3.Visible = true;
                    timer = 60;
                    timer1.Interval = 1;
                    label4.Visible = true;
                    timer1.Start();
                }
                else
                    MessageBox.Show("Ошибка создания временной почты: " + mail.error, "Ошибка");
                GetLimit();
            }
            catch (Exception ex){
                MessageBox.Show("Ошибка во время получения ответа от сервера\n" + ex.ToString(), "Ошибка запроса");
            }
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

        private void button2_Click(object sender, EventArgs e)
        {
            label3.Visible = false;
            label2.Text += Key;
            button2.Visible = false;
            label2.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"TempMail [Остаток: null]";
            if (String.IsNullOrEmpty(Properties.Settings.Default.hash))
                ChangeHash();
            else
                this.Text = $"TempMail [Остаток: {GetLimit()}]";
            label1.Text = "Адрес электронной почты: ";
            label2.Text = "Секретный ключ: ";
            label3.Visible = false;
            button2.Visible = false;
            label4.Visible = false;
            button3.Visible = false;
        }

        private void настройкиПроксиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Proxy().ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Interval == 1)
                timer1.Interval = 60000;
            if (timer != 0)
            {
                label4.Text = $"Ящик будет удален через: {timer} минут";
                timer--;
            }

            else if (timer <= 0)
            {
                label1.Text = "Адрес электронной почты: ";
                label2.Text = "Секретный ключ: ";
                button2.Visible = false;
                button3.Visible = false;
                timer1.Stop();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            new mail(Key, Email, proxy, this).ShowDialog();
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            if (label1.Text != "Адрес электронной почты: ")
                Clipboard.SetText(label1.Text.Substring(25)); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private void label2_DoubleClick(object sender, EventArgs e)
        {
            if (label2.Text != "Секретный ключ: ")
                Clipboard.SetText(label2.Text.Substring(16)); MessageBox.Show("Скопировано в буфер обмена!");
        }

        private void авторToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://irval.host");
        }

        static bool TestProxy(string host)
        {
            Ping ping = new Ping();

            try
            {
                PingReply reply = ping.Send(host.Split(':')[0], 2000);
                if (reply == null) return false;

                return (reply.Status == IPStatus.Success);
            }
            catch (PingException e)
            {
                MessageBox.Show("Прокси не прошел проверку!\n" + e.ToString());
                return false;
            }
        }
    }
}
