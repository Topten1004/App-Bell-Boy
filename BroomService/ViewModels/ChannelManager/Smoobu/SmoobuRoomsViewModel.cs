using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels.ChannelManager.Smoobu
{
    public class SmoobuRoomsViewModel
    {
        public int MaxOccupancy { get; set; }

        public int Bedrooms { get; set; }

        public int DoubleBeds { get; set; }

        public int SingleBeds { get; set; }

        public int SofaBeds { get; set; }

        public int Couches { get; set; }

        public int ChildBeds { get; set; }

        public int QueenSizeBeds { get; set; }

        public int KingSizeBeds { get; set; }

    }
}