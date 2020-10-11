using System;
using System.Linq;
using System.Text;

using SUS.HTTP;
using SUS.MvcFramework;

namespace MyFirstMvcApp.Controllers
{
    public class HomeController : Controller
    {
        public HttpResponse Index(HttpRequest request)
        {
            var responseHtml = "<h1>Welcome!</h1>" +
                       request.Headers.FirstOrDefault(x => x.Name == "User-Agent")?.Value;
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseHtml);
            var response = new HttpResponse("text.html", responseBodyBytes);
            response.Cookies.Add(new ResponseCookie(
                "sid", Guid.NewGuid().ToString())
            { HttpOnly = true, MaxAge = 60 * 60 * 60 * 24 * 60 });

            return response;
        }

        public HttpResponse About(HttpRequest request)
        {
            var responseHtml = "<h1>About</h1>";
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseHtml);
            var response = new HttpResponse("text.html", responseBodyBytes);
            response.Cookies.Add(new ResponseCookie("sid", Guid.NewGuid().ToString())
            { HttpOnly = true, MaxAge = 60 * 60 * 60 * 24 * 60 });

            return response;
        }
    }
}
