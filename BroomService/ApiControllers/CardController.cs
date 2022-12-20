using BroomService.Models;
using BroomService.Resources;
using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BroomService.ApiControllers
{
    public class CardController : ApiController
    {
        CardService cardService;

        public CardController()
        {
            cardService = new CardService();
        }

        /// <summary>
        /// Add Card Details 
        /// </summary>
        /// <returns>model</returns>
        [HttpPost]
        public IHttpActionResult AddCard(Card model)
        {
            var response = cardService.AddUpdateCard(model);
            return this.Ok(new
            {
                status = response,
                message = cardService.message
            });
        }

        /// <summary>
        /// Get Card Details by the user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetCardList(long user_id)
        {
            var result = cardService.GetCardList(user_id);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = result != null ? Resource.success : Resource.no_data_found,
                cardData = result
            });
        }

    }
}
