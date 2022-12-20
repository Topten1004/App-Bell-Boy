using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Text;

namespace BroomService.ApiControllers
{
    public class OrderController : ApiController
    {
        OrderService orderService;
        CategoryService categoryService;
        PropertyService propertyService;
        LaundryService laundryService;
        public OrderController()
        {
            orderService = new OrderService();
            categoryService = new CategoryService();
            propertyService = new PropertyService();
            laundryService = new LaundryService();
        }

        /// <summary>
        /// Get Inventory List api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetInventoryList()
        {
            var result = orderService.GetInventoryList();
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = result != null ? Resource.success : Resource.no_data_found,
                inventoryData = result
            });
        }

        /// <summary>
        /// Add Job request by the customer
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddJobRequest()
        {
            JobRequestViewModel model = new JobRequestViewModel();

            List<ChecklistImageVM> checklistImages = new List<ChecklistImageVM>();
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
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

                            var path = "~/Images/JobRequest/";

                            var imagePath = Path.GetFileNameWithoutExtension(postedImg.FileName) + "_" + date + ext.ToLower();
                            var fileName = path + imagePath;

                            IList<string> AllowedImageFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };

                            IList<string> AllowedVideoFileExtensions = new List<string> { ".mp4", ".3gp", ".flv", ".wmv", ".mov", ".MP4", ".3GP", ".FLV", ".WMV", ".MOV" };
                            if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                            postedImg.SaveAs(HttpContext.Current.Server.MapPath(fileName));

                            if (AllowedImageFileExtensions.Contains(ext))
                            {
                                checklistImages.Add(new ChecklistImageVM
                                {
                                    IsImage = true,
                                    ImageUrl = imagePath
                                });
                            }
                            else if (AllowedVideoFileExtensions.Contains(ext))
                            {
                                checklistImages.Add(new ChecklistImageVM
                                {
                                    IsVideo = true,
                                    VideoUrl = imagePath
                                });
                            }

                        }
                    }
                }
            }

            model.ReferenceImages = checklistImages;

            model.UserId = Convert.ToInt64(httpRequest.Form.Get("UserId"));
            model.JobDesc = httpRequest.Form.Get("JobDesc");
            model.IsFastOrder = Convert.ToBoolean(httpRequest.Form.Get("IsFastOrder"));
            model.FastOrderName = httpRequest.Form.Get("FastOrderName");

            if (httpRequest.Form.Get("PaymentInfo") != null)
            {
                model.PaymentInfo = Convert.ToInt32(httpRequest.Form.Get("PaymentInfo"));
            }

            if (httpRequest.Form.Get("isPaymentDone") != null)
            {
                model.isPaymentDone = Convert.ToBoolean(httpRequest.Form.Get("isPaymentDone"));
            }
            if (httpRequest.Form.Get("CheckList") != null)
            {
                var checklist = httpRequest.Form.Get("CheckList").ToString();
                model.CheckList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(checklist);
            }

            if (httpRequest.Form.Get("Categories") != null)
            {
                var subCategory = httpRequest.Form.Get("Categories").ToString();
                model.Categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceData>>(subCategory);
            }

            if (httpRequest.Form.Get("PropertyService") != null)
            {
                var subCategory = httpRequest.Form.Get("PropertyService").ToString();
                model.PropertyService = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PropertyServiceData>>(subCategory);
            }

            if (httpRequest.Form.Get("Property_List_Id") != null)
            {
                var propertyId = httpRequest.Form.Get("Property_List_Id").ToString();
                model.Property_List_Id = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(propertyId);
            }

            if (httpRequest.Form.Get("InventoryList") != null)
            {
                var inventoryList = httpRequest.Form.Get("InventoryList").ToString();
                model.InventoryList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InventoryItems>>(inventoryList);
            }
            if (httpRequest.Form.Get("DocInvoice") != null)
            {
                var subCategory = httpRequest.Form.Get("DocInvoice").ToString();
                model.DocumentJob = Newtonsoft.Json.JsonConvert.DeserializeObject<ICountResponse>(subCategory);
            }


            var result = orderService.AddJobRequest(model);
            return this.Ok(new
            {
                status = result,
                message = orderService.message,
                description = orderService.description,
                unitprice = orderService.unitprice,
                JobRequestID = orderService.JobReqId
            });
        }

        [HttpGet]
        public IHttpActionResult AcceptRejectSupervisorOffer(long notification_id, bool status)
        {
            var jobDetails = orderService.AcceptRejectSupervisorOffer(notification_id, status);
            return this.Ok(new
            {
                status = true,
                message = orderService.message
            });
        }

        [HttpPost]
        public IHttpActionResult SupervisorJobAccept(WorkersJobs workersJobs)
        {
            var jobDetails = orderService.AddJobRequestSentBySupervisor(workersJobs.JobPropId, workersJobs.JobDateTime, workersJobs.WorkerId);
            return this.Ok(new
            {
                status = true,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult AcceptRejectSubUserRequest(long notification_id, bool status, long UserId)
        {
            var result = orderService.AcceptRejectSubUserOffer(notification_id, status, UserId);
            return this.Ok(new
            {
                status = result,
                message = orderService.message
            });
        }

        /// <summary>
        /// Accept/Reject the job request by the service provider
        /// </summary>
        /// <param name="notification_id"></param>
        /// <param name="status"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>

        [HttpGet]
        public IHttpActionResult AcceptRejectProviderRequest(long notification_id, bool status)
        {
            var result = orderService.AcceptRejectProviderJobRequest(notification_id, status);
            return this.Ok(new
            {
                status = result,
                message = orderService.message
            });
        }

        [HttpGet]
        public IHttpActionResult GetPrice(long subId, long subSubId)
        {
            var result = orderService.GetPrice(subId, subSubId);
            return this.Ok(new
            {
                status = result,
                message = orderService.message,
                price = orderService.servicePrice
            });
        }

        /// <summary>
        /// when adding service if for workers then need to check which workers are 
        /// avaiable for that time and service
        /// </summary>
        /// <returns></returns>

        //[HttpPost]
        //public IHttpActionResult CheckForAvaiableWorker(ServiceData serviceData)
        //{
        //    var result = orderService.CheckForAvailbleWorker(serviceData);

        //    bool status = false;
        //    List<GetWorkers> workers = new List<GetWorkers>();
        //    if (result != null)
        //    {
        //        foreach (var item in result)
        //        {
        //            status = item.Key;
        //            workers = item.Value;
        //        }
        //    }

        //    return this.Ok(new
        //    {
        //        status = status,
        //        message = status == true ? Resource.success : Resource.some_error_occured,
        //        avaiableWorkers = workers
        //    });
        //}

        /// <summary>
        /// Getting the list of the avaiable timings
        /// </summary>
        /// <param name="serviceData"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult GetTimingsAvaiableForService(ServiceData serviceData)
        {
            var result = orderService.GetTimingsAvaiableForService(serviceData);

            return this.Ok(new
            {
                status = result.Count > 0 ? true : false,
                message = result.Count > 0 ? Resource.success : "No Timings Available",
                availableTimings = result
            });
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult GetAlternativeTimingsForService(ServiceData serviceData)
        {
            var result = orderService.GetAlternativeTimingsForService(serviceData);

            return this.Ok(new
            {
                status = result.Count > 0 ? true : false,
                message = result.Count > 0 ? Resource.success : "No Alternative Timings Available",
                availableTimings = result
            });
        }

        //[HttpPost]
        //public IHttpActionResult ChooseAlternativeJobOption(JobOptionModel model)
        //{
        //    // If client chooses to change date time
        //    bool status = orderService.ChangeJobTimings(model);
        //    return this.Ok(new
        //    {
        //        status = status,
        //        message = status == true ? Resource.success : Resource.some_error_occured
        //    });
        //}

        [System.Web.Http.HttpGet]
        public IHttpActionResult ContactSupport(long UserId)
        {
            // client requested for the contact support
            bool status = orderService.ContactToSupport(UserId);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetSubUserRequests(long userID)
        {
            var result = orderService.SubUserRequests(userID);
            return this.Ok(new
            {
                status = result.Count > 0 ? true : false,
                message = result.Count > 0 ? Resource.success : Resource.no_data_found,
                subRequestData = result
            });
        }
        public IHttpActionResult QuoteRequest()
        {
            JobRequestQuoteTypeViewModel model = new JobRequestQuoteTypeViewModel();

            var httpRequest = HttpContext.Current.Request;

            if (httpRequest.Form.Get("JobRequestId") != null)
            {
                model.JobRequestId = Convert.ToInt32(httpRequest.Form.Get("JobRequestId"));
            }

            if (httpRequest.Form.Get("UserId") != null)
            {
                model.UserId = Convert.ToInt32(httpRequest.Form.Get("UserId"));
            }
            if (httpRequest.Form.Get("Type") != null)
            {
                model.Type = Convert.ToInt32(httpRequest.Form.Get("Type"));
            }

            if (model.Type == Enums.QuoteTypeEnum.SendPrice.GetHashCode())
            {
                List<ChecklistImageVM> checklistImages = new List<ChecklistImageVM>();

                if (httpRequest.Files.Count > 0)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
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

                                var path = "~/Images/JobRequest/";

                                var imagePath = Path.GetFileNameWithoutExtension(postedImg.FileName) + "_" + date + ext.ToLower();
                                var fileName = path + imagePath;

                                IList<string> AllowedImageFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };

                                IList<string> AllowedVideoFileExtensions = new List<string> { ".mp4", ".3gp", ".flv", ".wmv", ".mov", ".MP4", ".3GP", ".FLV", ".WMV", ".MOV" };
                                if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                                postedImg.SaveAs(HttpContext.Current.Server.MapPath(fileName));

                                if (AllowedImageFileExtensions.Contains(ext))
                                {
                                    checklistImages.Add(new ChecklistImageVM
                                    {
                                        IsImage = true,
                                        ImageUrl = imagePath
                                    });
                                }
                                else if (AllowedVideoFileExtensions.Contains(ext))
                                {
                                    checklistImages.Add(new ChecklistImageVM
                                    {
                                        IsVideo = true,
                                        VideoUrl = imagePath
                                    });
                                }

                            }
                        }
                    }
                }

                model.Description = httpRequest.Form.Get("Description");
            }
            else if (model.Type == Enums.QuoteTypeEnum.Meeting.GetHashCode())
            {
                model.MeetingTime = httpRequest.Form.Get("MeetingTime");
            }

            var status = orderService.QuoteTypeDetails(model);

            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult WorkerAboutToFinish(long JobPropId)
        {
            bool status = orderService.WorkerAboutToFinish(JobPropId);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult MakeJobInProgress(long jobPropId)
        {
            bool status = orderService.MakeJobInProgress(jobPropId);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult MakeJobInCancel(long jobPropId)
        {
            bool status = orderService.MakeJobInCancel(jobPropId);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult MakeCancelRefund(long jobPropId)
        {
            //Change parameters as new changes
            bool status = orderService.MakeCancelRefund(jobPropId, jobPropId, jobPropId, jobPropId, jobPropId, true);
            return this.Ok(new
            {
                status = status,
                message = orderService.message,
                data = orderService.refundPrice
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult MonthInvoice(string userId)
        {
            //Change parameters as new changes
            List<MonthInvoice> data = orderService.MonthInvoiceMail(userId);
            return this.Ok(new
            {
                status = true,
                message = orderService.message,
                data = orderService.refundPrice
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetWorkerCancelRequests(long userId)
        {
            var result = orderService.GetWorkerCancelRequests(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        [System.Web.Http.HttpGet]
        public void MakeCancelRequestAccept(long jobPropId)
        {
            bool status = orderService.MakeCancelRequestAccept(jobPropId);
            /*  return this.Ok(new
              {
                  status = status,
                  message = orderService.message
              }); */
        }

        [System.Web.Http.HttpGet]
        public void MakeCancelRequestReject(long jobPropId)
        {
            bool status = orderService.MakeCancelRequestReject(jobPropId);
            /* return this.Ok(new
             {
                 status = status,
                 message = orderService.message
             });*/
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult MakeChangeRequestNotify(long jobPropId)
        {
            bool status = orderService.MakeChangeRequestNotify(jobPropId);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        //[System.Web.Http.HttpPost]
        //public IHttpActionResult SaveReceiptDocument(SaveReceiptDoc data)
        //{
        //    JobRequestViewModel model = new JobRequestViewModel();

        //    //var httpRequest = HttpContext.Current.Request;

        //    //if (httpRequest.Form.Get("JobReqId") != null)
        //    //{
        //    //    model.Id = Convert.ToInt64(httpRequest.Form.Get("JobReqId"));
        //    //}
        //    //if (httpRequest.Form.Get("DocReceipt") != null)
        //    //{
        //    //    var subCategory = httpRequest.Form.Get("DocReceipt").ToString();
        //    //    model.DocumentJob = Newtonsoft.Json.JsonConvert.DeserializeObject<ICountResponse>(subCategory);
        //    //}
        //    model.Id = data.JobReqId;
        //    model.DocReceipt_Url = data.Doc_Url;

        //    bool status = orderService.SaveReceiptDocument(model);
        //    return this.Ok(new
        //    {
        //        status = status,
        //        message = orderService.message
        //    });
        //}
        [System.Web.Http.HttpGet]
        public IHttpActionResult SaveReceiptDoc(long JobReqId, string Doc_Url)
        {
            JobRequestViewModel model = new JobRequestViewModel();

            //var httpRequest = HttpContext.Current.Request;

            //if (httpRequest.Form.Get("JobReqId") != null)
            //{
            //    model.Id = Convert.ToInt64(httpRequest.Form.Get("JobReqId"));
            //}
            //if (httpRequest.Form.Get("DocReceipt") != null)
            //{
            //    var subCategory = httpRequest.Form.Get("DocReceipt").ToString();
            //    model.DocumentJob = Newtonsoft.Json.JsonConvert.DeserializeObject<ICountResponse>(subCategory);
            //}
            model.Id = JobReqId;
            model.DocReceipt_Url = Doc_Url;

            bool status = orderService.SaveReceiptDocument(model);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetInvoiceDoc(long orderId)
        {
            DocumentJob documentJob = orderService.GetInvoiceDoc(orderId);
            return this.Ok(new
            {
                status = documentJob != null ? true : false,
                message = orderService.message,
                Data = documentJob
            });
        }


        [System.Web.Http.HttpGet]
        public IHttpActionResult MakeCheckListDone(long checkListId)
        {
            bool status = orderService.MakeCheckListDone(checkListId);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult SaveQuesResponse(SaveQuesResponse QuesResponse)
        {
            bool status = orderService.SaveWorkerQuesResponse(QuesResponse);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetWorkerQuesByWorkerQuesType(string QuesType)
        {
            var objQuestions = orderService.GetWorkerQuestionsByWorkerType(QuesType);
            return this.Ok(new
            {
                status = true,
                message = orderService.message,
                Questions = objQuestions
            });
        }

        #region Reminder Payment
        [System.Web.Http.HttpGet]
        public void ReminderPayment(string userId)
        {
            var status = orderService.ReminderPayment(long.Parse(userId));
        }
        #endregion

        [HttpPost]
        public IHttpActionResult ProcessPayment(string jobIdToken)
        {
            var result = orderService.ProcessPayment(jobIdToken);

            return Ok(new
            {
                status = result,
            });
        }

        #region Laundry

        [HttpGet]
        public IHttpActionResult GetLaundryRequests(long deliveryGuyId)
        {
            var result = laundryService.LaundryRequests(deliveryGuyId);
            return Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult GetLaundries(long laundryId)
        {
            var result = laundryService.GetLaundriesByLaundry(laundryId);
            return Ok(new
            {
                status = result != null,
                message = "",
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult PickupLaundry(long laundryRequestId, long pickupGuyId)
        {
            var result = laundryService.PickupLaundry(laundryRequestId, pickupGuyId);

            return Ok(new
            {
                status = result,
            });
        }

        [HttpGet]
        public IHttpActionResult ReceivedLaundry(long laundryRequestId, long laundryId)
        {
            var result = laundryService.ReceivedLaundry(laundryRequestId, laundryId);

            return Ok(new
            {
                status = result,
            });
        }

        [HttpPost]
        public IHttpActionResult SendLaundryPrice(LaundryPriceViewModel model)
        {
            var result = laundryService.SendLaundryPrice(model);
            return Ok(new
            {
                status = result,
                message = orderService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult DeliveryReceivedLaundry(long laundryRequestId, long returnGuyId)
        {
            var result = laundryService.DeliveryReceivedLaundry(laundryRequestId, returnGuyId);

            return Ok(new
            {
                status = result,
            });
        }

        [HttpGet]
        public IHttpActionResult DeliveredLaundry(long laundryRequestId, long returnGuyId)
        {
            var result = laundryService.DeliveredLaundry(laundryRequestId, returnGuyId);

            return Ok(new
            {
                status = result,
            });
        }

        [HttpGet]
        public IHttpActionResult GenerateLaundryPayment(long laundryRequestId)
        {
            var user = propertyService.GetUser(299);
            var res = laundryService.GeneratePaymentPage(laundryRequestId, user);
            return Ok(new
            {
                status = true,
            });
        }

        [HttpGet]
        public IHttpActionResult GetLaundry(long propertyId)
        {
            var selectedLaundry = laundryService.GetLaundry(propertyId);
            return Ok(new
            {
                status = true,
                data = selectedLaundry
            });
        }

        [HttpGet]
        public IHttpActionResult DeliveryAvailabilities()
        {
            var availableSchedules = laundryService.GetAvailabilities();
            return Ok(new
            {
                status = true,
                availableSchedules
            });
        }

        [HttpGet]
        public IHttpActionResult DeliveryBookedSchedules(long propertyId)
        {
            var bookedSchedules = laundryService.GetBookedSchedules(propertyId);

            return Ok(new
            {
                status = true,
                bookedSchedules
            });
        }


        [HttpPost]
        public IHttpActionResult LaundryPaymentProcess(string laundryIdToken)
        {
            var result = laundryService.ProcessPayment(laundryIdToken);
            return Ok(new
            {
                status = result
            });
        }
        #endregion

        #region Quotes
        [HttpGet]
        public IHttpActionResult GetQuotes(long userId)
        {
            var result = orderService.GetServiceQuotes(userId);
            return Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult AvailableQuotesSP(long serviceProviderId)
        {
            var result = orderService.AvailableQuotesSP(serviceProviderId);
            return Ok(new
            {
                status = result != null,
                message = orderService.message,
                data = result
            });
        }

        // Service Provider Quote Request
        [HttpPost]
        public IHttpActionResult QuotePriceSP(QuoteViewModel model)
        {
            var result = orderService.QuotePriceSP(model);
            return Ok(new
            {
                status = result,
                message = orderService.message,
                data = result
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetPropertyDetail(long PropertyId)
        {
            var result = orderService.GetPropertyDetailById(PropertyId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }
        #endregion

        #region GetWorker free timing
        [System.Web.Http.HttpPost]
        public IHttpActionResult AvailableTime(GetWorkerTiming obj)
        {

            //List<GetWorkers> obj = new List<GetWorkers>();
            //long AssignedId = 0;
            ServiceData serviceData = new ServiceData();
            serviceData.SubCategoryId = obj.subCategoryId;
            serviceData.SubSubCategoryyId = obj.SubSubCategoryyId;
            // serviceData.Day = 2; 
            serviceData.Day = obj.dayofweek;
            int TimeToDo = obj.price != null ? Convert.ToInt32(Convert.ToInt32(obj.price) / 110) : 0;
            //AssignedId = orderService.GetTimingsAvaiableWorker(serviceData, JobDate, TimeToDo);
            var result = orderService.GetTimingsAvaiableWorker(serviceData, obj.JobDate, TimeToDo);
            foreach (var item in result)
            {
                item.CategoryId = 0;
                item.Day = 0;
                item.Rating = 0;
                item.ToTime = "0";
                item.WorkerType = "0";
            }
            return this.Ok(new
            {
                status = result.Count > 0 ? true : false,
                message = result.Count > 0 ? Resource.success : "No Alternative Timings Available",
                availableTimings = result
            });
        }
        #endregion

        #region Checklist
        [System.Web.Http.HttpPost]
        public IHttpActionResult SaveChecklist(SaveChecklistModel obj)
        {
            bool Status = false;
            Status = orderService.SaveCheckList(obj.UserId, obj.PropId, obj.ServiceId, obj.SubCatId, obj.SubSubCatId, obj.ChecklistName, obj.Chklist);
            return this.Ok(new
            {
                status = Status,
                message = orderService.message
            });
        }
        [System.Web.Http.HttpPost]
        public IHttpActionResult GetChecklistName(SaveChecklistModel obj)
        { //Get Checklist Name
            var data = new List<tblSavedChecklist>();
            data = orderService.GetChecklist(obj.UserId, obj.PropId, obj.ServiceId, obj.SubCatId, obj.SubSubCatId);
            return this.Ok(new
            {
                status = data.Count > 0 ? true : false,
                message = orderService.message,
                data = data
            });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetChecklistText(long ChklistId)
        {
            var data = orderService.GetChecklistText(ChklistId);
            return this.Ok(new
            {
                status = data.Count > 0 ? true : false,
                message = orderService.message,
                data = data
            });
        }
        #endregion

        #region Edit JobRequest
        [System.Web.Http.HttpPost]
        public IHttpActionResult GetEditJobRequest(EditJob obj)
        {
            JobRequestSubSubCategory jobReqCat = new JobRequestSubSubCategory();
            jobReqCat = categoryService.GetCategoryDetailByJobId(obj.JobReqId);

            EditJobRequestApiModel EditjobRequest = new EditJobRequestApiModel();
            var JobDetail = orderService.GetJobDetail(obj.JobReqId, (long)jobReqCat.JobRequestId);
            if (JobDetail != null)
            {
                EditjobRequest.JobStartDateTime = JobDetail.JobStartDateTime;
                EditjobRequest.JobEndDateTime = JobDetail.JobEndDateTime;
                EditjobRequest.JobReqId = JobDetail.JobReqId;
                EditjobRequest.JobDesc = JobDetail.JobDesc;
                EditjobRequest.Property_List_Id = JobDetail.Property_List_Id;
                EditjobRequest.UserName = JobDetail.UserName;
                EditjobRequest.AssignWorker = JobDetail.AssignWorker;
            }
            EditjobRequest.JobReqId = obj.JobReqId;
            EditjobRequest.UserId = obj.UserId;
            // var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
            var categoryData = categoryService.GetCategoryById((long)jobReqCat.CategoryId);
            if (categoryData != null)
            {
                //categoryData.Name = culVal == "fr-FR" ? categoryData.Name_French
                //    : culVal == "ru-RU" ? categoryData.Name_Russian
                //    : culVal == "he-IL" ? categoryData.Name_Hebrew
                //    : categoryData.Name;
                //string CategoryData = JsonConvert.SerializeObject(categoryData,new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
                //EditjobRequest.CategoryData = JsonConvert.DeserializeObject<Category>(CategoryData);
                CategoryModel objCategory = new CategoryModel();
                objCategory.Id = categoryData.Id;
                objCategory.Name = categoryData.Name != null ? categoryData.Name : "";
                objCategory.Picture = categoryData.Picture != null ? categoryData.Picture : "";
                objCategory.HasPrice = categoryData.HasPrice;
                objCategory.Icon = categoryData.Icon != null ? categoryData.Icon : "";
                objCategory.Name_Russian = categoryData.Name_Russian != null ? categoryData.Name_Russian : "";
                objCategory.Name_Hebrew = categoryData.Name_Hebrew != null ? categoryData.Name_Hebrew : "";
                objCategory.Name_French = categoryData.Name_French != null ? categoryData.Name_French : "";
                objCategory.Description = categoryData.Description != null ? categoryData.Description : "";
                objCategory.Description_Russian = categoryData.Description_Russian != null ? categoryData.Description_Russian : "";
                objCategory.Description_Hebrew = categoryData.Description_Hebrew != null ? categoryData.Description_Hebrew : "";
                objCategory.Description_French = categoryData.Description_French != null ? categoryData.Description_French : "";
                EditjobRequest.CategoryData = objCategory;
            }
            var subCategoryData = categoryService.GetSubCategoryById((long)jobReqCat.SubCategoryId);
            if (subCategoryData != null)
            {
                //subCategoryData.Name = culVal == "fr-FR" ? subCategoryData.Name_French
                //: culVal == "ru-RU" ? subCategoryData.Name_Russian
                //: culVal == "he-IL" ? subCategoryData.Name_Hebrew
                //: subCategoryData.Name;
                SubCategoryModel objSubCategory = new SubCategoryModel();
                objSubCategory.Id = subCategoryData.Id;
                objSubCategory.Name = subCategoryData.Name;
                objSubCategory.CatId = subCategoryData.CatId;
                objSubCategory.Picture = subCategoryData.Picture;
                objSubCategory.Icon = subCategoryData.Icon;
                objSubCategory.ClientPrice = subCategoryData.ClientPrice;
                objSubCategory.Price = subCategoryData.Price;
                objSubCategory.HasSubSubCategories = subCategoryData.HasSubSubCategories;
                objSubCategory.Name_Russian = subCategoryData.Name_Russian;
                objSubCategory.Name_Hebrew = subCategoryData.Name_Hebrew;
                objSubCategory.Name_French = subCategoryData.Name_French;
                objSubCategory.Description = subCategoryData.Description;
                objSubCategory.Description_Russian = subCategoryData.Description_Russian;
                objSubCategory.Description_Hebrew = subCategoryData.Description_Hebrew;
                objSubCategory.Description_French = subCategoryData.Description_French;
                EditjobRequest.SubCategoryData = objSubCategory;
            }
            if (jobReqCat.SubSubCategoryId != 0)
            {
                var subSubCategoryData = categoryService.GetSubSubCategoryById((long)jobReqCat.SubSubCategoryId);
                if (subSubCategoryData != null)
                {
                    //subSubCategoryData.Name = culVal == "fr-FR" ? subSubCategoryData.Name_French
                    //: culVal == "ru-RU" ? subSubCategoryData.Name_Russian
                    //: culVal == "he-IL" ? subSubCategoryData.Name_Hebrew
                    //: subSubCategoryData.Name;
                    //EditjobRequest.SubSubCategoryData = subSubCategoryData;
                    SubSubCategoryModel objsubsubcat = new SubSubCategoryModel();
                    objsubsubcat.Id = subSubCategoryData.Id;
                    objsubsubcat.Name = subSubCategoryData.Name;
                    objsubsubcat.SubCatId = subSubCategoryData.SubCatId;
                    objsubsubcat.ClientPrice = subSubCategoryData.ClientPrice;
                    objsubsubcat.Price = subSubCategoryData.Price;
                    objsubsubcat.Picture = subSubCategoryData.Picture;
                    objsubsubcat.Icon = subSubCategoryData.Icon;
                    objsubsubcat.Name_Russian = subSubCategoryData.Name_Russian;
                    objsubsubcat.Name_French = subSubCategoryData.Name_French;
                    objsubsubcat.Name_Hebrew = subSubCategoryData.Name_Hebrew;
                    objsubsubcat.Description_French = subSubCategoryData.Description_French;
                    objsubsubcat.Description_Russian = subSubCategoryData.Description_Russian;
                    objsubsubcat.Description_Hebrew = subSubCategoryData.Description_Hebrew;
                    objsubsubcat.Description = subSubCategoryData.Description;
                    EditjobRequest.SubSubCategoryData = objsubsubcat;
                }
            }
            EditjobRequest.Properties = propertyService.GetPropertiesSelect(obj.UserId);

            var objInventory = propertyService.GetInventory();
            if (EditjobRequest.Property_List_Id != 0)
            {

                var JobReqPropService = propertyService.GetJobReqPropServiceByJobId(obj.JobReqId);
                var objJobInventory = propertyService.GetInventoryByJobReqId(JobReqPropService.JobRequestPropId, EditjobRequest.Property_List_Id);
                foreach (var item in objJobInventory)
                {
                    foreach (var item2 in objInventory)
                    {
                        if (item.InventoryId == item2.InventoryId)
                        {
                            item2.Qty = item.Qty;
                        }
                    }
                }
            }
            EditjobRequest.InventoryList = objInventory;
            if (obj.JobReqId != 0)
            {
                var Checklistdata = propertyService.GetJobReqChecklistByJobId((long)obj.JobReqId);
                List<JobRequestCheckListModel> objCheckList = new List<JobRequestCheckListModel>();
                if (Checklistdata != null)
                {
                    foreach (var item in Checklistdata)
                    {
                        JobRequestCheckListModel obj1 = new JobRequestCheckListModel();
                        obj1.Id = item.Id;
                        obj1.TaskDetail = item.TaskDetail;
                        obj1.JobRequestId = item.JobRequestId;
                        obj1.IsDone = item.IsDone;
                        objCheckList.Add(obj1);
                    }
                    EditjobRequest.CheckListDetails = objCheckList;
                }
            }

            return this.Ok(new
            {
                status = EditjobRequest != null ? true : false,
                message = EditjobRequest != null ? Resource.success : "No Record Found",
                data = EditjobRequest
            });
        }
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostEditJobRequest(EditJobApiData model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            EditJobRequestViewModel objJobData = new EditJobRequestViewModel();
            objJobData.JobStartDateTime = model.JobStartDateTime;
            objJobData.JobEndDateTime = model.JobEndDateTime;
            objJobData.JobReqId = model.JobReqId;
            objJobData.JobDesc = model.JobDesc;
            objJobData.Property_List_Id = model.Property_List_Id;
            objJobData.UserId = model.UserId;
            objJobData.AssignWorker = model.AssignWorker;
            objJobData.UserName = orderService.GetUserNameById((long)model.UserId);
            var status = orderService.EditjobDetails(objJobData);
            return this.Ok(new
            {
                status = status,
                message = orderService.message,
            });
        }
        #endregion

        #region Service Requests
        [System.Web.Http.HttpGet]
        public IHttpActionResult ServiceRequests(long UserId)
        {
            var result = orderService.GetAddServiceRequests(UserId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }
        [System.Web.Http.HttpPost]
        public IHttpActionResult IsAcceptRejectServiceRequest(AcceptRejectServiceRequest model)
        {
            /*value 1 as Accept and 2 as Reject Service Request*/
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            if (model.Status == 1)
            {
                var result = orderService.AcceptAddServiceRequest(model.ServiceReqId, model.UserId);
                return this.Ok(new
                {
                    status = result,
                    message = orderService.message
                });
            }
            else if (model.Status == 2)
            {
                bool status = orderService.RejectAddServiceRequest(model.ServiceReqId, model.UserId);
                return this.Ok(new
                {
                    status = status,
                    message = orderService.message
                });
            }
            else
            {
                return this.Ok(new
                {
                    status = false,
                    message = "Something went wrong!",
                });
            }
        }
        #endregion

        #region AddInventory Request
        [System.Web.Http.HttpPost]
        public IHttpActionResult AddInventoryRequest(InventoryReqest model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var UserName = orderService.GetUserNameById(model.UserId);
            bool status = orderService.AddInventoryRequest(model.InventoryName, model.UserId, UserName);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }
        #endregion

        #region Worker Send Service request

        [System.Web.Http.HttpPost]
        public IHttpActionResult GetAddMoreServiceDetail(GetAddServiceModelApi model)
        {
            GetAddServiceRequestDetail obj = new GetAddServiceRequestDetail();
            obj.Property = orderService.GetPropertyById(model.JobId);
            obj.JobStartDate = orderService.GetJobDateByJobId(model.JobId);
            obj.CategoryList = orderService.GetCategoryList();
            obj.UserId = model.UserId;
            return this.Ok(new
            {
                status = obj == null ? false : true,
                message = orderService.message,
                data = obj
            });
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult AddMoreService(AddServiceRequest model)
        {
            bool status = false;
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            status = orderService.AddServiceRequest(model.CatId, model.SubCatId, model.SubSubCatId, model.JobDateTime, model.PropertyId, model.Message, model.WorkerId, model.UserId, model.ServicePrice);
            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }
        #endregion

        #region GetDeliveryGuy

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDeliveryGuy(string JobDateTime)
        {
            var date = Convert.ToDateTime(JobDateTime);
            var DayofWeek = Convert.ToInt32(date.DayOfWeek);
            //var Date = date.ToShortDateString();
            //var time = date.ToShortTimeString();
            var dd = JobDateTime.ToString().Split(' ');
            var time = dd[0];
            var Date = dd[1];
            var Sp = orderService.GetDeliveryGuy(Date, DayofWeek, time);
            return this.Ok(new
            {
                status = Sp == 0 ? false : true,
                message = orderService.message,
                DeliveryGuyId = Sp
            });
        }
        #endregion

        #region Worker Add Inventory

        [System.Web.Http.HttpPost]
        public IHttpActionResult AddInventory(List<InventoryViewModel> InventoryList)
        {
            bool status = false;
            if (InventoryList == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            status = orderService.AddInventory(InventoryList);

            return this.Ok(new
            {
                status = status,
                message = orderService.message
            });
        }
        #endregion

        #region GetDeliveryguyDetail
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDeliveryGuyDetailById(long Id)
        {
            var result = orderService.GetDeliveryGuyDetailById(Id);
            return this.Ok(new
            {
                status = result.Count > 0 ? true : false,
                message = orderService.message,
                data = result
            });
        }
        #endregion

        [System.Web.Http.HttpPost]
        public IHttpActionResult DeliveryDone(DeliveryDoneApi model)
        {
            var result = orderService.IsDeliveryDone(model.DeliveryGuyId, model.JobReqPropServiceId);
            return this.Ok(new
            {
                status = result,
                message = orderService.message
            });
        }

    }
}
