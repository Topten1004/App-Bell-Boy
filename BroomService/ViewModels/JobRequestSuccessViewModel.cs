using BroomService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class JobRequestSuccessViewModel
    {
        public JobRequest JobRequest { get; set; }

        public JobRequestPropertyService PropertyService { get; set; }

        public bool Status { get; set; }

        public bool HasPrice { get; set; }
    }
}