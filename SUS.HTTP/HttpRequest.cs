﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SUS.HTTP
{
    public class HttpRequest
    {
        public static IDictionary<string, Dictionary<string, string>> Sessions =
            new Dictionary<string, Dictionary<string, string>>();

        public HttpRequest(string requestString)
        {
            this.Headers = new List<Header>();
            this.Cookies = new List<Cookie>();
            this.FormData = new Dictionary<string, string>();
            this.QueryData = new Dictionary<string, string>();

            var lines = requestString.Split(
                    new string[] { HttpConstants.NewLine }, StringSplitOptions.None);

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

            var sessionCookie = this.Cookies.FirstOrDefault(x => x.Name == HttpConstants.SessionCookieName);
            if (sessionCookie == null)
            {
                var sessionId = Guid.NewGuid().ToString();
                this.Session = new Dictionary<string, string>();
                Sessions.Add(sessionId, this.Session);
                this.Cookies.Add(new Cookie(HttpConstants.SessionCookieName, sessionId));
            }
            else if (!Sessions.ContainsKey(sessionCookie.Value))
            {
                this.Session = new Dictionary<string, string>();
                Sessions.Add(sessionCookie.Value, this.Session);
            }
            else
            {
                this.Session = Sessions[sessionCookie.Value];
            }

            if (this.Path.Contains("?"))
            {
                var pathParts = this.Path.Split(new char[] { '?' }, 2);
                this.Path = pathParts[0];
                this.QueryString = pathParts[1];
            }
            else
            {
                this.QueryString = string.Empty;
            }

            this.Body = bodyWriter.ToString().TrimEnd();

            SplitPArameters(this.Body, this.FormData);
            SplitPArameters(this.QueryString, this.QueryData);

        }

        private void SplitPArameters(string parametersAsString, IDictionary<string, string> output)
        {
            var parameters = parametersAsString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var parameter in parameters)
            {
                var parameterParts = parameter.Split(new[] { '=' }, 2);
                var name = parameterParts[0];
                var value = WebUtility.UrlDecode(parameterParts[1]);

                if (output.ContainsKey(name))
                {
                    output.Add(name, value);
                }
                else
                {
                    output[name] = value;
                }
            }
        }

        public string Path { get; set; }

        public HttpMethod Method { get; set; }

        public ICollection<Header> Headers { get; set; }

        public ICollection<Cookie> Cookies { get; set; }

        public Dictionary<string, string> Session { get; set; }

        public string Body { get; set; }

        public string QueryString { get; set; }

        public IDictionary<string, string> QueryData { get; set; }

        public IDictionary<string, string> FormData { get; set; }
    }
}
