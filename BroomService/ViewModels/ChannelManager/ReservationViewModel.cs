﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels.ChannelManager
{
    public class ReservationViewModel
    {
        public long ReservationId { get; set; }

        public string ReservationType { get; set; }

        public string ArrivalDate { get; set; }

        public string DepartureDate { get; set; }

        public long ApartmentId { get; set; }

        public string ApartmentName { get; set; }

        public long ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string GuestName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int Adults { get; set; }

        public int Children { get; set; }

        public string CheckIn { get; set; }

        public string Checkout { get; set; }
        
        public string Notice { get; set; }

        public float Price { get; set; }

        public string PricePaid { get; set; }

        public float Deposit { get; set; }

        public string DepositPaid { get; set; }

    }
}