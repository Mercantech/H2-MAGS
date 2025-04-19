using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    public class Booking : Common
    {
        [Required]
        public DateTime CheckIn { get; set; }

        [Required]
        public DateTime CheckOut { get; set; }

        // Navigation properties https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations
        public ICollection<BookingUser> BookingUsers { get; set; }
        public ICollection<BookingRoom> BookingRooms { get; set; }
    }
}
