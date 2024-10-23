using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DBContext;
using API.Services.Mapping.Users;
using DomainModels;
using DomainModels.DTOs.Users;
using Microsoft.CodeAnalysis.Scripting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly ActiveDirectoryService _adService;
        private readonly JWTService _jwtService;
        private readonly SignupService _signupService;
        public UsersController(HotelContext context, ActiveDirectoryService adService, JWTService jwtService, SignupService signupService)
        {
            _context = context;
            _adService = adService;
            _jwtService = jwtService;
            _signupService = signupService;

        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
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

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // PUT: api/Users/5/ResetPassword
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

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { user.Id, user.Email });
        }

        // DELETE: api/Users/5
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

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.HashedPassword))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = _jwtService.GenerateJwtToken(user);
            return Ok(new { token });
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


            return Ok(new
            {
                token,
                groups 
            });
        }


    }
}
