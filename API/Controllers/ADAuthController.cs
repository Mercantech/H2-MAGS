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
    /// Controller til håndtering af autentificering - både traditionel og Active Directory
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly ActiveDirectoryService _adService;
        private readonly LoginAttemptService _loginAttemptService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initialiserer en ny instans af AuthController
        /// </summary>
        /// <param name="context">Database context til adgang til brugerdata</param>
        /// <param name="jwtService">Service til håndtering af JWT tokens</param>
        /// <param name="adService">Service til håndtering af Active Directory autentificering</param>
        /// <param name="loginAttemptService">Service til håndtering af login forsøg og rate limiting</param>
        /// <param name="logger">Logger til fejlrapportering</param>
        public AuthController(
            AppDBContext context, 
            JwtService jwtService, 
            ActiveDirectoryService adService,
            LoginAttemptService loginAttemptService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _adService = adService;
            _loginAttemptService = loginAttemptService;
            _logger = logger;
        }

        /// <summary>
        /// Active Directory login endpoint - autentificerer bruger mod AD og returnerer JWT token
        /// </summary>
        /// <param name="dto">Login credentials indeholdende username og password</param>
        /// <returns>JWT token og brugerinformation ved succesfuldt login</returns>
        /// <response code="200">Login godkendt - returnerer token og brugerinfo fra AD</response>
        /// <response code="401">Ikke autoriseret - forkert credentials eller bruger ikke fundet i AD</response>
        /// <response code="429">For mange forsøg - konto midlertidigt låst</response>
        /// <response code="500">Der opstod en intern serverfejl</response>
        [HttpPost("ad-login")]
        public async Task<IActionResult> ADLogin([FromBody] ADLoginDto dto)
        {
            try
            {
                _logger.LogInformation("AD login forsøg for bruger: {Username}", dto.Username);

                // Tjek om bruger er låst på grund af for mange mislykkede forsøg
                if (_loginAttemptService.IsLockedOut(dto.Username))
                {
                    var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(dto.Username);
                    _logger.LogWarning("AD login forsøg for låst bruger: {Username}, {RemainingSeconds} sekunder tilbage", 
                        dto.Username, remainingSeconds);
                    
                    return StatusCode(429, new { 
                        message = "Konto midlertidigt låst på grund af for mange mislykkede login forsøg.",
                        remainingLockoutSeconds = remainingSeconds
                    });
                }

                // Autentificer mod Active Directory
                var adUser = await _adService.AuthenticateUserAsync(dto.Username, dto.Password);
                
                if (adUser == null)
                {
                    _logger.LogWarning("Mislykket AD login forsøg for bruger: {Username}", dto.Username);
                    
                    // Registrer mislykket forsøg og få delay tid
                    var delaySeconds = _loginAttemptService.RecordFailedAttempt(dto.Username);
                    
                    if (delaySeconds > 0)
                    {
                        _logger.LogInformation("Påføring af {DelaySeconds} sekunders delay for bruger: {Username}", 
                            delaySeconds, dto.Username);
                        
                        // Påfør progressiv delay
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        
                        return Unauthorized(new { 
                            message = "Forkert brugernavn eller adgangskode",
                            delayApplied = delaySeconds
                        });
                    }
                    else
                    {
                        // Konto er nu låst
                        var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(dto.Username);
                        return StatusCode(429, new { 
                            message = "For mange mislykkede forsøg. Konto er nu midlertidigt låst.",
                            remainingLockoutSeconds = remainingSeconds
                        });
                    }
                }

                // Succesfuldt login - ryd fejl cache
                _loginAttemptService.RecordSuccessfulLogin(dto.Username);

                // Map AD grupper til applikationsroller
                var role = _adService.MapADGroupToRole(adUser.Groups);
                role = "Admin";

                // Generer JWT token for AD bruger
                var token = _jwtService.GenerateTokenForADUser(adUser, role);

                _logger.LogInformation("Succesfuldt AD login for bruger: {Username} med rolle: {Role}", 
                    dto.Username, role);

                return Ok(new { 
                    message = "AD Login godkendt!", 
                    token = token,
                    user = new {
                        samAccountName = adUser.SamAccountName,
                        email = adUser.Email,
                        displayName = adUser.DisplayName,
                        firstName = adUser.FirstName,
                        lastName = adUser.LastName,
                        role = role,
                        adGroups = adUser.Groups,
                        isADUser = true
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved AD login for bruger: {Username}", dto?.Username);
                return StatusCode(500, "Der opstod en intern serverfejl ved AD login");
            }
        }

        /// <summary>
        /// Henter information om den nuværende AD bruger baseret på JWT token
        /// </summary>
        /// <returns>Detaljeret AD brugerinformation inklusiv grupper og roller</returns>
        /// <response code="200">AD brugerinformation blev hentet succesfuldt</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token</response>
        /// <response code="500">Der opstod en intern serverfejl</response>
        [Authorize]
        [HttpGet("ad-me")]
        public IActionResult GetCurrentADUser()
        {
            try
            {
                // Hent claims fra JWT token
                var samAccountName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var displayName = User.FindFirst(ClaimTypes.Name)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var isADUser = User.FindFirst("adUser")?.Value == "true";
                var adGroups = User.FindFirst("adGroups")?.Value?.Split(',') ?? new string[0];

                if (samAccountName == null)
                {
                    return Unauthorized("Bruger-ID ikke fundet i token.");
                }

                _logger.LogInformation("Henter nuværende AD bruger info for: {SamAccountName}", samAccountName);

                return Ok(new
                {
                    samAccountName = samAccountName,
                    email = email,
                    displayName = displayName,
                    role = role,
                    adGroups = adGroups,
                    isADUser = isADUser,
                    loginMethod = "Active Directory"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af nuværende AD bruger");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af AD brugerinfo");
            }
        }

        /// <summary>
        /// Test endpoint til at verificere AD forbindelse og konfiguration
        /// Kun tilgængelig for administratorer
        /// </summary>
        /// <returns>AD forbindelsesstatus og konfiguration</returns>
        /// <response code="200">AD status hentet succesfuldt</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token</response>
        /// <response code="403">Forbudt - kun administratorer har adgang</response>
        /// <response code="500">Der opstod en intern serverfejl</response>
        [HttpGet("ad-status")]
        public async Task<IActionResult> GetADStatus()
        {
            try
            {
                _logger.LogInformation("Henter AD status og konfiguration");

                // Test LDAP forbindelse først
                var ldapConnectionOk = await _adService.TestLDAPConnectionAsync();
                
                // Test AD forbindelse med test credentials
                _logger.LogInformation("Tester AD forbindelse med test credentials");
                var testUser = await _adService.AuthenticateUserAsync("adReader", "Merc1234!");
                
                var status = new
                {
                    adConfigured = true,
                    server = "10.133.71.100",
                    domain = "mags.local",
                    port = 389,
                    useSSL = false,
                    ldapConnection = ldapConnectionOk,
                    testConnection = testUser != null,
                    testUser = testUser?.SamAccountName ?? "Ikke tilgængelig",
                    timestamp = DateTime.UtcNow.AddHours(2)
                };

                var statusText = status.ldapConnection && status.testConnection ? "OK" : "FEJL";
                _logger.LogInformation("AD status hentet: {Status} (LDAP: {LdapStatus}, Auth: {AuthStatus})", 
                    statusText, status.ldapConnection ? "OK" : "FEJL", status.testConnection ? "OK" : "FEJL");

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af AD status");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af AD status");
            }
        }
    }

    /// <summary>
    /// DTO til AD login
    /// </summary>
    public class ADLoginDto
    {
        /// <summary>
        /// Brugernavn (kan være sAMAccountName, email eller userPrincipalName)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Adgangskode
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
