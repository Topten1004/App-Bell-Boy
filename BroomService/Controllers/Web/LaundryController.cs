using BroomService.bin.Controllers.Web;
using BroomService.CustomFilter;
using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using System.Web.Mvc;
using BroomService.ViewModels;

namespace BroomService.Controllers.Web
{
    public class LaundryController : MyController
    {
        int pageSize = 3;
        LaundryService laundryService;
        AccountService accountService;

        public LaundryController()
        {
            laundryService = new LaundryService();
            accountService = new AccountService();
        }

        [VerifyUser]
        public ActionResult Index(long? propertyId)
        {
            return View();
        }

        #region Laundry
        [HttpGet]
        public JsonResult GetAvailabilites()
        {
            var availableSchedules = laundryService.GetAvailabilities();

            return Json(availableSchedules, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeliveryBookedSchedules(long propertyId)
        {
            var bookedSchedules = laundryService.GetBookedSchedules(propertyId);

            return Json(bookedSchedules, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLaundry(long propertyId)
        {
            var selectedLaundry = laundryService.GetLaundry(propertyId);
            return Json(new
            {
                status = selectedLaundry != 0,
                laundryId = selectedLaundry
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDeliveryDistances(long propertyId)
        {
            var selectedLaundry = laundryService.GetDeliveryDistances(propertyId);
            return Json(selectedLaundry, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [VerifyUser]
        public JsonResult LaundryRequest(LaundryRequestViewModel laundryRequest)
        {
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            laundryRequest.UserId = userId;
            var result = laundryService.LaundryRequest(laundryRequest);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Laundry List
        [VerifyUser]
        public ActionResult LaundryList(int page = 1, int laundryStatus = 1)
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var laundryRequests = laundryService.GetLaundries(userId, laundryStatus);

            var laundryList = laundryRequests.ToPagedList(page, pageSize);

            @ViewBag.laundryStatus = laundryStatus;

            return View(laundryList);
        }
        #endregion

        [HttpPost]
        [VerifyUser]
        public JsonResult GenerateLaundryPayment(long laundryRequestId)
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var user = accountService.GetCurrentUser(userId);
            var result = laundryService.GeneratePaymentPage(laundryRequestId, user);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}