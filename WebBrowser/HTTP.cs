using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace WebBrowser
{
    /// <summary>
    /// HTTP 请求类型
    /// GET POST PUT DELETE HEAD
    /// </summary>
    public class HTTP
    {
        #region 分隔符等常量
        public const string CRLF = "\r\n";//换行符
        public const string END_P = "\r\n\r\n";//段落换行
        public const string KEY_SEPARATOR = ": ";// 响应头字段分隔符
        private string vesion = "1.1";
        #endregion

        /// <summary>
        /// TCP链接
        /// </summary>
        private TcpClient clientSocket = new TcpClient();

        #region 连接设置

        /// <summary>
        /// 请求头
        /// </summary>
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        public Dictionary<string, string> Header
        {
            get { return _headers; }
            private set { _headers = value; }
        }

        /// <summary>
        /// 请求头
        /// </summary>
        private string _method = "GET";
        public string Method
        {
            get { return _method; }
            set { _method = value.ToUpper(); }
        }

        /// <summary>
        /// cookie数据
        /// </summary>
        private string _cookie;
        public string Cookie
        {
            get { return _cookie; }
            private set { _cookie = value; }
        }

        #endregion

        ///// <summary>
        ///// 编码格式
        ///// </summary>
        //private Encoding _encode = Encoding.UTF8;
        //public Encoding Encode
        //{
        //    get { return _encode; }
        //    set { _encode = value; }
        //}

        /// <summary>
        /// 错误信息
        /// </summary>
        public string LastError { get; private set; }
        public string SendData { get; private set; }

        public HTTP(string userAgent = "NK Browser/1.0 (By NewFuture )")
        {
            this.Header["User-Agent"] = userAgent;
            this.Header["Accept"] = "*/*";
        }
        /// <summary>
        /// 发起请求
        /// </summary>
        /// <param name="_url"></param>
        /// <param name="_type"></param>
        /// <param name="_postdata"></param>
        /// <param name="_cookie"></param>
        /// <returns></returns>
        public Response Request(string _url, string _postdata = "")
        {
            try
            {
                Uri URI = new Uri(_url);
                AddHeader("Host", URI.Host);//设定主机
                clientSocket.Connect(URI.Host, URI.Port);//连接主机

                string requestHeader = BuildHeader(URI.PathAndQuery);//构建请求数据
                SendData = requestHeader + _postdata;
                byte[] request = Encoding.UTF8.GetBytes(SendData);
                clientSocket.Client.Send(request);//发送请求数据

                /*接收数据*/
                byte[] responseByte = new byte[1024000];
                int len = clientSocket.Client.Receive(responseByte);
                string result = "";
                do
                {
                    //分段传输全部下载
                    result += new String(responseByte.Take(len).Select((x) => (char)x).ToArray());
                    len = clientSocket.Client.Receive(responseByte);
                } while (len > 0);

                /*关闭连接*/
                clientSocket.Close();
                return new Response(result);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
#endif
                LastError = e.Message;
                return null;
            }
        }


        /// <summary>
        /// URL请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public Response GET(string url, object s = null)
        {
            Method = "GET";
            string q = BuildQuery(s);
            if (!String.IsNullOrWhiteSpace(q))
            {
                int index = url.IndexOf('?');
                if (index > 0)
                {
                    url = url.TrimEnd('&') + '&' + q;
                }
                else
                {
                    url = url + '?' + q;
                }
            }
            return Request(url);
        }
        public Response HEAD(string url)
        {
            Method = "HEAD";
            return Request(url);
        }
        public Response DELETE(string url)
        {
            Method = "DELETE";
            return Request(url);
        }
        public Response POST(string url, Dictionary<string, string> s = null)
        {
            Method = "POST";
            string q = BuildQuery(s);
            AddHeader("Content-Type", "application/x-www-form-urlencoded");
            AddHeader("Content-Length", q.Length.ToString());
            Response r = Request(url, q);
            return r;
        }
        public Response PUT(string url, Dictionary<string, string> s = null)
        {
            Method = "PUT";
            string q = BuildQuery(s);
            AddHeader("Content-Type", "application/x-www-form-urlencoded");
            AddHeader("Content-Length", q.Length.ToString());
            Response r = Request(url, q);
            return r;
        }


        /// <summary>
        /// 添加和设置Header内容
        /// </summary>
        /// <param name="key">字段</param>
        /// <param name="value">对应的值</param>
        /// <returns>HTTP自身</returns>
        public HTTP AddHeader(string key, string value)
        {
            if (this.Header.ContainsKey(key))
            {
                //重复键值覆盖
                this.Header[key] = value;
            }
            else
            {
                Header.Add(key, value);
            }
            return this;
        }

        /// <summary>
        /// 构建query字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string BuildQuery(object data = null)
        {
            string q = "";
            if (data == null)
            {
                return null;
            }
            else if (data is Dictionary<string, string>)
            {
                foreach (var item in (data as Dictionary<string, string>))
                {
                    q += item.Key + "=" + Uri.EscapeDataString(item.Value) + "&";
                }
                q = q.TrimEnd('&');
            }
            else
            {
                q = data.ToString();
            }
            return q;

        }

        /// <summary>
        /// 构建HTTP请求头
        /// </summary>
        /// <param name="path"></param>
        /// <returns>请求路径</returns>
        private string BuildHeader(string path = "/")
        {
            //请求方式和请求路径
            StringBuilder headersBuilder = new StringBuilder(this.Method + " " + path + " HTTP/" + this.vesion + HTTP.CRLF);
            foreach (var item in this.Header)
            {
                //头数据
                headersBuilder.Append(item.Key + HTTP.KEY_SEPARATOR + item.Value + HTTP.CRLF);
            }
            headersBuilder.Append("Connection: close" + HTTP.END_P);
            return headersBuilder.ToString();
        }

        //URL编码
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }
    }
}
