using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BroomService.Services
{
    public class OrderService
    {
        BroomServiceEntities1 _db;
        public string message;
        public double? refundPrice = 0;
        AccountService accountService;
        public string description;
        public bool jobTimeAlert = false;
        public string unitprice;
        public double? servicePrice = 0;
        public List<string> JobReqId = new List<string>();
        public ICountResponse orderData;
        PropertyService propertyService;
        TokenService tokenService;

        public OrderService()
        {
            _db = new BroomServiceEntities1();
            accountService = new AccountService();
            propertyService = new PropertyService();
            tokenService = new TokenService();
        }

        #region Job Request

        /// <summary>
        /// Getting the list of the categories
        /// </summary>
        /// <returns></returns>
        public List<InventoryViewModel> GetInventoryList()
        {
            List<InventoryViewModel> inventory = new List<InventoryViewModel>();
            try
            {
                inventory = _db.Inventories.Where(a => a.IsActive == true)
                    .Select(a => new InventoryViewModel
                    {
                        Description = a.Description,
                        Image = a.Image,
                        InventoryId = a.InventoryId,
                        Name = a.Name,
                        Price = a.Price,
                        Stock = a.Stock
                    }).ToList();
            }
            catch (Exception ex)
            {
            }
            return inventory;
        }

        /// <summary>
        /// Add Job Request Method
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JobRequestSuccessViewModel AddJobRequest(JobRequestViewModel model)
        {
            JobRequestSuccessViewModel requestSuccessViewModel = new JobRequestSuccessViewModel
            {
                Status = false
            };
            try
            {
                decimal? _clientPrice = 0;
                decimal? _price = 0;
                var isSubSubCategory = false;

                var category = GetCategory(model.ServiceId);
                if (model.Categories != null && model.Categories.Count > 0)
                {
                    // check if windows cleaning and check-in-cleaning service
                    // Getting price of all the categories added in the job request
                    foreach (var item in model.Categories)
                    {
                        if (item.SubCategoryId == 12 || item.SubCategoryId == 26)
                        {
                            var priceModel = propertyService.AutoPriceService(model.PropertyId, (long)item.SubCategoryId);
                            _clientPrice += priceModel.ClientPrice;
                            _price += priceModel.Price;
                        }
                        else
                        {
                            var priceModel = GetPriceService((long)item.SubCategoryId, (long)item.SubSubCategoryyId);
                            _clientPrice += priceModel.ClientPrice;
                            _price += priceModel.Price;
                            if (item.SubSubCategoryyId != 0) isSubSubCategory = true;
                        }
                    }
                }

                if (model.PropertyService != null && model.PropertyService.Count > 0)
                {
                    // Deciding with property belong to which service
                    foreach (var item in model.PropertyService)
                    {
                        //double price = 0;
                        SubSubCategory getSubSubCat = null;
                        SubCategory getSubCat = null;

                        JobRequest requestlist = new JobRequest
                        {
                            UserId = model.UserId,
                            Description = model.JobDesc,
                            CreatedDate = DateTime.Now,
                            IsFastOrder = model.IsFastOrder,
                            FastOrderName = model.FastOrderName,
                            QuotePrice = _price,
                            HasPrice = _price > 0,
                            PaymentMethod = model.PaymentInfo,
                            IsPaymentDone = false,
                            ServicePrice = _clientPrice,
                            RepeatService = 0

                        };
                        unitprice = _price.ToString();
                        description = model.JobDesc;
                        _db.JobRequests.Add(requestlist);
                        _db.SaveChanges();

                        var userData = new User();
                        if (model.IsVisitor != true)
                        {
                            //--------------------------Add price in tbl IncomeSourse----------
                            userData = _db.Users.Where(a => a.UserId == model.UserId).FirstOrDefault();
                            if (userData != null)
                            {
                                IncomeSource incomeSource = new IncomeSource();
                                if (userData.JobPayType == Enums.JobRequestPayType.BeforeJob.GetHashCode()) //Before Job
                                {
                                    incomeSource.User_Id = Convert.ToInt32(userData.UserId);
                                    incomeSource.Income = _price;
                                    incomeSource.Pending = 0;
                                    incomeSource.Outcome = 0;
                                }
                                else if (userData.JobPayType == Enums.JobRequestPayType.AfterJob.GetHashCode()) //After job
                                {
                                    incomeSource.User_Id = Convert.ToInt32(userData.UserId);
                                    incomeSource.Income = 0;
                                    incomeSource.Pending = _price;
                                    incomeSource.Outcome = 0;
                                }
                                else if (userData.JobPayType == Enums.JobRequestPayType.OnceMonth.GetHashCode()) //Once in a Month
                                {
                                    incomeSource.User_Id = Convert.ToInt32(userData.UserId);
                                    incomeSource.Income = 0;
                                    incomeSource.Pending = 0;
                                    incomeSource.Outcome = _price;
                                }
                                _db.IncomeSources.Add(incomeSource);
                                _db.SaveChanges();
                            }
                        }

                        // TODO cleanup. Only one property will have in the job request

                        if (model.Property_List_Id != null)
                        {
                            // Adding multiple properties for the job requests
                            if (model.Property_List_Id.Count > 0)
                            {
                                foreach (var propertyId in model.Property_List_Id)
                                {
                                    var id = Convert.ToInt64(propertyId);
                                    PropertyJobRequest propertyJobRequest = new PropertyJobRequest
                                    {
                                        PropertyId = id,
                                        JobRequestId = requestlist.Id
                                    };
                                    _db.PropertyJobRequests.Add(propertyJobRequest);

                                    _db.SaveChanges();
                                }
                            }
                        }

                        if (userData.JobPayType == Enums.JobRequestPayType.OnceMonth.GetHashCode() || model.DocumentJob == null) //Once a Month Job{
                        {

                        }
                        else
                        {
                            //Add value on DocumentJob Table
                            DocumentJob documentJob = new DocumentJob();
                            documentJob.JobRequestId = requestlist.Id;
                            documentJob.UserId = model.UserId;
                            documentJob.Invoice = model.DocumentJob.doc_url;
                            //JobReqId = requestlist.Id.ToString();                        
                            _db.DocumentJobs.Add(documentJob);
                            _db.SaveChanges();
                        }

                        ServiceData getData = null;

                        if (isSubSubCategory)
                        {
                            getData = model.Categories.Where(a => a.SubSubCategoryyId == item.ServiceId).FirstOrDefault();
                        }
                        else
                        {
                            getData = model.Categories.Where(a => a.SubCategoryId == item.ServiceId).FirstOrDefault();
                        }

                        var stringID = requestlist.Id.ToString();
                        JobReqId.Add(stringID);

                        JobRequestPropertyService jobRequestPropertyService = new JobRequestPropertyService
                        {
                            JobRequestId = requestlist.Id,
                            ServiceId = isSubSubCategory ? getData.SubCategoryId : item.ServiceId,
                            PropertyId = item.PropertyId,
                            StartDateTime = item.StartDateTime,
                            TimeToDo = item.TimeToDo,
                            type = item.Type,
                            AssignedWorker = item.AssignedUserId,
                            JobStatus = _price > 0 ? Enums.RequestStatus.UnPaid.GetHashCode() : Enums.RequestStatus.QuoteRequested.GetHashCode(),
                            IsVisitor = model.IsVisitor
                        };
                        if (model.JobEndDateTime != null)
                        {
                            jobRequestPropertyService.EndDateTime = Convert.ToDateTime(model.JobEndDateTime);
                        }
                        _db.JobRequestPropertyServices.Add(jobRequestPropertyService);
                        _db.SaveChanges();


                        if (getData != null)
                        {
                            JobRequestSubSubCategory jobRequestSubCategory = new JobRequestSubSubCategory
                            {
                                JobRequestId = jobRequestPropertyService.JobRequestPropId,
                                SubSubCategoryId = getData.SubSubCategoryyId,
                                CategoryId = getData.CategoryId,
                                SubCategoryId = getData.SubCategoryId
                            };
                            _db.JobRequestSubSubCategories.Add(jobRequestSubCategory);

                            _db.SaveChanges();
                        }

                        var getTime = item.StartDateTime.ToString("HH:mm");

                        var time = Convert.ToDouble(jobRequestPropertyService.TimeToDo);

                        var t1 = TimeSpan.Parse(getTime);
                        var t2 = TimeSpan.FromHours(time);

                        var busyTime = t1 + t2;

                        //Check EndTime before/After Job Complete
                        if (model.JobEndDateTime != null)
                        {
                            DateTime startDateTime = Convert.ToDateTime(model.JobStartDateTime);
                            DateTime endDateTime = Convert.ToDateTime(model.JobEndDateTime);
                            var ServiceEndTimeTotake = startDateTime.AddHours(time).ToString("HH:mm");
                            var endtime = endDateTime.ToString("HH:mm");
                            var ts = TimeSpan.Parse(ServiceEndTimeTotake);
                            var te = TimeSpan.Parse(endtime);
                            if (te < ts)
                            {
                                jobTimeAlert = true;
                            }
                            else
                            {
                                jobTimeAlert = false;
                            }
                        }


                        var getWorkerData = _db.Users.Where(a => a.UserId == item.AssignedUserId).FirstOrDefault();

                        var getPropertyName = _db.Properties.Where(A => A.Id == jobRequestPropertyService.PropertyId).FirstOrDefault();

                        string serviceName = string.Empty;
                        serviceName = getSubSubCat != null ? getSubSubCat.Name : getSubCat != null ? getSubCat.Name : string.Empty;

                        if (_clientPrice <= 0)
                        {
                            #region Send Notification to the admin/supervisor
                            try
                            {
                                // Send notification to admin
                                Notification notification = new Notification();
                                notification.CreatedDate = DateTime.Now;
                                notification.FromUserId = item.AssignedUserId;
                                notification.IsActive = true;
                                notification.JobRequestId = requestlist.Id;
                                notification.NotificationStatus = Enums.NotificationStatus.SentQuotation.GetHashCode();
                                notification.ToUserId = accountService.GetAdminId();
                                notification.Text = "Quote request successfully send for service" + serviceName + " for the property " + getPropertyName.Name;

                                notification.ServiceName = serviceName;
                                notification.PropertyName = getPropertyName.Name;
                                notification.PropertyAddress = getPropertyName.Address;

                                _db.Notifications.Add(notification);
                                _db.SaveChanges();

                                // send notification to supervisor
                                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                                && A.IsActive == true).ToList();

                                if (getSupervisors.Count > 0)
                                {
                                    foreach (var supervisor in getSupervisors)
                                    {
                                        notification.ToUserId = supervisor.UserId;
                                        _db.Notifications.Add(notification);

                                        _db.SaveChanges();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                            #endregion

                            #region Send Notification To Customer
                            // send notification to the customer
                            Notification _notification1 = new Notification
                            {
                                CreatedDate = DateTime.Now,
                                FromUserId = item.AssignedUserId,
                                IsActive = true,
                                JobRequestId = requestlist.Id,
                                NotificationStatus = Enums.NotificationStatus.SentQuotation.GetHashCode(),
                                ToUserId = getPropertyName.CreatedBy,
                                ServiceName = serviceName,
                                PropertyName = getPropertyName.Name,
                                PropertyAddress = getPropertyName.Address,
                                Text = "Quote request successfully send for service" + serviceName + " for the property " + getPropertyName.Name
                            };
                            _db.Notifications.Add(_notification1);
                            _db.SaveChanges();
                            #endregion

                            #region Send Notification to Service Provider
                            // check if categoryData is for worker
                            if (!(bool)category.ForWorkers)
                            {
                                Notification _notification = new Notification
                                {
                                    CreatedDate = DateTime.Now,
                                    FromUserId = getPropertyName.CreatedBy,
                                    IsActive = true,
                                    JobRequestId = requestlist.Id,
                                    NotificationStatus = Enums.NotificationStatus.SentQuotation.GetHashCode(),
                                    ToUserId = item.AssignedUserId,
                                    ServiceName = serviceName,
                                    PropertyName = getPropertyName.Name,
                                    PropertyAddress = getPropertyName.Address,
                                    Text = "Quote request sent for service" + serviceName + " for the property " + getPropertyName.Name
                                };

                                _db.Notifications.Add(_notification);
                                _db.SaveChanges();
                            }
                            #endregion
                        }

                        if (model.InventoryList != null && model.InventoryList.Count > 0)
                        {
                            if (model.IsVisitor != true)
                            {
                                foreach (var item1 in model.InventoryList)
                                {
                                    JobInventory jobInventory = new JobInventory
                                    {
                                        InventoryId = item1.InventoryId,
                                        Qty = item1.Qty,
                                        JobRequestId = jobRequestPropertyService.JobRequestPropId,
                                        PropertyId = item1.PropertyId
                                    };
                                    _db.JobInventories.Add(jobInventory);
                                    _db.SaveChanges();
                                }

                                OccupiedProvider occupiedDeleveryGuy = new OccupiedProvider
                                {
                                    FromTime = getTime,
                                    ToTime = busyTime.ToString(),
                                    JobDateTime = item.StartDateTime,
                                    JobPropertyServiceId = jobRequestPropertyService.JobRequestPropId,
                                    ProviderId = model.InventoryList[0].DeliveryGuyId,
                                    IsDeliveryGuy = true
                                };
                                _db.OccupiedProviders.Add(occupiedDeleveryGuy);
                                _db.SaveChanges();
                            }
                        }

                        if (model.CheckList != null)
                        {
                            foreach (var item2 in model.CheckList)
                            {
                                JobRequestCheckList task = new JobRequestCheckList();
                                task.JobRequestId = requestlist.Id;
                                task.TaskDetail = item2.ToString();
                                _db.JobRequestCheckLists.Add(task);

                                _db.SaveChanges();
                            }
                        }

                        if (model.ReferenceImages != null)
                        {
                            foreach (var item3 in model.ReferenceImages)
                            {
                                JobRequestRefImage jobRequestRefImage = new JobRequestRefImage();
                                jobRequestRefImage.JobRequestId = requestlist.Id;
                                jobRequestRefImage.PicturePath = item3.ImageUrl;
                                jobRequestRefImage.IsImage = item3.IsImage;
                                jobRequestRefImage.IsVideo = item3.IsVideo;
                                jobRequestRefImage.VideoUrl = item3.VideoUrl;
                                _db.JobRequestRefImages.Add(jobRequestRefImage);

                                _db.SaveChanges();
                            }
                        }

                        requestSuccessViewModel.HasPrice = _clientPrice > 0;
                        requestSuccessViewModel.JobRequest = requestlist;
                        requestSuccessViewModel.PropertyService = jobRequestPropertyService;
                    }
                }


                _db.SaveChanges();

                message = Resource.job_request_add_success;

                requestSuccessViewModel.Status = true;
            }
            catch (Exception ex)
            {
                message = ex.InnerException.Message;
                requestSuccessViewModel.Status = false;
            }
            return requestSuccessViewModel;
        }

        public bool AddJobRequestSentBySupervisor(long? jobRequestId, DateTime? jobDateTime, long? WorkerId)
        {
            bool status = false;
            try
            {
                var getJobRequest = _db.JobRequests.Where(a => a.Id == jobRequestId).FirstOrDefault();
                JobRequestPropertyService getJobPropertyData = null;
                if (getJobRequest != null)
                {
                    getJobPropertyData = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == getJobRequest.Id).FirstOrDefault();
                    if (getJobPropertyData != null)
                    {
                        getJobPropertyData.StartDateTime = jobDateTime;
                        getJobPropertyData.AssignedWorker = WorkerId;
                        getJobPropertyData.JobStatus = (int)Enums.RequestStatus.Pending;
                        _db.SaveChanges();
                    }
                }

                var getTime = jobDateTime.HasValue ? jobDateTime.Value.ToString("HH:mm") : string.Empty;

                var time = Convert.ToDouble(getJobPropertyData.TimeToDo);

                var t1 = TimeSpan.Parse(getTime);
                var t2 = TimeSpan.FromHours(time);

                var busyTime = t1 + t2;

                // add data in occupied provider table that the assigned worker is now busy
                OccupiedProvider occupiedProvider = new OccupiedProvider();
                occupiedProvider.FromTime = getTime;
                occupiedProvider.ToTime = busyTime.ToString();
                occupiedProvider.JobDateTime = jobDateTime;
                occupiedProvider.JobPropertyServiceId = getJobPropertyData.JobRequestPropId;
                occupiedProvider.ProviderId = WorkerId;

                _db.OccupiedProviders.Add(occupiedProvider);
                _db.SaveChanges();

                var getWorkerData = _db.Users.Where(a => a.UserId == WorkerId).FirstOrDefault();

                var getPropertyName = _db.Properties.Where(A => A.Id == getJobPropertyData.PropertyId).FirstOrDefault();

                string serviceName = string.Empty;

                SubSubCategory getSubSubCat = null;
                SubCategory getSubCat = null;
                if (getJobPropertyData.type == true)
                {
                    // sub sub category
                    getSubSubCat = _db.SubSubCategories.Where(a => a.Id == getJobPropertyData.ServiceId).FirstOrDefault();
                }
                else
                {
                    // sub category
                    getSubCat = _db.SubCategories.Where(a => a.Id == getJobPropertyData.ServiceId).FirstOrDefault();
                }

                serviceName = getSubSubCat != null ? getSubSubCat.Name : getSubCat != null ? getSubCat.Name : string.Empty;

                #region Send Notification to the admin/supervisor

                // Send notification to admin
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = WorkerId;
                notification.IsActive = true;
                notification.JobRequestId = jobRequestId;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = accountService.GetAdminId();
                notification.Text = getWorkerData.FullName + " has been assigned for the service " + serviceName + " for the property " + getPropertyName.Name +
                    "<br/>Job Start Time: " + jobDateTime + " Time To do the service: " + getJobPropertyData.TimeToDo;
                _db.Notifications.Add(notification);
                _db.SaveChanges();


                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                && A.IsActive == true).ToList();

                if (getSupervisors.Count > 0)
                {
                    foreach (var supervisor in getSupervisors)
                    {
                        notification.ToUserId = supervisor.UserId;
                        _db.Notifications.Add(notification);

                        _db.SaveChanges();
                    }
                }

                #endregion

                #region Send Notification To Customer

                // send notification to the customer
                Notification _notification1 = new Notification();
                _notification1.CreatedDate = DateTime.Now;
                _notification1.FromUserId = WorkerId;
                _notification1.IsActive = true;
                _notification1.JobRequestId = jobRequestId;
                _notification1.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                _notification1.ToUserId = getPropertyName.CreatedBy;
                _notification1.Text = getWorkerData.FullName + " has been assigned to you for the property " + getPropertyName.Name +
                    " " + "for the service " + serviceName +
                    "<br/>Job Start Time: " + jobDateTime;
                _db.Notifications.Add(_notification1);
                _db.SaveChanges();

                #endregion

                #region Send Notification to Worker

                // send notification to the assigned worker
                Notification _notification = new Notification();
                _notification.CreatedDate = DateTime.Now;
                _notification.FromUserId = getPropertyName.CreatedBy;
                _notification.IsActive = true;
                _notification.JobRequestId = jobRequestId;
                _notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                _notification.ToUserId = WorkerId;
                _notification.Text = serviceName + " has been assigned to you for the property " + getPropertyName.Name +
                    "<br/>Job Start Time: " + getJobPropertyData.StartDateTime + " Time To do the service: " + getJobPropertyData.TimeToDo;
                _db.Notifications.Add(_notification);
                _db.SaveChanges();

                #endregion

                message = Resource.job_request_add_success;
                status = true;
            }
            catch (Exception ex)
            {
                message = ex.InnerException.Message;
                status = false;
                status = false;
            }
            return status;
        }

        public bool SaveReceiptDocument(JobRequestViewModel model)
        {
            bool status = false;
            try
            {
                if (model.IdReceipt != null)
                {
                    for (int i = 0; i < model.IdReceipt.Count; i++)
                    {
                        var id = int.Parse(model.IdReceipt[i].ToString());
                        var jobReq = _db.DocumentJobs.Where(x => x.JobRequestId == id).FirstOrDefault();
                        if (jobReq != null)
                        {
                            jobReq.Receipt = model.DocumentJob.doc_url;
                            _db.SaveChanges();

                            status = true;
                            message = Resource.success;
                        }
                    }
                }
                else
                {
                    var jobReq = _db.DocumentJobs.Where(x => x.JobRequestId == model.Id).FirstOrDefault();
                    if (jobReq != null)
                    {
                        jobReq.Receipt = model.DocReceipt_Url;
                        _db.SaveChanges();
                        status = true;
                        message = Resource.success;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public bool QuoteTypeDetails(JobRequestQuoteTypeViewModel model)
        {
            bool status = false;
            try
            {
                JobRequestQuoteType quoteTypeDetails = new JobRequestQuoteType();
                quoteTypeDetails.JobRequestId = model.JobRequestId;
                quoteTypeDetails.UserId = model.UserId;
                quoteTypeDetails.CreatedDate = DateTime.Now;
                quoteTypeDetails.Type = model.Type;
                if (model.Type == Enums.QuoteTypeEnum.Meeting.GetHashCode())
                {
                    quoteTypeDetails.MeetingTime = model.MeetingTime;
                }
                _db.JobRequestQuoteTypes.Add(quoteTypeDetails);
                _db.SaveChanges();

                if (model.Type == Enums.QuoteTypeEnum.SendPrice.GetHashCode())
                {
                    if (model.Images != null)
                    {
                        if (model.Images.Count > 0)
                        {
                            foreach (var item in model.Images)
                            {
                                JobRequestQuoteDetail requestQuoteDetail = new JobRequestQuoteDetail();
                                requestQuoteDetail.Description = model.Description;
                                requestQuoteDetail.QuoteTypeId = quoteTypeDetails.QuoteTypeId;
                                requestQuoteDetail.ImageUrl = item.ImageUrl;
                                requestQuoteDetail.IsImage = item.IsImage;
                                requestQuoteDetail.IsVideo = item.IsVideo;
                                requestQuoteDetail.VideoUrl = item.VideoUrl;
                                _db.JobRequestQuoteDetails.Add(requestQuoteDetail);

                                _db.SaveChanges();
                            }
                        }
                    }
                }

                message = Resource.details_sent_success;
                status = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public List<AvailableTimeViewModel> GetAvailabilities(ServiceData data, int user_type)
        {
            List<GetWorkers> listofWorkers = new List<GetWorkers>();

            //var getJobDateTime = Convert.ToDateTime(data.SelectedTime);

            // get workers whose schedule is avail for the service day of week

            List<GetWorkers> getWorkersAvailability = new List<GetWorkers>();
            if (data.SubSubCategoryyId != 0)
            {
                getWorkersAvailability = (from availableSchedule in _db.tblProviderAvailableTimes
                                          join workers in _db.Users
                                          on availableSchedule.provider_id equals workers.UserId
                                          join subCat in _db.UserSubCategories
                                          on workers.UserId equals subCat.UserId
                                          where availableSchedule.vDate > DateTime.Now
                                          && availableSchedule.isCausallOff == false
                                          && availableSchedule.isOptionalOff == false
                                          && (workers.UserType == user_type)
                                          && (workers.standby == null || workers.standby == false)
                                          && subCat.SubSubCategoryId == data.SubSubCategoryyId
                                          select new GetWorkers
                                          {
                                              Day = availableSchedule.day_of_week,
                                              ToTime = availableSchedule.to_time,
                                              FromTime = availableSchedule.from_time,
                                              UserId = workers.UserId,
                                              Date = (DateTime)availableSchedule.vDate
                                          }).ToList();
            }
            else if (data.SubCategoryId != 0)
            {
                getWorkersAvailability = (from availableSchedule in _db.tblProviderAvailableTimes
                                          join workers in _db.Users
                                          on availableSchedule.provider_id equals workers.UserId
                                          join subCat in _db.UserSubCategories
                                          on workers.UserId equals subCat.UserId
                                          where availableSchedule.vDate > DateTime.Now
                                          && availableSchedule.isCausallOff == false
                                          && availableSchedule.isOptionalOff == false
                                          && (workers.UserType == user_type)
                                          && (workers.standby == null || workers.standby == false)
                                          && subCat.SubCategoryId == data.SubCategoryId

                                          select new GetWorkers
                                          {
                                              Day = availableSchedule.day_of_week,
                                              ToTime = availableSchedule.to_time,
                                              FromTime = availableSchedule.from_time,
                                              UserId = workers.UserId,
                                              Date = (DateTime)availableSchedule.vDate
                                          }).ToList();
            }

            var availabilities = getWorkersAvailability.Select(x => new AvailableTimeViewModel()
            {
                FromTime = x.FromTime,
                ToTime = x.ToTime,
                AvailableDate = x.Date.ToString("yyyy-MM-dd"),
                UserId = x.UserId
            }).ToList();

            // Distinct
            availabilities = availabilities.GroupBy(x => new { x.AvailableDate, x.UserId })
                .Select(x => x.First())
                .ToList();


            return availabilities;
        }

        public List<AvailableTimeViewModel> GetBookedTimes(long propertyId)
        {
            try
            {
                var property = _db.Properties.FirstOrDefault(x => x.Id == propertyId);
                // occupied providers property address
                // with the given property address calculate distance.
                var bookedTimes = (from oc in _db.OccupiedProviders
                                   join ps in _db.JobRequestPropertyServices on oc.JobPropertyServiceId equals ps.JobRequestPropId
                                   join p in _db.Properties on ps.PropertyId equals p.Id
                                   where oc.JobDateTime > DateTime.Now
                                   select new AvailableTimeViewModel
                                   {
                                       FromTime = oc.FromTime,
                                       ToTime = oc.ToTime,
                                       JobDate = oc.JobDateTime,
                                       UserId = (long)oc.ProviderId,
                                       Address = p.Address
                                   }).ToList();

                foreach (var item in bookedTimes)
                {
                    item.JobStartDate = item.JobDate.Value.ToString("s");
                    var distanceTimeVM = Common.CalculateDistanceTime(item.Address, property.Address);
                    item.Distance = distanceTimeVM.Distance;
                }

                return bookedTimes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<AvailableTimeViewModel>();
            }
        }

        public List<AvailableTimeViewModel> GetWorkersDistance(long propertyId, int userType)
        {
            var workerDistances = new List<AvailableTimeViewModel>();
            // get the property
            var property = _db.Properties.FirstOrDefault(x => x.Id == propertyId);
            if (property == null) return workerDistances;
            // get the workers or service providers
            var workers = _db.Users.Where(x => x.UserType == userType).ToList();

            foreach (var worker in workers)
            {
                var distanceTimeVM = Common.CalculateDistanceTime(worker?.Address, property?.Address);
                workerDistances.Add(new AvailableTimeViewModel
                {
                    UserId = worker.UserId,
                    Distance = distanceTimeVM.Distance
                });
            }

            return workerDistances;
        }

        public List<GetWorkers> GetTimingsAvaiableForService(ServiceData data)
        {
            List<GetWorkers> listofWorkers = new List<GetWorkers>();

            //var getJobDateTime = Convert.ToDateTime(data.SelectedTime);

            var worker_type = Enums.UserTypeEnum.Worker.GetHashCode();
            var service_provider_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();

            // get workers whose schedule is avail for the service day of week

            List<GetWorkers> getWorkersWhereDayAvailable = new List<GetWorkers>();
            if (data.SubSubCategoryyId != 0)
            {
                getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                               join workers in _db.Users
                                               on avialableSchedule.provider_id equals workers.UserId
                                               join subCat in _db.UserSubCategories
                                               on workers.UserId equals subCat.UserId
                                               where avialableSchedule.day_of_week == data.Day
                                               && (workers.UserType == worker_type
                                               || workers.UserType == service_provider_type)
                                               && (workers.standby == null || workers.standby == false)
                                               && subCat.SubSubCategoryId == data.SubSubCategoryyId
                                               select new GetWorkers
                                               {
                                                   Day = avialableSchedule.day_of_week,
                                                   ToTime = avialableSchedule.to_time,
                                                   FromTime = avialableSchedule.from_time,
                                                   UserId = workers.UserId
                                               }).ToList();
            }
            else if (data.SubCategoryId != 0)
            {
                getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                               join workers in _db.Users
                                               on avialableSchedule.provider_id equals workers.UserId
                                               join subCat in _db.UserSubCategories
                                               on workers.UserId equals subCat.UserId
                                               where avialableSchedule.day_of_week == data.Day
                                               && (workers.UserType == worker_type
                                               || workers.UserType == service_provider_type)
                                               && (workers.standby == null || workers.standby == false)
                                               && subCat.SubCategoryId == data.SubCategoryId

                                               select new GetWorkers
                                               {
                                                   Day = avialableSchedule.day_of_week,
                                                   ToTime = avialableSchedule.to_time,
                                                   FromTime = avialableSchedule.from_time,
                                                   UserId = workers.UserId
                                               }).ToList();
            }
            if (getWorkersWhereDayAvailable.Count > 0)
            {
                // get workers which are having time match to service time
                foreach (var item in getWorkersWhereDayAvailable)
                {
                    var worker_from_time = TimeSpan.Parse(item.FromTime);
                    var worker_to_time = TimeSpan.Parse(item.ToTime);

                    int num = (int)item.Day;
                    int num2 = (int)DateTime.Today.DayOfWeek;
                    DateTime result = DateTime.Today.AddDays(num - num2);

                    // remove workers that are already occupied at that time
                    var getOcuupiedProvidersList = _db.OccupiedProviders.Where(a => a.ProviderId == item.UserId).ToList();

                    if (getOcuupiedProvidersList.Count > 0)
                    {
                        // check with same date and with same provider, if available then check
                        // if the provider is between this time, then remove the worker from the list
                        for (int i = 0; i < getOcuupiedProvidersList.Count; i++)
                        {
                            if (getOcuupiedProvidersList[i].JobDateTime.Value.Date == result.Date)
                            {
                                // 10:00 to 02:00 // 04:00 to 06:00
                                var worker_busy_from_time = TimeSpan.Parse(getOcuupiedProvidersList[i].FromTime);
                                var worker_busy_to_time = TimeSpan.Parse(getOcuupiedProvidersList[i].ToTime);

                                if (worker_from_time <= worker_busy_from_time && worker_to_time >= worker_busy_to_time)
                                {
                                    // worker available
                                    listofWorkers.Add(item);
                                }
                            }
                            else
                            {
                                if (!listofWorkers.Contains(item))
                                {
                                    listofWorkers.Add(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        listofWorkers.Add(item);
                    }
                }
            }
            return listofWorkers;
        }
        public List<GetWorkers> GetTimingsAvaiableWorker(ServiceData data, string JobstartDate, decimal TimeToDo)
        {
            List<GetWorkers> listofWorkers = new List<GetWorkers>();
            // List<GetWorkers> listofworkerBusytime = new List<GetWorkers>();
            var ListofFreeWorkers = new List<GetWorkers>();
            if (TimeToDo == 0)
            {//if price is not set for some reason the set it default 2
                TimeToDo = 2;
            }

            try
            {
                //long AssignedWorkerId = 0;

                //var getJobDateTime = Convert.ToDateTime(data.SelectedTime);
                int weekNumber = GetWeekNumber(Convert.ToDateTime(JobstartDate));
                var worker_type = Enums.UserTypeEnum.Worker.GetHashCode();
                var service_provider_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();

                // get workers whose schedule is avail for the service day of week

                List<GetWorkers> getWorkersWhereDayAvailable = new List<GetWorkers>();
                if (data.SubSubCategoryyId != 0)
                {
                    getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                                   join workers in _db.Users
                                                   on avialableSchedule.provider_id equals workers.UserId
                                                   join subCat in _db.UserSubCategories
                                                   on workers.UserId equals subCat.UserId
                                                   where avialableSchedule.day_of_week == data.Day
                                                   && avialableSchedule.isCausallOff == false
                                                   && avialableSchedule.isOptionalOff == false
                                                   && avialableSchedule.week_of_year == weekNumber
                                                   && (workers.UserType == worker_type
                                                   || workers.UserType == service_provider_type)
                                                   && (workers.standby == null || workers.standby == false)
                                                   && subCat.SubSubCategoryId == data.SubSubCategoryyId
                                                   select new GetWorkers
                                                   {
                                                       Day = avialableSchedule.day_of_week,
                                                       ToTime = avialableSchedule.to_time,
                                                       FromTime = avialableSchedule.from_time,
                                                       UserId = workers.UserId,
                                                       WorkerType = workers.WorkerQuesType
                                                   }).OrderBy(x => x.WorkerType).ToList();
                }
                else if (data.SubCategoryId != 0)
                {
                    getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                                   join workers in _db.Users
                                                   on avialableSchedule.provider_id equals workers.UserId
                                                   join subCat in _db.UserSubCategories
                                                   on workers.UserId equals subCat.UserId
                                                   where avialableSchedule.day_of_week == data.Day
                                                   && avialableSchedule.isCausallOff == false
                                                   && avialableSchedule.isOptionalOff == false
                                                   && avialableSchedule.week_of_year == weekNumber
                                                   && (workers.UserType == worker_type
                                                   || workers.UserType == service_provider_type)
                                                   && (workers.standby == null || workers.standby == false)
                                                   && subCat.SubCategoryId == data.SubCategoryId


                                                   select new GetWorkers
                                                   {
                                                       Day = avialableSchedule.day_of_week,
                                                       ToTime = avialableSchedule.to_time,
                                                       FromTime = avialableSchedule.from_time,
                                                       UserId = workers.UserId,
                                                       WorkerType = workers.WorkerQuesType
                                                   }).OrderBy(x => x.WorkerType).ToList();
                }


                if (getWorkersWhereDayAvailable != null)
                {
                    for (int i = 0; i < getWorkersWhereDayAvailable.Count; i++)
                    {
                        long ProviderId = getWorkersWhereDayAvailable[i].UserId;
                        var WorkerScheduleByDate = _db.tblProviderScheduleByDates.Where(x => x.ProviderId == ProviderId && x.Date == JobstartDate).FirstOrDefault();
                        if (WorkerScheduleByDate != null)
                        {
                            if (WorkerScheduleByDate.IsAbsent == true)
                            {
                                getWorkersWhereDayAvailable.RemoveAt(i);
                            }
                            else
                            {
                                getWorkersWhereDayAvailable[i].FromTime = WorkerScheduleByDate.FromTime;
                                getWorkersWhereDayAvailable[i].ToTime = WorkerScheduleByDate.ToTime;
                            }
                        }
                    }
                }


                if (getWorkersWhereDayAvailable.Count > 0)
                {
                    // get workers which are having time match to service time
                    foreach (var item in getWorkersWhereDayAvailable)
                    {
                        var WorkerFromTime = string.Empty;
                        var WorkerToTime = string.Empty;

                        if (item.FromTime == null || item.FromTime == "")
                        {
                            WorkerFromTime = "08"; //"09:00";
                            WorkerToTime = "17"; //"18:00";
                        }
                        else
                        {
                            var worker_from_time = TimeSpan.Parse(item.FromTime);
                            var worker_to_time = TimeSpan.Parse(item.ToTime);
                            var from = worker_from_time.ToString().Split(':');
                            var to = worker_to_time.ToString().Split(':');
                            WorkerFromTime = from[0];
                            WorkerToTime = to[0];
                        }


                        int num = (int)item.Day;
                        int num2 = (int)DateTime.Today.DayOfWeek;
                        DateTime result = DateTime.Today.AddDays(num - num2);
                        DateTime resultCurrent = DateTime.ParseExact(JobstartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                        DateTime resultAfter = resultCurrent.AddDays(1);
                        var startTime = resultCurrent.ToString("HH:mm");
                        var jobStartTime = TimeSpan.Parse(startTime);
                        // remove workers that are already occupied at that time
                        var getOcuupiedProvidersList = _db.OccupiedProviders.Where(a => a.ProviderId == item.UserId && a.JobDateTime >= resultCurrent && a.JobDateTime <= resultAfter).ToList();

                        if (getOcuupiedProvidersList.Count == 0) // when worker does't have any service on a day
                        {

                            int endTime = Convert.ToInt32(Convert.ToInt32(WorkerToTime) - TimeToDo);
                            for (int i = Convert.ToInt32(WorkerFromTime); i <= endTime; i++)
                            {
                                GetWorkers WorkerTimeSlot = new GetWorkers();
                                int StartTime = i;
                                WorkerTimeSlot.FromTime = StartTime.ToString() + ":00";
                                WorkerTimeSlot.FromTimeInt = StartTime;
                                WorkerTimeSlot.UserId = item.UserId;
                                WorkerTimeSlot.TimeToDo = TimeToDo;
                                if (listofWorkers.Where(x => x.FromTime == WorkerTimeSlot.FromTime).ToList().Count > 0)
                                {
                                }
                                else
                                {
                                    listofWorkers.Add(WorkerTimeSlot);
                                }
                            }
                        }
                        else if (getOcuupiedProvidersList.Count > 0) // when worker already have some services
                        {
                            List<GetWorkers> listofworker_Schedule = new List<GetWorkers>();
                            int endTime = Convert.ToInt32(Convert.ToInt32(WorkerToTime) - TimeToDo);
                            for (int i = Convert.ToInt32(WorkerFromTime); i <= endTime; i++)
                            {
                                GetWorkers WorkerTimeSlot = new GetWorkers();
                                int StartTime = i;
                                WorkerTimeSlot.FromTime = StartTime.ToString() + ":00";
                                WorkerTimeSlot.FromTimeInt = StartTime;
                                WorkerTimeSlot.UserId = item.UserId;
                                WorkerTimeSlot.TimeToDo = TimeToDo;
                                if (listofWorkers.Where(x => x.FromTime == WorkerTimeSlot.FromTime).ToList().Count > 0)
                                {
                                }
                                else
                                {
                                    listofworker_Schedule.Add(WorkerTimeSlot);
                                }
                            }

                            foreach (var workerlist in getOcuupiedProvidersList)
                            {
                                var temp = new List<GetWorkers>();
                                var t1 = workerlist.FromTime.Split(':');
                                var t2 = workerlist.ToTime.Split(':');
                                int FromTime = Convert.ToInt32(t1[0]);
                                int ToTime = Convert.ToInt32(t2[0]);
                                for (int i = FromTime; i < ToTime; i++)
                                {
                                    var te = i.ToString() + ":00";
                                    for (int del = 0; del < listofworker_Schedule.Count; del++)
                                    {
                                        if (listofworker_Schedule[del].FromTime == te)
                                        {
                                            listofworker_Schedule.RemoveAt(del);

                                        }
                                    }
                                }

                            }
                            if (listofworker_Schedule != null)
                            {
                                foreach (var Availableslot in listofworker_Schedule)
                                {
                                    if (listofWorkers.Contains(Availableslot)) // if sloat already added into list then 
                                    {

                                    }
                                    else
                                    {
                                        listofWorkers.Add(Availableslot);
                                    }
                                }
                            }

                        }

                    }
                }

                ListofFreeWorkers = listofWorkers.OrderBy(x => x.FromTimeInt).ToList();
            }
            catch (Exception ex)
            {
                ListofFreeWorkers = null;
            }
            return ListofFreeWorkers;
        }

        public int IsWorkerAvailable(ServiceData data, DateTime JobstartDate, decimal TimeToDo)
        {
            int vAvailability = -1;
            List<GetWorkers> listofWorkers = new List<GetWorkers>();
            var ListofFreeWorkers = new List<GetWorkers>();
            if (TimeToDo == 0)
            {
                TimeToDo = 2;
            }

            try
            {
                int weekNumber = GetWeekNumber(JobstartDate);
                var worker_type = Enums.UserTypeEnum.Worker.GetHashCode();
                var service_provider_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();

                List<GetWorkers> getWorkersWhereDayAvailable = new List<GetWorkers>();
                if (data.SubSubCategoryyId != 0)
                {
                    getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                                   join workers in _db.Users
                                                   on avialableSchedule.provider_id equals workers.UserId
                                                   join subCat in _db.UserSubCategories
                                                   on workers.UserId equals subCat.UserId
                                                   where avialableSchedule.day_of_week == data.Day
                                                   && avialableSchedule.isCausallOff == false
                                                   && avialableSchedule.isOptionalOff == false
                                                   && avialableSchedule.week_of_year == weekNumber
                                                   && (workers.UserType == worker_type
                                                   || workers.UserType == service_provider_type)
                                                   && (workers.standby == null || workers.standby == false)
                                                   && subCat.SubSubCategoryId == data.SubSubCategoryyId
                                                   select new GetWorkers
                                                   {
                                                       Day = avialableSchedule.day_of_week,
                                                       ToTime = avialableSchedule.to_time,
                                                       FromTime = avialableSchedule.from_time,
                                                       UserId = workers.UserId,
                                                       WorkerType = workers.WorkerQuesType
                                                   }).OrderBy(x => x.WorkerType).ToList();
                }
                else if (data.SubCategoryId != 0)
                {
                    getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                                   join workers in _db.Users
                                                   on avialableSchedule.provider_id equals workers.UserId
                                                   join subCat in _db.UserSubCategories
                                                   on workers.UserId equals subCat.UserId
                                                   where avialableSchedule.day_of_week == data.Day
                                                   && avialableSchedule.isCausallOff == false
                                                   && avialableSchedule.isOptionalOff == false
                                                   && avialableSchedule.week_of_year == weekNumber
                                                   && (workers.UserType == worker_type
                                                   || workers.UserType == service_provider_type)
                                                   && (workers.standby == null || workers.standby == false)
                                                   && subCat.SubCategoryId == data.SubCategoryId


                                                   select new GetWorkers
                                                   {
                                                       Day = avialableSchedule.day_of_week,
                                                       ToTime = avialableSchedule.to_time,
                                                       FromTime = avialableSchedule.from_time,
                                                       UserId = workers.UserId,
                                                       WorkerType = workers.WorkerQuesType
                                                   }).OrderBy(x => x.WorkerType).ToList();
                }

                int days = -1;
                TimeSpan WorkerFromTime;
                TimeSpan WorkerToTime;

                foreach (var item in getWorkersWhereDayAvailable)
                {
                    if (!string.IsNullOrEmpty(item.FromTime) && !string.IsNullOrEmpty(item.ToTime))
                    {
                        if (TimeSpan.TryParse(item.FromTime, out WorkerFromTime) && TimeSpan.TryParse(item.ToTime, out WorkerToTime))
                        {
                            days = (WorkerToTime - WorkerFromTime).Days;
                            vAvailability = days >= TimeToDo ? 1 : 0;
                            break;
                        }
                    }
                }
                if (days < 1) vAvailability = -1;
            }
            catch
            {
                return -1;
            }

            return vAvailability;
        }

        public List<GetWorkers> IsAvailableLaundryOrInventory(ServiceData data, string JobstartDate)
        {
            int weekNumber = GetWeekNumber(Convert.ToDateTime(JobstartDate));
            var worker_type = Enums.UserTypeEnum.Worker.GetHashCode();
            var delivery_guy_type = Enums.UserTypeEnum.DeliveryGuy.GetHashCode();
            var service_provider_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();

            // get workers whose schedule is avail for the service day of week

            List<GetWorkers> getWorkersWhereDayAvailable = new List<GetWorkers>();
            // Laundry
            getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                           join workers in _db.Users
                                           on avialableSchedule.provider_id equals workers.UserId
                                           join subCat in _db.UserSubCategories
                                           on workers.UserId equals subCat.UserId
                                           where avialableSchedule.day_of_week == data.Day
                                           && avialableSchedule.isCausallOff == false
                                           && avialableSchedule.isOptionalOff == false
                                           && avialableSchedule.week_of_year == weekNumber
                                           && (workers.UserType == delivery_guy_type || workers.UserType == service_provider_type)
                                           && (workers.standby == null || workers.standby == false)
                                           && subCat.CategoryId == data.CategoryId


                                           select new GetWorkers
                                           {
                                               Day = avialableSchedule.day_of_week,
                                               ToTime = avialableSchedule.to_time,
                                               FromTime = avialableSchedule.from_time,
                                               UserId = workers.UserId,
                                               WorkerType = workers.WorkerQuesType
                                           }).OrderBy(x => x.WorkerType).ToList();

            // Here calculate the shortest path of the delivery guy from propertyA to propertyB.
            return getWorkersWhereDayAvailable;
        }

        public bool CheckRoomAvailability(string JobDate, long PropId)
        {
            bool status = false;
            try
            {
                int? NoOfRooms = 0;
                var data = _db.Properties.Where(x => x.Id == PropId).FirstOrDefault();
                if (data != null)
                {
                    NoOfRooms = data.NoOfBedRooms;
                }
                var Jobdata = _db.JobRequestPropertyServices.Where(x => x.PropertyId == PropId && x.IsVisitor == true).ToList();
                int count = 0;
                if (Jobdata != null)
                {
                    foreach (var item in Jobdata)
                    {
                        if (item.StartDateTime.ToString().Contains(JobDate))
                        {
                            count++;
                        }
                    }
                    if (count < NoOfRooms)
                    {
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        public List<GetWorkers> GetAlternativeTimingsForService(ServiceData data)
        {
            List<GetWorkers> listofWorkers = new List<GetWorkers>();

            var worker_type = Enums.UserTypeEnum.Worker.GetHashCode();
            var service_provider_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();

            // get workers whose schedule is avail for the service

            List<GetWorkers> getWorkersWhereDayAvailable = new List<GetWorkers>();
            if (data.SubSubCategoryyId != 0)
            {
                getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                               join workers in _db.Users
                                               on avialableSchedule.provider_id equals workers.UserId
                                               join subCat in _db.UserSubCategories
                                               on workers.UserId equals subCat.UserId
                                               where (workers.UserType == worker_type
                                               || workers.UserType == service_provider_type)
                                               && (workers.standby == null || workers.standby == false)
                                               && subCat.SubSubCategoryId == data.SubSubCategoryyId
                                               select new GetWorkers
                                               {
                                                   Day = avialableSchedule.day_of_week,
                                                   ToTime = avialableSchedule.to_time,
                                                   FromTime = avialableSchedule.from_time,
                                                   UserId = workers.UserId
                                               }).ToList();
            }
            else if (data.SubCategoryId != 0)
            {
                getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                               join workers in _db.Users
                                               on avialableSchedule.provider_id equals workers.UserId
                                               join subCat in _db.UserSubCategories
                                               on workers.UserId equals subCat.UserId
                                               where (workers.UserType == worker_type
                                               || workers.UserType == service_provider_type)
                                               && (workers.standby == null || workers.standby == false)
                                               && subCat.SubCategoryId == data.SubCategoryId

                                               select new GetWorkers
                                               {
                                                   Day = avialableSchedule.day_of_week,
                                                   ToTime = avialableSchedule.to_time,
                                                   FromTime = avialableSchedule.from_time,
                                                   UserId = workers.UserId
                                               }).ToList();
            }

            if (getWorkersWhereDayAvailable.Count > 0)
            {
                // get workers which are having time match to service time
                foreach (var item in getWorkersWhereDayAvailable)
                {
                    TimeSpan worker_from_time = new TimeSpan();
                    TimeSpan worker_to_time = new TimeSpan();
                    if (!string.IsNullOrEmpty(item.FromTime))
                    {
                        worker_from_time = TimeSpan.Parse(item.FromTime);
                    }
                    if (!string.IsNullOrEmpty(item.ToTime))
                    {
                        worker_to_time = TimeSpan.Parse(item.ToTime);
                    }

                    int num = (int)item.Day;
                    int num2 = (int)DateTime.Today.DayOfWeek;
                    DateTime result = DateTime.Today.AddDays(num - num2);

                    // remove workers that are already occupied at that time
                    var getOcuupiedProvidersList = _db.OccupiedProviders.Where(a => a.ProviderId == item.UserId).ToList();

                    if (getOcuupiedProvidersList.Count > 0)
                    {
                        // check with same date and with same provider, if available then check
                        // if the provider is between this time, then remove the worker from the list
                        for (int i = 0; i < getOcuupiedProvidersList.Count; i++)
                        {
                            if (getOcuupiedProvidersList[i].JobDateTime.Value.Date == result.Date)
                            {
                                // 10:00 to 02:00 // 04:00 to 06:00
                                var worker_busy_from_time = TimeSpan.Parse(getOcuupiedProvidersList[i].FromTime);
                                var worker_busy_to_time = TimeSpan.Parse(getOcuupiedProvidersList[i].ToTime);

                                if (worker_from_time >= worker_busy_from_time
                                    && worker_to_time <= worker_busy_to_time)
                                {
                                    // worker available
                                    listofWorkers.Add(item);
                                }
                            }
                            else
                            {
                                listofWorkers.Add(item);
                            }
                        }
                    }
                    else
                    {
                        listofWorkers.Add(item);
                    }
                }
            }
            return listofWorkers;
        }

        public List<AvailableTimeViewModel> GetAvaiability()
        {
            var availableTimes = _db.tblProviderAvailableTimes.AsEnumerable()
                .Where(a => a.isCausallOff == false &&
                a.isOptionalOff == false &&
                a.vDate >= DateTime.Now && a.vDate <= DateTime.Now.AddDays(15))
                .Select(x => new AvailableTimeViewModel()
                {
                    Id = x.Id,
                    FromTime = x.from_time,
                    ToTime = x.to_time,
                    AvailableDate = x.vDate.GetValueOrDefault().ToString("yyyy-MM-dd"),
                    UserEmail = x.User.Email,
                    UserId = x.User.UserId

                }).ToList();

            return availableTimes;
        }


        // a msg will be sent to the supervisors and admin they can try and contact manualy the sp to open an avalable date on his schedule
        //if they dont succeed then they can replay that there are no avalable dates for this service soon only on the dates the client can see on the app

        public bool ContactToSupport(long UserId)
        {
            bool status = false;
            try
            {
                //var getJobServiceData = _db.JobRequestPropertyServices.Where(a => a.JobRequestPropId == model.JoPropId).FirstOrDefault();

                var clientData = _db.Users.Where(a => a.UserId == UserId).FirstOrDefault();

                #region Send Notification to the admin/supervisor

                // Send notification to admin
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = UserId;
                notification.IsActive = true;
                notification.JobRequestId = 0;
                notification.NotificationStatus = Enums.NotificationStatus.ContactSupport.GetHashCode();
                notification.ToUserId = accountService.GetAdminId();
                var getName = string.IsNullOrEmpty(clientData.FullName) ? clientData.Email : clientData.FullName;
                notification.Text = getName + " has requested for the customer support as client wants to book in an unavailable time.";

                _db.Notifications.Add(notification);
                _db.SaveChanges();


                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                && A.IsActive == true).ToList();

                if (getSupervisors.Count > 0)
                {
                    foreach (var item in getSupervisors)
                    {
                        notification.ToUserId = item.UserId;
                        _db.Notifications.Add(notification);
                        _db.SaveChanges();
                    }
                }
                message = Resource.success;
                status = true;
                #endregion
            }
            catch (Exception ex)
            {
                status = false;
                message = Resource.some_error_occured;
            }
            return status;
        }

        public bool MakeJobInProgress(long JobRequestId)
        {
            bool status = false;
            try
            {
                var jobReq = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    var propertyData = _db.Properties.Where(a => a.Id == jobReq.PropertyId).FirstOrDefault();

                    jobReq.JobStatus = Enums.RequestStatus.InProgress.GetHashCode();
                    _db.SaveChanges();

                    var serviceProviderData = _db.Users.Where(a => a.UserId == jobReq.AssignedWorker).FirstOrDefault();

                    string msgText = serviceProviderData.FullName + " has reached your location "
                        + propertyData.Address + " to start the job request.";

                    // Send notification to the client about starting of the job request
                    Notification notification = new Notification()
                    {
                        CreatedDate = DateTime.Now,
                        FromUserId = jobReq.AssignedWorker,
                        ToUserId = propertyData.CreatedBy,
                        IsActive = true,
                        NotificationStatus = (int)Enums.NotificationStatus.TimerStarted,
                        Text = msgText,
                        JobRequestId = jobReq.JobRequestId,
                    };
                    _db.Notifications.Add(notification);

                    _db.SaveChanges();
                    Common.PushNotification(propertyData.User?.UserType, propertyData.User?.DeviceId, propertyData.User?.DeviceToken,
                        serviceProviderData.FullName + " " + msgText);

                    status = true;
                    message = Resource.success;
                }
                else
                {
                    status = false;
                    message = Resource.job_not_exists;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public bool MakeJobInCancel(long JobRequestId)
        {
            bool status = false;
            try
            {
                var jobReq = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    var propertyData = _db.Properties.Where(a => a.Id == jobReq.PropertyId).FirstOrDefault();
                    var serviceProviderData = _db.Users.Where(a => a.UserId == jobReq.AssignedWorker).FirstOrDefault();

                    string msgText = serviceProviderData.FullName + " want the Job Cancel for the property " + propertyData.Name;

                    RejectJobRequest rejectJobRequest = new RejectJobRequest()
                    {
                        JobRequestId = jobReq.JobRequestId,
                        CreatedDate = DateTime.Now,
                        UserId = jobReq.AssignedWorker,
                        Text = msgText,
                        Status = (int)Enums.RejectJobRequest.Pending,
                        Notify = (int)Enums.Notify.False,
                        PropertyName = propertyData.Name,
                    };

                    var getData = _db.RejectJobRequests.Where(x => x.JobRequestId == JobRequestId).ToList();
                    if (getData.Count >= 1)
                    {
                        foreach (var id in getData)
                        {
                            if (id.JobRequestId == JobRequestId)
                            {
                                var jobRequest = _db.RejectJobRequests.Where(x => x.JobRequestId == JobRequestId).FirstOrDefault();
                                if (jobRequest != null)
                                {
                                    jobRequest.Status = (int)Enums.RejectJobRequest.Pending;
                                    jobRequest.Notify = (int)Enums.Notify.False;
                                    jobRequest.CreatedDate = DateTime.Now;
                                    _db.SaveChanges();
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    else
                    {
                        _db.RejectJobRequests.Add(rejectJobRequest);
                    }
                    _db.SaveChanges();

                    status = true;
                    message = Resource.success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }



        public bool GetPrice(long subId, long subSubId)
        {
            bool status = false;
            var subCategory = _db.SubCategories.Where(x => x.Id == subId).FirstOrDefault();
            var subsubCategoty = _db.SubSubCategories.Where(x => x.Id == subSubId).ToList();
            if (subsubCategoty.Count > 1)
            {
                if (subsubCategoty[0].ClientPrice != null)
                {
                    servicePrice = subsubCategoty[0].ClientPrice != null ? Math.Round(Convert.ToDouble(subsubCategoty[0].ClientPrice.ToString())) : 0;
                    status = true;
                }
                else
                {
                    servicePrice = subCategory.ClientPrice != null ? Math.Round(Convert.ToDouble(subCategory.ClientPrice.ToString())) : 0;
                    status = true;
                }
            }
            else
            {
                servicePrice = subCategory.ClientPrice != null ? Math.Round(Convert.ToDouble(subCategory.ClientPrice.ToString())) : 0;
                status = true;
            }
            return status;
        }

        //For Web App
        public ServicePriceViewModel GetPriceService(long subId, long subSubId)
        {
            decimal clientPrice = 0;
            decimal price = 0;
            ServicePriceViewModel priceModel = new ServicePriceViewModel();
            try
            {
                priceModel.ClientPrice = 0;
                priceModel.Price = 0;

                var subCategory = _db.SubCategories.FirstOrDefault(x => x.Id == subId);
                var subsubCategoty = _db.SubSubCategories.FirstOrDefault(x => x.Id == subSubId);
                if (subsubCategoty != null)
                {
                    clientPrice = subsubCategoty.ClientPrice != null ? Math.Round(Convert.ToDecimal(subsubCategoty.ClientPrice.ToString())) : 0;
                    price = subsubCategoty.Price != null ? Math.Round(Convert.ToDecimal(subsubCategoty.Price.ToString())) : 0;
                    priceModel.ClientPrice = clientPrice;
                    priceModel.Price = price;

                }
                else
                {
                    clientPrice = subCategory.ClientPrice != null ? Math.Round(Convert.ToDecimal(subCategory.ClientPrice.ToString())) : 0;
                    price = subCategory.Price != null ? Math.Round(Convert.ToDecimal(subCategory.Price.ToString())) : 0;
                    priceModel.ClientPrice = clientPrice;
                    priceModel.Price = price;
                }
                return priceModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return priceModel;
            }
        }

        public bool MakeCancelRefund(long JobRequestPropId, long workerId, long userID, long propertyId, long serviceId, bool serviceType)
        {
            bool status = false;
            try
            {
                var jobReq = _db.JobRequestSubSubCategories.Where(x => x.JobRequestId == JobRequestPropId).FirstOrDefault();
                if (jobReq != null)
                {
                    var category = _db.Categories.Where(x => x.Id == jobReq.CategoryId).FirstOrDefault();
                    var subCategory = _db.SubCategories.Where(x => x.Id == jobReq.SubCategoryId).FirstOrDefault();
                    var subsubCategoty = _db.SubSubCategories.Where(x => x.Id == jobReq.SubSubCategoryId).ToList();
                    var jobDetails = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == JobRequestPropId).FirstOrDefault(); //Job Date
                    var requestDetails = _db.JobRequests.Where(x => x.Id == jobDetails.JobRequestId).FirstOrDefault();   //Request Create Date
                    var currentDate = DateTime.Now;
                    int? price = 0;

                    var hoursDistance = currentDate - requestDetails.CreatedDate;
                    var hoursDistance_endJob = currentDate - jobDetails.StartDateTime;
                    // subCategory.ClientPrice = 100; 
                    //if (subsubCategoty.Count > 1)
                    //{
                    //    if (subsubCategoty[0].ClientPrice != null)
                    //    {
                    //        price = subsubCategoty[0].ClientPrice != null ? Decimal.ToInt32(subsubCategoty[0].ClientPrice ?? 0) : 0;
                    //    }
                    //    else
                    //    {
                    //        price = subCategory.ClientPrice != null ? Decimal.ToInt32(subCategory.ClientPrice ?? 0) : 0;
                    //    }
                    //}
                    //else
                    //{
                    //    price = subCategory.ClientPrice != null ? Decimal.ToInt32(subCategory.ClientPrice ?? 0) : 0;
                    //}
                    var JobReqId = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == JobRequestPropId).Select(x => x.JobRequestId).FirstOrDefault();
                    var servicePrice = _db.JobRequests.Where(x => x.Id == JobReqId).Select(x => x.ServicePrice).FirstOrDefault();
                    price = servicePrice != null ? Convert.ToInt32(servicePrice) : 0;

                    if (requestDetails.IsPaymentDone == true)
                    {
                        if (hoursDistance.Value.Hours < 24 && hoursDistance.Value.Days == 0)
                        {
                            refundPrice = price * 1.0;
                        }
                        else if (hoursDistance.Value.Days != 0 && hoursDistance_endJob.Value.Days < 0)
                        {
                            refundPrice = price * 0.5;
                        }
                        else
                        {
                            refundPrice = price * 0.0;
                        }
                        //if (currentDate.Day == jobDetails.StartDateTime.Value.Day && !(hoursDistance_endJob.Value.Days > 0))
                        //{
                        //    refundPrice = price * 0.0;
                        //}
                    }
                    if (requestDetails.IsPaymentDone == false)
                    {
                        refundPrice = 1000;
                    }

                    var jobReqprop = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == JobRequestPropId).FirstOrDefault();
                    jobReqprop.JobStatus = Enums.RequestStatus.Canceled.GetHashCode();
                    _db.SaveChanges();



                    SubSubCategory getSubSubCat = null;
                    SubCategory getSubCat = null;
                    if (serviceType == true)
                    {
                        // sub sub category
                        getSubSubCat = _db.SubSubCategories.Where(a => a.Id == serviceId).FirstOrDefault();
                    }
                    else
                    {
                        // sub category
                        getSubCat = _db.SubCategories.Where(a => a.Id == serviceId).FirstOrDefault();
                    }



                    var getWorkerData = _db.Users.Where(a => a.UserId == jobDetails.AssignedWorker).FirstOrDefault();
                    var getPropertyName = _db.Properties.Where(A => A.Id == jobDetails.PropertyId).FirstOrDefault();

                    string serviceName = string.Empty;
                    serviceName = getSubSubCat != null ? getSubSubCat.Name : getSubCat != null ? getSubCat.Name : string.Empty;


                    #region Send Notification to the admin/supervisor
                    try
                    {
                        // Send notification to admin
                        Notification notification = new Notification();
                        notification.CreatedDate = DateTime.Now;
                        notification.FromUserId = requestDetails.UserId;
                        notification.IsActive = true;
                        notification.JobRequestId = JobRequestPropId;
                        notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                        notification.ToUserId = accountService.GetAdminId();
                        notification.Text = "Client cancel the job Request";

                        notification.ServiceName = serviceName;
                        notification.PropertyName = getPropertyName.Name != null ? getPropertyName.Name : "";
                        notification.PropertyAddress = getPropertyName.Address != null ? getPropertyName.Address : "";

                        _db.Notifications.Add(notification);
                        _db.SaveChanges();

                        // send notification to supervisor
                        var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                        var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                        && A.IsActive == true).ToList();

                        if (getSupervisors.Count > 0)
                        {
                            foreach (var supervisor in getSupervisors)
                            {
                                notification.ToUserId = supervisor.UserId;
                                _db.Notifications.Add(notification);

                                _db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    #endregion

                    #region Send Notification to Worker

                    // send notification to the assigned worker
                    Notification _notification = new Notification();
                    _notification.CreatedDate = DateTime.Now;
                    _notification.FromUserId = requestDetails.UserId;
                    _notification.IsActive = true;
                    _notification.JobRequestId = JobRequestPropId;
                    _notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    _notification.ToUserId = jobDetails.AssignedWorker;
                    _notification.Text = "Client cancel the job request";

                    _db.Notifications.Add(_notification);
                    _db.SaveChanges();
                    #endregion

                    status = true;
                    message = Resource.success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public List<RejectJobRequest> GetWorkerCancelRequests(long userId)
        {
            List<RejectJobRequest> lstData = new List<RejectJobRequest>();
            try
            {
                #region Worker Bookings

                var data = _db.RejectJobRequests.Where(x => x.UserId == userId).ToList();
                RejectJobRequest rejectJobRequest = new RejectJobRequest();
                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {

                        if (item.Status == 2 || item.Status == 3)
                        {
                            // sub sub category                           
                            rejectJobRequest.JobRequestId = item.JobRequestId;
                            rejectJobRequest.CreatedDate = item.CreatedDate;
                            rejectJobRequest.UserId = item.UserId;
                            rejectJobRequest.Text = item.Text;
                            rejectJobRequest.Status = item.Status;
                            rejectJobRequest.Notify = item.Notify;
                            rejectJobRequest.PropertyName = item.PropertyName;
                            lstData.Add(rejectJobRequest);
                        }

                    }
                }
                #endregion

                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return lstData;
        }

        public bool MakeCancelRequestAccept(long JobRequestId)
        {
            bool status = false;
            try
            {
                var jobReq = _db.RejectJobRequests.Where(x => x.JobRequestId == JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    jobReq.Status = (int)Enums.RejectJobRequest.Accepted;
                    jobReq.Notify = (int)Enums.Notify.True;
                    _db.SaveChanges();

                    var adminUserType = Enums.UserTypeEnum.Admin.GetHashCode();
                    var getAdmin = _db.Users.Where(a => a.IsActive == true && a.UserType == adminUserType).FirstOrDefault();
                    string msgText = "Your Cancel Request is Accepted by Admin for the property " + jobReq.PropertyName;

                    Notification notification = new Notification()
                    {
                        CreatedDate = DateTime.Now,
                        FromUserId = getAdmin != null ? getAdmin.UserId : 0,
                        ToUserId = jobReq.UserId,
                        IsActive = true,
                        NotificationStatus = (int)Enums.NotificationStatus.Accepted,
                        Text = msgText,
                        JobRequestId = jobReq.JobRequestId,
                    };
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();


                    var jobReqprop = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == JobRequestId).FirstOrDefault();
                    jobReqprop.JobStatus = Enums.RequestStatus.Canceled.GetHashCode();
                    _db.SaveChanges();

                    status = true;
                    message = Resource.success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public bool MakeCancelRequestReject(long JobRequestId)
        {
            bool status = false;
            try
            {
                var jobReq = _db.RejectJobRequests.Where(x => x.JobRequestId == JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    jobReq.Status = (int)Enums.RejectJobRequest.Rejected;
                    jobReq.Notify = (int)Enums.Notify.True;
                    _db.SaveChanges();


                    var adminUserType = Enums.UserTypeEnum.Admin.GetHashCode();
                    var getAdmin = _db.Users.Where(a => a.IsActive == true && a.UserType == adminUserType).FirstOrDefault();
                    string msgText = "Your Cancel Request is Rejected by Admin for the property " + jobReq.PropertyName;

                    Notification notification = new Notification()
                    {
                        CreatedDate = DateTime.Now,
                        FromUserId = getAdmin != null ? getAdmin.UserId : 0,
                        ToUserId = jobReq.UserId,
                        IsActive = true,
                        NotificationStatus = (int)Enums.NotificationStatus.Rejected,
                        Text = msgText,
                        JobRequestId = jobReq.JobRequestId,
                    };
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();


                    status = true;
                    message = Resource.success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public bool MakeChangeRequestNotify(long JobRequestId)
        {
            bool status = false;
            try
            {
                var jobReq = _db.RejectJobRequests.Where(x => x.JobRequestId == JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    jobReq.Notify = (int)Enums.Notify.False;
                    _db.SaveChanges();

                    status = true;
                    message = Resource.success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public DocumentJob GetInvoiceDoc(long orderId)
        {
            try
            {
                var jobReq = _db.DocumentJobs.Where(x => x.JobRequestId == orderId).FirstOrDefault();
                if (jobReq != null)
                {
                    return jobReq;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return null;
        }
        public DocumentJob GetReceiptDoc(long orderId)
        {
            try
            {
                var jobReq = _db.DocumentJobs.Where(x => x.JobRequestId == orderId).FirstOrDefault();
                if (jobReq != null)
                {
                    return jobReq;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return null;
        }


        public bool RemoveinvoiceData(string userId)
        {
            bool status = false;
            long userid = long.Parse(userId);
            var monthInvoiceData = _db.MonthInvoices.Where(x => x.UserID == userId).ToList();
            var monthInvoiceServiceData = _db.MonthInvoiceServices.Where(x => x.UserId == userid).FirstOrDefault();
            if (monthInvoiceData != null)
            {
                foreach (var items in monthInvoiceData)
                {
                    _db.MonthInvoices.Remove(items);
                    _db.SaveChanges();
                }

                status = true;
            }

            if (monthInvoiceServiceData != null)
            {
                _db.MonthInvoiceServices.Remove(monthInvoiceServiceData);
                _db.SaveChanges();
                status = true;
            }


            return status;
        }

        public bool MeetingSchedule(long id)
        {
            bool status = false;
            try
            {
                var bookingData = BookingsDetails(id);

                CheckRequest checkRequest = new CheckRequest()
                {
                    CustomerName = bookingData.CustomerName,
                    PropertyName = bookingData.JobData[0].PropertyName,
                    PropertyAddress = bookingData.JobData[0].PropertyAddress,
                    DateTime = bookingData.JobData[0].JobDateTime
                };

                _db.CheckRequests.Add(checkRequest);

                var getJobData = _db.JobRequests.Where(a => a.Id == id).FirstOrDefault();
                if (getJobData != null)
                {
                    getJobData.RequestSupervisior = true;
                }

                _db.SaveChanges();

                if (checkRequest != null)
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
        public bool MakeCheckListDone(long CheckListId)
        {
            bool status = false;
            try
            {
                var checkList = _db.JobRequestCheckLists.Where(x => x.Id == CheckListId).FirstOrDefault();
                if (checkList != null)
                {
                    checkList.IsDone = true;
                    _db.SaveChanges();


                    status = true;
                    message = Resource.success;
                }
                else
                {
                    status = false;
                    message = Resource.job_not_exists;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public bool WorkerAboutToFinish(long JobRequestId)
        {
            bool status = false;
            try
            {
                var jobReq = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    var propertyData = _db.Properties.Where(a => a.Id == jobReq.PropertyId).FirstOrDefault();

                    var serviceProviderData = _db.Users.Where(a => a.UserId == jobReq.AssignedWorker).FirstOrDefault();

                    string msgText = serviceProviderData.FullName + " is about to finish the job for the location "
                        + propertyData.Address;

                    var adminUserType = Enums.UserTypeEnum.Admin.GetHashCode();
                    var getAdmin = _db.Users.Where(a => a.IsActive == true && a.UserType == adminUserType).FirstOrDefault();

                    // Send notification to the admin that worker is about to finish the job
                    Notification notification = new Notification()
                    {
                        CreatedDate = DateTime.Now,
                        FromUserId = jobReq.AssignedWorker,
                        ToUserId = getAdmin != null ? getAdmin.UserId : 0,
                        IsActive = true,
                        NotificationStatus = (int)Enums.NotificationStatus.TimerStarted,
                        Text = msgText,
                        JobRequestId = jobReq.JobRequestId,
                    };
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();

                    var supervisorUserType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                    var getSupervisors = _db.Users.Where(a => a.IsActive == true && a.UserType == supervisorUserType).ToList();
                    if (getSupervisors.Count > 0)
                    {
                        foreach (var item in getSupervisors)
                        {
                            notification.ToUserId = item.UserId;
                            _db.Notifications.Add(notification);
                            _db.SaveChanges();
                        }
                    }

                    status = true;
                    message = Resource.success;
                }
                else
                {
                    status = false;
                    message = Resource.job_not_exists;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public List<ServiceData> JobServiceDetails(long jobRequestId)
        {
            var getJobServiceData = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == jobRequestId).ToList();
            // getting details of property and services
            List<ServiceData> serviceData = new List<ServiceData>();
            if (getJobServiceData != null)
            {
                if (getJobServiceData.Count > 0)
                {
                    for (int i = 0; i < getJobServiceData.Count; i++)
                    {
                        ServiceData data = new ServiceData();
                        data.Day = getJobServiceData[i].StartDateTime.HasValue ?
                            (int)getJobServiceData[i].StartDateTime.Value.DayOfWeek : 0;
                        data.SelectedTime = getJobServiceData[i].StartDateTime.ToString();
                        if (getJobServiceData[i].type == true)
                        {
                            // it is a sub sub category
                            var subSubCatId = getJobServiceData[i].ServiceId;
                            var getSubSubCategory = _db.SubSubCategories.Where(a => a.Id == subSubCatId).FirstOrDefault();
                            data.SubSubCategoryyId = getSubSubCategory.Id;
                            data.SubCategoryId = getSubSubCategory.SubCatId;
                            data.CategoryId = getSubSubCategory.SubCategory.CatId;
                        }
                        else
                        {
                            // it is a sub category
                            var subCatId = getJobServiceData[i].ServiceId;
                            var getSubCategory = _db.SubCategories.Where(a => a.Id == subCatId).FirstOrDefault();
                            data.SubSubCategoryyId = 0;
                            data.SubCategoryId = getSubCategory.Id;
                            data.CategoryId = getSubCategory.CatId;
                        }
                        data.Id = getJobServiceData[i].JobRequestPropId;
                        serviceData.Add(data);
                    }
                }
            }
            return serviceData;
        }

        /// <summary>
        /// Start End Timer Method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public DateTime? StartEndTimer(UpdateTimerTimeModel model)
        {
            DateTime? TimerDate = null;
            try
            {
                var jobReq = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == model.JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    var propertyData = _db.Properties.Where(a => a.Id == jobReq.PropertyId).FirstOrDefault();

                    if (model.IsStart)
                    {
                        if (jobReq.TimerStartDate == null)
                        {
                            jobReq.TimerStartDate = DateTime.Now;
                            _db.SaveChanges();

                            var serviceProviderData = _db.Users.Where(a => a.UserId == jobReq.AssignedWorker).FirstOrDefault();

                            string msgText = serviceProviderData.FullName + " has started timer for your job request for the property "
                                + propertyData.Name;

                            // Send notification to the client about starting of the job request
                            Notification notification = new Notification()
                            {
                                CreatedDate = DateTime.Now,
                                FromUserId = model.UserId,
                                ToUserId = propertyData.CreatedBy,
                                IsActive = true,
                                NotificationStatus = (int)Enums.NotificationStatus.TimerStarted,
                                Text = msgText,
                                JobRequestId = jobReq.JobRequestId,
                            };
                            _db.Notifications.Add(notification);

                            _db.SaveChanges();
                            Common.PushNotification(propertyData.User?.UserType, propertyData.User?.DeviceId, propertyData.User?.DeviceToken,
                                serviceProviderData.FullName + " " + msgText);

                            TimerDate = jobReq.TimerStartDate;
                            message = Resource.timer_started_success;
                        }
                        else
                        {
                            message = Resource.timer_already_started;
                        }
                    }
                    else
                    {
                        if (jobReq.TimerEndDate == null)
                        {
                            jobReq.TimerEndDate = DateTime.Now;
                            _db.SaveChanges();

                            var serviceProviderData = _db.Users.Where(a => a.UserId == jobReq.AssignedWorker).FirstOrDefault();

                            string msgText = serviceProviderData.FullName + " has ended timer for your job request for the property "
                                + propertyData.Name;

                            // Send notification to the client about the end of the job request
                            Notification notification = new Notification()
                            {
                                CreatedDate = DateTime.Now,
                                FromUserId = model.UserId,
                                ToUserId = propertyData.CreatedBy,
                                IsActive = true,
                                NotificationStatus = (int)Enums.NotificationStatus.TimerEnded,
                                Text = msgText,
                                JobRequestId = jobReq.JobRequestId,
                            };
                            _db.Notifications.Add(notification);


                            _db.SaveChanges();
                            Common.PushNotification(propertyData.User?.UserType, propertyData.User?.DeviceId, propertyData.User?.DeviceToken,
                                 msgText);

                            TimerDate = jobReq.TimerEndDate;
                            message = Resource.timer_ended_success;
                        }
                        else
                        {
                            message = Resource.timer_already_ended;
                        }
                    }
                }
                else
                {
                    message = Resource.job_not_exists;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return TimerDate;
        }


        /// <summary>
        /// Complete Job Request Method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CompleteJobRequest(CompleteJobRequestModel model)
        {
            bool status = false;
            try
            {
                var jobReq = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == model.JobRequestId).FirstOrDefault();
                if (jobReq != null)
                {
                    var propertyData = _db.Properties.Where(a => a.Id == jobReq.PropertyId).FirstOrDefault();

                    jobReq.JobStatus = (int)Enums.RequestStatus.Completed;
                    _db.SaveChanges();

                    var serviceProviderData = _db.Users.Where(a => a.UserId == jobReq.AssignedWorker).FirstOrDefault();

                    string msgText = serviceProviderData.FullName + " has completed your job request for the property "
                        + propertyData.Name;

                    // Send notification to client for let know that the job has been completed
                    Notification notification = new Notification()
                    {
                        CreatedDate = DateTime.Now,
                        FromUserId = model.UserId,
                        ToUserId = propertyData.CreatedBy,
                        IsActive = true,
                        NotificationStatus = (int)Enums.NotificationStatus.Completed,
                        Text = msgText,
                        JobRequestId = jobReq.JobRequestId,
                    };
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();


                    Common.PushNotification(propertyData.User?.UserType, propertyData.User?.DeviceId, propertyData.User?.DeviceToken,
                        serviceProviderData.FullName + " " + msgText);

                    status = true;
                    message = Resource.job_complete_success;

                }
                else
                {
                    message = Resource.job_not_exists;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        #endregion

        #region Bookings By User Id

        public MainUserJobsVM GetCustomerJobRequests(long userId, int UserType)
        {
            MainUserJobsVM returnModel = new MainUserJobsVM();
            try
            {
                if (UserType != 7)
                {
                    #region Grant User Bookings

                    var listOfGrantAccessUser = _db.tblGrantAccesses.Where(a => a.CreatedBy == userId
                   && a.IsAccountAccepted == true).ToList();

                    if (listOfGrantAccessUser.Count > 0)
                    {
                        var lstDataa = new List<JobRequestViewModel>();
                        foreach (var item in listOfGrantAccessUser)
                        {
                            var getAccesedBookings = _db.JobRequests.Where(x => x.UserId == item.UserId)
                            .OrderByDescending(a => a.CreatedDate).ToList();

                            for (int i = 0; i < getAccesedBookings.Count; i++)
                            {
                                var jobId = getAccesedBookings[i].Id;

                                var getJobData = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == jobId).ToList();

                                var statusList = getJobData.GroupBy(a => a.JobStatus).Select(a => a.Key).ToList();

                                int PendingStatus = Enums.RequestStatus.Pending.GetHashCode();
                                int ProgressStatus = Enums.RequestStatus.InProgress.GetHashCode();
                                int CompletedStatus = Enums.RequestStatus.Completed.GetHashCode();
                                int CanceledStatus = Enums.RequestStatus.Canceled.GetHashCode();

                                int? status = 0;
                                if (statusList.Count == 1)
                                {
                                    status = statusList[0];
                                }
                                else
                                {
                                    var checkCompletedStatus = statusList.Any(a => a.Value == CompletedStatus);
                                    if (checkCompletedStatus)
                                        status = CompletedStatus;

                                    var checkProgressStatus = statusList.Any(a => a.Value == ProgressStatus);
                                    if (checkProgressStatus)
                                        status = ProgressStatus;

                                    var checkPendingStatus = statusList.Any(a => a.Value == PendingStatus);
                                    if (checkPendingStatus)
                                        status = PendingStatus;

                                    var checkCancelledStatus = statusList.Any(a => a.Value == CanceledStatus);
                                    if (checkCancelledStatus)
                                        status = CanceledStatus;
                                }

                                var workerJobsList = new List<WorkersJobs>();

                                // setting property and service data
                                var _data = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == jobId).ToList();
                                if (_data != null)
                                {
                                    if (_data.Count > 0)
                                    {
                                        foreach (var item1 in _data)
                                        {
                                            SubSubCategory serviceData = null;
                                            SubCategory subCatData = null;
                                            JobRequestSubSubCategory getServiceData = null;
                                            var getPropertyData = _db.Properties.Where(a => a.Id == item1.PropertyId).FirstOrDefault();
                                            if (item1.type == true)
                                            {
                                                // sub sub category
                                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item1.JobRequestPropId && a.SubSubCategoryId == item1.ServiceId).FirstOrDefault();
                                            }
                                            else
                                            {
                                                // sub category
                                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item1.JobRequestPropId
                                                && a.SubCategoryId == item1.ServiceId).FirstOrDefault();
                                            }

                                            if (getServiceData != null)
                                            {
                                                if (getServiceData.SubSubCategoryId != 0)
                                                {
                                                    serviceData = _db.SubSubCategories.Where(a => a.Id == getServiceData.SubSubCategoryId).FirstOrDefault();
                                                }
                                                if (getServiceData.SubCategoryId != 0)
                                                {
                                                    subCatData = _db.SubCategories.Where(A => A.Id == getServiceData.SubCategoryId).FirstOrDefault();
                                                }
                                            }

                                            var getWorkerData = _db.Users.Where(a => a.UserId == item1.AssignedWorker).FirstOrDefault();
                                            WorkersJobs workersJobs = new WorkersJobs();

                                            if (getPropertyData != null)
                                            {
                                                var propimg = _db.tblPropertyImages.Where(x => x.PropertyId == getPropertyData.Id).FirstOrDefault();
                                                workersJobs.PropertyImage = propimg != null ? propimg.ImageUrl : null;
                                            }
                                            workersJobs.OrderId = item1.JobRequestId;
                                            workersJobs.PropertyName = getPropertyData != null ? getPropertyData.Name : string.Empty;
                                            workersJobs.PropertyAddress = getPropertyData != null ? getPropertyData.Address : string.Empty;
                                            workersJobs.PropertyId = getPropertyData != null ? getPropertyData?.Id : 0;
                                            workersJobs.JobDate = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToString("MM/dd/yyyy") : string.Empty;
                                            workersJobs.JobTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                            workersJobs.JobStatus = item1.JobStatus;
                                            workersJobs.ServiceName = serviceData != null ? serviceData.SubCategory?.Category?.Name
                                                : subCatData != null ? subCatData.Category.Name : string.Empty;
                                            workersJobs.SubServiceName = serviceData != null ? serviceData.SubCategory?.Name
                                                : subCatData != null ? subCatData.Name : string.Empty;
                                            workersJobs.SubSubServiceName = serviceData != null ? serviceData.Name : string.Empty;
                                            workersJobs.TimeToDo = item1.TimeToDo.ToString();
                                            workersJobs.JobDateTime = item1.StartDateTime;
                                            workersJobs.WorkerId = item1.AssignedWorker;
                                            workersJobs.WorkerName = getWorkerData != null ? getWorkerData.FullName : string.Empty;
                                            workersJobs.WorkerImage = getWorkerData != null ? getWorkerData.PicturePath : string.Empty;
                                            workersJobs.FromTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                            var hours = Convert.ToDouble(item1.TimeToDo);
                                            workersJobs.ToTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.AddHours(hours).ToShortTimeString() : string.Empty;
                                            workersJobs.JobPropId = item1.JobRequestPropId;

                                            workerJobsList.Add(workersJobs);
                                        }
                                    }
                                }


                                var model = new JobRequestViewModel()
                                {
                                    JobStatus = status.Value,
                                    FastOrderName = getAccesedBookings[i].FastOrderName,
                                    IsFastOrder = getAccesedBookings[i].IsFastOrder,
                                    JobDesc = getAccesedBookings[i].Description,
                                    UserId = getAccesedBookings[i].UserId,
                                    Id = getAccesedBookings[i].Id,
                                    JobData = workerJobsList,

                                    CheckListData = getAccesedBookings[i].JobRequestCheckLists.Select(p => new JobRequestCheckListModel()
                                    {
                                        Id = p.Id,
                                        TaskDetail = p.TaskDetail
                                    }).ToList(),

                                    ReferenceImages = getAccesedBookings[i].JobRequestRefImages
                                .Select(y => new ChecklistImageVM
                                {
                                    ImageUrl = y.PicturePath,
                                    IsImage = y.IsImage,
                                    IsVideo = y.IsVideo,
                                    VideoUrl = y.VideoUrl

                                }).ToList()
                                };
                                lstDataa.Add(model);
                            }
                            returnModel.GrantUserJobs = lstDataa;
                        }
                    }

                    #endregion

                    #region Sub User Bookings

                    var getMainUserProperties = _db.Properties.Where(a => a.IsActive == true
                      && a.CreatedBy == userId && (a.SubUserId != 0 || a.SubUserId != null)).ToList();

                    if (getMainUserProperties.Count > 0)
                    {
                        var lstDataaa = new List<JobRequestViewModel>();
                        foreach (var item in getMainUserProperties)
                        {
                            var getAccesedBookings = _db.JobRequests.Where(x => x.UserId == item.SubUserId)
                          .OrderByDescending(a => a.CreatedDate).ToList();

                            for (int i = 0; i < getAccesedBookings.Count; i++)
                            {
                                var jobId = getAccesedBookings[i].Id;

                                var getJobData = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == jobId).ToList();

                                var statusList = getJobData.GroupBy(a => a.JobStatus).Select(a => a.Key).ToList();

                                int PendingStatus = Enums.RequestStatus.Pending.GetHashCode();
                                int ProgressStatus = Enums.RequestStatus.InProgress.GetHashCode();
                                int CompletedStatus = Enums.RequestStatus.Completed.GetHashCode();
                                int CanceledStatus = Enums.RequestStatus.Canceled.GetHashCode();

                                int? status = 0;
                                if (statusList.Count == 1)
                                {
                                    status = statusList[0];
                                }
                                else
                                {
                                    var checkCompletedStatus = statusList.Any(a => a.Value == CompletedStatus);
                                    if (checkCompletedStatus)
                                        status = CompletedStatus;

                                    var checkProgressStatus = statusList.Any(a => a.Value == ProgressStatus);
                                    if (checkProgressStatus)
                                        status = ProgressStatus;

                                    var checkPendingStatus = statusList.Any(a => a.Value == PendingStatus);
                                    if (checkPendingStatus)
                                        status = PendingStatus;

                                    var checkCancelledStatus = statusList.Any(a => a.Value == CanceledStatus);
                                    if (checkCancelledStatus)
                                        status = CanceledStatus;
                                }

                                var workerJobsList = new List<WorkersJobs>();

                                // setting property and service data
                                var _data = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == jobId).ToList();
                                if (_data != null)
                                {
                                    if (_data.Count > 0)
                                    {
                                        foreach (var item1 in _data)
                                        {
                                            SubSubCategory serviceData = null;
                                            SubCategory subCatData = null;
                                            JobRequestSubSubCategory getServiceData = null;
                                            var getPropertyData = _db.Properties.Where(a => a.Id == item1.PropertyId).FirstOrDefault();
                                            if (item1.type == true)
                                            {
                                                // sub sub category
                                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item1.JobRequestPropId && a.SubSubCategoryId == item1.ServiceId).FirstOrDefault();
                                            }
                                            else
                                            {
                                                // sub category
                                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item1.JobRequestPropId
                                                && a.SubCategoryId == item1.ServiceId).FirstOrDefault();
                                            }

                                            if (getServiceData != null)
                                            {
                                                if (getServiceData.SubSubCategoryId != 0)
                                                {
                                                    serviceData = _db.SubSubCategories.Where(a => a.Id == getServiceData.SubSubCategoryId).FirstOrDefault();
                                                }
                                                if (getServiceData.SubCategoryId != 0)
                                                {
                                                    subCatData = _db.SubCategories.Where(A => A.Id == getServiceData.SubCategoryId).FirstOrDefault();
                                                }
                                            }

                                            var getWorkerData = _db.Users.Where(a => a.UserId == item1.AssignedWorker).FirstOrDefault();
                                            WorkersJobs workersJobs = new WorkersJobs();
                                            if (getPropertyData != null)
                                            {
                                                var propimg = _db.tblPropertyImages.Where(x => x.PropertyId == getPropertyData.Id).FirstOrDefault();
                                                workersJobs.PropertyImage = propimg != null ? propimg.ImageUrl : null;
                                            }
                                            workersJobs.OrderId = item1.JobRequestId;
                                            workersJobs.PropertyName = getPropertyData != null ? getPropertyData.Name : string.Empty;
                                            workersJobs.PropertyAddress = getPropertyData != null ? getPropertyData.Address : string.Empty;
                                            workersJobs.PropertyId = getPropertyData != null ? getPropertyData?.Id : 0;
                                            workersJobs.JobDate = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToString("MM/dd/yyyy") : string.Empty;
                                            workersJobs.JobTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                            workersJobs.JobStatus = item1.JobStatus;
                                            workersJobs.ServiceName = serviceData != null ? serviceData.SubCategory?.Category?.Name
                                                : subCatData != null ? subCatData.Category.Name : string.Empty;
                                            workersJobs.SubServiceName = serviceData != null ? serviceData.SubCategory?.Name
                                                : subCatData != null ? subCatData.Name : string.Empty;
                                            workersJobs.SubSubServiceName = serviceData != null ? serviceData.Name : string.Empty;
                                            workersJobs.TimeToDo = item1.TimeToDo.ToString();
                                            workersJobs.JobDateTime = item1.StartDateTime;
                                            workersJobs.WorkerId = item1.AssignedWorker;
                                            workersJobs.WorkerName = getWorkerData != null ? getWorkerData.FullName : string.Empty;
                                            workersJobs.WorkerImage = getWorkerData != null ? getWorkerData.PicturePath : string.Empty;
                                            workersJobs.FromTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                            var hours = Convert.ToDouble(item1.TimeToDo);
                                            workersJobs.ToTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.AddHours(hours).ToShortTimeString() : string.Empty;
                                            workersJobs.JobPropId = item1.JobRequestPropId;
                                            workerJobsList.Add(workersJobs);
                                        }
                                    }
                                }

                                var model = new JobRequestViewModel()
                                {
                                    JobStatus = status.Value,
                                    FastOrderName = getAccesedBookings[i].FastOrderName,
                                    IsFastOrder = getAccesedBookings[i].IsFastOrder,
                                    JobDesc = getAccesedBookings[i].Description,
                                    UserId = getAccesedBookings[i].UserId,
                                    Id = getAccesedBookings[i].Id,
                                    JobData = workerJobsList,

                                    CheckListData = getAccesedBookings[i].JobRequestCheckLists.Select(p => new JobRequestCheckListModel()
                                    {
                                        Id = p.Id,
                                        TaskDetail = p.TaskDetail
                                    }).ToList(),

                                    ReferenceImages = getAccesedBookings[i].JobRequestRefImages
                                .Select(y => new ChecklistImageVM
                                {
                                    ImageUrl = y.PicturePath,
                                    IsImage = y.IsImage,
                                    IsVideo = y.IsVideo,
                                    VideoUrl = y.VideoUrl

                                }).ToList()
                                };
                                lstDataaa.Add(model);
                            }
                            returnModel.SubUserJobs = lstDataaa;
                        }
                    }

                    #endregion

                    #region Main User Bookings

                    var data = _db.JobRequests.Where(x => x.UserId == userId).ToList();

                    var lstData = new List<JobRequestViewModel>();

                    if (data.Count > 0)
                    {
                        for (int i = 0; i < data.Count; i++)
                        {
                            var jobId = data[i].Id;
                            if (jobId == 130536)
                            {

                            }

                            var getJobData = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == jobId
                            && a.StartDateTime != null).ToList();

                            var statusList = getJobData.GroupBy(a => a.JobStatus).Select(a => a.Key).ToList();

                            int PendingStatus = Enums.RequestStatus.Pending.GetHashCode();
                            int ProgressStatus = Enums.RequestStatus.InProgress.GetHashCode();
                            int CompletedStatus = Enums.RequestStatus.Completed.GetHashCode();
                            int CanceledStatus = Enums.RequestStatus.Canceled.GetHashCode();

                            int? status = 0;
                            if (statusList.Count == 1)
                            {
                                status = statusList[0];
                            }
                            else
                            {
                                var checkCompletedStatus = statusList.Any(a => a.Value == CompletedStatus);
                                if (checkCompletedStatus)
                                    status = CompletedStatus;

                                var checkProgressStatus = statusList.Any(a => a.Value == ProgressStatus);
                                if (checkProgressStatus)
                                    status = ProgressStatus;

                                var checkPendingStatus = statusList.Any(a => a.Value == PendingStatus);
                                if (checkPendingStatus)
                                    status = PendingStatus;

                                var checkCancelledStatus = statusList.Any(a => a.Value == CanceledStatus);
                                if (checkCancelledStatus)
                                    status = CanceledStatus;
                            }

                            var workerJobsList = new List<WorkersJobs>();

                            // setting property and service data
                            var _data = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == jobId
                            && x.StartDateTime != null).ToList();
                            if (_data != null)
                            {
                                if (_data.Count > 0)
                                {
                                    foreach (var item in _data)
                                    {
                                        SubSubCategory serviceData = null;
                                        SubCategory subCatData = null;
                                        JobRequestSubSubCategory getServiceData = null;
                                        var getPropertyData = _db.Properties.Where(a => a.Id == item.PropertyId).FirstOrDefault();
                                        if (item.type == true)
                                        {
                                            // sub sub category
                                            getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item.JobRequestPropId && a.SubSubCategoryId == item.ServiceId).FirstOrDefault();
                                        }
                                        else
                                        {
                                            // sub category
                                            getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item.JobRequestPropId
                                            && a.SubCategoryId == item.ServiceId).FirstOrDefault();
                                        }

                                        if (getServiceData != null)
                                        {
                                            if (getServiceData.SubSubCategoryId != 0)
                                            {
                                                serviceData = _db.SubSubCategories.Where(a => a.Id == getServiceData.SubSubCategoryId).FirstOrDefault();
                                            }
                                            if (getServiceData.SubCategoryId != 0)
                                            {
                                                subCatData = _db.SubCategories.Where(A => A.Id == getServiceData.SubCategoryId).FirstOrDefault();
                                            }
                                        }

                                        var getWorkerData = _db.Users.Where(a => a.UserId == item.AssignedWorker).FirstOrDefault();
                                        WorkersJobs workersJobs = new WorkersJobs();

                                        //changes code
                                        if (getPropertyData != null)
                                        {
                                            var propimg = _db.tblPropertyImages.Where(x => x.PropertyId == getPropertyData.Id).FirstOrDefault();
                                            workersJobs.PropertyImage = propimg != null ? propimg.ImageUrl : null;
                                        }
                                        workersJobs.OrderId = item.JobRequestId;
                                        workersJobs.PropertyName = getPropertyData != null ? getPropertyData.Name : string.Empty;
                                        workersJobs.PropertyAddress = getPropertyData != null ? getPropertyData.Address : string.Empty;
                                        workersJobs.PropertyId = getPropertyData != null ? getPropertyData?.Id : 0;
                                        workersJobs.JobDate = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToString("MM/dd/yyyy") : string.Empty;
                                        workersJobs.JobTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                        workersJobs.JobStatus = item.JobStatus;
                                        workersJobs.ServiceName = serviceData != null ? serviceData.SubCategory?.Category?.Name
                                            : subCatData != null ? subCatData.Category.Name : string.Empty;
                                        workersJobs.SubServiceName = serviceData != null ? serviceData.SubCategory?.Name
                                            : subCatData != null ? subCatData.Name : string.Empty;
                                        workersJobs.SubSubServiceName = serviceData != null ? serviceData.Name : string.Empty;
                                        workersJobs.TimeToDo = item.TimeToDo.ToString();
                                        workersJobs.JobDateTime = item.StartDateTime;
                                        workersJobs.WorkerId = item.AssignedWorker;
                                        workersJobs.WorkerName = getWorkerData != null ? getWorkerData.FullName : string.Empty;
                                        workersJobs.WorkerImage = getWorkerData != null ? getWorkerData.PicturePath : string.Empty;
                                        workersJobs.FromTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                        var hours = Convert.ToDouble(item.TimeToDo);
                                        workersJobs.ToTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.AddHours(hours).ToShortTimeString() : string.Empty;
                                        workersJobs.JobPropId = item.JobRequestPropId;
                                        workerJobsList.Add(workersJobs);
                                    }
                                }
                            }

                            var model = new JobRequestViewModel()
                            {
                                JobStatus = status.Value,
                                FastOrderName = data[i].FastOrderName,
                                IsFastOrder = data[i].IsFastOrder,
                                JobDesc = data[i].Description,
                                UserId = data[i].UserId,
                                Id = data[i].Id,
                                JobData = workerJobsList,

                                CheckListData = data[i].JobRequestCheckLists.Select(p => new JobRequestCheckListModel()
                                {
                                    Id = p.Id,
                                    TaskDetail = p.TaskDetail
                                }).ToList(),

                                ReferenceImages = data[i].JobRequestRefImages
                            .Select(y => new ChecklistImageVM
                            {
                                ImageUrl = y.PicturePath,
                                IsImage = y.IsImage,
                                IsVideo = y.IsVideo,
                                VideoUrl = y.VideoUrl

                            }).ToList()
                            };
                            if (_data != null)
                            {
                                if (_data.Count > 0)
                                {
                                    lstData.Add(model);
                                }
                            }
                        }
                        returnModel.MainUserJobs = lstData;
                    }

                    #endregion
                }
                else
                {
                    #region Sub User Bookings

                    var getMainUserProperties = _db.Properties.Where(a => a.IsActive == true && a.SubUserId == userId).ToList();

                    if (getMainUserProperties.Count > 0)
                    {
                        var lstDataaa = new List<JobRequestViewModel>();
                        foreach (var item in getMainUserProperties)
                        {
                            var getAccesedBookings = new List<JobRequest>();
                            var dd = _db.JobRequestPropertyServices.Where(a => a.PropertyId == item.Id && a.IsVisitor == true).ToList();
                            if (dd != null)
                            {
                                foreach (var tt in dd)
                                {
                                    var data1 = _db.JobRequests.Where(x => x.Id == tt.JobRequestId).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
                                    if (data1 != null)
                                    {
                                        getAccesedBookings.Add(data1);
                                    }
                                }
                            }

                            for (int i = 0; i < getAccesedBookings.Count; i++)
                            {
                                var jobId = getAccesedBookings[i].Id;
                                var getJobData = _db.JobRequestPropertyServices.Where(a => a.PropertyId == item.Id && a.IsVisitor == true).ToList();

                                var statusList = getJobData.GroupBy(a => a.JobStatus).Select(a => a.Key).ToList();

                                int PendingStatus = Enums.RequestStatus.Pending.GetHashCode();
                                int ProgressStatus = Enums.RequestStatus.InProgress.GetHashCode();
                                int CompletedStatus = Enums.RequestStatus.Completed.GetHashCode();
                                int CanceledStatus = Enums.RequestStatus.Canceled.GetHashCode();

                                int? status = 0;
                                if (statusList.Count == 1)
                                {
                                    status = statusList[0];
                                }
                                else
                                {
                                    var checkCompletedStatus = statusList.Any(a => a.Value == CompletedStatus);
                                    if (checkCompletedStatus)
                                        status = CompletedStatus;

                                    var checkProgressStatus = statusList.Any(a => a.Value == ProgressStatus);
                                    if (checkProgressStatus)
                                        status = ProgressStatus;

                                    var checkPendingStatus = statusList.Any(a => a.Value == PendingStatus);
                                    if (checkPendingStatus)
                                        status = PendingStatus;

                                    var checkCancelledStatus = statusList.Any(a => a.Value == CanceledStatus);
                                    if (checkCancelledStatus)
                                        status = CanceledStatus;
                                }

                                var workerJobsList = new List<WorkersJobs>();

                                // setting property and service data
                                var _data = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == jobId).ToList();
                                if (_data != null)
                                {
                                    if (_data.Count > 0)
                                    {
                                        foreach (var item1 in _data)
                                        {
                                            SubSubCategory serviceData = null;
                                            SubCategory subCatData = null;
                                            JobRequestSubSubCategory getServiceData = null;
                                            var getPropertyData = _db.Properties.Where(a => a.Id == item1.PropertyId).FirstOrDefault();
                                            if (item1.type == true)
                                            {
                                                // sub sub category
                                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item1.JobRequestPropId && a.SubSubCategoryId == item1.ServiceId).FirstOrDefault();
                                            }
                                            else
                                            {
                                                // sub category
                                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item1.JobRequestPropId
                                                && a.SubCategoryId == item1.ServiceId).FirstOrDefault();
                                            }

                                            if (getServiceData != null)
                                            {
                                                if (getServiceData.SubSubCategoryId != 0)
                                                {
                                                    serviceData = _db.SubSubCategories.Where(a => a.Id == getServiceData.SubSubCategoryId).FirstOrDefault();
                                                }
                                                if (getServiceData.SubCategoryId != 0)
                                                {
                                                    subCatData = _db.SubCategories.Where(A => A.Id == getServiceData.SubCategoryId).FirstOrDefault();
                                                }
                                            }

                                            var getWorkerData = _db.Users.Where(a => a.UserId == item1.AssignedWorker).FirstOrDefault();
                                            WorkersJobs workersJobs = new WorkersJobs();

                                            if (getPropertyData != null)
                                            {
                                                var propimg = _db.tblPropertyImages.Where(x => x.PropertyId == getPropertyData.Id).FirstOrDefault();
                                                workersJobs.PropertyImage = propimg != null ? propimg.ImageUrl : null;
                                            }
                                            workersJobs.OrderId = item1.JobRequestId;
                                            workersJobs.PropertyName = getPropertyData != null ? getPropertyData.Name : string.Empty;
                                            workersJobs.PropertyAddress = getPropertyData != null ? getPropertyData.Address : string.Empty;
                                            workersJobs.PropertyId = getPropertyData != null ? getPropertyData?.Id : 0;
                                            workersJobs.JobDate = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToString("MM/dd/yyyy") : string.Empty;
                                            workersJobs.JobTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                            workersJobs.JobStatus = item1.JobStatus;
                                            workersJobs.ServiceName = serviceData != null ? serviceData.SubCategory?.Category?.Name
                                                : subCatData != null ? subCatData.Category.Name : string.Empty;
                                            workersJobs.SubServiceName = serviceData != null ? serviceData.SubCategory?.Name
                                                : subCatData != null ? subCatData.Name : string.Empty;
                                            workersJobs.SubSubServiceName = serviceData != null ? serviceData.Name : string.Empty;
                                            workersJobs.TimeToDo = item1.TimeToDo.ToString();
                                            workersJobs.JobDateTime = item1.StartDateTime;
                                            workersJobs.WorkerId = item1.AssignedWorker;
                                            workersJobs.WorkerName = getWorkerData != null ? getWorkerData.FullName : string.Empty;
                                            workersJobs.WorkerImage = getWorkerData != null ? getWorkerData.PicturePath : string.Empty;
                                            workersJobs.FromTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.ToShortTimeString() : string.Empty;
                                            var hours = Convert.ToDouble(item1.TimeToDo);
                                            workersJobs.ToTime = item1.StartDateTime.HasValue ? item1.StartDateTime.Value.AddHours(hours).ToShortTimeString() : string.Empty;
                                            workersJobs.JobPropId = item1.JobRequestPropId;
                                            workerJobsList.Add(workersJobs);
                                        }
                                    }
                                }

                                var model = new JobRequestViewModel()
                                {
                                    JobStatus = status.Value,
                                    FastOrderName = getAccesedBookings[i].FastOrderName,
                                    IsFastOrder = getAccesedBookings[i].IsFastOrder,
                                    JobDesc = getAccesedBookings[i].Description,
                                    UserId = getAccesedBookings[i].UserId,
                                    Id = getAccesedBookings[i].Id,
                                    JobData = workerJobsList,

                                    CheckListData = getAccesedBookings[i].JobRequestCheckLists.Select(p => new JobRequestCheckListModel()
                                    {
                                        Id = p.Id,
                                        TaskDetail = p.TaskDetail
                                    }).ToList(),

                                    ReferenceImages = getAccesedBookings[i].JobRequestRefImages
                                .Select(y => new ChecklistImageVM
                                {
                                    ImageUrl = y.PicturePath,
                                    IsImage = y.IsImage,
                                    IsVideo = y.IsVideo,
                                    VideoUrl = y.VideoUrl

                                }).ToList()
                                };
                                lstDataaa.Add(model);
                            }
                            returnModel.SubUserJobs = lstDataaa;
                        }
                    }
                    #endregion
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return returnModel;
        }

        public List<WorkersJobs> GetWorkerJobRequests(long userId)
        {
            List<WorkersJobs> lstData = new List<WorkersJobs>();
            try
            {
                int pendingStatus = Enums.RequestStatus.Pending.GetHashCode();
                int progressStatus = Enums.RequestStatus.InProgress.GetHashCode();
                int canceledStatus = Enums.RequestStatus.Canceled.GetHashCode();
                int completedStatus = Enums.RequestStatus.Completed.GetHashCode();
                #region Worker Bookings

                var data = _db.JobRequestPropertyServices
                    .Where(x => x.AssignedWorker == userId &&
                    (x.JobStatus == pendingStatus ||
                    x.JobStatus == progressStatus ||
                    x.JobStatus == canceledStatus ||
                    x.JobStatus == completedStatus))
                    .ToList();

                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        SubSubCategory serviceData = null;
                        SubCategory subCatData = null;
                        JobRequestSubSubCategory getServiceData = null;
                        var getPropertyData = _db.Properties.Where(a => a.Id == item.PropertyId).FirstOrDefault();
                        if (item.type == true)
                        {
                            // sub sub category
                            getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item.JobRequestPropId && a.SubSubCategoryId == item.ServiceId).FirstOrDefault();
                        }
                        else
                        {
                            // sub category
                            getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item.JobRequestPropId
                            && a.SubCategoryId == item.ServiceId).FirstOrDefault();
                        }

                        if (getServiceData != null)
                        {
                            if (getServiceData.SubSubCategoryId != 0)
                            {
                                serviceData = _db.SubSubCategories.Where(a => a.Id == getServiceData.SubSubCategoryId).FirstOrDefault();
                            }
                            if (getServiceData.SubCategoryId != 0)
                            {
                                subCatData = _db.SubCategories.Where(A => A.Id == getServiceData.SubCategoryId).FirstOrDefault();
                            }
                        }

                        var getWorkerData = _db.Users.Where(a => a.UserId == item.AssignedWorker).FirstOrDefault();
                        var getCustomerData = _db.Users.Where(a => a.UserId == getPropertyData.CreatedBy).FirstOrDefault();

                        WorkersJobs workersJobs = new WorkersJobs();
                        workersJobs.OrderId = item.JobRequestId;
                        workersJobs.JobPropId = item.JobRequestPropId;
                        workersJobs.PropertyName = getPropertyData != null ? getPropertyData.Name : string.Empty;
                        workersJobs.PropertyAddress = getPropertyData != null ? getPropertyData.Address : string.Empty;
                        workersJobs.PropertyId = getPropertyData != null ? getPropertyData?.Id : 0;
                        workersJobs.JobDate = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToString("MM/dd/yyyy") : string.Empty;
                        workersJobs.JobTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToShortTimeString() : string.Empty;
                        workersJobs.JobStatus = item.JobStatus;
                        workersJobs.ServiceName = serviceData != null ? serviceData.SubCategory?.Category?.Name
                            : subCatData != null ? subCatData.Category.Name : string.Empty;
                        workersJobs.SubServiceName = serviceData != null ? serviceData.SubCategory?.Name
                            : subCatData != null ? subCatData.Name : string.Empty;
                        workersJobs.SubSubServiceName = serviceData != null ? serviceData.Name : string.Empty;
                        workersJobs.CategoryId = serviceData != null ? serviceData.SubCategory?.Category?.Id
                            : subCatData != null ? subCatData.Category.Id : 0;
                        workersJobs.SubCategoryId = serviceData != null ? serviceData.SubCategory?.Id
                            : subCatData != null ? subCatData.Id : 0;
                        workersJobs.SubSubCategoryId = serviceData != null ? serviceData.Id : 0;
                        workersJobs.TimeToDo = item.TimeToDo.ToString();
                        workersJobs.JobDateTime = item.StartDateTime;
                        workersJobs.WorkerId = item.AssignedWorker;
                        workersJobs.WorkerName = getWorkerData != null ? getWorkerData.FullName : string.Empty;
                        workersJobs.WorkerImage = getWorkerData != null ? getWorkerData.PicturePath : string.Empty;
                        workersJobs.CustomerId = getCustomerData.UserId;
                        workersJobs.CustomerName = getCustomerData != null ?
                           !string.IsNullOrEmpty(getCustomerData.FullName) ? getCustomerData.FullName : getCustomerData.Email :
                            string.Empty;
                        workersJobs.CustomerImage = getCustomerData != null ? getCustomerData.PicturePath : string.Empty;
                        workersJobs.FromTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToShortTimeString() : string.Empty;
                        var hours = Convert.ToDouble(item.TimeToDo);
                        workersJobs.ToTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.AddHours(hours).ToShortTimeString() : string.Empty;
                        lstData.Add(workersJobs);
                    }
                }

                #endregion

                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return lstData;
        }
        public JobRequestViewModel BookingsDetails(long requestId)
        {
            JobRequestViewModel lstData = null;
            try
            {
                // Getting Job Data
                User getUserData = null;

                var getJobData = _db.JobRequests.Where(a => a.Id == requestId).FirstOrDefault();
                if (getJobData != null)
                {
                    getUserData = _db.Users.Where(a => a.UserId == getJobData.UserId).FirstOrDefault();
                }
                lstData = new JobRequestViewModel();
                lstData.Id = requestId;
                lstData.FastOrderName = getJobData != null ? getJobData.FastOrderName : string.Empty;
                lstData.IsFastOrder = getJobData != null ? getJobData.IsFastOrder : false;
                lstData.IsAddedBySupervisor = getJobData != null ? getJobData.IsAddedBySupervisor : false;
                lstData.CustomerName = getUserData != null ? getUserData.FullName : string.Empty;
                lstData.CustomerId = getUserData != null ? getUserData.UserId : 0;
                lstData.CustomerImage = getUserData != null ? getUserData.PicturePath : string.Empty;
                lstData.JobDesc = getJobData != null ? getJobData.Description : string.Empty;
                lstData.RequestSupervisior = getJobData != null ? getJobData.RequestSupervisior != null ? getJobData.RequestSupervisior : false : false;
                // setting property and service data
                var data = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == requestId).ToList();
                if (data != null)
                {
                    if (data.Count > 0)
                    {
                        lstData.JobData = new List<WorkersJobs>();
                        foreach (var item in data)
                        {
                            SubSubCategory serviceData = null;
                            SubCategory subCatData = null;
                            JobRequestSubSubCategory getServiceData = null;
                            var getPropertyData = _db.Properties.Where(a => a.Id == item.PropertyId).FirstOrDefault();
                            if (item.type == true)
                            {
                                // sub sub category
                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item.JobRequestPropId && a.SubSubCategoryId == item.ServiceId).FirstOrDefault();
                            }
                            else
                            {
                                // sub category
                                getServiceData = _db.JobRequestSubSubCategories.Where(a => a.JobRequestId == item.JobRequestPropId
                                && a.SubCategoryId == item.ServiceId).FirstOrDefault();
                            }

                            if (getServiceData != null)
                            {
                                if (getServiceData.SubSubCategoryId != 0)
                                {
                                    serviceData = _db.SubSubCategories.Where(a => a.Id == getServiceData.SubSubCategoryId).FirstOrDefault();
                                }
                                if (getServiceData.SubCategoryId != 0)
                                {
                                    subCatData = _db.SubCategories.Where(A => A.Id == getServiceData.SubCategoryId).FirstOrDefault();
                                }
                            }

                            var getWorkerData = _db.Users.Where(a => a.UserId == item.AssignedWorker).FirstOrDefault();
                            WorkersJobs workersJobs = new WorkersJobs();
                            workersJobs.OrderId = item.JobRequestId;
                            workersJobs.PropertyName = getPropertyData != null ? getPropertyData.Name : string.Empty;
                            workersJobs.PropertyAddress = getPropertyData != null ? getPropertyData.Address : string.Empty;
                            workersJobs.PropertyId = getPropertyData != null ? getPropertyData?.Id : 0;
                            workersJobs.JobDate = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToString("MM/dd/yyyy") : string.Empty;
                            workersJobs.JobTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToShortTimeString() : string.Empty;
                            workersJobs.JobStatus = item.JobStatus;
                            workersJobs.CategoryId = serviceData != null ? serviceData.SubCategory?.Category?.Id : subCatData != null ? subCatData.Category.Id : 0;
                            workersJobs.ServiceName = serviceData != null ? serviceData.SubCategory?.Category?.Name
                                : subCatData != null ? subCatData.Category.Name : string.Empty;
                            workersJobs.SubServiceName = serviceData != null ? serviceData.SubCategory?.Name
                                : subCatData != null ? subCatData.Name : string.Empty;
                            workersJobs.SubSubServiceName = serviceData != null ? serviceData.Name : string.Empty;
                            workersJobs.SubCategoryId = serviceData != null ? serviceData.SubCategory?.Id
                                : subCatData != null ? subCatData.Id : 0;
                            workersJobs.SubSubCategoryId = serviceData != null ? serviceData.Id : 0;
                            workersJobs.TimeToDo = item.TimeToDo.ToString();
                            workersJobs.JobDateTime = item.StartDateTime;
                            workersJobs.WorkerId = item.AssignedWorker;
                            workersJobs.WorkerName = getWorkerData != null ? getWorkerData.FullName : string.Empty;
                            workersJobs.WorkerImage = getWorkerData != null ? getWorkerData.PicturePath : string.Empty;
                            workersJobs.FromTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.ToShortTimeString() : string.Empty;
                            var hours = Convert.ToDouble(item.TimeToDo);
                            workersJobs.ToTime = item.StartDateTime.HasValue ? item.StartDateTime.Value.AddHours(hours).ToShortTimeString() : string.Empty;
                            workersJobs.IsStarted = item.TimerStartDate.HasValue ? true : false;
                            workersJobs.TimerStartedTime = item.TimerStartDate;
                            workersJobs.TimerEndTime = item.TimerEndDate;
                            workersJobs.JobPropId = item.JobRequestPropId;

                            //Getting inventory details
                            var getInventoryDetails = _db.JobInventories.Where(a => a.JobRequestId == item.JobRequestPropId).ToList();
                            if (getInventoryDetails.Count > 0)
                            {
                                workersJobs.InventoryDetails = new List<InventoryItems>();
                                foreach (var item1 in getInventoryDetails)
                                {
                                    var getPropertyDetails = _db.Properties.Where(a => a.Id == item1.PropertyId).FirstOrDefault();
                                    InventoryItems inventoryItems = new InventoryItems();
                                    inventoryItems.PropertyName = getPropertyDetails != null ? getPropertyDetails.Name : string.Empty;
                                    inventoryItems.InventoryName = item1.Inventory?.Name;
                                    inventoryItems.Qty = item1.Qty;
                                    workersJobs.InventoryDetails.Add(inventoryItems);
                                }
                            }
                            lstData.JobData.Add(workersJobs);
                        }
                    }
                }

                //Getting checklist details
                var getCheckLists = _db.JobRequestCheckLists.Where(a => a.JobRequestId == requestId).ToList();
                if (getCheckLists.Count > 0)
                {
                    lstData.CheckListDetails = new List<JobRequestCheckListModel>();
                    foreach (var item1 in getCheckLists)
                    {
                        JobRequestCheckListModel checkListModel = new JobRequestCheckListModel();
                        checkListModel.TaskDetail = item1.TaskDetail;
                        checkListModel.Id = item1.Id;
                        checkListModel.IsDone = item1.IsDone;
                        lstData.CheckListDetails.Add(checkListModel);
                    }
                }

                //Getting reference image details
                var getRefrenceImages = _db.JobRequestRefImages.Where(a => a.JobRequestId == requestId).ToList();
                if (getRefrenceImages.Count > 0)
                {
                    lstData.ReferenceImages = new List<ChecklistImageVM>();
                    foreach (var item1 in getRefrenceImages)
                    {
                        ChecklistImageVM checkListModel = new ChecklistImageVM();
                        checkListModel.IsImage = item1.IsImage;
                        checkListModel.IsVideo = item1.IsVideo;
                        checkListModel.ImageUrl = item1.PicturePath;
                        checkListModel.VideoUrl = item1.VideoUrl;
                        lstData.ReferenceImages.Add(checkListModel);
                    }
                }

                message = Resource.success;
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }

        #endregion

        #region Fast Orders

        public List<JobRequestViewModel> GetFastOrdersByPropertyId(long property_id, long user_id)
        {
            List<JobRequestViewModel> lstData = new List<JobRequestViewModel>();
            try
            {
                var data = (from jr in _db.JobRequests
                            join property in _db.JobRequestPropertyServices on jr.Id equals property.JobRequestId
                            where property.PropertyId == property_id
                            && jr.IsFastOrder == true
                            && jr.UserId == user_id
                            select jr).ToList();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        var model = new JobRequestViewModel()
                        {
                            FastOrderName = data[i].FastOrderName,
                            IsFastOrder = data[i].IsFastOrder,
                            JobDesc = data[i].Description,
                            UserId = data[i].UserId,
                            Id = data[i].Id,

                            CheckListData = data[i].JobRequestCheckLists.Select(p => new JobRequestCheckListModel()
                            {
                                Id = p.Id,
                                TaskDetail = p.TaskDetail
                            }).ToList(),

                            ReferenceImages = data[i].JobRequestRefImages
                        .Select(y => new ChecklistImageVM
                        {
                            ImageUrl = y.PicturePath,
                            IsImage = y.IsImage,
                            IsVideo = y.IsVideo,
                            VideoUrl = y.VideoUrl

                        }).ToList()
                        };
                        lstData.Add(model);
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }

        public List<JobRequestViewModel> GetFastOrdersForUsers(long user_id)
        {
            List<JobRequestViewModel> lstData = new List<JobRequestViewModel>();
            //List<JobRequest> grantAccesedBookings = new List<JobRequest>();

            try
            {
                var data = _db.JobRequests.Where(x => x.UserId == user_id && x.IsFastOrder == true).DistinctBy(x => x.FastOrderName).ToList();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        var model = new JobRequestViewModel()
                        {
                            FastOrderName = data[i].FastOrderName,
                            IsFastOrder = data[i].IsFastOrder,
                            JobDesc = data[i].Description,
                            UserId = data[i].UserId,
                            Id = data[i].Id,
                            PropertyName = GetPropertyName(data[i].Id),
                            CheckListData = data[i].JobRequestCheckLists.Select(p => new JobRequestCheckListModel()
                            {
                                Id = p.Id,
                                TaskDetail = p.TaskDetail
                            }).ToList(),

                            ReferenceImages = data[i].JobRequestRefImages
                        .Select(y => new ChecklistImageVM
                        {
                            ImageUrl = y.PicturePath,
                            IsImage = y.IsImage,
                            IsVideo = y.IsVideo,
                            VideoUrl = y.VideoUrl

                        }).ToList()
                        };
                        lstData.Add(model);
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }

        private string GetPropertyName(long id)
        {
            var jobPropertyService = _db.JobRequestPropertyServices.FirstOrDefault(x => x.JobRequestId == id);

            if (jobPropertyService == null) return "";

            var property = _db.Properties.FirstOrDefault(x => x.Id == jobPropertyService.PropertyId);

            if (property == null) return "";

            return property.Name;
        }
        #endregion

        #region Accept/Reject Service sent by Supervisor

        public JobRequestViewModel AcceptRejectSupervisorOffer(long NotificationId, bool Status)
        {
            JobRequestViewModel jobRequest = new JobRequestViewModel();
            // Offer is not accepted by the client
            if (!Status)
            {
                var getNotificationData = _db.Notifications.Where(a => a.Id == NotificationId).FirstOrDefault();

                int _notificationStatus = Enums.NotificationStatus.ServiceBySuperVisor.GetHashCode();

                int notificationStatus = Enums.NotificationStatus.SupervisorReject.GetHashCode();

                var getUserData = _db.Users.Where(a => a.UserId == getNotificationData.ToUserId).FirstOrDefault();
                string userName = getUserData != null ? getUserData.FullName : string.Empty;

                // Sending notification to the supervisor regarding job reject
                SendNotification(getNotificationData.ToUserId, getNotificationData.FromUserId,
                    notificationStatus, "The job offer has been rejected by the " + userName + " that is being sent by you.",
                    getNotificationData.JobRequestId);

                // Sending notification to the admin regarding job reject
                SendNotification(getNotificationData.ToUserId, accountService.GetAdminId(),
                    notificationStatus, "The job offer has been rejected by the " + userName + " that is being sent by the supervisor.",
                    getNotificationData.JobRequestId);

                // Sending notification to the customer regarding job reject
                SendNotification(getNotificationData.ToUserId, getNotificationData.ToUserId,
                    notificationStatus, "The job offer has been rejected by you that is being sent by the supervisor.",
                    getNotificationData.JobRequestId);

                if (getNotificationData != null)
                {
                    var getAdminNotification = _db.Notifications.Where(A => A.JobRequestId == getNotificationData.JobRequestId
                      && A.NotificationStatus == _notificationStatus).FirstOrDefault();

                    if (getAdminNotification != null)
                    {
                        getAdminNotification.NotificationStatus = notificationStatus;
                        _db.SaveChanges();
                    }

                    _db.Notifications.Remove(getNotificationData);
                    _db.SaveChanges();

                    message = Resource.offer_rejected_success;
                    jobRequest = null;
                }
            }
            else
            {
                // Offer is accepted by the client
                var getNotificationData = _db.Notifications.Where(a => a.Id == NotificationId).FirstOrDefault();
                getNotificationData.NotificationStatus = Enums.NotificationStatus.SupervisorAccept.GetHashCode();
                _db.SaveChanges();

                int notificationStatus = Enums.NotificationStatus.SupervisorAccept.GetHashCode();

                if (getNotificationData != null)
                {
                    jobRequest = BookingsDetails(getNotificationData.JobRequestId.Value);
                }
                var getUserData = _db.Users.Where(a => a.UserId == getNotificationData.ToUserId).FirstOrDefault();
                string userName = getUserData != null ? getUserData.FullName : string.Empty;

                // Sending notification to the supervisor regarding job accept
                SendNotification(getNotificationData.ToUserId, getNotificationData.FromUserId,
                    notificationStatus, "The job offer has been accepted by the " + userName + " that is being sent by you.",
                    getNotificationData.JobRequestId);

                // Sending notification to the admin regarding job accept
                SendNotification(getNotificationData.ToUserId, accountService.GetAdminId(),
                    notificationStatus, "The job offer has been accepted by the client that is being sent by the supervisor.",
                    getNotificationData.JobRequestId);

                message = Resource.offer_accepted_success;
            }

            return jobRequest;
        }

        public void SendNotification(long? fromUserId, long? toUserId, int status, string message, long? jobRequestId)
        {

            Notification notification = new Notification
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                NotificationStatus = status,
                IsActive = true,
                CreatedDate = DateTime.Now,
                Text = message,
                JobRequestId = jobRequestId
            };

            _db.Notifications.Add(notification);
            _db.SaveChanges();
        }

        #endregion

        #region Accept/Reject Sub-User Offer

        public bool AcceptRejectSubUserOffer(long NotificationId, bool Status, long UserId)
        {
            bool status = false;
            // Offer is not accepted by the main user
            if (!Status)
            {
                var getNotificationData = _db.Notifications.Where(a => a.Id == NotificationId).FirstOrDefault();

                // Get Sub User Request Data
                var getSubUserRequest = _db.SubUserRequests.Where(a => a.SubUserRequestId == getNotificationData.JobRequestId).FirstOrDefault();

                if (getSubUserRequest != null)
                {
                    getSubUserRequest.IsApproved = false;
                    _db.SaveChanges();
                }

                int notificationStatus = Enums.NotificationStatus.SubUserServiceRejected.GetHashCode();

                var getUserData = _db.Users.Where(a => a.UserId == UserId).FirstOrDefault();
                string userName = getUserData != null ? getUserData.FullName : string.Empty;

                // Sending notification to the sub-user regarding job reject
                SendNotification(getNotificationData.ToUserId, getNotificationData.FromUserId,
                    notificationStatus, "The request for ordering the service by you has been rejected by the " + userName,
                    getNotificationData.JobRequestId);

                // Sending notification to the main-user to let know that he has rejected the job
                SendNotification(getNotificationData.FromUserId, getNotificationData.ToUserId,
                    notificationStatus, "The request for ordering the service by the sub user has been rejected by you",
                    getNotificationData.JobRequestId);

                if (getNotificationData != null)
                {
                    _db.Notifications.Remove(getNotificationData);
                    _db.SaveChanges();
                    message = Resource.offer_rejected_success;
                    status = true;
                }
            }
            else
            {
                // Offer is accepted by the client
                var getNotificationData = _db.Notifications.Where(a => a.Id == NotificationId).FirstOrDefault();

                // Get Sub User Request Data
                var getSubUserRequest = _db.SubUserRequests.Where(a => a.SubUserRequestId == getNotificationData.JobRequestId).FirstOrDefault();

                if (getSubUserRequest != null)
                {
                    getSubUserRequest.IsApproved = true;
                    _db.SaveChanges();
                }


                int notificationStatus = Enums.NotificationStatus.SubUserServiceAccepted.GetHashCode();

                var getUserData = _db.Users.Where(a => a.UserId == UserId).FirstOrDefault();
                string userName = getUserData != null ? getUserData.FullName : string.Empty;

                // Sending notification to the sub-user regarding job accept
                SendNotification(getNotificationData.ToUserId, getNotificationData.FromUserId,
                    notificationStatus, "The request for ordering the service by you has been accepted by the " + userName,
                    getNotificationData.JobRequestId);

                // Sending notification to the main-user to let know about the job accept
                SendNotification(getNotificationData.FromUserId, getNotificationData.ToUserId,
                    notificationStatus, "The request for ordering the service by the sub user has been accepted by you",
                    getNotificationData.JobRequestId);

                if (getNotificationData != null)
                {
                    _db.Notifications.Remove(getNotificationData);
                    _db.SaveChanges();
                    message = Resource.offer_accepted_success;
                    status = true;
                }
            }

            return status;
        }

        public List<SubUserRequestVM> SubUserRequests(long userId)
        {
            List<SubUserRequestVM> lstData = new List<SubUserRequestVM>();
            try
            {
                var getSubUserRequestData = _db.SubUserRequests.Where(a => a.SubUserId == userId).ToList();
                if (getSubUserRequestData.Count > 0)
                {
                    foreach (var item in getSubUserRequestData)
                    {
                        SubUserRequestVM subUserRequest = new SubUserRequestVM
                        {
                            Reason = item.Reason,
                            ServiceId = item.ServiceId ?? 0,
                            UserId = item.SubUserId ?? 0,
                            IsAccepted = item.IsApproved,
                            CreatedDate = item.CreatedDate,
                            CategoryName = item.Category?.Name
                        };

                        lstData.Add(subUserRequest);
                    }
                }

                message = Resource.success;
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }


        #endregion

        #region Accept/Reject Provider Job Request

        public bool AcceptRejectProviderJobRequest(long NotificationId, bool Status)
        {
            bool status = false;
            // Offer is not accepted by the main user
            if (!Status)
            {
                var getNotificationData = _db.Notifications.Where(a => a.Id == NotificationId).FirstOrDefault();

                var getJobServiceData = _db.JobRequestPropertyServices.Where(a => a.JobRequestPropId == getNotificationData.JobRequestId).FirstOrDefault();

                var userData = _db.Users.Where(a => a.UserId == getNotificationData.ToUserId).FirstOrDefault();

                var fromUserData = _db.Users.Where(a => a.UserId == getNotificationData.FromUserId).FirstOrDefault();

                // Send notification to admin
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = getNotificationData.ToUserId;
                notification.IsActive = true;
                notification.JobRequestId = getJobServiceData.JobRequestId;
                notification.NotificationStatus = Enums.NotificationStatus.Rejected.GetHashCode();
                notification.ToUserId = accountService.GetAdminId();
                notification.Text = "Service provider, " + userData.FullName + " has rejected the job request for the client " +
                    fromUserData.FullName;
                _db.Notifications.Add(notification);
                _db.SaveChanges();


                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                && A.IsActive == true).ToList();

                if (getSupervisors.Count > 0)
                {
                    foreach (var item in getSupervisors)
                    {
                        notification.ToUserId = item.UserId;
                        _db.Notifications.Add(notification);

                        _db.SaveChanges();
                    }
                }

                // send notification to the provider who has rejected the job request
                getNotificationData.NotificationStatus = Enums.NotificationStatus.Rejected.GetHashCode();
                getNotificationData.CreatedDate = DateTime.Now;
                getNotificationData.Text = "You have rejected the job request that is being assigned to you";
                _db.SaveChanges();
            }
            else
            {
                // job request is accepted by the service provider
                var getNotificationData = _db.Notifications.Where(a => a.Id == NotificationId).FirstOrDefault();

                var getJobServiceData = _db.JobRequestPropertyServices.Where(a => a.JobRequestPropId == getNotificationData.JobRequestId).FirstOrDefault();

                if (getJobServiceData != null)
                {
                    var getPropertyName = _db.Properties.Where(A => A.Id == getJobServiceData.PropertyId).FirstOrDefault();

                    getJobServiceData.AssignedWorker = getNotificationData.ToUserId;
                    getJobServiceData.JobStatus = (int)Enums.RequestStatus.InProgress;

                    var getJobDateTime = getJobServiceData.StartDateTime;
                    var getTime = getJobDateTime.Value.ToString("HH:mm");

                    var time = Convert.ToDouble(getJobServiceData.TimeToDo);

                    var t1 = TimeSpan.Parse(getTime);
                    var t2 = TimeSpan.FromHours(time);

                    var busyTime = t1 + t2;

                    // add data in occupied provider table
                    OccupiedProvider occupiedProvider = new OccupiedProvider();
                    occupiedProvider.FromTime = getTime;
                    occupiedProvider.ToTime = busyTime.ToString();
                    occupiedProvider.JobDateTime = getJobServiceData.StartDateTime;
                    occupiedProvider.JobPropertyServiceId = getJobServiceData.JobRequestPropId;
                    occupiedProvider.ProviderId = getNotificationData.ToUserId;

                    _db.OccupiedProviders.Add(occupiedProvider);
                    _db.SaveChanges();

                    var getWorkerData = _db.Users.Where(a => a.UserId == getNotificationData.ToUserId).FirstOrDefault();

                    #region Send Notification to the admin/supervisor

                    // Send notification to admin
                    Notification notification = new Notification();
                    notification.CreatedDate = DateTime.Now;
                    notification.FromUserId = getNotificationData.ToUserId;
                    notification.IsActive = true;
                    notification.JobRequestId = getJobServiceData.JobRequestId;
                    notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    notification.ToUserId = accountService.GetAdminId();
                    notification.Text = getWorkerData.FullName + " has been assigned for the property " + getPropertyName.Name +
                        "<br/>Job Start Time: " + getJobServiceData.StartDateTime + " Time To do the service: " + getJobServiceData.TimeToDo;
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();


                    // send notification to supervisor
                    var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                    var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                    && A.IsActive == true).ToList();

                    if (getSupervisors.Count > 0)
                    {
                        foreach (var item in getSupervisors)
                        {
                            notification.ToUserId = item.UserId;
                            _db.Notifications.Add(notification);

                            _db.SaveChanges();
                        }
                    }


                    #endregion

                    // send notification to the assigned worker
                    Notification _notification = new Notification();
                    _notification.CreatedDate = DateTime.Now;
                    _notification.FromUserId = getPropertyName.CreatedBy;
                    _notification.IsActive = true;
                    _notification.JobRequestId = getJobServiceData.JobRequestId;
                    _notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    _notification.ToUserId = getNotificationData.ToUserId;
                    _notification.Text = "You have accepted the job request";
                    _db.Notifications.Add(_notification);
                    _db.SaveChanges();

                    // send notification to the customer
                    Notification _notification1 = new Notification();
                    _notification1.CreatedDate = DateTime.Now;
                    _notification1.FromUserId = getNotificationData.ToUserId;
                    _notification1.IsActive = true;
                    _notification1.JobRequestId = getJobServiceData.JobRequestId;
                    _notification1.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    _notification1.ToUserId = getNotificationData.FromUserId;
                    _notification1.Text = getWorkerData.FullName + " has been assigned to you for the property " + getPropertyName.Name +
                        "<br/>Job Start Time: " + getJobServiceData.StartDateTime;
                    _db.Notifications.Add(_notification);
                    _db.SaveChanges();
                }



                status = true;
                message = Resource.offer_accepted_success;
            }

            return status;
        }

        #endregion

        #region Reviews

        /// <summary>
        /// Submit UserReview Method.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SubmitUserReview(UserReviewModel model)
        {
            bool status = false;
            try
            {
                var review = _db.UserReviews.Where(x => x.CustomerId == model.CustomerId
                && x.ToUserId == model.ToUserId).FirstOrDefault();
                if (review == null)
                {
                    UserReview userReview = new UserReview()
                    {
                        CreatedDate = DateTime.Now,
                        CustomerId = model.CustomerId,
                        IsActive = true,
                        Rating = model.UserRating,
                        Review = model.UserReview,
                        ToUserId = model.ToUserId
                    };
                    _db.UserReviews.Add(userReview);
                    _db.SaveChanges();

                    status = true;
                    message = Resource.review_success;

                }
                else
                {
                    review.Review = model.UserReview;
                    review.Rating = model.UserRating;
                    review.ModifiedDate = DateTime.Now;

                    _db.SaveChanges();
                    status = true;
                    message = Resource.review_success;
                }
                if (model.UserRating < 4)
                {
                    var UserName = "";
                    var WorkerName = "";
                    var data = _db.Users.Where(x => x.UserId == model.CustomerId).FirstOrDefault();
                    if (data != null)
                    {
                        UserName = data.FullName != null ? data.FullName : data.CompanyName;
                    }
                    WorkerName = _db.Users.Where(x => x.UserId == model.ToUserId).Select(x => x.FullName).FirstOrDefault();
                    Notification notification = new Notification();
                    notification.CreatedDate = DateTime.Now;
                    notification.FromUserId = model.CustomerId;
                    notification.IsActive = true;
                    notification.JobRequestId = 0;
                    notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                    notification.Text = "User " + UserName + " Rating " + model.UserRating + " to Worker " + WorkerName;
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public bool SubmitJobReview(UserReviewModel model)
        {
            bool status = false;
            try
            {
                var review = _db.UserJobReviews.Where(x => x.JobRequestId == model.JobRequestId).FirstOrDefault();
                if (review == null)
                {
                    UserJobReview userJobReview = new UserJobReview()
                    {
                        CreatedDate = DateTime.Now,
                        CustomerId = model.CustomerId,
                        JobRequestId = model.JobRequestId,
                        IsActive = true,
                        Rating = model.UserRating,
                        Review = model.UserReview
                    };
                    _db.UserJobReviews.Add(userJobReview);
                    _db.SaveChanges();

                    status = true;
                    message = Resource.review_success;

                }
                else
                {
                    review.Review = model.UserReview;
                    review.Rating = model.UserRating;
                    review.ModifiedDate = DateTime.Now;
                    _db.SaveChanges();

                    status = true;
                    message = Resource.review_success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public List<UserReviewModel> GetUserReviews(long UserId)
        {
            List<UserReviewModel> userReviews = new List<UserReviewModel>();
            try
            {
                var review = _db.UserReviews.Where(x => x.ToUserId == UserId
                || x.CustomerId == UserId).ToList();
                if (review.Count > 0)
                {
                    foreach (var item in review)
                    {
                        UserReviewModel userJobReview = new UserReviewModel()
                        {
                            CustomerId = item.CustomerId,
                            UserRating = item.Rating,
                            ToUserId = item.ToUserId,
                            UserReview = item.Review,
                            CreatedDate = item.CreatedDate,
                            CustomerImage = item.User?.PicturePath,
                            CustomerName = item.User?.FullName,
                            WorkerName = item.User1?.FullName,
                            WorkerImage = item.User1?.PicturePath,
                            UserType = item.User1?.UserType
                        };
                        userReviews.Add(userJobReview);
                    }

                }

                List<UserJobReview> jobReviews = new List<UserJobReview>();
                var getUserData = _db.Users.Where(a => a.UserId == UserId).FirstOrDefault();
                if (getUserData != null)
                {
                    if (getUserData.UserType == Enums.UserTypeEnum.Customer.GetHashCode())
                    {
                        jobReviews = _db.UserJobReviews.Where(x => x.CustomerId == UserId).ToList();
                    }
                    else if (getUserData.UserType == Enums.UserTypeEnum.Worker.GetHashCode()
                        || getUserData.UserType == Enums.UserTypeEnum.ServiceProvider.GetHashCode())
                    {
                        var getJobData = _db.JobRequestPropertyServices.Where(a => a.AssignedWorker == UserId).ToList();
                        if (getJobData != null)
                        {
                            if (getJobData.Count > 0)
                            {
                                foreach (var item in getJobData)
                                {
                                    var getJobReview = _db.UserJobReviews.Where(a => a.JobRequestId == item.JobRequestPropId).FirstOrDefault();
                                    if (getJobReview != null)
                                    {
                                        jobReviews.Add(getJobReview);
                                    }
                                }
                            }
                        }
                    }
                }
                if (jobReviews.Count > 0)
                {
                    foreach (var item in jobReviews)
                    {
                        UserReviewModel userJobReview = new UserReviewModel()
                        {
                            CustomerId = item.CustomerId,
                            UserRating = item.Rating,
                            ToUserId = item.ToUserId,
                            UserReview = item.Review,
                            CreatedDate = item.CreatedDate,
                            CustomerImage = item.User?.PicturePath,
                            CustomerName = item.User?.FullName,
                            WorkerName = item.User1?.FullName,
                            WorkerImage = item.User1?.PicturePath,
                            UserType = item.User1?.UserType
                        };
                        userReviews.Add(userJobReview);
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;

            }
            return userReviews;
        }

        public UserReviewModel CheckUserReview(long FromUserId, long ToUserId)
        {
            UserReviewModel userJobReview = new UserReviewModel();
            try
            {
                var review = _db.UserReviews.Where(x => x.ToUserId == ToUserId
                && x.CustomerId == FromUserId).FirstOrDefault();
                if (review != null)
                {
                    userJobReview.CustomerId = review.CustomerId;
                    userJobReview.UserRating = review.Rating;
                    userJobReview.ToUserId = review.ToUserId;
                    userJobReview.UserReview = review.Review;
                    userJobReview.CreatedDate = review.CreatedDate;
                    userJobReview.CustomerImage = review.User?.PicturePath;
                    userJobReview.CustomerName = review.User?.FullName;

                    message = Resource.success;

                }
            }
            catch (Exception ex)
            {
                message = ex.Message;

            }
            return userJobReview;
        }


        public UserReviewModel CheckJobReview(long JobId)
        {
            UserReviewModel userJobReview = new UserReviewModel();
            try
            {
                var review = _db.UserJobReviews.Where(x => x.JobRequestId == JobId).FirstOrDefault();
                if (review != null)
                {
                    userJobReview.CustomerId = review.CustomerId;
                    userJobReview.UserRating = review.Rating;
                    userJobReview.ToUserId = review.ToUserId;
                    userJobReview.UserReview = review.Review;
                    userJobReview.CreatedDate = review.CreatedDate;
                    userJobReview.CustomerImage = review.User?.PicturePath;
                    userJobReview.CustomerName = review.User?.FullName;
                    userJobReview.JobRequestId = review.JobRequestId;

                    message = Resource.success;

                }
            }
            catch (Exception ex)
            {
                message = ex.Message;

            }
            return userJobReview;
        }
        #endregion

        #region Invoice Month
        public bool InvoiceMonth(string userId, string description, string price)
        {
            bool status = false;
            MonthInvoice monthInvoice = new MonthInvoice();
            MonthInvoiceService monthInvoiceService = new MonthInvoiceService();
            long UserId = long.Parse(userId);
            try
            {
                var invoiceUser = _db.MonthInvoiceServices.Where(x => x.UserId == UserId).FirstOrDefault();
                if (invoiceUser != null)
                {
                    invoiceUser.LastService = DateTime.Now;
                    _db.SaveChanges();
                }
                else
                {
                    monthInvoiceService.UserId = UserId;
                    monthInvoiceService.FirstService = DateTime.Now;
                    _db.MonthInvoiceServices.Add(monthInvoiceService);
                    _db.SaveChanges();
                }


                monthInvoice.UserID = userId;
                monthInvoice.Description = description;
                monthInvoice.Price = price;
                monthInvoice.CreateDate = DateTime.Now;
                _db.MonthInvoices.Add(monthInvoice);
                _db.SaveChanges();
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
        #endregion

        #region MonthInvoiceMail
        public List<MonthInvoice> MonthInvoiceMail(string userId)
        {
            bool status = false;

            MonthInvoice monthInvoice = new MonthInvoice();
            var invoices = _db.MonthInvoices.Where(x => x.UserID == userId).ToList();
            return invoices;
        }
        #endregion

        #region Modify CheckList
        public bool ModifyCheckList(List<JobRequestCheckListModel> objChkdata, long JobReqId)
        {
            bool status = false;
            //  var jobId = 0;
            try
            {
                JobRequestCheckList objChkList = new JobRequestCheckList();
                foreach (var item in objChkdata)
                {
                    if (item.Id != 0)
                    {
                        var objchk = _db.JobRequestCheckLists.Where(x => x.Id == item.Id).FirstOrDefault();
                        objchk.Id = item.Id;
                        objchk.IsDone = item.IsDone;
                        objchk.TaskDetail = item.TaskDetail;
                        _db.SaveChanges();
                    }
                    else
                    {
                        objChkList.Id = item.Id;
                        objChkList.IsDone = item.IsDone;
                        objChkList.TaskDetail = item.TaskDetail;
                        objChkList.JobRequestId = JobReqId;
                        _db.JobRequestCheckLists.Add(objChkList);
                        _db.SaveChanges();
                    }
                }
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        #endregion

        public bool SaveWorkerQuesResponse(SaveQuesResponse QuesResponse)
        {
            bool status = false;
            int count = 0;
            try
            {
                var objQues = _db.Questions.Where(x => x.Type == QuesResponse.Type && x.Status == true).Take(5).ToList();
                foreach (var item in objQues)
                {
                    count++;
                    WorkerQuestion objWorkerQues = new WorkerQuestion();
                    objWorkerQues.QuesId = item.QuesId;
                    objWorkerQues.UserId = QuesResponse.WorkerId;
                    if (count == 1)
                    {
                        objWorkerQues.Response = QuesResponse.QuesResponse1;
                    }
                    else if (count == 2)
                    {
                        objWorkerQues.Response = QuesResponse.QuesResponse2;
                    }
                    else if (count == 3)
                    {
                        objWorkerQues.Response = QuesResponse.QuesResponse3;
                    }
                    else if (count == 4)
                    {
                        objWorkerQues.Response = QuesResponse.QuesResponse4;
                    }
                    else if (count == 5)
                    {
                        objWorkerQues.Response = QuesResponse.QuesResponse5;
                    }
                    objWorkerQues.CreateDate = DateTime.Now;
                    _db.WorkerQuestions.Add(objWorkerQues);
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
        public List<Question> GetWorkerQuestionsByWorkerType(string type)
        {
            List<Question> objWorkerQues = new List<Question>();
            try
            {
                objWorkerQues = _db.Questions.Where(x => x.Type == type && x.Status == true).Take(5).ToList();
                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return objWorkerQues;
        }

        public bool AddMeetingSchedule(string MeetingDate, string MeetingTime, long UserId)
        {
            bool status = false;
            MeetingSchedulRequest objMeeting = new MeetingSchedulRequest();
            try
            {
                objMeeting.UserId = UserId;
                objMeeting.Date = MeetingDate;
                objMeeting.Time = MeetingTime;
                objMeeting.RequestStatus = "0";
                _db.MeetingSchedulRequests.Add(objMeeting);
                _db.SaveChanges();
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = UserId;
                notification.IsActive = true;
                notification.JobRequestId = 0;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                notification.Text = "User Send You a Meeting Request";
                _db.Notifications.Add(notification);
                _db.SaveChanges();
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


        #region ICount Payment

        public decimal CalculateVat(decimal value)
        {
            return (value / 100) * 17;
        }

        public ICountSalesResponse GeneratePaymentPage(JobRequest jobRequest, User user)
        {
            var jss = new JavaScriptSerializer();
            var jobDetails = GetJobRequestDetails(jobRequest.Id);
            var price = jobRequest.ServicePrice;
            var description = jobDetails.PropertyName + ", " + jobDetails.ServiceName + ", " + String.Format("{0:g}", jobDetails.StartDateTime);
            string page_id = "";
            try
            {
                string cid = Convert.ToString(ConfigurationManager.AppSettings["icount_cid"]);
                string icount_user = Convert.ToString(ConfigurationManager.AppSettings["icount_user"]);
                string pass = Convert.ToString(ConfigurationManager.AppSettings["icount_pass"]);

                ICountSalesResponse CreatePageResponse = CreatePage(cid, icount_user, pass, description, price.ToString());
                if (CreatePageResponse != null && CreatePageResponse.status)
                {
                    page_id = CreatePageResponse.paypage_id;
                }

                decimal dUnitprice = Convert.ToDecimal(price);
                int unitprice = Convert.ToInt32(dUnitprice);
                decimal priceIncludingVat = dUnitprice + CalculateVat(dUnitprice);
                string client_name = user.CompanyName ?? user.FullName.Trim();
                string client_address = user.BillingAddress ?? user.Address;


                string email = user.Email;
                string email_to = user.Email;
                var jobIdToken = tokenService.GenerateToken(jobRequest.Id);
                string[,] arr = new string[2, 3];
                arr[0, 0] = description;
                arr[0, 1] = Convert.ToString(price);
                arr[0, 2] = "1";

                string path = Common.BaseUrl;

                string success_url = string.Format(@"{0}/Order/PaymentSuccess?description={1}", path, description);
                string failiure_url = string.Format("{0}/Order/PaymentCancel?description={1}", path, description);
                string ipn_url = string.Format("{0}/api/Order/ProcessPayment?jobIdToken={1}", path, jobIdToken);

                var items = "items[0][description]=" + arr[0, 0] + "&items[0][unitprice]=" + arr[0, 1] + "&items[0][quantity]=" + arr[0, 2];
                var url = string.Format(Common.GeneratSale + items, cid, icount_user, pass, client_name, page_id, price, email, user.PhoneNumber, user.IdNumber, success_url, failiure_url, ipn_url);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.ContentType = "application/json";
                var response = (HttpWebResponse)myReq.GetResponse();
                string text;

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
                ICountSalesResponse countResponse = jss.Deserialize<ICountSalesResponse>(text);
                // here update the PayPageId and SaleUrl of the Job Request.
                jobRequest.PayPageId = page_id;
                jobRequest.SaleUrl = countResponse.sale_url;

                _db.SaveChanges();

                return countResponse;
            }
            catch (Exception exp)
            {
                return jss.Deserialize<ICountSalesResponse>(exp.Message);
            }
        }

        private ICountSalesResponse CreatePage(string cid, string user, string pass, string description, string price, string CurrencyId = "5")
        {
            var jss = new JavaScriptSerializer();
            try
            {
                string[,] arr = new string[2, 3];
                arr[0, 0] = description;
                arr[0, 1] = Convert.ToString(price);
                arr[0, 2] = "1";

                var items = "items[0][description]=" + arr[0, 0] + "&items[0][unitprice]=" + arr[0, 1] + "&items[0][quantity]=" + arr[0, 2];
                var url = string.Format(Common.CreatePage + items, cid, user, pass, description, CurrencyId);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.ContentType = "application/json";
                var response = (HttpWebResponse)myReq.GetResponse();
                string text;

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
                ICountSalesResponse countResponse = jss.Deserialize<ICountSalesResponse>(text);
                return countResponse;
            }
            catch (Exception exp)
            {
                return jss.Deserialize<ICountSalesResponse>(exp.Message);
            }
        }

        private bool DeletePaymentPage(string pageId)
        {
            try
            {
                string cid = Convert.ToString(ConfigurationManager.AppSettings["icount_cid"]);
                string icount_user = Convert.ToString(ConfigurationManager.AppSettings["icount_user"]);
                string icount_pass = Convert.ToString(ConfigurationManager.AppSettings["icount_pass"]);
                var url = string.Format(Common.DeletePage, cid, icount_user, icount_pass, pageId);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.ContentType = "application/json";
                var response = (HttpWebResponse)myReq.GetResponse();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public JobRequestDetailViewModel GetJobRequestDetails(long jobId)
        {
            var jobRequestVM = (from jr in _db.JobRequests
                                where jr.Id == jobId
                                join ps in _db.JobRequestPropertyServices on jr.Id equals ps.JobRequestId
                                join p in _db.Properties on ps.PropertyId equals p.Id
                                join s in _db.SubCategories on ps.ServiceId equals s.Id
                                select new JobRequestDetailViewModel
                                {
                                    JobRequestId = (int)jr.Id,
                                    JobDescription = jr.Description,
                                    PropertyId = (int)p.Id,
                                    PropertyName = p.Name,
                                    PropertyAddress = p.Address,
                                    FromUserId = (int)jr.UserId,
                                    AssignedWorkerId = (int)ps.AssignedWorker,
                                    StartDateTime = ps.StartDateTime,
                                    JobRequestPropId = (int)ps.JobRequestPropId,
                                    PaymentPageId = jr.PayPageId,
                                    ServiceName = s.Name,
                                    JobStatus = (int)ps.JobStatus,
                                    TimeToDo = (int)ps.TimeToDo,
                                }).FirstOrDefault();

            if (jobRequestVM != null)
            {
                var service = _db.JobRequestSubSubCategories.FirstOrDefault(x => x.JobRequestId == jobRequestVM.JobRequestPropId);

                if (service.SubSubCategoryId != 0)
                {
                    var subSubCategory = _db.SubSubCategories.FirstOrDefault(x => x.Id == service.SubSubCategoryId);
                    jobRequestVM.ServiceName = subSubCategory.Name;
                }
            }
            return jobRequestVM;
        }

        public JobRequest GetJobRequest(long jobId)
        {
            try
            {
                var jobRequest = _db.JobRequests.FirstOrDefault(x => x.Id == jobId);

                return jobRequest;
            }
            catch (Exception)
            {

                return null;
            }

        }

        public bool ProcessPayment(string jobIdToken)
        {
            try
            {
                // jobId is jwt token so validate the jobId
                var jobId = tokenService.ValidateToken(jobIdToken);
                // get the job by the jobId
                if (jobId == null) return false;

                var jobRequest = _db.JobRequests.FirstOrDefault(x => x.Id == jobId);

                var jobRequestProperty = _db.JobRequestPropertyServices.FirstOrDefault(x => x.JobRequestId == jobId);

                if (jobRequestProperty == null) return false;

                var jobDetails = GetJobRequestDetails((long)jobId);

                var jobUser = _db.Users.FirstOrDefault(x => x.UserId == jobRequest.UserId);

                var jobWorker = _db.Users.FirstOrDefault(x => x.UserId == jobRequestProperty.AssignedWorker);

                // check if the jobRequest user is subuser then add 5% balance to the property owner
                if (jobUser.UserType == Enums.UserTypeEnum.SubUser.GetHashCode())
                {
                    // get the property owner
                    var property = _db.Properties.FirstOrDefault(x => x.Id == jobRequestProperty.PropertyId);
                    var propertyOwner = _db.Users.FirstOrDefault(x => x.UserId == property.CreatedBy);
                    var percentage = (jobRequest.ServicePrice.Value * 5) / 100;
                    propertyOwner.Balance += percentage;

                    // send notification to the property owner
                    Notification ownerNotification = new Notification();
                    ownerNotification.CreatedDate = DateTime.Now;
                    ownerNotification.FromUserId = jobWorker.UserId;
                    ownerNotification.IsActive = true;
                    ownerNotification.JobRequestId = jobId;
                    ownerNotification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    ownerNotification.ToUserId = propertyOwner.UserId;
                    ownerNotification.Text = "you have received " + percentage + " ils for ordering " + jobDetails.ServiceName + " for the property " + jobDetails.PropertyName;
                    _db.Notifications.Add(ownerNotification);
                }

                #region Send Notification to the admin/supervisor

                // Send notification to admin
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = jobWorker.UserId;
                notification.IsActive = true;
                notification.JobRequestId = jobId;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = accountService.GetAdminId();
                notification.Text = jobWorker.FullName + " has been assigned for the service " + jobDetails.ServiceName + " for the property " + jobDetails.PropertyName +
                    "<br/>Job Start Time: " + jobRequestProperty.StartDateTime + " Time To do the service: " + jobRequestProperty.TimeToDo;
                _db.Notifications.Add(notification);


                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType
                && A.IsActive == true).ToList();

                if (getSupervisors.Count > 0)
                {
                    foreach (var supervisor in getSupervisors)
                    {
                        notification.ToUserId = supervisor.UserId;
                        _db.Notifications.Add(notification);

                        _db.SaveChanges();
                    }
                }

                #endregion

                #region Send Notification To Customer

                // send notification to the customer
                Notification _notification1 = new Notification();
                _notification1.CreatedDate = DateTime.Now;
                _notification1.FromUserId = jobWorker.UserId;
                _notification1.IsActive = true;
                _notification1.JobRequestId = jobId;
                _notification1.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                _notification1.ToUserId = jobUser.UserId;
                _notification1.Text = jobWorker.FullName + " has been assigned to you for the property " + jobDetails.PropertyName +
                    " " + "for the service " + jobDetails.ServiceName +
                    "<br/>Job Start Time: " + jobRequestProperty.StartDateTime;
                _db.Notifications.Add(_notification1);

                #endregion

                #region Send Notification to Worker

                // send notification to the assigned worker
                Notification _notification = new Notification();
                _notification.CreatedDate = DateTime.Now;
                _notification.FromUserId = jobUser.UserId;
                _notification.IsActive = true;
                _notification.JobRequestId = jobId;
                _notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                _notification.ToUserId = jobWorker.UserId;
                _notification.Text = jobDetails.ServiceName + " has been assigned to you for the property " + jobDetails.PropertyName +
                    "<br/>Job Start Time: " + jobRequestProperty.StartDateTime + " Time To do the service: " + jobRequestProperty.TimeToDo;
                _db.Notifications.Add(_notification);

                #endregion

                // get the job assigned worker
                // Occupy the assigned
                //------------Add worker/serviceprovider to user chat----------------
                var ChatData = _db.UserChats.Where(x => x.FromUserId == jobRequestProperty.AssignedWorker || x.ToUserId == jobRequestProperty.AssignedWorker).FirstOrDefault();
                if (ChatData == null)
                {
                    UserChat user = new UserChat
                    {
                        FromUserId = jobRequest.UserId,
                        ToUserId = jobRequestProperty.AssignedWorker,
                        CreatedDate = DateTime.Now
                    };
                    _db.UserChats.Add(user);
                }
                var getTime = jobRequestProperty.StartDateTime.Value.ToString("HH:mm");

                var time = Convert.ToDouble(jobRequestProperty.TimeToDo);

                var t1 = TimeSpan.Parse(getTime);
                var t2 = TimeSpan.FromHours(time);

                var busyTime = t1 + t2;
                // add data in occupied provider table that the assigned worker is now busy
                OccupiedProvider occupiedProvider = new OccupiedProvider
                {
                    FromTime = getTime,
                    ToTime = busyTime.ToString(),
                    JobDateTime = jobRequestProperty.StartDateTime,
                    JobPropertyServiceId = jobRequestProperty.JobRequestPropId,
                    ProviderId = jobRequestProperty.AssignedWorker,
                    IsDeliveryGuy = false
                };
                _db.OccupiedProviders.Add(occupiedProvider);
                // add notification for the assigned worker

                // update the jobstatus to pending
                // IsPaymentDone to true
                jobRequest.IsPaymentDone = true;
                jobRequestProperty.JobStatus = Enums.RequestStatus.Pending.GetHashCode();
                _db.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;

        }
        #endregion

        public PropertyModelApi GetPropertyDetailById(long PropId)
        {
            PropertyModelApi obj = new PropertyModelApi();
            try
            {
                obj = _db.Properties.Where(x => x.Id == PropId).Select(x => new PropertyModelApi()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address
                }).FirstOrDefault();
                message = Resource.success;
            }
            catch (Exception ex)
            {
                obj = null;
                message = ex.Message;
            }
            return obj;
        }


        public ServiceQuote GetQuotesPrice(long UserId, long PropId, int CatId, int? SubCatId, int? SubSubCatId)
        {
            ServiceQuote obj = new ServiceQuote();
            try
            {
                if (SubSubCatId != 0 && SubSubCatId != null)
                {
                    obj = _db.ServiceQuotes.Where(x => x.UserId == UserId && x.ServiceId == CatId && x.PropertyId == PropId && x.SubSubCategoryId == SubSubCatId).OrderByDescending(x => x.Id).FirstOrDefault();

                }
                else
                {
                    obj = _db.ServiceQuotes.Where(x => x.UserId == UserId && x.ServiceId == CatId && x.PropertyId == PropId && x.SubCategoryId == SubCatId).OrderByDescending(x => x.Id).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                obj = null;
            }
            return obj;
        }

        #region Quote
        public JobRequestDetailViewModel AcceptQuotes(long jobId, long userId)
        {
            try
            {
                // get the job by jobId
                var jobRequest = _db.JobRequests.FirstOrDefault(x => x.Id == jobId);

                if (jobRequest.UserId != userId) return null;

                var user = _db.Users.FirstOrDefault(x => x.UserId == userId);

                if (user == null) return null;

                JobRequestDetailViewModel jobRequestDetail = new JobRequestDetailViewModel
                {
                    SaleUrl = jobRequest.SaleUrl
                };
                var jobPropertyService = _db.JobRequestPropertyServices.FirstOrDefault(x => x.JobRequestId == jobId);
                // update the job status to unpaid
                jobPropertyService.JobStatus = Enums.RequestStatus.UnPaid.GetHashCode();
                _db.SaveChanges();

                // here send notification to the worker or service provider that quote accepted.

                return jobRequestDetail;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public bool RejectQuotes(long jobId, long userId)
        {
            bool status = false;
            try
            {
                var jobRequest = _db.JobRequests.FirstOrDefault(x => x.Id == jobId);

                if (jobRequest.UserId != userId) return false;

                var user = _db.Users.FirstOrDefault(x => x.UserId == userId);

                if (user == null) return false;

                var jobPropertyService = _db.JobRequestPropertyServices.FirstOrDefault(x => x.JobRequestId == jobId);
                // update the job status to unpaid
                jobPropertyService.JobStatus = Enums.RequestStatus.QuoteRejected.GetHashCode();
                _db.SaveChanges();

                AddNotification(userId, user.FullName, jobId);

                return true;
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            return status;
        }

        public List<JobRequestDetailViewModel> GetServiceQuotes(long userId)
        {
            int quoteRequestStatus = Enums.RequestStatus.QuoteRequested.GetHashCode();
            int quotePriceStatus = Enums.RequestStatus.QuotePriced.GetHashCode();
            int unpaidStatus = Enums.RequestStatus.UnPaid.GetHashCode();
            int quoteRejectedStatus = Enums.RequestStatus.QuoteRejected.GetHashCode();
            try
            {
                var jobRequestsVM = (from jr in _db.JobRequests
                                     join ps in _db.JobRequestPropertyServices on jr.Id equals ps.JobRequestId
                                     join p in _db.Properties on ps.PropertyId equals p.Id
                                     join s in _db.SubCategories on ps.ServiceId equals s.Id
                                     where jr.UserId == userId && (ps.JobStatus == unpaidStatus ||
                                     ps.JobStatus == quoteRequestStatus ||
                                     ps.JobStatus == quoteRejectedStatus ||
                                     ps.JobStatus == quotePriceStatus)
                                     select new JobRequestDetailViewModel
                                     {
                                         JobRequestId = (int)jr.Id,
                                         JobDescription = jr.Description,
                                         PropertyId = (int)p.Id,
                                         PropertyName = p.Name,
                                         PropertyAddress = p.Address,
                                         FromUserId = (int)jr.UserId,
                                         AssignedWorkerId = (int)ps.AssignedWorker,
                                         StartDateTime = ps.StartDateTime,
                                         JobRequestPropId = (int)ps.JobRequestPropId,
                                         PaymentPageId = jr.PayPageId,
                                         ServiceName = s.Name,
                                         JobStatus = (int)ps.JobStatus,
                                         TimeToDo = (int)ps.TimeToDo,
                                         QuotePrice = (decimal)jr.ServicePrice,
                                         SaleUrl = jr.SaleUrl,
                                         ServicePrice = jr.ServicePrice
                                     }).ToList();
                return jobRequestsVM;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }
        }

        public List<JobRequestDetailViewModel> AvailableQuotesSP(long serviceProviderId)
        {
            int quoteRequestStatus = Enums.RequestStatus.QuoteRequested.GetHashCode();
            int quotePriceStatus = Enums.RequestStatus.QuotePriced.GetHashCode();
            int unpaidStatus = Enums.RequestStatus.UnPaid.GetHashCode();
            int quoteRejectedStatus = Enums.RequestStatus.QuoteRejected.GetHashCode();
            try
            {
                var jobRequestsVM = (from ps in _db.JobRequestPropertyServices
                                     join jr in _db.JobRequests on ps.JobRequestId equals jr.Id
                                     join p in _db.Properties on ps.PropertyId equals p.Id
                                     join s in _db.SubCategories on ps.ServiceId equals s.Id
                                     where ps.AssignedWorker == serviceProviderId &&
                                     (ps.JobStatus == quoteRequestStatus ||
                                     ps.JobStatus == quotePriceStatus ||
                                     ps.JobStatus == unpaidStatus ||
                                     ps.JobStatus == quoteRejectedStatus)
                                     select new JobRequestDetailViewModel
                                     {
                                         JobRequestId = (int)jr.Id,
                                         JobDescription = jr.Description,
                                         PropertyId = (int)p.Id,
                                         PropertyName = p.Name,
                                         PropertyAddress = p.Address,
                                         FromUserId = (int)jr.UserId,
                                         AssignedWorkerId = (int)ps.AssignedWorker,
                                         StartDateTime = ps.StartDateTime,
                                         JobRequestPropId = (int)ps.JobRequestPropId,
                                         PaymentPageId = jr.PayPageId,
                                         ServiceName = s.Name,
                                         JobStatus = (int)ps.JobStatus,
                                         TimeToDo = (int)ps.TimeToDo,
                                     }).ToList();
                return jobRequestsVM;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }
        }

        public bool QuotePriceSP(QuoteViewModel model)
        {
            if (model == null) return false;
            try
            {
                // get the job by jobId
                var jobRequest = _db.JobRequests.FirstOrDefault(x => x.Id == model.JobRequestId);

                if (jobRequest == null) return false;

                var percentage = 0;

                var serviceProvider = _db.Users.FirstOrDefault(x => x.UserId == model.ServiceProviderId);

                if (serviceProvider == null) return false;

                // check if this is the service provider assigned for the job
                var jobPropertyService = _db.JobRequestPropertyServices.FirstOrDefault(x => x.JobRequestId == model.JobRequestId);

                if (jobPropertyService == null) return false;

                if (jobPropertyService.AssignedWorker != model.ServiceProviderId) return false;

                percentage = serviceProvider.Percentage != null ? Convert.ToInt32(serviceProvider.Percentage) : 0;

                var servicePrice = model.QuotePrice + (model.QuotePrice * percentage) / 100;

                jobRequest.ServicePrice = servicePrice;
                jobRequest.QuotePrice = model.QuotePrice;
                jobRequest.ClientQuotePrice = servicePrice;
                // update the job status to unpaid
                jobPropertyService.JobStatus = Enums.RequestStatus.QuotePriced.GetHashCode();
                _db.SaveChanges();

                var user = _db.Users.FirstOrDefault(x => x.UserId == jobRequest.UserId);
                // generate payment page
                var countSalesResponse = GeneratePaymentPage(jobRequest, user);

                // Notification
                Notification notification = new Notification
                {
                    CreatedDate = DateTime.Now,
                    FromUserId = model.ServiceProviderId,
                    IsActive = true,
                    JobRequestId = 0,
                    NotificationStatus = Enums.NotificationStatus.SentQuotation.GetHashCode(),
                    ToUserId = jobRequest.UserId,
                    Text = "Service Provider " + serviceProvider.FullName + " Sent Quote Price"
                };
                _db.Notifications.Add(notification);

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "Quote price failed!";
                return false;
            }

            return true;
        }

        #endregion
        public bool AddNotification(long UserId, string UserName, long QuotesId)
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
                notification.Text = "User " + UserName + " Reject the Quotes Price you have offered";

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
        #region Reminder Payment
        public bool ReminderPayment(long userId)
        {
            bool status = false;
            var userData = _db.Users.Where(x => x.UserId == userId).FirstOrDefault();
            string message = "You Job will be complete after 1 hour, Please pay you amount for Service Thanks!";
            Common.PushNotification(userData.UserType, userData.DeviceId, userData.DeviceToken, message);

            return status;
        }
        #endregion

        #region RoomServicePrice
        public Property RoomServicePrice(long propertyId)
        {
            var data = _db.Properties.Where(x => x.Id == propertyId).FirstOrDefault();
            return data;
        }

        public Property GetProperty(long propertyId)
        {
            var data = _db.Properties.Where(x => x.Id == propertyId).FirstOrDefault();
            return data;
        }

        #endregion

        public List<JobRequestPaymentDetail> GetAllOrders(long userId)
        {
            //AllOrderViewModel allData = new AllOrderViewModel();
            List<JobRequestPaymentDetail> lstData = new List<JobRequestPaymentDetail>();
            try
            {
                var data = _db.JobRequests.Where(x => x.UserId == userId).OrderByDescending(a => a.Id).ToList();
                if (data != null)
                {
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            var getUserData = _db.Users.Where(a => a.UserId == item.UserId).FirstOrDefault();
                            JobRequestPaymentDetail viewModel = new JobRequestPaymentDetail();
                            viewModel.CustomerName = getUserData != null ? getUserData.FullName : string.Empty;
                            viewModel.CustomerId = item.UserId;
                            viewModel.StartDateTime = item.CreatedDate;
                            viewModel.FastOrderName = item.FastOrderName;
                            viewModel.IsFastOrder = item.IsFastOrder;
                            viewModel.JobDesc = item.Description;
                            viewModel.Id = item.Id;
                            viewModel.JobDate = item.CreatedDate.HasValue ? item.CreatedDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                            //viewModel.JobStatus = item.JobStatus;    
                            var jobreqPropservice = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == item.Id).FirstOrDefault();
                            if (jobreqPropservice != null)
                            {
                                var Property = _db.Properties.Where(x => x.Id == jobreqPropservice.PropertyId).FirstOrDefault();
                                viewModel.PropertyName = Property != null ? Property.Name : "";
                            }

                            lstData.Add(viewModel);
                        }
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                lstData = null;
                message = ex.Message;
            }
            return lstData;
        }
        public bool SaveCheckList(long UserId, long PropId, int ServiceId, int SubCatId, int SubSubCatId, string ChecklistName, List<CheckList> Chklist)
        {
            bool status = false;
            try
            {
                tblSavedChecklist obj = new tblSavedChecklist();
                obj.UserId = UserId;
                obj.PropertyId = PropId;
                obj.ServiceId = ServiceId;
                obj.SubServiceId = SubCatId;
                obj.SubSubServiceId = SubSubCatId;
                obj.ChecklistName = ChecklistName;
                _db.tblSavedChecklists.Add(obj);
                _db.SaveChanges();
                if (Chklist.Count > 0)
                {
                    foreach (var item in Chklist)
                    {
                        tblSavedChecklistDetail objChecklist = new tblSavedChecklistDetail();

                        objChecklist.ChecklistId = obj.Id;
                        objChecklist.ChecklistText = item.CheckListText;
                        _db.tblSavedChecklistDetails.Add(objChecklist);
                        _db.SaveChanges();
                    }
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

        public List<tblSavedChecklist> GetChecklist(long UserId, long PropId, int ServiceId, int SubCatId, int SubSubCatId)
        {
            List<tblSavedChecklist> obj = new List<tblSavedChecklist>();
            try
            {
                if (SubCatId != 0)
                {
                    obj = _db.tblSavedChecklists.Where(x => x.UserId == UserId && x.ServiceId == ServiceId && x.SubServiceId == SubCatId).ToList();
                }
                else
                {
                    obj = _db.tblSavedChecklists.Where(x => x.UserId == UserId && x.ServiceId == ServiceId && x.SubSubServiceId == SubSubCatId).ToList();
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                obj = new List<tblSavedChecklist>();
                message = ex.Message;
            }
            return obj;
        }
        public List<tblSavedChecklistDetail> GetChecklistText(long ChklistId)
        {
            List<tblSavedChecklistDetail> obj = new List<tblSavedChecklistDetail>();
            try
            {
                obj = _db.tblSavedChecklistDetails.Where(x => x.ChecklistId == ChklistId).ToList();
                message = Resource.success;
            }
            catch (Exception ex)
            {
                obj = new List<tblSavedChecklistDetail>();
                message = ex.Message;
            }
            return obj;
        }

        public EditJobRequestViewModel GetJobDetail(long jobId, long jobReqPropServiceId)
        {
            EditJobRequestViewModel objEditJob = new EditJobRequestViewModel();
            try
            {
                objEditJob = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == jobId).Select(x => new EditJobRequestViewModel()
                {
                    Property_List_Id = (long)x.PropertyId,
                    JobStartDateTime = x.StartDateTime,
                    JobEndDateTime = x.EndDateTime,
                    AssignWorker = x.AssignedWorker,
                    JobStatus = x.JobStatus,
                }).FirstOrDefault();
                var JobDescription = _db.JobRequests.Where(x => x.Id == jobId).Select(x => x.Description).FirstOrDefault();
                if (JobDescription != null)
                {
                    objEditJob.JobDesc = JobDescription;
                }
            }
            catch (Exception ex)
            {
                objEditJob = new EditJobRequestViewModel();
            }
            return objEditJob;
        }

        public bool EditjobDetails(EditJobRequestViewModel model)
        {
            bool status = false;
            try
            {
                var data = _db.JobRequests.Where(x => x.Id == model.JobReqId).FirstOrDefault();
                if (data != null)
                {
                    data.Description = model.JobDesc;
                    _db.SaveChanges();
                }

                var dataJobRequestProp = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == model.JobReqId).FirstOrDefault();
                if (model.JobEndDateTime != null)
                {
                    dataJobRequestProp.EndDateTime = model.JobEndDateTime;
                    _db.SaveChanges();
                }
                tblEditJobRequest objEditJob = new tblEditJobRequest();
                objEditJob.JobRequestId = model.JobReqId;
                objEditJob.NewServiceDateTime = model.JobStartDateTime;
                objEditJob.OldServiceDateTime = dataJobRequestProp.StartDateTime;
                objEditJob.NewPropertyId = model.Property_List_Id;
                objEditJob.OldPropertyId = dataJobRequestProp.PropertyId;
                objEditJob.UserId = model.UserId;
                objEditJob.AssignWorker = model.AssignWorker;
                objEditJob.IsApproved = false;
                _db.tblEditJobRequests.Add(objEditJob);
                _db.SaveChanges();
                if (model.InventoryList != null)
                {
                    AddEditInventory(model.InventoryList);
                }
                //--------------Send Notification to admin------------------
                Notification _notification = new Notification();
                _notification.CreatedDate = DateTime.Now;
                _notification.FromUserId = model.UserId;
                _notification.IsActive = true;
                _notification.JobRequestId = model.JobReqId;
                _notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                _notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                _notification.Text = model.UserName + "has  Edit a job Request";
                _db.Notifications.Add(_notification);
                _db.SaveChanges();

                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType && A.IsActive == true).ToList();
                if (getSupervisors.Count > 0)
                {
                    foreach (var supervisor in getSupervisors)
                    {
                        _notification.ToUserId = supervisor.UserId;
                        _db.Notifications.Add(_notification);
                        _db.SaveChanges();
                    }
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
        public bool AddEditInventory(List<InventoryViewModel> obj)
        {
            bool result = false;
            try
            {
                foreach (var item in obj)
                {
                    if (item.JobReqPropServiceId != 0)
                    {
                        long JobReqPropServiceId = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == item.JobReqPropServiceId).Select(x => x.JobRequestPropId).FirstOrDefault();
                        item.JobReqPropServiceId = JobReqPropServiceId;
                        var data = _db.JobInventories.Where(x => x.JobRequestId == item.JobReqPropServiceId && x.InventoryId == item.InventoryId).FirstOrDefault();
                        if (data != null)
                        {
                            data.Qty = item.Qty;
                            _db.SaveChanges();
                        }
                        else
                        {
                            JobInventory jobInventory = new JobInventory();
                            var jobdata = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == item.JobReqPropServiceId).FirstOrDefault();
                            if (jobdata != null)
                            {
                                jobInventory.JobRequestId = jobdata.JobRequestPropId;
                            }
                            jobInventory.InventoryId = item.InventoryId;
                            jobInventory.Qty = item.Qty;
                            jobInventory.PropertyId = item.PropertyId;
                            _db.JobInventories.Add(jobInventory);
                            _db.SaveChanges();
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
        public string GetUserNameById(long UserId)
        {
            var UserName = "";
            var UserData = _db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
            if (UserData != null)
            {
                UserName = UserData.FullName != null ? UserData.FullName : UserData.CompanyName;
            }
            return UserName;
        }
        public bool AddInventoryRequest(string InventoryName, long UserId, string UserName)
        {
            bool status = false;
            try
            {
                tblAddInventoryRequest obj = new tblAddInventoryRequest();
                obj.UserId = UserId;
                obj.InventoryName = InventoryName;
                obj.CreateDate = DateTime.Now;
                _db.tblAddInventoryRequests.Add(obj);
                _db.SaveChanges();

                Notification _notification = new Notification();
                _notification.CreatedDate = DateTime.Now;
                _notification.FromUserId = UserId;
                _notification.IsActive = true;
                _notification.JobRequestId = 0;
                _notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                _notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                _notification.Text = UserName + " Request for Add Inventory " + InventoryName;
                _db.Notifications.Add(_notification);
                _db.SaveChanges();
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
        public List<AddServiceRequestViewModel> GetAddServiceRequests(long UserId)
        {
            List<AddServiceRequestViewModel> obj = new List<AddServiceRequestViewModel>();
            try
            {
                obj = _db.tblAddServiceRequests.Where(x => x.UserId == UserId && x.IsAccepted == false).Select(x => new AddServiceRequestViewModel()
                {

                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    SubCatId = x.SubCatId,
                    SubSubCatId = x.SubSubCatId,
                    PropertId = x.PropertId,
                    UserId = x.UserId,
                    JobStartDateTime = x.JobStartDateTime,
                    AssignWorker = x.AssignWorker,
                    Message = x.Message,
                    IsAccepted = x.IsAccepted,
                    ServicePrice = x.ServicePrice != null ? x.ServicePrice : 0
                }).ToList();
                foreach (var data in obj)
                {
                    var dd = _db.Categories.Where(x => x.Id == data.CategoryId).FirstOrDefault();
                    if (dd != null)
                    {
                        data.ServiceName = dd.Name;
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                obj = new List<AddServiceRequestViewModel>();
                message = ex.Message;
            }
            return obj;
        }
        public bool AcceptAddServiceRequest(long ServiceReqId, long UserId)
        {
            bool status = false;
            try
            {
                var obj = _db.tblAddServiceRequests.Where(x => x.Id == ServiceReqId).FirstOrDefault();
                obj.IsAccepted = true;
                _db.SaveChanges();

                JobRequest requestlist = new JobRequest();
                requestlist.UserId = obj.UserId;
                requestlist.Description = obj.Message;
                requestlist.CreatedDate = DateTime.Now;
                requestlist.IsFastOrder = false;
                requestlist.PaymentMethod = 0;
                requestlist.IsPaymentDone = false;
                requestlist.RepeatService = 0;
                requestlist.QuotePrice = obj.ServicePrice;
                _db.JobRequests.Add(requestlist);
                _db.SaveChanges();

                PropertyJobRequest propertyJobRequest = new PropertyJobRequest();
                propertyJobRequest.PropertyId = obj.PropertId;
                propertyJobRequest.JobRequestId = requestlist.Id;
                _db.PropertyJobRequests.Add(propertyJobRequest);
                _db.SaveChanges();

                JobRequestPropertyService jobRequestPropertyService = new JobRequestPropertyService();
                jobRequestPropertyService.JobRequestId = requestlist.Id;
                jobRequestPropertyService.ServiceId = obj.SubSubCatId != 0 ? obj.SubSubCatId : obj.SubCatId;
                jobRequestPropertyService.PropertyId = obj.PropertId;
                jobRequestPropertyService.StartDateTime = Convert.ToDateTime(obj.JobStartDateTime);
                jobRequestPropertyService.TimeToDo = 0;
                jobRequestPropertyService.type = obj.SubSubCatId != 0 ? true : false;
                jobRequestPropertyService.AssignedWorker = obj.AssignWorker;
                jobRequestPropertyService.JobStatus = (int)Enums.RequestStatus.Pending;
                _db.JobRequestPropertyServices.Add(jobRequestPropertyService);
                _db.SaveChanges();

                JobRequestSubSubCategory jobRequestSubCategory = new JobRequestSubSubCategory();
                jobRequestSubCategory.JobRequestId = jobRequestPropertyService.JobRequestPropId;
                jobRequestSubCategory.SubSubCategoryId = obj.SubSubCatId;
                jobRequestSubCategory.CategoryId = obj.CategoryId;
                jobRequestSubCategory.SubCategoryId = obj.SubCatId;
                _db.JobRequestSubSubCategories.Add(jobRequestSubCategory);
                _db.SaveChanges();
                var UserName = "";
                var UserEmail = "";
                var data = _db.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                if (data != null)
                {
                    UserName = data.FullName;
                    UserEmail = data.Email;
                }
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = UserId;
                notification.IsActive = true;
                notification.JobRequestId = 0;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                notification.Text = "User " + UserName + " Accept  Add Service Request you have offered";
                _db.Notifications.Add(notification);
                _db.SaveChanges();
                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType && A.IsActive == true).ToList();
                if (getSupervisors.Count > 0)
                {
                    foreach (var supervisor in getSupervisors)
                    {
                        notification.ToUserId = supervisor.UserId;
                        _db.Notifications.Add(notification);
                        _db.SaveChanges();
                    }
                }
                message = Resource.success;
                status = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                status = false;
            }
            return status;
        }

        public bool RejectAddServiceRequest(long ServiceReqId, long UserId)
        {
            bool status = false;
            try
            {
                var obj = _db.tblAddServiceRequests.Where(x => x.Id == ServiceReqId).FirstOrDefault();
                obj.IsAccepted = false;
                _db.SaveChanges();

                var UserName = _db.Users.Where(x => x.UserId == UserId).Select(x => x.FullName).FirstOrDefault();
                if (UserName == null)
                {
                    UserName = "";
                }
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = UserId;
                notification.IsActive = true;
                notification.JobRequestId = 0;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                notification.Text = "User " + UserName + " Reject the Add Service Request you have offered";
                _db.Notifications.Add(notification);
                _db.SaveChanges();
                // send notification to supervisor
                var supervisorType = Enums.UserTypeEnum.Supervisor.GetHashCode();
                var getSupervisors = _db.Users.Where(A => A.UserType == supervisorType && A.IsActive == true).ToList();
                if (getSupervisors.Count > 0)
                {
                    foreach (var supervisor in getSupervisors)
                    {
                        notification.ToUserId = supervisor.UserId;
                        _db.Notifications.Add(notification);
                        _db.SaveChanges();
                    }
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

        public long GetDeliveryGuy(string JobstartDate, int DayofWeek, string JobTime)
        {
            try
            {
                //var service_provider_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();
                var delivery_guy_type = Enums.UserTypeEnum.DeliveryGuy.GetHashCode();

                // get workers whose schedule is avail for the service day of week
                List<GetWorkers> getWorkersWhereDayAvailable = new List<GetWorkers>();

                getWorkersWhereDayAvailable = (from avialableSchedule in _db.tblProviderAvailableTimes
                                               join workers in _db.Users
                                               on avialableSchedule.provider_id equals workers.UserId
                                               where avialableSchedule.day_of_week == DayofWeek && workers.UserType == delivery_guy_type && (workers.standby == null || workers.standby == false)
                                               && avialableSchedule.isCausallOff.Value == false && avialableSchedule.isOptionalOff.Value == false

                                               select new GetWorkers
                                               {
                                                   Day = avialableSchedule.day_of_week,
                                                   ToTime = avialableSchedule.to_time,
                                                   FromTime = avialableSchedule.from_time,
                                                   UserId = workers.UserId,
                                                   WorkerType = workers.WorkerQuesType
                                               }).OrderBy(x => x.WorkerType).ToList();
                if (getWorkersWhereDayAvailable.Count > 0)
                {
                    // get workers which are having time match to service time
                    foreach (var item in getWorkersWhereDayAvailable)
                    {
                        var WorkerFromTime = string.Empty;
                        var WorkerToTime = string.Empty;

                        if (item.FromTime == null || item.FromTime == "")
                        {
                            WorkerFromTime = "08"; //"09:00";
                            WorkerToTime = "17"; //"18:00";
                        }
                        else
                        {
                            var worker_from_time = TimeSpan.Parse(item.FromTime);
                            var worker_to_time = TimeSpan.Parse(item.ToTime);
                            var from = worker_from_time.ToString().Split(':');
                            var to = worker_to_time.ToString().Split(':');
                            WorkerFromTime = from[0];
                            WorkerToTime = to[0];
                        }
                        DateTime resultCurrent = DateTime.ParseExact(JobstartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                        DateTime resultAfter = resultCurrent.AddDays(1);
                        //var startTime = JobTime.ToString("HH:mm");
                        var startTime = JobTime;
                        var jobStartTime = TimeSpan.Parse(JobTime);
                        // remove workers that are already occupied at that time
                        var getOcuupiedProvidersList = _db.OccupiedProviders.Where(a => a.ProviderId == item.UserId && a.JobDateTime >= resultCurrent && a.JobDateTime <= resultAfter).ToList();

                        if (getOcuupiedProvidersList.Count == 0) // when worker does't have any service on a day
                        {
                            message = Resource.success;
                            return item.UserId;
                        }
                        else if (getOcuupiedProvidersList.Count > 0) // when worker already have some services
                        {
                            for (int i = 0; i < getOcuupiedProvidersList.Count; i++)
                            {
                                var workerFromTime = TimeSpan.Parse(getOcuupiedProvidersList[i].FromTime);
                                var workerToTime = TimeSpan.Parse(getOcuupiedProvidersList[i].ToTime);

                                if (workerFromTime != jobStartTime)
                                {
                                    message = Resource.success;
                                    return (long)getOcuupiedProvidersList[i].ProviderId;
                                }
                            }
                        }
                    }
                }
                message = "Resourses Not Available";
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return 0;
            }
            return 0;
        }

        public bool AddServiceRequest(int CatId, int SubCatId, int SubSubCatId, string JobDateTime, long PropertyId, string Message, long WorkerId, long UserId, string Price)
        {
            bool status = false;
            tblAddServiceRequest obj = new tblAddServiceRequest();
            try
            {
                obj.CategoryId = CatId;
                obj.SubCatId = SubCatId;
                obj.SubSubCatId = SubSubCatId;
                obj.PropertId = PropertyId;
                obj.JobStartDateTime = JobDateTime;
                obj.Message = Message;
                obj.AssignWorker = WorkerId;
                obj.UserId = UserId;
                obj.IsAccepted = false;
                obj.ServicePrice = Price != null ? Convert.ToDecimal(Price) : 0;
                _db.tblAddServiceRequests.Add(obj);
                _db.SaveChanges();

                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                notification.IsActive = true;
                notification.JobRequestId = 0;
                notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                notification.ToUserId = UserId;
                notification.Text = "Worker Send You a Add Service Request";
                _db.Notifications.Add(notification);
                _db.SaveChanges();
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
        public SelectListItem GetPropertyById(long JobId)
        {
            // List<SelectListItem> listItems = new List<SelectListItem>();
            SelectListItem listItems = new SelectListItem();
            try
            {
                var data = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == JobId).FirstOrDefault();
                if (data != null)
                {
                    var tt = _db.Properties.Where(x => x.Id == data.PropertyId).FirstOrDefault();
                    if (tt != null)
                    {
                        listItems.Text = tt.Name;
                        listItems.Value = tt.Id != null ? tt.Id.ToString() : "0";
                    }
                    //.Select(x => new SelectListItem
                    //{
                    //    Text = x.Name,
                    //    Value = x.Id
                    //}).ToList();
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                //listItems = new List<SelectListItem>();
                listItems = new SelectListItem();
                message = ex.Message;
            }
            return listItems;
        }
        public string GetJobDateByJobId(long JobId)
        {
            var JobDate = "";
            try
            {
                var data = _db.JobRequestPropertyServices.Where(a => a.JobRequestId == JobId).Select(x => x.StartDateTime).FirstOrDefault();
                if (data != null)
                {
                    var dd = data.ToString().Split(' ');
                    JobDate = dd[0];
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                JobDate = "";
                message = ex.Message;
            }
            return JobDate;
        }
        public List<SelectListItem> GetCategoryList()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            try
            {
                var CatData = _db.Categories.Where(a => a.IsActive == true).ToList();
                if (CatData != null)
                {
                    foreach (var item in CatData)
                    {
                        SelectListItem objCat = new SelectListItem();
                        objCat.Text = item.Name;
                        objCat.Value = item.Id.ToString();
                        listItems.Add(objCat);
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                listItems = new List<SelectListItem>();
                message = ex.Message;
            }
            return listItems;
        }

        public Category GetCategory(long categoryId)
        {
            var category = _db.Categories.FirstOrDefault(x => x.Id == categoryId);

            return category;
        }

        public List<GetWorkerRating> GetWorkerAvgRating()
        {
            List<GetWorkerRating> objWokerRating = new List<GetWorkerRating>();
            try
            {
                var Sp = Enums.UserTypeEnum.ServiceProvider.GetHashCode();
                var Worker = Enums.UserTypeEnum.Worker.GetHashCode();
                var Data = _db.UserReviews.GroupBy(l => l.ToUserId)
                        .Select(x => new GetWorkerRating
                        {
                            Rating = x.Sum(c => c.Rating),
                            TotalService = x.Count(),
                            UserId = x.FirstOrDefault().ToUserId
                        }).ToList();

                foreach (var item in Data)
                {
                    var data = _db.Users.Where(x => x.UserId == item.UserId && (x.UserType == Sp || x.UserType == Worker)).FirstOrDefault();
                    if (data != null)
                    {
                        GetWorkerRating obj = new GetWorkerRating();
                        obj.Rating = item.Rating / item.TotalService;
                        obj.TotalService = item.TotalService;
                        obj.UserId = item.UserId;
                        if (data.PicturePath != null)
                        {
                            obj.Image = "~/Images/User/" + data.PicturePath;
                        }
                        else
                        {
                            obj.Image = "~/Content/images/image_placeholder.jpg";
                        }
                        obj.Name = data.FullName;
                        objWokerRating.Add(obj);
                    }
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                objWokerRating = new List<GetWorkerRating>();
                message = ex.Message;
            }
            return objWokerRating;
        }

        public bool AddInventory(List<InventoryViewModel> obj)
        {
            bool result = false;
            try
            {
                foreach (var item in obj)
                {
                    if (item.JobReqPropServiceId != 0)
                    {
                        var data = _db.JobInventories.Where(x => x.JobRequestId == item.JobReqPropServiceId && x.InventoryId == item.InventoryId).FirstOrDefault();
                        if (data != null)
                        {
                            data.Qty = item.Qty;
                            _db.SaveChanges();
                        }
                        else
                        {
                            JobInventory jobInventory = new JobInventory();
                            var jobdata = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == item.JobReqPropServiceId).FirstOrDefault();
                            if (jobdata != null)
                            {
                                jobInventory.JobRequestId = jobdata.JobRequestPropId;
                            }
                            jobInventory.InventoryId = item.InventoryId;
                            jobInventory.Qty = item.Qty;
                            jobInventory.PropertyId = item.PropertyId;
                            _db.JobInventories.Add(jobInventory);
                            _db.SaveChanges();

                            if (jobdata != null)
                            {
                                var getTime = Convert.ToDateTime(jobdata.StartDateTime).ToString("HH:mm");
                                var time = Convert.ToDouble(jobdata.TimeToDo);
                                var t1 = TimeSpan.Parse(getTime);
                                var t2 = TimeSpan.FromHours(time);
                                var busyTime = t1 + t2;

                                OccupiedProvider occupiedDeleveryGuy = new OccupiedProvider();
                                occupiedDeleveryGuy.FromTime = getTime;
                                occupiedDeleveryGuy.ToTime = busyTime.ToString();
                                occupiedDeleveryGuy.JobDateTime = jobdata.StartDateTime;
                                occupiedDeleveryGuy.JobPropertyServiceId = jobdata.JobRequestPropId;
                                occupiedDeleveryGuy.ProviderId = obj[0].DeliveryGuyId;
                                occupiedDeleveryGuy.IsDeliveryGuy = true;
                                _db.OccupiedProviders.Add(occupiedDeleveryGuy);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
                message = Resource.success;
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                message = ex.Message;
            }
            return result;
        }

        public List<DeliveryGuyDetail> GetDeliveryGuyDetailById(long Id)
        {
            List<DeliveryGuyDetail> data = new List<DeliveryGuyDetail>();
            try
            {
                data = _db.OccupiedProviders.Where(x => x.ProviderId == Id && x.IsDeliveryGuy == true).Select(x => new DeliveryGuyDetail()
                {
                    JobReqPropServiceId = x.JobPropertyServiceId,
                    DeliveryGuyId = x.ProviderId,
                    JobDateTime = x.JobDateTime,
                    FromTime = x.FromTime,
                    ToTime = x.ToTime,
                    IsDeliveryDone = x.IsDeliveryDone != null ? x.IsDeliveryDone : false
                }).ToList();

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        var JobDetail = _db.JobRequestPropertyServices.Where(x => x.JobRequestPropId == (long)item.JobReqPropServiceId).FirstOrDefault();
                        if (JobDetail != null)
                        {
                            item.PropertyId = JobDetail.PropertyId;
                            item.WorkerId = JobDetail.AssignedWorker;
                        }
                        item.DeliveryGuyName = _db.Users.Where(x => x.UserId == item.DeliveryGuyId).Select(a => a.FullName).FirstOrDefault();
                        var WorkerDetail = _db.Users.Where(x => x.UserId == item.WorkerId).FirstOrDefault();
                        if (WorkerDetail != null)
                        {
                            item.WorkerName = WorkerDetail.FullName;
                            if (WorkerDetail.CountryCode != null && WorkerDetail.PhoneNumber != null)
                            {
                                item.WorkerContactNo = WorkerDetail.CountryCode + WorkerDetail.PhoneNumber;
                            }
                            else if (WorkerDetail.PhoneNumber != null)
                            {
                                item.WorkerContactNo = WorkerDetail.PhoneNumber;
                            }
                        }
                        var PropertyDetail = _db.Properties.Where(x => x.Id == (long)item.PropertyId).FirstOrDefault();
                        if (PropertyDetail != null)
                        {
                            item.PropertyName = PropertyDetail.Name;
                            item.PropertyAddress = PropertyDetail.Address;
                            item.Latitude = PropertyDetail.Latitude != null ? PropertyDetail.Latitude.ToString() : "";
                            item.Longitude = PropertyDetail.Longitude != null ? PropertyDetail.Longitude.ToString() : "";
                        }
                        List<DeliveryGuyInventory> objInventory = new List<DeliveryGuyInventory>();
                        objInventory = _db.JobInventories.Where(x => x.JobRequestId == item.JobReqPropServiceId).Select(x => new DeliveryGuyInventory()
                        {

                            InventoryId = x.InventoryId,
                            Qty = x.Qty,
                            PropertyId = x.PropertyId
                        }).ToList();
                        if (objInventory != null)
                        {
                            foreach (var inventorydata in objInventory)
                            {
                                var dd = _db.Inventories.Where(a => a.InventoryId == inventorydata.InventoryId).FirstOrDefault();
                                inventorydata.InventoryName = dd.Name;
                                inventorydata.Image = dd.Image;
                            }
                        }
                        item.Inventory = objInventory;
                    }
                    message = Resource.success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                data = new List<DeliveryGuyDetail>();
            }
            return data;
        }
        public bool IsDeliveryDone(long DeliveryGuyid, long JobReqPropServiceId)
        {
            try
            {
                var data = _db.OccupiedProviders.Where(x => x.ProviderId == DeliveryGuyid && x.JobPropertyServiceId == JobReqPropServiceId).FirstOrDefault();
                if (data != null)
                {
                    data.IsDeliveryDone = true;
                    _db.SaveChanges();

                    var DeliveryGuyName = _db.Users.Where(x => x.UserId == data.ProviderId).Select(a => a.FullName).FirstOrDefault();
                    Notification notification = new Notification();
                    notification.CreatedDate = DateTime.Now;
                    notification.FromUserId = data.ProviderId;
                    notification.IsActive = true;
                    notification.JobRequestId = 0;
                    notification.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
                    notification.ToUserId = Enums.UserTypeEnum.Admin.GetHashCode();
                    notification.Text = "DeliveryGuy " + DeliveryGuyName + "Successfully Delivered Items";
                    _db.Notifications.Add(notification);
                    _db.SaveChanges();
                }
                message = Resource.success;
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public PropertyModelData getCheckInList(long Id)
        {
            try
            {
                long? PropertyId;
                var res = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == Id).FirstOrDefault();
                if (res != null)
                {
                    res.JobStatus = 2;
                    _db.SaveChanges();
                }
                PropertyId = Convert.ToInt64(res.PropertyId);
                var Result = _db.Properties.Where(x => x.Id == PropertyId).FirstOrDefault();
                PropertyModelData obj = new PropertyModelData();
                obj.PropertyId = Result.Id;
                obj.Latitude = Result.Latitude;
                obj.Longitude = Result.Longitude;
                return obj;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public bool updateCheckout(long Id)
        {
            bool status = false;

            try
            {
                long? PropertyId;
                var res = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == Id).FirstOrDefault();
                PropertyId = Convert.ToInt64(res.PropertyId);
                if (res != null)
                {
                    res.JobStatus = 3;
                    _db.SaveChanges();

                    var Result = _db.Properties.Where(x => x.Id == PropertyId).FirstOrDefault();
                    if (Result != null)
                    {
                        Random generator = new Random();
                        int r = generator.Next(1, 1000000);
                        string randomNo = r.ToString().PadLeft(6, '0');
                        Result.AccessToProperty = randomNo;
                        _db.SaveChanges();
                    }
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
        public SubUserPropDetail GetPropertyDetail(long PropId)
        {
            SubUserPropDetail obj = new SubUserPropDetail();
            try
            {
                var data = _db.Properties.Where(x => x.Id == PropId && x.ShortTermApartment == true).FirstOrDefault();
                if (data != null)
                {
                    var Pass = "";
                    var UserData = _db.Users.Where(x => x.UserId == data.SubUserId).FirstOrDefault();
                    if (UserData != null)
                    {
                        Pass = Common.DecryptString(UserData.Password, UserData.PasswordSalt);
                    }
                    var CompanyName = _db.Users.Where(x => x.UserId == data.CreatedBy).Select(x => x.CompanyName).FirstOrDefault();
                    obj.LoginId = UserData.Email;
                    obj.Password = Pass;
                    obj.CompanyName = CompanyName != null ? CompanyName : "";
                    obj.Address = data.Address != null ? data.Address : "";
                    obj.ApartmentNumber = data.ApartmentNumber != null ? data.ApartmentNumber : 0;
                    obj.FloorNumber = data.FloorNumber != null ? data.FloorNumber : 0;
                    obj.BuildingCode = data.BuildingCode != null ? data.BuildingCode : "";
                    obj.AccessToProperty = data.AccessToProperty != null ? data.AccessToProperty : "";
                    obj.WifiLogin = data.WifiLogin != null ? data.WifiLogin : "";
                    obj.LocationOfKey = data.LocationOfKey != null ? data.LocationOfKey : "";
                    obj.SubUserId = data.SubUserId;
                    obj.status = true;
                }
            }
            catch (Exception ex)
            {
                obj = new SubUserPropDetail();
            }
            return obj;
        }

        private int GetWeekNumber(DateTime vDate)
        {

            // Gets the Calendar instance associated with a CultureInfo.
            CultureInfo myCI = new CultureInfo("he-IL");
            Calendar myCal = myCI.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            return myCal.GetWeekOfYear(vDate, myCWR, myFirstDOW);
        }

        public bool CancelRefund(long jobRequestId, long userId)
        {
            try
            {
                // first get the jobRequestId
                var jobRequest = _db.JobRequests.FirstOrDefault(x => x.Id == jobRequestId);
                // check the jobRequest Status Pending
                if (jobRequest == null) return false;
                // check the jobRequest user
                if (jobRequest.UserId != userId) return false;
                // check if the order created date is greater than 24 hours
                var hours = Math.Abs((jobRequest.CreatedDate - DateTime.Now).Value.TotalHours);
                var jobUser = _db.Users.FirstOrDefault(x => x.UserId == userId);
                var jobReqProp = _db.JobRequestPropertyServices.FirstOrDefault(x => x.JobRequestId == jobRequestId);
                var pendingStatus = Enums.RequestStatus.Pending.GetHashCode();
                // 
                if (hours >= 24 && jobReqProp.StartDateTime < DateTime.Now && jobReqProp.JobStatus == pendingStatus)
                {
                    // then make 50% refund.
                    // add the balance to the user account
                    // change the jobRequest status to cancelled.
                    var servicePrice = jobRequest.ServicePrice.Value;
                    decimal refundAmount = (servicePrice * 50) / 100;
                    jobUser.Balance += refundAmount;
                    // refund and free booked worker
                }

                var bookedWorker = _db.OccupiedProviders.FirstOrDefault(x => x.JobPropertyServiceId == jobReqProp.JobRequestPropId);
                if (bookedWorker != null)
                {
                    _db.OccupiedProviders.Remove(bookedWorker);
                }

                jobReqProp.JobStatus = Enums.RequestStatus.Canceled.GetHashCode();

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}