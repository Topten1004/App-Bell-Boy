using BroomService.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BroomService.ViewModels
{
    public class ChannelManagerViewModel
    {
        public long ChannelManagerId { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

    }

    public class UserChannelManagerViewModel
    {
        public long UserChannelManagerId { get; set; }

        public long ChannelManagerId { get; set; }

        public long UserId { get; set; }

        public string ApiKey { get; set; }

        public bool Active { get; set; }

        public string ChannelManagerUsername { get; set; }

        public string ChannelManagerPassword { get; set; }

    }

    public class UserChannelManagerSettingsViewModel
    {
        public long UserChannelManagerSettingsId { get; set; }

        public long UserChannelManagerId { get; set; }

        [Display(Name ="Fast Order")]
        public long? FastOrderId { get; set; }

        public bool CheckInCleaning { get; set; }

        public bool WindowsCleaning { get; set; }

        public bool LundryPickup { get; set; }

        public bool LinenRentals { get; set; }

        public bool Amenties { get; set; }

    }
}