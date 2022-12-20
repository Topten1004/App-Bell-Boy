using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class LaundryPriceViewModel
    {
        public long LaundryId { get; set; }

        public long LaundryRequestId { get; set; }

        public decimal LaundryPrice { get; set; }
    }
}