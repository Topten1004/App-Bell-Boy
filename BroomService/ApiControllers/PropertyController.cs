using BroomService.Models;
using BroomService.Services;
using BroomService.ViewModels;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;

namespace BroomService.ApiControllers
{
    public class PropertyController : ApiController
    {
        BroomServiceEntities1 _db;
        PropertyService propertyService;

        public PropertyController()
        {
            propertyService = new PropertyService();
            _db = new BroomServiceEntities1();
        }

        #region Properties List

        /// <summary>
        /// Get Property by UserId Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPropertiesByUserId(long userId)
        {
            var result = propertyService.GetPropertiesByUserId(userId,0);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = propertyService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult GetGrantAccessProperties(long userId)
        {
            var result = propertyService.GetPropertiesForGrantAccess(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = propertyService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult GetPropertiesForSubUser(long userId)
        {
            var result = propertyService.GetPropertiesForSubUser(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = propertyService.message,
                data = result
            });
        }

        #endregion

        #region Add/Update Property

        [HttpPost]
        public IHttpActionResult AddUpdateProperty()
        {
            Property model = new Property();

            List<tblPropertyImage> propertyImages = new List<tblPropertyImage>();
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

                            IList<string> AllowedImageFileExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".JPG", ".PNG", ".JPEG" };

                            IList<string> AllowedVideoFileExtensions = new List<string> { ".mp4", ".3gp", ".flv", ".wmv", ".mov", ".MP4", ".3GP", ".FLV", ".WMV", ".MOV" };


                            var path = "~/Images/Property/";

                            var imagePath = Path.GetFileNameWithoutExtension(postedImg.FileName) + "_" + date + ext.ToLower();
                            var fileName = path + imagePath;
                            

                            if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                            postedImg.SaveAs(HttpContext.Current.Server.MapPath(fileName));

                            if (AllowedImageFileExtensions.Contains(ext))
                            {
                                propertyImages.Add(new tblPropertyImage
                                {
                                    CreatedDate = DateTime.Now,
                                    IsImage = true,
                                    ImageUrl = imagePath
                                });
                            }
                            else if(AllowedVideoFileExtensions.Contains(ext))
                            {
                                propertyImages.Add(new tblPropertyImage
                                {
                                    CreatedDate = DateTime.Now,
                                    IsVideo = true,
                                    VideoUrl = imagePath
                                });
                            }
                        }
                    }
                }
            }
            model.Name = httpRequest.Form.Get("Name");
            model.AccesstoCode = Convert.ToBoolean(httpRequest.Form.Get("AccesstoCode"));
            model.AccessToProperty = httpRequest.Form.Get("AccessToProperty");
            model.Address = httpRequest.Form.Get("Address");
            model.ApartmentNumber = Convert.ToInt32(httpRequest.Form.Get("ApartmentNumber"));
            model.Balcony = Convert.ToBoolean(httpRequest.Form.Get("Balcony"));
            model.BuildingCode = httpRequest.Form.Get("BuildingCode");
            model.CoffeeMachine = Convert.ToBoolean(httpRequest.Form.Get("CoffeeMachine"));
            model.CreatedBy = Convert.ToInt32(httpRequest.Form.Get("UserId"));
            model.CreatedDate = DateTime.Now;
            model.Dishwasher = Convert.ToBoolean(httpRequest.Form.Get("Dishwasher"));
            model.Doorman = Convert.ToBoolean(httpRequest.Form.Get("Doorman"));
            model.Elevator = Convert.ToBoolean(httpRequest.Form.Get("Elevator"));
            model.FloorNumber = Convert.ToInt32(httpRequest.Form.Get("FloorNumber"));
            model.Garden = Convert.ToBoolean(httpRequest.Form.Get("Garden"));
            if (httpRequest.Form.Get("Id") != null)
            {
                model.Id = Convert.ToInt32(httpRequest.Form.Get("Id"));
            }
            model.IsActive = true;
            model.IsKingBed = Convert.ToBoolean(httpRequest.Form.Get("IsKingBed"));
            model.IsSingleBed = Convert.ToBoolean(httpRequest.Form.Get("IsSingleBed"));
            model.IsSofaBed = Convert.ToBoolean(httpRequest.Form.Get("IsSofaBed"));
            model.Latitude = Convert.ToDecimal(httpRequest.Form.Get("Latitude"));
            model.Longitude =Convert.ToDecimal(httpRequest.Form.Get("Longitude"));
            model.NoOfBathrooms = Convert.ToInt32(httpRequest.Form.Get("NoOfBathrooms"));
            model.NoOfBedRooms = Convert.ToInt32(httpRequest.Form.Get("NoOfBedRooms"));
            model.NoOfDoubleBeds = Convert.ToInt32(httpRequest.Form.Get("NoOfDoubleBeds"));
            model.NoOfDoubleSofaBeds = Convert.ToInt32(httpRequest.Form.Get("NoOfDoubleSofaBeds"));
            model.NoOfQueenBeds = Convert.ToInt32(httpRequest.Form.Get("NoOfQueenBeds"));
            model.NoOfSingleBeds = Convert.ToInt32(httpRequest.Form.Get("NoOfSingleBeds"));
            model.NoOfSingleSofaBeds = Convert.ToInt32(httpRequest.Form.Get("NoOfSingleSofaBeds"));
            model.NoOfToilets = Convert.ToInt32(httpRequest.Form.Get("NoOfToilets"));
            model.Parking = Convert.ToBoolean(httpRequest.Form.Get("Parking"));
            model.Pool = Convert.ToBoolean(httpRequest.Form.Get("Pool"));
            model.ShortTermApartment = Convert.ToBoolean(httpRequest.Form.Get("ShortTermApartment"));
            model.Size = httpRequest.Form.Get("Size");
            model.WifiLogin = httpRequest.Form.Get("WifiLogin");
            model.Type = httpRequest.Form.Get("Type");

            var propertySuccessVM = propertyService.AddUpdateProperty(model, propertyImages);

            if (propertySuccessVM.PropertyId != 0)
            {
                return this.Ok(new
                {
                    status = true,
                    message = propertyService.message,
                    property_id = propertySuccessVM.PropertyId,
                    SubUserEmail = propertySuccessVM.SubUserEmail,
                    SubUserPassword = propertySuccessVM.SubUserPassword,
            });
            }
            else
            {
                return this.Ok(new
                {
                    status = false,
                    message = propertyService.message,
                    property_id = propertySuccessVM.PropertyId
                });
            }
        }

        #endregion

        #region Add Multiple Properties

        /// <summary>
        /// Method that will use to upload multiple files by excel
        /// </summary>
        /// <param name="FileUpload"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ImportPropertyByExcel(long user_id)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count > 0)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var count = httpRequest.Files.Count;
                    if (count > 0)
                    {
                        for (int i = 0; i < httpRequest.Files.Count; i++)
                        {
                            if (httpRequest.Files[i] != null)
                            {
                                var postedImg = httpRequest.Files[i];
                                var path = "~/Images/PropertyDocs/";

                                var fileName = path + postedImg.FileName;

                                if (!Directory.Exists(HttpContext.Current.Server.MapPath(path)))
                                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));

                                postedImg.SaveAs(HttpContext.Current.Server.MapPath(fileName));

                                string pathToExcelFile = HttpContext.Current.Server.MapPath(fileName);
                                string sheetName = "Worksheet";

                                var excelFile = new ExcelQueryFactory(pathToExcelFile);
                                var propertyDetails = from a in excelFile.Worksheet<PropertyExcelVM>(sheetName) select a;
                                foreach (var model in propertyDetails)
                                {
                                    Property property = new Property();
                                    // check for the required fields of properties
                                    if (!string.IsNullOrEmpty(model.Name) &&
                                        !string.IsNullOrEmpty(model.Type) &&
                                        !string.IsNullOrEmpty(model.Address) &&
                                        model.ApartmentNumber != null &&
                                        model.FloorNumber != null &&
                                        !string.IsNullOrEmpty(model.AccessToProperty) &&
                                        !string.IsNullOrEmpty(model.Country) &&
                                        !string.IsNullOrEmpty(model.City))
                                    {
                                        property.AccesstoCode = model.AccesstoCode == "Yes" ? true : model.AccesstoCode == "No" ? false : false;
                                        property.CoffeeMachine = model.CoffeeMachine == "Yes" ? true : model.CoffeeMachine == "No" ? false : false;
                                        property.Doorman = model.Doorman == "Yes" ? true : model.Doorman == "No" ? false : false;
                                        property.Parking = model.Parking == "Yes" ? true : model.Parking == "No" ? false : false;
                                        property.Pool = model.Pool == "Yes" ? true : model.Pool == "No" ? false : false;
                                        property.ShortTermApartment = model.ShortTermApartment == "Yes" ? true : model.ShortTermApartment == "No" ? false : false;
                                        property.IsKingBed = model.IsKingBed == "Yes" ? true : model.IsKingBed == "No" ? false : false;
                                        property.IsSingleBed = model.IsSingleBed == "Yes" ? true : model.IsSingleBed == "No" ? false : false;
                                        property.IsSofaBed = model.IsSofaBed == "Yes" ? true : model.IsSofaBed == "No" ? false : false;
                                        property.CreatedBy = user_id;
                                        property.Dishwasher = model.Dishwasher == "Yes" ? true : model.Dishwasher == "No" ? false : false;
                                        property.Elevator = model.Elevator == "Yes" ? true : model.Elevator == "No" ? false : false;
                                        property.Garden = model.Garden == "Yes" ? true : model.Garden == "No" ? false : false;
                                        property.IsActive = true;
                                        property.CreatedDate = DateTime.Now;
                                        property.AccessToProperty = model.AccessToProperty;
                                        property.Address = model.Address;
                                        property.ApartmentNumber = model.ApartmentNumber;
                                        property.BuildingCode = model.BuildingCode;
                                        if (!string.IsNullOrEmpty(model.Country))
                                        {
                                            var countryData = _db.Countries.Where(a => a.Name == model.Country).FirstOrDefault();
                                            if (countryData != null)
                                            {
                                                property.CountryId = countryData.CountryId;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(model.City))
                                        {
                                            var cityData = _db.Cities.Where(a => a.Name == model.City).FirstOrDefault();
                                            if (cityData != null)
                                            {
                                                property.CityId = cityData.CityId;
                                            }
                                        }
                                        property.FloorNumber = model.FloorNumber;
                                        property.Name = model.Name;
                                        property.NoOfBathrooms = model.NoOfBathrooms;
                                        property.NoOfBedRooms = model.NoOfBedRooms;
                                        property.NoOfDoubleBeds = model.NoOfDoubleBeds;
                                        property.NoOfDoubleSofaBeds = model.NoOfDoubleSofaBeds;
                                        property.NoOfQueenBeds = model.NoOfQueenBeds;
                                        property.NoOfSingleBeds = model.NoOfSingleBeds;
                                        property.NoOfSingleSofaBeds = model.NoOfSingleSofaBeds;
                                        property.NoOfToilets = model.NoOfToilets;
                                        property.Size = model.Size;
                                        property.WifiLogin = model.WifiLogin;
                                        property.Type = model.Type;
                                        var status = propertyService.AddUpdateProperty(property, null);                                        
                                    }
                                }

                                //deleting excel file from folder  
                                if ((System.IO.File.Exists(pathToExcelFile)))
                                {
                                    System.IO.File.Delete(pathToExcelFile);
                                }
                            }
                        }
                    }
                }
                return this.Ok(new
                {
                    status = true,
                    message = propertyService.message
                });
            }
            catch (Exception ex)
            {
                return this.Ok(new
                {
                    status = false,
                    message = ex.ToString()
                });
            }
        }

        #endregion

        #region Change Access Code

        [HttpGet]
        public IHttpActionResult EditAccessCodeDetails(long? propertyID, string buildingCode, string accessToProperty)
        {
            try
            {
                var status = propertyService.UpdateAccessCodeDetails(propertyID, buildingCode, accessToProperty);
                return this.Ok(new
                {
                    status = status,
                    message = propertyService.message
                });
            }
            catch (Exception ex)
            {
                return this.Ok(new
                {
                    status = false,
                    message = ex.Message.ToString()
                });
            }

        }

        #endregion
    }
}
