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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Http;
using HtmlAgilityPack;
using System.IO;

namespace WebBrowser
{
    /// <summary>
    /// HtmlPage.xaml 的交互逻辑
    /// </summary>
    public partial class HtmlPage : Page
    {
        //public double ViewWidth { get; private set; }
        //public double ViewHeight { get; set; }

        string FilePath;
        string SavePath;
        Stack<string> urlList = new Stack<string>();
        Download download;
        /// <summary>
        /// 当前页面URL
        /// </summary>
        string Url
        {
            set { URL_textBox.Text = value; }
            get
            {
                string url = URL_textBox.Text.Trim();
                return url.StartsWith("http") ? url : "http://" + url;
            }
        }
        /// <summary>
        /// 浏览界面高度
        /// </summary>
        double ViewHeight
        {
            set { this.wb.Height = value - 30; }
        }
        /// <summary>
        /// 浏览界面宽度
        /// </summary>
        double ViewWeight
        {
            set { this.wb.Width = value - 2; }
        }

        Response response;

        public HtmlPage(string storePath = "")
        {
            InitializeComponent(); ;
            this.SizeChanged += HtmlPage_SizeChanged;
            wb.Navigating += Wb_Navigating;
            wb.ContextMenuOpening += Wb_ContextMenuOpening;
            FilePath = storePath;
            Url = "http://www.nankai.edu.cn/";

        }

        private void Wb_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            //e.
            //throw new NotImplementedException();
        }

        private void HtmlPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewHeight = e.NewSize.Height;
            ViewWeight = e.NewSize.Width;
        }

        /// <summary>
        /// 导航拦截
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wb_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null && !e.Uri.IsLoopback)
            {
                e.Cancel = true;
                navigate(e.Uri.ToString());
            }
            else
            {
                if (e.Uri != null)
                {
                    urlList.Push(e.Uri.AbsolutePath);
                }
            }

        }

        /// <summary>
        /// 浏览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GO_button_Click(object sender, RoutedEventArgs e)
        {
            this.navigate(Url);
            urlList.Push(Url);
        }

        /// <summary>
        /// 新的链接
        /// </summary>
        /// <param name="url"></param>
        private void navigate(string url, bool noCache = false)
        {
            this.Url = url;
            SavePath = Download.GetFileNameFromUrl(Url, FilePath);
            if (noCache || SavePath == null || !File.Exists(SavePath))
            {
                download = new Download(url, FilePath);
                response = download.response;
            }
            var f = File.OpenText(SavePath);
            wb.NavigateToString(f.ReadToEnd());
            f.Close();
        }


        /// <summary>
        /// 查看源码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sourceBtn_Click(object sender, RoutedEventArgs e)
        {
            HTTP http = new HTTP();
            var response = http.Request(Url);
            if (response != null)
            {
                var sourceWindow = new SourceWindow("查看源码-" + Url, response);
                sourceWindow.Show();
            }
            else
            {
                MessageBox.Show(http.LastError, "请求异常");
            }

        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.SavePath == null || !File.Exists(SavePath))
            {
                this.navigate(Url);
            }
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select,\"" + SavePath + "\"";
            System.Diagnostics.Process.Start(psi);
        }

        /// <summary>
        /// 回退
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //urlList.Count
            if (urlList.Count > 0)
            {
                navigate(urlList.Pop());
            }
            //if (this.wb.CanGoBack)
            //{
            //    this.wb.GoBack();
            //}
        }

        /// <summary>
        /// 下一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (this.wb.CanGoForward)
            {
                this.wb.GoForward();
            }
        }

        private void TextView(object sender, RoutedEventArgs e)
        {
            textView t = new textView(Url);
            t.Show();
        }
    }
}
