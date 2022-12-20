using BroomService.bin.Controllers.Web;
using BroomService.CustomFilter;
using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BroomService.Views.Home
{
    public class AccountController : MyController
    {
        AccountService accountService;
        OrderService orderService;
        PropertyService propertyService;
        ChannelManagerService channelManagerService;

        public AccountController()
        {
            accountService = new AccountService();
            orderService = new OrderService();
            propertyService = new PropertyService();
            channelManagerService = new ChannelManagerService();
        }

        #region Login

        public ActionResult Login()
        {
            return View();
        }

        private HttpCookie CreateCookie(string name)
        {
            return new HttpCookie(name) { Path = "/", Expires = DateTime.UtcNow.AddYears(1), Domain = Request.Url.Host };
        }

        [HttpPost]
        public ActionResult Login(User model)
        {
            //if (ModelState.IsValid)
            //{
                var user = accountService.GetLoginUserWeb(model);
                if (user != null)
                {
                    HttpCookie cookie = new HttpCookie("Login");
                    cookie.Values.Add("UserEmail", user.Email);
                    cookie.Values.Add("UserName", user.FullName.Trim());
                //cookie.Values.Add("CompanyName", user.CompanyName);
                cookie.Values.Add("CompanyName", "");
                cookie.Values.Add("ProfilePic", user.PicturePath);
                    cookie.Values.Add("UserType", user.UserType.ToString());
                    cookie.Values.Add("UserId", user.UserId.ToString());
                    cookie.Values.Add("JobTypeMethod", user.JobPayType.ToString());
                    cookie.Values.Add("PaymentMethod", user.PaymentMethod.ToString());
                    cookie.Values.Add("ClientId", user.Client_Id != null ? user.Client_Id.ToString() : "");
                    if (user.UserType != 7)
                    {
                    //if (user.BillingAddress != null)
                    //{
                    //    cookie.Values.Add("BillingAddress", user.BillingAddress.ToString());
                    //}
                    //else
                    //{
                    //    cookie.Values.Add("BillingAddress", "");
                    //}
                    cookie.Values.Add("BillingAddress", "");
                }
                cookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Add(cookie);
                    Session["PaymentMethod"] = user.PaymentMethod.ToString();
                    Session["UserType"] = user.UserType.ToString();
                Session["UserName"] = user.FullName.Trim();
                
                return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMsg"] = accountService.message;
                    return View();
                }
            //}
            //else
            //{
            //    return View();
            //}
        }

        #endregion

        #region Register

        public ActionResult Register()
        {
            User userModel = new User();
            userModel.CountryCode = "+972";
            return View(userModel);
        }

        [HttpPost]
        public ActionResult Register(User userModel, HttpPostedFileBase PicturePath)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (PicturePath != null)
                    {
                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };
                        var ext = System.IO.Path.GetExtension(PicturePath.FileName);

                        var fileName = Path.GetFileNameWithoutExtension(PicturePath.FileName) + ext.ToLower();

                        if (!AllowedFileExtensions.Contains(ext))
                        {
                            ViewBag.ErrorMessage = Resource.file_extension_invalid;
                        }
                        ext = System.IO.Path.GetExtension(PicturePath.FileName);
                        {
                            // Saving the image in to the local path
                            string path = System.IO.Path.Combine(Server.MapPath("~/Images/User/"),
                                                       System.IO.Path.GetFileName(fileName));
                            PicturePath.SaveAs(path);
                            userModel.PicturePath = fileName;
                        }
                    }
                    userModel.UserType = Enums.UserTypeEnum.Customer.GetHashCode();
                    bool errorInIcount = true;
                    var user = accountService.SignUp(userModel);
                    if (user != null)
                    {

                        ModelState.Clear();
                        var contactNo = user.CountryCode.Replace("+", "") + user.PhoneNumber;

                        string icount_cid = Convert.ToString(ConfigurationManager.AppSettings["icount_cid"]);
                        string icount_user = Convert.ToString(ConfigurationManager.AppSettings["icount_user"]);
                        string icount_pass = Convert.ToString(ConfigurationManager.AppSettings["icount_pass"]);
                        string clientName = userModel.FullName;
                        string clientEmail = userModel.Email;
                        var url = string.Format(Common.CreateClient, icount_cid, icount_user, icount_pass, clientName, clientEmail);

                        HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                        myReq.ContentType = "application/json";
                        var response = (HttpWebResponse)myReq.GetResponse();
                        string text;
                        using (var sr = new StreamReader(response.GetResponseStream()))
                        {
                            text = sr.ReadToEnd();
                        }
                        var jss = new JavaScriptSerializer();
                        ICountResponse countResponse = jss.Deserialize<ICountResponse>(text);
                        ViewBag.InvoiceDoc = text;
                        if (countResponse != null)
                        {
                            if (countResponse.status)
                            {
                                accountService.SaveIcountClientId(countResponse.client_id, user.UserId);
                                bool status = accountService.SMSVerification(contactNo, user.UserId.ToString());
                                if (status)
                                {
                                    Session["ContactNo"] = contactNo;
                                    Session["SMSCode"] = accountService.SMSCode;
                                    return RedirectToAction("MobileVerification", "Account");
                                } else
                                {
                                    Common.SendSignupConfirmationEmail(user.FullName, user.Email, user.UserId.ToString());
                                }

                                TempData["SuccessMsg"] = "Register Successfully Please check your Email and Phone verification";
                                errorInIcount = false;
                                return RedirectToAction("Login", "Account");
                            }
                            else
                            {
                                //TempData["ErrorMsg"] = "Something went Wrong please try Again.";
                                if (countResponse != null)
                                {
                                    if (countResponse.reason != null)
                                    {

                                        TempData["ErrorMsg"] = countResponse.reason;
                                    }
                                    else if (countResponse.error_description != null)
                                    {
                                        TempData["ErrorMsg"] = countResponse.error_description;
                                    }
                                    else
                                    {
                                        TempData["ErrorMsg"] = "Something went Wrong please try Again.";
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = "Something went Wrong please try Again.";

                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = accountService.message;
                    }

                    if (errorInIcount && user != null && user.UserId > 0) accountService.Remove(user.UserId);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMsg"] = ex.Message;
                }
            }
            User userModel2 = new User();
            userModel2.CountryCode = "+972";
            return View(userModel2);
        }

        #endregion

        #region Email Verification

        [AllowAnonymous]
        public ActionResult EmailVerification(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMsg"] = "User not found";
                return View();
            }

            var status = accountService.CheckVerifyEmail(userId);
            if (status)
            {
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMsg"] = "User not found";
                return View();
            }
        }

        #endregion

        #region MobileVerification
        public ActionResult MobileVerification()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public string CheckMobileVerification(string UserId, string OtpCode)
        {
            var user = accountService.GetUser(UserId);            
            bool status = accountService.CheckVerifySMS(UserId, OtpCode);
            if (status)
            {
                TempData["SuccessMsg"] = "Mobile Successfully Verified!";
                Common.SendSignupConfirmationEmail(user.FullName, user.Email, user.UserId.ToString());
                return "true";
            }
            else
            {
                return "false";
            }
        }
        #endregion

        #region Logout

        public ActionResult Logout()
        {
            if (Request.Cookies["Login"] != null)
            {
                var c = new HttpCookie("Login");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            if (Request.Cookies["Card"] != null)
            {
                var c = new HttpCookie("Card");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Forgot Password

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(User model)
        {
            if (ModelState.IsValid)
            {
                var result = accountService.ForgotPassword(model.Email);
                if (result)
                {
                    ModelState.Clear();
                    TempData["SuccessMsg"] = accountService.message;
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["ErrorMsg"] = accountService.message;
                }
            }
            return View();
        }


        #endregion

        #region Terms Conditions

        public ActionResult TermsConditions()
        {
            var result = accountService.GetTermsConditions();
            return PartialView("TermsConditions", result);
        }

        #endregion

        #region Profile

        public ActionResult Profile()
        {
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    var user_id = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    var result = accountService.GetProfile(user_id);
                    result.CompanyName = string.IsNullOrEmpty(result.CompanyName) ? null : result.CompanyName;
                    result.BillingAddress = string.IsNullOrEmpty(result.BillingAddress) ? null : result.BillingAddress;
                    result.CountryCode = result.CountryCode != null ? result.CountryCode : "+972";
                    //Update Cookie Value.
                    HttpCookie cookie = new HttpCookie("Login");
                    cookie.Values.Add("UserEmail", Request.Cookies["Login"].Values["UserEmail"]);
                    cookie.Values.Add("UserName", Request.Cookies["Login"].Values["UserName"]);
                    cookie.Values.Add("ProfilePic", result.PicturePath);
                    cookie.Values.Add("UserType", Request.Cookies["Login"].Values["UserType"]);
                    cookie.Values.Add("UserId", Request.Cookies["Login"].Values["UserId"]);
                    cookie.Values.Add("CompanyName", Request.Cookies["Login"].Values["CompanyName"]);
                    cookie.Values.Add("JobTypeMethod", Request.Cookies["Login"].Values["JobTypeMethod"]);
                    cookie.Values.Add("PaymentMethod", Request.Cookies["Login"].Values["PaymentMethod"]);
                    cookie.Values.Add("ClientId", Request.Cookies["Login"].Values["ClientId"]);
                    if (Request.Cookies["Login"].Values["BillingAddress"] != null)
                    {
                        cookie.Values.Add("BillingAddress", Request.Cookies["Login"].Values["BillingAddress"]);
                    }
                    cookie.Expires = DateTime.Now.AddYears(1);
                    Response.SetCookie(cookie);

                    return View(result);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult Profile(User userModel, HttpPostedFileBase PicturePath)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (PicturePath != null)
                    {
                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };
                        var ext = System.IO.Path.GetExtension(PicturePath.FileName);

                        var fileName = Path.GetFileNameWithoutExtension(PicturePath.FileName) + ext.ToLower();

                        // PicturePath.SaveAs(Server.MapPath("https://appmantechnologies.com:6082/Images/User/" + fileName));

                        if (!AllowedFileExtensions.Contains(ext))
                        {
                            ViewBag.ErrorMessage = Resource.file_extension_invalid;
                        }
                        ext = System.IO.Path.GetExtension(PicturePath.FileName);
                        {
                            // Saving the image in to the local path
                            string path = System.IO.Path.Combine(Server.MapPath("~/Images/User/"),
                                                       System.IO.Path.GetFileName(fileName));
                            PicturePath.SaveAs(path);
                            userModel.PicturePath = fileName;
                        }
                    }
                    userModel.UserType = Enums.UserTypeEnum.Customer.GetHashCode();

                    var user = accountService.EditProfile(userModel);
                    if (user)
                    {
                        ModelState.Clear();
                        TempData["SuccessMsg"] = accountService.message;
                        return RedirectToAction("Profile");

                    }
                    else
                    {
                        TempData["ErrorMsg"] = accountService.message;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                }
            }
            return View(userModel);
        }

        #endregion

        #region Change Password

        public ActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    model.userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = accountService.ChangePassword(viewModel);
                    if (user)
                    {
                        ModelState.Clear();
                        TempData["SuccessMsg"] = accountService.message;
                        return RedirectToAction("Profile");

                    }
                    else
                    {
                        TempData["ErrorMsg"] = accountService.message;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                }
            }
            return View(viewModel);
        }

        #endregion

        #region Grant Access

        public ActionResult GrantAccess()
        {
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                    if (userId != 0)
                    {
                        ViewBag.Properties = propertyService.GetPropertiesSelect(userId);
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        [HttpPost]
        public JsonResult GrantAccess(GrantAccessViewModel model)
        {
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                    model.CreatedBy = userId;

                    try
                    {
                        var getProperties = propertyService.GetPropertiesSelect(userId);
                        if (getProperties.Count == model.Property_List_id.Count)
                        {
                            model.ForAllProperties = true;
                        }
                        else
                        {
                            model.ForAllProperties = false;
                        }
                        var user = accountService.GrantAccess(model);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = ex.Message;
                    }
                    ViewBag.Properties = propertyService.GetPropertiesSelect(userId);
                    return Json(accountService.message, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(accountService.message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(accountService.message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GrantAccessList()
        {
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                    var getData = accountService.GetAccessListData(userId);
                    return View(getData);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult MyReview()
        {
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                    var getData = orderService.GetUserReviews(userId);
                    var avrRating = getData.Average(x => x.UserRating);
                    ViewData["AverageRating"] = avrRating.ToString();
                    return View(getData);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            //return View();
        }

        [VerifyUser]
        [HttpGet]
        public JsonResult Balance()
        {
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);

            var user = accountService.GetCurrentUser(userId);
            return Json(user.Balance, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FastOrder()
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var getData = orderService.GetFastOrdersForUsers(userId);

            return View(getData);
        }

        public ActionResult FastOrderdDetail(long id)
        {
            var getData = orderService.BookingsDetails(id);

            var catId = getData.JobData[0].CategoryId != null ? getData.JobData[0].CategoryId : 0;
            var subCatId = getData.JobData[0].SubCategoryId != null ? getData.JobData[0].SubCategoryId : 0;
            var subSubcatId = getData.JobData[0].SubSubCategoryId != null ? getData.JobData[0].SubSubCategoryId : 0;
            var PropertyId = getData.JobData[0].PropertyId != null ? getData.JobData[0].PropertyId : 0;
            var JobReqId = getData.JobData[0].OrderId != null ? getData.JobData[0].OrderId : 0;

            return RedirectToAction("AddJobRequest", "Order", new { categoryId = catId, subCategoryId = subCatId, subSubCategoryId = subSubcatId, propertyId = PropertyId, jobReqId = JobReqId });
            //  return View(getData);
        }


        // Creation of the account for grant access user
        [HttpGet]
        public ActionResult CreateAccount(string email, int id)
        {
            User _model = new User();
            _model.Email = email;
            _model.DeviceId = id;
            return View(_model);
        }

        [HttpPost]
        public ActionResult CreateAccount(User userModel)
        {
            try
            {
                userModel.UserType = Enums.UserTypeEnum.GrantAccessUser.GetHashCode();

                var user = accountService.CreateAccountGrantUser(userModel);
                if (user != null)
                {
                    ModelState.Clear();
                    TempData["SuccessMsg"] = accountService.message;
                    return View();
                }
                else
                {
                    TempData["ErrorMsg"] = accountService.message;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            return View();
        }

        #endregion

        #region Offers
        [VerifyUser]
        public ActionResult Offers()
        {

            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);

            var offers = accountService.getOffers(userId);
            return View(offers);


        }
        public bool IsAccept_Rejectoffers(int OfferStatus, long OfferId)
        {
            /*value 1 as Accept and 2 as Reject offers*/
            bool status = false;
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            //var UserName = Request.Cookies["Login"].Values["UserName"].ToString();
            if (userId != 0)
            {
                status = accountService.IsAccept_RejectOffers(OfferStatus, OfferId, userId);
                //if(status)
                //{
                //    status = accountService.AddNotification(userId, UserName, OfferId, OfferStatus);
                //}               
            }
            return status;
        }
        #endregion

        #region Mission
        [VerifyUser]
        public ActionResult Mission()
        {

            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);

            var MissionList = accountService.GetMissionByUserId(userId);
            return View(MissionList);

        }
        public bool Mission_IsDone(int MissionId)
        {
            bool status = false;
            status = accountService.Mission_IsDone(MissionId);
            return status;
        }
        #endregion
        public bool SaveVisitorInfo(string Email, string ContactNo)
        {
            long status = 0;
            status = accountService.SaveVisitorinfo(Email, ContactNo);
            if (status != 0)
            {
                HttpCookie cookie = new HttpCookie("VisitorLogin");
                cookie.Values.Add("UserEmail", Email);
                cookie.Values.Add("UserId", status.ToString());
                cookie.Values.Add("UserContactNo", ContactNo);
                cookie.Values.Add("PaymentMethod", "1");
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
                Session["PaymentMethod"] = "1";
                return true;
            }
            return false;
        }

        [VerifyUser]
        public ActionResult PropertyManagers()
        {
            // we should return all the channel managers and user channel managers

            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            var propertyManagerModel = new PropertyManagerViewModel();
            propertyManagerModel.ChannelManagers = channelManagerService.ChannelManagers();
            propertyManagerModel.UserChannelManagers = channelManagerService.UserChannelManagers(userId);
            return View(propertyManagerModel);

        }

        [VerifyUser]
        public ActionResult PropertySettings(long userChannelManagerId)
        {
            // we should return all the channel managers and user channel managers
            var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            var propertySettings = new PropertySettingsViewModel();
            var userChannelManagerSettings = channelManagerService.UserChannelManagerSettings(userChannelManagerId);
            var userChannelManagerViewModel = channelManagerService.UserChannelManager(userChannelManagerId);
            var channelManagerViewModel = channelManagerService.ChannelManagerById(userChannelManagerViewModel.ChannelManagerId);
            var fastOrders = orderService.GetFastOrdersForUsers(userId);

            propertySettings.UserChannelManagerSettings = userChannelManagerSettings;
            propertySettings.UserChannelManagerViewModel = userChannelManagerViewModel;
            propertySettings.ChannelManagerViewModel = channelManagerViewModel;
            propertySettings.FastOrders = fastOrders;
            return View(propertySettings);
        }
    }
}