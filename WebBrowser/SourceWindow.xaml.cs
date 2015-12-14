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

namespace WebBrowser
{
    /// <summary>
    /// SourceWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SourceWindow : Window
    {
        public SourceWindow(string title = "源码查看", Response response = null)
        {
            InitializeComponent();
            this.Title = title;
            if (response != null)
            {
                this.Head_textBlock.Text = response.Headers;
                this.Body_textBlock.Text = response.Body;
            }
            this.AllowDrop=true;
    }
    }
}
