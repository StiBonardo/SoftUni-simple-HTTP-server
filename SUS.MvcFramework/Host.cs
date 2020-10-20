using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using SUS.HTTP;
using System.Linq;
using System.Reflection;

namespace SUS.MvcFramework
{
    public static class Host
    {
        public static async Task CreteHostAsync(IMvcApplication application, int port = 80)
        {
            var routeTable = new List<Route>();
            var serviceCollection = new ServiceCollection();

            application.ConfigureServices(serviceCollection);
            application.Configure(routeTable);

            AutoRegisterStaticFiles(routeTable);
            AutoRegisterRoutes(routeTable, application, serviceCollection);

            IHttpServer server = new HttpServer(routeTable);

            await server.StartAsync(port);
        }

        private static void AutoRegisterRoutes(
            List<Route> routeTable, IMvcApplication application, IServiceCollection serviceCollection)
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

                    routeTable.Add(
                        new Route(url, httpMethod, request =>
                        ExecuteAction(request, controllerType, method, serviceCollection)));
                }
            }
        }

        private static HttpResponse ExecuteAction(HttpRequest request, Type controllerType, MethodInfo action, IServiceCollection serviceCollection)
        {
            var instance = serviceCollection.CreateInstance(controllerType) as Controller;
            instance.Request = request;
            var arguments = new List<object>();
            var parameters = action.GetParameters();

            foreach (var parameter in parameters)
            {

                var httpParameterValue = GetParameterFromRequest(request, parameter.Name);
                var parameterValue = Convert.ChangeType(httpParameterValue, parameter.ParameterType);

                if (parameterValue == null && 
                    parameter.ParameterType != typeof(string) &&
                    parameter.ParameterType != typeof(int?)&&
                    parameter.ParameterType != typeof(decimal?) &&
                    parameter.ParameterType != typeof(DateTime?))
                {
                    parameterValue = Activator.CreateInstance(parameter.ParameterType);
                    var properties = parameter.ParameterType.GetRuntimeProperties();

                    foreach (var property in properties)
                    {
                        var propertyHttpParameterValue = GetParameterFromRequest(request, property.Name);
                        var propertyParameterValue = Convert.ChangeType(propertyHttpParameterValue, property.PropertyType );
                        property.SetValue(parameterValue, propertyParameterValue);
                    }
                }

                arguments.Add(parameterValue);
            }

            var response = action.Invoke(instance, arguments.ToArray()) as HttpResponse;
            return response;
        }

        private static string GetParameterFromRequest(HttpRequest request, string parameterName)
        {
            parameterName = parameterName.ToLower();

            if (request.FormData.Any(x => x.Key.ToLower() == parameterName))
            {
                return request.FormData.FirstOrDefault(x => x.Key.ToLower() == parameterName).Value;
            };

            if (request.QueryData.Any(x => x.Key.ToLower() == parameterName))
            {
                return request.QueryData.FirstOrDefault(x => x.Key.ToLower() == parameterName).Value;
            }

            return null;
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
        }
    }
}
