using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BroomService.Helpers;
using BroomService.Models;
using BroomService.Resources;
using BroomService.ViewModels;
using static BroomService.Helpers.Enums;

namespace BroomService.Services
{
    public class PropertyService
    {
        BroomServiceEntities1 _db;
        public string message;

        public PropertyService()
        {
            _db = new BroomServiceEntities1();
        }

        public Property GetPropertiesById(long Id)
        {
            Property Data = null;
            try
            {
                if (Id != 0)
                {
                    Data = _db.Properties.Where(x => x.Id == Id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Data = null;
            }
            return Data;
        }

        public PropertyViewModel GetPropertiesDetails(long Id)
        {
            PropertyViewModel Data = null;
            try
            {
                var getData = _db.Properties.Where(x => x.Id == Id).FirstOrDefault();

                if (getData != null)
                {
                    var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == Id)
                        .Select(x => new PropertyImageVM()
                        {
                            PropertyId = x.PropertyId,
                            CreatedDate = x.CreatedDate,
                            Id = x.Id,
                            ImageUrl = x.ImageUrl,
                            IsImage = x.IsImage,
                            IsVideo = x.IsVideo,
                            VideoThumbnail = x.VideoThumbnail,
                            VideoUrl = x.VideoUrl
                        }).ToList();

                    Data = new PropertyViewModel
                    {
                        PropertyModel = new PropertyVM
                        {
                            AccesstoCode = getData.AccesstoCode,
                            AccessToProperty = getData.AccessToProperty,
                            Address = getData.Address,
                            ApartmentNumber = getData.ApartmentNumber,
                            Balcony = getData.Balcony,
                            BuildingCode = getData.BuildingCode,
                            CityId = getData.CityId,
                            CoffeeMachine = getData.CoffeeMachine,
                            CountryId = getData.CountryId,
                            CreatedBy = getData.CreatedBy,
                            CreatedDate = getData.CreatedDate,
                            Dishwasher = getData.Dishwasher,
                            Doorman = getData.Doorman,
                            Elevator = getData.Elevator,
                            FloorNumber = getData.FloorNumber,
                            Garden = getData.Garden,
                            Id = getData.Id,
                            IsKingBed = getData.IsKingBed,
                            IsSingleBed = getData.IsSingleBed,
                            IsSofaBed = getData.IsSofaBed,
                            Latitude = getData.Latitude,
                            Longitude = getData.Longitude,
                            Name = getData.Name,
                            NoOfBathrooms = getData.NoOfBathrooms,
                            NoOfBedRooms = getData.NoOfBedRooms,
                            NoOfDoubleBeds = getData.NoOfDoubleBeds,
                            NoOfDoubleSofaBeds = getData.NoOfDoubleSofaBeds,
                            NoOfQueenBeds = getData.NoOfQueenBeds,
                            NoOfKingBeds = getData.NoOfkingBeds,
                            NoOfSingleBeds = getData.NoOfSingleBeds,
                            NoOfSingleSofaBeds = getData.NoOfSingleSofaBeds,
                            NoOfToilets = getData.NoOfToilets,
                            Parking = getData.Parking,
                            Pool = getData.Pool,
                            ShortTermApartment = getData.ShortTermApartment,
                            Size = getData.Size,
                            Type = getData.Type,
                            WifiLogin = getData.WifiLogin
                        },
                        PropertyImages = getPropertyImages
                    };
                }
            }
            catch (Exception ex)
            {
                Data = null;
            }
            return Data;
        }

        public User GenerateSubuser(Property model, string password)
        {
            // Generating encrypted password
            string salt = string.Empty;
            string encryptedPswd = string.Empty;
            Common.GeneratePassword(password, "new", ref salt, ref encryptedPswd);
            var username = "";
            // generate username until get the unique one
            var found = true;
            while (found)
            {
                username = Common.GenerateUsername(7);
                var foundUser = _db.Users.FirstOrDefault(x => x.Email == username);
                if (foundUser == null) found = false;
            }
            User user = new User
            {
                Password = encryptedPswd,
                PasswordSalt = salt,
                Email = username,
                FullName = model.Name,
                IsActive = true,
                CreatedDate = DateTime.Now,
                EmailVerified = true,
                UserType = UserTypeEnum.SubUser.GetHashCode(),
                PaymentMethod = Enums.PaymentMethod.ByCreditCard.GetHashCode(),
                JobPayType = Enums.JobRequestPayType.BeforeJob.GetHashCode(),
                IsSubUser = true
            };
            _db.Users.Add(user);
            _db.SaveChanges();

            return user;
        }

        public PropertySuccessVM AddUpdateProperty(Property model, List<tblPropertyImage> _propertyImages)
        {
            var propertySuccessVM = new PropertySuccessVM();
            long propertyId = 0;
            try
            {
                if (model.Id != 0)
                {
                    var result = _db.Properties.Where(x => x.Id == model.Id).FirstOrDefault();
                    // Case 1: Airbnb apartment field changed
                    if (model.ShortTermApartment == true)
                    {
                        // Case 2: Name of the property changed
                        //var checkNameChanged = model.Name.Equals(result.Name);
                        //if (!checkNameChanged) //Both names are not equal
                        //{
                        var getUserDetails = _db.Users.Where(a => a.UserId == result.SubUserId).FirstOrDefault();
                        if (getUserDetails != null) // subUser Already added, just need to update password
                        {
                            string password = string.Join("", model.Name.Split(' ')) + "bnb";

                            // Generating encrypted password
                            string salt = string.Empty;
                            string encryptedPswd = string.Empty;
                            Common.GeneratePassword(password, "new", ref salt, ref encryptedPswd);
                            getUserDetails.Password = encryptedPswd;
                            getUserDetails.PasswordSalt = salt;

                            propertySuccessVM.SubUserEmail = getUserDetails.Email;
                            propertySuccessVM.SubUserPassword = password;
                            _db.SaveChanges();
                        }
                        else
                        {
                            //adding data for sub-user for airbnb aparments
                            string password = string.Join("", model.Name.Split(' ')) + "bnb";
                            var subUser = GenerateSubuser(model, password);

                            model.SubUserId = subUser.UserId;
                            propertySuccessVM.SubUserEmail = subUser.Email;
                            propertySuccessVM.SubUserPassword = password;
                        }
                        //}
                    }
                    else
                    {
                        var getUserDetails = _db.Users.Where(a => a.UserId == result.SubUserId).FirstOrDefault();
                        if (getUserDetails != null) //Data already stored, need to remove that from table
                        {
                            getUserDetails.IsActive = false;
                            _db.SaveChanges();
                        }
                    }
                    if (result != null)
                    {
                        result.AccesstoCode = model.AccesstoCode;
                        result.AccessToProperty = model.AccessToProperty;
                        result.Address = model.Address;
                        result.ApartmentNumber = model.ApartmentNumber;
                        result.Balcony = model.Balcony;
                        result.BuildingCode = model.BuildingCode;
                        result.CityId = model.CityId;
                        result.CoffeeMachine = model.CoffeeMachine;
                        result.CountryId = model.CountryId;
                        result.CreatedBy = model.CreatedBy;
                        result.Dishwasher = model.Dishwasher;
                        result.Doorman = model.Doorman;
                        result.Elevator = model.Elevator;
                        result.FloorNumber = model.FloorNumber;
                        result.Garden = model.Garden;
                        result.IsKingBed = model.IsKingBed;
                        result.IsSingleBed = model.IsSingleBed;
                        result.IsSofaBed = model.IsSofaBed;
                        result.Longitude = model.Longitude;
                        result.Latitude = model.Latitude;
                        result.ModifiedDate = DateTime.Now;
                        result.Name = model.Name;
                        result.NoOfBathrooms = model.NoOfBathrooms;
                        result.NoOfBedRooms = model.NoOfBedRooms;
                        result.NoOfDoubleBeds = model.NoOfDoubleBeds;
                        result.NoOfDoubleSofaBeds = model.NoOfDoubleSofaBeds;
                        result.NoOfQueenBeds = model.NoOfQueenBeds;
                        result.NoOfkingBeds = model.NoOfkingBeds;
                        result.NoOfSingleBeds = model.NoOfSingleBeds;
                        result.NoOfSingleSofaBeds = model.NoOfSingleSofaBeds;
                        result.NoOfToilets = model.NoOfToilets;
                        result.Parking = model.Parking;
                        result.Pool = model.Pool;
                        result.ShortTermApartment = model.ShortTermApartment;
                        result.Size = model.Size;
                        result.Type = model.Type;
                        result.WifiLogin = model.WifiLogin;

                        //result.RoomPrice = model.RoomPrice;
                        double roomPrice = Convert.ToDouble(model.RoomPrice);
                        result.RoomPrice = Convert.ToDecimal(Math.Round(roomPrice) + ".00");

                        //result.Price2Star = model.Price2Star;
                        double price2Star = Convert.ToDouble(model.Price2Star);
                        result.Price2Star = Convert.ToDecimal(Math.Round(price2Star) + ".00");

                        //result.Price3Star = model.Price3Star;
                        double price3Star = Convert.ToDouble(model.Price3Star);
                        result.Price3Star = Convert.ToDecimal(Math.Round(price3Star) + ".00");

                        //result.Price4Star = model.Price4Star;
                        double price4Star = Convert.ToDouble(model.Price4Star);
                        result.Price4Star = Convert.ToDecimal(Math.Round(price4Star) + ".00");

                        //result.Price5Star = model.Price5Star;
                        double price5Star = Convert.ToDouble(model.Price5Star);
                        result.Price5Star = Convert.ToDecimal(Math.Round(price5Star) + ".00");

                        //result.DeluxPrice = model.DeluxPrice;
                        double deluxPrice = Convert.ToDouble(model.DeluxPrice);
                        result.DeluxPrice = Convert.ToDecimal(Math.Round(deluxPrice) + ".00");

                        _db.SaveChanges();

                        if (_propertyImages != null)
                        {
                            if (_propertyImages.Count > 0)
                            {
                                for (int i = 0; i < _propertyImages.Count; i++)
                                {
                                    var propertyImg = _propertyImages[i];
                                    propertyImg.PropertyId = result.Id;
                                    _db.tblPropertyImages.Add(propertyImg);
                                    _db.SaveChanges();
                                }
                            }
                        }
                        message = Resource.property_update_success;
                    }
                    else
                    {
                        message = Resource.property_not_exists;
                    }
                    propertyId = model.Id;
                }
                else
                {
                    //adding data for sub-user for airbnb aparments
                    if (model.ShortTermApartment == true)
                    {
                        //adding data for sub-user for airbnb aparments
                        string password = string.Join("", model.Name.Split(' ')) + "bnb";
                        var subUser = GenerateSubuser(model, password);
                        model.SubUserId = subUser.UserId;
                        propertySuccessVM.SubUserEmail = subUser.Email;
                        propertySuccessVM.SubUserPassword = password;
                    }
                    var result = _db.Properties.Add(model);
                    _db.SaveChanges();

                    if (_propertyImages != null)
                    {
                        if (_propertyImages.Count > 0)
                        {
                            for (int i = 0; i < _propertyImages.Count; i++)
                            {
                                var propertyImg = _propertyImages[i];
                                propertyImg.PropertyId = result.Id;
                                _db.tblPropertyImages.Add(propertyImg);
                                _db.SaveChanges();
                            }
                        }
                    }

                    propertyId = result.Id;
                    message = Resource.property_add_success;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                propertyId = 0;
            }
            propertySuccessVM.PropertyId = propertyId;
            return propertySuccessVM;
        }

        public List<SelectListItem> GetPropertiesSelectForInventory(List<long> PropertyIdList)
        {
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
            List<Property> _listProperty = new List<Property>();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (var item in PropertyIdList)
            {
                var data = _db.Properties.Where(x => x.Id == item).FirstOrDefault();
                if (data.ShortTermApartment == true)
                {
                    _listProperty.Add(data);
                }
            }

            foreach (var item in _listProperty)
            {
                listItems.Add(new SelectListItem
                {
                    Text = item.Name + ", " + item.Address,
                    Value = item.Id.ToString(),
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetPropertiesSelectForMultiple(List<long> PropertyIdList)
        {
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;
            List<Property> _listProperty = new List<Property>();
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (var item in PropertyIdList)
            {
                var data = _db.Properties.Where(x => x.Id == item).FirstOrDefault();
                _listProperty.Add(data);
            }

            foreach (var item in _listProperty)
            {
                listItems.Add(new SelectListItem
                {
                    Text = item.Name + ", " + item.Address,
                    Value = item.Id.ToString(),
                });
            }

            return listItems;
        }

        public MainUserProperty GetPropertiesByUserId(long userId, int? UserType)
        {
            MainUserProperty returnModel = new MainUserProperty();
            List<PropertyViewModel> lstData = new List<PropertyViewModel>();
            List<PropertyViewModel> grantData = new List<PropertyViewModel>();
            try
            {
                var user = _db.Users.Where(x => x.UserId == userId).FirstOrDefault();

                if (user != null)
                {
                    if (UserType != 7)
                    {
                        #region Grant Access Properties

                        var listOfGrantAccessUser = _db.tblGrantAccesses.Where(a => a.CreatedBy == userId
                        && a.IsAccountAccepted == true).ToList();

                        if (listOfGrantAccessUser.Count > 0)
                        {
                            foreach (var item in listOfGrantAccessUser)
                            {
                                var getProperties = _db.Properties.Where(x => x.CreatedBy == item.UserId && x.IsActive == true)
                                .OrderByDescending(a => a.CreatedDate).ToList();
                                if (getProperties.Count > 0)
                                {
                                    lstData = new List<PropertyViewModel>();
                                    for (int i = 0; i < getProperties.Count; i++)
                                    {
                                        var getId = getProperties[i].Id;

                                        var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == getId)
                                            .Select(x => new PropertyImageVM()
                                            {
                                                PropertyId = x.PropertyId,
                                                CreatedDate = x.CreatedDate,
                                                Id = x.Id,
                                                ImageUrl = x.ImageUrl,
                                                IsImage = x.IsImage,
                                                IsVideo = x.IsVideo,
                                                VideoThumbnail = x.VideoThumbnail,
                                                VideoUrl = x.VideoUrl
                                            }).ToList();

                                        lstData.Add(new PropertyViewModel
                                        {
                                            PropertyModel = new PropertyVM
                                            {
                                                AccesstoCode = getProperties[i].AccesstoCode,
                                                AccessToProperty = getProperties[i].AccessToProperty,
                                                Address = getProperties[i].Address,
                                                ApartmentNumber = getProperties[i].ApartmentNumber,
                                                Balcony = getProperties[i].Balcony,
                                                BuildingCode = getProperties[i].BuildingCode,
                                                CityId = getProperties[i].CityId,
                                                CoffeeMachine = getProperties[i].CoffeeMachine,
                                                CountryId = getProperties[i].CountryId,
                                                CreatedBy = getProperties[i].CreatedBy,
                                                CreatedDate = getProperties[i].CreatedDate,
                                                Dishwasher = getProperties[i].Dishwasher,
                                                Doorman = getProperties[i].Doorman,
                                                Elevator = getProperties[i].Elevator,
                                                FloorNumber = getProperties[i].FloorNumber,
                                                Garden = getProperties[i].Garden,
                                                Id = getProperties[i].Id,
                                                IsKingBed = getProperties[i].IsKingBed,
                                                IsSingleBed = getProperties[i].IsSingleBed,
                                                IsSofaBed = getProperties[i].IsSofaBed,
                                                Latitude = getProperties[i].Latitude,
                                                Longitude = getProperties[i].Longitude,
                                                Name = getProperties[i].Name,
                                                NoOfBathrooms = getProperties[i].NoOfBathrooms,
                                                NoOfBedRooms = getProperties[i].NoOfBedRooms,
                                                NoOfDoubleBeds = getProperties[i].NoOfDoubleBeds,
                                                NoOfDoubleSofaBeds = getProperties[i].NoOfDoubleSofaBeds,
                                                NoOfQueenBeds = getProperties[i].NoOfQueenBeds,
                                                NoOfSingleBeds = getProperties[i].NoOfSingleBeds,
                                                NoOfSingleSofaBeds = getProperties[i].NoOfSingleSofaBeds,
                                                NoOfToilets = getProperties[i].NoOfToilets,
                                                Parking = getProperties[i].Parking,
                                                Pool = getProperties[i].Pool,
                                                ShortTermApartment = getProperties[i].ShortTermApartment,
                                                Size = getProperties[i].Size,
                                                Type = getProperties[i].Type,
                                                WifiLogin = getProperties[i].WifiLogin
                                            },
                                            PropertyImages = getPropertyImages
                                        });
                                    }

                                    returnModel.GrantUserProperties = lstData;
                                }
                            }
                        }

                        #endregion

                        #region Main Access Properties

                        var getProperties1 = _db.Properties.Where(x => x.CreatedBy == userId && x.IsActive == true)
        .OrderByDescending(a => a.CreatedDate).ToList();

                        if (getProperties1.Count > 0)
                        {
                            lstData = new List<PropertyViewModel>();
                            for (int i = 0; i < getProperties1.Count; i++)
                            {
                                var getId = getProperties1[i].Id;

                                var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == getId)
                                    .Select(x => new PropertyImageVM()
                                    {
                                        PropertyId = x.PropertyId,
                                        CreatedDate = x.CreatedDate,
                                        Id = x.Id,
                                        ImageUrl = x.ImageUrl,
                                        IsImage = x.IsImage,
                                        IsVideo = x.IsVideo,
                                        VideoThumbnail = x.VideoThumbnail,
                                        VideoUrl = x.VideoUrl
                                    }).ToList();

                                lstData.Add(new PropertyViewModel
                                {
                                    PropertyModel = new PropertyVM
                                    {
                                        AccesstoCode = getProperties1[i].AccesstoCode,
                                        AccessToProperty = getProperties1[i].AccessToProperty,
                                        Address = getProperties1[i].Address,
                                        ApartmentNumber = getProperties1[i].ApartmentNumber,
                                        Balcony = getProperties1[i].Balcony,
                                        BuildingCode = getProperties1[i].BuildingCode,
                                        CityId = getProperties1[i].CityId,
                                        CoffeeMachine = getProperties1[i].CoffeeMachine,
                                        CountryId = getProperties1[i].CountryId,
                                        CreatedBy = getProperties1[i].CreatedBy,
                                        CreatedDate = getProperties1[i].CreatedDate,
                                        Dishwasher = getProperties1[i].Dishwasher,
                                        Doorman = getProperties1[i].Doorman,
                                        Elevator = getProperties1[i].Elevator,
                                        FloorNumber = getProperties1[i].FloorNumber,
                                        Garden = getProperties1[i].Garden,
                                        Id = getProperties1[i].Id,
                                        IsKingBed = getProperties1[i].IsKingBed,
                                        IsSingleBed = getProperties1[i].IsSingleBed,
                                        IsSofaBed = getProperties1[i].IsSofaBed,
                                        Latitude = getProperties1[i].Latitude,
                                        Longitude = getProperties1[i].Longitude,
                                        Name = getProperties1[i].Name,
                                        NoOfBathrooms = getProperties1[i].NoOfBathrooms,
                                        NoOfBedRooms = getProperties1[i].NoOfBedRooms,
                                        NoOfDoubleBeds = getProperties1[i].NoOfDoubleBeds,
                                        NoOfDoubleSofaBeds = getProperties1[i].NoOfDoubleSofaBeds,
                                        NoOfQueenBeds = getProperties1[i].NoOfQueenBeds,
                                        NoOfSingleBeds = getProperties1[i].NoOfSingleBeds,
                                        NoOfSingleSofaBeds = getProperties1[i].NoOfSingleSofaBeds,
                                        NoOfToilets = getProperties1[i].NoOfToilets,
                                        Parking = getProperties1[i].Parking,
                                        Pool = getProperties1[i].Pool,
                                        ShortTermApartment = getProperties1[i].ShortTermApartment,
                                        Size = getProperties1[i].Size,
                                        Type = getProperties1[i].Type,
                                        WifiLogin = getProperties1[i].WifiLogin
                                    },
                                    PropertyImages = getPropertyImages
                                });
                            }

                            returnModel.MainUserProperties = lstData;
                        }
                        #endregion
                    }
                    else
                    {
                        #region SubUser Access Properties

                        var getProperties1 = _db.Properties.Where(x => x.SubUserId == userId && x.IsActive == true).OrderByDescending(a => a.CreatedDate).ToList();

                        if (getProperties1.Count > 0)
                        {
                            lstData = new List<PropertyViewModel>();
                            for (int i = 0; i < getProperties1.Count; i++)
                            {
                                var getId = getProperties1[i].Id;

                                var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == getId)
                                    .Select(x => new PropertyImageVM()
                                    {
                                        PropertyId = x.PropertyId,
                                        CreatedDate = x.CreatedDate,
                                        Id = x.Id,
                                        ImageUrl = x.ImageUrl,
                                        IsImage = x.IsImage,
                                        IsVideo = x.IsVideo,
                                        VideoThumbnail = x.VideoThumbnail,
                                        VideoUrl = x.VideoUrl
                                    }).ToList();

                                lstData.Add(new PropertyViewModel
                                {
                                    PropertyModel = new PropertyVM
                                    {
                                        AccesstoCode = getProperties1[i].AccesstoCode,
                                        AccessToProperty = getProperties1[i].AccessToProperty,
                                        Address = getProperties1[i].Address,
                                        ApartmentNumber = getProperties1[i].ApartmentNumber,
                                        Balcony = getProperties1[i].Balcony,
                                        BuildingCode = getProperties1[i].BuildingCode,
                                        CityId = getProperties1[i].CityId,
                                        CoffeeMachine = getProperties1[i].CoffeeMachine,
                                        CountryId = getProperties1[i].CountryId,
                                        CreatedBy = getProperties1[i].CreatedBy,
                                        CreatedDate = getProperties1[i].CreatedDate,
                                        Dishwasher = getProperties1[i].Dishwasher,
                                        Doorman = getProperties1[i].Doorman,
                                        Elevator = getProperties1[i].Elevator,
                                        FloorNumber = getProperties1[i].FloorNumber,
                                        Garden = getProperties1[i].Garden,
                                        Id = getProperties1[i].Id,
                                        IsKingBed = getProperties1[i].IsKingBed,
                                        IsSingleBed = getProperties1[i].IsSingleBed,
                                        IsSofaBed = getProperties1[i].IsSofaBed,
                                        Latitude = getProperties1[i].Latitude,
                                        Longitude = getProperties1[i].Longitude,
                                        Name = getProperties1[i].Name,
                                        NoOfBathrooms = getProperties1[i].NoOfBathrooms,
                                        NoOfBedRooms = getProperties1[i].NoOfBedRooms,
                                        NoOfDoubleBeds = getProperties1[i].NoOfDoubleBeds,
                                        NoOfDoubleSofaBeds = getProperties1[i].NoOfDoubleSofaBeds,
                                        NoOfQueenBeds = getProperties1[i].NoOfQueenBeds,
                                        NoOfSingleBeds = getProperties1[i].NoOfSingleBeds,
                                        NoOfSingleSofaBeds = getProperties1[i].NoOfSingleSofaBeds,
                                        NoOfToilets = getProperties1[i].NoOfToilets,
                                        Parking = getProperties1[i].Parking,
                                        Pool = getProperties1[i].Pool,
                                        ShortTermApartment = getProperties1[i].ShortTermApartment,
                                        Size = getProperties1[i].Size,
                                        Type = getProperties1[i].Type,
                                        WifiLogin = getProperties1[i].WifiLogin
                                    },
                                    PropertyImages = getPropertyImages
                                });
                            }

                            returnModel.MainUserProperties = lstData;
                        }
                        #endregion
                    }
                    message = Resource.success;
                }
                else
                {
                    message = Resource.user_not_exist;
                }
            }
            catch (Exception ex)
            {
                returnModel = new MainUserProperty();
            }
            return returnModel;
        }

        internal User GetUser(int userId)
        {
            return _db.Users.Where(u => u.UserId == userId).FirstOrDefault();
        }

        public List<PropertyViewModel> GetPropertiesForGrantAccess(long userId)
        {
            List<PropertyViewModel> lstData = null;
            List<Property> getProperties = null;
            try
            {
                var getGrantAccessData = _db.tblGrantAccesses.Where(a => a.UserId == userId).FirstOrDefault();
                if (getGrantAccessData != null)
                {
                    if (getGrantAccessData.ForAllProperties == true)
                    {
                        getProperties = _db.Properties.Where(x => x.CreatedBy == getGrantAccessData.CreatedBy && x.IsActive == true)
                        .OrderByDescending(a => a.CreatedDate).ToList();
                    }
                    else
                    {
                        var getAssignedProperties = getGrantAccessData.tblGrantProperties.ToList();

                        if (getAssignedProperties.Count > 0)
                        {
                            getProperties = new List<Property>();
                            for (int i = 0; i < getAssignedProperties.Count; i++)
                            {
                                var property_id = getAssignedProperties[i].PropertyId;

                                var Properties = _db.Properties.Where(x => x.CreatedBy == getGrantAccessData.CreatedBy
                                && x.IsActive == true
                                && x.Id == property_id)
                .OrderByDescending(a => a.CreatedDate).FirstOrDefault();

                                getProperties.Add(Properties);
                            }
                        }
                    }

                    var getProperty = _db.Properties.Where(a => a.CreatedBy == userId && a.IsActive == true)
                        .OrderByDescending(a => a.CreatedDate).ToList();
                    if (getProperty.Count > 0)
                    {
                        getProperties.AddRange(getProperty);
                    }
                }


                if (getProperties != null)
                {
                    if (getProperties.Count > 0)
                    {
                        lstData = new List<PropertyViewModel>();
                        for (int i = 0; i < getProperties.Count; i++)
                        {
                            if (getProperties[i] != null)
                            {
                                var getId = getProperties[i].Id;

                                var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == getId)
                                    .Select(x => new PropertyImageVM()
                                    {
                                        PropertyId = x.PropertyId,
                                        CreatedDate = x.CreatedDate,
                                        Id = x.Id,
                                        ImageUrl = x.ImageUrl,
                                        IsImage = x.IsImage,
                                        IsVideo = x.IsVideo,
                                        VideoThumbnail = x.VideoThumbnail,
                                        VideoUrl = x.VideoUrl
                                    }).ToList();

                                lstData.Add(new PropertyViewModel
                                {
                                    PropertyModel = new PropertyVM
                                    {
                                        AccesstoCode = getProperties[i].AccesstoCode,
                                        AccessToProperty = getProperties[i].AccessToProperty,
                                        Address = getProperties[i].Address,
                                        ApartmentNumber = getProperties[i].ApartmentNumber,
                                        Balcony = getProperties[i].Balcony,
                                        BuildingCode = getProperties[i].BuildingCode,
                                        CityId = getProperties[i].CityId,
                                        CoffeeMachine = getProperties[i].CoffeeMachine,
                                        CountryId = getProperties[i].CountryId,
                                        CreatedBy = getProperties[i].CreatedBy,
                                        CreatedDate = getProperties[i].CreatedDate,
                                        Dishwasher = getProperties[i].Dishwasher,
                                        Doorman = getProperties[i].Doorman,
                                        Elevator = getProperties[i].Elevator,
                                        FloorNumber = getProperties[i].FloorNumber,
                                        Garden = getProperties[i].Garden,
                                        Id = getProperties[i].Id,
                                        IsKingBed = getProperties[i].IsKingBed,
                                        IsSingleBed = getProperties[i].IsSingleBed,
                                        IsSofaBed = getProperties[i].IsSofaBed,
                                        Latitude = getProperties[i].Latitude,
                                        Longitude = getProperties[i].Longitude,
                                        Name = getProperties[i].Name,
                                        NoOfBathrooms = getProperties[i].NoOfBathrooms,
                                        NoOfBedRooms = getProperties[i].NoOfBedRooms,
                                        NoOfDoubleBeds = getProperties[i].NoOfDoubleBeds,
                                        NoOfDoubleSofaBeds = getProperties[i].NoOfDoubleSofaBeds,
                                        NoOfQueenBeds = getProperties[i].NoOfQueenBeds,
                                        NoOfSingleBeds = getProperties[i].NoOfSingleBeds,
                                        NoOfSingleSofaBeds = getProperties[i].NoOfSingleSofaBeds,
                                        NoOfToilets = getProperties[i].NoOfToilets,
                                        Parking = getProperties[i].Parking,
                                        Pool = getProperties[i].Pool,
                                        ShortTermApartment = getProperties[i].ShortTermApartment,
                                        Size = getProperties[i].Size,
                                        Type = getProperties[i].Type,
                                        WifiLogin = getProperties[i].WifiLogin
                                    },
                                    PropertyImages = getPropertyImages
                                });
                            }
                        }
                    }
                }
                message = Resource.success;

            }
            catch (Exception ex)
            {
                lstData = new List<PropertyViewModel>();
            }
            lstData = lstData.OrderByDescending(a => a.PropertyModel.CreatedDate).ToList();
            return lstData;
        }

        public List<PropertyViewModel> GetPropertiesForSubUser(long userId)
        {
            List<PropertyViewModel> lstData = null;
            try
            {
                var getProperties = _db.Properties.Where(A => A.SubUserId == userId && A.IsActive == true).ToList();

                if (getProperties != null)
                {
                    if (getProperties.Count > 0)
                    {
                        lstData = new List<PropertyViewModel>();
                        for (int i = 0; i < getProperties.Count; i++)
                        {
                            var getId = getProperties[i].Id;

                            var getPropertyImages = _db.tblPropertyImages.Where(a => a.PropertyId == getId)
                                .Select(x => new PropertyImageVM()
                                {
                                    PropertyId = x.PropertyId,
                                    CreatedDate = x.CreatedDate,
                                    Id = x.Id,
                                    ImageUrl = x.ImageUrl,
                                    IsImage = x.IsImage,
                                    IsVideo = x.IsVideo,
                                    VideoThumbnail = x.VideoThumbnail,
                                    VideoUrl = x.VideoUrl
                                }).ToList();

                            lstData.Add(new PropertyViewModel
                            {
                                PropertyModel = new PropertyVM
                                {
                                    AccesstoCode = getProperties[i].AccesstoCode,
                                    AccessToProperty = getProperties[i].AccessToProperty,
                                    Address = getProperties[i].Address,
                                    ApartmentNumber = getProperties[i].ApartmentNumber,
                                    Balcony = getProperties[i].Balcony,
                                    BuildingCode = getProperties[i].BuildingCode,
                                    CityId = getProperties[i].CityId,
                                    CoffeeMachine = getProperties[i].CoffeeMachine,
                                    CountryId = getProperties[i].CountryId,
                                    CreatedBy = getProperties[i].CreatedBy,
                                    CreatedDate = getProperties[i].CreatedDate,
                                    Dishwasher = getProperties[i].Dishwasher,
                                    Doorman = getProperties[i].Doorman,
                                    Elevator = getProperties[i].Elevator,
                                    FloorNumber = getProperties[i].FloorNumber,
                                    Garden = getProperties[i].Garden,
                                    Id = getProperties[i].Id,
                                    IsKingBed = getProperties[i].IsKingBed,
                                    IsSingleBed = getProperties[i].IsSingleBed,
                                    IsSofaBed = getProperties[i].IsSofaBed,
                                    Latitude = getProperties[i].Latitude,
                                    Longitude = getProperties[i].Longitude,
                                    Name = getProperties[i].Name,
                                    NoOfBathrooms = getProperties[i].NoOfBathrooms,
                                    NoOfBedRooms = getProperties[i].NoOfBedRooms,
                                    NoOfDoubleBeds = getProperties[i].NoOfDoubleBeds,
                                    NoOfDoubleSofaBeds = getProperties[i].NoOfDoubleSofaBeds,
                                    NoOfQueenBeds = getProperties[i].NoOfQueenBeds,
                                    NoOfSingleBeds = getProperties[i].NoOfSingleBeds,
                                    NoOfSingleSofaBeds = getProperties[i].NoOfSingleSofaBeds,
                                    NoOfToilets = getProperties[i].NoOfToilets,
                                    Parking = getProperties[i].Parking,
                                    Pool = getProperties[i].Pool,
                                    ShortTermApartment = getProperties[i].ShortTermApartment,
                                    Size = getProperties[i].Size,
                                    Type = getProperties[i].Type,
                                    WifiLogin = getProperties[i].WifiLogin
                                },
                                PropertyImages = getPropertyImages
                            });
                        }
                    }
                }
                message = Resource.success;

            }
            catch (Exception ex)
            {
                lstData = new List<PropertyViewModel>();
            }
            return lstData;
        }

        public bool UpdateAccessCodeDetails(long? propertyID, string buildingCode, string accessToProperty)
        {
            bool status = false;
            try
            {
                if (propertyID != null && propertyID != 0)
                {
                    var result = _db.Properties.Where(x => x.Id == propertyID).FirstOrDefault();
                    if (result != null)
                    {
                        if (!string.IsNullOrEmpty(accessToProperty))
                        {
                            result.AccessToProperty = accessToProperty;
                        }
                        if (!string.IsNullOrEmpty(buildingCode))
                        {
                            result.BuildingCode = buildingCode;
                        }
                        _db.SaveChanges();
                        message = Resource.info_update_success;
                        status = true;
                    }
                    else
                    {
                        message = Resource.property_not_exists;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return status;
        }

        public List<SelectListItem> GetCountriesSelect()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var data = _db.Countries.Where(x => x.IsActive == true).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                listItems.Add(new SelectListItem
                {
                    Text = data[i].Name,
                    Value = data[i].CountryId.ToString()
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetCitiesSelect()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            var data = _db.Cities.Where(x => x.IsActive == true).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                listItems.Add(new SelectListItem
                {
                    Text = data[i].Name,
                    Value = data[i].CityId.ToString()
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetPropertiesSelect(long user_id)
        {
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;

            List<SelectListItem> listItems = new List<SelectListItem>();
            var items = _db.Properties.Where(x => x.IsActive == true && (x.CreatedBy == user_id || x.SubUserId == user_id)).ToList();
            foreach (var item in items)
            {
                listItems.Add(new SelectListItem
                {
                    Text = item.Name + ", " + item.Address,
                    Value = item.Id.ToString()
                });
            }
            return listItems;
        }
        public int GetUserPropertyCount(long user_id)
        {
            var PropCount = 0;
            try
            {
                PropCount = _db.Properties.Where(x => x.IsActive == true && x.CreatedBy == user_id).Count();
            }
            catch (Exception ex)
            {
                PropCount = 0;
            }
            return PropCount;
        }
        public List<SelectListItem> GetAllAirbnbProperty()
        {
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;

            List<SelectListItem> listItems = new List<SelectListItem>();
            var items = _db.Properties.Where(x => x.IsActive == true && x.ShortTermApartment == true).ToList();
            foreach (var item in items)
            {
                listItems.Add(new SelectListItem
                {
                    Text = item.Name + ", " + item.Address,
                    Value = item.Id.ToString()
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetSubServicesSelect(long category_id)
        {
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;

            List<SelectListItem> listItems = new List<SelectListItem>();
            var items = _db.SubCategories.Where(x => x.IsActive == true
            && x.CatId == category_id).ToList();
            foreach (var item in items)
            {
                item.Name = culVal == "fr-FR" ? item.Name_French
: culVal == "ru-RU" ? item.Name_Russian
: culVal == "he-IL" ? item.Name_Hebrew
: item.Name;

                listItems.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                });
            }

            return listItems;
        }

        public List<SelectListItem> GetSubSubServicesSelect(long sub_category_id)
        {
            var culVal = System.Globalization.CultureInfo.CurrentCulture.Name;

            List<SelectListItem> listItems = new List<SelectListItem>();
            var items = _db.SubSubCategories.Where(x => x.IsActive == true
            && x.SubCatId == sub_category_id).ToList();
            foreach (var item in items)
            {
                item.Name = culVal == "fr-FR" ? item.Name_French
: culVal == "ru-RU" ? item.Name_Russian
: culVal == "he-IL" ? item.Name_Hebrew
: item.Name;

                listItems.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                });
            }

            return listItems;
        }

        public SelectList GetServiceSelectList()
        {
            Array values = Enum.GetValues(typeof(JobServiceRepeatList));
            List<SelectListItem> items = new List<SelectListItem>(values.Length);

            foreach (var i in values)
            {
                items.Add(new SelectListItem
                {
                    Text = Enum.GetName(typeof(JobServiceRepeatList), i),
                    Value = i.ToString()
                });
            }

            return new SelectList(items);
        }

        public List<InventoryViewModel> GetInventory()
        {
            List<InventoryViewModel> objInvertory = new List<InventoryViewModel>();
            try
            {
                objInvertory = _db.Inventories.Where(x => x.IsActive == true).Select(x => new InventoryViewModel()
                {
                    InventoryId = x.InventoryId,
                    Name = x.Name,
                    Description = x.Description,
                    Stock = x.Stock,
                    Price = x.Price,
                    Image = x.Image
                }).ToList();
            }
            catch (Exception ex)
            {
                objInvertory = null;
            }
            return objInvertory;
        }
        public List<JobInventory> GetInventoryByJobReqId(long JobReqPropId, long PropId)
        {
            List<JobInventory> objJobInvertory = new List<JobInventory>();
            try
            {
                objJobInvertory = _db.JobInventories.Where(x => x.PropertyId == PropId && x.JobRequestId == JobReqPropId).ToList();

            }
            catch (Exception ex)
            {
                objJobInvertory = null;
            }
            return objJobInvertory;
        }
        public List<JobRequestCheckList> GetJobReqChecklistByJobId(long jobId)
        {
            List<JobRequestCheckList> obj = new List<JobRequestCheckList>();
            try
            {
                obj = _db.JobRequestCheckLists.Where(x => x.JobRequestId == jobId).ToList();
            }
            catch (Exception ex)
            {
                obj = null;
            }
            return obj;
        }

        public JobRequestPropertyService GetJobReqPropServiceByJobId(long jobId)
        {
            JobRequestPropertyService obj = new JobRequestPropertyService();
            try
            {
                obj = _db.JobRequestPropertyServices.Where(x => x.JobRequestId == jobId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                obj = null;
            }
            return obj;
        }
        public bool DeletePropertyImg(long id)
        {
            bool status = false;
            var data = _db.tblPropertyImages.Where(x => x.Id == id).FirstOrDefault();
            if (data != null)
            {
                _db.tblPropertyImages.Remove(data);
                _db.SaveChanges();
                status = true;
            }
            return status;
        }

        public ServicePriceViewModel AutoPriceService(long propertyId, long subCategoryId)
        {
            ServicePriceViewModel priceModel = new ServicePriceViewModel
            {
                ClientPrice = 0,
                Price = 0,
                TimeToDo = "0"
            };
            var property = _db.Properties.FirstOrDefault(p => p.Id == propertyId);
            if (property != null)
            {
                var autoPrice = _db.ServiceAutoPrices.FirstOrDefault(sa => sa.ServiceId == subCategoryId && sa.Size == property.Size);
                if (autoPrice != null)
                {
                    priceModel.ClientPrice = autoPrice.Price;
                    priceModel.Price = autoPrice.ServiceProviderPrice;
                    priceModel.TimeToDo = autoPrice.Time;
                }
            }
            return priceModel;
        }

        public void SendNotification(long? AssignedUserId, long? JobRequestId, long? ToUserId, string msg, string serviceName, string PropertyName, string Address)
        {
            Notification _notification1 = new Notification();
            _notification1.CreatedDate = DateTime.Now;
            _notification1.FromUserId = AssignedUserId;
            _notification1.IsActive = true;
            _notification1.JobRequestId = JobRequestId;
            _notification1.NotificationStatus = Enums.NotificationStatus.Assigned.GetHashCode();
            _notification1.ToUserId = ToUserId;
            _notification1.Text = msg;

            _notification1.ServiceName = serviceName;
            _notification1.PropertyName = PropertyName;
            _notification1.PropertyAddress = Address;
            _db.Notifications.Add(_notification1);
            _db.SaveChanges();
        }

    }
}