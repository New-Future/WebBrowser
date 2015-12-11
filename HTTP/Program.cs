using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HTTP
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.CookieContainer c = new System.Net.CookieContainer();
            HttpHeader h = new HttpHeader("https://www.baidu.com:443/");
            h.Host = "www.baidu.com";
            //h.
            //h.Url=""
            //var s = HttpHelper.Get("https://www.baidu.com", h);//
            //Console.WriteLine(s);
            //Console.Read();
            X509CertificateCollection xcert = new X509Certificate2Collection();
            var r = ssl.InternalSslSocketHttp("GET","https://www.baidu.com:443/",h,xcert);
            Console.WriteLine(r.Header);
            Console.Read();
        }
    }
}
