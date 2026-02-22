using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller til håndtering af database seeding med test data.
    /// Kun tilgængelig for administratorer og kun i development miljø.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DataSeederController : ControllerBase
    {
        private readonly DataSeederService _seederService;
        private readonly ILogger<DataSeederController> _logger;
        private readonly IWebHostEnvironment _environment;

        public DataSeederController(
            DataSeederService seederService, 
            ILogger<DataSeederController> logger,
            IWebHostEnvironment environment)
        {
            _seederService = seederService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Seeder databasen med test data. Kun tilgængelig i development miljø.
        /// </summary>
        /// <param name="userCount">Antal brugere at oprette (standard: 50)</param>
        /// <param name="hotelCount">Antal hoteller at oprette (standard: 10)</param>
        /// <param name="roomsPerHotel">Antal rum per hotel (standard: 20)</param>
        /// <param name="bookingCount">Antal bookinger at oprette (standard: 100)</param>
        /// <returns>Seeding resultat og statistikker.</returns>
        /// <response code="200">Database seeding fuldført succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel eller ikke development miljø.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPost("seed")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<object>> SeedDatabase(
            [FromQuery] int userCount = 50,
            [FromQuery] int hotelCount = 10,
            [FromQuery] int roomsPerHotel = 20,
            [FromQuery] int bookingCount = 100)
        {
            // Kun tillad seeding i development miljø
            if (!_environment.IsDevelopment())
            {
                return BadRequest("Database seeding er kun tilladt i development miljø");
            }

            try
            {
                _logger.LogInformation("Starter database seeding med parametre: Users={UserCount}, Hotels={HotelCount}, RoomsPerHotel={RoomsPerHotel}, Bookings={BookingCount}",
                    userCount, hotelCount, roomsPerHotel, bookingCount);

                var result = await _seederService.SeedDatabaseAsync(userCount, hotelCount, roomsPerHotel, bookingCount);
                var stats = await _seederService.GetDatabaseStatsAsync();

                return Ok(new
                {
                    Message = "Database seeding fuldført succesfuldt",
                    Details = result,
                    Statistics = stats,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under database seeding");
                return StatusCode(500, "Der opstod en fejl under database seeding");
            }
        }

        /// <summary>
        /// Rydder alle data fra databasen. Kun tilgængelig i development miljø.
        /// </summary>
        /// <returns>Rydning resultat.</returns>
        /// <response code="200">Database ryddet succesfuldt.</response>
        /// <response code="400">Ikke development miljø.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpDelete("clear")]
        public async Task<ActionResult<object>> ClearDatabase()
        {
            // Kun tillad rydning i development miljø
            if (!_environment.IsDevelopment())
            {
                return BadRequest("Database rydning er kun tilladt i development miljø");
            }

            try
            {
                _logger.LogWarning("Starter database rydning - ALLE DATA SLETTES!");

                var result = await _seederService.ClearDatabaseAsync();

                return Ok(new
                {
                    Message = "Database ryddet succesfuldt",
                    Details = result,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under database rydning");
                return StatusCode(500, "Der opstod en fejl under database rydning");
            }
        }

        /// <summary>
        /// Henter database statistikker.
        /// </summary>
        /// <returns>Aktuelle database statistikker.</returns>
        /// <response code="200">Statistikker hentet succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetDatabaseStats()
        {
            try
            {
                var stats = await _seederService.GetDatabaseStatsAsync();

                return Ok(new
                {
                    Message = "Database statistikker hentet succesfuldt",
                    Statistics = stats,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af database statistikker");
                return StatusCode(500, "Der opstod en fejl ved hentning af statistikker");
            }
        }

        /// <summary>
        /// Seeder kun brugere med test data.
        /// </summary>
        /// <param name="count">Antal brugere at oprette</param>
        /// <returns>Seeding resultat.</returns>
        [HttpPost("seed-users")]
        public async Task<ActionResult<object>> SeedUsersOnly([FromQuery] int count = 25)
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("Database seeding er kun tilladt i development miljø");
            }

            try
            {
                var result = await _seederService.SeedDatabaseAsync(userCount: count, hotelCount: 0, roomsPerHotel: 0, bookingCount: 0);
                var stats = await _seederService.GetDatabaseStatsAsync();

                return Ok(new
                {
                    Message = "Bruger seeding fuldført succesfuldt",
                    Details = result,
                    Statistics = stats,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under bruger seeding");
                return StatusCode(500, "Der opstod en fejl under bruger seeding");
            }
        }

        /// <summary>
        /// Seeder kun hoteller og rum med test data.
        /// </summary>
        /// <param name="hotelCount">Antal hoteller at oprette</param>
        /// <param name="roomsPerHotel">Antal rum per hotel</param>
        /// <returns>Seeding resultat.</returns>
        [HttpPost("seed-hotels")]
        public async Task<ActionResult<object>> SeedHotelsOnly(
            [FromQuery] int hotelCount = 5,
            [FromQuery] int roomsPerHotel = 15)
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("Database seeding er kun tilladt i development miljø");
            }

            try
            {
                var result = await _seederService.SeedDatabaseAsync(userCount: 0, hotelCount: hotelCount, roomsPerHotel: roomsPerHotel, bookingCount: 0);
                var stats = await _seederService.GetDatabaseStatsAsync();

                return Ok(new
                {
                    Message = "Hotel og rum seeding fuldført succesfuldt",
                    Details = result,
                    Statistics = stats,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under hotel seeding");
                return StatusCode(500, "Der opstod en fejl under hotel seeding");
            }
        }

        /// <summary>
        /// Seeder kun bookinger baseret på eksisterende brugere og rum.
        /// </summary>
        /// <param name="bookingCount">Antal bookinger at oprette</param>
        /// <returns>Seeding resultat.</returns>
        /// <response code="200">Booking seeding fuldført succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel eller ikke development miljø.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPost("seed-bookings")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<object>> SeedBookingsOnly([FromQuery] int bookingCount = 50)
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("Database seeding er kun tilladt i development miljø");
            }

            try
            {
                _logger.LogInformation("Starter booking-only seeding med {BookingCount} bookinger", bookingCount);

                var result = await _seederService.SeedBookingsOnlyAsync(bookingCount);
                var stats = await _seederService.GetDatabaseStatsAsync();

                return Ok(new
                {
                    Message = "Booking seeding fuldført succesfuldt",
                    Details = result,
                    Statistics = stats,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under booking seeding");
                return StatusCode(500, "Der opstod en fejl under booking seeding");
            }
        }

        /// <summary>
        /// Opretter demo data specifikt for demo brugeren.
        /// </summary>
        /// <returns>Demo data oprettelse resultat.</returns>
        [HttpPost("seed-demo")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<ActionResult<object>> SeedDemoData()
        {
            try
            {
                _logger.LogInformation("Starter demo data seeding");

                // Opret demo bruger hvis den ikke eksisterer
                await _seederService.EnsureDemoUserExistsAsync();

                // Seed basic data hvis det ikke eksisterer
                var stats = await _seederService.GetDatabaseStatsAsync();
                if (stats.Users == 0 || stats.Hotels == 0 || stats.Rooms == 0)
                {
                    await _seederService.SeedDatabaseAsync(userCount: 10, hotelCount: 3, roomsPerHotel: 10, bookingCount: 20);
                }

                // Opret specifikke bookinger for demo brugeren
                await _seederService.SeedDemoUserBookingsAsync();

                var finalStats = await _seederService.GetDatabaseStatsAsync();

                return Ok(new
                {
                    Message = "Demo data oprettet succesfuldt",
                    Statistics = finalStats,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl under demo data seeding");
                return StatusCode(500, "Der opstod en fejl under demo data seeding");
            }
        }
    }
}
