using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class PropertyManagerViewModel
    {
        public List<ChannelManagerViewModel> ChannelManagers { get; set; }

        public List<UserChannelManagerViewModel> UserChannelManagers { get; set; }
    }
}