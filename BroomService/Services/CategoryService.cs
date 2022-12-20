using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.Services
{
    public class CategoryService
    {
        BroomServiceEntities1 _db;
        public string message;

        public CategoryService()
        {
            _db = new BroomServiceEntities1();
        }

        /// <summary>
        /// Getting the list of the categories
        /// </summary>
        /// <returns></returns>
        public List<CategoryViewModel> GetCategories(int UserType = 0)
        {
            List<CategoryViewModel> categories = new List<CategoryViewModel>();
            try
            {
                IQueryable<Category> iQcategories = null;
                if(UserType == 7)
                {
                    iQcategories = _db.Categories.Where(a => a.IsActive == true && (a.ForSubUser == true || a.SubUserSendRequest == true || a.SubUserOrder == true)); 
                }
                else
                {
                    iQcategories = _db.Categories.Where(a => a.IsActive == true);
                }

                if (iQcategories != null)
                {
                    categories = iQcategories
                    .Select(x => new CategoryViewModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Name_French = x.Name_French,
                        Name_Hebrew = x.Name_Hebrew,
                        Name_Russian = x.Name_Russian,
                        Description = x.Description,
                        Description_French = x.Description_French,
                        Description_Hebrew = x.Description_Hebrew,
                        Description_Russian = x.Description_Russian,
                        Picture = x.Picture,
                        IsActive = x.IsActive,
                        Icon = x.Icon,
                        ForWorkers = x.ForWorkers ?? false,
                        DisplayOrder = x.DisplayOrder
                    }).OrderBy(z => z.DisplayOrder ?? 100000).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return categories;
        }

        /// <summary>
        /// Getting the category by id
        /// </summary>
        /// <returns></returns>
        public Category GetCategoryById(long categoryId)
        {
            Category category = new Category();
            try
            {
                category = _db.Categories.Where(a => a.IsActive == true
                && a.Id == categoryId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                category = null;
            }
            return category;
        }

        public SubCategory GetSubCategoryById(long subCatId)
        {
            SubCategory category = null;
            try
            {
                category = _db.SubCategories.Where(a => a.IsActive == true
                && a.Id == subCatId).FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return category;
        }

        /// <summary>
        /// Getting the sub sub category by id
        /// </summary>
        /// <returns></returns>
        public SubSubCategory GetSubSubCategoryById(long subCatId)
        {
            SubSubCategory category = null;
            try
            {
                category = _db.SubSubCategories.Where(a => a.IsActive == true
                && a.Id == subCatId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                category = null;
            }
            return category;
        }

        /// <summary>
        /// Method that will use for getting the sub categories based on category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<CategoryViewModel> GetSubCategoryByCatId(long id)
        {
            List<CategoryViewModel> category = new List<CategoryViewModel>();
            try
            {
                category = _db.SubCategories.Where(x => x.CatId == id && x.IsActive == true).Select(x => new CategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Name_French = x.Name_French,
                    Name_Hebrew = x.Name_Hebrew,
                    Name_Russian = x.Name_Russian,
                    Description = x.Description,
                    Description_French = x.Description_French,
                    Description_Hebrew = x.Description_Hebrew,
                    Description_Russian = x.Description_Russian,
                    Picture = x.Picture,
                    IsActive = x.IsActive,
                    Icon = x.Icon,
                    Price = x.Price != null ? x.Price:0,
                    HasSubSubCategories = x.HasSubSubCategories,
                    ForWorkers = x.Category.ForWorkers ?? false,
                    ForSubUser = x.Category.ForSubUser ?? false,
                    SubUserOrder = x.Category.SubUserOrder ?? false,
                    SubUserSendRequest = x.Category.SubUserSendRequest ?? false
                }).ToList();
                message = Resource.success;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                category = new List<CategoryViewModel>();
            }
            return category;
        }

        /// <summary>
        /// Method that will use to get the sub sub category based on sub category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<SubSubCategoryViewModel> GetSubSubCategoryBySubCatId(long id)
        {
            List<SubSubCategoryViewModel> subcategory = new List<SubSubCategoryViewModel>();
            try
            {
                subcategory = _db.SubSubCategories.Where(x => x.SubCatId == id && x.IsActive==true).Select(x => new SubSubCategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryId = x.SubCategory.CatId.Value,
                    Name_French = x.Name_French,
                    Name_Hebrew = x.Name_Hebrew,
                    Name_Russian = x.Name_Russian,
                    Description = x.Description,
                    Description_French = x.Description_French,
                    Description_Hebrew = x.Description_Hebrew,
                    Description_Russian = x.Description_Russian,
                    IsActive = x.IsActive,
                    SubCategoryId = x.SubCategory.Id,
                    SubCategoryName = x.SubCategory.Name,
                    Price = x.Price,
                    ClientPrice = x.ClientPrice,
                    Picture = x.Picture,
                    Icon = x.Icon
                }).ToList();
            }
            catch (Exception ex)
            {
            }
            return subcategory;
        }

        /// <summary>
        /// Method to return all the categories for the application
        /// </summary>
        /// <returns></returns>
        public List<CategoryViewModelApp> GetCatSubCategoriesApp()
        {
            List<CategoryViewModelApp> categories = new List<CategoryViewModelApp>();
            try
            {
                categories = _db.Categories.ToList().Where(x => x.IsActive == true).OrderBy(p => p.DisplayOrder ?? 100000).Select(x => new CategoryViewModelApp()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Name_French = x.Name_French,
                    Name_Hebrew = x.Name_Hebrew,
                    Name_Russian = x.Name_Russian,
                    Description = x.Description,
                    Description_French = x.Description_French,
                    Description_Hebrew = x.Description_Hebrew,
                    Description_Russian = x.Description_Russian,
                    Picture = x.Picture,
                    SubUserOrder = x.SubUserOrder,
                    SubUserSendRequest = x.SubUserSendRequest,
                    Icon = x.Icon,
                    SubCategories = x.SubCategories.Where(y => y.IsActive == true).Select(y => new SubCategoryViewModelApp()
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Name_French = y.Name_French,
                        Name_Hebrew = y.Name_Hebrew,
                        Name_Russian = y.Name_Russian,
                        Description = y.Description,
                        Description_French = y.Description_French,
                        Description_Hebrew = y.Description_Hebrew,
                        Description_Russian = y.Description_Russian,
                        Icon = y.Icon,
                        Picture = y.Picture,
                        Price = y.Price,
                        ClientPrice = y.ClientPrice,
                        HasSubSubCategories = y.HasSubSubCategories ?? false,
                        SubSubCategories = y.SubSubCategories.Where(z => z.IsActive == true).Select(z => new SubSubCategoryViewModel()
                        {
                            Id = z.Id,
                            Name = z.Name,
                            Name_French = z.Name_French,
                            Name_Hebrew = z.Name_Hebrew,
                            Name_Russian = z.Name_Russian,
                            Description = z.Description,
                            Description_French = z.Description_French,
                            Description_Hebrew = z.Description_Hebrew,
                            Description_Russian = z.Description_Russian,
                            Icon = z.Icon,
                            Picture = z.Picture,
                            Price = z.Price,
                            ClientPrice = z.ClientPrice,
                        }).ToList()
                    }).ToList(),
                }).ToList();
            }
            catch (Exception ex)
            {
            }
            return categories;
        }


        public bool SubUserRequest(SubUserRequestVM subUserRequest)
        {
            bool status = false;
            try
            {
                SubUserRequest _request = new SubUserRequest();
                _request.SubUserId = subUserRequest.UserId;
                _request.Reason = subUserRequest.Reason;
                _request.CreatedDate = DateTime.Now;
                _request.ServiceId = subUserRequest.ServiceId;

                // Getting the data of the user who has created the sub user
                var getParentId = _db.Properties.Where(a => a.SubUserId == subUserRequest.UserId).FirstOrDefault();
                if(getParentId!=null)
                {
                    _request.ParentId = getParentId.CreatedBy;
                }
                _db.SubUserRequests.Add(_request);
                _db.SaveChanges();

                var categoryData = _db.Categories.Where(a => a.Id == _request.ServiceId).FirstOrDefault();
                var categoryName = categoryData != null ? categoryData.Name : string.Empty;


                // Send notification to the main user
                Notification notification = new Notification();
                notification.CreatedDate = DateTime.Now;
                notification.FromUserId = _request.SubUserId;
                notification.IsActive = true;
                notification.ToUserId=_request.ParentId;
                notification.JobRequestId = _request.SubUserRequestId;
                notification.Text ="Sub user of the property "+getParentId.Name
                    +" has requested for ordering the service "+ categoryName
                    +"<br/> Reason: "+_request.Reason;
                notification.NotificationStatus =Enums.NotificationStatus.SubUserRequestService.GetHashCode();

                _db.Notifications.Add(notification);
                _db.SaveChanges();
                message = Resource.success;
                status = true;
                    
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.ToString();
            }
            return status;
        }
        public JobRequestSubSubCategory GetCategoryDetailByJobId(long JobReqId)
        {
            JobRequestSubSubCategory obj = new JobRequestSubSubCategory();
            try
            {
                var JobRequestPropId = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == JobReqId).Select(x => x.JobRequestPropId).FirstOrDefault();
                obj = _db.JobRequestSubSubCategories.Where(x => x.JobRequestId == JobRequestPropId).FirstOrDefault();

            }
            catch(Exception ex)
            {
                obj= new JobRequestSubSubCategory();
            }
            return obj;
        }
    }
}