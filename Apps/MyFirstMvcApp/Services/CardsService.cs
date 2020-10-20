using System;
using System.Collections.Generic;
using System.Linq;

using BattleCards.Data;
using BattleCards.ViewModels.Cards;

namespace BattleCards.Services
{
    public class CardsService : ICardsService
    {
        private readonly ApplicationDbContext db;

        public CardsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public int AddCard(AddCardInputModel model)
        {
            var card = new Card()
            {
                Attack = model.Attack,
                Health = model.Health,
                Description = model.Description,
                Name = model.Name,
                Keyword = model.Keyword,
                ImageUrl = model.Image,
            };

            this.db.Cards.Add(card);
            this.db.SaveChanges();

            return card.Id;
        }

        public IEnumerable<CardViewModel> GetAll()
        {
            return this.db.Cards.Select(x => new CardViewModel()
            {
                Id = x.Id,
                Attack = x.Attack,
                Health = x.Health,
                ImageUrl = x.ImageUrl,
                Type = x.Keyword,
            }).ToList();
        }

        public IEnumerable<CardViewModel> GetByUserId(string userId)
        {
            return this.db
                .UsersCards
                .Where(x => x.UserId == userId)
                .Select(x => new CardViewModel
                {
                    Attack = x.Card.Attack,
                    Health = x.Card.Health,
                    Name = x.Card.Name,
                    Description = x.Card.Description,
                    ImageUrl = x.Card.ImageUrl,
                    Type = x.Card.Keyword,
                    Id = x.CardId,
                }).ToList();
        }

        public void AddCardToUserCollection(string userId, int cardId)
        {
            if (this.db.UsersCards.Any(x => x.UserId == userId && x.CardId == cardId))
            {
                return;
            }

            this.db.UsersCards.Add(new UserCard 
            {
                CardId = cardId,
                UserId = userId
            });
            this.db.SaveChanges();
        }

        public void RemoveCardFromUserCollection(string userId, int cardId)
        {
            var userCard = this.db.UsersCards.FirstOrDefault(x => x.CardId == cardId && x.UserId == userId);

            if (userCard == null)
            {
                return;
            }

            this.db.UsersCards.Remove(userCard);
            this.db.SaveChanges();
        }
    }
}
