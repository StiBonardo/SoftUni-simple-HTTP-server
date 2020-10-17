using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SUS.HTTP
{
    public class HttpRequest
    {

        public HttpRequest(string requestString)
        {
            this.Headers = new List<Header>();
            this.Cookies = new List<Cookie>();
            this.FormData = new Dictionary<string, string>();

            var lines = requestString.Split(
                    new string[] { HttpConstants.NewLine }, System.StringSplitOptions.None);

            var headerLine = lines[0];
            var headerLineParts = headerLine.Split(' ');
            this.Method = (HttpMethod)(Enum.Parse(typeof(HttpMethod), headerLineParts[0], true));
            this.Path = headerLineParts[1];
            var bodyWriter = new StringBuilder();

            var isHeader = true;
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    isHeader = false;
                    continue;
                }

                if (isHeader)
                {
                    this.Headers.Add(new Header(line));
                }
                else
                {
                    bodyWriter.AppendLine(line);
                }
            }

            var cookiesLine = this.Headers.FirstOrDefault(x => x.Name == HttpConstants.RequestCookieHeader);

            if (cookiesLine != null)
            {
                var cookiesAsStringArr = cookiesLine.Value.Split(
                    new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var cookieStr in cookiesAsStringArr)
                {
                    this.Cookies.Add(new Cookie(cookieStr));
                }
            }

            this.Body = bodyWriter.ToString();

            if (!string.IsNullOrEmpty(this.Body))
            {


                var parameters = this.Body.Split('&');

                foreach (var paramParts in parameters)
                {
                    var kvp = paramParts.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    var key = kvp[0];
                    var value = WebUtility.UrlDecode(kvp[1]);

                    if (!this.FormData.ContainsKey(key))
                    {
                        this.FormData.Add(key, value);
                    }
                    else
                    {
                        this.FormData[key] = value;
                    }
                }
            }
        }

        public string Path { get; set; }

        public HttpMethod Method { get; set; }

        public ICollection<Header> Headers { get; set; }

        public ICollection<Cookie> Cookies { get; set; }

        public string Body { get; set; }

        public IDictionary<string, string> FormData { get; set; }
    }
}
