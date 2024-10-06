using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DBContext;
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
        public UsersController(HotelContext context, ActiveDirectoryService adService)
        {
            _context = context;
            _adService = adService;
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
        public async Task<IActionResult> PutUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
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

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
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

            var token = "Login virker!";//GenerateJwtToken(user);
            return Ok(new { token });
        }

        // POST: api/Users/loginAD
        [HttpPost("loginAD")]
        public async Task<IActionResult> LoginOnAD([FromBody] Login login)
        {
            Console.WriteLine("Test1");

            // Forsøg at validere brugeren mod AD
            bool isValidUser = _adService.ValidateUser(login.Email, login.Password);

            if (!isValidUser)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Hent brugerens grupper fra AD
            var groups = _adService.GetGroups(login.Email);

            var token = "Login virker!"; // Testbesked i stedet for en JWT-token

            // Returner en succesbesked sammen med brugerens grupper
            return Ok(new
            {
                token,
                groups // Returnerer grupper som en liste
            });
        }


    }
}
