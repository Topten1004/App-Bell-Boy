using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels.ChannelManager.Smoobu
{
    public class SmoobuReservationDataViewModel
    {
        public long Id { get; set; }
        public ChannelManagerAccomodationViewModel Channel { get; set; }

        public ChannelManagerAccomodationViewModel Apartment { get; set; }

        public string Type { get; set; }

        public string Arrival { get; set; }

        public string Departure { get; set; }

        [JsonProperty(PropertyName = "guest-name")]
        public string GuestName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        [JsonProperty(PropertyName = "check-in")]
        public string CheckIn { get; set; }

        [JsonProperty(PropertyName = "check-out")]
        public string Checkout { get; set; }

        public string Notice { get; set; }

        public decimal? Price { get; set; }

        [JsonProperty(PropertyName = "price-paid")]
        public string PricePaid { get; set; }

        public decimal? Deposit { get; set; }

        [JsonProperty(PropertyName = "deposit-paid")]
        public string DepositPaid { get; set; }
    }
}