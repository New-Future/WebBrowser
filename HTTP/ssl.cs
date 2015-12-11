using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HTTP
{
    class ssl
    {
        static byte[] ParseHttpHeaderToBytes(string method, HttpHeader header)
        {
            StringBuilder bulider = new StringBuilder();
            if (method.Equals("POST"))
            {
                bulider.AppendLine(string.Format("POST {0} HTTP/1.1",
                    header.Url));
                bulider.AppendLine("Content-Type: application/x-www-form-urlencoded");
            }
            else
            {
                bulider.AppendLine(string.Format("GET {0} HTTP/1.1",
                header.Url));
            }
            if (!string.IsNullOrEmpty(header.Host))
                bulider.AppendLine(string.Format("Host: {0}",
                    header.Host));
            bulider.AppendLine("User-Agent: Mozilla/5.0 (Windows NT 6.1; IE 9.0)");
            if (!string.IsNullOrEmpty(header.Referer))
                bulider.AppendLine(string.Format("Referer: {0}",
                    header.Referer));
            bulider.AppendLine("Connection: keep-alive");
            if (!string.IsNullOrEmpty(header.Accept))
            {
                bulider.AppendLine(string.Format("Accept: {0}",
                 header.Accept));
            }
            else
            {
                bulider.AppendLine("Accept: */*");
            }
            if (!string.IsNullOrEmpty(header.Cookies))
                bulider.AppendLine(string.Format("Cookie: {0}",
                    header.Cookies));
            if (method.Equals("POST"))
            {
                bulider.AppendLine(string.Format("Content-Length: {0}\r\n",
                   Encoding.Default.GetBytes(header.Body).Length));
                bulider.Append(header.Body);
            }
            else
            {
                bulider.Append("\r\n");
            }
            return Encoding.Default.GetBytes(bulider.ToString());
        }

        public static HttpResponse InternalSslSocketHttp(string method,
            string url,
             HttpHeader header,
             X509CertificateCollection x509certs)
        {
            HttpResponse response = null;
            try
            {
                TcpClient tcp = new TcpClient();
                Uri u = new Uri(url);
                tcp.Connect(u.Host, u.Port);
                if (tcp.Connected)
                {
                    byte[] buff = ParseHttpHeaderToBytes(method, header);  //生成协议包
                    if (x509certs != null)
                    {
                        using (SslStream ssl = new SslStream(tcp.GetStream(),
                                                false,
                                                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                                                null))
                        {
                            ssl.AuthenticateAsClient("SslServerName",
                                x509certs,
                                SslProtocols.Tls,
                                false);
                            if (ssl.IsAuthenticated)
                            {
                                ssl.Write(buff);
                                ssl.Flush();
                                response = ReadResponse(ssl);
                            }
                        }
                    }
                    else
                    {
                        using (NetworkStream ns = tcp.GetStream())
                        {
                            ns.Write(buff, 0, buff.Length);
                            ns.Flush();
                            response = ReadResponse(ns);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return response;
        }




        static bool ValidateServerCertificate(
                 object sender,
                 X509Certificate certificate,
                 X509Chain chain,
                 SslPolicyErrors sslPolicyErrors)
        {
            /*
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
            */
            return true;
        }

        class TaskArguments
        {
            public TaskArguments(CancellationTokenSource cancelSource, Stream sm)
            {
                this.CancelSource = cancelSource;
                this.Stream = sm;
            }
            public CancellationTokenSource CancelSource { get; private set; }
            public Stream Stream { get; private set; }
        }

        static string ReadHeaderProcess(object args)
        {
            TaskArguments argument = args as TaskArguments;
            StringBuilder bulider = new StringBuilder();
            if (argument != null)
            {
                Stream sm = argument.Stream;
                while (!argument.CancelSource.IsCancellationRequested)
                {
                    try
                    {
                        int read = sm.ReadByte();
                        if (read != -1)
                        {
                            byte b = (byte)read;
                            bulider.Append((char)b);
                            string temp = bulider.ToString();
                            if (temp.EndsWith("\r\n\r\n"))//Http协议头尾
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        break;
                    }
                }
            }
            return bulider.ToString();
        }
        private static HttpResponse ReadResponse(Stream sm)
        {
            HttpResponse response = null;
            CancellationTokenSource cancelSource = new CancellationTokenSource();
            Task<string> myTask = Task.Factory.StartNew<string>(
                new Func<object, string>(ReadHeaderProcess),
                new TaskArguments(cancelSource, sm),
                cancelSource.Token);
            if (myTask.Wait(3 * 1000)) //尝试3秒时间读取协议头
            {
                string header = myTask.Result;
                if (!string.IsNullOrEmpty(header))
                {
                    if (header.StartsWith("HTTP/1.1 100"))
                    {
                        return ReadResponse(sm);
                    }
                    byte[] buff = null;
                    int start = header.ToUpper().IndexOf("CONTENT-LENGTH");
                    int content_length = -1;  //fix bug
                    if (start > 0)
                    {
                        string temp = header.Substring(start, header.Length - start);
                        string[] sArry = Regex.Split(temp, "\r\n");
                        content_length = Convert.ToInt32(sArry[0].Split(':')[1]);
                        if (content_length > 0)
                        {
                            buff = new byte[content_length];
                            int inread = sm.Read(buff, 0, buff.Length);
                            while (inread < buff.Length)
                            {
                                inread += sm.Read(buff, inread, buff.Length - inread);
                            }
                        }
                    }
                    else
                    {
                        start = header.ToUpper().IndexOf("TRANSFER-ENCODING: CHUNKED");
                        if (start > 0)
                        {
                            buff = ChunkedReadResponse(sm);
                        }
                        else
                        {
                            buff = SpecialReadResponse(sm);//例外
                        }
                    }
                    response = new HttpResponse(header, buff);
                }
            }
            else
            {
                cancelSource.Cancel(); //超时的话，别忘记取消任务哦
            }
            return response;
        }

        static byte[] ChunkedReadResponse(Stream sm)
        {
            ArraySegmentList<byte> arraySegmentList = new ArraySegmentList<byte>();
            int chunked = GetChunked(sm);
            while (chunked > 0)
            {
                byte[] buff = new byte[chunked];
                try
                {
                    int inread = sm.Read(buff, 0, buff.Length);
                    while (inread < buff.Length)
                    {
                        inread += sm.Read(buff, inread, buff.Length - inread);
                    }
                    arraySegmentList.Add(new ArraySegment<byte>(buff));
                    if (sm.ReadByte() != -1)//读取段末尾的\r\n
                    {
                        sm.ReadByte();
                    }
                }
                catch (Exception)
                {
                    break;
                }
                chunked = GetChunked(sm);
            }
            return arraySegmentList.ToArray();
        }

        class ArraySegmentList<T>
        {
            List<ArraySegment<T>> m_SegmentList = new List<ArraySegment<T>>();
            public ArraySegmentList() { }

            int m_Count = 0;
            public void Add(ArraySegment<T> arraySegment)
            {
                m_Count += arraySegment.Count;
                m_SegmentList.Add(arraySegment);
            }

            public T[] ToArray()
            {
                T[] array = new T[m_Count];
                int index = 0;
                for (int i = 0; i < m_SegmentList.Count; i++)
                {
                    ArraySegment<T> arraySegment = m_SegmentList[i];
                    Array.Copy(arraySegment.Array,
                        0,
                        array,
                        index,
                        arraySegment.Count);
                    index += arraySegment.Count;
                }
                return array;
            }
        }
        static int GetChunked(Stream sm)
        {
            int chunked = 0;
            StringBuilder bulider = new StringBuilder();
            while (true)
            {
                try
                {
                    int read = sm.ReadByte();
                    if (read != -1)
                    {
                        byte b = (byte)read;
                        bulider.Append((char)b);
                        string temp = bulider.ToString();
                        if (temp.EndsWith("\r\n"))
                        {
                            chunked = Convert.ToInt32(temp.Trim(), 16);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    break;
                }
            }
            return chunked;
        }

        /*
        * 注意：该方法仅供测试，实际使用时请根据需要定制
          */
        static byte[] SpecialReadResponse(Stream sm)
        {
            ArrayList array = new ArrayList();
            StringBuilder bulider = new StringBuilder();
            int length = 0;
            DateTime now = DateTime.Now;
            while (true)
            {
                byte[] buff = new byte[1024 * 10];
                int len = sm.Read(buff, 0, buff.Length);
                if (len > 0)
                {
                    length += len;
                    byte[] reads = new byte[len];
                    Array.Copy(buff, 0, reads, 0, len);
                    array.Add(reads);
                    bulider.Append(Encoding.Default.GetString(reads));
                }
                string temp = bulider.ToString();
                if (temp.ToUpper().Contains("</HTML>"))
                {
                    break;
                }
                if (DateTime.Now.Subtract(now).TotalSeconds >= 30)
                {
                    break;//超时30秒则跳出
                }
            }
            byte[] bytes = new byte[length];
            int index = 0;
            for (int i = 0; i < array.Count; i++)
            {
                byte[] temp = (byte[])array[i];
                Array.Copy(temp, 0, bytes,
                    index, temp.Length);
                index += temp.Length;
            }
            return bytes;
        }
    }


}
