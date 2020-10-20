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

        public async Task StartAsync(int port = 80)
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
                    var route = this.routeTable.FirstOrDefault(
                        x => string.Compare(x.Path, request.Path, true) == 0 && x.Method == request.Method);
                    if (route != null)
                    {
                        response = route.Action(request);
                    }
                    else
                    {
                        response = new HttpResponse("text/htm", new byte[0], HttpStatusCode.NotFound);
                    }

                    var sessionCookie = request.Cookies.FirstOrDefault(x => x.Name == HttpConstants.SessionCookieName);
                    if (sessionCookie != null)
                    {
                        var responseSessionCookie = new ResponseCookie(sessionCookie.Name, sessionCookie.Value);
                        response.Cookies.Add(responseSessionCookie);
                    }

                    var responseHeaderBytes = Encoding.UTF8.GetBytes(response.ToString());
                    await stream.WriteAsync(responseHeaderBytes, 0, responseHeaderBytes.Length);

                    if (response.Body != null)
                    {
                        await stream.WriteAsync(response.Body, 0, response.Body.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
