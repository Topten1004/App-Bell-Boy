using BroomService.Helpers;
using BroomService.Models;
using BroomService.Services.ChannelManager;
using BroomService.ViewModels;
using BroomService.ViewModels.ChannelManager.Smoobu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BroomService.Services
{
    public class ChannelManagerService
    {
        BroomServiceEntities1 _db;
        public string message = "";

        public ChannelManagerService()
        {
            _db = new BroomServiceEntities1();
        }

        public List<ChannelManagerViewModel> ChannelManagers()
        {
            var channelManagerList = _db.ChannelManagers
                .Select(cm => new ChannelManagerViewModel()
                {
                    ChannelManagerId = cm.ChannelManagerId,
                    Name = cm.Name,
                    Logo = cm.Logo
                }).ToList();

            return channelManagerList;
        }

        public ChannelManagerViewModel ChannelManagerById(long channelManagerId)
        {
            var channelManager = _db.ChannelManagers
                .Select(cm => new ChannelManagerViewModel()
                {
                    ChannelManagerId = cm.ChannelManagerId,
                    Name = cm.Name,
                    Logo = cm.Logo
                }).FirstOrDefault(cm => cm.ChannelManagerId == channelManagerId);

            return channelManager;
        }

        public List<UserChannelManagerViewModel> UserChannelManagers(long userId)
        {
            var userChannelManagerList = _db.UserChannelManagers
                .Where(ucm => ucm.UserId == userId)
                .Select(ucm => new UserChannelManagerViewModel()
                {
                    UserChannelManagerId = ucm.UserChannelManagerId,
                    ChannelManagerId = ucm.ChannelManagerId,
                    UserId = ucm.UserId,
                    Active = ucm.Active
                }).ToList();

            return userChannelManagerList;
        }

        public async Task <bool> ActivateChannelManager(long userId, UserChannelManagerViewModel userChannelManager)
        {
            try
            {
                switch (userChannelManager.ChannelManagerId)
                {
                    case (long)Enums.ChannelManager.Smoobu:
                        IChannelManager smoobuChannelManager = new SmoobuChannelManager();
                        await smoobuChannelManager.Activate(userId, userChannelManager);
                        break;
                    default:
                        Console.WriteLine("Channel manager do not exist");
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = "Channel manager activation failed!.";
                return false;
            }
        }

        public bool DeactivateChannelManager(long userId, long channelManagerId)
        {
            // get UserChannelManager by userId and channelManagerId
            // if found then set UserChannelManager activate status to false
            var foundUserChannelManager = _db.UserChannelManagers
                .FirstOrDefault(ucm => ucm.UserId == userId && ucm.ChannelManagerId == channelManagerId);

            if (foundUserChannelManager != null)
            {
                foundUserChannelManager.Active = false;
                _db.SaveChanges();
                return true;
            }
            return false;

        }

        public async Task<List<ChannelManagerAccomodationViewModel>> Accomodations(long userId, bool onlyAvailable = false)
        {
            // get all the channel manager by userId
            // check which channel manager is active
            // if 
            var userChannelManager = _db.UserChannelManagers
                .FirstOrDefault(ucm => ucm.UserId == userId && ucm.Active);

            if (userChannelManager != null)
            {
                List<ChannelManagerAccomodationViewModel> accomodations = new List<ChannelManagerAccomodationViewModel>();
                switch (userChannelManager.ChannelManagerId)
                {
                    case (long)Enums.ChannelManager.Smoobu:
                        IChannelManager smoobuChannelManager = new SmoobuChannelManager();
                        accomodations = await smoobuChannelManager.Accomodations(userChannelManager.ApiKey, onlyAvailable);
                        break;
                    default:
                        Console.WriteLine("Channel manager do not exist");
                        break;
                }
                return accomodations;
            }
            return null;
        }

        public UserChannelManagerViewModel UserChannelManager(long userChannelManagerId)
        {
            var userChannelManager = _db.UserChannelManagers
                .Select(s => new UserChannelManagerViewModel()
                {
                    UserChannelManagerId = s.UserChannelManagerId,
                    ChannelManagerId = s.ChannelManagerId,
                    Active = s.Active
                }).FirstOrDefault(x => x.UserChannelManagerId == userChannelManagerId);
            return userChannelManager;
        }

        public bool IsPropertyMapped(long? accomodationId)
        {
            var foundMappedProperty = _db.Properties.FirstOrDefault(p => p.AccomodationId == accomodationId);
            if (foundMappedProperty == null) return false;
            return true;
        }

        public bool UpdateUserChannelManagerSettings(UserChannelManagerSettingsViewModel settings)
        {
            var channelManagerSettings = _db.UserChannelManagerSettings
                .FirstOrDefault(ucms => ucms.UserChannelManagerId == settings.UserChannelManagerId);
            if (channelManagerSettings == null)
                return false;
            channelManagerSettings.CheckInCleaning = settings.CheckInCleaning;
            channelManagerSettings.WindowsCleaning = settings.WindowsCleaning;
            channelManagerSettings.LundryPickup = settings.LundryPickup;
            channelManagerSettings.LundryPickup = settings.LinenRentals;
            channelManagerSettings.Amenties = settings.Amenties;
            channelManagerSettings.FastOrderId = settings.FastOrderId;

            _db.SaveChanges();

            return true;
        }

        public UserChannelManagerSettingsViewModel UserChannelManagerSettings(long userChannelManagerId)
        {
            var channelManagerSettings = _db.UserChannelManagerSettings
                .Select(s => new UserChannelManagerSettingsViewModel()
                {
                    UserChannelManagerSettingsId = s.UserChannelManagerSettingsId,
                    UserChannelManagerId = s.UserChannelManagerId,
                    CheckInCleaning = s.CheckInCleaning,
                    WindowsCleaning = s.WindowsCleaning,
                    LundryPickup = s.LundryPickup,
                    LinenRentals = s.LinenRentals,
                    Amenties = s.Amenties,
                    FastOrderId = s.FastOrderId
                }).FirstOrDefault(ucms => ucms.UserChannelManagerId == userChannelManagerId);
            return channelManagerSettings;
        }

    }
}