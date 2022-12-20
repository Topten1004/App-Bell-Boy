using BroomService.Helpers;
using BroomService.Resources;
using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static BroomService.ViewModels.ChatViewModel;

namespace BroomService.ApiControllers
{
    public class ChatController : ApiController
    {
        ChatService chatService;

        public ChatController()
        {
            chatService = new ChatService();
        }

        /// <summary>
        /// Add ChatRequest Api
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddChatRequest(ChatRequestModel model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var status = chatService.AddChatRequest(model);

            return this.Ok(new
            {
                status = status,
                message = chatService.message,
            });
        }
        [HttpPost]
        public IHttpActionResult AddLastMessage(UserLastMessage model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var status = chatService.AddLastMessage(model);

            return this.Ok(new
            {
                status = status,
                message = chatService.message,
            });
        }

        /// <summary>
        /// Get Chat for all the users Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetChat(long userId)
        {
            var result = chatService.GetChat(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = chatService.message,
                data = result
            });
        }
        
      /*  [HttpGet]
        public IHttpActionResult PushNotification(int? usertype, int? deviceid, string devicetoken, string message)
        {
            if (!string.IsNullOrEmpty(devicetoken))
            {
                string appTitle = usertype == (int)Enums.UserTypeEnum.Customer ? Resource.broom_service : "bs_crew";
                string serverKey = string.Empty;
                //USE FOR Customer Key
                //serverKey = "AAAAotg6m_0:APA91bHaXoRugZ0YrNVjrSWlDk8F7KdgShWAKH-TCnA4OysL44qlk6Nc9xZjT6WCA_PIr46ioez_1H7ZpE7jrmOQkhCJ0ETdPHezHPmKFWyzZ26wX7dUmblvfImGHxnN9gwOXwWe_qyu";

                //Use for Worker key
                serverKey = "AAAAVRNtLW8:APA91bGZmIvwtAMPTR_p25oEjTlQVFnsOg1U8wF9gzgBIGeSQjOk899EMMVMZqX5thV3rC9JWpaUBRZmH6XlBWFzS8ZNwTe3SAle1BnMUWPGyJUNgJDehn7fWznoJXgv4j4_9CWgg4SA";
                var result21 = "-1";
                var webAddr = "https://fcm.googleapis.com/fcm/send";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization:key=" + serverKey);
                httpWebRequest.Method = "POST";
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = string.Empty;
                    if (deviceid == 1)
                    {
                        json = "{\"to\":\"" + devicetoken + "\",\"notification\":{\"title\":\"" + appTitle + "\",\"body\":\"" + message + "\"},\"priority\":\"high\"}";
                    }
                    else
                    {
                        json = "{\"to\": \"" + devicetoken + "\",\"notification\": {\"body\": \"" + message + "\",\"sound\": \"default\",\"content_available\": true}}";
                    }
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    result21 = streamReader.ReadToEnd();
                }
                return this.Ok();
            }
            return Ok();
        }
        */
    }
}
