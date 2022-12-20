using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BroomService.ApiControllers
{
    public class CategoryController : ApiController
    {
        CategoryService categoryService;

        public CategoryController()
        {
            categoryService = new CategoryService();
        }

        /// <summary>
        /// Get Categories api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetCategories()
        {
            var result = categoryService.GetCatSubCategoriesApp();
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = result!=null?Resource.success:Resource.no_data_found,
                categoryData = result
            });
        }

        [HttpGet]
        public IHttpActionResult GetAllCategories()
        {
            var result = categoryService.GetCategories();
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = result != null ? Resource.success : Resource.no_data_found,
                categoryData = result
            });
        }

        [HttpGet]
        public IHttpActionResult GetSubCatByCatId(int CatId)
        {
            var result = categoryService.GetSubCategoryByCatId(CatId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = result != null ? Resource.success : Resource.no_data_found,
                SubcategoryData = result
            });
        }
        [HttpGet]
        public IHttpActionResult GetSubSubCatByCatId(int SubCatId)
        {
            var result = categoryService.GetSubSubCategoryBySubCatId(SubCatId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = result != null ? Resource.success : Resource.no_data_found,
                SubSubcategoryData = result
            });
        }

        /// <summary>
        /// Get Categories api
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SubUserRequest(SubUserRequestVM subUserRequest)
        {
            var result = categoryService.SubUserRequest(subUserRequest);
            return this.Ok(new
            {
                status = result,
                message = categoryService.message
            });
        }
    }
}
