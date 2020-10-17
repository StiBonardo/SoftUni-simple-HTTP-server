using System.Linq;

using BattleCards.Data;
using BattleCards.ViewModels;

using SUS.HTTP;
using SUS.MvcFramework;

namespace BattleCards.Controllers
{
    public class CardsController : Controller
    {
        public HttpResponse Add()
        {
            return this.View();
        }

        [HttpPost("/Cards/Add")]
        public HttpResponse DoAdd()
        {
            var dbContext = new ApplicationDbContext();
            dbContext.Cards.Add(new Card() 
            {
                Attack = int.Parse(this.Request.FormData["attack"]),
                Health = int.Parse(this.Request.FormData["health"]),
                Description = this.Request.FormData["description"],
                Name = this.Request.FormData["name"],
                Keyword = this.Request.FormData["keyword"],
                ImageUrl = this.Request.FormData["image"],
            });

            dbContext.SaveChanges();
            var request = this.Request;
            return this.Redirect("/"); 
        }

        public HttpResponse All()
        {
            var db = new ApplicationDbContext();
            var cardsViewModel = db.Cards.Select(x => new CardViewModel()
            {
                //Id = x.Id,
                Attack = x.Attack,
                Health = x.Health,
                ImageUrl = x.ImageUrl,
                Type = x.Keyword
            }).ToList();

            return this.View(new AllCardsViewModel() {Cards = cardsViewModel});
        }

        public HttpResponse Collection()
        {
            return this.View();
        }
    }
}
