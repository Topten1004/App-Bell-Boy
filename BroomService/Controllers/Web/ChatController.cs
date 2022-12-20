using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BroomService.ViewModels;
using static BroomService.ViewModels.ChatViewModel;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.ObjectModel;
using Remotion.Data.Linq.Collections;
using Firebase.Database;
using Firebase.Database.Query;
using BroomService.Resources;
using BroomService.Helpers;
using BroomService.bin.Controllers.Web;

namespace BroomService.Controllers
{
    public class ChatController : MyController
    {
        ChatService chatService;
        private List<ChatDetailListModel> chatlistdetail = new List<ChatDetailListModel>();
        public static FirebaseClient firebase = new FirebaseClient("https://broomserviceapp-1cf32.firebaseio.com/");
        public ChatController()
        {
            chatService = new ChatService();                
        }
       
        public  async Task<ActionResult> GetChatForUserID(string senderUserId, string recieverUserId)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create("https://broomserviceapp-1cf32.firebaseio.com/Chat/" + senderUserId + "/" + recieverUserId + ".json");
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string urlText = reader.ReadToEnd(); // it takes the response from your url. now you can use as your need  
                if(urlText != null && urlText != "null")
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, ChatDetailListModel>>(urlText).ToList();
                    chatlistdetail.Add(data[data.Count - 1].Value);
                }
                else
                {
                    ChatDetailListModel obj = new ChatDetailListModel();
                    chatlistdetail.Add(obj);
                }
                return View();
            }
            catch(Exception ex)
            {
                throw;
            }
        }


        [HttpGet]
        public ActionResult ChatList()
        {
            ChatListVM chatListVM = new ChatListVM();
            ChatlastMessage chatlastMessage = new ChatlastMessage();
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
                try
                {
                    var result =  chatService.GetChat(userId);
                    if (result != null)
                    {
                        if (result.Count > 0)
                        {
                            chatListVM.chatUser = result;

                            for (int m = 0; m < chatListVM.chatUser.Count; m++)
                            {
                                GetChatForUserID(userId.ToString(), chatListVM.chatUser[m].UserId.ToString());
                            }
                        }
                    }
                    ViewBag.lastMess = chatlistdetail;
                   return View(chatListVM);                 
                }
                catch(Exception ex)
                {
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public ActionResult ChatListForReport()
        {
            ChatListVM chatListVM = new ChatListVM();
            ChatlastMessage chatlastMessage = new ChatlastMessage();
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
                try
                {
                    var result = chatService.GetChat(userId);
                    if (result != null)
                    {
                        if (result.Count > 0)
                        {
                            chatListVM.chatUser = result;

                            for (int m = 0; m < chatListVM.chatUser.Count; m++)
                            {
                                GetChatForUserID(userId.ToString(), chatListVM.chatUser[m].UserId.ToString());
                            }
                        }
                    }
                    ViewBag.lastMess = chatlistdetail;
                    return View(chatListVM);
                }
                catch (Exception ex)
                {
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public JsonResult SendMessage(long userId)
        {
            if (userId != 0)
            {
                int userID = 0;
                if (Request.Cookies["Login"] != null)
                {
                    if (Request.Cookies["Login"].Values["UserId"] != null)
                    {
                        userID = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    }
                }
                ChatRequestModel m = new ChatRequestModel()
                {
                    FromUserId = userID,
                    ToUserId =  userId
                };
                chatService.AddChatRequest(m);
                wsBase wsBase = new wsBase
                {
                    status = true
                };
                return Json(wsBase, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Session Expired", JsonRequestBehavior.AllowGet);
            }
        }

    }
}