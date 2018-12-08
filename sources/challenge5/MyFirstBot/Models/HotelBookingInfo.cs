using System;

namespace MyFirstBot.Models
{
    public class HotelBookingInfo
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public UserProfile UserInfo { get; set; }

        public RoomType RoomType { get; set; }
    }

    public enum RoomType
    {
        RitzySuite = 0,
        SuperiorDouble = 1,
        DeluxeTwin = 2
    }
}