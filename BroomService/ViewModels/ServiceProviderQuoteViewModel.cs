using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class ServiceProviderQuoteViewModel
    {
        public long ServiceProviderId { get; set; }

        public long QuoteId { get; set; }

        public long ServiceId { get; set; }

        public long PropertyId { get; set; }

        public decimal Price { get; set; }

    }
}