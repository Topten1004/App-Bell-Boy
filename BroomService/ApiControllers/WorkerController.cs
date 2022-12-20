using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace BroomService.ApiControllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("")]
    public class WorkerController : ApiController
    {
        WorkerService workerService;
        public WorkerController()
        {
            workerService = new WorkerService();
        }
        [HttpGet]
        public IHttpActionResult Schedules(long user_id, bool isNextWeek = false)
        {
            var getData = workerService.GetAvailableScheduleList(user_id, isNextWeek);
            return Ok(new
            {
                status = getData != null,
                message = "",
                data = getData
            });
        }

        public IHttpActionResult UpdateScheduleTime(AvailableTime model)
        {
            try
            {

                bool error = false;
                StringBuilder oLogError = new StringBuilder();

                if (!TimeSpan.TryParse(model.from_time, out TimeSpan tsfrom))
                {
                    oLogError.Append("Invalid from time schedule." + Environment.NewLine);
                    error = true;
                }
                if (!TimeSpan.TryParse(model.to_time, out TimeSpan tsto))
                {

                    oLogError.Append("Invalid to time schedule." + Environment.NewLine);
                    error = true;
                }
                if (tsfrom > tsto)
                {
                    oLogError.Append("to time schedule must be greater than from time schedule.");
                    error = true;
                }

                if ((!model.isCausallOff && !model.isOptionalOff) && (model.from_time == "0:00" || model.to_time == "0:00"))
                {
                    oLogError.Append("Pleae enter valid time schedule.");
                }

                if (error)
                {
                    return Ok(new
                    {
                        status = false,
                        message = oLogError,
                        data = false
                    });
                }
                model = workerService.AddAvailableTimeInProvider(model);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message,
                    data = false
                });
            }

            return Ok(new
            {
                status = model != null,
                message = "",
                data = model
            });
        }


    }
}