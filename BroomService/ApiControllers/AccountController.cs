using BroomService.Models;
using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace BroomService.ApiControllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")] 
    [RoutePrefix("")]
    public class AccountController : ApiController
    {
        AccountService accountService;
        HomeService homeService;
        public AccountController()
        {
            accountService = new AccountService();
            homeService = new HomeService();
        }

        #region Login

        /// <summary>
        /// Login Api
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Login(User model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var userData = accountService.GetLoginUserApp(model);

            return this.Ok(new
            {
                status = userData == null ? false : true,
                message = accountService.message,
                userData = userData
            });
        }
        #endregion

        #region Register

        /// <summary>
        /// SignUp Api
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SignUp()
        {
            User userModel = new User();
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };
                var date = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss").Replace("-", "_");

                var count = httpRequest.Files.Count;
                if (count > 0)
                {
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        if (httpRequest.Files[i] != null)
                        {
                            var postedImg = httpRequest.Files[i];
                            var ext = postedImg.FileName.Substring(postedImg.FileName.LastIndexOf('.'));

                            var path = "~/Images/User/";

                            var imagePath = Path.GetFileNameWithoutExtension(postedImg.FileName) + "_" + date + ext.ToLower();
                            var fileName = path + imagePath;

                            if (!AllowedFileExtensions.Contains(ext))
                            {
                                return this.Ok(new
                                {
                                    status = false,
                                    message = Resource.file_extension_invalid
                                });
                            }

                            if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                            postedImg.SaveAs(HttpContext.Current.Server.MapPath(fileName));

                            userModel.PicturePath = imagePath;
                        }
                    }
                }
            }

            userModel.FullName = httpRequest.Form.Get("FullName");
            userModel.Password = httpRequest.Form.Get("Password");
            userModel.Email = httpRequest.Form.Get("Email");
            userModel.DeviceId = Convert.ToInt32(httpRequest.Form.Get("DeviceId"));
            userModel.DeviceToken = httpRequest.Form.Get("DeviceToken");
            userModel.CompanyName = httpRequest.Form.Get("CompanyName");
            userModel.CountryCode = httpRequest.Form.Get("CountryCode");
            userModel.PhoneNumber = httpRequest.Form.Get("PhoneNumber");
            userModel.Address = httpRequest.Form.Get("Address");
            userModel.BillingAddress = httpRequest.Form.Get("BillingAddress");
            userModel.UserType = Convert.ToInt32(httpRequest.Form.Get("UserType"));

            if (string.IsNullOrEmpty(userModel.FullName)   || string.IsNullOrEmpty(userModel.Email)  || string.IsNullOrEmpty(userModel.Password))
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }

            var userData = accountService.SignUp(userModel);

            return this.Ok(new
            {
                status = userData == null ? false : true,
                message = accountService.message,
                userData = userData
            });
        }

        #endregion

        #region Forget Password

        /// <summary>
        /// Forgot Password Api
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ForgetPassword(User model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    return this.Ok(new
                    {
                        Status = false,
                        Message = Resource.enter_email,
                    });
                }
                var response = accountService.ForgotPassword(model.Email);
                return this.Ok(new
                {
                    status = response,
                    message = accountService.message
                });
            }
            catch (Exception ex)
            {
                return this.Ok(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        #endregion

        #region Logout

        /// <summary>
        /// Logout Api
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Logout(User model)
        {
            var status = accountService.Logout(model.UserId);
            return this.Ok(new
            {
                status = status,
                message = accountService.message
            });
        }

        #endregion

        #region Update Device Info

        /// <summary>
        /// Update Device info api

        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult UpdateDeviceInfo(User model)
        {
            try
            {
                var response = accountService.UpdateDeviceInfo(model.UserId, model.DeviceId ?? 0, model.DeviceToken);
                return this.Ok(new
                {
                    status = response,
                    message = accountService.message
                });
            }
            catch (Exception ex)
            {
                return this.Ok(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        #endregion

        #region Change Password

        [HttpPost]
        public IHttpActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var response = accountService.ChangePassword(model);
            return this.Ok(new
            {
                status = response,
                message = accountService.message
            });
        }

        #endregion

        #region Profile

        /// <summary>
        /// Get Profile Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetProfile(long userId)
        {
            var result = accountService.GetProfile(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = accountService.message,
                data = result
            });
        }

        /// <summary>
        /// Edit Profile APi
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditProfile()
        {
            User userModel = new User();

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };
                var date = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss").Replace("-", "_");

                var count = httpRequest.Files.Count;
                if (count > 0)
                {
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        if (httpRequest.Files[i] != null)
                        {
                            var postedImg = httpRequest.Files[i];
                            var ext = postedImg.FileName.Substring(postedImg.FileName.LastIndexOf('.'));


                            var path = "~/Images/User/";

                            var imagePath = Path.GetFileNameWithoutExtension(postedImg.FileName) + "_" + date + ext.ToLower();
                            var fileName = path + imagePath;

                            if (!AllowedFileExtensions.Contains(ext))
                            {
                                return this.Ok(new
                                {
                                    status = false,
                                    message = Resource.file_extension_invalid
                                });
                            }

                            if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                            postedImg.SaveAs(HttpContext.Current.Server.MapPath(fileName));

                            userModel.PicturePath = imagePath;
                        }
                    }
                }
            }
            userModel.UserId = Convert.ToInt32(httpRequest.Form.Get("UserId"));
            userModel.FullName = httpRequest.Form.Get("FullName");
            userModel.CompanyName = httpRequest.Form.Get("CompanyName");     
            userModel.CountryCode = httpRequest.Form.Get("CountryCode");
            userModel.PhoneNumber = httpRequest.Form.Get("PhoneNumber");
            userModel.Address = httpRequest.Form.Get("Address");
            userModel.BillingAddress = httpRequest.Form.Get("BillingAddress");

            var status = accountService.EditProfile(userModel);

            return this.Ok(new
            {
                status = status,
                message = accountService.message
            });
        }
        #endregion

        #region Message

        [HttpPost]
        public IHttpActionResult GetMessages(MessageList model)
        {
            var oo = DateTime.Now.ToString(); 
            DateTime _dateTime = DateTime.ParseExact(model.current_date_time,"dd-MM-yyyy",CultureInfo.InvariantCulture);
            var result = accountService.GetMessagesList(_dateTime);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = accountService.message,
                data = result
            });
        }

        #endregion

        #region SMS Verification
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult SMSVerification(string phoneNumber, string userID)
        {
            //var jobDetails = accountService.CheckVerifySMS(phoneNumber,userID);
            var jobDetails = accountService.SMSVerification(phoneNumber,userID);

            return this.Ok(new
            {
                status = jobDetails,
                message = accountService.message,
                SMSCode = accountService.SMSCode
            });
        }
        #endregion

        #region Check SMS Verification
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult CheckSMSVerification(string phoneNumber, string userID)
        {
            //var jobDetails = accountService.CheckVerifySMS(phoneNumber,userID);
            var jobDetails = accountService.CheckVerifySMS(phoneNumber, userID);

            return this.Ok(new
            {
                status = jobDetails,
                message = accountService.message,
                SMSCode = accountService.SMSCode
            });
        }
        #endregion

        #region Add Schedule For Provider and Worker
        [HttpPost]
        public IHttpActionResult AddSchedule(AvailableTime model)
        {
            var response = accountService.AddAvailableTimeInProvider(model);
            return this.Ok(new
            {
                status = response,
                message = accountService.message
            });
        }

        public IHttpActionResult GetSchedule(long UserId)
        {
            var response = accountService.GetAvailableSchedule(UserId);
            return this.Ok(new
            {
                status = response.Count>0?true:false,
                message = accountService.message,
                ScheduleData=response
            });
        }
        public IHttpActionResult GetScheduleByDay(long UserId)
        {
            var response = accountService.GetScheduleByDay(UserId);
            return this.Ok(new
            {
                status = response != null? true : false,
                message = accountService.message,
                ScheduleData = response
            });
        }

        [HttpPost]
        public IHttpActionResult AddScheduleByDate(tblProviderScheduleByDate model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var response = accountService.AddProviderScheduleByDate(model);
            return this.Ok(new
            {
                status = response,
                message = accountService.message
            });
        }

        [HttpGet]
        public IHttpActionResult GetScheduleByDate(long UserId)
        {
            if (UserId == 0)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var response = accountService.GetScheduleByDate(UserId);
            return this.Ok(new
            {
                status = response.Count > 0 ? true : false,
                message = accountService.message,
                ScheduleData = response
            });
        }
        #endregion

        [HttpPost]
        public IHttpActionResult AddLocation(SaveLocation objLoc)
        {
            var status = false;
            Location objlocation = new Location();
            objlocation.UserId = Convert.ToInt32(objLoc.UserId);          
            objlocation.latitude = objLoc.latitude;
            objlocation.longitude = objLoc.longitude;
            objlocation.DateTime = DateTime.Now;
            status = accountService.AddLocation(objlocation);

            return this.Ok(new
            {
                status = status,
                message = accountService.message
            });
        }

        #region Offers
        [HttpGet]
        public IHttpActionResult GetOffer(long userId)
        {
            var result = accountService.getOffers(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = accountService.message,
                data = result
            });
        }

        [HttpPost]
        public IHttpActionResult IsAcceptRejectoffer(ChangeOfferStatus model)
        {
            /*value 1 as Accept and 2 as Reject offers*/
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
           bool status =  accountService.IsAccept_RejectOffers(model.OfferStatus, model.OfferId, model.UserId);
            return this.Ok(new
            {
                status = status,
                message = accountService.message,
            });
        }
        #endregion

        #region Mission
        [HttpGet]
        public IHttpActionResult GetMission(long userId)
        {
            var result  = accountService.GetMissionByUserId(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = accountService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult MissionIsDone(int MissionId)
        {
            var  status = accountService.Mission_IsDone(MissionId);
            return this.Ok(new
            {
                status = status,
                message = accountService.message
            });
        }
        #endregion

        #region Monday Popup

        [HttpGet]
        public IHttpActionResult GetService(long UserId)
        {
            var getData = homeService.GetServiceData(UserId);
            return this.Ok(new
            {
                status = getData == null ? false : true,
                message = homeService.message,
                data = getData
            });
        }
        [HttpGet]
        public IHttpActionResult AskLater(long UserId)
        {
            bool status = false;
            status = homeService.AskLater(UserId);
            return this.Ok(new
            {
                status = status,
                message = homeService.message
            });
        }
        [HttpGet]
        public IHttpActionResult Cancel(long UserId)
        {
            bool status = false;
            status = homeService.Cancel(UserId);
            return this.Ok(new
            {
                status = status,
                message = homeService.message
            });
        }
        [HttpPost]
        public IHttpActionResult BookServiceNextWeek(UserJobApiModal obj)
        {
            bool status = false;
            status = homeService.BookServiceNextWeek(obj.UserId, obj.JobRequestId);
            return this.Ok(new
            {
                status = status,
                message = homeService.message
            });
        }
        #endregion
    }
}
