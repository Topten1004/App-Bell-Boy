using BroomService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class UserViewModel

    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public long? CountryId { get; set; }
        public string CountryCode { get; set; }
        public string BillingAddress { get; set; }
        public string PicturePath { get; set; }
        public int? DeviceId { get; set; }
        public string Address { get; set; }
        public string DeviceToken { get; set; }
        public int? UserType { get; set; }
        public int? PaymentMethod { get; set; }
        public int? JobTypeMethod { get; set; }
        public bool? OrderServicesAccess { get; set; }
        public bool? AddEditPropertiesAccess { get; set; }
        public bool? BillingPriceAccess { get; set; }
        public bool? AddChangeCardAccess { get; set; }
        public long? ClientId { get; set; }
        public string PaymentMethodName
        {
            get
            {
                return PaymentMethod != null ? Enums.GetPaymentMethod(PaymentMethod ?? 1) : "";
            }
        }
        public string WorkerQuesType { get; set; }
    }
    public class SaveLocation
    {
        public long UserId { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
    public class ServiceQuoteViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        public string UserName { get; set; }
        public long PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public long? SubCategoryId { get; set; }
        public long? SubSubCategoryId { get; set; }
        public decimal QuotePrice { get; set; }
        public bool IsQuoteApproved { get; set; }
        public bool IsQuoted { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
    public class AddServiceRequestViewModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string ServiceName { get; set; }
        public int? SubCatId { get; set; }
        public int? SubSubCatId { get; set; }
        public long? PropertId { get; set; }
        public long? UserId { get; set; }
        public string JobStartDateTime { get; set; }
        public long? AssignWorker { get; set; }
        public string Message { get; set; }
        public bool? IsAccepted { get; set; }
        public decimal? ServicePrice { get; set; }
    }
}