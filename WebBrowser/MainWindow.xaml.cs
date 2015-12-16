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
            FilePath = Directory.CreateDirectory("download").FullName + "/";
            var hp = new HtmlPage(FilePath);
            this.frame.Navigate(hp);
            this.SizeChanged += MainWindow_SizeChanged;
        }

        /// <summary>
        /// 窗口自适应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            frame.Height = e.NewSize.Height - 40;
            frame.Width = e.NewSize.Width - 20;
        }
    }
}
