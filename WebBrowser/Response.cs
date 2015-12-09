using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser
{
    /// <summary>
    /// 响应数据
    /// </summary>
    public class Response
    {
        /// <summary>
        /// 行切割参数
        /// </summary>
        public static readonly string[] LINE_SEPARATOR = { HTTP.CRLF };

        /// <summary>
        /// http状态代码
        /// </summary>
        private int code;
        public int Code
        {
            get { return code; }
            private set { code = value; }
        }
        private string status;
        public string Status
        {
            get { return Code + status; }
        }

        /// <summary>
        /// 响应头
        /// </summary>
        public string Headers { get; private set; }//字符串头
        Dictionary<string, string> responserHeaders = new Dictionary<string, string>();
        public Dictionary<string, string> Header//键值对
        {
            get { return responserHeaders; }
            set { responserHeaders = value; }
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        string responserText;
        public string Body
        {
            get { return responserText; }
            set { responserText = value; }
        }

        /// <summary>
        /// 构建响应数据
        /// </summary>
        /// <param name="result">响应数据字符</param>
        public Response(string result)
        {
            int headerIndex = result.IndexOf(HTTP.END_P);
            if (headerIndex > 0)
            {
                Headers = result.Substring(0, headerIndex);
                this.ParseHeader(Headers);
                this.ParseBody(result.Substring(headerIndex + HTTP.END_P.Length));
            }
            else
            {
                this.Body = result;
            }
        }

        /// 构建响应数据
        public Response()
        {
        }


        /// <summary>
        /// 获取字符集
        /// </summary>
        /// <returns></returns>
        public string GetCharset()
        {
            if (Header.ContainsKey("Content-Type"))
            {
                string content = Header["Content-Type"];
                int index = content.LastIndexOf("charset=", StringComparison.CurrentCultureIgnoreCase);
                if (index > 0)
                {
                    return content.Substring(index + 8).Trim(new char[] { ' ', ';', '\n', '\r' }).ToUpper();
                }
            }
            return null;
        }
        /// <summary>
        /// 解析头部
        /// </summary>
        /// <param name="headerString">header字符串</param>
        /// <returns>是否解析成功</returns>
        private bool ParseHeader(string headerString = null)
        {
            string[] allHeaders = headerString.Split(LINE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            /*判断响应状态*/
            string[] httpstatus = allHeaders[0].Split(new string[] { " " }, 3, StringSplitOptions.RemoveEmptyEntries);
            if (httpstatus.Length >= 2 && int.TryParse(httpstatus[1], out this.code))
            {
                if (httpstatus.Length == 3)
                {
                    //响应消息如 OK
                    this.status = httpstatus[2];
                }
            }
            else
            {
                this.Code = 400;
                return false;
            }
            /*解析请求头*/
            for (int i = 1; i < allHeaders.Length; i++)
            {
                var index = allHeaders[i].IndexOf(HTTP.KEY_SEPARATOR);
                if (index > 0)
                {
                    var key = allHeaders[i].Substring(0, index);
                    var value = allHeaders[i].Substring(index + HTTP.KEY_SEPARATOR.Length);
                    if (this.Header.ContainsKey(key))
                    {
                        //重复键值覆盖
                        this.Header[key] = value;
                    }
                    else
                    {
                        Header.Add(key, value);
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// 解析头部
        /// </summary>
        /// <param name="headerString">header字符串</param>
        /// <returns>是否解析成功</returns>
        private void ParseBody(string bodyString = null)
        {
            if (this.Header.ContainsKey("Transfer-Encoding") && this.Header["Transfer-Encoding"] == "chunked")
            {
                /*分段传输*/
                this.Body = "";

                int last = 0;
                int i = bodyString.IndexOf(HTTP.CRLF, last);
                int len;
                do
                {
                    //逐个chunked处理
                    var s = bodyString.Substring(last, i - last);
                    last = i + HTTP.CRLF.Length;
                    len = Convert.ToInt32(s, 16);
                    if (len > 0)
                    {
                        var temp = bodyString.Substring(last, len);
                        this.Body += convert(temp);
                        last += len + HTTP.CRLF.Length;
                    }
                    else
                    {
                        break;
                    }
                    i = bodyString.IndexOf(HTTP.CRLF, last);
                } while (i > last);

            }
            else
            {//转换编码
                this.Body = convert(bodyString);
            }
        }

        /// <summary>
        /// 转换编码,默认UTF8
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns></returns>
        private string convert(string s)
        {
            string charset = GetCharset() ?? "UTF-8";
            Encoding encode = Encoding.GetEncoding(charset);
            return encode.GetString(s.ToString().ToCharArray().Select(b => (byte)b).ToArray());
        }
    }
}
