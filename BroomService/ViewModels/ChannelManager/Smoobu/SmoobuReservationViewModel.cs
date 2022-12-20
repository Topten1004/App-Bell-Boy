using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels.ChannelManager.Smoobu
{
    public class SmoobuReservationViewModel
    {
        public string Action { get; set; }

        public long User { get; set; }

        public SmoobuReservationDataViewModel Data { get; set; }
    }
}