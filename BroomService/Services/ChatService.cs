using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static BroomService.ViewModels.ChatViewModel;

namespace BroomService.Services
{
    public class ChatService
    {
        BroomServiceEntities1 _db;

        public ChatService()
        {
            _db = new BroomServiceEntities1();
        }
        public string message=string.Empty;

        /// <summary>
        /// Add ChatRequest Method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddChatRequest(ChatRequestModel model)
        {
            bool status = false;
            try
            {
                var chatRequestData = _db.UserChats.Where(a => (a.FromUserId == model.FromUserId && a.ToUserId == model.ToUserId)
                   || (a.FromUserId == model.ToUserId && a.ToUserId == model.FromUserId)).FirstOrDefault();
                if (chatRequestData == null)
                {
                    UserChat chat = new UserChat();
                    chat.CreatedDate = DateTime.Now;
                    chat.FromUserId = model.FromUserId;
                    chat.ToUserId = model.ToUserId;

                    _db.UserChats.Add(chat);
                    _db.SaveChanges();
                    status = true;
                    message = Resource.success;
                }
                else
                {
                    chatRequestData.CreatedDate = DateTime.Now;
                    _db.SaveChanges();
                    status = true;
                    message = Resource.success;
                }
                var toUserDta = _db.Users.Where(x => x.UserId == model.ToUserId).FirstOrDefault();
                if (toUserDta != null && toUserDta.DeviceId != null && !string.IsNullOrEmpty(toUserDta.DeviceToken))
                {
                    var fromUserDta = _db.Users.Where(x => x.UserId == model.FromUserId).FirstOrDefault();
                    if (fromUserDta != null)
                    {
                        string msg = Resource.you_have_new_msg + " " + fromUserDta.FullName;
                        Common.PushNotification(toUserDta.UserType, toUserDta.DeviceId, toUserDta.DeviceToken, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }
        public bool AddLastMessage(UserLastMessage model)
        {
            bool status = false;
            try
            {
                var data = _db.UserChats.Where(x => x.FromUserId == model.FromUserId && x.ToUserId == model.ToUserId ||
                 x.FromUserId == model.ToUserId && x.ToUserId == model.FromUserId).FirstOrDefault();
                if(data != null)
                {
                    data.LastMessage = model.LastMessage;
                    data.LastMsgDate = model.CreatedDate;
                    _db.SaveChanges();
                }
                status = true;
                message = Resource.success;
            }
            catch(Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            return status;
        }

        /// <summary>
        /// Method to get the chat list
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ChatUser> GetChat(long userId)
        {
            List<ChatUser> lstData = null;
            try
            {

                lstData = _db.UserChats.Where(x => x.FromUserId == userId || x.ToUserId == userId)
                    .Select
                    (x => new ChatUser()
                    {
                        Name = x.FromUserId == userId ?!string.IsNullOrEmpty(x.User1.FullName)?
                        x.User1.FullName: x.User1.Email:!string.IsNullOrEmpty(x.User.FullName)?
                        x.User.FullName:x.User.Email,
                        UserId = x.FromUserId == userId ? x.ToUserId : x.FromUserId,
                        PicturePath = x.FromUserId == userId ? x.User1.PicturePath : x.User.PicturePath != null ? x.User.PicturePath : "UserDefault.jpg",
                        CreatedDate = x.CreatedDate,
                        UserType = x.User.UserType,
                        LastMessage= x.LastMessage,
                        LastMsgDate=x.LastMsgDate,
                        IsVisibleMessage=true,
                        IsRatedUser=_db.UserReviews.Any(a=>a.ToUserId==x.FromUserId&&a.CustomerId==x.ToUserId)
                    }).OrderByDescending(x => x.CreatedDate).ToList();
                message = Resource.success;

                foreach (var item in lstData)
                {
                    var data = _db.Users.Where(a => a.UserId == item.UserId).FirstOrDefault();
                    if (data != null)
                    {
                        item.UserType = data.UserType;                     
                    }
                }

                if (lstData==null || lstData.Count==0)
                {
                    UserChat Admin = new UserChat();
                    Admin.FromUserId = userId;
                    Admin.ToUserId =1;
                    Admin.CreatedDate = DateTime.Now;
                    _db.UserChats.Add(Admin);
                    _db.SaveChanges();

                    var TypeSupervisor = Enums.UserTypeEnum.Supervisor.GetHashCode();
                    var SupervisorData = _db.Users.Where(x => x.UserType == TypeSupervisor).FirstOrDefault();
                    if(SupervisorData != null)
                    {
                        UserChat Supervisor = new UserChat();
                        Supervisor.FromUserId = userId;
                        Supervisor.ToUserId = SupervisorData.UserId;
                        Supervisor.CreatedDate = DateTime.Now;
                        _db.UserChats.Add(Supervisor);
                        _db.SaveChanges();
                    }                    

                    lstData = _db.UserChats.Where(x => x.FromUserId == userId || x.ToUserId == userId)
                    .Select
                    (x => new ChatUser()
                    {
                        Name = x.FromUserId == userId ? !string.IsNullOrEmpty(x.User1.FullName) ?
                        x.User1.FullName : x.User1.Email : !string.IsNullOrEmpty(x.User.FullName) ?
                        x.User.FullName : x.User.Email,
                        UserId = x.FromUserId == userId ? x.ToUserId : x.FromUserId,
                        PicturePath = x.FromUserId == userId ? x.User1.PicturePath : x.User.PicturePath != null ? x.User.PicturePath : "UserDefault.jpg",
                        CreatedDate = x.CreatedDate,
                        UserType = x.User.UserType,
                        LastMessage = x.LastMessage,
                        LastMsgDate = x.LastMsgDate,
                        IsRatedUser = _db.UserReviews.Any(a => a.ToUserId == x.FromUserId && a.CustomerId == x.ToUserId)
                    }).OrderByDescending(x => x.CreatedDate).ToList();
                    message = Resource.success;
                }
                else
                {
                    // Add supervisor to user chat
                    var datasup = lstData.Where(x => x.UserType == Enums.UserTypeEnum.Supervisor.GetHashCode()).FirstOrDefault();
                    var TypeSupervisor = Enums.UserTypeEnum.Supervisor.GetHashCode();
                    var SupervisorData = _db.Users.Where(x => x.UserType == TypeSupervisor).FirstOrDefault();
                    if (SupervisorData != null)
                    {
                        if(datasup==null)
                        {
                            UserChat user = new UserChat();
                            user.FromUserId = userId;
                            user.ToUserId = SupervisorData.UserId;
                            user.CreatedDate = DateTime.Now;
                            _db.UserChats.Add(user);
                            _db.SaveChanges();
                        }                       
                    }
                    // Add Admin to user chat
                    var dataAdmin = lstData.Where(x => x.UserType == Enums.UserTypeEnum.Admin.GetHashCode()).FirstOrDefault();
                    if(dataAdmin ==null)
                    {
                        UserChat user = new UserChat();
                        user.FromUserId = userId;
                        user.ToUserId = 1;
                        user.CreatedDate = DateTime.Now;
                        _db.UserChats.Add(user);
                        _db.SaveChanges();
                    }
                    lstData = _db.UserChats.Where(x => x.FromUserId == userId || x.ToUserId == userId)
                    .Select
                    (x => new ChatUser()
                    {
                        Name = x.FromUserId == userId ? !string.IsNullOrEmpty(x.User1.FullName) ?
                        x.User1.FullName : x.User1.Email : !string.IsNullOrEmpty(x.User.FullName) ?
                        x.User.FullName : x.User.Email,
                        UserId = x.FromUserId == userId ? x.ToUserId : x.FromUserId,
                        PicturePath = x.FromUserId == userId ? x.User1.PicturePath : x.User.PicturePath != null ? x.User.PicturePath : "UserDefault.jpg",
                        CreatedDate = x.CreatedDate,
                        UserType = x.User.UserType,
                        LastMessage = x.LastMessage,
                        LastMsgDate = x.LastMsgDate,
                        IsRatedUser = _db.UserReviews.Any(a => a.ToUserId == x.FromUserId && a.CustomerId == x.ToUserId)
                    }).OrderByDescending(x => x.CreatedDate).ToList();
                    message = Resource.success;
                }
                foreach(var item in lstData)
                {
                    var data = _db.Users.Where(a => a.UserId == item.UserId).FirstOrDefault();
                    if(data !=null)
                    {
                        item.UserType = data.UserType;
                        item.Name = data.FullName != null ? data.FullName : data.CompanyName;
                        item.PicturePath = data.PicturePath!= null ? data.PicturePath : "UserDefault.jpg";

                        if (data.CountryCode !=null && data.PhoneNumber != null)
                        {
                            item.PhoneNumber = data.CountryCode + data.PhoneNumber;
                        }
                        else if(data.PhoneNumber != null)
                        {
                            item.PhoneNumber = data.PhoneNumber;
                        }
                        else
                        {
                            item.PhoneNumber = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }
    }
}