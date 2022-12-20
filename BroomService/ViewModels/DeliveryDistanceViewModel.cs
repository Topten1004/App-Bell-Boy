using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class DeliveryDistanceViewModel
    {
        public long DeliveryGuyId { get; set; }

        public string DeliveryGuyName { get; set; }

        public decimal Distance { get; set; }

        public decimal Time { get; set; }
    }
}