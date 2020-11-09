using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TempMail
{
    public partial class Proxy : Form
    {
        private static StringBuilder builder = new StringBuilder();

        public Proxy()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                ParseProxy(new Uri(textBox1.Text));
                if (builder.Length <= 0)
                    MessageBox.Show("Введенная страница не поддерживает парсинг прокси!");
                else
                {
                    Properties.Settings.Default.server = textBox1.Text;
                    Properties.Settings.Default.UseProxy = checkBox1.Checked;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Настройки успешно сохранины!");
                }
            }
            else
            {
                Properties.Settings.Default.UseProxy = checkBox1.Checked;
                Properties.Settings.Default.Save();
                MessageBox.Show("Настройки успешно сохранины!");
            }
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

        private void Proxy_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Properties.Settings.Default.UseProxy;
            textBox1.Text = Properties.Settings.Default.server;
            textBox1.Enabled = checkBox1.Checked ? true : false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = checkBox1.Checked ? true : false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
