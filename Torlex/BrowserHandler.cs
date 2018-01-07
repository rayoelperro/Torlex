using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Torlex
{
    class BrowserHandler
    {
        SearchEngine se;
        ImageList l;
        string img;
        string title;
        string path;
        WebBrowser v;
        bool apply = false;
        const string DOWNURL = "http://www.mejortorrent.com/secciones.php?sec=descargas&ap=contar_varios";

        public BrowserHandler(SearchEngine se, ImageList l, string img, string title, string path)
        {
            this.se = se;
            this.l = l;
            this.img = img;
            this.title = title;
            this.path = path;
            v = CreateWB(path);
            v.DocumentCompleted += Handle;
        }

        private void Handle(object o, WebBrowserDocumentCompletedEventArgs e)
        {
            if (apply && v.Url.ToString() == DOWNURL)
            {
                se.urls.Add(se.DownloadMultiURL(v.DocumentText));
                se.multi.Add(true);
                l.Images.Add(se.ImageFromURL(img));
                se.lv.LargeImageList = l;
                se.lv.Items.Add(new ListViewItem(title, l.Images.Count - 1));
                v.DocumentCompleted -= Handle;
            }
            else if(!apply && v.Url.ToString() == path)
            {
                HtmlElementCollection c = v.Document.GetElementsByTagName("input");
                foreach (HtmlElement a in c)
                {
                    if (a.GetAttribute("type") == "checkbox")
                        a.InvokeMember("click");
                }
                HtmlElementCollection f = v.Document.GetElementsByTagName("input");
                foreach (HtmlElement a in f)
                {
                    if (a.GetAttribute("value") == "Descargar Seleccionados" && a.GetAttribute("type") == "submit")
                    {
                        a.InvokeMember("click");
                        apply = true;
                    }
                }
            }
        }

        private WebBrowser CreateWB(string initial)
        {
            WebBrowser tn = new WebBrowser();
            tn.Dock = DockStyle.Fill;
            tn.Name = "Buscador";
            tn.TabIndex = 3;
            tn.ScriptErrorsSuppressed = true;
            tn.NewWindow += (se, o) =>
            {
                o.Cancel = true;
            };
            tn.Navigate(initial);
            /*Form1.form.Controls.Add(tn);
            tn.BringToFront();
            tn.Dock = DockStyle.Fill;*/
            return tn;
        }

        ~BrowserHandler() { }
    }
}
