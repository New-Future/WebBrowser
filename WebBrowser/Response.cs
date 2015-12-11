/**
Response 
HTPP响应处理类
支持stream流和string数据创建
*属性
 Dictionary Header响应头字典(键转大写)
 string Body数据
 string Headers响应头字符串
 byte[] Data 响应数据字节流
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
        public byte[] Data { get; private set; }
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
            int headerIndex = result.IndexOf(HTTP.END);
            if (headerIndex > 0)
            {
                Headers = result.Substring(0, headerIndex);
                this.ParseHeader(Headers);
                this.ParseBody(result.Substring(headerIndex + HTTP.END.Length));
            }
            else
            {
                this.Body = result;
            }
        }

        /// <summary>
        /// 通过字节流创建
        /// </summary>
        /// <param name="responseStream"></param>
        public Response(Stream responseStream)
        {

            //解析头
            Headers = ReadHead(responseStream);
            if (this.ParseHeader(Headers))
            {
                ReadBody(responseStream);
                //判断文件格式
                string charset = GetCharset() ?? "UTF-8";
                Encoding encode = Encoding.GetEncoding(charset);
                this.Body = encode.GetString(Data);
            }
            else
            {
                this.Body = Headers;
            }
            responseStream.Close();
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

        public bool Save(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                if (Data != null)
                {
                    fs.Write(Data, 0, Data.Length);
                }
                else
                {
                    string charset = GetCharset() ?? "UTF-8";
                    var bytes = Encoding.GetEncoding(charset).GetBytes(Body);
                    fs.Write(bytes, 0, bytes.Length);
                }
                fs.Close();
                return true;
            }catch(Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// 从字节流中读取响应头
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string ReadHead(Stream stream)
        {
            StringBuilder header = new StringBuilder(128);
            int currentByte = -1;
            /*读取header*/
            try
            {
                while (stream.CanRead)
                {
                    currentByte = stream.ReadByte();
                    if (currentByte == -1)
                    {
                        break;//读到数据流末尾
                    }
                    else
                    {
                        header.Append((char)currentByte);
                        if (header.ToString().EndsWith(HTTP.END))
                        {
                            break;//读取到结束符
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            string Headers = header.ToString();
            if (Headers.StartsWith("HTTP/1.1 100"))
            {
                //HTTP 100 continue响应继续
                return ReadHead(stream);
            }
            else
            {
                return Headers.Trim();
            }
        }

        /// <summary>
        /// 读取响应正文数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private bool ReadBody(Stream stream)
        {
            int len = 0;
            if (Header.ContainsKey("CONTENT-LENGTH") && int.TryParse(Header["CONTENT-LENGTH"], out len))
            {
                //指定了长度
                this.Data = ReadStream(stream, len);
            }
            else if (Header.ContainsKey("TRANSFER-ENCODING") && Header["TRANSFER-ENCODING"].ToUpper() == "CHUNKED")
            {
                //分段下载
                int chucked = GetChunked(stream);
                while (chucked > 0)
                {
                    Data = Data == null ? ReadStream(stream, chucked) : Data.Concat(ReadStream(stream, chucked)).ToArray();
                    chucked = GetChunked(stream);
                }
            }
            else
            {
                //未指定
                int bufflen = 102400;

                byte[] buff;// = new byte[bufflen];
                do
                {
                    buff = ReadStream(stream, bufflen);
                    Data = Data == null ? buff : Data.Concat(buff).ToArray();
                } while (buff.Length < bufflen);
            }


            return true;
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
                    var key = allHeaders[i].Substring(0, index).Trim().ToUpper();
                    var value = allHeaders[i].Substring(index + HTTP.KEY_SEPARATOR.Length).TrimStart();
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

        /// <summary>
        /// 读取chunked值
        /// </summary>
        /// <param name="sm"></param>
        /// <returns></returns>
        static int GetChunked(Stream sm)
        {
            int chunked = 0;
            StringBuilder bulider = new StringBuilder();
            int read = 0;
            //去掉前面的换行
            do
            {
                read = sm.ReadByte();
            } while (HTTP.CRLF.Contains((char)read));
            //读取主体内容
            while (read > 0)
            {
                try
                {
                    var temp = bulider.Append((char)read).ToString();
                    if (read == '\n' && temp.EndsWith(HTTP.CRLF))
                    {
                        int index = temp.IndexOf(';');
                        if (index > 0)
                        {
                            temp = temp.Substring(0, index);
                        }
                        chunked = Convert.ToInt32(temp.Trim(), 16);
                        break;
                    }
                    read = sm.ReadByte();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
            }
            return chunked;
        }

        /// <summary>
        /// 读取数据流中指定长度
        /// </summary>
        /// <param name="s">数据流</param>
        /// <param name="len">长度</param>
        /// <returns></returns>
        public static byte[] ReadStream(Stream s, int len)
        {
            byte[] buff = new byte[len];
            var l = s.Read(buff, 0, len);
            while (l > 0 && l < len)
            {
                var t = s.Read(buff, l, len - l);
                if (t < 0)
                {
                    break;
                }
                l += t;
            }
            return buff.Take(l).ToArray();
        }
    }
}
