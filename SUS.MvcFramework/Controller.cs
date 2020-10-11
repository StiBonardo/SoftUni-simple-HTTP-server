using SUS.HTTP;

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace SUS.MvcFramework
{
    public abstract class Controller
    {
        public HttpResponse View([CallerMemberName]string path = null)
        {
            var layout = System.IO.File.ReadAllText("Views/Shared/_Layout.cshtml");
             

            var viewContent = System.IO.File.ReadAllText(
                "Views/"+ 
                this.GetType().Name.Replace("Controller", String.Empty) + 
                "/" + path + ".cshtml");

            var responseHtml = layout.Replace("@RenderBody()", viewContent);

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


    }
}
