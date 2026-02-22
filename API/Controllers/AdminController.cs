using API.Data;
using API.Services;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    /// <summary>
    /// Controller til admin/receptionist funktionalitet
    /// </summary>
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDBContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Henter dashboard statistikker for receptionisten
        /// </summary>
        /// <returns>Dashboard statistikker</returns>
        /// <response code="200">Statistikker hentet succesfuldt</response>
        /// <response code="401">Ikke autoriseret</response>
        /// <response code="500">Intern serverfejl</response>
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                _logger.LogInformation("Henter dashboard statistikker");

                var totalBookings = await _context.Bookings.CountAsync();
                var activeBookings = await _context.Bookings
                    .Where(b => b.StartDate <= DateTime.UtcNow.AddHours(2) && b.EndDate >= DateTime.UtcNow.AddHours(2))
                    .CountAsync();
                var totalRooms = await _context.Rooms.CountAsync();
                var availableRooms = await _context.Rooms
                    .Where(r => !_context.Bookings.Any(b => b.RoomId == r.Id && 
                        b.StartDate <= DateTime.UtcNow.AddHours(2) && 
                        b.EndDate >= DateTime.UtcNow.AddHours(2)))
                    .CountAsync();

                var stats = new
                {
                    totalBookings,
                    activeBookings,
                    totalRooms,
                    availableRooms,
                    totalRevenue = 0, // Placeholder - would need pricing logic
                    lastUpdated = DateTime.UtcNow.AddHours(2)
                };

                _logger.LogInformation("Dashboard statistikker hentet: {TotalBookings} bookinger, {ActiveBookings} aktive", 
                    totalBookings, activeBookings);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af dashboard statistikker");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af statistikker");
            }
        }

        /// <summary>
        /// Henter seneste bookinger for dashboard
        /// </summary>
        /// <param name="limit">Antal bookinger at hente (default: 5)</param>
        /// <returns>Seneste bookinger</returns>
        /// <response code="200">Bookinger hentet succesfuldt</response>
        /// <response code="401">Ikke autoriseret</response>
        /// <response code="500">Intern serverfejl</response>
        [HttpGet("dashboard/recent-bookings")]
        public async Task<IActionResult> GetRecentBookings([FromQuery] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Henter seneste {Limit} bookinger", limit);

                var recentBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.Hotel)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(limit)
                    .Select(b => new
                    {
                        b.Id,
                        b.StartDate,
                        b.EndDate,
                        b.CreatedAt,
                        User = new
                        {
                            b.User.Id,
                            b.User.Username,
                            b.User.Email
                        },
                        Room = new
                        {
                            b.Room.Id,
                            b.Room.Number,
                            b.Room.Capacity,
                            Hotel = new
                            {
                                b.Room.Hotel.Id,
                                b.Room.Hotel.Name
                            }
                        }
                    })
                    .ToListAsync();

                _logger.LogInformation("Hentet {Count} seneste bookinger", recentBookings.Count);

                return Ok(recentBookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af seneste bookinger");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af bookinger");
            }
        }

        /// <summary>
        /// Henter værelses tilgængelighed for dashboard
        /// </summary>
        /// <returns>Værelses tilgængelighed</returns>
        /// <response code="200">Tilgængelighed hentet succesfuldt</response>
        /// <response code="401">Ikke autoriseret</response>
        /// <response code="500">Intern serverfejl</response>
        [HttpGet("dashboard/room-availability")]
        public async Task<IActionResult> GetRoomAvailability()
        {
            try
            {
                _logger.LogInformation("Henter værelses tilgængelighed");

                var rooms = await _context.Rooms
                    .Include(r => r.Hotel)
                    .Select(r => new
                    {
                        r.Id,
                        r.Number,
                        r.Capacity,
                        Hotel = new
                        {
                            r.Hotel.Id,
                            r.Hotel.Name
                        },
                        IsAvailable = !_context.Bookings.Any(b => b.RoomId == r.Id && 
                            b.StartDate <= DateTime.UtcNow.AddHours(2) && 
                            b.EndDate >= DateTime.UtcNow.AddHours(2))
                    })
                    .ToListAsync();

                var totalRooms = rooms.Count;
                var availableRooms = rooms.Count(r => r.IsAvailable);
                var totalCapacity = rooms.Sum(r => r.Capacity);

                var availability = new
                {
                    totalRooms,
                    availableRooms,
                    totalCapacity,
                    rooms = rooms.Take(10) // Limit for dashboard display
                };

                _logger.LogInformation("Værelses tilgængelighed hentet: {AvailableRooms}/{TotalRooms} ledige", 
                    availableRooms, totalRooms);

                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af værelses tilgængelighed");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af tilgængelighed");
            }
        }

        /// <summary>
        /// Henter bookinger for en specifik dato
        /// </summary>
        /// <param name="date">Dato at hente bookinger for (YYYY-MM-DD format)</param>
        /// <returns>Bookinger for den angivne dato</returns>
        /// <response code="200">Bookinger hentet succesfuldt</response>
        /// <response code="400">Ugyldig dato format</response>
        /// <response code="401">Ikke autoriseret</response>
        /// <response code="500">Intern serverfejl</response>
        [HttpGet("bookings/by-date")]
        public async Task<IActionResult> GetBookingsByDate([FromQuery] string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var targetDate))
                {
                    return BadRequest("Ugyldig dato format. Brug YYYY-MM-DD format.");
                }

                _logger.LogInformation("Henter bookinger for dato: {Date}", date);

                var bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                        .ThenInclude(r => r.Hotel)
                    .Where(b => b.StartDate.Date <= targetDate.Date && b.EndDate.Date >= targetDate.Date)
                    .Select(b => new
                    {
                        b.Id,
                        b.StartDate,
                        b.EndDate,
                        b.CreatedAt,
                        User = new
                        {
                            b.User.Id,
                            b.User.Username,
                            b.User.Email
                        },
                        Room = new
                        {
                            b.Room.Id,
                            b.Room.Number,
                            b.Room.Capacity,
                            Hotel = new
                            {
                                b.Room.Hotel.Id,
                                b.Room.Hotel.Name
                            }
                        }
                    })
                    .ToListAsync();

                _logger.LogInformation("Hentet {Count} bookinger for dato {Date}", bookings.Count, date);

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af bookinger for dato: {Date}", date);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af bookinger");
            }
        }

        /// <summary>
        /// Henter tilgængelige værelser for en dato periode
        /// </summary>
        /// <param name="startDate">Start dato (YYYY-MM-DD format)</param>
        /// <param name="endDate">Slut dato (YYYY-MM-DD format)</param>
        /// <returns>Tilgængelige værelser</returns>
        /// <response code="200">Værelser hentet succesfuldt</response>
        /// <response code="400">Ugyldig dato format</response>
        /// <response code="401">Ikke autoriseret</response>
        /// <response code="500">Intern serverfejl</response>
        [HttpGet("rooms/available")]
        public async Task<IActionResult> GetAvailableRooms([FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                {
                    return BadRequest("Ugyldig dato format. Brug YYYY-MM-DD format.");
                }

                if (start >= end)
                {
                    return BadRequest("Start dato skal være før slut dato.");
                }

                _logger.LogInformation("Henter tilgængelige værelser fra {StartDate} til {EndDate}", startDate, endDate);

                var availableRooms = await _context.Rooms
                    .Include(r => r.Hotel)
                    .Where(r => !_context.Bookings.Any(b => b.RoomId == r.Id && 
                        b.StartDate < end && b.EndDate > start))
                    .Select(r => new
                    {
                        r.Id,
                        r.Number,
                        r.Capacity,
                        Hotel = new
                        {
                            r.Hotel.Id,
                            r.Hotel.Name,
                            r.Hotel.Address
                        }
                    })
                    .ToListAsync();

                _logger.LogInformation("Hentet {Count} tilgængelige værelser", availableRooms.Count);

                return Ok(availableRooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af tilgængelige værelser");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af værelser");
            }
        }

        /// <summary>
        /// Henter dagens check-ins og check-outs
        /// </summary>
        /// <returns>Dagens check-ins og check-outs</returns>
        /// <response code="200">Check-ins og check-outs hentet succesfuldt</response>
        /// <response code="401">Ikke autoriseret</response>
        /// <response code="500">Intern serverfejl</response>
        [HttpGet("daily-checkins")]
        public async Task<IActionResult> GetDailyCheckIns()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                
                _logger.LogInformation("Henter dagens check-ins og check-outs for {Date}", today.ToString("yyyy-MM-dd"));
                
                // Hent check-ins (bookinger der starter i dag)
                var checkIns = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Where(b => b.StartDate.Date == today)
                    .OrderBy(b => b.StartDate)
                    .Select(b => new
                    {
                        b.Id,
                        UserName = b.User.Username,
                        UserEmail = b.User.Email,
                        RoomNumber = b.Room.Number,
                        HotelName = b.Room.Hotel.Name,
                        CheckInTime = b.StartDate,
                        Duration = (b.EndDate - b.StartDate).Days,
                        Status = "Check-in"
                    })
                    .ToListAsync();

                // Hent check-outs (bookinger der slutter i dag)
                var checkOuts = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Where(b => b.EndDate.Date == today)
                    .OrderBy(b => b.EndDate)
                    .Select(b => new
                    {
                        b.Id,
                        UserName = b.User.Username,
                        UserEmail = b.User.Email,
                        RoomNumber = b.Room.Number,
                        HotelName = b.Room.Hotel.Name,
                        CheckOutTime = b.EndDate,
                        Duration = (b.EndDate - b.StartDate).Days,
                        Status = "Check-out"
                    })
                    .ToListAsync();

                var result = new
                {
                    Date = today.ToString("yyyy-MM-dd"),
                    CheckIns = checkIns,
                    CheckOuts = checkOuts,
                    TotalCheckIns = checkIns.Count,
                    TotalCheckOuts = checkOuts.Count,
                    HasAnyActivity = checkIns.Count > 0 || checkOuts.Count > 0,
                    Message = (checkIns.Count == 0 && checkOuts.Count == 0) 
                        ? "Ingen check-ins eller check-outs planlagt for i dag" 
                        : null
                };

                _logger.LogInformation("Hentet {CheckIns} check-ins og {CheckOuts} check-outs for {Date}", 
                    checkIns.Count, checkOuts.Count, today.ToString("yyyy-MM-dd"));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af dagens check-ins og check-outs");
                
                // Return empty data instead of error for better UX
                var emptyResult = new
                {
                    Date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd"),
                    CheckIns = new List<object>(),
                    CheckOuts = new List<object>(),
                    TotalCheckIns = 0,
                    TotalCheckOuts = 0,
                    HasAnyActivity = false,
                    Message = "Kunne ikke hente data - prøv igen senere"
                };
                
                return Ok(emptyResult);
            }
        }
    }
}
