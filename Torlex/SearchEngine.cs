using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Torlex
{
    class SearchEngine
    {
        private System.Windows.Forms.ListView lv;
        public ArrayList urls;
        private string savepath = "";

        public SearchEngine(System.Windows.Forms.ListView lv)
        {
            this.lv = lv;
            urls = new ArrayList();
        }

        public static string ParseTorrentPage(string name)
        {
            return "http://www.mejortorrent.com/secciones.php?sec=buscador&valor=" + name.Replace("+", "%2B").Replace(" ", " +");
        }

        public async Task<HtmlDocument> LoadAsync(string url)
        {
            HttpClient client = new HttpClient();
            Form1.form.TitleState = "Searching...";
            using (var response = await client.GetAsync(url))
            {
                using (var content = response.Content)
                {
                    Form1.form.TitleState = "Searching...";
                    var result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    return document;
                }
            }
        }

        public async void Overlap(HtmlDocument doc)
        {
            urls.Clear();
            System.Windows.Forms.ImageList l = new System.Windows.Forms.ImageList();
            l.ImageSize = new Size(120, 165);
            lv.Items.Clear();
            try
            {
                foreach (HtmlNode nd in doc.DocumentNode.SelectNodes("//tr[@height='22']"))
                {
                    foreach (HtmlNode ins in nd.SelectNodes("./td"))
                    {
                        if (ins.InnerHtml == "Pel�cula")
                        {
                            string prepath = nd.SelectSingleNode("./td").SelectSingleNode("./a").GetAttributeValue("href", "Error");
                            string path = "http://www.mejortorrent.com" + prepath;
                            string dnum = DownloadNumByURL(prepath);
                            HtmlDocument inside = await LoadAsync(path);
                            Form1.form.TitleState = "Loading...";
                            string Img = inside.DocumentNode.SelectSingleNode("//img[@style='border-bottom:1px solid black; margin:0px;']").GetAttributeValue("src", "Error");
                            string title = inside.DocumentNode.SelectSingleNode("//span[@style='font-size:18px; color:#0A3A86;']").SelectSingleNode("./b").InnerText + nd.SelectSingleNode("./td").SelectSingleNode("./span").InnerText;
                            string url = await DownloadURL(dnum);
                            l.Images.Add(ImageFromURL(Img));
                            lv.LargeImageList = l;
                            lv.Items.Add(new System.Windows.Forms.ListViewItem(title, l.Images.Count - 1));
                            urls.Add(url);
                        }
                    }
                }
                Form1.form.TitleState = "Finish!";
            }
            catch (Exception)
            {
                Form1.form.TitleState = "Error...";
                System.Windows.Forms.MessageBox.Show("Not found exception","Error",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        private string DownloadNumByURL(string url)
        {
            string dnum = url.Substring(24);
            string toreturn = "";
            foreach(char n in dnum)
            {
                if (n != '-')
                    toreturn += n;
                else
                    break;
            }
            return toreturn;
        }

        private async Task<string> DownloadURL(string downloadnum)
        {
            HtmlDocument getDownPage = await LoadAsync("http://www.mejortorrent.com/secciones.php?sec=descargas&ap=contar&tabla=peliculas&id=" + downloadnum + "&link_bajar=1");
            foreach(HtmlNode p in getDownPage.DocumentNode.SelectNodes("//a"))
            {
                if(p.InnerHtml == "<b>aqu�</b>")
                    return "http://www.mejortorrent.com" + p.GetAttributeValue("href","error");
            }
            return null;
        }

        private Image ImageFromURL(string url)
        {
            try
            {
                WebClient wc = new WebClient();
                byte[] bytes = wc.DownloadData(url);
                MemoryStream ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch (Exception)
            {
                return Properties.Resources.not_found;
            }
        }

        public string DownloadFile(string url)
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(bytes);
            string n = GetName(url);
            if (savepath == "")
            {
                System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();
                if(fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = fb.SelectedPath + "/" + n;
                    File.WriteAllBytes(path, bytes);
                    savepath = fb.SelectedPath;
                    return path;
                }
                return null;
            }
            else
            {
                string np = savepath + "/" + n;
                File.WriteAllBytes(np, bytes);
                return np;
            }
        }

        private string GetName(string url)
        {
            int l = url.LastIndexOf("/");
            return url.Remove(0, l);
        }
    }
}
