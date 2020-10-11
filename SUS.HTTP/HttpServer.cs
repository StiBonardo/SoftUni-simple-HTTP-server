using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SUS.HTTP
{
    public class HttpServer : IHttpServer
    {

        private List<Route> routeTable;

        public HttpServer(List<Route> routeTable)
        {
            this.routeTable = routeTable;
        }
        
        public async Task StartAsync(int port)
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, port);
            tcpListener.Start();

            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                ProcessClinetAsync(tcpClient);
            }
        }

        private async Task ProcessClinetAsync(TcpClient tcpClient)
        {
            try
            {
                using (var stream = tcpClient.GetStream())
                {
                    var data = new List<byte>();
                    var buffer = new byte[HttpConstants.BufferSize];
                    var position = 0;

                    while (true)
                    {

                        var count = await stream.ReadAsync(buffer, position, buffer.Length);
                        position += count;

                        if (count < buffer.Length)
                        {
                            var partialBuffer = new byte[count];
                            Array.Copy(buffer, partialBuffer, count);
                            data.AddRange(partialBuffer);
                            break;
                        }
                        else
                        {
                            data.AddRange(buffer);
                        }
                    }

                    var requestString = Encoding.UTF8.GetString(data.ToArray());
                    var request = new HttpRequest(requestString);

                    Console.WriteLine($"{request.Method} {request.Path} => {request.Headers.Count}");

                    HttpResponse response;
                    var route = this.routeTable.FirstOrDefault(x => x.Path == request.Path);
                    if (route != null)
                    {
                        response = route.Action(request);
                    }
                    else
                    {
                        response = new HttpResponse("text/htm", null, HttpStatusCode.NotFound);
                    }

                    response.Cookies.Add(new ResponseCookie("sid", Guid.NewGuid().ToString())
                    { HttpOnly = true, MaxAge = 60 * 60 * 60 * 24 * 60 });
                    var responseHeaderBytes = Encoding.UTF8.GetBytes(response.ToString());
                    await stream.WriteAsync(responseHeaderBytes, 0, responseHeaderBytes.Length);
                    await stream.WriteAsync(response.Body, 0, response.Body.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
