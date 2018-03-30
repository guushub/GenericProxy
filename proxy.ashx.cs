using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GenericProxy
{
    /// <summary>
    /// Generic quick and dirty proxy to forward urls
    /// </summary>
    public class proxy : IHttpHandler
    {

        private AllowedHosts AllowedHostsConfigured { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            HttpResponse response = context.Response;
            var url = GetUrlFromQueryString(context.Request.QueryString);
            var allowedHosts = GetAllowedHosts();
            var contents = "";

            try
            {
                if (allowedHosts.UrlIsAllowed(url))
                {
                    contents = GetSourceContents(url);
                    response.ContentType = "text/plain";
                    response.Write(contents);

                }
                else
                {
                    response.StatusCode = 403;
                    response.Write(contents);
                }
            }
            catch
            {
                response.StatusCode = 500;
                response.Write(contents);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string GetUrlFromQueryString(System.Collections.Specialized.NameValueCollection queryString)
        {

            var url = Uri.UnescapeDataString(queryString.ToString());
            return url;
        }

        private bool CanRequestSource()
        {

            return true;
        }

        private string GetSourceContents(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string contents = String.Empty;
            using (StreamReader streamReader = new StreamReader(data))
            {
                contents = streamReader.ReadToEnd();
            }

            return contents;
        }

        private AllowedHosts GetAllowedHosts()
        {

            var allowedHosts = new AllowedHosts();

            var allowedHostsConfigured = new List<string>(ConfigurationManager.AppSettings["allowedHosts"].Split(new char[] { ';' }));
            foreach (var host in allowedHostsConfigured)
            {
                allowedHosts.Hosts.Add(host);
            }

            return allowedHosts;
        }

    }
}

public class AllowedHosts
{
    public List<string> Hosts { get; set; }

    public AllowedHosts()
    {
        Hosts = new List<string>();
    }

    public bool UrlIsAllowed(string url)
    {
        Uri urlToMatch = new Uri(url);
        string hostToMatch = urlToMatch.Host;

        return Hosts.Any((host) =>
        {
            Uri urlAllowed = new Uri(host);
            string hostAllowed = urlAllowed.Host;
            return hostToMatch == hostAllowed;
        });
    }
}
