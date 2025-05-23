﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using API.DBContext;
using API.Services.Mapping.Users;
using DomainModels;
using DomainModels.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace API.Controllers
{
    /// <summary>
    /// API-controller til håndtering af brugere.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly ActiveDirectoryService _adService;
        private readonly JWTService _jwtService;
        private readonly SignupService _signupService;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public UsersController(
            HotelContext context,
            ActiveDirectoryService adService,
            JWTService jwtService,
            SignupService signupService,
            EmailService emailService,
            IConfiguration configuration
        )
        {
            _context = context;
            _adService = adService;
            _jwtService = jwtService;
            _signupService = signupService;
            _emailService = emailService;
            _configuration = configuration;
        }

        /// <summary>
        /// Henter alle brugere.
        /// </summary>
        /// <returns>En liste af brugere.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Henter en specifik bruger ud fra ID.
        /// </summary>
        /// <param name="id">Brugerens unikke ID.</param>
        /// <returns>Brugerens detaljer.</returns>
        /// <response code="404">Bruger ikke fundet.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Opdaterer en eksisterende bruger.
        /// </summary>
        /// <param name="id">Brugerens unikke ID.</param>
        /// <param name="updateUserDto">Objekt med opdaterede brugerdata.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="400">ID matcher ikke eller ugyldig e-mail.</response>
        /// <response code="404">Bruger ikke fundet.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UpdateUserDTO updateUserDto)
        {
            if (!_signupService.IsValidEmail(updateUserDto.Email))
            {
                return BadRequest(new { message = "Ugyldig e-mailadresse." });
            }

            if (id != updateUserDto.Id)
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = updateUserDto.Email;
            user.Name = updateUserDto.Name;
            // Andre felter, der skal opdateres

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        /// <summary>
        /// Nulstiller adgangskoden for en bruger.
        /// </summary>
        /// <param name="id">Brugerens unikke ID.</param>
        /// <param name="resetPasswordDto">Objekt med ny adgangskode.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="400">Adgangskoden er ikke sikker nok.</response>
        /// <response code="404">Bruger ikke fundet.</response>
        [HttpPut("{id}/ResetPassword")]
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordDTO resetPasswordDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!_signupService.IsPasswordSecure(resetPasswordDto.NewPassword))
            {
                return BadRequest(new { message = "Adgangskoden er ikke sikker nok." });
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            user.HashedPassword = hashedPassword;
            user.Salt = hashedPassword.Substring(0, 29);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        /// <summary>
        /// Opretter en ny bruger (registrering).
        /// </summary>
        /// <param name="userSignUp">Objekt med brugerdata til oprettelse.</param>
        /// <returns>Resultat af oprettelsen.</returns>
        /// <response code="400">Ugyldig e-mailadresse.</response>
        /// <response code="409">E-mailadressen er allerede i brug eller adgangskoden er ikke sikker nok.</response>
        [HttpPost("register")]
        public async Task<IActionResult> PostUser(SignUp userSignUp)
        {
            if (!_signupService.IsValidEmail(userSignUp.Email))
            {
                return BadRequest(new { message = "Ugyldig e-mailadresse." });
            }

            if (!_signupService.IsPasswordSecure(userSignUp.Password))
            {
                return Conflict(new { message = "Adgangskoden er ikke sikker nok." });
            }

            if (await _context.Users.AnyAsync(u => u.Email == userSignUp.Email))
            {
                return Conflict(new { message = "E-mailadressen er allerede i brug." });
            }

            var user = _signupService.MapSignUpDTOToUser(userSignUp);

            user.EmailConfirmationToken = Guid.NewGuid().ToString();
            user.IsEmailConfirmed = false;

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();

                await _emailService.SendConfirmationEmail(user.Email, user.EmailConfirmationToken);

                return Ok(
                    new
                    {
                        user.Id,
                        user.Email,
                        message = "Bruger oprettet. Tjek venligst din email for at bekræfte din konto."
                    }
                );
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return Conflict();
                }
                throw;
            }
        }

        /// <summary>
        /// Bekræfter en brugers e-mail via token.
        /// </summary>
        /// <param name="token">Bekræftelsestoken sendt til brugerens e-mail.</param>
        /// <param name="email">Brugerens e-mailadresse.</param>
        /// <returns>Redirect til bekræftelsesside.</returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string token,
            [FromQuery] string email
        )
        {
            var user = await _context.Users.SingleOrDefaultAsync(u =>
                u.Email == email && u.EmailConfirmationToken == token
            );

            var baseUrl =
                Environment.GetEnvironmentVariable("APPLICATION_BASE_URL")
                ?? _configuration["Application:BaseUrl"];

            if (user == null)
            {
                return Redirect($"https://{baseUrl}/email-confirmation?status=error");
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();

            return Redirect($"https://{baseUrl}/email-confirmation?status=success");
        }

        /// <summary>
        /// Henter alle gæster (brugere, der ikke er Google-brugere).
        /// </summary>
        /// <returns>En liste af gæster.</returns>
        [HttpGet("guests")]
        public async Task<ActionResult<IEnumerable<GuestDTO>>> GetGuests()
        {
            var guests = await _context.Users
                .Where(u => !u.IsGoogleUser) // Antager at normale brugere er gæster
                .Select(u => new GuestDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    LastLogin = u.LastLogin,
                    TotalBookings = u.Bookings.Count,
                    IsEmailConfirmed = u.IsEmailConfirmed
                })
                .ToListAsync();

            return Ok(guests);
        }

        /// <summary>
        /// Sletter en bruger ud fra ID.
        /// </summary>
        /// <param name="id">Brugerens unikke ID.</param>
        /// <returns>NoContent ved succes, ellers fejlbesked.</returns>
        /// <response code="404">Bruger ikke fundet.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        /// <summary>
        /// Logger en bruger ind.
        /// </summary>
        /// <param name="login">Loginoplysninger (e-mail og adgangskode).</param>
        /// <returns>JWT access og refresh tokens.</returns>
        /// <response code="401">Ugyldig email eller adgangskode, eller email ikke bekræftet.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.HashedPassword))
            {
                return Unauthorized(new { message = "Ugyldig email eller adgangskode." });
            }

            if (!user.IsEmailConfirmed)
            {
                return Unauthorized(
                    new
                    {
                        message = "Email er ikke bekræftet. Tjek venligst din email for bekræftelses-link."
                    }
                );
            }

            var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);

            return Ok(
                new
                {
                    accessToken,
                    refreshToken,
                    expiresIn = 30
                }
            );
        }

        /// <summary>
        /// Fornyer JWT access token ved brug af refresh token.
        /// </summary>
        /// <param name="request">Objekt med nuværende access og refresh token.</param>
        /// <returns>Nyt access og refresh token.</returns>
        /// <response code="400">Ugyldigt token.</response>
        /// <response code="401">Ugyldigt refresh token.</response>
        /// <response code="404">Bruger ikke fundet.</response>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(request.AccessToken) as JwtSecurityToken;
                var userId = jsonToken
                    ?.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub)
                    .Value;

                if (userId == null)
                {
                    return BadRequest(new { message = "Invalid token format" });
                }

                var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (!_jwtService.ValidateRefreshToken(request.RefreshToken))
                {
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                var (accessToken, newRefreshToken) = _jwtService.GenerateTokens(user);

                return Ok(
                    new
                    {
                        accessToken,
                        refreshToken = newRefreshToken,
                        expiresIn = 30
                    }
                );
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Invalid token" });
            }
        }
    }
}
