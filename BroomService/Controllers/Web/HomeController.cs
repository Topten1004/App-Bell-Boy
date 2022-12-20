using BroomService.bin.Controllers.Web;
using BroomService.Helpers;
using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.Controllers
{
    public class HomeController : MyController
    {
        HomeService homeService;

        public HomeController()
        {
            homeService = new HomeService();
        }

        public ActionResult Index()
       {
            int getUserId = 0;
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    getUserId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                }
            }
            var getData = homeService.GetHomeData(getUserId);
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;

            if (getData != null)
            {
                if (getData.Categories != null && getData.Categories.Count > 0)
                {
                    for (int i = 0; i < getData.Categories.Count; i++)
                    {
                        getData.Categories[i].Name = culVal == "fr-FR" ? getData.Categories[i].Name_French
: culVal == "ru-RU" ? getData.Categories[i].Name_Russian
: culVal == "he-IL" ? getData.Categories[i].Name_Hebrew
: getData.Categories[i].Name;

                        getData.Categories[i].Description = culVal == "fr-FR" ? getData.Categories[i].Description_French
    : culVal == "ru-RU" ? getData.Categories[i].Description_Russian
    : culVal == "he-IL" ? getData.Categories[i].Description_Hebrew
    : getData.Categories[i].Description;
                    }
                }
                if(getData.Properties != null)
                {
                    if (getData.Properties.Count == 1)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            getData.Properties.Add(new PropertyViewModel
                            {
                                PropertyModel = new PropertyVM
                                {
                                    AccesstoCode = false,
                                    AccessToProperty = "",
                                    Address = "",
                                    ApartmentNumber = 0,
                                    BuildingCode = "",
                                    FloorNumber = 0,
                                    Id = 0,
                                    Name = "",
                                    ShortTermApartment = false,
                                    Type = String.Empty
                                }
                            });
                        }
                    }
                    else if (getData.Properties.Count == 2)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            getData.Properties.Add(new PropertyViewModel
                            {
                                PropertyModel = new PropertyVM
                                {
                                    AccesstoCode = false,
                                    AccessToProperty = "",
                                    Address = "",
                                    ApartmentNumber = 0,
                                    BuildingCode = "",
                                    FloorNumber =0,                             
                                    Id = 0,                                  
                                    Name = "",
                                    ShortTermApartment = false,
                                    Type = String.Empty
                                }
                            });
                        }
                    }
                }
            }         
            return View(getData);
        }
        public ActionResult ChangeLanguage(string lang)
        {
            new LanguageMang().SetLanguage(lang);
            ViewBag.SelectedLang = lang;
            if (System.Web.HttpContext.Current.Request.UrlReferrer != null)
            {
                TempData["PreviousUrl"] = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
            }
            return Redirect((string)TempData["PreviousUrl"]);
        }
        public JsonResult GetService(long UserId)
        {
            var getData = homeService.GetServiceData(UserId);
            return Json(getData, JsonRequestBehavior.AllowGet);
        }
        public bool AskLater(long UserId)
        {
            bool status = false;
            status = homeService.AskLater(UserId);
            return status;
        }
        public bool Cancel(long UserId)
        {
            bool status = false;
            status = homeService.Cancel(UserId);
            return status;
        }
        public bool BookServiceNextWeek(long UserId, long JobId)
        {
            bool status = false;
            status = homeService.BookServiceNextWeek(UserId, JobId);
            return status;
        }
    }
}