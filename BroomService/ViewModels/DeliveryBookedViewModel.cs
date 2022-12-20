using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class DeliveryBookedViewModel
    {
        public long PickupGuyId { get; set; }

        public long ReturnGuyId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public string Address { get; set; }

        public decimal Time { get; set; }

        public decimal Distance { get; set; }
    }
}