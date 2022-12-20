using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class ChatViewModel
    {
        public class ChatRequestModel
        {
            public long? FromUserId { get; set; }
            public long? ToUserId { get; set; }
        }
        public class ChatUser
        {
            public bool IsRatedUser { get; set; }
            public int? UserType { get; set; }
            public long? UserId { get; set; }
            public string Name { get; set; }
            public string PicturePath { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int SelectUserId { get; set; }
            public string PhoneNumber { get; set; }
            public string LastMessage { get; set; }
            public string LastMsgDate { get; set; }
            public bool  IsVisibleMessage { get; set; }
        }
        public class ChatListVM
        {
            public List<ChatUser> chatUser { get; set; }
            public List<ChatDetailListModel> listChat { get; set; }
            public int SelectUserId { get; set; }
        }

        public class ChatDetailListModel
        {
            public long SenderUserId { get; set; }
            public long RecieverUserId { get; set; }
            public string UserMessage { get; set; }
            public string UserMessageTime { get; set; }
            public bool IsSender { get; set; }
            public object TimeStamp { get; set; }
        }

        public class ChatlastMessage
        {
            public List<ChatDetailListModel> listChat { get; set; }

        }
        public class UserLastMessage
        {
            public long? FromUserId { get; set; }
            public long? ToUserId { get; set; }
            public string LastMessage { get; set; }
            public string CreatedDate { get; set; }
        }
    }
}