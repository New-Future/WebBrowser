using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;
using HtmlAgilityPack;
using System.IO;

namespace WebBrowser
{
    class Download
    {
        public static string Folder { get; private set; }
        public string IndexFile { get; private set; }
        public HtmlDocument html = new HtmlDocument();
        public Response response { get; private set; }
        public List<string> fileList = new List<string>();

        public Download(string url, string folder)
        {
            Folder = folder;
            Start(url);
        }


        public Download(string folder)
        {
            Folder = folder;
        }

        /// <summary>
        /// 根据URL创建并获取文件名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFileNameFromUrl(string url, string Folder = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }
            url = url.Replace("://", "-").Replace(':', '_').Replace('?', '-').Replace('&', '+').TrimEnd('/');
            if (url.IndexOf('/') < 0)
            {
                url += "/index.html";
            }
            else if (url.Last() == '/')
            {
                url += "index.html";
            }
            Folder = Folder ?? Download.Folder;
            string path = Folder + "/" + url;
            string folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public Download Start(string baseUrl)
        {

            HTTP http = new HTTP();
            Uri uri;
            try
            {
                uri = new Uri(baseUrl);
                response = http.Request(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }


            if (response != null)
            {
                IndexFile = GetFileNameFromUrl(baseUrl);
                var charset = response.GetCharset();
                var encode = charset != null ? Encoding.GetEncoding(charset) : Encoding.UTF8;
                html.LoadHtml(response.Body);
                //下载图片
                var imgs = html.DocumentNode.SelectNodes("//img[@src]");
                if (imgs != null)
                {
                    foreach (var item in imgs)
                    {
                        var url = item.Attributes["src"].Value;
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            url = GetAbsUri(url, uri);
                            item.Attributes["src"].Value = GetFileNameFromUrl(url);
                            fileList.Add(url);
                            if (item.Attributes.Contains("title"))
                            {
                                item.Attributes["title"].Value = url;
                            }
                            else
                            {
                                item.SetAttributeValue("title", url);
                            }

                        }
                    }
                }

                //下载JS脚本
                var scripts = html.DocumentNode.SelectNodes("//script[@src]");
                if (scripts != null)
                {
                    foreach (var item in scripts)
                    {
                        var link = item.Attributes["src"];
                        if (!string.IsNullOrWhiteSpace(link.Value))
                        {
                            var url = GetAbsUri(link.Value, uri);
                            link.Value = GetFileNameFromUrl(url);
                            fileList.Add(url);
                        }
                    }
                }

                //下载CSS
                var css = html.DocumentNode.SelectNodes("//link[@href]");
                if (css != null)
                {
                    foreach (var item in css)
                    {
                        var link = item.Attributes["href"];
                        if (!string.IsNullOrWhiteSpace(link.Value))
                        {
                            link.Value = GetAbsUri(link.Value, uri);
                            var url = GetAbsUri(link.Value, uri);
                            link.Value = GetFileNameFromUrl(url);
                            fileList.Add(url);
                        }
                    }
                }

                //超链接换成绝对链接
                var links = html.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var item in links)
                    {
                        if (item.Attributes["target"] != null)
                        {
                            item.Attributes["target"].Value = "_self";
                        }
                        var link = item.Attributes["href"];
                        if (!string.IsNullOrWhiteSpace(link.Value))
                        {
                            link.Value = GetAbsUri(link.Value, uri);
                        }
                    }
                }

                //TODO
                //http.AddHeader('REF')
                //COOKIE
                foreach (var item in fileList)
                {
                    var h = new HTTP();
                    var r = h.Request(item);
                    if (r != null)
                    {
                        r.Save(GetFileNameFromUrl(item));
                    }
                }
                html.Save(IndexFile, encode);
            }
            return this;
        }

        /// <summary>
        /// 获取URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetAbsUri(string url, Uri uri)
        {
            Uri u;
            if (String.IsNullOrWhiteSpace(url))
            {
                return null;
            }
            else if (url.StartsWith("://"))
            {
                url = url.Replace("://", uri.Scheme);
                u = new Uri(url);
            }
            else
            {
                u = new Uri(uri, url);
            }
            return u.GetLeftPart(UriPartial.Query);
        }

    }
}
