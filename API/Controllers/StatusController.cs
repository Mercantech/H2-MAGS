using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using API.DBContext;
using API.Services;

namespace API.Controllers
{
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
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok("The server is Live");
        }
        [HttpGet("DB")]
        public IActionResult GetStatusDB()
        {
            if (_context.Database.CanConnect())
            {
                return Ok("The database and Server is Live!");
            }
            else return NotFound();
        }
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
