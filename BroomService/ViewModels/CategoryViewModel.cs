using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class CategoryViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Name_Russian { get; set; }
        public string Name_Hebrew { get; set; }
        public string Name_French { get; set; }
        public int? DisplayOrder { get; set; }
        public string Description { get; set; }
        public string Description_Russian { get; set; }
        public string Description_Hebrew { get; set; }
        public string Description_French { get; set; }
        public string Picture { get; set; }
        public string Icon { get; set; }
        public bool HasPrice { get; set; }
        public decimal? Price { get; set; }
        public bool? HasSubSubCategories { get; set; }
        public bool ForWorkers { get; set; }
        public bool? IsActive { get; set; }
        public bool IsSelected { get; set; }
        public bool? SubUserOrder { get; set; }
        public bool? SubUserSendRequest { get; set; }
        public bool? ForSubUser { get; set; }
    }

    public class CategoryPropertyModel
    {
        public string Property_List_Id { get; set; }
        public List<CategoryViewModel> CategoryList { get; set; }
    }

    public class SubCategoryPropertyModel
    {
        public string Property_List_Id { get; set; }
        public List<SubSubCategoryViewModel> SubCategoryList { get; set; }
    }

    public class CategoryViewModelApp:CategoryViewModel
    {
        public List<SubCategoryViewModelApp> SubCategories { get; set; }
    }

    public class SubCategoryViewModelApp
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Name_Russian { get; set; }
        public string Name_Hebrew { get; set; }
        public string Name_French { get; set; }
        public string Description { get; set; }
        public string Description_Russian { get; set; }
        public string Description_Hebrew { get; set; }
        public string Description_French { get; set; }
        public decimal? Price { get; set; }
        public decimal? ClientPrice { get; set; }
        public string Picture { get; set; }
        public string Icon { get; set; }
        public bool HasSubSubCategories { get; set; }
        public List<SubSubCategoryViewModel> SubSubCategories { get; set; }
    }

    public class SubUserRequestVM
    {
        public DateTime? CreatedDate { get; set; }
        public string CategoryName { get; set; }
        public bool? IsAccepted { get; set; }
        public string Reason { get; set; }
        public long UserId { get; set; }
        public long ServiceId { get; set; }
    }
}