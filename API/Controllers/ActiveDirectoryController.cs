using Microsoft.AspNetCore.Mvc;
using API.Services;
using DomainModels.DTOs.Users;
using API.Services.Mapping.Users;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActiveDirectoryController : ControllerBase
    {
        private readonly ActiveDirectoryService _adService;
        private readonly JWTService _jwtService;

        public ActiveDirectoryController(ActiveDirectoryService adService, JWTService jwtService)
        {
            _adService = adService;
            _jwtService = jwtService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                // Kontroller forbindelsen til Active Directory ved at validere en dummy-bruger
                bool isConnected = _adService.ValidateUser("dummy", "ToTest1234!");
                return Ok(new { Connected = isConnected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fejl ved kontrol af AD-forbindelse: {ex.Message}");
            }
        }
        // POST: api/Users/loginAD
        [HttpPost("loginAD")]
        public async Task<IActionResult> LoginOnAD([FromBody] Login login)
        {
            // Forsøg at validere brugeren mod AD
            bool isValidUser = _adService.ValidateUser(login.Email, login.Password);

            if (!isValidUser)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Hent brugerens grupper fra AD
            var groups = _adService.GetGroups(login.Email);

            var token = _jwtService.GenerateJwtTokenAD(login.Email, login.Email);

            return Ok(new { token, groups });
        }
    }
} 