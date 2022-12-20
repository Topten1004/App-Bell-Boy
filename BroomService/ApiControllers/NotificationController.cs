using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BroomService.ApiControllers
{
    public class NotificationController : ApiController
    {
        NotificationService notificationService;

        public NotificationController()
        {
            notificationService = new NotificationService();
        }

        /// <summary>
        /// Get Notifications
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetNotifications(long userId)
        {
            var result = notificationService.GetNotifications(userId);

            return this.Ok(new
            {
                status = result == null ? false : true,
                message = notificationService.message,
                data = result
            });
        }
    }
}
