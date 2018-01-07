using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Torlex
{
    public partial class Form1 : Form
    {
        SearchEngine se;

        public string TitleState { set { Text = "Torlex (" + Application.ProductVersion + ") Estado: " + value;  } }

        public static Form1 form;

        public Form1()
        {
            InitializeComponent();
            se = new SearchEngine(listView1);
            listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
            TitleState = "Bienvenido!";
            form = this;
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            se = new SearchEngine(listView1);
            listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
            TitleState = "Bienvenido!";
            form = this;
            try
            {
                if (args.Length > 1)
                    se.savepath = args[1];
                string path = se.DownloadFile(args[0]);
                if (path != null)
                {
                    Process.Start(path);
                    se.FocusAndPlayTorrent();
                }
                else
                    MessageBox.Show("No se pudo guardar", "Error de guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error general", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            WindowState = FormWindowState.Minimized;
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int indx = listView1.SelectedItems[0].Index;
            if ((bool)se.multi[indx])
            {
                string[] urls = (string[])se.urls[indx];
                if (urls != null)
                {
                    foreach (string url in urls)
                    {
                        string path = se.DownloadFile(url);
                        if (path != null)
                            Process.Start(path);
                        else
                            MessageBox.Show("No se pudo guardar un Torrent", "Error de guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    MessageBox.Show("No se pudo guardar un grupo de Torrents", "Error de guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string path = se.DownloadFile((string)se.urls[indx]);
                if (path != null)
                    Process.Start(path);
                else
                    MessageBox.Show("No se pudo guardar un Torrent", "Error de guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Search()
        {
            if (textBox1.Text.Length > 2)
            {
                listView1.Items.Clear();
                se.Key++;
                se = new SearchEngine(listView1);
                HtmlAgilityPack.HtmlDocument webpage = await se.LoadMainAsync(SearchEngine.ParseTorrentPage(textBox1.Text));
                se.Overlap(webpage, se.Key);
            }
            else
            {
                MessageBox.Show($"El nombre de la película de debe ser mayor a { textBox1.Text.Length } letras", "Error de longitud del texto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                Search();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void authorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Alberto Elorza Rubio", "Autor",MessageBoxButtons.OK,MessageBoxIcon.Question);
        }
    }
}
