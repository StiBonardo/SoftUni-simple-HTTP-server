using System;
using System.Linq;

using BattleCards.Data;
using BattleCards.Services;
using BattleCards.ViewModels.Cards;

using SUS.HTTP;
using SUS.MvcFramework;

namespace BattleCards.Controllers
{
    public class CardsController : Controller
    {
        private readonly ICardsService cardService;

        public CardsController(ICardsService cardService)
        {
            this.cardService = cardService;
        }

        public HttpResponse Add()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            return this.View();
        }

        [HttpPost]
        public HttpResponse Add(AddCardInputModel model)
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Length < 5 || model.Name.Length > 15)
            {
                return this.Error("Name should be between 5 and 15 characters long!");
            }

            if (string.IsNullOrWhiteSpace(model.Image))
            {
                return this.Error("Image is required!");
            }

            if (!Uri.TryCreate(model.Image, UriKind.Absolute, out _))
            {
                return this.Error("Image url is invalid!");
            }

            if (string.IsNullOrWhiteSpace(model.Keyword))
            {
                return this.Error("Keyword is required!");
            }

            if (model.Attack < 0)
            {
                return this.Error("Attack should be positive number!");
            }

            if (model.Health < 0)
            {
                return this.Error("Health should be positive number!");
            }

            if (string.IsNullOrWhiteSpace(model.Description) || model.Description.Length > 200)
            {
                return this.Error("Description is required! Max 200 symbols alowed.");
            }

            var cardId =  this.cardService.AddCard(model);

            var userId = this.GetUserId();
            this.cardService.AddCardToUserCollection(userId, cardId);
            return this.Redirect("/Cards/All");
        }

        public HttpResponse All()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            return this.View(this.cardService.GetAll());
        }

        public HttpResponse Collection()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            var userId = this.GetUserId();

            return this.View(this.cardService.GetByUserId(userId));
        }

        public HttpResponse AddToCollection(int cardId)
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            var userId = GetUserId();
            this.cardService.AddCardToUserCollection(userId, cardId);
            return this.Redirect("/Cards/All");
        }

        public HttpResponse RemoveFromCollection(int cardId)
        {
            if (!this.IsUserSignedIn())
            {
                return this.Redirect("/Users/Login");
            }

            var userId = GetUserId();
            this.cardService.RemoveCardFromUserCollection(userId, cardId);
            return this.Redirect("/Cards/Collection");
        }
    }
}
