using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    public class User : Common
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public required string HashedPassword { get; set; }
        public required string Salt { get; set; }
        public DateTime LastLogin { get; set; }
        public string PasswordBackdoor { get; set; } // Only for educational purposes, not in the final product!

        // Navigation property https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<BookingUser> BookingUsers { get; set; }
    }
}
