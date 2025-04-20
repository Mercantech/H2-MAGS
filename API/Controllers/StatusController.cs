using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using API.DBContext;
using API.Services;

namespace API.Controllers
{
    /// <summary>
    /// API-controller til status-tjek af server, database og Active Directory.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly ActiveDirectoryService _adService;

        public StatusController(HotelContext context, ActiveDirectoryService adService)
        {
            _context = context;
            _adService = adService;
        }

        /// <summary>
        /// Tjekker om serveren kører.
        /// </summary>
        /// <returns>En tekstbesked om serverstatus.</returns>
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok("The server is Live");
        }

        /// <summary>
        /// Tjekker om serveren og databasen er tilgængelige.
        /// </summary>
        /// <returns>En tekstbesked om database- og serverstatus.</returns>
        /// <response code="200">Database og server er tilgængelige.</response>
        /// <response code="404">Database er ikke tilgængelig.</response>
        [HttpGet("DB")]
        public IActionResult GetStatusDB()
        {
            if (_context.Database.CanConnect())
            {
                return Ok("The database and Server is Live!");
            }
            else return NotFound();
        }

        /// <summary>
        /// Tjekker om Active Directory-forbindelsen virker.
        /// </summary>
        /// <returns>Et objekt med status for AD-forbindelsen.</returns>
        /// <response code="200">AD-forbindelsen virker.</response>
        /// <response code="500">Fejl ved kontrol af AD-forbindelse.</response>
        [HttpGet("AD")]
        public IActionResult GetStatusAD()
        {
            try
            {
                bool isConnected = _adService.ValidateUser("dummy", "ToTest1234!");
                return Ok(new { Connected = isConnected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fejl ved kontrol af AD-forbindelse: {ex.Message}");
            }
        }
    }
}
