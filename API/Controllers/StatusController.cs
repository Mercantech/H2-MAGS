using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using API.DBContext;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly HotelContext _context;

        public StatusController(HotelContext context)
        {
            _context = context;
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
    }
}
