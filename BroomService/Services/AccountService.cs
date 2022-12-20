using BroomService.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BroomService.Models;
using BroomService.Helpers;
using BroomService.ViewModels;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.Message;
using Twilio.Types;
using System.Configuration;

namespace BroomService.Services
{
    public class AccountService
    {
        BroomServiceEntities1 _db;
        public AccountService()
        {
            _db = new BroomServiceEntities1();
        }
        public string message = string.Empty;
        public string SMSCode = String.Empty;

        public long GetAdminId()
        {
            long userId = 0;
            try
            {
                var usertypeA = Enums.UserTypeEnum.Admin.GetHashCode();

                var user = _db.Users.Where(x => x.UserType == usertypeA).FirstOrDefault();
                if (user != null)
                {
                    userId = user.UserId;
                }
            }
            catch (Exception ex)
            {
            }
            return userId;
        }

        /// <summary>
        /// Sign Up Method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public User SignUp(User model)
        {
            string _path = "https://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Images/User/";
            try
            {
                // check if email already exist
                var getExistUserEmail = _db.Users.Where(x => x.Email == model.Email && x.IsActive == true).FirstOrDefault();
                if (getExistUserEmail != null)
                {
                    model = null;
                    message = Resource.email_already_exists;
                    return model;
                }

                model.IsActive = false;
                model.CreatedDate = DateTime.Now;
                model.EmailVerified = false;
                model.PhoneVerified = false;

                // Generating encrypted password
                string salt = string.Empty;
                string encryptedPswd = string.Empty;
                Common.GeneratePassword(model.Password, "new", ref salt, ref encryptedPswd);

                model.Password = encryptedPswd;
                model.PasswordSalt = salt;
                model.PaymentMethod = Enums.PaymentMethod.ByCreditCard.GetHashCode();
                model.JobPayType = Enums.JobRequestPayType.BeforeJob.GetHashCode();
                _db.Users.Add(model);
                _db.SaveChanges();
                message = Resource.confirmation_email_sent; //"You have successfully registered.";
                //Common.SendSignupConfirmationEmail(model.FullName, model.Email, model.UserId.ToString());
                //var phoneNumber = String.Format("{0}{1}", model.CountryCode, model.PhoneNumber).Replace("+", " ");
                //SMSVerification(phoneNumber, model.UserId.ToString());
                model.PicturePath = _path + model.PicturePath;
                return model;

            }
            catch (Exception ex)
            {
                model = null;
            }
            return model;
        }

        public User CreateAccountGrantUser(User model)
        {
            try
            {
                model.IsActive = true;
                model.CreatedDate = DateTime.Now;
                model.EmailVerified = true;

                // Generating encrypted password
                string salt = string.Empty;
                string encryptedPswd = string.Empty;
                Common.GeneratePassword(model.Password, "new", ref salt, ref encryptedPswd);

                model.Password = encryptedPswd;
                model.PasswordSalt = salt;
                model.IsGrantUser = true;
                model.PaymentMethod = Enums.PaymentMethod.ByCreditCard.GetHashCode();
                model.JobPayType = Enums.JobRequestPayType.BeforeJob.GetHashCode();

                _db.Users.Add(model);
                _db.SaveChanges();

                // getting grant access property data
                var grantAccessData = _db.tblGrantAccesses.Where(a => a.GrantAccessId == model.DeviceId).FirstOrDefault();
                if (grantAccessData != null)
                {
                    grantAccessData.IsAccountAccepted = true;
                    grantAccessData.UserId = model.UserId;
                }

                _db.SaveChanges();

                message = Resource.account_created_successfully;
                return model;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                model = null;
            }

            return model;
        }

        public User GetCurrentUser(long userId)
        {
            return _db.Users.FirstOrDefault(a => a.UserId == userId);
        }

        public LoginViewModel GetLoginUserWeb(User model)
        {
            LoginViewModel user = null;
            string _path = "https://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Images/User/";
            try
            {
                //var usertypeA = Enums.UserTypeEnum.Admin.GetHashCode();
                int usertypeA = Enums.UserTypeEnum.Customer.GetHashCode();
                int grandaccessUser = Enums.UserTypeEnum.GrantAccessUser.GetHashCode();
                int subuser = Enums.UserTypeEnum.SubUser.GetHashCode();
                user = _db.Users.Where(i => i.Email == model.Email && (i.UserType == usertypeA || i.UserType == grandaccessUser || i.UserType == subuser)).Select(u => new LoginViewModel()
                {
                    UserId = u.UserId,
                    //BillingAddress = u.BillingAddress,
                    Client_Id = u.Client_Id,
                    //CompanyName = u.CompanyName,
                    JobPayType = u.JobPayType,
                    PaymentMethod = u.PaymentMethod,
                    PicturePath = u.PicturePath,
                    UserType = u.UserType,
                    Email = u.Email,
                    FullName = u.FullName,
                    Password = u.Password,
                    PasswordSalt = u.PasswordSalt,
                    PhoneVerified = u.PhoneVerified,
                    EmailVerified = u.EmailVerified,
                    IsActive = u.IsActive,
                    Locked = u.Locked
                }).FirstOrDefault();

                if (user == null)
                {
                    message = Resource.please_enter_valid_email;
                    return null;
                }
                var gg = Common.DecryptString(user.Password, user.PasswordSalt);
                if (Common.EncryptString(model.Password, user.PasswordSalt) != user.Password)
                {
                    message = Resource.please_enter_valid_password;
                    return null;
                }
                if (user.PhoneVerified == false && user.EmailVerified == false)
                {

                    message = user.FullName + " , " + Resource.verify_email_before_signing;
                    user = null;
                    return user;
                }
                if (user.IsActive == false)
                {

                    message = user.FullName + "," + Resource.account_deactivate_message;
                    user = null;
                    return user;
                }
                if (user.Locked == true)
                {

                    message = user.FullName + "," + Resource.account_locked_message;
                    user = null;
                    return user;
                }


                message = Resource.user_logged_in_successfully;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                user = null;
            }
            return user;
        }

        /// <summary>
        /// Login Method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public UserViewModel GetLoginUserApp(User model)
        {
            UserViewModel obj = null;
            try
            {
                var usertypeA = Enums.UserTypeEnum.Admin.GetHashCode();
                var usertypeS = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var user = _db.Users.Where(i => i.Email == model.Email && !(i.UserType == usertypeA) && !(i.UserType == usertypeS)).FirstOrDefault();

                if (user != null)
                {
                    user.DeviceId = model.DeviceId;
                    user.DeviceToken = model.DeviceToken;
                    _db.SaveChanges();

                    var gg = Common.DecryptString(user.Password, user.PasswordSalt);
                    if (Common.EncryptString(model.Password, user.PasswordSalt) == user.Password)
                    {
                        if (user.IsActive == false)
                        {
                            obj = null;
                            message = user.FullName + " " + " , " + Resource.account_deactivate_message;
                            return obj;
                        }
                        if (user.EmailVerified == false)
                        {
                            obj = null;
                            message = user.FullName + " " + " , " + Resource.verify_email_before_signing;
                            return obj;
                        }
                        obj = new UserViewModel()
                        {
                            UserId = user.UserId,
                            FullName = user.FullName,
                            DeviceId = user.DeviceId,
                            DeviceToken = user.DeviceToken,
                            Email = user.Email,
                            Address = user.Address,
                            PhoneNumber = user.PhoneNumber,
                            UserType = user.UserType,
                            PicturePath = !string.IsNullOrEmpty(user.PicturePath) ? user.PicturePath : string.Empty,
                            CompanyName = user.CompanyName,
                            BillingAddress = user.BillingAddress,
                            PaymentMethod = user.PaymentMethod,
                            JobTypeMethod = user.JobPayType,
                            WorkerQuesType = user.WorkerQuesType
                        };

                        // In case the user is of grant access type
                        if (user.UserType == Enums.UserTypeEnum.GrantAccessUser.GetHashCode())
                        {
                            var grantAccessData = _db.tblGrantAccesses.Where(a => a.UserId == user.UserId).FirstOrDefault();

                            if (grantAccessData != null)
                            {
                                obj.OrderServicesAccess = grantAccessData.OrderServicesAccess;
                                obj.AddChangeCardAccess = grantAccessData.AddChangeCardAccess;
                                obj.AddEditPropertiesAccess = grantAccessData.AddEditPropertiesAccess;
                                obj.BillingPriceAccess = grantAccessData.BillingPriceAccess;
                            }
                        }

                        message = Resource.user_logged_in_successfully;
                    }
                    else
                    {
                        obj = null;
                        message = Resource.please_enter_valid_password;
                    }
                }
                else
                {
                    obj = null;
                    message = Resource.please_enter_valid_email;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                obj = null;
            }
            return obj;
        }

        public VerificationViewModel GetUser(string userId)
        {
            var _userId = Convert.ToInt64(userId);
            return _db.Users.Where(a => a.UserId == _userId).Select(v => new VerificationViewModel
            {
                UserId = v.UserId,
                FullName = v.FullName,
                Email = v.Email
            }).FirstOrDefault();
        }

        public void Remove(long userId)
        {
            var user = _db.Users.Where(u => u.UserId == userId).FirstOrDefault();
            if (user != null && user.UserId > 0)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
            }
        }

        public bool CheckVerifyEmail(string userId)
        {
            bool status = false;
            try
            {
                var _userId = Convert.ToInt64(userId);
                var user = _db.Users.Where(a => a.UserId == _userId
                && a.EmailVerified != true).FirstOrDefault();
                if (user != null)
                {
                    user.EmailVerified = true;
                    _db.SaveChanges();
                    status = true;
                }
            }
            catch (Exception ex)
            {

            }

            return status;
        }
        public bool SaveIcountClientId(long ClientId, long UserId)
        {
            try
            {
                var UserData = _db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                if (UserData != null)
                {
                    UserData.Client_Id = ClientId;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool CheckVerifySMS(string userID, string OtpCode = "")
        {
            bool status = false;
            try
            {                var _userId = Convert.ToInt64(userID);
                int result = _db.Database.ExecuteSqlCommand("Update [user] SET PhoneVerified = 1 WHERE OtpCode = '" + OtpCode + "' AND UserId = " + _userId);
                //var user = _db.Users.FirstOrDefault(a => a.UserId == _userId && a.OtpCode == OtpCode);
                if (result > 0)
                {
                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        public bool SMSVerification(string phoneNumber, string userID)
        {
            String randomOtp = "";
            try
            {
                string TwilioAccountSID = Convert.ToString(ConfigurationManager.AppSettings["TwilioAccountSID"]);
                string TwilioAuthToken = Convert.ToString(ConfigurationManager.AppSettings["TwilioAuthToken"]);


                TwilioClient.Init(TwilioAccountSID, TwilioAuthToken);

                Random generate = new Random();
                randomOtp = generate.Next(0, 9999).ToString("D6");

                var messageTwilio = MessageResource.Create(
                    body: "Your OTP code is " + randomOtp + " For UserID " + userID,
                     from: new PhoneNumber("+972526954864"),
                    to: new PhoneNumber("+" + phoneNumber),
                    provideFeedback: true
                );

                Console.WriteLine("Twilio " + messageTwilio);
                Console.WriteLine(messageTwilio.Sid);
                message = messageTwilio.Sid;
                SMSCode = randomOtp;
                //if (messageTwilio.Sid.Length > 9)
                //{
                //    return true;
                //}
            }
            catch (Exception ex)
            {
                return false;
            }
            SaveOtp(userID, randomOtp);
            return true;
        }

        public void SaveOtp(string userID, string otpCode)
        {
            try
            {
                var _userId = Convert.ToInt64(userID);
                var user = _db.Users.Where(a => a.UserId == _userId && (a.PhoneVerified == null || a.PhoneVerified == false)).FirstOrDefault();
                if (user != null)
                {
                    user.OtpCode = otpCode;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public bool ForgotPassword(string email)
        {
            bool status = false;
            try
            {
                var result = _db.Users.Where(x => x.Email == email).FirstOrDefault();
                if (result != null)
                {
                    string newPass = Common.FetchRandString(6);
                    bool passCheck = Common.SendEmailResetPassword(result.FullName, result.Email, newPass);
                    if (passCheck == true)
                    {
                        string salt = string.Empty;
                        string encryptedPswd = string.Empty;
                        Common.GeneratePassword(newPass, "new", ref salt, ref encryptedPswd);

                        result.PasswordSalt = salt;
                        result.Password = encryptedPswd;
                        _db.SaveChanges();

                        message = Resource.new_password_sent_success;
                        status = true;
                    }
                    else
                    {
                        message = Resource.try_again;
                    }
                }
                else
                {
                    message = Resource.error_requesting_password;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }
        public TermsCondition GetTermsConditions()
        {
            TermsCondition data = null;
            try
            {
                data = _db.TermsConditions.FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return data;
        }
        public bool UpdateDeviceInfo(long userId, int deviceId, string deviceToken)
        {
            bool status = false;
            try
            {
                var user = _db.Users.Where(i => i.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    user.DeviceId = deviceId;
                    user.DeviceToken = deviceToken;
                    _db.SaveChanges();
                    status = true;
                    message = Resource.success;
                }

                else
                {
                    message = Resource.user_not_exist;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        /// <summary>
        /// Logout Method.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool Logout(long userId)
        {
            bool status = false;
            try

            {
                var user = _db.Users.Where(i => i.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    user.DeviceToken = string.Empty;
                    _db.SaveChanges();
                    status = true;
                    message = Resource.success;
                }
                else
                {
                    message = Resource.user_not_exist;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public bool ChangePassword(ChangePasswordViewModel model)
        {
            bool status = false;
            try
            {
                var result = _db.Users.Where(x => x.UserId == model.userId && x.IsActive == true).FirstOrDefault();
                if (result != null)
                {
                    if (Common.EncryptString(model.oldPassword, result.PasswordSalt) == result.Password)
                    {
                        string salt = string.Empty;
                        string encryptedPswd = string.Empty;
                        Common.GeneratePassword(model.newPassword, "new", ref salt, ref encryptedPswd);

                        result.Password = encryptedPswd;
                        result.PasswordSalt = salt;

                        _db.SaveChanges();
                        message = Resource.password_changed_success;
                        status = true;
                    }
                    else
                    {
                        message = Resource.please_enter_valid_old_password;
                    }
                }

                else
                {
                    message = Resource.user_not_exist;
                }
            }
            catch (Exception ex)
            {

            }

            return status;
        }

        public bool EditProfile(User userModel)
        {
            bool status = false;
            try
            {
                // check if name already exist 
                var uData = _db.Users.Where(a => a.UserId == userModel.UserId).FirstOrDefault();
                if (uData != null)
                {
                    //var validEmail = _db.Users.Where(x => x.Email.ToLower().Trim() ==
                    //      userModel.Email.ToLower().Trim() && x.UserId != userModel.UserId).FirstOrDefault();
                    //if (validEmail != null)
                    //{
                    //    message = Resource.email_already_exists;
                    //}
                    //else
                    //{
                    uData.FullName = userModel.FullName;
                    uData.CompanyName = userModel.CompanyName;
                    uData.Address = userModel.Address;
                    uData.BillingAddress = userModel.BillingAddress;
                    uData.PicturePath = !string.IsNullOrEmpty(userModel.PicturePath) ?
                        userModel.PicturePath : uData.PicturePath;
                    uData.ModifiedDate = DateTime.Now;
                    uData.CountryCode = userModel.CountryCode;
                    uData.PhoneNumber = userModel.PhoneNumber;
                    uData.Email = userModel.Email;
                    _db.SaveChanges();
                    status = true;
                    message = Resource.profile_update_success;
                    //}
                }
                else
                {
                    message = Resource.user_not_exist;
                }

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public UserViewModel GetProfile(long userId)
        {
            UserViewModel userModel = new UserViewModel();
            try
            {
                var user = _db.Users.Where(x => x.UserId == userId)
                    .Select(x => new UserViewModel
                    {
                        Address = x.Address,
                        BillingAddress = x.BillingAddress,
                        CompanyName = x.CompanyName,
                        CountryCode = x.CountryCode,
                        DeviceId = x.DeviceId,
                        DeviceToken = x.DeviceToken,
                        Email = x.Email,
                        FullName = x.FullName,
                        PaymentMethod = x.PaymentMethod,
                        PhoneNumber = x.PhoneNumber,
                        PicturePath = x.PicturePath,
                        UserId = x.UserId,
                        UserType = x.UserType,
                        ClientId = x.Client_Id
                    }).FirstOrDefault();
                if (user != null)
                {
                    userModel = user;
                    message = Resource.success;
                }
                else
                {
                    message = Resource.user_not_exist;
                }
            }
            catch (Exception ex)
            {
                userModel = new UserViewModel();
            }
            return userModel;
        }

        public bool GrantAccess(GrantAccessViewModel model)
        {
            bool status = false;
            try
            {
                // check if email already exist
                var getExistUserEmail = _db.Users.Where(x => x.Email == model.Email).FirstOrDefault();
                var getExistGrantUserEmail = _db.tblGrantAccesses.Where(a => a.Email == model.Email).FirstOrDefault();
                if (getExistUserEmail != null || getExistGrantUserEmail != null)
                {
                    message = Resource.email_already_exists;
                    status = false;
                }

                else
                {
                    tblGrantAccess grantAccess = new tblGrantAccess();
                    grantAccess.CreatedBy = model.CreatedBy;
                    grantAccess.CreatedDate = DateTime.Now;
                    grantAccess.Comment = model.Comment;
                    grantAccess.Email = model.Email;
                    grantAccess.OrderServicesAccess = model.OrderServicesAccess;
                    grantAccess.AddChangeCardAccess = model.AddChangeCardAccess;
                    grantAccess.AddEditPropertiesAccess = model.AddEditPropertiesAccess;
                    grantAccess.BillingPriceAccess = model.BillingPriceAccess;
                    grantAccess.ForAllProperties = model.ForAllProperties;

                    var response = _db.tblGrantAccesses.Add(grantAccess);
                    _db.SaveChanges();

                    if (!model.ForAllProperties.Value)//false
                    {
                        for (int i = 0; i < model.Property_List_id.Count; i++)
                        {
                            var id = model.Property_List_id[i];

                            tblGrantProperty grantProperty = new tblGrantProperty();
                            grantProperty.GrantAccessId = response.GrantAccessId;
                            grantProperty.PropertyId = id;

                            _db.tblGrantProperties.Add(grantProperty);
                            _db.SaveChanges();
                        }
                    }

                    var getUserData = _db.Users.Where(a => a.UserId == model.CreatedBy).FirstOrDefault();
                    string providedAccess = string.Empty;
                    if (model.OrderServicesAccess == true)
                    {
                        providedAccess = ". " + Resource.access_to_order_change_services + "<br/>";
                    }
                    if (model.AddEditPropertiesAccess == true)
                    {
                        providedAccess += ". " + Resource.access_to_change_properties + "<br/>";
                    }
                    if (model.AddChangeCardAccess == true)
                    {
                        providedAccess += ". " + Resource.access_to_edit_change_card + "<br/>";
                    }
                    if (model.BillingPriceAccess == true)
                    {
                        providedAccess += ". " + Resource.access_to_see_bill_price + "<br/>";
                    }

                    // Sending email to the user letting them know they are invited to use the properties
                    string data = getUserData.Email + " " + Resource.grant_access_heading + "<br/><br/>" +
                        Resource.comment + ": " + model.Comment + "<br/><br/>" + Resource.provided_with_grant_access + "<br/>" +
                        providedAccess;

                    // string host = HttpContext.Current.Request.Url.Host;
                    string host = GetBaseUrl();


                    //var redirectUrl = "https://" + host + ":" + "8071" + "/Account/CreateAccount?email=" + model.Email
                    //    + "&id=" + grantAccess.GrantAccessId;
                    //string vURL = HttpContext.Current.Request.ApplicationPath.ToLower();
                    var redirectUrl = host + "Account/CreateAccount?email=" + model.Email
                        + "&id=" + grantAccess.GrantAccessId;
                    Common.SendEmailForGrantAccess(model.Email, data, redirectUrl);

                    message = Resource.grant_access_successfully;
                    status = true;
                }
            }
            catch (Exception ex)
            {

            }

            return status;
        }

        public List<GrantAccessViewModel> GetAccessListData(long user_id)
        {
            var getData = _db.tblGrantAccesses.Where(a => a.CreatedBy == user_id).ToList();

            List<GrantAccessViewModel> list = new List<GrantAccessViewModel>();
            foreach (var item in getData)
            {
                GrantAccessViewModel model = new GrantAccessViewModel();
                model.AddChangeCardAccess = item.AddChangeCardAccess;
                model.AddEditPropertiesAccess = item.AddEditPropertiesAccess;
                model.BillingPriceAccess = item.BillingPriceAccess;
                model.Comment = item.Comment;
                model.Email = item.Email;
                model.Date = item.CreatedDate;
                model.OrderServicesAccess = item.OrderServicesAccess;
                model.ForAllProperties = item.ForAllProperties;

                if (item.ForAllProperties == false)
                {
                    model.PropertiesList = new List<PropertyGrantAccess>();

                    var getPropertyDetails = _db.tblGrantProperties.Where(a => a.GrantAccessId == item.GrantAccessId).ToList();
                    for (int i = 0; i < getPropertyDetails.Count; i++)
                    {
                        PropertyGrantAccess propertyGrantAccess = new PropertyGrantAccess();
                        propertyGrantAccess.PropertyId = getPropertyDetails[i].PropertyId;
                        propertyGrantAccess.PropertyName = getPropertyDetails[i].Property.Name;
                        propertyGrantAccess.PropertyAddress = getPropertyDetails[i].Property.Address;
                        model.PropertiesList.Add(propertyGrantAccess);
                    }
                }
                list.Add(model);
                list = list.OrderByDescending(a => a.Date).ToList();
            }
            return list;
        }

        public MessageViewModel GetMessagesList(DateTime current_date_time)
        {
            MessageViewModel getMessagesList = new MessageViewModel();
            tblMessageType getData;
            try
            {
                int dayOfDate = current_date_time.DayOfWeek.GetHashCode();
                getData = _db.tblMessageTypes.Where(a => a.SelectedDate == current_date_time).FirstOrDefault();
                if (getData == null)
                {
                    getData = _db.tblMessageTypes.Where(a => a.SelectedDay == dayOfDate).FirstOrDefault();
                }
                if (getData != null)
                {
                    if (getData.tblMessages != null)
                    {
                        if (getData.tblMessages.Count > 0)
                        {
                            getMessagesList.Messages = new List<string>();
                            for (int i = 0; i < getData.tblMessages.Count; i++)
                            {
                                var data = getData.tblMessages.ToList();
                                getMessagesList.Messages.Add(data[i].Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                getMessagesList = new MessageViewModel();
            }
            return getMessagesList;
        }

        public bool AddAvailableTimeInProvider(AvailableTime model)
        {
            bool status = false;
            try
            {
                var getSchedule = _db.tblProviderAvailableTimes.Where(a => a.provider_id == model.user_id
                && a.day_of_week == model.day_of_week).FirstOrDefault();

                if (getSchedule == null)
                {
                    tblProviderAvailableTime tblProviderAvailableTime = new tblProviderAvailableTime();
                    tblProviderAvailableTime.created_date = DateTime.Now;
                    tblProviderAvailableTime.day_of_week = model.day_of_week;
                    tblProviderAvailableTime.from_time = model.from_time;
                    tblProviderAvailableTime.provider_id = model.user_id;
                    tblProviderAvailableTime.to_time = model.to_time;
                    tblProviderAvailableTime.IsVisible = model.IsVisible;
                    _db.tblProviderAvailableTimes.Add(tblProviderAvailableTime);
                    _db.SaveChanges();
                    message = Resource.schedule_added_successfully;
                    status = true;
                }
                else
                {
                    getSchedule.day_of_week = model.day_of_week;
                    getSchedule.created_date = DateTime.Now;
                    getSchedule.from_time = model.from_time;
                    getSchedule.to_time = model.to_time;
                    getSchedule.IsVisible = model.IsVisible;
                    _db.SaveChanges();
                    message = Resource.schedule_added_successfully;
                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = Resource.try_again;
            }
            return status;
        }

        public List<AvailableTime> GetAvailableSchedule(long user_id)
        {
            var getData = _db.tblProviderAvailableTimes.Where(a => a.provider_id == user_id)
                .Select(a => new AvailableTime
                {
                    user_id = (long)a.provider_id,
                    id = a.Id,
                    user_name = a.User.FullName,
                    day_of_week = a.day_of_week,
                    from_time = a.from_time,
                    to_time = a.to_time,
                    IsVisible = a.IsVisible
                }).OrderBy(a => a.day_of_week).ToList();

            if (getData == null || getData.Count == 0)
            {
                for (var day = 0; day < 7; day++)
                {
                    tblProviderAvailableTime tblProviderAvailableTime = new tblProviderAvailableTime();
                    tblProviderAvailableTime.created_date = DateTime.Now;
                    tblProviderAvailableTime.day_of_week = day;
                    tblProviderAvailableTime.from_time = "08:00";
                    tblProviderAvailableTime.provider_id = user_id;
                    tblProviderAvailableTime.to_time = "17:00";
                    if (day == 5 || day == 6)
                    {
                        tblProviderAvailableTime.IsVisible = false;
                        _db.tblProviderAvailableTimes.Add(tblProviderAvailableTime);
                        _db.SaveChanges();
                    }
                    else
                    {
                        tblProviderAvailableTime.IsVisible = true;
                        _db.tblProviderAvailableTimes.Add(tblProviderAvailableTime);
                        _db.SaveChanges();
                    }
                }
                getData = _db.tblProviderAvailableTimes.Where(a => a.provider_id == user_id)
                .Select(a => new AvailableTime
                {
                    user_id = (long)a.provider_id,
                    id = a.Id,
                    user_name = a.User.FullName,
                    day_of_week = a.day_of_week,
                    from_time = a.from_time,
                    to_time = a.to_time,
                    IsVisible = a.IsVisible
                }).OrderBy(a => a.day_of_week).ToList();
            }
            else
            {
                for (var day = 0; day < 7; day++)
                {
                    var getSchedule = _db.tblProviderAvailableTimes.Where(a => a.provider_id == user_id
                            && a.day_of_week == day).FirstOrDefault();

                    if (getSchedule == null)
                    {
                        tblProviderAvailableTime tblProviderAvailableTime = new tblProviderAvailableTime();
                        tblProviderAvailableTime.created_date = DateTime.Now;
                        tblProviderAvailableTime.day_of_week = day;
                        tblProviderAvailableTime.from_time = "08:00";
                        tblProviderAvailableTime.provider_id = user_id;
                        tblProviderAvailableTime.to_time = "17:00";

                        if (day == 5 || day == 6)
                        {
                            tblProviderAvailableTime.IsVisible = false;
                            _db.tblProviderAvailableTimes.Add(tblProviderAvailableTime);
                            _db.SaveChanges();
                        }
                        else
                        {
                            tblProviderAvailableTime.IsVisible = true;
                            _db.tblProviderAvailableTimes.Add(tblProviderAvailableTime);
                            _db.SaveChanges();
                        }
                    }
                }
                getData = _db.tblProviderAvailableTimes.Where(a => a.provider_id == user_id)
                .Select(a => new AvailableTime
                {
                    user_id = (long)a.provider_id,
                    id = a.Id,
                    user_name = a.User.FullName,
                    day_of_week = a.day_of_week,
                    from_time = a.from_time,
                    to_time = a.to_time,
                    IsVisible = a.IsVisible
                }).OrderBy(a => a.day_of_week).ToList();
            }
            return getData;
        }
        public bool AddProviderScheduleByDate(tblProviderScheduleByDate model)
        {
            bool status = false;
            try
            {
                var getSchedule = _db.tblProviderScheduleByDates.Where(a => a.ProviderId == model.ProviderId && a.Date == model.Date).FirstOrDefault();
                if (getSchedule == null)
                {
                    tblProviderScheduleByDate tblProviderschedule = new tblProviderScheduleByDate();
                    tblProviderschedule.CreateDate = DateTime.Now;
                    tblProviderschedule.ProviderId = model.ProviderId;
                    tblProviderschedule.FromTime = model.FromTime;
                    tblProviderschedule.ToTime = model.ToTime;
                    tblProviderschedule.Date = model.Date;
                    tblProviderschedule.IsAbsent = model.IsAbsent;
                    _db.tblProviderScheduleByDates.Add(tblProviderschedule);
                    _db.SaveChanges();
                    message = Resource.schedule_added_successfully;
                    status = true;
                }
                else
                {
                    getSchedule.Date = model.Date;
                    getSchedule.CreateDate = DateTime.Now;
                    getSchedule.FromTime = model.FromTime;
                    getSchedule.ToTime = model.ToTime;
                    getSchedule.IsAbsent = model.IsAbsent;
                    _db.SaveChanges();
                    message = Resource.schedule_added_successfully;
                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            return status;
        }
        public List<tblProviderScheduleByDate> GetScheduleByDate(long UserId)
        {
            var data = new List<tblProviderScheduleByDate>();
            try
            {
                data = _db.tblProviderScheduleByDates.Where(x => x.ProviderId == UserId).ToList();
                message = "Success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
                data = new List<tblProviderScheduleByDate>();
            }
            return data;
        }
        public List<OfferViewModel> getOffers(long userId)
        {
            List<OfferViewModel> offerView = new List<OfferViewModel>();
            try
            {
                var offerData = _db.Offers.Where(x => x.UserId == userId && x.Status != 2)
                     .Select(x => new OfferViewModel
                     {
                         Id = x.Id,
                         UserId = x.UserId,
                         OfferName = x.OfferName,
                         OfferDesc = x.OfferDesc,
                         Status = (int)x.Status
                     }).ToList();

                if (offerData != null)
                {
                    offerView = offerData;
                    message = Resource.success;
                }
                else
                {
                    // message = Resource.offer_not_exist;
                }
            }
            catch (Exception ex)
            {
                offerView = new List<OfferViewModel>();
                message = ex.Message;
            }
            return offerView;
        }
        public bool IsAccept_RejectOffers(int offerStatus, long offerId, long UserId)
        {
            bool status = false;
            try
            {
                Offer objOffer = new Offer();
                objOffer = _db.Offers.Where(x => x.Id == offerId && x.UserId == UserId).FirstOrDefault();
                objOffer.Status = offerStatus;
                _db.SaveChanges();

                var UserName = _db.Users.Where(x => x.UserId == UserId).Select(x => x.FullName).FirstOrDefault();
                if (UserName == null)
                {
                    UserName = "";
                }
                AddNotification(UserId, UserName, offerId, offerStatus);
                status = true;
                message = Resource.success;
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            return status;
        }
        public bool AddNotification(long UserId, string UserName, long OfferId, int OfferStatus)
        {
            bool status = false;
            try
            {
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = UserId;
                notification.IsActive = true;
                notification.JobRequestId = 0;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                if (OfferStatus == 1)
                {
                    notification.Text = "User " + UserName + " Accept the Offer offerId is " + OfferId;
                }
                else if (OfferStatus == 2)
                {
                    notification.Text = "User " + UserName + " Reject the Offer offerId is " + OfferId;
                }
                _db.Notifications.Add(notification);
                _db.SaveChanges();
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        public GreetingMessage GetGreetingMessages()
        {
            var dayMsg = new GreetingMessage();
            try
            {
                var day = DateTime.Now.DayOfWeek.GetHashCode();
                dayMsg = _db.GreetingMessages.Where(x => x.Day == day).FirstOrDefault();
            }
            catch (Exception ex)
            {
                dayMsg = null;
            }
            return dayMsg;
        }
        public bool AddLocation(Location objLocation)
        {
            bool status = false;
            try
            {
                var Userdata = _db.Users.Where(x => x.UserId == objLocation.UserId).FirstOrDefault();
                if (Userdata != null)
                {
                    objLocation.UserType = Userdata.UserType;
                }
                var locationExist = _db.Locations.Where(x => x.UserId == objLocation.UserId).FirstOrDefault();
                if (locationExist != null)
                {
                    locationExist.UserId = Convert.ToInt32(objLocation.UserId);
                    locationExist.UserType = objLocation.UserType;
                    locationExist.latitude = objLocation.latitude;
                    locationExist.longitude = objLocation.longitude;
                    locationExist.DateTime = objLocation.DateTime;
                    _db.SaveChanges();
                    status = true;
                    message = "Location Update Successfully";
                }
                else
                {
                    _db.Locations.Add(objLocation);
                    _db.SaveChanges();
                    status = true;
                    message = "Location Add Successfully";
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            return status;
        }
        public List<tblMission> GetMissionByUserId(long userId)
        {
            List<tblMission> Mission = new List<tblMission>();
            try
            {
                Mission = _db.tblMissions.Where(x => x.UserId == userId && x.IsActive == true).ToList();
                message = Resource.success;
            }
            catch (Exception ex)
            {
                Mission = new List<tblMission>();
                message = ex.Message;
            }
            return Mission;
        }
        public bool Mission_IsDone(int Id)
        {
            bool status = false;
            try
            {
                var data = _db.tblMissions.Where(x => x.Id == Id && x.IsActive == true).FirstOrDefault();
                if (data != null)
                {
                    data.IsDone = true;
                    _db.SaveChanges();
                }
                status = true;
                message = Resource.success;
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            return status;
        }
        public long SaveVisitorinfo(string Email, string ContactNo)
        {
            long status = 0;
            try
            {
                var data = _db.tblVisitors.Where(x => x.Email == Email).FirstOrDefault();
                if (data != null)
                {
                    data.ContactNo = ContactNo;
                    _db.SaveChanges();
                }
                else
                {
                    tblVisitor obj = new tblVisitor();
                    obj.Email = Email;
                    obj.ContactNo = ContactNo;
                    _db.tblVisitors.Add(obj);
                    _db.SaveChanges();
                }
                status = data.Id;
                message = Resource.success;
            }
            catch (Exception ex)
            {
                status = 0;
                message = ex.Message;
            }
            return status;
        }

        public AvailableTime GetScheduleByDay(long user_id)
        {
            var getData = new AvailableTime();
            int wk = Convert.ToInt32(DateTime.Today.DayOfWeek);
            try
            {
                getData = _db.tblProviderAvailableTimes.Where(a => a.provider_id == user_id && a.day_of_week == wk)
               .Select(a => new AvailableTime
               {
                   user_id = (long)a.provider_id,
                   id = a.Id,
                   user_name = a.User.FullName,
                   day_of_week = a.day_of_week,
                   from_time = a.from_time,
                   to_time = a.to_time,
                   IsVisible = a.IsVisible
               }).FirstOrDefault();
                message = "Success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
                getData = null;
            }
            return getData;
        }

        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request; var appRootFolder = request.ApplicationPath;

            if (!appRootFolder.EndsWith("/"))
            {
                appRootFolder += "/";
            }

            return string.Format(
                "{0}://{1}{2}",
                request.Url.Scheme,
                request.Url.Authority,
                appRootFolder
            );
        }
    }
}