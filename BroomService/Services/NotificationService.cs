using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.Services
{
    public class NotificationService
    {
        BroomServiceEntities1 _db;
        AccountService accountService;
        public NotificationService()
        {
            _db = new BroomServiceEntities1();
            accountService = new AccountService();
        }
        public string message = string.Empty;

        /// <summary>
        /// Getting the list of the notifications
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<NotificationViewModel> GetNotifications(long userId)
        {
            List<NotificationViewModel> lstData = null;
            List<Notification> lstNoti = new List<Notification>();
            try
            {
                var user = _db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    lstNoti = _db.Notifications.Where(x => x.ToUserId == userId && x.IsActive == true).ToList();

                    var adminId = accountService.GetAdminId();
                    if (lstNoti.Count > 0)
                    {
                        lstData = new List<NotificationViewModel>();
                        foreach (var x in lstNoti)
                        {
                            NotificationViewModel notificationViewModel = new NotificationViewModel();
                            notificationViewModel.CreatedDate = x.CreatedDate;
                            notificationViewModel.FromUserId = x.FromUserId;
                            notificationViewModel.FromUserName = x.FromUserId != null ? (x.FromUserId == adminId) ? Resource.broom_service : x.User.FullName : string.Empty;
                            notificationViewModel.FromUserImage = x.User != null ? x.User.PicturePath : string.Empty;
                            notificationViewModel.ToUserId = x.ToUserId;
                            notificationViewModel.ToUserName = x.User1 != null ? x.User1.FullName : "";
                            notificationViewModel.ToUserImage = x.User1 != null ? x.User1.PicturePath : "";
                            notificationViewModel.Id = x.Id;
                            notificationViewModel.JobRequestId = x.JobRequestId;
                            notificationViewModel.NotificationStatus = x.NotificationStatus;
                            notificationViewModel.Text = x.Text;
                            notificationViewModel.QuotePrice = x.QuotePrice;
                            notificationViewModel.ServiceName = x.ServiceName;
                            notificationViewModel.PropertyAddress = x.PropertyAddress;
                            notificationViewModel.PropertyName = x.PropertyName;
                            lstData.Add(notificationViewModel);
                        }
                    };
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }

        public int changeNotificationAlertIcon(long AdminId)
        {
            int notificationList;
            List<Notification> lstNoti = new List<Notification>();
            try
            {
                lstNoti = _db.Notifications.Where(x => x.ToUserId == AdminId && x.IsActive == true).ToList();
                notificationList = lstNoti.Count;
            }
            catch (Exception ex)
            {
                notificationList = 0;
                message = ex.Message;
            }
            return notificationList;
        }

        public NotificationViewModel GetNotification(long id)
        {
            
            Notification  Noti = new Notification();
            NotificationViewModel notificationViewModel = new NotificationViewModel();
            try
            {
                Noti = _db.Notifications.Where(x => x.Id == id && x.IsActive == true).FirstOrDefault();

                    var adminId = accountService.GetAdminId();
                    if (Noti != null)
                    {  
                           
                            notificationViewModel.CreatedDate = Noti.CreatedDate;
                            notificationViewModel.FromUserId = Noti.FromUserId;
                            notificationViewModel.FromUserName = Noti.FromUserId != null ? (Noti.FromUserId == adminId) ? Resource.broom_service : Noti.User.FullName : string.Empty;
                            notificationViewModel.FromUserImage = Noti.User != null ? Noti.User.PicturePath : string.Empty;
                            notificationViewModel.ToUserId = Noti.ToUserId;
                            notificationViewModel.ToUserName = Noti.User1 != null ? Noti.User1.FullName : "";
                            notificationViewModel.ToUserImage = Noti.User1 != null ? Noti.User1.PicturePath : "";
                            notificationViewModel.Id = Noti.Id;
                            notificationViewModel.JobRequestId = Noti.JobRequestId;
                            notificationViewModel.NotificationStatus = Noti.NotificationStatus;
                            notificationViewModel.Text = Noti.Text;
                            notificationViewModel.QuotePrice = Noti.QuotePrice;
                            notificationViewModel.ServiceName = Noti.ServiceName;
                            notificationViewModel.PropertyAddress = Noti.PropertyAddress;
                            notificationViewModel.PropertyName = Noti.PropertyName;
                    };
                
                message = Resource.success;
            }
            catch (Exception ex)
            {
               
                message = ex.Message;
            }
            return notificationViewModel;
        }
    }
}