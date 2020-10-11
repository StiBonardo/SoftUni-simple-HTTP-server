using SUS.HTTP;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SUS.MvcFramework
{
    public static class Host
    {
        public static async Task CreteHostAsync(IMvcApplication application, int port = 80)
        {
            var routeTable = new List<Route>();

            application.ConfigureServices();
            application.Configure(routeTable);

            IHttpServer server = new HttpServer(routeTable);

            await server.StartAsync(port);

        }
    }
}
