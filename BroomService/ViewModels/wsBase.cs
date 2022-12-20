using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class wsBase
    {
        public bool status { get; set; }
        public bool IsPaymentDone { get; set; }
        public string message { get; set; }
        public long property_id { get; set; }
        public string description { get; set; }
        public string unitprice { get; set; }
        public bool jobTimeAlert { get; set; }
        public List<string> JobRequestID { get; set; }
        public string SubUserEmail { get; set; }
        public string SubUserPassword { get; set; }

        public bool Salegenerated { get; set; }
        public string SaleUrl { get; set; }

    }
    public class Attachments
    {
        public string result { get; set; }
        public string type { get; set; }
    }

    public class csBase
    {
        public bool status { get; set; }

        public string pictureName { get; set; }
    }
    public class BaseModels
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string Data { get; set; }
        public string description { get; set; }
        public string unitprice { get; set; }
        public string price { get; set; }
        public int JobRequestID { get; set; }


    }
}