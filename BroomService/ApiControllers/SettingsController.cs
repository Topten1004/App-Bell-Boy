using BroomService.Models;
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
    public class SettingsController : ApiController
    {
        public SettingsService settingsService;
        public AccountService accountService;
        public PropertyService propertyService;

        public SettingsController()
        {
            settingsService = new SettingsService();
            accountService = new AccountService();
            propertyService = new PropertyService();
        }

        #region About Us

        /// <summary>
        /// About Us Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetAboutus()
        {
            var result = settingsService.GetAboutUsData();
            string msg = string.Empty;
            if (result == null)
            {
                msg = Resource.no_data_found;
            }
            else
            {
                msg = Resource.success;
            }
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = msg,
                AboutUsData = result
            });
        }

        #endregion

        #region Terms Conditions

        /// <summary>
        /// Terms Conditions Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetTermsConditions()
        {
            var result = accountService.GetTermsConditions();
            string msg = string.Empty;
            if (result == null)
            {
                msg = Resource.no_data_found;
            }
            else
            {
                msg = Resource.success;
            }
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = msg,
                TermsConditionsData = result
            });
        }

        #endregion

        #region Privacy Policy

        /// <summary>
        /// Privacy Policy Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPrivacyPolicy()
        {
            var result = settingsService.GetPrivacyPolicy();
            string msg = string.Empty;
            if (result == null)
            {
                msg = Resource.no_data_found;
            }
            else
            {
                msg = Resource.success;
            }
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = msg,
                PrivacyPolicyData = result
            });
        }

        #endregion

        #region Contact Us

        /// <summary>
        /// Contact Us
        /// </summary>
        /// <returns>model</returns>
        [HttpPost]
        public IHttpActionResult ContactUs(ContactU model)
        {
            var response = settingsService.ContactUs(model);
            return this.Ok(new
            {
                status = response,
                message = settingsService.message
            });
        }

        #endregion

        #region Testimonials

        /// <summary>
        /// Testimonials Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetTestimonials()
        {
            var result = settingsService.GetTestimonialData();
            string msg = string.Empty;
            if (result == null)
            {
                msg = Resource.no_data_found;
            }
            else
            {
                msg = Resource.success;
            }
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = msg,
                TestimonialData = result
            });
        }

        #endregion

        #region Grant Access

        /// <summary>
        /// Adding the grant option for the propeties
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult GrantAccess(GrantAccessViewModel model)
        {
            try
            {
                var getProperties = propertyService.GetPropertiesSelect(model.CreatedBy);
                if (model.Property_List_id != null)
                {
                    if (model.Property_List_id.Count > 0)
                    {
                        if (getProperties.Count == model.Property_List_id.Count)
                        {
                            model.ForAllProperties = true;
                        }
                        else
                        {
                            model.ForAllProperties = false;
                        }
                    }
                }
                var _status = accountService.GrantAccess(model);

                return this.Ok(new
                {
                    status = _status,
                    message = accountService.message
                });
            }
            catch (Exception ex)
            {
                return this.Ok(new
                {
                    status = false,
                    message = accountService.message
                });
            }
        }

        /// <summary>
        /// Testimonials Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GrantAccessList(long user_id)
        { 
            var result = accountService.GetAccessListData(user_id);
            string msg = string.Empty;
            if (result == null)
            {
                msg = Resource.no_data_found;
            }
            else
            {
                msg = Resource.success;
            }
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = msg,
                GrantAccessData = result
            });
        }

        #endregion

    }
}
