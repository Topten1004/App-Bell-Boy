using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class CardViewModel
    {
        public long CardId { get; set; }
        public string NameOnCard { get; set; }
        public string Email { get; set; }
        public string CardNumber { get; set; }
        public int? CVV { get; set; }
        public int? ExpireMonth { get; set; }
        public int? ExpireYear { get; set; }
        public string CardType { get; set; }
    }

    public class AddCardModel
    {
        public string NameOnCard { get; set; }
        public string Email { get; set; }
        public long CardNumber { get; set; }
        public int CVV { get; set; }
        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }
        public long UserId { get; set; }
        public string CardType { get; set; }
    }

    public class MessageList
    {
        public string current_date_time { get; set; }
    }

    public class JobRequestQuoteTypeViewModel
    {
        public int? Type { get; set; }
        public long? JobRequestId { get; set; }
        public long? UserId { get; set; }
        public string MeetingTime { get; set; }
        public string Description { get; set; }
        public List<ChecklistImageVM> Images { get; set; }
    }

    public class MessageViewModel
    {
        public List<string> Messages { get; set; }
    }

    public class SelectCard
   {
        public CardViewModel selectCardDetail { get; set; }
    }
}