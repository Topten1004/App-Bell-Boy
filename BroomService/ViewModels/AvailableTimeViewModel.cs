using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class AvailableTimeViewModel
    {
        public long Id { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string AvailableDate { get; set; }
        public DateTime? JobDate { get; set; }
        public string JobStartDate { get; set; }
        public string UserEmail { get; set; }
        public long UserId { get; set; }
        public string Address { get; set; }
        public decimal Distance { get; set; }
    }
}