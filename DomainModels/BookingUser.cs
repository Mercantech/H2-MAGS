using System.Collections.Generic;

namespace DomainModels
{
    public class BookingUser
    {
        public string BookingId { get; set; }
        public Booking Booking { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
} 