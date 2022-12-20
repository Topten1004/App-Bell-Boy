using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class SaveQuesResponse
    {
        public long  WorkerId { get; set; }
        public string Type { get; set; }
        public string QuesResponse1 { get; set; }
        public string QuesResponse2 { get; set; }
        public string QuesResponse3 { get; set; }
        public string QuesResponse4 { get; set; }
        public string QuesResponse5 { get; set; }
    }
}