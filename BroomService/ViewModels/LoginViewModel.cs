using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class LoginViewModel
    {
        public long UserId { get; set; }
        //public string CompanyName { get; set; }
        public string PicturePath { get; set; }
        public int? UserType { get; set; }
        public int? JobPayType { get; set; }
        public int? PaymentMethod { get; set; }
        public long? Client_Id { get; set; }
        //public string BillingAddress { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public bool? PhoneVerified  { get; set; }        
        public bool? EmailVerified { get; set; }
        public bool? IsActive { get; set; }
        public bool Locked { get; set; }
    }
}