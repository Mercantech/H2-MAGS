using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DBContext;
using DomainModels;
using DomainModels.DTOs.Booking;

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
                    UserId = b.UserId,
                    RoomId = b.RoomId,
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
                    UserId = b.UserId,
                    RoomId = b.RoomId,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut
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

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.UserId = updateBookingDto.UserId;
            booking.RoomId = updateBookingDto.RoomId;
            booking.CheckIn = updateBookingDto.CheckIn;
            booking.CheckOut = updateBookingDto.CheckOut;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Entry(booking).State = EntityState.Modified;

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
        public async Task<ActionResult<CreateBookingDTO>> PostBooking(CreateBookingDTO createBookingDto)
        {
            // Validering: Sikre at CheckIn er i fremtiden
            if (createBookingDto.CheckIn <= DateTime.Now)
            {
                return BadRequest("Check-in dato skal være i fremtiden.");
            }

            // Validering: Sikre at CheckOut er senere end CheckIn
            if (createBookingDto.CheckOut <= createBookingDto.CheckIn)
            {
                return BadRequest("Check-out dato skal være senere end check-in datoen.");
            }

            // Tjek om værelset allerede er booket i den ønskede periode
            var overlappingBookings = await _context.Bookings
                .Where(b => b.RoomId == createBookingDto.RoomId &&
                            b.CheckIn < createBookingDto.CheckOut && 
                            b.CheckOut > createBookingDto.CheckIn)
                .ToListAsync();

            if (overlappingBookings.Any())
            {
                return BadRequest("Dette værelse er allerede booket i den ønskede periode.");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = createBookingDto.UserId,
                RoomId = createBookingDto.RoomId,
                CheckIn = createBookingDto.CheckIn,
                CheckOut = createBookingDto.CheckOut,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, createBookingDto);
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
    }
}
