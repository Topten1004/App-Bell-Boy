using BroomService.Models;
using BroomService.Resources;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.Services
{
    public class HomeService
    {
        public string message = string.Empty;
        BroomServiceEntities1 _db;

        public HomeService()
        {
            _db = new BroomServiceEntities1();
        }

        public HomeViewModel GetHomeData(long userID)
        {
            HomeViewModel data = new HomeViewModel();
            try
            {
                var categories = _db.Categories.Where(x => x.IsActive == true).ToList();
                if (categories.Count > 0)
                {
                    data.Categories = categories.Select(x => new CategoryViewModel()
                    {
                        Name = x.Name,
                        Name_French = x.Name_French,
                        Name_Hebrew = x.Name_Hebrew,
                        Name_Russian = x.Name_Russian,
                        Picture = x.Picture,
                        Description = x.Description,
                        Description_French = x.Description_French,
                        Description_Hebrew = x.Description_Hebrew,
                        Description_Russian = x.Description_Russian,
                        Id = x.Id,
                        IsActive = x.IsActive
                    }).ToList();
                }

                var getAboutUsData = _db.AboutUs.FirstOrDefault();
                if (getAboutUsData != null)
                {
                    data.AboutUsText = getAboutUsData.Text;
                }

                data.Testimonials = _db.Testimonials.Where(A => A.IsActive == true).ToList();

                var getProperties = _db.Properties.Where(A => A.CreatedBy == userID
                 && A.IsActive == true).OrderByDescending(a => a.CreatedDate).ToList();

                if (getProperties.Count > 0)
                {
                    data.Properties = new List<PropertyViewModel>();
                    if (getProperties.Count > 0)
                    {
                        for (int i = 0; i < getProperties.Count; i++)
                        {
                            var getId = getProperties[i].Id;

                            var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == getId)
                                .Select(x => new PropertyImageVM()
                                {
                                    PropertyId = x.PropertyId,
                                    CreatedDate = x.CreatedDate,
                                    Id = x.Id,
                                    ImageUrl = x.ImageUrl,
                                    IsImage = x.IsImage,
                                    IsVideo = x.IsVideo,
                                    VideoThumbnail = x.VideoThumbnail,
                                    VideoUrl = x.VideoUrl
                                }).ToList();

                            data.Properties.Add(new PropertyViewModel
                            {
                                PropertyModel = new PropertyVM
                                {
                                    AccesstoCode = getProperties[i].AccesstoCode,
                                    AccessToProperty = getProperties[i].AccessToProperty,
                                    Address = getProperties[i].Address,
                                    ApartmentNumber = getProperties[i].ApartmentNumber,
                                    Balcony = getProperties[i].Balcony,
                                    BuildingCode = getProperties[i].BuildingCode,
                                    CityId = getProperties[i].CityId,
                                    CoffeeMachine = getProperties[i].CoffeeMachine,
                                    CountryId = getProperties[i].CountryId,
                                    CreatedBy = getProperties[i].CreatedBy,
                                    CreatedDate = getProperties[i].CreatedDate,
                                    Dishwasher = getProperties[i].Dishwasher,
                                    Doorman = getProperties[i].Doorman,
                                    Elevator = getProperties[i].Elevator,
                                    FloorNumber = getProperties[i].FloorNumber,
                                    Garden = getProperties[i].Garden,
                                    Id = getProperties[i].Id,
                                    IsKingBed = getProperties[i].IsKingBed,
                                    IsSingleBed = getProperties[i].IsSingleBed,
                                    IsSofaBed = getProperties[i].IsSofaBed,
                                    Latitude = getProperties[i].Latitude,
                                    Longitude = getProperties[i].Longitude,
                                    Name = getProperties[i].Name,
                                    NoOfBathrooms = getProperties[i].NoOfBathrooms,
                                    NoOfBedRooms = getProperties[i].NoOfBedRooms,
                                    NoOfDoubleBeds = getProperties[i].NoOfDoubleBeds,
                                    NoOfDoubleSofaBeds = getProperties[i].NoOfDoubleSofaBeds,
                                    NoOfQueenBeds = getProperties[i].NoOfQueenBeds,
                                    NoOfSingleBeds = getProperties[i].NoOfSingleBeds,
                                    NoOfSingleSofaBeds = getProperties[i].NoOfSingleSofaBeds,
                                    NoOfToilets = getProperties[i].NoOfToilets,
                                    Parking = getProperties[i].Parking,
                                    Pool = getProperties[i].Pool,
                                    ShortTermApartment = getProperties[i].ShortTermApartment,
                                    Size = getProperties[i].Size,
                                    Type = getProperties[i].Type,
                                    WifiLogin = getProperties[i].WifiLogin
                                },
                                PropertyImages = getPropertyImages
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return data;

        }
        public MondayPopupDetail GetServiceData(long UserId)
        {
            MondayPopupDetail obj = new MondayPopupDetail();
            try
            {
                var data = _db.JobRequests.Where(x => x.UserId == UserId).ToList();
                if (data != null)
                {
                    var date = DateTime.Now;
                    foreach (var item in data)
                    {
                        var dd = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == item.Id && x.StartDateTime >= date).FirstOrDefault();
                        if (dd != null)
                        {
                            obj.ServiceId = dd.ServiceId;
                            obj.PropId = dd.PropertyId;
                            obj.IsUpcommingService = true;
                            obj.JobId = dd.JobRequestId;
                            obj.JobReqPropServiceId = dd.JobRequestPropId;
                            break;
                        }
                    }
                    if (obj != null)
                    {
                        var CatId = _db.JobRequestSubSubCategories.Where(x => x.JobRequestId == obj.JobReqPropServiceId).FirstOrDefault();
                        if(CatId != null)
                        {
                            obj.ServiceName = _db.Categories.Where(x => x.Id == CatId.CategoryId).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                }
                var IsLater = _db.tblAskForServices.Where(x => x.UserId == UserId).Select(x => x.IsLater).FirstOrDefault();
                if (IsLater != null)
                {
                    obj.Later = IsLater;
                }
                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                obj = new MondayPopupDetail();
            }
            return obj;
        }
        public bool Cancel(long UserId)
        {
            bool status = false;
            try
            {
                var data = _db.tblAskForServices.Where(x => x.UserId == UserId).FirstOrDefault();
                if (data != null)
                {
                    data.IsLater = false;
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
        public bool AskLater(long UserId)
        {
            bool status = false;
            try
            {
                var data = _db.tblAskForServices.Where(x => x.UserId == UserId).FirstOrDefault();
                if (data != null)
                {
                    data.IsLater = true;
                    _db.SaveChanges();
                }
                else
                {
                    tblAskForService objservice = new tblAskForService();
                    objservice.UserId = UserId;
                    objservice.IsLater = true;
                    _db.tblAskForServices.Add(objservice);
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
        public bool BookServiceNextWeek(long UserId, long JobId)
        {
            DateTime CurrentDate = DateTime.Now;
            var Bookingdate = CurrentDate.AddDays(7).ToShortDateString();
            bool status = false;
            try
            {
                var data = _db.tblAskForServices.Where(x => x.UserId == UserId).FirstOrDefault();
                if (data != null)
                {
                    data.IsLater = false;
                    _db.SaveChanges();
                }
               var JobData= _db.JobRequestPropertyServices.Where(x => x.JobRequestId == JobId).FirstOrDefault();
                if(JobData != null)
                {
                    var dd = JobData.StartDateTime.ToString().Split(' ');
                    var NewDatetime= Bookingdate + " " + dd[1]+" "+ dd[2];
                    JobData.StartDateTime = Convert.ToDateTime(NewDatetime);
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
    }
}