using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class QuoteViewModel
    {
        public long ServiceProviderId { get; set; }

        public long JobRequestId { get; set; }

        public decimal QuotePrice { get; set; }
    }
}