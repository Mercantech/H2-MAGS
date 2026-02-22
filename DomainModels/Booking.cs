using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels
{
    // Booking.cs
    public class Booking : Common
    {
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public string RoomId { get; set; } = string.Empty;
        public Room? Room { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; } = "Confirmed"; // Confirmed, Pending, Cancelled, CheckedIn, CheckedOut
        public string? SpecialRequests { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }

    // DTO for booking creation / POST
    public class BookingPostDto
    {
        [Required(ErrorMessage = "Bruger ID er påkrævet")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rum ID er påkrævet")]
        public string RoomId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start dato er påkrævet")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Slut dato er påkrævet")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Antal gæster er påkrævet")]
        [Range(1, 10, ErrorMessage = "Antal gæster skal være mellem 1 og 10")]
        public int NumberOfGuests { get; set; }

        [StringLength(500, ErrorMessage = "Særlige ønsker må maksimalt være 500 tegn")]
        public string? SpecialRequests { get; set; }
    }

    // DTO for booking retrieval / GET
    public class BookingGetDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string? UserUsername { get; set; }
        public string RoomId { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public string? HotelName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string? SpecialRequests { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO for booking update / PUT
    public class BookingPutDto
    {
        [Required(ErrorMessage = "Booking ID er påkrævet")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bruger ID er påkrævet")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rum ID er påkrævet")]
        public string RoomId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start dato er påkrævet")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Slut dato er påkrævet")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Antal gæster er påkrævet")]
        [Range(1, 10, ErrorMessage = "Antal gæster skal være mellem 1 og 10")]
        public int NumberOfGuests { get; set; }

        [StringLength(20, ErrorMessage = "Booking status må maksimalt være 20 tegn")]
        public string BookingStatus { get; set; } = "Confirmed";

        [StringLength(500, ErrorMessage = "Særlige ønsker må maksimalt være 500 tegn")]
        public string? SpecialRequests { get; set; }
    }
}
