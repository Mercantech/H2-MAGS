using Microsoft.AspNetCore.Mvc;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Controller til håndtering af systemstatus og sundhedstjek.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialiserer en ny instans af StatusController.
        /// </summary>
        /// <param name="context">Database kontekst til adgang til data.</param>
        public StatusController(AppDBContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Tjekker om API'en kører korrekt.
        /// </summary>
        /// <returns>Status og besked om API'ens tilstand.</returns>
        /// <response code="200">API'en er kørende.</response>
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "OK", message = "API'en er kørende!" });
        }

        /// <summary>
        /// Tjekker om databasen er tilgængelig (dummy indtil EFCore er sat op).
        /// </summary>
        /// <returns>Status og besked om databaseforbindelse.</returns>
        /// <response code="200">Database er kørende eller fejlbesked gives.</response>
    
        [HttpGet("dbhealthcheck")]
        public async Task<IActionResult> DBHealthCheck()
        {
            try 
            {
                // Tjek om vi kan forbinde til databasen
                await _context.Database.CanConnectAsync();
                return Ok(new { status = "OK", message = "Database er kørende!" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "Error", message = "Fejl ved forbindelse til database: " + ex.Message });
            }
        }

        /// <summary>
        /// Simpelt ping-endpoint til at teste API'en.
        /// </summary>
        /// <returns>Status og "Pong" besked.</returns>
        /// <response code="200">API'en svarede med Pong.</response>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }

        /// <summary>
        /// Henter antal rækker i alle hovedtabeller.
        /// </summary>
        /// <returns>Antal rækker i Hotels, Users, Rooms og Bookings tabellerne.</returns>
        /// <response code="200">Returnerer antal rækker i hver tabel.</response>
        [HttpGet("tablecount")]
        public async Task<IActionResult> GetTableCount()
        {
            try
            {
                var hotelCount = await _context.Hotels.CountAsync();
                var userCount = await _context.Users.CountAsync();
                var roomCount = await _context.Rooms.CountAsync();
                var bookingCount = await _context.Bookings.CountAsync();

                return Ok(new 
                { 
                    status = "OK",
                    hotels = hotelCount,
                    users = userCount,
                    rooms = roomCount,
                    bookings = bookingCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    status = "Error", 
                    message = "Fejl ved hentning af tabel antal: " + ex.Message 
                });
            }
        }
    }
}
