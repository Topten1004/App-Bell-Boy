using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static BroomService.Helpers.Enums;
using System.Drawing.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using TheArtOfDev.HtmlRenderer.WinForms;
using System.Threading;
using System.Windows.Forms;
using Syncfusion.HtmlConverter;
using System.Diagnostics;
using EvoPdf;
using BroomService.bin.Controllers.Web;
using System.Configuration;
using BroomService.CustomFilter;

namespace BroomService.Controllers
{
    public class OrderController : MyController
    {
        CategoryService categoryService;
        PropertyService propertyService;
        OrderService orderService;
        CardService cardService;
        AccountService accountService;
        TokenService tokenService;
        public ICountResponse doc_invoice;
        public ICountResponse doc_receipt;
        public CardViewModel card_details;
        public bool payment_status = false;
        public long ConfirmationCode = 0;

        public OrderController()
        {
            categoryService = new CategoryService();
            propertyService = new PropertyService();
            orderService = new OrderService();
            cardService = new CardService();
            accountService = new AccountService();
            tokenService = new TokenService();
        }

        #region Services

        [VerifyProperty]
        public ActionResult CategoryList(string propertyId)
        {
            int getUserId = 0;
            int UserType = 0;
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    getUserId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    UserType = Convert.ToInt32(Request.Cookies["Login"].Values["UserType"]);
                }
            }

            var categories = categoryService.GetCategories(UserType);
            CategoryPropertyModel categoryPropertyModel = new CategoryPropertyModel();
            if (!string.IsNullOrEmpty(propertyId))
            {
                categoryPropertyModel.Property_List_Id = string.Join(",", propertyId);
            }

            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Name = culVal == "fr-FR" ? categories[i].Name_French
                : culVal == "ru-RU" ? categories[i].Name_Russian
                : culVal == "he-IL" ? categories[i].Name_Hebrew
                : categories[i].Name;

                categories[i].Description = culVal == "fr-FR" ? categories[i].Description_French
                : culVal == "ru-RU" ? categories[i].Description_Russian
                : culVal == "he-IL" ? categories[i].Description_Hebrew
                : categories[i].Description;
            }
            categoryPropertyModel.CategoryList = categories;
            return View(categoryPropertyModel);
        }

        [VerifyProperty]
        public ActionResult SubCategoryList(int id, string Name, string propertyId)
        {
            // check if laundry then redirect to the laundry order page
            if (id == 25)
            {
                return RedirectToAction("Index", "Laundry", new
                {
                    propertyId
                });
            }
            var categories = categoryService.GetSubCategoryByCatId(id);
            var propertyData = string.IsNullOrEmpty(propertyId) ? null : orderService.GetProperty(long.Parse(propertyId));

            CategoryPropertyModel categoryPropertyModel = new CategoryPropertyModel();
            categoryPropertyModel.Property_List_Id = propertyId;

            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
            for (int i = 0; i < categories.Count; i++)
            {
                if (propertyData != null && (categories[i].Id == 12 || categories[i].Id == 26))
                {
                    var priceModel = propertyService.AutoPriceService(long.Parse(propertyId), categories[i].Id);
                    categories[i].Price = priceModel.ClientPrice;

                }
                categories[i].Name = culVal == "fr-FR" ? categories[i].Name_French
                : culVal == "ru-RU" ? categories[i].Name_Russian
                : culVal == "he-IL" ? categories[i].Name_Hebrew
                : categories[i].Name;

                categories[i].Description = culVal == "fr-FR" ? categories[i].Description_French
                : culVal == "ru-RU" ? categories[i].Description_Russian
                : culVal == "he-IL" ? categories[i].Description_Hebrew
                : categories[i].Description;
            }

            categoryPropertyModel.CategoryList = categories;
            TempData["CategoryName"] = Name;
            Session["CategoryId"] = id;
            return View(categoryPropertyModel);
        }

        [VerifyProperty]
        public ActionResult SubSubCategoryList(int id, string propertyId)
        {
            if (propertyId == "")
            {
                propertyId = "0";
            }
            var categories = categoryService.GetSubSubCategoryBySubCatId(id);
            SubCategoryPropertyModel subCategoryPropertyModel = new SubCategoryPropertyModel();
            subCategoryPropertyModel.Property_List_Id = propertyId;
            if (categories.Count > 0)
            {
                TempData["PropertyId"] = propertyId;
                var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].Name = culVal == "fr-FR" ? categories[i].Name_French
                    : culVal == "ru-RU" ? categories[i].Name_Russian
                    : culVal == "he-IL" ? categories[i].Name_Hebrew
                    : categories[i].Name;

                    categories[i].Description = culVal == "fr-FR" ? categories[i].Description_French
                    : culVal == "ru-RU" ? categories[i].Description_Russian
                    : culVal == "he-IL" ? categories[i].Description_Hebrew
                    : categories[i].Description;

                    if (propertyId != "0" && (id == 12 || id == 26))
                    {
                        var priceModel = propertyService.AutoPriceService(long.Parse(propertyId), id);
                        categories[i].Price = priceModel.ClientPrice;

                    }

                }
                subCategoryPropertyModel.SubCategoryList = categories;
                return View(subCategoryPropertyModel);
            }
            else
            {

                var subCategoryData = categoryService.GetSubCategoryById(id);
                var cat = categoryService.GetCategoryById(subCategoryData.CatId.Value);

                return RedirectToAction("AddJobRequest", new
                {
                    categoryId = subCategoryData.CatId,
                    subCategoryId = id,
                    subSubCategoryId = 0,
                    propertyId
                });
                // check if logged in user is subuser
                // chceck if category SubUserOrder value true then proceed
                // otherwise redirect to RequestOwner page
                //if (cat != null && cat.SubUserOrder.Value == false)
                //{
                //    return Redirect(Request.UrlReferrer.ToString());
                //}
                //else
                //{
                //    return RedirectToAction("AddJobRequest", new
                //    {
                //        categoryId = subCategoryData.CatId,
                //        subCategoryId = id,
                //        subSubCategoryId = 0,
                //        propertyId = propertyId
                //    });
                //}
            }
        }

        #endregion

        #region Convert Html To Image
        [HttpGet]
        public void btnConvert_Click()
        {
            //var source = @"  
            //    <!DOCTYPE html>  
            //        <html>  
            //            <head>  
            //                <style>  
            //                    table {  
            //                      font-family: arial, sans-serif;  
            //                      border-collapse: collapse;  
            //                      width: 100%;  
            //                    }  

            //                    td, th {  
            //                      border: 1px solid #dddddd;  
            //                      text-align: left;  
            //                      padding: 8px;  
            //                    }  

            //                    tr:nth-child(even) {  
            //                      background-color: #dddddd;  
            //                    }  

            //                    textarea {
            //                        border: none; 
            //                        visibility: visible;    
            //                        margin: 0px; 
            //                        padding: 2px; 
            //                        position: absolute; 
            //                        top: 84px; 
            //                        left: 333px; 
            //                        background: none;   
            //                        color: rgb(235, 7, 7); 
            //                        font-size: 12px;    
            //                        height: 24px; 
            //                        width: 150px;
            //                        overflow: hidden;
            //                        outline: none;
            //                        box-shadow: none; 
            //                        resize: none;
            //                    }
            //              </style>  
            //             </head>  
            //        <body>  

            //            <h2>HTML Table</h2>  

            //            <textarea>
            //                <h2>Demo TextArea </h2>
            //            </textarea>
            //         </body>  
            //        </html> ";

            // var source = @"<textarea style='border: none; visibility: visible; margin: 0px; padding: 2px; position: absolute; top: 84px; left: 333px; background: none; color: rgb(235, 7, 7); font-size: 12px; height: 24px; width: 150px; overflow: hidden; outline: none; box-shadow: none; resize: none;\'>hgfh</textarea><textarea style='border: none; visibility: visible; margin: 0px; padding: 2px; position: absolute; top: 158px; left: 161px; background: none; color: rgb(235, 7, 7); font - size: 12px; height: 24px; width: 150px; overflow: hidden; outline: none; box - shadow: none; resize: none;'>hgfhfghgf</textarea><textarea style='border: none; visibility: visible; margin: 0px; padding: 2px; position: absolute; top: 181px; left: 603px; background: none; color: rgb(235, 7, 7); font - size: 12px; height: 24px; width: 150px; overflow: hidden; outline: none; box - shadow: none; resize: none; '>grasss</textarea>";
            var source = @"<img class='' src='https://appmantechnologies.com:6082/images/user/iisstart.png'></img>";

            //string htmlText = source;
            ////string plaintext = HtmlToPlainText(htmlText);
            //string text = source;
            //Bitmap bitmap = new Bitmap(700, 300);
            //Font font = new Font("Arial", 25, FontStyle.Regular, GraphicsUnit.Pixel);
            //Graphics graphics = Graphics.FromImage(bitmap);
            //int width = 700;
            //int height = 300;
            //bitmap = new Bitmap(bitmap, new Size(width, height));
            //graphics = Graphics.FromImage(bitmap);
            //graphics.Clear(Color.White);
            //graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            //graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(255, 0, 0)), 0, 0);
            //graphics.Flush();
            //graphics.Dispose();
            //string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".jpg";
            //bitmap.Save(Server.MapPath("~/images/") + fileName, ImageFormat.Jpeg);
            //////imgText.ImageUrl = "~/images/" + fileName;
            //////imgText.Visible = true;
            /////

            // Create a HTML to Image converter object with default settings
            HtmlToImageConverter htmlToImageConverter = new HtmlToImageConverter();

            // Set license key received after purchase to use the converter in licensed mode
            // Leave it not set to use the converter in demo mode
            // htmlToImageConverter.LicenseKey = "4W9+bn19bn5ue2B+bn1/YH98YHd3d3c=";

            // Set HTML Viewer width in pixels which is the equivalent in converter of the browser window width
            htmlToImageConverter.HtmlViewerWidth = int.Parse("700");
            htmlToImageConverter.HtmlViewerHeight = int.Parse("300");
            // Set HTML viewer height in pixels to convert the top part of a HTML page 
            // Leave it not set to convert the entire HTML


            // Set if the created image has a transparent background
            htmlToImageConverter.TransparentBackground = false;

            // Set the maximum time in seconds to wait for HTML page to be loaded 
            // Leave it not set for a default 60 seconds maximum wait time
            //htmlToImageConverter.NavigationTimeout = int.Parse(navigationTimeoutTextBox.Text);

            // Set an adddional delay in seconds to wait for JavaScript or AJAX calls after page load completed
            // Set this property to 0 if you don't need to wait for such asynchcronous operations to finish


            System.Drawing.Image[] imageTiles = null;

            string htmlString = source;
            string baseUrl = "https://www.evopdf.com/html-to-image-converter.aspx";

            // Convert a HTML string with a base URL to a set of Image objects
            imageTiles = htmlToImageConverter.ConvertHtmlToImageTiles(htmlString, baseUrl);

            // Save the first image tile to a memory buffer

            System.Drawing.Image outImage = imageTiles[0];

            // Create a memory stream where to save the image
            System.IO.MemoryStream imageOutputStream = new System.IO.MemoryStream();

            // Save the image to memory stream
            outImage.Save(imageOutputStream, ImageFormat.Jpeg);

            string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".jpg";
            outImage.Save(Server.MapPath("~/images/") + fileName, ImageFormat.Jpeg);

            // Write the memory stream to a memory buffer
            imageOutputStream.Position = 0;
            byte[] outImageBuffer = imageOutputStream.ToArray();

            // Close the output memory stream
            imageOutputStream.Close();

            // Send the image as response to browser

            string imageFormatName = "jpg";

            // Set response content type
            Response.AddHeader("Content-Type", "image/" + (imageFormatName == "jpg" ? "jpeg" : imageFormatName));

            // Instruct the browser to open the image file as an attachment or inline
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}; size={1}", "HTML_to_Image." + imageFormatName, outImageBuffer.Length.ToString()));

            // Write the image buffer to HTTP response
            Response.BinaryWrite(outImageBuffer);

            // End the HTTP response and stop the current page processing
            Response.End();


        }
        //private static string HtmlToPlainText(string html)
        //    {
        //        const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
        //        const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
        //        const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
        //        var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
        //        var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
        //        var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

        //        var text = html;
        //        //Decode html specific characters
        //        text = System.Net.WebUtility.HtmlDecode(text);
        //        //Remove tag whitespace/line breaks
        //        text = tagWhiteSpaceRegex.Replace(text, "><");
        //        //Replace <br /> with line breaks
        //        text = lineBreakRegex.Replace(text, Environment.NewLine);
        //        //Strip formatting
        //        text = stripFormattingRegex.Replace(text, string.Empty);

        //        return text;
        //    }

        #endregion

        #region Add Job Request
        [VerifyUser]
        [VerifyProperty]
        public ActionResult AddJobRequest(long categoryId, long subCategoryId, long subSubCategoryId, string propertyId, long? jobReqId)
        {
            ViewBag.TimeToDo = 1;
            int userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
            if (string.IsNullOrEmpty(propertyId)) propertyId = "0";
            var prop = propertyService.GetPropertiesById(Convert.ToInt64(propertyId));
            var user = propertyService.GetUser(userId);
            if (user != null && user.JobPayType != null) Request.Cookies["Login"].Values["JobTypeMethod"] = Convert.ToString(user.JobPayType.Value);
            var categoryData = categoryService.GetCategoryById(categoryId);
            if (categoryData != null && user?.UserType == 7 && categoryData.SubUserSendRequest == true)
            {
                if (prop != null)
                {
                    var userTo = propertyService.GetUser((int)prop.CreatedBy.Value);
                    if (userTo != null)
                    {
                        string link = "/Order/AddJobRequest?categoryId=" + categoryId + "&subCategoryId=" + subCategoryId + "&subSubCategoryId=" + subSubCategoryId + "&propertyId=" + propertyId;
                        Common.SendReportToPropertyOwner(user.FullName, prop.Name, categoryData.Name, userTo.FullName, userTo.Email, link);
                        string Body = "";
                        Body = Resource.hello + " " + userTo.FullName + ",<br/><br/>";
                        Body += "<b> Please Order following service for Fix & Install</b>"
                                + "<br/>" +
                                "Renter Name: " + user.FullName +
                                "<br/>" +
                                "Porperty Name: " + prop.Name +
                                "<br/>" +
                                "Category Name: " + categoryData.Name;
                        Body += "<br />";
                        Body += Resource.thank_you;

                        propertyService.SendNotification(user.UserId, jobReqId, userTo.UserId, Body, "Fix & Install", prop.Name, prop.Address);
                        TempData["SuccessMsg"] = "Report successfully sent to owner of propery.";
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Owner to send report not found";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Property not found.";
                }

                return RedirectToAction("PropertyList", "Property", new
                {
                    pageNumber = 1,
                    type = "increment"
                });

            }
            else
            {
                //Get Price
                ServicePriceViewModel priceModel;
                if (prop != null && (subCategoryId == 12 || subCategoryId == 26))
                {
                    priceModel = propertyService.AutoPriceService(long.Parse(propertyId), subCategoryId);
                    ViewBag.TimeToDo = priceModel.TimeToDo;

                }
                else
                {
                    priceModel = orderService.GetPriceService(subCategoryId, subSubCategoryId);
                }

                Session["Price"] = priceModel.ClientPrice;
                TempData["Price"] = priceModel.ClientPrice;
                TempData.Keep("Price");

                TempData["categoryId"] = categoryId;
                TempData["subCategoryId"] = subCategoryId;
                TempData["subSubCategoryId"] = subSubCategoryId;
                TempData.Keep("categoryId");
                TempData.Keep("subCategoryId");
                TempData.Keep("subSubCategoryId");

                //Get Service Schedule
                ServiceData serviceData = new ServiceData();
                serviceData.SubCategoryId = subCategoryId;
                serviceData.SubSubCategoryyId = subSubCategoryId;
                serviceData.Day = (int)DateTime.Now.DayOfWeek;
                JobRequestViewModel jobRequestViewModel = new JobRequestViewModel();
                var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;


                if (categoryData != null)
                {
                    categoryData.Name = culVal == "fr-FR" ? categoryData.Name_French
                        : culVal == "ru-RU" ? categoryData.Name_Russian
                        : culVal == "he-IL" ? categoryData.Name_Hebrew
                        : categoryData.Name;
                    jobRequestViewModel.CategoryData = categoryData;
                }

                var subCategoryData = categoryService.GetSubCategoryById(subCategoryId);
                if (subCategoryData != null)
                {
                    subCategoryData.Name = culVal == "fr-FR" ? subCategoryData.Name_French
                    : culVal == "ru-RU" ? subCategoryData.Name_Russian
                    : culVal == "he-IL" ? subCategoryData.Name_Hebrew
                    : subCategoryData.Name;

                    jobRequestViewModel.SubCategoryData = subCategoryData;
                }

                if (subSubCategoryId != 0)
                {
                    var subSubCategoryData = categoryService.GetSubSubCategoryById(subSubCategoryId);
                    if (subSubCategoryData != null)
                    {
                        subSubCategoryData.Name = culVal == "fr-FR" ? subSubCategoryData.Name_French
                        : culVal == "ru-RU" ? subSubCategoryData.Name_Russian
                        : culVal == "he-IL" ? subSubCategoryData.Name_Hebrew
                        : subSubCategoryData.Name;

                        jobRequestViewModel.SubSubCategoryData = subSubCategoryData;
                    }
                }

                var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);

                jobRequestViewModel.JobStartDateTime = localNow.ToShortDateString();
                jobRequestViewModel.JobEndDateTime = localNow.ToShortTimeString();

                if (!string.IsNullOrEmpty(propertyId))
                {
                    jobRequestViewModel.Property_List_Id = new List<long>();
                    var splitId = propertyId.Split(',');
                    foreach (var item in splitId)
                    {
                        var id = Convert.ToInt32(item);
                        jobRequestViewModel.Property_List_Id.Add(id);
                    }
                }

                ViewBag.Properties = propertyService.GetPropertiesSelect(userId);

                //ViewBag.Inventory = propertyService.GetInventory();
                var objInventory = propertyService.GetInventory();

                if (jobReqId != null)
                {
                    var JobReqPropService = propertyService.GetJobReqPropServiceByJobId((long)jobReqId);
                    var objJobInventory = propertyService.GetInventoryByJobReqId(JobReqPropService.JobRequestPropId, Convert.ToInt64(propertyId));
                    foreach (var item in objJobInventory)
                    {
                        foreach (var item2 in objInventory)
                        {
                            if (item.InventoryId == item2.InventoryId)
                            {
                                item2.Qty = item.Qty;
                            }
                        }
                    }
                }

                ViewBag.Inventory = objInventory;
                if (jobReqId != null)
                {
                    ViewBag.CheckList = propertyService.GetJobReqChecklistByJobId((long)jobReqId);
                }

                if (priceModel.ClientPrice > 0)
                {
                    jobRequestViewModel.HasPrice = true;
                }
                return View(jobRequestViewModel);
            }
        }

        [HttpPost]
        [VerifyUser]
        public JsonResult AddJobRequest(System.Web.Mvc.FormCollection formCollection)
        {
            JobRequestViewModel model = new JobRequestViewModel();
            try
            {
                var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                model.UserId = userId;
                model.IsVisitor = false;
                if (formCollection["JobStartDateTime"] != null)
                {
                    model.JobStartDateTime = formCollection["JobStartDateTime"];
                }
                if (formCollection["JobEndDateTime"] != null)
                {
                    model.JobEndDateTime = formCollection["JobEndDateTime"];
                }
                if (formCollection["ServicePrice"] != null)
                {
                    model.ServicePrice = formCollection["ServicePrice"];
                }
                if (formCollection["PaymentInfo"] != null)
                {
                    model.PaymentInfo = Convert.ToInt32(formCollection["PaymentInfo"]);
                }

                if (formCollection["IsPaymentDone"] != null)
                {
                    model.isPaymentDone = Convert.ToBoolean(formCollection["IsPaymentDone"]);
                }
                if (formCollection["IsVisitor"] != null)
                {
                    model.IsVisitor = Convert.ToBoolean(formCollection["IsVisitor"]);
                    model.UserId = Convert.ToInt64(formCollection["UserId"]);
                }
                if (formCollection["CheckList"] != null)
                {
                    var checklist = formCollection["CheckList"].ToString();
                    model.CheckList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(checklist);
                }
                model.JobDesc = formCollection["JobDesc"];
                model.IsFastOrder = Convert.ToBoolean(formCollection["IsFastOrder"]);
                model.FastOrderName = formCollection["FastOrderName"];
                model.PropertyId = Convert.ToInt64(formCollection["PropertyId"]);
                if (formCollection["Categories"] != null)
                {
                    var subCategory = formCollection["Categories"].ToString();
                    model.Categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceData>>(subCategory);
                }

                if (formCollection["PropertyService"] != null)
                {
                    var subCategory = formCollection["PropertyService"].ToString();
                    model.PropertyService = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PropertyServiceData>>(subCategory);
                }

                if (formCollection["Property_List_Id"] != null)
                {
                    model.Property_List_Id = new List<long>();
                    var propertyId = formCollection["Property_List_Id"].ToString();
                    var _propertyId = propertyId.Split(',');
                    for (int i = 0; i < _propertyId.Count(); i++)
                    {
                        var convertPropertyId = Convert.ToInt32(_propertyId[i]);
                        model.Property_List_Id.Add(convertPropertyId);
                    }
                }
                if (formCollection["InventoryList"] != null)
                {
                    var inventoryList = formCollection["InventoryList"].ToString();
                    model.InventoryList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InventoryItems>>(inventoryList);
                }
                if (formCollection["DocInvoice"] != null)
                {
                    var subCategory = formCollection["DocInvoice"].ToString();
                    model.DocumentJob = Newtonsoft.Json.JsonConvert.DeserializeObject<ICountResponse>(subCategory);
                }
                if (formCollection["ReferenceImages"] != "" && formCollection["ReferenceImages"] != null)
                {
                    var json = formCollection["ReferenceImages"].ToString();
                    List<Attachments> getAttachments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Attachments>>(json);

                    var date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss").Replace("-", "_");

                    List<ChecklistImageVM> checklistImages = new List<ChecklistImageVM>();

                    if (getAttachments.Count > 0)
                    {
                        for (int i = 0; i < getAttachments.Count(); i++)
                        {
                            string[] splitAttachment = getAttachments[i].result.Split(',');

                            if (getAttachments[i].type == "image")
                            {
                                string imageName = "JobCheckList" + Guid.NewGuid() + date + ".jpg";
                                Common.SaveImage(splitAttachment[1], imageName);

                                checklistImages.Add(new ChecklistImageVM
                                {
                                    IsImage = true,
                                    ImageUrl = imageName
                                });
                            }
                            else
                            {
                                string imageName = "JobCheckList" + Guid.NewGuid() + date + ".mp4";
                                Common.SaveImage(splitAttachment[1], imageName);

                                checklistImages.Add(new ChecklistImageVM
                                {
                                    IsVideo = true,
                                    VideoUrl = imageName
                                });
                            }
                        }
                    }
                    model.ReferenceImages = checklistImages;
                }

                if (formCollection["ServiceId"] != null)
                {
                    model.ServiceId = Convert.ToInt64(formCollection["ServiceId"]);
                }
                var jobRequestSuccess = orderService.AddJobRequest(model);
                string saleUrl = "";

                if (jobRequestSuccess.HasPrice && jobRequestSuccess.Status)
                {
                    var user = propertyService.GetUser(userId);
                    var countSalesResponse = orderService.GeneratePaymentPage(jobRequestSuccess.JobRequest, user);
                    if (countSalesResponse != null && countSalesResponse.status)
                    {
                        saleUrl = countSalesResponse.sale_url;
                    }
                }

                wsBase obj = new wsBase()
                {
                    status = jobRequestSuccess.Status,
                    message = orderService.message,
                    description = orderService.description,
                    SaleUrl = saleUrl
                };
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                wsBase obj = new wsBase()
                {
                    status = false,
                    message = ex.Message,
                    description = "",
                    unitprice = orderService.unitprice,
                    JobRequestID = orderService.JobReqId,
                    jobTimeAlert = false
                };
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GeneratePayPage(long jobRequestId)
        {
            try
            {
                var userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                var user = propertyService.GetUser(userId);

                var jobRequest = orderService.GetJobRequest(jobRequestId);
                string saleUrl = "";
                if (jobRequest != null)
                {
                    var countSalesResponse = orderService.GeneratePaymentPage(jobRequest, user);
                    if (countSalesResponse != null && countSalesResponse.status)
                    {
                        saleUrl = countSalesResponse.sale_url;
                    }
                }
                wsBase obj = new wsBase()
                {
                    status = saleUrl != "",
                    message = orderService.message,
                    description = orderService.description,
                    SaleUrl = saleUrl
                };
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                wsBase obj = new wsBase()
                {
                    status = false
                };
                return Json(obj, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult InventoryView(string Property_List_Id)
        {
            List<long> PropertyList = new List<long>();
            if (!string.IsNullOrEmpty(Property_List_Id))
            {
                string[] splitId = Property_List_Id.Split(',');
                for (int i = 0; i < splitId.Length; i++)
                {
                    var item = Convert.ToInt32(splitId[i]);
                    PropertyList.Add(item);
                }
            }
            ViewBag.Properties = propertyService.GetPropertiesSelectForInventory(PropertyList);
            var getItems = orderService.GetInventoryList();
            return PartialView("InventoryView", getItems);
        }


        public JsonResult GetCategories()
        {
            int getUserId = 0;
            int UserType = 0;
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    getUserId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    UserType = Convert.ToInt32(Request.Cookies["Login"].Values["UserType"]);
                }
            }

            var categories = categoryService.GetCategories(UserType);
            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSubCategories(int category_id)
        {
            var categories = categoryService.GetSubCategoryByCatId(category_id);
            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSubSubCategories(int sub_category_id)
        {
            var categories = categoryService.GetSubSubCategoryBySubCatId(sub_category_id);
            return Json(categories, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPropertyList()
        {
            int userId = 0;
            var data = new List<SelectListItem>();
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    data = propertyService.GetPropertiesSelect(userId);
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public int GetPropertyCount(long UserId)
        {
            var PropCount = propertyService.GetUserPropertyCount(UserId);
            return PropCount;
        }
        #endregion

        #region Visitor AddJobRequest
        public ActionResult VisitorAddJobRequest(long categoryId, long subCategoryId, long subSubCategoryId)
        {
            //Get Price
            var getPrice = orderService.GetPriceService(subCategoryId, subSubCategoryId);
            Session["Price"] = getPrice;
            TempData["Price"] = getPrice;
            TempData.Keep("Price");

            TempData["categoryId"] = categoryId;
            TempData["subCategoryId"] = subCategoryId;
            TempData["subSubCategoryId"] = subSubCategoryId;
            TempData.Keep("categoryId");
            TempData.Keep("subCategoryId");
            TempData.Keep("subSubCategoryId");

            //Get Service Schedule
            ServiceData serviceData = new ServiceData();
            serviceData.SubCategoryId = subCategoryId;
            serviceData.SubSubCategoryyId = subSubCategoryId;
            // serviceData.Day = 2;
            serviceData.Day = (int)DateTime.Now.DayOfWeek;
            JobRequestViewModel jobRequestViewModel = new JobRequestViewModel();
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;

            var categoryData = categoryService.GetCategoryById(categoryId);
            if (categoryData != null)
            {
                categoryData.Name = culVal == "fr-FR" ? categoryData.Name_French
                    : culVal == "ru-RU" ? categoryData.Name_Russian
                    : culVal == "he-IL" ? categoryData.Name_Hebrew
                    : categoryData.Name;
                jobRequestViewModel.CategoryData = categoryData;
            }

            var subCategoryData = categoryService.GetSubCategoryById(subCategoryId);
            if (subCategoryData != null)
            {
                subCategoryData.Name = culVal == "fr-FR" ? subCategoryData.Name_French
                : culVal == "ru-RU" ? subCategoryData.Name_Russian
                : culVal == "he-IL" ? subCategoryData.Name_Hebrew
                : subCategoryData.Name;

                jobRequestViewModel.SubCategoryData = subCategoryData;
            }

            if (subSubCategoryId != 0)
            {
                var subSubCategoryData = categoryService.GetSubSubCategoryById(subSubCategoryId);
                if (subSubCategoryData != null)
                {
                    subSubCategoryData.Name = culVal == "fr-FR" ? subSubCategoryData.Name_French
                    : culVal == "ru-RU" ? subSubCategoryData.Name_Russian
                    : culVal == "he-IL" ? subSubCategoryData.Name_Hebrew
                    : subSubCategoryData.Name;

                    jobRequestViewModel.SubSubCategoryData = subSubCategoryData;
                }
            }

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);

            jobRequestViewModel.JobStartDateTime = localNow.ToShortDateString();
            jobRequestViewModel.JobEndDateTime = localNow.ToShortTimeString();

            ViewBag.Properties = propertyService.GetAllAirbnbProperty();
            ViewBag.Inventory = propertyService.GetInventory();

            return View(jobRequestViewModel);
        }

        #endregion

        #region Edit Job
        public ActionResult EditJobRequest(long JobReqId)
        {
            int userId = 0;
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                }
            }
            JobRequestSubSubCategory jobReqCat = new JobRequestSubSubCategory();
            jobReqCat = categoryService.GetCategoryDetailByJobId(JobReqId);

            EditJobRequestViewModel EditjobRequest = new EditJobRequestViewModel();
            EditjobRequest = orderService.GetJobDetail(JobReqId, (long)jobReqCat.JobRequestId);
            EditjobRequest.JobReqId = JobReqId;
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
            var categoryData = categoryService.GetCategoryById((long)jobReqCat.CategoryId);
            if (categoryData != null)
            {
                categoryData.Name = culVal == "fr-FR" ? categoryData.Name_French
                    : culVal == "ru-RU" ? categoryData.Name_Russian
                    : culVal == "he-IL" ? categoryData.Name_Hebrew
                    : categoryData.Name;
                EditjobRequest.CategoryData = categoryData;
            }
            var subCategoryData = categoryService.GetSubCategoryById((long)jobReqCat.SubCategoryId);
            if (subCategoryData != null)
            {
                subCategoryData.Name = culVal == "fr-FR" ? subCategoryData.Name_French
                : culVal == "ru-RU" ? subCategoryData.Name_Russian
                : culVal == "he-IL" ? subCategoryData.Name_Hebrew
                : subCategoryData.Name;
                EditjobRequest.SubCategoryData = subCategoryData;
            }

            if (jobReqCat.SubSubCategoryId != 0)
            {
                var subSubCategoryData = categoryService.GetSubSubCategoryById((long)jobReqCat.SubSubCategoryId);
                if (subSubCategoryData != null)
                {
                    subSubCategoryData.Name = culVal == "fr-FR" ? subSubCategoryData.Name_French
                    : culVal == "ru-RU" ? subSubCategoryData.Name_Russian
                    : culVal == "he-IL" ? subSubCategoryData.Name_Hebrew
                    : subSubCategoryData.Name;

                    EditjobRequest.SubSubCategoryData = subSubCategoryData;
                }
            }
            ViewBag.Properties = propertyService.GetPropertiesSelect(userId);

            var objInventory = propertyService.GetInventory();
            if (EditjobRequest.Property_List_Id != 0)
            {

                var JobReqPropService = propertyService.GetJobReqPropServiceByJobId(JobReqId);
                var objJobInventory = propertyService.GetInventoryByJobReqId(JobReqPropService.JobRequestPropId, EditjobRequest.Property_List_Id);
                foreach (var item in objJobInventory)
                {
                    foreach (var item2 in objInventory)
                    {
                        if (item.InventoryId == item2.InventoryId)
                        {
                            item2.Qty = item.Qty;
                        }
                    }
                }
            }
            ViewBag.Inventory = objInventory;
            if (JobReqId != 0)
            {
                ViewBag.CheckList = propertyService.GetJobReqChecklistByJobId((long)JobReqId);
            }
            return View(EditjobRequest);
        }
        [HttpPost]
        public bool EditJobRequest(System.Web.Mvc.FormCollection formCollection)
        {
            EditJobRequestViewModel model = new EditJobRequestViewModel();
            if (Request.Cookies["Login"] != null)
            {
                if (Request.Cookies["Login"].Values["UserId"] != null)
                {
                    model.UserId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]);
                    model.UserName = Request.Cookies["Login"].Values["UserName"] != null ? Request.Cookies["Login"].Values["UserName"].ToString() : Request.Cookies["Login"].Values["CompanyName"].ToString();
                }
            }
            if (formCollection["JobStartDateTime"] != null)
            {
                model.JobStartDateTime = Convert.ToDateTime(formCollection["JobStartDateTime"]);
            }
            if (formCollection["JobEndDateTime"] != null)
            {
                model.JobEndDateTime = Convert.ToDateTime(formCollection["JobEndDateTime"]);
            }
            if (formCollection["Property_List_Id"] != null)
            {
                model.Property_List_Id = Convert.ToInt64(formCollection["Property_List_Id"]);
            }
            if (formCollection["JobDesc"] != null)
            {
                model.JobDesc = formCollection["JobDesc"];
            }
            if (formCollection["JobReqId"] != null)
            {
                model.JobReqId = Convert.ToInt64(formCollection["JobReqId"]);
            }
            if (formCollection["AssignWorker"] != null)
            {
                model.AssignWorker = Convert.ToInt64(formCollection["AssignWorker"]);
            }
            if (formCollection["InventoryList"] != null)
            {
                var inventoryList = formCollection["InventoryList"].ToString();
                model.InventoryList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InventoryViewModel>>(inventoryList);
            }
            var status = orderService.EditjobDetails(model);
            return true;
        }
        #endregion

        #region Fast Orders
        [VerifyUser]
        public ActionResult FastOrderList()
        {
            int userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]); ;
            var getData = orderService.GetFastOrdersForUsers(userId);
            return View(getData);
        }

        [VerifyUser]
        public ActionResult GetFastOrdersByPropertyId(int PropertyId)
        {
            int userId = Convert.ToInt32(Request.Cookies["Login"].Values["UserId"]); ;
            var getData = orderService.GetFastOrdersByPropertyId(PropertyId, userId);
            return View("/Views/Order/FastOrderList.cshtml", getData);
        }
        #endregion

        public ActionResult PaymentSuccess(string description)
        {
            @ViewBag.description = description;
            return View();
        }
        public ActionResult PaymentCancel(string description)
        {
            @TempData["ErrorMsg"] = "Payment has been cancelled for the following service.";
            @ViewBag.description = description;
            return View("/Views/Order/PaymentCancel.cshtml");
        }
        public ActionResult PaymentFailiur(string description, string strMessage = "Payment has been Failed for the following service.")
        {
            @TempData["ErrorMsg"] = strMessage;
            @ViewBag.description = description;
            return View("/Views/Order/PaymentCancel.cshtml");
        }


        public bool AddMeetingRequest(string MeetingDate, string MeetingTime)
        {
            bool status = false;

            if (Request.Cookies["Login"].Values["UserId"] != null)
            {
                var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                if (userId != 0)
                {
                    status = orderService.AddMeetingSchedule(MeetingDate, MeetingTime, userId);
                }
            }
            return status;
        }

        [HttpPost]
        public JsonResult AvailableServiceTime(ServiceData serviceData)
        {
            // if subSubCategoy then massage service
            // check who this category belongs to worker or service provider
            var user_type = Enums.UserTypeEnum.Worker.GetHashCode();
            var category = orderService.GetCategory((long)serviceData.CategoryId);
            if (category != null)
            {
                if (!(bool)category.ForWorkers)
                {
                    user_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();
                }
            }
            var availableTimes = orderService.GetAvailabilities(serviceData, user_type);
            return Json(new { availableTimes }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult WorkersDistance(long propertyId, long categoryId)
        {
            var user_type = Enums.UserTypeEnum.Worker.GetHashCode();
            var category = orderService.GetCategory(categoryId);
            if (category != null)
            {
                if (!(bool)category.ForWorkers)
                {
                    user_type = Enums.UserTypeEnum.ServiceProvider.GetHashCode();
                }
            }
            var workersDistance = orderService.GetWorkersDistance(propertyId, user_type);
            return Json(workersDistance, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult BookedWorkers(long propertyId)
        {
            var workersDistance = orderService.GetBookedTimes(propertyId);
            return Json(workersDistance, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AvailableTime(string dayofweek, string subCategoryId, string subSubCategoryId, string JobDate, string price, long propertyId = 0)
        {
            //long AssignedId = 0;
            ServiceData serviceData = new ServiceData();
            serviceData.SubCategoryId = long.Parse(subCategoryId);
            serviceData.SubSubCategoryyId = long.Parse(subSubCategoryId);
            // serviceData.Day = 2; 
            serviceData.Day = int.Parse(dayofweek);
            //ViewBag.Schedule
            decimal TimeToDo = 2;
            if (propertyId > 0 && (serviceData.SubCategoryId == 12 || serviceData.SubCategoryId == 26))
            {
                TimeToDo = GetTimeToDo(propertyId, serviceData.SubCategoryId);
            }

            //AssignedId = orderService.GetTimingsAvaiableWorker(serviceData, JobDate, TimeToDo);
            var obj = orderService.GetTimingsAvaiableWorker(serviceData, JobDate, TimeToDo);
            // ViewBag.AvailableTimingSlot = obj;
            //return AssignedId;
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LaundryAvailableTime(string dayofweek, long categoryId, string jobDate, long propertyId)
        {
            var serviceData = new ServiceData
            {
                CategoryId = categoryId,
                Day = int.Parse(dayofweek)
            };
            var obj = orderService.IsAvailableLaundryOrInventory(serviceData, jobDate);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AvailableDate(string dayofweek, string subCategoryId, string subSubCategoryId, string JobDate, string price, long propertyId = 0)
        {
            // bool status = false;
            List<AvailablityViewModel> obj = new List<AvailablityViewModel>();
            //long AssignedId = 0;
            ServiceData serviceData = new ServiceData();
            serviceData.SubCategoryId = long.Parse(subCategoryId);
            serviceData.SubSubCategoryyId = long.Parse(subSubCategoryId);
            // serviceData.Day = 2; 
            serviceData.Day = int.Parse(dayofweek);
            //ViewBag.Schedule
            decimal TimeToDo = 2;
            if (propertyId > 0 && (serviceData.SubCategoryId == 12 || serviceData.SubCategoryId == 26))
            {
                TimeToDo = GetTimeToDo(propertyId, serviceData.SubCategoryId);
            }
            else
            {
                TimeToDo = price != null ? Convert.ToInt32(Convert.ToInt32(price) / 110) : 0;
            }

            DateTime oJobDate;
            for (int i = 1; i < 7; i++)
            {
                oJobDate = DateTime.Now.AddDays(i);
                int vAvailablity = orderService.IsWorkerAvailable(serviceData, oJobDate, TimeToDo);
                obj.Add(new AvailablityViewModel
                {
                    availablity = vAvailablity,
                    date = oJobDate
                });
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        private decimal GetTimeToDo(long propertyId, long? subCategoryId)
        {
            decimal returnVal = 2;
            var propertyData = orderService.GetProperty(propertyId);
            if (propertyData != null)
            {
                if (propertyData.Size == "1-25")
                {
                    returnVal = 1;
                }
                else if (propertyData.Size == "1-45")
                {
                    returnVal = 2;
                }
                else if (propertyData.Size == "2-65")
                {
                    returnVal = 3;
                }
                else if (propertyData.Size == "3-85")
                {
                    if (subCategoryId.Value == 12)
                    {
                        returnVal = 4;
                    }
                    else
                    {
                        returnVal = 3.5M;
                    }

                }
                else if (propertyData.Size == "4-110")
                {
                    returnVal = 5;
                }
                else if (propertyData.Size == "5-140")
                {
                    returnVal = 5.5M;
                }
            }

            return returnVal;
        }

        public bool CheckAvailability(string dayofweek, string subCategoryId, string subSubCategoryId, string JobDate, long PropId)
        {
            bool status = orderService.CheckRoomAvailability(JobDate, PropId);
            return status;
        }
        public long GetDeliveryGuy(string JobDateTime)
        {
            var date = Convert.ToDateTime(JobDateTime);
            var DayofWeek = Convert.ToInt32(date.DayOfWeek);
            //var Date = date.ToShortDateString();
            //var time = date.ToShortTimeString();
            var dd = JobDateTime.ToString().Split(' ');
            var time = dd[1];
            var Date = dd[0];
            var Sp = orderService.GetDeliveryGuy(Date, DayofWeek, time);
            return Sp;
        }

        public ActionResult Availability()
        {
            List<AvailableTimeViewModel> list = orderService.GetAvaiability();
            return View(list);
        }

        public JsonResult AvailabilityAjax()
        {
            List<AvailableTimeViewModel> list = orderService.GetAvaiability();
            return Json(new { result = list }, JsonRequestBehavior.AllowGet);
        }


        #region Quotes
        public ActionResult Quotes()
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var quotes = orderService.GetServiceQuotes(userId);
            return View(quotes);
        }

        [VerifyUser]
        public JsonResult AcceptQuotes(long jobId)
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var data = orderService.AcceptQuotes(jobId, userId);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [VerifyUser]
        public JsonResult RejectQuotes(long jobId)
        {
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            var data = orderService.RejectQuotes(jobId, userId);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion Quotes

        #region Checklist
        public bool SaveChecklist(long PropId, int ServiceId, int SubCatId, int SubSubCatId, string ChecklistName, List<CheckList> Chklist)
        {
            bool status = false;
            if (Request.Cookies["Login"].Values["UserId"] != null)
            {
                var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                status = orderService.SaveCheckList(userId, PropId, ServiceId, SubCatId, SubSubCatId, ChecklistName, Chklist);
            }
            return status;
        }
        [HttpGet]
        public JsonResult GetChecklist(long PropId, int ServiceId, int SubCatId, int SubSubCatId)
        { //Get Checklist Name
            var data = new List<tblSavedChecklist>();
            if (Request.Cookies["Login"].Values["UserId"] != null)
            {
                var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                data = orderService.GetChecklist(userId, PropId, ServiceId, SubCatId, SubSubCatId);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetChecklistText(long ChklistId)
        {
            var data = orderService.GetChecklistText(ChklistId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region AddInventory Request
        [HttpPost]
        public bool AddInventoryRequest(string InventoryName)
        {
            bool status = false;
            if (Request.Cookies["Login"].Values["UserId"] != null)
            {
                string UserName = Request.Cookies["Login"].Values["UserName"];
                var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                status = orderService.AddInventoryRequest(InventoryName, userId, UserName);
            }
            return status;
        }
        #endregion

        #region Add service Request
        public ActionResult ServiceRequests()
        {

            if (Request.Cookies["Login"].Values["UserId"] != null)
            {
                var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
                if (userId != 0)
                {
                    var data = orderService.GetAddServiceRequests(userId);
                    return View(data);
                }
            }
            return View();
        }
        public bool IsAccept_RejectServiceRequest(int status, long ServiceReqId)
        {
            /*value 1 as Accept and 2 as Reject offers*/
            bool statuss = false;
            var userId = Convert.ToInt64(Request.Cookies["Login"].Values["UserId"]);
            // var UserName = Request.Cookies["Login"].Values["UserName"].ToString();
            if (status == 1)
            {
                statuss = orderService.AcceptAddServiceRequest(ServiceReqId, userId);
            }
            else if (status == 2)
            {
                if (userId != 0)
                {
                    statuss = orderService.RejectAddServiceRequest(ServiceReqId, userId);
                }
            }
            return statuss;
        }
        #endregion


    }
}