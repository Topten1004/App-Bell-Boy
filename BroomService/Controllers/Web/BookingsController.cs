using BroomService.bin.Controllers.Web;
using BroomService.CustomFilter;
using BroomService.Helpers;
using BroomService.Models;
using BroomService.Services;
using BroomService.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BroomService.Controllers
{
    public class BookingsController : MyController
    {
        OrderService orderService;
        BookingService bookingService;
        int pageSize = 10;
        public BookingsController()
        {
            orderService = new OrderService();
            bookingService = new BookingService();
        }

        #region Booking List
        [VerifyUser]
        public ActionResult BookingsList(int page = 1, int jobStatus = 1)
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var jobRequests = bookingService.GetBookings(userId, jobStatus);

            var bookingList = jobRequests.ToPagedList(page, pageSize);

            @ViewBag.jobStatus = jobStatus;

            return View(bookingList);
        }
        #endregion

        #region Bookings Details

        public ActionResult BookingDetails(long booking_id)
        {
            var getDetails = orderService.BookingsDetails(booking_id);
            return View(getDetails);
        }
        public bool EditCheckList(string data, long JobReqId)
        {
            bool status = false;
            List<JobRequestCheckListModel> checklist = new List<JobRequestCheckListModel>();
            checklist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JobRequestCheckListModel>>(data);
            status = orderService.ModifyCheckList(checklist, JobReqId);
            return status;
        }
        #endregion

        #region Make Cancel job
       [System.Web.Http.HttpPost]
        public string MakeCancelRefund(long jobPropId, long workerId, long userId, long propertyId, long serviceId,Boolean serviceType)
        {
            //Change parameters as new changes
            bool status = orderService.MakeCancelRefund(jobPropId, workerId, userId, propertyId, serviceId, serviceType);
            if (status)
            {
                return orderService.refundPrice.ToString();
            }
            else
            {
                return "false";
            }
        }
        #endregion

        #region Invoice/Receipt Download 
        [System.Web.Http.HttpPost]
        public string InvoiceDownload(long orderId)
        {
            try
            {
                var jobReq = orderService.GetInvoiceDoc(orderId);
                if (jobReq != null)
                {
                    return jobReq.Invoice;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }
        [System.Web.Http.HttpPost]
        public string ReceiptDownload(long orderId)
        {
            try
            {
                var jobReq = orderService.GetReceiptDoc(orderId);
                if (jobReq != null)
                {
                    return jobReq.Receipt;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        #endregion

        #region Meeting Schedule
        [System.Web.Http.HttpPost]
        public bool MeetingSchedule(long Id)
        {
            bool status = false;
            try
            {
                status = orderService.MeetingSchedule(Id);

            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        #endregion

        [VerifyUser]
        public ActionResult PaymentDetail(int? pageNumber)
        {
            long userId = 0;
            if(pageNumber== null)
            {
                pageNumber = 1;
            }
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                }
            }
            var list = orderService.GetAllOrders(userId);
            var orders = list.ToPagedList(pageNumber ?? 1, 10);
            return View(orders);
        }

        [System.Web.Http.HttpGet]
        [VerifyUser]
        public JsonResult GetCheckInList(long Id)
        {

            try
            {
                var Result = orderService.getCheckInList(Id);
                return Json(Result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [System.Web.Http.HttpPost]
        public JsonResult UpdateCheckout(long Id)
        {
            var Result = orderService.updateCheckout(Id);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        #region Make Cancel Refund
        [HttpGet]
        [VerifyUser]
        public ActionResult MakeCancelRefund(long jobRequestId)
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var result = orderService.CancelRefund(jobRequestId, userId);

            if (result)
            {
                var cancelStatus = Enums.RequestStatus.Canceled.GetHashCode();
                return RedirectToAction("BookingsList", new { page = 1, jobStatus = cancelStatus });
            }

            return RedirectToAction("BookingsList");

        }
        #endregion
    }
}