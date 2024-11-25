using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    public class Room : Common
    {
        [Required]
        [MaxLength(50)]
        public string RoomNumber { get; set; }

        [Required]
        public string RoomTypeId { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        // Navigation properties https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations
        public RoomType RoomType { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<BookingRoom> BookingRooms { get; set; }
    }
}
