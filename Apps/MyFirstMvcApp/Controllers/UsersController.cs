using System;
using System.Text;

using SUS.HTTP;
using SUS.MvcFramework;

namespace MyFirstMvcApp.Controllers
{
    public class UsersController : Controller
    {
        public HttpResponse Login(HttpRequest request)
        {
            var responseHtml = "<h1>Login</h1>";
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseHtml);
            var response = new HttpResponse("text.html", responseBodyBytes);
            response.Cookies.Add(new ResponseCookie("sid", Guid.NewGuid().ToString())
            { HttpOnly = true, MaxAge = 60 * 60 * 60 * 24 * 60 });

            return response;
        }
        public HttpResponse Register(HttpRequest request)
        {
            var responseHtml = "<h1>Register</h1>";
            var responseBodyBytes = Encoding.UTF8.GetBytes(responseHtml);
            var response = new HttpResponse("text.html", responseBodyBytes);
            response.Cookies.Add(new ResponseCookie("sid", Guid.NewGuid().ToString())
            { HttpOnly = true, MaxAge = 60 * 60 * 60 * 24 * 60 });

            return response;
        }
    }
}
