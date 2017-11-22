using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Torlex
{
    public partial class Form1 : Form
    {
        SearchEngine se;

        public string TitleState { set { Text = "Torlex (" + Application.ProductVersion + ") State: " + value;  } }

        public static Form1 form;

        public Form1()
        {
            InitializeComponent();
            se = new SearchEngine(listView1);
            listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
            TitleState = "Welcome!";
            form = this;
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int indx = listView1.SelectedItems[0].Index;
            string path = se.DownloadFile((string)se.urls[indx]);
            if (path != null)
                Process.Start(path);
            else
                MessageBox.Show("No se guardo");
        }

        private async void Search()
        {
            HtmlAgilityPack.HtmlDocument webpage = await se.LoadAsync(SearchEngine.ParseTorrentPage(textBox1.Text));
            se.Overlap(webpage);
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
            MessageBox.Show("Alberto Elorza Rubio", "Author",MessageBoxButtons.OK,MessageBoxIcon.Question);
        }
    }
}
