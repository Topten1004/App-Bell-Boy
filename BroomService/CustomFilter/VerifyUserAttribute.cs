using BroomService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.CustomFilter
{
    public class VerifyUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            BroomServiceEntities1 _db = new BroomServiceEntities1();
            var login = filterContext.HttpContext.Request.Cookies["Login"];
            if (login != null)
            {
                long userId = Convert.ToInt32(login.Values["UserId"]);
                var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
                if(user == null)
                {
                    filterContext.Result = new RedirectResult(string.Format("/Account/Login?targetUrl={0}", filterContext.HttpContext.Request.Url.AbsolutePath));

                } else if(user.IsActive != true)
                {
                    filterContext.Result = new RedirectResult(string.Format("/Account/Login?targetUrl={0}", filterContext.HttpContext.Request.Url.AbsolutePath));
                }
            } else
            {
                filterContext.Result = new RedirectResult(string.Format("/Account/Login?targetUrl={0}", filterContext.HttpContext.Request.Url.AbsolutePath));
            }
        }
    }
}