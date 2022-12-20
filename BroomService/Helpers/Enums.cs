using BroomService.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.Helpers
{
    public class Enums
    {
        public enum UserTypeEnum
        {
            Admin = 1,
            ServiceProvider = 2,
            Worker = 3,
            Customer = 4,
            Supervisor = 5,
            GrantAccessUser =6,
            SubUser=7,
            DeliveryGuy = 8,
            Laundry = 9
        }

        public enum ServiceRequestTypeEnum
        {
            Morning1=1,
            Morning2=2,
            Noon1=3,
            Noon2=4,
            Noon3=5
        }

        public enum JobServiceRepeatList
        {
            OnceAWeek=1,
            EveryMonth = 2,
            EveryYear = 3,
        }

        public enum RequestStatus
        {
            
            Pending = 1,
            InProgress = 2,
            Completed = 3,
            Canceled = 4,
            QuoteRequested = 5,
            QuotePriced = 6,
            UnPaid = 7,
            QuoteRejected = 8,
            QuoteCanceled = 9
        }

        public enum LaundryStatus
        {
            Requested = 1,
            Pickedup = 2,
            LaundryReceived = 3,
            Priced = 4,
            Paid = 5,
            DeliveryReceived = 6,
            Delivered = 7
        }

        public enum InventoryStatus
        {
            Requested = 1,
            Paid = 2,
            Shipped = 3,
            Delivered = 4
        }

        public enum RejectJobRequest
        {
            Pending = 1,
            Accepted = 2,
            Rejected = 3
        }

        public enum Notify
        {
            False = 0,
            True = 1
        }

        public enum NotificationStatus
        {
            Pending = 1,
            Accepted = 2,
            Rejected = 3,
            SentQuotation = 4,
            AcceptedQuotation = 5,
            RejectedQuotation = 6,
            TimerStarted = 7,
            TimerEnded = 8,
            Completed = 9,
            Assigned = 10,
            ServiceBySuperVisor = 11,
            SubUserRequestService=12,
            SupervisorAccept=13,
            SupervisorReject=14,
            SubUserServiceAccepted=15,
            SubUserServiceRejected=16,
            WorkerNotAvailable=17,
            ContactSupport=18,
            SupervisorTimeChange = 19,
            SupervisorTimeAccept = 20,
            SupervisorTimeReject = 21
        }

        public enum JobRequestPayType
        {
            BeforeJob = 1,
            AfterJob = 2,
            OnceMonth = 3
        }

        public enum PaymentMethod
        {
            ByCreditCard = 1,
            ByCash = 2,
            ByCheque = 3,
            ByMoneyTransfer = 4
        }

        public enum RepeatServiceEnum
        {
            OnceMonth = 1,
            OnceWeek = 2,
            TwiceWeek = 3        }
        public enum ChannelManager
        {
            Smoobu = 1
        }

        public enum QuoteTypeEnum
        {
            Meeting = 1,
            SendPrice = 2
        }

        public static string GetPaymentMethod(int method)
        {
            string status = string.Empty;
            if (method == (int)PaymentMethod.ByCash)
            {
                status = Resource.by_cash;
            }
            else if (method == (int)PaymentMethod.ByCheque)
            {
                status = Resource.by_cheque;
            }
            else if (method == (int)PaymentMethod.ByMoneyTransfer)
            {
                status = Resource.by_money_transfer;
            }
            else
            {
                status = Resource.by_credit_card;
            }
            return status;
        }

    }
}
