using BroomService.bin.Controllers.Web;
using BroomService.Models;
using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.Controllers
{
    public class SettingsController : MyController
    {
        public SettingsService settingsService;
        public AccountService accountService;

        public SettingsController()
        {
            settingsService = new SettingsService();
            accountService = new AccountService();
        }

        #region Terms Conditions

        public ActionResult TermsConditions()
        {
            var result = accountService.GetTermsConditions();
            return View(result);
        }

        #endregion

        #region Privacy Policy

        public ActionResult PrivacyPolicy()
        {
            var result = settingsService.GetPrivacyPolicy();
            return View(result);
        }

        #endregion

        #region Contact us

        public ActionResult ContactUs()
        {
            ViewBag.Categories = settingsService.GetContactUs();
            return View();
        }


        [HttpPost]
        public ActionResult ContactUs(ContactU data)
        {
            var response = settingsService.ContactUs(data);
            if (response)
            {
                ModelState.Clear();
                TempData["SuccessMsg"] = settingsService.message;
            }
            else
            {
                TempData["ErrorMsg"] = settingsService.message;
            }
            ViewBag.Categories = settingsService.GetContactUs();
            return View();
        }

        #endregion

        #region About us

        public ActionResult AboutUs()
        {
            var result = settingsService.GetAboutUsData();
            return View(result);
        }

        #endregion

        #region Testimonials

        public ActionResult TestimonialList()
        {
            var result = settingsService.GetTestimonialData();
            return View(result);
        }

        #endregion
    }
}