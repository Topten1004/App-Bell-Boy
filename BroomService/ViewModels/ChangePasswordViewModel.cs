using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
        public long? userId { get; set; }
    }

    public class GrantAccessViewModel
    {
        public List<long> Property_List_id { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public DateTime? Date { get; set; }
        public long CreatedBy { get; set; }
        public bool? OrderServicesAccess { get; set; }
        public bool? AddEditPropertiesAccess { get; set; }
        public bool? BillingPriceAccess { get; set; }
        public bool? AddChangeCardAccess { get; set; }
        public bool? ForAllProperties { get; set; }
        public List<PropertyGrantAccess> PropertiesList { get; set; }
    }

    public class PropertyGrantAccess
    {
        public long? PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
    }
}