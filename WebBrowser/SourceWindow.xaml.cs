using System.Windows;
using Http;

namespace WebBrowser
{
    /// <summary>
    ///查看源码页
    /// </summary>
    public partial class SourceWindow : Window
    {
        public SourceWindow(string title = "源码查看", Response response = null)
        {
            InitializeComponent();
            this.Title = title;//标题
            if (response != null)
            {
                this.Head_textBlock.Text = response.Headers;//头
                this.Body_textBlock.Text = response.Body;//数据
            }
            this.AllowDrop = true;
        }
    }
}
