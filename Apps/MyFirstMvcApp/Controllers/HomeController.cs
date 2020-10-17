using System;

using SUS.HTTP;
using SUS.MvcFramework;
using BattleCards.ViewModels;

namespace BattleCards.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public HttpResponse Index()
        {
            var viewModel = new IndexViewModel
            {
                Year = DateTime.UtcNow.Year,
                Message = "Welcome to Battle Cards",
            };

            return this.View(viewModel);
        }

        public HttpResponse About()
        {
            return this.View();
        }
    }
}
