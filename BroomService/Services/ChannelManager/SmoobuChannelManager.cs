using BroomService.Helpers;
using BroomService.Models;
using BroomService.ViewModels;
using BroomService.ViewModels.ChannelManager.Smoobu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace BroomService.Services.ChannelManager
{
    public class SmoobuChannelManager : IChannelManager
    {
        private readonly string SMOOBU_URL = "https://login.smoobu.com";

        BroomServiceEntities1 _db;

        public SmoobuChannelManager()
        {
            _db = new BroomServiceEntities1();
        }

        public async Task<List<ChannelManagerAccomodationViewModel>> Accomodations(string apiKey, bool onlyAvailable = false)
        {
            // get UserChannelManager where userId and ChannelManagerId 1
            // if there is document and UserChannelManager is active
            // check if there is ApiKey

            // if all condition fullfill then send request to smoobu server
            var accomodations = new List<ChannelManagerAccomodationViewModel>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-key", apiKey);
                HttpResponseMessage Res = await client.GetAsync(SMOOBU_URL + "/api/apartments").ConfigureAwait(false);
                if (Res.IsSuccessStatusCode)
                {
                    var record = Res.Content.ReadAsStringAsync().Result;
                    var apartmentVM = JsonConvert.DeserializeObject<ApartmentViewModel>(record);
                    if (onlyAvailable)
                    {
                        foreach (var apartment in apartmentVM.Apartments)
                        {
                            var foundProperty = _db.Properties.FirstOrDefault(a => a.AccomodationId == apartment.Id);
                            if (foundProperty == null)
                            {
                                accomodations.Add(apartment);
                            }
                        }
                    }
                    else
                    {
                        accomodations = apartmentVM.Apartments;
                    }
                }
            }

            return accomodations;
        }

        public async Task<bool> Activate(long userId, UserChannelManagerViewModel userChannelManager)
        {
            // First check if Smoobu Channel Manager exist by the userId
            // if not exist create one
            // if exist then fetch that one and set the value of userChannelManager
            // finally save

            // validate APiKey

            try
            {
                var accomodations = await Accomodations(userChannelManager.ApiKey);
                if (accomodations == null) return false;
                var foundUserChannelManager = _db.UserChannelManagers
                    .FirstOrDefault(ucm => ucm.UserId == userId && ucm.ChannelManagerId == userChannelManager.ChannelManagerId);

                if (foundUserChannelManager == null)
                {
                    // validate the apiKey

                    // create new channel manager
                    UserChannelManager userChannel = new UserChannelManager
                    {
                        UserId = userId,
                        ChannelManagerId = userChannelManager.ChannelManagerId,
                        ChannelManagerUsername = userChannelManager.ChannelManagerUsername,
                        ChannelManagerPassword = userChannelManager.ChannelManagerPassword,
                        ApiKey = userChannelManager.ApiKey,
                        Active = true
                    };

                    _db.UserChannelManagers.Add(userChannel);
                    _db.SaveChanges();

                    // create settings for the channel manager
                    UserChannelManagerSetting userChannelManagerSettings = new UserChannelManagerSetting
                    {
                        UserChannelManagerId = userChannel.UserChannelManagerId,
                        WindowsCleaning = false,
                        CheckInCleaning = false,
                        LundryPickup = false,
                        LinenRentals = false,
                        Amenties = false
                    };

                    _db.UserChannelManagerSettings.Add(userChannelManagerSettings);
                    _db.SaveChanges();

                }
                else
                {
                    foundUserChannelManager.Active = true;
                    foundUserChannelManager.ApiKey = userChannelManager.ApiKey;
                    foundUserChannelManager.ChannelManagerPassword = userChannelManager.ChannelManagerUsername;
                    foundUserChannelManager.ChannelManagerPassword = userChannelManager.ChannelManagerPassword;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


            return true;
        }

        public bool AutoOrderServices(Property property)
        {
            // check the userId of the property
            // get active user channel manager by the userId
            // get all the channel manager settings by the userChannelManagerId
            // finally auto order services by every settings which is selected.
            var userActiveChannelManager = _db.UserChannelManagers.FirstOrDefault(x => x.UserId == property.CreatedBy);
            if (userActiveChannelManager == null) return false;

            var channelManagerSettings = _db.UserChannelManagerSettings.FirstOrDefault(x => x.UserChannelManagerId == userActiveChannelManager.UserChannelManagerId);
            if (channelManagerSettings == null) return false;

            if (channelManagerSettings.CheckInCleaning)
            {
                // order check in cleaning service
            }
            if (channelManagerSettings.WindowsCleaning)
            {
                // order windows cleaning service
            }
            if (channelManagerSettings.LundryPickup)
            {
                // order laundry pickup service
            }
            if (channelManagerSettings.LinenRentals)
            {
                // order linen rentals
            }
            if (channelManagerSettings.Amenties)
            {
                // order amenties
            }

            return true;
        }

        public string GetSizeByBedrooms(int noOfBedrooms)
        {
            string size = "1-25";
            if (noOfBedrooms == 1) size = "1-45";
            if (noOfBedrooms == 2) size = "2-65";
            if (noOfBedrooms == 3) size = "3-85";
            if (noOfBedrooms == 4) size = "4-110";
            if (noOfBedrooms == 5) size = "5-140";
            return size;
        }

        public async Task<bool> ImportPropertyByApartmentId(long userId, long apartmentId)
        {
            var userChannelManager = _db.UserChannelManagers.FirstOrDefault(uc => uc.UserId == userId && uc.ChannelManagerId == ((long)Enums.ChannelManager.Smoobu));
            var smoobuProperty = await AccomodationById(userChannelManager, apartmentId);

            if (smoobuProperty != null)
            {
                string address = smoobuProperty.Location?.Street + ", " + smoobuProperty.Location?.City + ", " + smoobuProperty.Location?.Country;
                string searchAddress = address.ToLower();
                // create property with smoobuProperty
                if (searchAddress.Contains("tel aviv") ||
                    searchAddress.Contains("tel aviv-yafo") ||
                    searchAddress.Contains("תל אביב, ישראל") ||
                    searchAddress.Contains("תל אביב"))
                {
                    var property = new Property
                    {
                        CreatedBy = userId,
                        NoOfBedRooms = smoobuProperty.Rooms.Bedrooms,
                        NoOfDoubleBeds = smoobuProperty.Rooms.DoubleBeds,
                        NoOfQueenBeds = smoobuProperty.Rooms.QueenSizeBeds,
                        NoOfkingBeds = smoobuProperty.Rooms.KingSizeBeds,
                        NoOfSingleSofaBeds = smoobuProperty.Rooms.SofaBeds,
                        NoOfSingleBeds = smoobuProperty.Rooms.SingleBeds,
                        Size = GetSizeByBedrooms(smoobuProperty.Rooms.Bedrooms),
                        Address = address,
                        Latitude = smoobuProperty.Location != null ? smoobuProperty.Location.Latitude : 0,
                        Longitude = smoobuProperty.Location != null ? smoobuProperty.Location.Longitude : 0,
                        Doorman = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "doorman"),
                        Pool = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "pool"),
                        Garden = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "garden"),
                        Parking = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "parking"),
                        Balcony = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "balcony"),
                        Dishwasher = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "diswasher"),
                        Elevator = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "elevator"),
                        CoffeeMachine = Array.Exists(smoobuProperty.Amenities, element => element.ToLower() == "coffeemachine"),
                        AccomodationId = apartmentId
                    };
                    _db.Properties.Add(property);
                    _db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public async Task<SmoobuPropertyViewModel> AccomodationById(UserChannelManager userChannelManager, long apartmentId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-key", userChannelManager.ApiKey);
                HttpResponseMessage Res = await client.GetAsync(SMOOBU_URL + "/api/apartments/" + apartmentId).ConfigureAwait(false);
                if (Res.IsSuccessStatusCode)
                {
                    var record = Res.Content.ReadAsStringAsync().Result;
                    var smoobuProperty = JsonConvert.DeserializeObject<SmoobuPropertyViewModel>(record);
                    return smoobuProperty;
                }
            }
            return null;
        }

        public bool NewReservation(SmoobuReservationDataViewModel smoobuReservation)
        {
            var reservation = new Reservation
            {
                ReservationId = smoobuReservation.Id,
                ReservationType = smoobuReservation.Type,
                ArrivalDate = smoobuReservation.Arrival,
                DepartureDate = smoobuReservation.Departure,
                ApartmentId = smoobuReservation.Apartment.Id,
                ApartmentName = smoobuReservation.Apartment.Name,
                ChannelId = smoobuReservation.Channel.Id,
                ChannelName = smoobuReservation.Channel.Name,
                GuestName = smoobuReservation.GuestName,
                Email = smoobuReservation.Email,
                Phone = smoobuReservation.Phone,
                Adults = smoobuReservation.Adults,
                Children = smoobuReservation.Children,
                Notice = smoobuReservation.Notice,
                Price = smoobuReservation.Price,
                PricePaid = smoobuReservation.PricePaid,
                Deposit = smoobuReservation.Deposit,
                DepositPaid = smoobuReservation.DepositPaid
            };

            var property = _db.Properties.FirstOrDefault(x => x.AccomodationId == smoobuReservation.Apartment.Id);
            property.Status = "booked";  //active, inactive, booked.

            _db.Reservations.Add(reservation);
            _db.SaveChangesAsync();
            return true;
        }

        public bool UpdateReservation(SmoobuReservationDataViewModel smoobuReservation)
        {
            try
            {
                var reservation = _db.Reservations.FirstOrDefault(r => r.ReservationId == smoobuReservation.Id);
                var property = _db.Properties.FirstOrDefault(p => p.AccomodationId == smoobuReservation.Apartment.Id);
                if (reservation == null) return false;
                if (property == null) return false;
                // ******** CHECK IN *********
                // get property by the apartmentId
                // get reservation by reservationId
                // check if checkIn
                // get the property SubUser
                // change subUser password
                // generate random password
                // update property status to active
                // send email and password to the subUser and the mainUser
                // Auto order services

                // ******** CHECK OUT *********
                // get property by the apartmentId
                // get reservation by reservationId
                // check if checkout
                // get the property SubUser
                // change subUser password
                // send email to the mainUser
                // update property status to inactive
                if (reservation.CheckIn == null && smoobuReservation.CheckIn != null)
                {
                    var getUserDetails = _db.Users.Where(a => a.UserId == property.SubUserId).FirstOrDefault();
                    if (getUserDetails != null) // subUser Already added, just need to update password
                    {
                        string password = Common.FetchRandString(6);

                        // Generating encrypted password
                        string salt = string.Empty;
                        string encryptedPswd = string.Empty;
                        Common.GeneratePassword(password, "new", ref salt, ref encryptedPswd);
                        getUserDetails.Password = encryptedPswd;
                        getUserDetails.PasswordSalt = salt;

                        // set property status to active
                        property.Status = "active";
                        property.IsActive = true;

                        _db.SaveChanges();

                        // send new password to the mainUser through sms and email
                        // send subUser password and email to the reservation user
                    }
                    AutoOrderServices(property);

                }
                else if (reservation.Checkout == null && smoobuReservation.Checkout != null)
                {
                    var getUserDetails = _db.Users.Where(a => a.UserId == property.SubUserId).FirstOrDefault();
                    if (getUserDetails != null) // subUser Already added, just need to update password
                    {
                        string password = Common.FetchRandString(6);

                        // Generating encrypted password
                        string salt = string.Empty;
                        string encryptedPswd = string.Empty;
                        Common.GeneratePassword(password, "new", ref salt, ref encryptedPswd);
                        getUserDetails.Password = encryptedPswd;
                        getUserDetails.PasswordSalt = salt;

                        // set property status to active
                        property.Status = "inactive";

                        _db.SaveChanges();

                        // send new password to the mainUser through sms and email
                    }
                }
                else
                {
                    Console.WriteLine("No Operation");
                }

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