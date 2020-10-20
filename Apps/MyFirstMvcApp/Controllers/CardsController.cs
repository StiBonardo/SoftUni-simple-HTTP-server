using System.Linq;

using BattleCards.Data;
using BattleCards.ViewModels.Cards;

using SUS.HTTP;
using SUS.MvcFramework;

namespace BattleCards.Controllers
{
    public class CardsController : Controller
    {
        private readonly ApplicationDbContext db;

        public CardsController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public HttpResponse Add()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            return this.View();
        }

        [HttpPost("/Cards/Add")]
        public HttpResponse DoAdd(AddCardInputModel inputModel)
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            if (this.Request.FormData["name"].Length < 5)
            {
                return this.Error("Name should be at least five(5), characters long!");
            }

            this.db.Cards.Add(new Card() 
            {
                Attack = inputModel.Attack,
                Health = inputModel.Health,
                Description = inputModel.Description,
                Name = inputModel.Name,
                Keyword = inputModel.Keyword,
                ImageUrl = inputModel.Image,
            });
            this.db.SaveChanges();

            return this.Redirect("/Cards/All"); 
        }

        public HttpResponse All()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            var cardsViewModel = this.db.Cards.Select(x => new CardViewModel()
            {
                Attack = x.Attack,
                Health = x.Health,
                ImageUrl = x.ImageUrl,
                Type = x.Keyword
            }).ToList();

            return this.View(cardsViewModel);
        }

        public HttpResponse Collection()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            return this.View();
        }
    }
}
