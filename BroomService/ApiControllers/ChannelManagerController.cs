using BroomService.Services;
using BroomService.Services.ChannelManager;
using BroomService.ViewModels;
using BroomService.ViewModels.ChannelManager.Smoobu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BroomService.ApiControllers
{
    [RoutePrefix("api/channel-managers")]
    public class ChannelManagerController : ApiController
    {
        private readonly ChannelManagerService _channelManagerService;
        public ChannelManagerController()
        {
            _channelManagerService = new ChannelManagerService();
        }

        #region Channel Manager List

        /// <summary>
        /// Get Channel Manager List
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public IHttpActionResult GetChannelManagers()
        {
            var result = _channelManagerService.ChannelManagers();

            return this.Ok(new
            {
                status = result == null ? false : true,
                message = _channelManagerService.message,
                data = result
            });
        }

        #endregion Channel Manager List

        #region Channel Manager Accomodations

        /// <summary>
        /// Channel Manager Accomodation List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("accomodations/{userId}")]
        public async Task<IHttpActionResult> ChannelManagerAccomodations(long userId)
        {
            var result = await _channelManagerService.Accomodations(userId);

            return this.Ok(new
            {
                status = result != null,
                message = _channelManagerService.message,
                data = result
            });
        }

        #endregion Channel Manager Accomodations

        #region Channel Manager Available Accomodations

        /// <summary>
        /// Channel Manager Accomodation List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("accomodations/available/{userId}")]
        public async Task<IHttpActionResult> ChannelManagerAvailableAccomodations(long userId)
        {
            var result = await _channelManagerService.Accomodations(userId, true);

            return this.Ok(new
            {
                status = result != null,
                message = _channelManagerService.message,
                data = result
            });
        }

        #endregion Channel Manager Available Accomodations

        /// <summary>
        /// Stripe webhook handling
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("smoobu/webhook")]
        public async Task<IHttpActionResult> SmoobuWebhookAsync()
        {
            try
            {
                string result = await Request.Content.ReadAsStringAsync(); // json data send from smoobu

                var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                SmoobuReservationViewModel smoobuReservationVM = null;
                SmoobuChannelManager smoobuChannelManager = new SmoobuChannelManager();
                if (dict["action"] == "newReservation" || dict["action"] == "updateReservation")
                {
                    smoobuReservationVM = JsonConvert.DeserializeObject<SmoobuReservationViewModel>(result);
                }
                // check the result action
                switch (dict["action"])
                {
                    case "newReservation":
                        smoobuChannelManager.NewReservation(smoobuReservationVM.Data);
                        break;
                    case "updateReservation":
                        smoobuChannelManager.UpdateReservation(smoobuReservationVM.Data);
                        break;
                    default:
                        Console.WriteLine("No Operation");
                        break;
                }

                return Ok();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }
    }
}