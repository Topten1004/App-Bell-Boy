using BroomService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BroomService.CustomFilter
{
    public class VerifyPropertyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            BroomServiceEntities1 _db = new BroomServiceEntities1();
            string propertyIdStr = filterContext.HttpContext.Request.QueryString["propertyId"];
            if (!string.IsNullOrEmpty(propertyIdStr))
            {
                long propertyId = Convert.ToInt32(filterContext.HttpContext.Request.QueryString["propertyId"]);
                var property = _db.Properties.FirstOrDefault(u => u.Id == propertyId);
                if (property != null)
                {
                    if(property.Blocked)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Property", action = "Blocked", propertyId }));
                    }
                }
            }
        }
    }
}