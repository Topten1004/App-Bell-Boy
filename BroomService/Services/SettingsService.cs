using BroomService.Models;
using BroomService.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.Services
{
    public class SettingsService
    {
        public string message = string.Empty;

        BroomServiceEntities1 _db;
        public SettingsService()
        {
            _db = new BroomServiceEntities1();
        }

        public bool ContactUs(ContactU model)
        {
            bool status = false;
            try
            {
                model.CreatedDate = DateTime.Now;
                _db.ContactUs.Add(model);
                _db.SaveChanges();
                message = Resource.message_sent_success;
                status = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public AboutU GetAboutUsData()
        {
            var result = _db.AboutUs.FirstOrDefault();
            return result;
        }

        public PrivacyPolicy GetPrivacyPolicy()
        {
            PrivacyPolicy data = null;
            try
            {
                data = _db.PrivacyPolicies.FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return data;
        }

        public List<SelectListItem> GetCategoriesSelect()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var data = _db.Categories.Where(x => x.IsActive == true).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                listItems.Add(new SelectListItem
                {
                    Text = data[i].Name,
                    Value = data[i].Id.ToString()
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetContactUs()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var data = _db.Categories.Where(x => x.IsActive == true).ToList();
            //for (int i = 0; i < data.Count; i++)
            //{
            //    listItems.Add(new SelectListItem
            //    {
            //       Text= "Bug Issues",
            //       Value = "1"
            //    });
            // }

            listItems.Add(new SelectListItem
            {
                Text = "Bug Issues",
                Value = "1"
            });

            listItems.Add(new SelectListItem
            {
                Text = "Question",
                Value = "2"
            });

            listItems.Add(new SelectListItem
            {
                Text = "Other",
                Value = "3"
            });
            return listItems;
        }

        public List<Testimonial> GetTestimonialData()
        {
            var result = _db.Testimonials.ToList();
            return result;
        }
    }
}