using BroomService.bin.Controllers.Web;
using BroomService.CustomFilter;
using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using LinqToExcel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BroomService.Controllers
{
    public class PropertyController : MyController
    {
        BroomServiceEntities1 _db;
        PropertyService propertyService;
        static int Pageval_Min = 1;
        static int Pageval_Max = 1;


        public PropertyController()
        {
            propertyService = new PropertyService();
            _db = new BroomServiceEntities1();
        }

        #region My Properties
        public async System.Threading.Tasks.Task<ActionResult> PropertyList(int? pageNumber, string type)
        {
            if (pageNumber == null)
            {
                pageNumber = 1;
            }
            int getUserId = 0;
            int UserType = 0;
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    getUserId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    UserType = Convert.ToInt32(Request.Cookies["Login"].Values["UserType"]);
                    Session["UserType"] = UserType.ToString();
                }
            }
            if (type == "decrement")
            {
                Pageval_Max = Pageval_Min;
                Pageval_Min = Pageval_Min - 5;
                if (Pageval_Min <= 0)
                {
                    Pageval_Min = 1;
                }
                if (Pageval_Max < 5)
                {
                    Pageval_Max = 5;
                }
                pageNumber = Pageval_Min;
                ViewBag.ActivePage = pageNumber;
            }
            else if (type == "increment")
            {
                Pageval_Min = Pageval_Max;
                Pageval_Max = Pageval_Max + 5;

                pageNumber = Pageval_Min;
                ViewBag.ActivePage = pageNumber;
            }
            else
            {
                Pageval_Min = Convert.ToInt32(pageNumber);
                Pageval_Max = Pageval_Min + 5;
                pageNumber = Pageval_Min;
                ViewBag.ActivePage = pageNumber;

            }
            if (getUserId != 0)
            {
                try
                {
                    List<PropertyViewModel> objUserProperty = new List<PropertyViewModel>();
                    List<PropertyViewModel> UserPropertyReturnToView = new List<PropertyViewModel>();
                    var result = propertyService.GetPropertiesByUserId(getUserId, UserType);
                    if (result.MainUserProperties != null)
                    {
                        objUserProperty = result.MainUserProperties.ToList();

                    }
                    if (result.GrantUserProperties != null)
                    {
                        foreach (var item in result.GrantUserProperties)
                        {
                            objUserProperty.Add(item);
                        }
                    }

                    //get airbnb Property data
                    //if (UserType != 7)
                    //{
                    //    var AirbnbPropertyData = await GetAirbnbProperty();
                    //    if (AirbnbPropertyData != null)
                    //    {
                    //        foreach (var item in AirbnbPropertyData.content.list)
                    //        {
                    //            PropertyViewModel obj = new PropertyViewModel();
                    //            PropertyVM tset = new PropertyVM();

                    //            tset.Name = item.name == null ? string.Empty : item.name;
                    //            tset.Latitude = (decimal)item.lat;
                    //            tset.Longitude = (decimal)item.lng;
                    //            tset.Address = item.address != null ? item.address : string.Empty;
                    //            // tset.NoOfKingBeds = item.beds;
                    //            tset.NoOfBedRooms = item.bedrooms;
                    //            tset.ShortTermApartment = true;
                    //            tset.Type = item.property_type;
                    //            obj.PropertyModel = tset;
                    //            objUserProperty.Add(obj);
                    //        }
                    //    }
                    //}

                    //end get airbnb Property data


                    var CountProperty = objUserProperty.Count / 6;
                    if (objUserProperty.Count % 6 != 0)
                    {
                        CountProperty = CountProperty + 1;
                    }
                    else
                    {
                        CountProperty = CountProperty;
                    }

                    if (Pageval_Max <= CountProperty)
                    {
                        for (int i = Pageval_Min; i <= Pageval_Max; i++)
                        {
                            ViewBag.PropertyCount = i;
                        }
                        ViewBag.Pageval_Min = Pageval_Min;
                        ViewBag.ActivePage = Pageval_Min;
                        pageNumber = Pageval_Min;
                        if (Pageval_Min >= CountProperty - 5 && Pageval_Min != 1)
                        {
                            Pageval_Min = CountProperty - 5;
                            ViewBag.Pageval_Min = Pageval_Min;
                        }
                    }
                    else
                    {
                        if (CountProperty > 0)
                        {
                            ViewBag.PropertyCount = CountProperty;
                            ViewBag.Pageval_Min = Pageval_Min > CountProperty ? CountProperty - 5 : Pageval_Min;
                            Pageval_Max = CountProperty;
                            Pageval_Min = ViewBag.Pageval_Min;
                            ViewBag.ActivePage = ViewBag.Pageval_Min;
                            pageNumber = ViewBag.Pageval_Min;
                            if (CountProperty <= 5)
                            {
                                Pageval_Min = 1;
                                ViewBag.Pageval_Min = 1;
                            }
                            else if (Pageval_Min >= CountProperty - 5)
                            {
                                Pageval_Min = CountProperty - 5;
                                ViewBag.Pageval_Min = Pageval_Min;
                            }
                        }
                        else
                        {
                            ViewBag.PropertyCount = 0;
                            Pageval_Max = Pageval_Min = 1;
                        }
                    }

                    foreach (var item in objUserProperty.Skip((Convert.ToInt32(pageNumber) - 1) * 6).Take(6))
                    {
                        UserPropertyReturnToView.Add(item);
                    }

                    return View(UserPropertyReturnToView);
                }
                catch (Exception ex)
                {
                    return RedirectToAction("AddJobRequest", "Order");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        public async System.Threading.Tasks.Task<Root> GetAirbnbProperty()
        {
            var body = "";
            Root countResponse = new Root();
            var client = new HttpClient();
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://mashvisor-api.p.rapidapi.com/airbnb-property/newly-listed?state=CA&city=San%20Francisco"),
                    Headers =
                                {
                                    { "x-rapidapi-key", "cf2337a760msh293e77d4482a676p1d827fjsn9ecf5ebf6aa7" },
                                    { "x-rapidapi-host", "mashvisor-api.p.rapidapi.com" },
                                },
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(body);
                    var jss = new JavaScriptSerializer();
                    countResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(body);
                    //countResponse = jss.Deserialize<Root>(body);
                }
                return countResponse;

            }
            catch (Exception ex)
            {
                return null;
            }
            // return Json(body, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Property Details

        public ActionResult PropertyDetails(int id)
        {

            if (id == 0)
            {
                return RedirectToAction("AddProperty", "Property");
            }
            var result = propertyService.GetPropertiesDetails(id);
            return View(result);
        }

        #endregion

        #region Add Multiple Properties

        /// <summary>  
        /// This function is used to download excel format.  
        /// </summary>  
        /// <param name="Path"></param>  
        /// <returns>file</returns>  
        public FileResult DownloadExcel()
        {
            string path = "/Content/PropertyTemplate.xlsx";
            return File(path, "application/vnd.ms-excel", "PropertyTemplate.xlsx");
        }

        /// <summary>
        /// Method that will use to upload multiple files by excel
        /// </summary>
        /// <param name="FileUpload"></param>
        /// <returns></returns>

        [HttpPost]
        [VerifyUser]
        public ActionResult UploadExcel(HttpPostedFileBase FileUpload)
        {
            long userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            try
            {
                List<long> Property_Id_List = new List<long>();
                List<string> data = new List<string>();
                if (FileUpload != null)
                {
                    if (FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = FileUpload.FileName;
                        string targetpath = Server.MapPath("~/Images/PropertyDocs/");
                        FileUpload.SaveAs(targetpath + filename);
                        string pathToExcelFile = targetpath + filename;
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
                                property.AccesstoCode = model.AccesstoCode == "Yes" ? true : model.AccesstoCode == "No" || model.AccesstoCode == null ? false : false;
                                property.CoffeeMachine = model.CoffeeMachine == "Yes" ? true : model.CoffeeMachine == "No" || model.CoffeeMachine == null ? false : false;
                                property.Doorman = model.Doorman == "Yes" ? true : model.Doorman == "No" || model.Doorman == null ? false : false;
                                property.Parking = model.Parking == "Yes" ? true : model.Parking == "No" || model.Parking == null ? false : false;
                                property.Balcony = model.Balcony == "Yes" ? true : model.Balcony == "No" || model.Balcony == null ? false : false;
                                property.Pool = model.Pool == "Yes" ? true : model.Pool == "No" || model.Pool == null ? false : false;
                                property.ShortTermApartment = model.ShortTermApartment == "Yes" ? true : model.ShortTermApartment == "No" || model.ShortTermApartment == null ? false : false;
                                property.IsKingBed = model.IsKingBed == "Yes" ? true : model.IsKingBed == "No" || model.IsKingBed == null ? false : false;
                                property.IsSingleBed = model.IsSingleBed == "Yes" ? true : model.IsSingleBed == "No" || model.IsSingleBed == null ? false : false;
                                property.IsSofaBed = model.IsSofaBed == "Yes" ? true : model.IsSofaBed == "No" || model.IsSofaBed == null ? false : false;
                                property.CreatedBy = userId;
                                property.Dishwasher = model.Dishwasher == "Yes" ? true : model.Dishwasher == "No" || model.Dishwasher == null ? false : false;
                                property.Elevator = model.Elevator == "Yes" ? true : model.Elevator == "No" || model.Elevator == null ? false : false;
                                property.Garden = model.Garden == "Yes" ? true : model.Garden == "No" || model.Garden == null ? false : false;
                                property.IsActive = true;
                                property.CreatedDate = DateTime.Now;
                                property.AccessToProperty = model.AccessToProperty;
                                property.Address = model.Address;
                                property.ApartmentNumber = model.ApartmentNumber;
                                property.BuildingCode = model.BuildingCode;
                                property.NoOfkingBeds = model.NoOfkingBeds;
                                property.RoomPrice = model.RoomPrice;
                                property.Latitude = model.Latitude;
                                property.Longitude = model.Longitude;

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
                                var response = propertyService.AddUpdateProperty(property, null);
                                property.Id = response.PropertyId;
                                Property_Id_List.Add(response.PropertyId);

                                if (response.PropertyId != 0)
                                {
                                    var propertyAllDetails = propertyDetails.ToList();
                                    //TempData["SuccessMsg"] = propertyService.message;
                                    if (propertyAllDetails.Count > 0)
                                    {
                                        TempData["Properties"] = propertyService.GetPropertiesSelectForMultiple(Property_Id_List);
                                    }
                                    return Redirect("/Home/Index#order_service");
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = propertyService.message;
                                }
                            }
                        }

                        //deleting excel file from folder  
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }
                    }
                    else
                    {
                        //alert message for invalid file format
                        TempData["ErrorMsg"] = Resource.only_excel_file_format;
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = Resource.please_choose_excel_file;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = ex.ToString();
            }
            var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
            return Redirect(PreviousUrl);
        }

        #endregion

        #region Add-Update Property

        /// <summary>
        /// Get method of the property which will check the add/edit return view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [VerifyUser]
        public ActionResult AddProperty(long? id)
        {
            Property getPropertyData = null;
            if (id != 0 && id != null)
            {
                ViewBag.TitleText = Resource.edit_property;
                getPropertyData = propertyService.GetPropertiesById((long)id);
            }
            else
            {
                ViewBag.TitleText = Resource.add_property;
            }
            return View(getPropertyData);

        }

        [HttpPost]
        [VerifyUser]
        public JsonResult AddProperty(FormCollection formCollection)
        {
            long userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            Property model = new Property();
            model.CreatedBy = userId;
            model.Name = formCollection["Name"];
            model.AccesstoCode = Convert.ToBoolean(formCollection["AccesstoCode"]);
            model.AccessToProperty = formCollection["AccessToProperty"];
            model.Address = formCollection["Address"];
            model.ApartmentNumber = Convert.ToInt32(formCollection["ApartmentNumber"]);
            model.Balcony = Convert.ToBoolean(formCollection["Balcony"]);
            model.BuildingCode = formCollection["BuildingCode"];
            model.CoffeeMachine = Convert.ToBoolean(formCollection["CoffeeMachine"]);
            model.CreatedBy = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            model.CreatedDate = DateTime.Now;
            model.Dishwasher = Convert.ToBoolean(formCollection["Dishwasher"]);
            model.Doorman = Convert.ToBoolean(formCollection["Doorman"]);
            model.Elevator = Convert.ToBoolean(formCollection["Elevator"]);
            model.FloorNumber = Convert.ToInt32(formCollection["FloorNumber"]);
            model.Garden = Convert.ToBoolean(formCollection["Garden"]);
            model.Id = Convert.ToInt32(formCollection["Id"]);
            model.IsActive = true;
            model.IsKingBed = Convert.ToBoolean(formCollection["IsKingBed"]);
            model.IsSingleBed = Convert.ToBoolean(formCollection["IsSingleBed"]);
            model.IsSofaBed = Convert.ToBoolean(formCollection["IsSofaBed"]);
            if (formCollection["Latitude"].ToString() != "" && formCollection["Longitude"].ToString() != "")
            {
                model.Latitude = Convert.ToDecimal(formCollection["Latitude"].ToString());
                model.Longitude = Convert.ToDecimal(formCollection["Longitude"].ToString());
            }
            model.NoOfBathrooms = Convert.ToInt32(formCollection["NoOfBathrooms"]);
            model.NoOfBedRooms = Convert.ToInt32(formCollection["NoOfBedRooms"]);
            model.NoOfDoubleBeds = Convert.ToInt32(formCollection["NoOfDoubleBeds"]);
            model.NoOfDoubleSofaBeds = Convert.ToInt32(formCollection["NoOfDoubleSofaBeds"]);
            model.NoOfQueenBeds = Convert.ToInt32(formCollection["NoOfQueenBeds"]);
            model.NoOfkingBeds = Convert.ToInt32(formCollection["NoOfKingBeds"]);
            model.NoOfSingleBeds = Convert.ToInt32(formCollection["NoOfSingleBeds"]);
            model.NoOfSingleSofaBeds = Convert.ToInt32(formCollection["NoOfSingleSofaBeds"]);
            model.NoOfToilets = Convert.ToInt32(formCollection["NoOfToilets"]);
            model.Parking = Convert.ToBoolean(formCollection["Parking"]);
            model.Pool = Convert.ToBoolean(formCollection["Pool"]);
            model.ShortTermApartment = Convert.ToBoolean(formCollection["ShortTermApartment"]);
            model.Size = formCollection["Size"];
            model.WifiLogin = formCollection["WifiLogin"];
            model.Type = formCollection["Type"];
            if (formCollection["RoomPrice"] != null)
            {
                model.RoomPrice = decimal.Parse(formCollection["RoomPrice"]);
            }
            var json = formCollection["Attachments"].ToString();
            List<Attachments> getAttachments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Attachments>>(json);

            var date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss").Replace("-", "_");

            var listImages = new List<tblPropertyImage>();

            if (getAttachments.Count > 0)
            {
                var count = getAttachments.Count();
                if (count > 0)
                {
                    for (int i = 0; i < getAttachments.Count(); i++)
                    {
                        string[] splitAttachment = getAttachments[i].result.Split(',');

                        if (getAttachments[i].type == "image")
                        {
                            string imageName = "Property" + Guid.NewGuid() + date + ".jpg";
                            Common.SavePropertyImage(splitAttachment[1], imageName);

                            listImages.Add(new tblPropertyImage
                            {
                                CreatedDate = DateTime.Now,
                                IsImage = true,
                                ImageUrl = imageName,
                                PropertyId = model.Id
                            });
                        }
                        else
                        {
                            string imageName = "Property" + Guid.NewGuid() + date + ".mp4";
                            Common.SavePropertyImage(splitAttachment[1], imageName);

                            listImages.Add(new tblPropertyImage
                            {
                                CreatedDate = DateTime.Now,
                                IsVideo = true,
                                VideoUrl = imageName,
                                PropertyId = model.Id
                            });
                        }
                    }
                }
            }
            if (model.ShortTermApartment == true)
            {
                model.Price2Star = model.RoomPrice;
                model.Price3Star = model.RoomPrice + ((model.RoomPrice / 100) * 10);
                model.Price4Star = model.RoomPrice + ((model.RoomPrice / 100) * 20);
                model.Price5Star = model.RoomPrice + ((model.RoomPrice / 100) * 30);
                model.DeluxPrice = model.Price5Star + ((model.Price5Star / 100) * 7);
            }
            else
            {
                model.Price2Star = model.RoomPrice - ((model.RoomPrice / 100) * 10);
                model.Price3Star = model.RoomPrice;
                model.Price4Star = model.RoomPrice + ((model.RoomPrice / 100) * 10);
                model.Price5Star = model.RoomPrice + ((model.RoomPrice / 100) * 20);
                model.DeluxPrice = model.Price5Star + ((model.Price5Star / 100) * 7);
            }
            var propertySuccessVM = propertyService.AddUpdateProperty(model, listImages);

            wsBase obj = new wsBase();
            if (propertySuccessVM.PropertyId != 0)
            {
                obj.status = true;
                obj.message = propertyService.message;
                obj.property_id = propertySuccessVM.PropertyId;
                obj.SubUserEmail = propertySuccessVM.SubUserEmail;
                obj.SubUserPassword = propertySuccessVM.SubUserPassword;
            }
            else
            {
                obj.status = false;
                obj.message = propertyService.message;
                obj.property_id = propertySuccessVM.PropertyId;
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [VerifyUser]
        public ActionResult EditProperty(long id)
        {


            PropertyViewModel getPropertyData = null;
            if (id != 0)
            {
                ViewBag.TitleText = Resource.edit_property;
                getPropertyData = propertyService.GetPropertiesDetails(id);
                if (getPropertyData != null)
                {
                    if (getPropertyData.PropertyImages != null)
                    {
                        for (int i = 0; i < getPropertyData.PropertyImages.Count; i++)
                        {
                            if (getPropertyData.PropertyImages[i].IsImage.HasValue &&
                                getPropertyData.PropertyImages[i].IsImage.Value)
                            {
                                getPropertyData.PropertyImages[i].ImageString = Common.ConvertImageString(getPropertyData.PropertyImages[i].ImageUrl);
                            }

                            if (getPropertyData.PropertyImages[i].IsVideo.HasValue &&
                               getPropertyData.PropertyImages[i].IsVideo.Value)
                            {
                                getPropertyData.PropertyImages[i].VideoString = Common.ConvertImageString(getPropertyData.PropertyImages[i].VideoUrl);
                            }
                        }
                    }
                }
            }
            return View(getPropertyData);
        }

        #endregion

        #region Property Access Codes and Building Codes

        [HttpPost]
        public JsonResult EditAccessCodeDetails(long? propertyID, string buildingCode, string accessToProperty)
        {
            var status = propertyService.UpdateAccessCodeDetails(propertyID, buildingCode, accessToProperty);
            return Json(propertyService.message, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public bool DeletePropertyImg(long propId)
        {
            bool status = propertyService.DeletePropertyImg(propId);
            return status;
        }

        [HttpGet]
        [VerifyUser]
        public JsonResult AutoPriceService(long propertyId, long subCategoryId)
        {
            var result = propertyService.AutoPriceService(propertyId, subCategoryId);
            return Json(new { price = result.ClientPrice }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Blocked(long propertyId)
        {
            return View();
        }
    }
}