using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace WebBrowser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string FilePath;
        public MainWindow()
        {
            InitializeComponent();
            FilePath= Directory.CreateDirectory("download").FullName+"/";
            Download();
        }

        private void GO_button_Click(object sender, RoutedEventArgs e)
        {
            string url = URL_textBox.Text.TrimStart();
            HTTP h = new HTTP();
            var response = h.Request(url, "GET");
            if (response!=null)
            {
            this.Body_textBlock.Text = response.Body;
            this.Head_textBlock.Text = response.Headers;
            }
        }

        void Download(string url= "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/logo_white.png")
        {
            string file = FilePath + url.Replace(':','_').Replace("/", "-");
            HTTP h = new HTTP();
            h.Download(url, file);
        }
    }
}
