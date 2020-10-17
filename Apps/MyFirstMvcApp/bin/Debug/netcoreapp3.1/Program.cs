using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleCards.Controllers;
using SUS.HTTP;
using SUS.MvcFramework;

namespace BattleCards
{
    class Program
    {
        static async Task Main(string[] args)
        {


            await Host.CreteHostAsync(new Startup(), 80);
        }
    }
}
