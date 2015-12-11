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

namespace WebBrowser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HTTP h = new HTTP();
            string url= "https://www.nankai.edu.cn/";
            //var response = h.Request(url, "GET");
            //var t=response.Body;
            //this.Body_textBlock.Text = t;
            //this.richTextBox.AppendText (t);
            Uri u = new Uri(url);
            Console.WriteLine(u.Host);
            Console.WriteLine(u.Port);

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
    }
}
