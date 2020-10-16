using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using SUS.HTTP;
using System.Linq;

namespace SUS.MvcFramework
{
    public static class Host
    {
        public static async Task CreteHostAsync(IMvcApplication application, int port = 80)
        {
            var routeTable = new List<Route>();

            AutoRegisterRoutes(routeTable, application);
            AutoRegisterStaticFiles(routeTable);

            application.ConfigureServices();
            application.Configure(routeTable);

            IHttpServer server = new HttpServer(routeTable);

            await server.StartAsync(port);

        }

        private static void AutoRegisterRoutes(List<Route> routeTable, IMvcApplication application)
        {
            var controllerTypes = application
                .GetType()
                .Assembly
                .GetTypes()
                .Where(x => x.IsClass &&
                       !x.IsAbstract &&
                       x.IsSubclassOf(typeof(Controller)));

            foreach (var controllerType in controllerTypes)
            {
                Console.WriteLine(controllerType.Name + Environment.NewLine);

                var methods = controllerType
                    .GetMethods()
                    .Where(x => 
                           x.IsPublic && 
                           !x.IsStatic && 
                           x.DeclaringType == controllerType &&
                           !x.IsAbstract && 
                           !x.IsConstructor &&
                           !x.IsSpecialName);

                foreach (var method in methods)
                {
                    var url = "/" + controllerType.Name.Replace("Controller", string.Empty) + "/" + method.Name;

                    var attribute = method.GetCustomAttributes(false)
                        .Where(x => x.GetType().IsSubclassOf(typeof(BaseHttpAttribute)))
                        .FirstOrDefault() as BaseHttpAttribute;

                    var httpMethod = HttpMethod.Get;

                    if (attribute != null)
                    {
                        httpMethod = attribute.Method;
                    }

                    if (!string.IsNullOrEmpty(attribute?.Url))
                    {
                        url = attribute.Url;
                    }

                    routeTable.Add(new Route(url, httpMethod, (request) =>
                    {
                        var instance = Activator.CreateInstance(controllerType) as Controller;
                        instance.Request = request;

                        var response = method.Invoke(instance, new object[] { }) as HttpResponse;
                        return response;
                    }));

                    Console.WriteLine(url);
                }
            }
        }

        private static void AutoRegisterStaticFiles(List<Route> routeTable)
        {
            var staticFiles = Directory.GetFiles("wwwroot", "*", SearchOption.AllDirectories);
            foreach (var file in staticFiles)
            {
                var path = file.Replace("wwwroot", string.Empty)
                    .Replace("\\", "/");
                routeTable.Add(new Route(path, HttpMethod.Get, (request) =>
                {
                    var fileContent = File.ReadAllBytes(file);
                    var fileExtention = new FileInfo(file).Extension;
                    var contentType = fileExtention switch
                    {
                        ".txt" => "text/plain",
                        ".css" => "text/css",
                        ".js" => "text/javascript",
                        ".png" => "image/png",
                        ".jpg" => "image/jpg",
                        ".jpeg" => "image/jpg",
                        ".gif" => "image/gif",
                        ".ico" => "image/vnd.microsoft.icon",
                        ".html" => "text/html",
                        _ => "text/plain"
                    };

                    return new HttpResponse(contentType, fileContent);
                }));
            }

            Console.WriteLine("All registered routes:");
            foreach (var route in routeTable)
            {
                Console.WriteLine($"{route.Method} {route.Path}");
            }
        }
    }
}
