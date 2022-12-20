using BroomService.Models;
using BroomService.Resources;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.Services
{
    public class CardService
    {
        BroomServiceEntities1 _db;
        public string message;

        public CardService()
        {
            _db = new BroomServiceEntities1();
        }   

        public bool AddUpdateCard(Card model)
        {
            bool status = false;
            try
            {
                if (model.CardId != 0)
                {
                    var data = _db.Cards.Where(x => x.CardId == model.CardId && x.UserId == model.UserId).FirstOrDefault();

                    data.CardNumber = model.CardNumber;
                    data.CVV = model.CVV;
                    data.Email = model.Email;
                    data.NameOnCard = model.NameOnCard;
                    data.ExpireMonth = model.ExpireMonth;
                    data.ExpireYear = model.ExpireYear;
                    data.CardType = model.CardType;
                    _db.SaveChanges();
                    status = true;
                    message = "Card Updated Succesfully";
                }
                else
                {
                    _db.Cards.Add(model);
                    _db.SaveChanges();
                    status = true;
                    message = Resource.card_add_success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public List<CardViewModel> GetCardList(long user_id)
        {
            List<CardViewModel> cardData = new List<CardViewModel>();
            try
            {
                cardData = _db.Cards.Where(a => a.UserId == user_id)
                    .Select(A => new CardViewModel
                    {
                        CardId = A.CardId,
                        NameOnCard = A.NameOnCard,
                        CardNumber = A.CardNumber,
                        CVV = A.CVV,
                        Email = A.Email,
                        ExpireMonth = A.ExpireMonth,
                        ExpireYear = A.ExpireYear,   
                        CardType = A.CardType,
                    }).ToList();
            }
            catch (Exception ex)
            {
            }
            return cardData;
        }
        public CardViewModel GetCardByCardId(long cardId)
        {
           CardViewModel cardData = new CardViewModel();
            try
            {
                cardData = _db.Cards.Where(a => a.CardId == cardId)
                    .Select(A => new CardViewModel
                    {
                        CardId = A.CardId,
                        NameOnCard = A.NameOnCard,
                        CardNumber = A.CardNumber,
                        CVV = A.CVV,
                        Email = A.Email,
                        ExpireMonth = A.ExpireMonth,
                        ExpireYear = A.ExpireYear,
                        CardType = A.CardType,
                    }).FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return cardData;
        }
        public bool DeleteCard(int cardId,long userId)
        {
            bool status = false;
            try
            {
               var data = _db.Cards.Where(x => x.CardId == cardId && x.UserId == userId).FirstOrDefault();
                if(data != null)
                {
                    _db.Cards.Remove(data);
                    _db.SaveChanges();
                    status = true;
                }               
            }
            catch(Exception ex)
            {
                status = false;
            }
            return status;
        }
    }
}