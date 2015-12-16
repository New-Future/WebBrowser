using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using HtmlAgilityPack;
using Http;

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
            URL_textBox.Text = Url;
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
                MemoryStream ms = new MemoryStream();
                Encoding e = html.DeclaredEncoding ?? (Encoding.Default);
                html.Save(ms);
                this.richTextBox.Text = e.GetString(ms.ToArray());

            }

        }

        /// <summary>
        /// 回车更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void URL_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string url = URL_textBox.Text.Trim();
                update(url);
            }

        }
    }
}
