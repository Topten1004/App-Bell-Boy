using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels.ChannelManager.Smoobu
{
    public class SmoobuPropertyViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string TimeZone { get; set; }

        public string Currency { get; set; }

        public string[] Amenities { get; set; }

        public ChannelManagerAccomodationViewModel Type { get; set; }

        public SmoobuRoomsViewModel Rooms { get; set; }

        public SmoobuPropertyLocationViewModel Location { get; set; }

    }
}