using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BroomService
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings =
            //new JsonSerializerSettings
            //{
            //    DateFormatHandling = DateFormatHandling.IsoDateFormat,
            //    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            //    Culture = CultureInfo.GetCultureInfo("en-US")
            //};
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
