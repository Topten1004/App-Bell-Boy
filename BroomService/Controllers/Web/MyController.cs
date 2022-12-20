using BroomService.Helpers;
using BroomService.Resources;
using BroomService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.bin.Controllers.Web
{
    public class MyController : Controller
    {
        // GET: My
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            AccountService accountService = new AccountService();
            try
            {
                string lang = null;
                HttpCookie langCookie = Request.Cookies["culture"];
                if (langCookie != null)
                {
                    lang = langCookie.Value;
                }
                else
                {
                    var userLanguage = Request.UserLanguages;
                    var userLang = userLanguage != null ? userLanguage[0] : "";
                    if (userLang != "")
                    {
                        lang = userLang;
                    }
                    else
                    {
                        lang = LanguageMang.GetDefaultLanguage();
                    }
                }
                lang = "en"; //to be remove
                new LanguageMang().SetLanguage(lang);               
                ViewBag.SelectedLang = lang;
                ViewBag.GreetingMessage = accountService.GetGreetingMessages();
            }
            catch (Exception Exc)
            {

            }
            return base.BeginExecuteCore(callback, state);
        }
    }
}