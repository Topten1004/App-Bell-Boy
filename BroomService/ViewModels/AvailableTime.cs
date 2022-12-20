using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class AvailableTime
    {
        public long user_id { get; set; }
        public long? id { get; set; }
        public string user_name { get; set; }
        public int? day_of_week { get; set; }
        public string from_time { get; set; }
        public string to_time { get; set; }

        public bool isOptionalOff { get; set; }
        public bool isCausallOff { get; set; }
        public int week_of_year { get; set; }
        public int year { get; set; }

        public bool isNextWeek { get; set; }

        public int userType { get; set; }

        public DateTime? vDate { get; set; }

        public string scheduleDate { get; set; }

        public bool? IsVisible { get; set; }
    }
}