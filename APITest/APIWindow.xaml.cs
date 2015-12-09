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
using WebBrowser;

namespace APITest
{
    /// <summary>
    /// APIWindow.xaml 的交互逻辑
    /// </summary>
    public partial class APIWindow : Window
    {
        private string method = "GET";
        public APIWindow()
        {
            InitializeComponent();
            radioButtonGet.IsChecked = true;
        }


        /// <summary>
        /// 获取输入的头
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetAddHeaders()
        {
            string[] allHeaders = this.addHead.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> Header = new Dictionary<string, string>();
            foreach (var head in allHeaders)
            {
                var index = head.IndexOf(':');
                if (index > 0)
                {
                    var key = head.Substring(0, index);
                    var value = head.Substring(index + 1).Trim();
                    if (Header.ContainsKey(key))
                    {
                        //重复键值覆盖
                        Header[key] = value;
                    }
                    else
                    {
                        Header.Add(key, value);
                    }
                }
            }
            return Header;
        }

        /// <summary>
        /// 获取输入字段
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetFields()
        {
            string[] allfields = this.addField.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> Fields = new Dictionary<string, string>();
            foreach (var field in allfields)
            {
                var index = field.IndexOf('=');
                if (index > 0)
                {
                    var key = field.Substring(0, index);
                    var value = field.Substring(index + 1).Trim();
                    if (Fields.ContainsKey(key))
                    {
                        //重复键值覆盖
                        Fields[key] = value;
                    }
                    else
                    {
                        Fields.Add(key, value);
                    }
                }
            }
            return Fields;
        }
        /// <summary>
        /// 点击发送按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = URLBox.Text.TrimStart();
            Uri uri = new Uri(url);
            HTTP http = new HTTP();
      
            /*添加自定义头*/
            var addHeaders = GetAddHeaders();
            foreach (var item in addHeaders)
            {
                http.AddHeader(item.Key, item.Value);
            }
            /*自定义字段*/
            var Data = GetFields();
            Response response;
            if (radioButtonGet.IsChecked==true)
            {
                //GET请求
                response = http.GET(url, Data);

            }else if(true==radioButtonHead.IsChecked)
            {
                //HEAD请求
                response = http.HEAD(url);
            }else if(true== radioButtonPost.IsChecked)
            {
                //POST请求
                response = http.POST(url, Data);
            }else if(true == radioButtonPut.IsChecked)
            {
                //PUT请求
                response = http.PUT(url, Data);
            }
            else if(true == radioButtonDelete.IsChecked)
            {
                response = http.DELETE(url);
            }
            else
            {
                http.Method = method;
                response = http.Request(url);
            }

            SendText.Text = http.SendData;
            if (response == null)
            {
                MessageBox.Show(http.LastError, "请求异常");
            }
            else
            {
                HeadText.Text = response.Headers;
                BodyText.Text = response.Body;
            }
        }
        /// <summary>
        /// 绑定回车事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void URLBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBtn_Click(sender, null);
            }
        }

        /// <summary>
        /// 切换方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_Change(object sender, RoutedEventArgs e)
        {
            method = (sender as RadioButton).Content.ToString();
            //POST和GET,PUT可以设置字段，其他不可以
            if(method=="GET"||method=="POST"||method=="PUT")
            {
                addField.IsEnabled = true;
            }
            else
            {
                addField.IsEnabled = false;
            }
        }

       
    }
}
