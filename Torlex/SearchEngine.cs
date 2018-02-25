using HtmlAgilityPack;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Torlex
{
    class SearchEngine
    {
        public System.Windows.Forms.ListView lv;
        public ArrayList urls;
        public ArrayList multi;
        public string savepath = "";
        public int Key = 0;
        private string[] CanEnter = { "Pel�cula", "Serie" };

        public SearchEngine(System.Windows.Forms.ListView lv)
        {
            this.lv = lv;
            urls = new ArrayList();
            multi = new ArrayList();
        }

        public static string ParseTorrentPage(string name)
        {
            return "http://www.mejortorrent.com/secciones.php?sec=buscador&valor=" + name;
        }

        public async Task<HtmlDocument> LoadMainAsync(string url)
        {
            Form1.form.TitleState = "Buscando...";
            return await LoadAsync(url);
        }

        public async Task<HtmlDocument> LoadAsync(string url)
        {
            HttpClient client = new HttpClient();
            using (var response = await client.GetAsync(url))
            {
                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    return document;
                }
            }
        }
        public async void Overlap(HtmlDocument doc, int asg)
        {
            System.Windows.Forms.ImageList l = new System.Windows.Forms.ImageList();
            urls.Clear();
            l.ImageSize = new Size(120, 165);
            lv.Items.Clear();
            try
            {
                foreach (HtmlNode nd in doc.DocumentNode.SelectNodes("//tr[@height='22']"))
                {
                    Form1.form.TitleState = "Buscando...";
                    foreach (HtmlNode ins in nd.SelectNodes("./td"))
                    {
                        string type = ins.InnerHtml;
                        if (Key == asg && Array.IndexOf(CanEnter, type) >= 0)
                        {
                            string prepath = nd.SelectSingleNode("./td").SelectSingleNode("./a").GetAttributeValue("href", "Error");
                            string path = "http://www.mejortorrent.com" + prepath;
                            HtmlDocument inside = await LoadAsync(path);
                            string Img = "";
                            string title = "";
                            if (type == "Pel�cula")
                            {
                                Img = inside.DocumentNode.SelectSingleNode("//img[@style='border-bottom:1px solid black; margin:0px;']").GetAttributeValue("src", "Error");
                                title = inside.DocumentNode.SelectSingleNode("//span[@style='font-size:18px; color:#0A3A86;']").SelectSingleNode("./b").InnerText + nd.SelectSingleNode("./td").SelectSingleNode("./span").InnerText;
                                string dnum = DownloadNumByURL(prepath);
                                string url = await DownloadURL(dnum);
                                urls.Add(url);
                                multi.Add(false);
                                l.Images.Add(ImageFromURL(Img));
                                lv.LargeImageList = l;
                                lv.Items.Add(new System.Windows.Forms.ListViewItem(title, l.Images.Count - 1));
                            }
                            else if (type == "Serie")
                            {
                                Img = inside.DocumentNode.SelectSingleNode("//img[@style='border-right:1px solid black; border-bottom:1px solid black;']").GetAttributeValue("src", "Error");
                                string temp = inside.DocumentNode.SelectSingleNode("//span[@style='font-size:16px;']").SelectSingleNode("./b").InnerText;
                                title = inside.DocumentNode.SelectSingleNode("//span[@style='font-size:18px; color:#0A3A86;']").SelectSingleNode("./b").InnerText + nd.SelectSingleNode("./td").SelectSingleNode("./span").InnerText + temp;
                                BrowserHandler b = new BrowserHandler(this, l, Img, title, path);
                            }
                        }
                    }
                }
                if(Key == asg)
                    Form1.form.TitleState = "Acabado!";
            }
            catch (Exception e)
            {
                Form1.form.TitleState = "Error";
                System.Windows.Forms.MessageBox.Show("No pudimos encontrar su película:\n " + e.ToString(), "Error de busqueda", System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
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

        public async Task<string> DownloadURL(string downloadnum)
        {
            HtmlDocument getDownPage = await LoadAsync("http://www.mejortorrent.com/secciones.php?sec=descargas&ap=contar&tabla=peliculas&id=" + downloadnum + "&link_bajar=1");
            foreach (HtmlNode p in getDownPage.DocumentNode.SelectNodes("//a"))
            {
                if (p.InnerHtml == "<b>aqu�</b>")
                    return p.GetAttributeValue("href", "error");
            }
            return null;
        }

        public string[] DownloadMultiURL(string content)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@style='text-decoration:none;']");
            if (nodes != null)
            {
                string[] torreturn = new string[nodes.Count];
                int x = 0;
                foreach (HtmlNode p in nodes)
                {
                    torreturn[x] = p.GetAttributeValue("href", "error");
                    x++;
                }
                return torreturn;
            }
            else
            {
                return null;
            }
        }

        public Image ImageFromURL(string url)
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

        public string DownloadFile(string url, ref bool stop)
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(bytes);
            string n = GetName(url);
            if (System.Windows.Forms.MessageBox.Show("¿Quieres realmente descargar el torrent?", "Descargar Torrent", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                return null;
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
                stop = true;
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

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        public void FocusAndPlayTorrent()
        {
            try
            {
                Process[] aProcess = Process.GetProcessesByName("uTorrent");
                if(aProcess.Length == 0)
                    System.Windows.Forms.MessageBox.Show("El proceso de uTorrent no se encuentra operativo", "Error de inicio de uTorrent", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                foreach (Process bProcess in aProcess)
                {
                    if (bProcess != null)
                    {
                        if (bProcess.MainWindowHandle == IntPtr.Zero)
                        {
                            ShowWindow(bProcess.Handle, ShowWindowEnum.Restore);
                        }
                        SetForegroundWindow(bProcess.MainWindowHandle);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("No pudimos abrir uTorrent, asegurese de que esta instalado e iniciado", "Error de inicio de uTorrent", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    break;
                }
                Thread.Sleep(5000);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString(), "Error de inicio de uTorrent", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
