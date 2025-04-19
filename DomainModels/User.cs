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

        public string? HashedPassword { get; set; }
        public string? Salt { get; set; }
        public DateTime LastLogin { get; set; }
        public string? PasswordBackdoor { get; set; }

        // Google-specifik information
        public string? GoogleId { get; set; }
        public string? PictureUrl { get; set; }
        public bool IsGoogleUser { get; set; }

        // Navigation property https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations
        public ICollection<Booking> Bookings { get; set; }

        public bool IsEmailConfirmed { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public ICollection<BookingUser> BookingUsers { get; set; }
    }
}
