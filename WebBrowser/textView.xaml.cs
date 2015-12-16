using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Http;
using HtmlAgilityPack;
using System.IO;

namespace WebBrowser
{
    /// <summary>
    /// textView.xaml 的交互逻辑
    /// </summary>
    public partial class textView : Window
    {
        public textView(string Url)
        {
            InitializeComponent();

            update(Url);
        }

        public void update(string Url)
        {

            HTTP h = new HTTP();
            Uri uri = new Uri(Url);
            Response response = h.GET(Url);
            if (response != null)
            {
                HtmlDocument html = new HtmlDocument();
                var charset = response.GetCharset();
                var encode = charset != null ? Encoding.GetEncoding(charset) : Encoding.UTF8;
                html.LoadHtml(response.Body);
                //下载图片
                var imgs = html.DocumentNode.SelectNodes("//img[@src]");
                if (imgs != null)
                {
                    foreach (var item in imgs)
                    {
                        var url = item.Attributes["src"].Value;
                        url = Download.GetAbsUri(url, uri);
                        item.Attributes["src"].Value = url;
                    }
                }

                //下载JS脚本
                var scripts = html.DocumentNode.SelectNodes("//script[@src]");
                if (scripts != null)
                {
                    foreach (var item in scripts)
                    {
                        var link = item.Attributes["src"];
                        link.Value = Download.GetAbsUri(link.Value, uri);
                    }
                }

                //下载CSS
                var css = html.DocumentNode.SelectNodes("//link[@href]");
                if (css != null)
                {
                    foreach (var item in css)
                    {
                        var link = item.Attributes["href"];
                        link.Value = Download.GetAbsUri(link.Value, uri);
                    }
                }

                //超链接换成绝对链接
                var links = html.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var item in links)
                    {
                        if (item.Attributes["target"] != null)
                            item.Attributes["target"].Value = "_self";
                        var link = item.Attributes["href"];
                        link.Value = Download.GetAbsUri(link.Value, uri);
                    }
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    html.Save(ms);
                    this.richTextBox.Text = UTF8Encoding.Default.GetString(ms.ToArray());
                }

            }
            if (response != null)
            {
                var html = response.Body;
                this.richTextBox.Text = html;
            }
        }
    }
}
