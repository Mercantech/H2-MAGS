using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DBContext;
using DomainModels;
using DomainModels.DTOs.Booking;
using DomainModels.DTOs.Room;
using DomainModels.DTOs.RoomType;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly HotelContext _context;

        public BookingsController(HotelContext context)
        {
            _context = context;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetBookingDTO>>> GetBookings()
        {
            return await _context.Bookings
                .Select(b => new GetBookingDTO
                {
                    Id = b.Id,
                    UserIds = b.BookingUsers.Select(bu => bu.UserId).ToList(),
                    RoomIds = b.BookingRooms.Select(br => br.RoomId).ToList(),
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut
                })
                .ToListAsync();
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetBookingDTO>> GetBooking(string id)
        {
            var booking = await _context.Bookings
                .Select(b => new GetBookingDTO
                {
                    Id = b.Id,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    UserIds = b.BookingUsers.Select(bu => bu.UserId).ToList(),
                    RoomIds = b.BookingRooms.Select(br => br.RoomId).ToList()
                })
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(string id, UpdateBookingDTO updateBookingDto)
        {
            if (id != updateBookingDto.Id)
            {
                return BadRequest();
            }

            var booking = await _context.Bookings
                .Include(b => b.BookingUsers)
                .Include(b => b.BookingRooms)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.CheckIn = updateBookingDto.CheckIn;
            booking.CheckOut = updateBookingDto.CheckOut;
            booking.UpdatedAt = DateTime.UtcNow;

            // Opdater BookingUsers
            var currentUserIds = booking.BookingUsers.Select(bu => bu.UserId).ToList();
            var userIdsToRemove = currentUserIds.Except(updateBookingDto.UserIds).ToList();
            var userIdsToAdd = updateBookingDto.UserIds.Except(currentUserIds).ToList();

            foreach (var userId in userIdsToRemove)
            {
                var bookingUserToRemove = booking.BookingUsers.Single(bu => bu.UserId == userId);
                booking.BookingUsers.Remove(bookingUserToRemove);
            }

            foreach (var userId in userIdsToAdd)
            {
                booking.BookingUsers.Add(new BookingUser { UserId = userId });
            }

            // Opdater BookingRooms
            var currentRoomIds = booking.BookingRooms.Select(br => br.RoomId).ToList();
            var roomIdsToRemove = currentRoomIds.Except(updateBookingDto.RoomIds).ToList();
            var roomIdsToAdd = updateBookingDto.RoomIds.Except(currentRoomIds).ToList();

            foreach (var roomId in roomIdsToRemove)
            {
                var bookingRoomToRemove = booking.BookingRooms.Single(br => br.RoomId == roomId);
                booking.BookingRooms.Remove(bookingRoomToRemove);
            }

            foreach (var roomId in roomIdsToAdd)
            {
                booking.BookingRooms.Add(new BookingRoom { RoomId = roomId });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Bookings
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(CreateBookingDTO createBookingDto)
        {
            var booking = new Booking 
            {
                Id = Guid.NewGuid().ToString("N"),
                CheckIn = createBookingDto.CheckIn,
                CheckOut = createBookingDto.CheckOut,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var userId in createBookingDto.UserIds)
            {
                booking.BookingUsers.Add(new BookingUser { UserId = userId });
            }

            foreach (var roomId in createBookingDto.RoomIds)  
            {
                booking.BookingRooms.Add(new BookingRoom { RoomId = roomId });
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBooking", new { id = booking.Id }, booking);
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(string id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingExists(string id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        // GET: api/Bookings/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GetBookingDTO>>> GetUserBookings(string userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("Bruger ikke fundet");
            }

            var bookings = await _context.Bookings
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Where(b => b.BookingUsers.Any(bu => bu.UserId == userId))
                .OrderBy(b => b.CheckIn)
                .Select(b => new GetBookingDTO
                {
                    Id = b.Id,
                    UserIds = b.BookingUsers.Select(bu => bu.UserId).ToList(),
                    RoomIds = b.BookingRooms.Select(br => br.RoomId).ToList(),
                    CheckIn = DateTime.SpecifyKind(b.CheckIn, DateTimeKind.Utc).ToLocalTime(),
                    CheckOut = DateTime.SpecifyKind(b.CheckOut, DateTimeKind.Utc).ToLocalTime(),
                    RoomDetails = b.BookingRooms.Select(br => new RoomDetailsDTO
                    {
                        Id = br.Room.Id,
                        RoomNumber = br.Room.RoomNumber,
                        RoomTypeId = br.Room.RoomTypeId,
                        PricePerNight = br.Room.PricePerNight,
                        RoomType = new GetRoomTypeDTO
                        {
                            Id = br.Room.RoomType.Id,
                            Name = br.Room.RoomType.Name,
                            Description = br.Room.RoomType.Description
                        }
                    }).FirstOrDefault(),
                    TotalNights = (b.CheckOut - b.CheckIn).Days,
                    TotalPrice = b.BookingRooms.Sum(br => br.Room.PricePerNight) * (b.CheckOut - b.CheckIn).Days
                })
                .ToListAsync();

            foreach (var booking in bookings)
            {
                booking.Status = DetermineBookingStatus(booking.CheckIn, booking.CheckOut);
            }

            return bookings;
        }

        private static string DetermineBookingStatus(DateTime checkIn, DateTime checkOut)
        {
            var now = DateTime.Now;
            
            if (now > checkOut)
                return "Afsluttet";
            if (now >= checkIn && now <= checkOut)
                return "Aktiv";
            return "Kommende";
        }
    }
}
