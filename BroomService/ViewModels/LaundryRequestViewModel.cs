using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class LaundryRequestViewModel
    {
        public long LaundryRequestId { get; set; }

        public int LaundryItems { get; set; }

        public int IroningItems { get; set; }

        public int DryingItems { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public decimal Price { get; set; }

        public int LaundryStatus { get; set; }

        public long PropertyId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyAddress { get; set; }

        public long LaundryId { get; set; }

        public string LaundryName { get; set; }

        public string LaundryAddress { get; set; }

        public long PickupGuyId { get; set; }

        public long ReturnGuyId { get; set; }

        public long JobRequestId { get; set; }

        public long UserId { get; set; }
    }
}