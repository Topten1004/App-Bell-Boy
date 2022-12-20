using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class SubSubCategoryViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string HasPrice { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string Description_Russian { get; set; }
        public string Description_Hebrew { get; set; }
        public string Description_French { get; set; }
        public decimal? ClientPrice { get; set; }
        public bool IsSelected { get; set; }
        public string Picture { get; set; }
        public string Icon { get; set; }
        public string Name_Russian { get; set; }
        public string Name_Hebrew { get; set; }
        public string Name_French { get; set; }
    }
}