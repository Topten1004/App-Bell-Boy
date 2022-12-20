using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class PropertySettingsViewModel
    {
        public UserChannelManagerSettingsViewModel UserChannelManagerSettings { get; set; }

        public UserChannelManagerViewModel UserChannelManagerViewModel { get; set; }

        public ChannelManagerViewModel ChannelManagerViewModel { get; set; }

        public List<JobRequestViewModel> FastOrders { get; set; }

    }
}