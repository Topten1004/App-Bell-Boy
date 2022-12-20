using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels.ChannelManager.Smoobu
{
    public class SmoobuPropertyLocationViewModel
    {
        // Israel Address Format street, city, zip
        public string Street { get; set; }

        public string Zip { get; set; }

        public string MyProperty { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}