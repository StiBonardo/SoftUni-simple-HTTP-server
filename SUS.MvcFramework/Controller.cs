﻿using SUS.HTTP;
using SUS.MvcFramework.ViewEngine;

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace SUS.MvcFramework
{
    public abstract class Controller
    {
        private SUSViewEngine viewEngine;

        public Controller()
        {
            this.viewEngine = new SUSViewEngine();
        }

        public HttpRequest Request { get; set; }

        public HttpResponse View(object viewModel = null,
            [CallerMemberName]string path = null)
        {

            var viewContent = System.IO.File.ReadAllText(
                "Views/" +
                this.GetType().Name.Replace("Controller", String.Empty) +
                "/" + path + ".cshtml");
            viewContent = this.viewEngine.GetHtml(viewContent, viewModel);

            var responseHtml = this.PutViewInLayout(viewContent, viewModel);

            var responseBodyBytes = Encoding.UTF8.GetBytes(responseHtml);
            var response = new HttpResponse("text.html", responseBodyBytes);

            return response;
        }

        public HttpResponse File(string path, string contentType)
        {
            var fileBytes = System.IO.File.ReadAllBytes(path);
            var response = new HttpResponse(contentType, fileBytes);
            return response;
        }

        public HttpResponse Redirect(string url)
        {
            var response = new HttpResponse(HttpStatusCode.Found);
            response.Headers.Add(new Header("Location", url));
            return response;
        }

        public HttpResponse Error(string errorMessage)
        {
            var viewContent = $"<div class=\"alert alert-danger\" role=\"alert\">{errorMessage}</div>";
            var responseHtml = this.PutViewInLayout(viewContent);
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseHtml);
            var response = new HttpResponse("text.html", responseBodyBytes, HttpStatusCode.ServerError);

            return response;
        }

        private string PutViewInLayout(string viewContent, object viewModel = null)
        {
            var layout = System.IO.File.ReadAllText("Views/Shared/_Layout.cshtml");
            layout = layout.Replace("@RenderBody()", "VIEW_GOES_HERE");
            layout = this.viewEngine.GetHtml(layout, viewContent);
            var responseHtml = layout.Replace("VIEW_GOES_HERE", viewContent);
            return responseHtml;
        }
    }
}
