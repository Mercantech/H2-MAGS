using System.Collections.Generic;

namespace DomainModels
{
    public class BookingRoom
    {
        public string BookingId { get; set; }
        public Booking Booking { get; set; }

        public string RoomId { get; set; }
        public Room Room { get; set; }
    }
} 