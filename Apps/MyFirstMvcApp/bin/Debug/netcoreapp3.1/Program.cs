using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyFirstMvcApp.Controllers;
using SUS.HTTP;
using SUS.MvcFramework;

namespace MyFirstMvcApp
{
    class Program
    {
        static async Task Main(string[] args)
        {

           

            await Host.CreteHostAsync(new Startup(), 80);
        }
    }
}
