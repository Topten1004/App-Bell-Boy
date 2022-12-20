using BroomService.Helpers;
using BroomService.Models;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace BroomService.Services
{
    public class LaundryService
    {
        readonly decimal deliveryCharge = 40;
        BroomServiceEntities1 _db;
        TokenService tokenService;
        public LaundryService()
        {
            _db = new BroomServiceEntities1();
            tokenService = new TokenService();
        }

        // LaundryStatus
        //Requested = 1,
        //Pickedup = 2,
        //LaundryReceived = 3,
        //Priced = 4,
        //Paid = 5,
        //DeliveryReceived = 6,
        //Delivered = 7

        public LaundryRequestViewModel LaundryRequestDetails(long id)
        {
            try
            {
                var laundryRequest = (from lr in _db.LaundryRequests
                                      join p in _db.Properties on lr.PropertyId equals p.Id
                                      join l in _db.Users on lr.LaundryId equals l.UserId
                                      where lr.LaundryRequestId == id
                                      select new LaundryRequestViewModel
                                      {
                                          LaundryRequestId = lr.LaundryRequestId,
                                          LaundryItems = lr.LaundryItems,
                                          IroningItems = lr.IroningItems,
                                          DryingItems = lr.DryingItems,
                                          PickupDate = lr.PickupDate,
                                          ReturnDate = lr.ReturnDate,
                                          Price = lr.Price,
                                          LaundryStatus = lr.LaundryStatus,
                                          PropertyId = lr.PropertyId,
                                          PropertyName = p.Name,
                                          PropertyAddress = p.Address,
                                          PickupGuyId = lr.PickupGuyId,
                                          ReturnGuyId = (long)lr.ReturnGuyId,
                                          LaundryId = l.UserId,
                                          LaundryName = l.FullName,
                                          LaundryAddress = l.Address
                                      }).FirstOrDefault();
                return laundryRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public List<LaundryRequestViewModel> LaundryRequests(long deliveryGuyId)
        {
            try
            {
                var laundryRequests = (from lr in _db.LaundryRequests
                                       where lr.PickupGuyId == deliveryGuyId || lr.ReturnGuyId == deliveryGuyId
                                       join p in _db.Properties on lr.PropertyId equals p.Id
                                       join l in _db.Users on lr.LaundryId equals l.UserId
                                       select new LaundryRequestViewModel
                                       {
                                           LaundryRequestId = lr.LaundryRequestId,
                                           LaundryItems = lr.LaundryItems,
                                           IroningItems = lr.IroningItems,
                                           DryingItems = lr.DryingItems,
                                           PickupDate = lr.PickupDate,
                                           ReturnDate = lr.ReturnDate,
                                           Price = lr.Price,
                                           LaundryStatus = lr.LaundryStatus,
                                           PropertyId = lr.PropertyId,
                                           PropertyName = p.Name,
                                           PropertyAddress = p.Address,
                                           PickupGuyId = lr.PickupGuyId,
                                           ReturnGuyId = (long)lr.ReturnGuyId,
                                           LaundryId = l.UserId,
                                           LaundryName = l.FullName,
                                           LaundryAddress = l.Address
                                       }).ToList();
                return laundryRequests;
            }
            catch (Exception)
            {
                return new List<LaundryRequestViewModel>();
            }
        }

        public List<LaundryRequestViewModel> GetLaundries(long userId, int laundryStatus)
        {
            try
            {
                var laundryRequests = (from lr in _db.LaundryRequests
                                       where lr.UserId == userId && lr.LaundryStatus == laundryStatus
                                       join p in _db.Properties on lr.PropertyId equals p.Id
                                       join l in _db.Users on lr.LaundryId equals l.UserId
                                       select new LaundryRequestViewModel
                                       {
                                           LaundryRequestId = lr.LaundryRequestId,
                                           LaundryItems = lr.LaundryItems,
                                           IroningItems = lr.IroningItems,
                                           DryingItems = lr.DryingItems,
                                           PickupDate = lr.PickupDate,
                                           ReturnDate = lr.ReturnDate,
                                           Price = lr.Price,
                                           LaundryStatus = lr.LaundryStatus,
                                           PropertyId = lr.PropertyId,
                                           PropertyName = p.Name,
                                           PropertyAddress = p.Address,
                                           PickupGuyId = lr.PickupGuyId,
                                           ReturnGuyId = (long)lr.ReturnGuyId,
                                           LaundryId = l.UserId,
                                           LaundryName = l.FullName,
                                           LaundryAddress = l.Address
                                       }).ToList();
                return laundryRequests;
            }
            catch (Exception)
            {
                return new List<LaundryRequestViewModel>();
            }
        }

        public List<LaundryRequestViewModel> GetLaundriesByLaundry(long laundryId)
        {
            try
            {
                var laundryRequests = (from lr in _db.LaundryRequests
                                       where lr.LaundryId == laundryId
                                       join p in _db.Properties on lr.PropertyId equals p.Id
                                       join l in _db.Users on lr.LaundryId equals l.UserId
                                       select new LaundryRequestViewModel
                                       {
                                           LaundryRequestId = lr.LaundryRequestId,
                                           LaundryItems = lr.LaundryItems,
                                           IroningItems = lr.IroningItems,
                                           DryingItems = lr.DryingItems,
                                           PickupDate = lr.PickupDate,
                                           ReturnDate = lr.ReturnDate,
                                           Price = lr.Price,
                                           LaundryStatus = lr.LaundryStatus,
                                           PropertyId = lr.PropertyId,
                                           PropertyName = p.Name,
                                           PropertyAddress = p.Address,
                                           PickupGuyId = lr.PickupGuyId,
                                           ReturnGuyId = (long)lr.ReturnGuyId,
                                           LaundryId = l.UserId,
                                           LaundryName = l.FullName,
                                           LaundryAddress = l.Address
                                       }).ToList();
                return laundryRequests;
            }
            catch (Exception)
            {
                return new List<LaundryRequestViewModel>();
            }
        }

        public bool LaundryRequest(LaundryRequestViewModel laundryRequest)
        {
            var laundryStatus = Enums.LaundryStatus.Requested.GetHashCode();
            try
            {
                var newRequest = new LaundryRequest
                {
                    LaundryId = laundryRequest.LaundryId,
                    PickupGuyId = laundryRequest.PickupGuyId,
                    ReturnGuyId = laundryRequest.ReturnGuyId,
                    PickupDate = laundryRequest.PickupDate,
                    ReturnDate = laundryRequest.ReturnDate,
                    PropertyId = laundryRequest.PropertyId,
                    LaundryItems = laundryRequest.LaundryItems,
                    IroningItems = laundryRequest.IroningItems,
                    DryingItems = laundryRequest.DryingItems,
                    LaundryStatus = laundryStatus,
                    UserId = laundryRequest.UserId
                };
                _db.LaundryRequests.Add(newRequest);
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string ItemsBuilder(string[,] arr)
        {
            var items = "";
            var length = arr.GetLength(0);
            for (var row = 0; row < length; row++)
            {
                items += string.Format("items[{0}][description]={1}&items[{0}][unitprice]={2}&items[{0}][unitprice_incl]={2}&items[{0}][quantity]={3}&", row, arr[row, 0], arr[row, 1], arr[row, 2]);
            }
            return items;

        }

        public decimal CalculateVat(decimal value)
        {
            return (value / 100) * 17;
        }

        public ICountSalesResponse GeneratePaymentPage(long laundryRequestId, User user)
        {
            var jss = new JavaScriptSerializer();
            var laundryDetails = LaundryRequestDetails(laundryRequestId);
            var price = laundryDetails.Price;
            var description = laundryDetails.PropertyName + ", Laundry" + ", " + String.Format("{0:g}", laundryDetails.ReturnDate);
            string page_id = "";
            try
            {
                string cid = Convert.ToString(ConfigurationManager.AppSettings["icount_cid"]);
                string icount_user = Convert.ToString(ConfigurationManager.AppSettings["icount_user"]);
                string pass = Convert.ToString(ConfigurationManager.AppSettings["icount_pass"]);

                string[,] arr = new string[2, 3]{
                    {description,  Convert.ToString(price), "1"},
                    { "Delivery Charge", deliveryCharge.ToString(), "1" }
                };
                var items = ItemsBuilder(arr);

                ICountSalesResponse CreatePageResponse = CreatePage(cid, icount_user, pass, description, items);
                if (CreatePageResponse != null && CreatePageResponse.status)
                {
                    page_id = CreatePageResponse.paypage_id;
                }

                decimal dUnitprice = Convert.ToDecimal(price);
                int unitprice = Convert.ToInt32(dUnitprice);
                decimal priceIncludingVat = dUnitprice + CalculateVat(dUnitprice);
                string client_name = user.CompanyName ?? user.FullName.Trim();
                string client_address = user.BillingAddress ?? user.Address;


                string email = user.Email;
                string email_to = user.Email;
                var laundryIdToken = tokenService.GenerateToken(laundryDetails.LaundryRequestId);

                string path = Common.BaseUrl;

                string success_url = string.Format(@"{0}/Order/PaymentSuccess?description={1}", path, description);
                string failiure_url = string.Format("{0}/Order/PaymentCancel?description={1}", path, description);
                string ipn_url = string.Format("{0}/api/Order/LaundryPaymentProcess?laundryIdToken={1}", path, laundryIdToken);

                var url = string.Format(Common.GeneratSale + items, cid, icount_user, pass, client_name, page_id, price, email, user.PhoneNumber, user.IdNumber, success_url, failiure_url, ipn_url);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.ContentType = "application/json";
                var response = (HttpWebResponse)myReq.GetResponse();
                string text;

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
                ICountSalesResponse countResponse = jss.Deserialize<ICountSalesResponse>(text);

                return countResponse;
            }
            catch (Exception exp)
            {
                return jss.Deserialize<ICountSalesResponse>(exp.Message);
            }
        }

        private ICountSalesResponse CreatePage(string cid, string user, string pass, string description, string items, string CurrencyId = "5")
        {
            var jss = new JavaScriptSerializer();
            try
            {
                var url = string.Format(Common.CreatePage + items, cid, user, pass, description, CurrencyId);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.ContentType = "application/json";
                var response = (HttpWebResponse)myReq.GetResponse();
                string text;

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
                ICountSalesResponse countResponse = jss.Deserialize<ICountSalesResponse>(text);
                return countResponse;
            }
            catch (Exception exp)
            {
                return jss.Deserialize<ICountSalesResponse>(exp.Message);
            }
        }

        public long GetLaundry(long propertyId)
        {
            // get the closet laundry
            var property = _db.Properties.FirstOrDefault(x => x.Id == propertyId);
            var laundryType = Enums.UserTypeEnum.Laundry.GetHashCode();
            var laundryList = _db.Users.Where(x => x.UserType == laundryType).ToList();
            long selectedLaundry = 0;
            decimal minTime = 100000;

            foreach (var laundry in laundryList)
            {
                // time in seconds
                var distanceTimeVM = Common.CalculateDistanceTime(property.Address, laundry.Address);
                var timeInMinutes = distanceTimeVM.Time;
                if (timeInMinutes < minTime)
                {
                    minTime = timeInMinutes;
                    selectedLaundry = laundry.UserId;
                }
            }
            return selectedLaundry;
        }

        public List<AvailableTimeViewModel> GetAvailabilities()
        {
            var delivery_type = Enums.UserTypeEnum.DeliveryGuy.GetHashCode();
            List<GetWorkers> getWorkersAvailability = (from avialableSchedule in _db.tblProviderAvailableTimes
                                                       join workers in _db.Users
                                                       on avialableSchedule.provider_id equals workers.UserId
                                                       join subCat in _db.UserSubCategories
                                                       on workers.UserId equals subCat.UserId
                                                       where avialableSchedule.vDate > DateTime.Now
                                                       && avialableSchedule.isCausallOff == false
                                                       && avialableSchedule.isOptionalOff == false
                                                       && (workers.UserType == delivery_type)
                                                       && (workers.standby == null || workers.standby == false)
                                                       select new GetWorkers
                                                       {
                                                           Day = avialableSchedule.day_of_week,
                                                           ToTime = avialableSchedule.to_time,
                                                           FromTime = avialableSchedule.from_time,
                                                           UserId = workers.UserId,
                                                           Date = (DateTime)avialableSchedule.vDate
                                                       }).ToList();
            var availabilities = getWorkersAvailability.Select(x => new AvailableTimeViewModel()
            {
                FromTime = x.FromTime,
                ToTime = x.ToTime,
                AvailableDate = x.Date.ToString("yyyy-MM-dd"),
                UserId = x.UserId
            }).ToList();

            // Distinct
            availabilities = availabilities.GroupBy(x => new { x.AvailableDate, x.UserId })
                .Select(x => x.First())
                .ToList();


            return availabilities;
        }

        public List<DeliveryBookedViewModel> GetBookedSchedules(long propertyId)
        {
            try
            {
                var property = _db.Properties.FirstOrDefault(x => x.Id == propertyId);

                var bookings = (from laundryRequest in _db.LaundryRequests
                                join p in _db.Properties on laundryRequest.PropertyId equals p.Id
                                where laundryRequest.PickupDate > DateTime.Now ||
                                laundryRequest.ReturnDate > DateTime.Now
                                select new DeliveryBookedViewModel
                                {
                                    PickupGuyId = laundryRequest.PickupGuyId,
                                    ReturnGuyId = (long)laundryRequest.ReturnGuyId,
                                    PickupDate = laundryRequest.PickupDate,
                                    ReturnDate = laundryRequest.ReturnDate,
                                    Address = p.Address
                                }).ToList();
                foreach (var item in bookings)
                {
                    var distanceTimeVM = Common.CalculateDistanceTime(property.Address, item.Address);
                    item.Time = distanceTimeVM.Time; // in Minutes
                    item.Distance = distanceTimeVM.Distance; // in km
                }

                return bookings;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<DeliveryBookedViewModel>();
            }

        }

        public List<DeliveryDistanceViewModel> GetDeliveryDistances(long propertyId)
        {
            var deliveryDistances = new List<DeliveryDistanceViewModel>();
            var property = _db.Properties.FirstOrDefault(x => x.Id == propertyId);
            var deliveryType = Enums.UserTypeEnum.DeliveryGuy.GetHashCode();
            // get each delivery guy
            var deliveryGuys = _db.Users.Where(u => u.UserType == deliveryType).ToList();

            foreach (var deliveryGuy in deliveryGuys)
            {
                var distanceTimeVM = Common.CalculateDistanceTime(deliveryGuy.Address, property.Address);
                deliveryDistances.Add(new DeliveryDistanceViewModel
                {
                    DeliveryGuyId = deliveryGuy.UserId,
                    DeliveryGuyName = deliveryGuy.FullName,
                    Time = distanceTimeVM.Time,
                    Distance = distanceTimeVM.Distance
                });
            }
            return deliveryDistances;
        }

        public bool PickupLaundry(long laundryRequestId, long pickupGuyId)
        {
            try
            {
                var laundryRequest = _db.LaundryRequests.FirstOrDefault(x => x.LaundryRequestId == laundryRequestId);

                if (laundryRequest == null) return false;

                if (laundryRequest.PickupGuyId != pickupGuyId) return false;

                laundryRequest.LaundryStatus = Enums.LaundryStatus.Pickedup.GetHashCode();

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool ReceivedLaundry(long laundryRequestId, long laundryId)
        {
            try
            {
                var laundryRequest = _db.LaundryRequests.FirstOrDefault(x => x.LaundryRequestId == laundryRequestId);

                if (laundryRequest == null) return false;

                if (laundryRequest.LaundryId != laundryId) return false;

                laundryRequest.LaundryStatus = Enums.LaundryStatus.LaundryReceived.GetHashCode();

                // send notification to the client that laundry received.

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool SendLaundryPrice(LaundryPriceViewModel model)
        {
            try
            {
                var laundryRequest = _db.LaundryRequests.FirstOrDefault(x => x.LaundryRequestId == model.LaundryRequestId);

                if (laundryRequest == null) return false;

                if (laundryRequest.LaundryId != model.LaundryId) return false;

                var laundry = _db.Users.FirstOrDefault(x => x.UserId == model.LaundryId);

                if (laundry == null) return false;

                laundryRequest.Price = model.LaundryPrice;

                laundryRequest.LaundryStatus = Enums.LaundryStatus.Priced.GetHashCode();

                // send notification to the user
                Notification notification = new Notification
                {
                    CreatedDate = DateTime.Now,
                    FromUserId = model.LaundryId,
                    IsActive = true,
                    JobRequestId = 0,
                    NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode(),
                    ToUserId = laundryRequest.UserId,
                    Text = string.Format("{0} sent laundry price {1}ils", laundry.FullName, model.LaundryPrice)
                };
                _db.Notifications.Add(notification);

                _db.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool ProcessPayment(string laundryIdToken)
        {
            try
            {
                var laundryRequestId = tokenService.ValidateToken(laundryIdToken);

                if (laundryRequestId == null) return false;

                var laundryRequest = _db.LaundryRequests.FirstOrDefault(x => x.LaundryRequestId == laundryRequestId);

                if (laundryRequest == null) return false;

                laundryRequest.LaundryStatus = Enums.LaundryStatus.Paid.GetHashCode();

                // send notification to the laundry that payment is done for orderId.

                _db.SaveChanges();

                // send email to bell-boy official mail.

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool DeliveryReceivedLaundry(long laundryRequestId, long returnGuyId)
        {
            try
            {
                var laundryRequest = _db.LaundryRequests.FirstOrDefault(x => x.LaundryRequestId == laundryRequestId);

                if (laundryRequest == null) return false;

                if (laundryRequest.ReturnGuyId != returnGuyId) return false;

                laundryRequest.LaundryStatus = Enums.LaundryStatus.DeliveryReceived.GetHashCode();

                // send notification to the client that delivery received items.

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool DeliveredLaundry(long laundryRequestId, long returnGuyId)
        {
            try
            {
                var laundryRequest = _db.LaundryRequests.FirstOrDefault(x => x.LaundryRequestId == laundryRequestId);

                if (laundryRequest == null) return false;

                if (laundryRequest.ReturnGuyId != returnGuyId) return false;

                laundryRequest.LaundryStatus = Enums.LaundryStatus.Delivered.GetHashCode();

                // send notification to the client that delivered.

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}