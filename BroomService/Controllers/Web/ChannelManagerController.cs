using BroomService.CustomFilter;
using BroomService.Services;
using BroomService.Services.ChannelManager;
using BroomService.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace BroomService.bin.Controllers.Web
{
    public class ChannelManagerController : MyController
    {
        private readonly ChannelManagerService channelManagerService;
        public ChannelManagerController()
        {
            channelManagerService = new ChannelManagerService();
        }

        #region Channel Manager Activate

        // Activate
        /// <summary>
        /// Activate Channel Manager
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [VerifyUser]
        public async Task<JsonResult> Activate(UserChannelManagerViewModel userChannelManager)
        {
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            var result = await channelManagerService.ActivateChannelManager(userId, userChannelManager);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion Channel Manager Activate

        // Deactivate
        #region Channel Manager Deactivate

        /// <summary>
        /// Deactivate Channel Manager
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [VerifyUser]
        public ActionResult Deactivate(long channelManagerId)
        {
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            channelManagerService.DeactivateChannelManager(userId, channelManagerId);
            return RedirectToAction("PropertyManagers", "Account");
        }

        #endregion Channel Manager Deactivate
        // Settings
        // UpdateSettings

        #region Channel Manager UpdateSettings

        /// <summary>
        /// Deactivate Channel Manager
        /// </summary>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        [VerifyUser]
        public ActionResult UpdateSettings(UserChannelManagerSettingsViewModel userChannelManagerSettings)
        {
            var routeValuesDictionary = new RouteValueDictionary();
            Request.QueryString.AllKeys.ForEach(key => routeValuesDictionary.Add(key, Request.QueryString[key]));
            routeValuesDictionary.Add("userChannelManagerId", userChannelManagerSettings.UserChannelManagerId);

            var result = channelManagerService.UpdateUserChannelManagerSettings(userChannelManagerSettings);
            return RedirectToAction("PropertySettings", "Account", routeValuesDictionary);
        }

        #endregion Channel Manager UpdateSettings

        [HttpPost]
        [VerifyUser]
        public async Task<JsonResult> ImportSmoobuProperty(ChannelManagerAccomodationViewModel apartment)
        {
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            SmoobuChannelManager smoobuChannelManager = new SmoobuChannelManager();
            var result = await smoobuChannelManager.ImportPropertyByApartmentId(userId, apartment.Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [VerifyUser]
        public async Task<JsonResult> Accomodations()
        {
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            var result = await channelManagerService.Accomodations(userId);

            return Json(new
            {
                status = result != null,
                message = channelManagerService.message,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }
    }
}