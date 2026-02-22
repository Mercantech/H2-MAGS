using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Mapping
{
    public class BookingMapping
    {
        /// <summary>
        /// Konverterer Booking til BookingGetDto
        /// </summary>
        public static BookingGetDto ToBookingGetDto(Booking booking)
        {
            return new BookingGetDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                UserEmail = booking.User?.Email,
                UserUsername = booking.User?.Username,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.Number,
                RoomType = booking.Room?.RoomType,
                HotelName = booking.Room?.Hotel?.Name ?? string.Empty,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                BookingStatus = booking.BookingStatus,
                SpecialRequests = booking.SpecialRequests,
                CheckInTime = booking.CheckInTime,
                CheckOutTime = booking.CheckOutTime,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }

        /// <summary>
        /// Konverterer liste af Booking til liste af BookingGetDto
        /// </summary>
        public static List<BookingGetDto> ToBookingGetDtos(List<Booking> bookings)
        {
            return bookings.Select(b => ToBookingGetDto(b)).ToList();
        }

        /// <summary>
        /// Konverterer BookingPostDto til Booking entity
        /// </summary>
        public static Booking ToBookingFromPostDto(BookingPostDto bookingPostDto, decimal roomPricePerNight)
        {
            var nights = (bookingPostDto.EndDate - bookingPostDto.StartDate).Days;
            return new Booking
            {
                Id = Guid.NewGuid().ToString(),
                UserId = bookingPostDto.UserId,
                RoomId = bookingPostDto.RoomId,
                StartDate = bookingPostDto.StartDate,
                EndDate = bookingPostDto.EndDate,
                NumberOfGuests = bookingPostDto.NumberOfGuests,
                TotalPrice = roomPricePerNight * nights,
                BookingStatus = "Confirmed",
                SpecialRequests = bookingPostDto.SpecialRequests,
                CreatedAt = DateTime.UtcNow.AddHours(2),
                UpdatedAt = DateTime.UtcNow.AddHours(2)
            };
        }

        /// <summary>
        /// Opdaterer Booking entity med data fra BookingPutDto
        /// </summary>
        public static void UpdateBookingFromPutDto(Booking booking, BookingPutDto bookingPutDto, decimal roomPricePerNight)
        {
            var nights = (bookingPutDto.EndDate - bookingPutDto.StartDate).Days;
            booking.UserId = bookingPutDto.UserId;
            booking.RoomId = bookingPutDto.RoomId;
            booking.StartDate = bookingPutDto.StartDate;
            booking.EndDate = bookingPutDto.EndDate;
            booking.NumberOfGuests = bookingPutDto.NumberOfGuests;
            booking.TotalPrice = roomPricePerNight * nights;
            booking.BookingStatus = bookingPutDto.BookingStatus;
            booking.SpecialRequests = bookingPutDto.SpecialRequests;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(2);
        }
    }
}
