using BroomService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class HomeViewModel
    {
        public List<PropertyViewModel> Properties { get; set; }
        ///public List<JobRequestViewModel> MyBookings { get; set; }
        public String AboutUsText { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
        public List<Testimonial> Testimonials { get; set; }
    }
    public class UserJobApiModal
    {
        public long UserId { get; set; }
        public long JobRequestId { get; set; }
    }
}