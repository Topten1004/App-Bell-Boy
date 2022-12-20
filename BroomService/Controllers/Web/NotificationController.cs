using BroomService.bin.Controllers.Web;
using BroomService.Models;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.Controllers
{
    public class NotificationController : MyController
    {
        // GET: Notification
        NotificationService notify = new NotificationService();

        public ActionResult NotificationList()
        {
            List<NotificationViewModel> notificationList = new List<NotificationViewModel>();
            int userId = 0;
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                }
            }
            if (userId != 0)
            {
                var result = notify.GetNotifications(userId);
                if (result != null)
                {
                    foreach (NotificationViewModel notification in result) {
                        notificationList.Add(notification);
                    }
                }
                Session["OldNotificationList"] = notificationList.Count;
                return View(notificationList.OrderByDescending(x=> x.CreatedDate).ToList()) ;
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult Notification(long id)
        {          
            
                var result = notify.GetNotification(id);                
                return View(result);            
        }

        [HttpGet]
        public JsonResult ChangeNotificationAlertIcon()
        {
            try
            {
                string notifiCount = string.Empty;
                int userId = 0;
                if (Request.Cookies["Login"] != null)
                {
                    if (Request.Cookies["Login"].Values["UserId"] != null)
                    {
                        userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    }
                }
                if (userId != 0)
                {
                    var Result = notify.changeNotificationAlertIcon(userId);
                    int oldNotification = Convert.ToInt32(Session["OldNotificationList"]);
                    int newNotification = Result;
                   
                    if (newNotification > oldNotification)
                    {
                        notifiCount = (newNotification - oldNotification).ToString();
                    }
                }

                return Json(notifiCount, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}