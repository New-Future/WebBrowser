using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Http;

namespace WebBrowser
{
    /// <summary>
    /// HtmlPage.xaml 的交互逻辑
    /// </summary>
    public partial class HtmlPage : Page
    {
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

        public HtmlPage(string storePath = "")
        {
            InitializeComponent(); ;
            this.SizeChanged += HtmlPage_SizeChanged;//窗口大小自适应
            wb.Navigating += Wb_Navigating;//导航拦截
            URL_textBox.KeyDown += URL_textBox_KeyDown;//回车键
            FilePath = storePath;
            Url = "https://www.zhihu.com/";
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
            }
            if (File.Exists(SavePath))
            {
                var f = File.OpenText(SavePath);
                wb.NavigateToString(f.ReadToEnd());
                f.Close();
            }
        }

        /// <summary>
        /// 回车键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void URL_textBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GO_button_Click(null, null);
            }
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
            if (urlList.Count > 0)
            {
                navigate(urlList.Pop());
            }
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

        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextView(object sender, RoutedEventArgs e)
        {
            textView t = new textView(Url);
            t.Show();
        }

        /// <summary>
        /// 大小自动调整
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HtmlPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewHeight = e.NewSize.Height;
            ViewWeight = e.NewSize.Width;
        }
    }
}
