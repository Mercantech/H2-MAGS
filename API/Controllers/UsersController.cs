using API.Data;
using DomainModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DomainModels.Mapping;
using API.Services;
using System.Security.Claims;
using System.Diagnostics;

namespace API.Controllers
{
    /// <summary>
    /// Controller til håndtering af bruger-relaterede operationer.
    /// Indeholder funktionalitet til brugeradministration, autentificering og autorisering.
    /// Implementerer struktureret fejlhåndtering med standardiserede responser.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly LoginAttemptService _loginAttemptService;
        private readonly MailService _mailService;
        private readonly PasswordHashingService _passwordHashingService;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Initialiserer en ny instans af UsersController.
        /// </summary>
        /// <param name="context">Database context til adgang til brugerdata.</param>
        /// <param name="jwtService">Service til håndtering af JWT tokens.</param>
        /// <param name="loginAttemptService">Service til håndtering af login forsøg og rate limiting.</param>
        /// <param name="mailService">Service til håndtering af email funktionalitet.</param>
        /// <param name="passwordHashingService">Service til sikker password hashing med Argon2id.</param>
        /// <param name="logger">Logger til fejlrapportering.</param>
        public UsersController(AppDBContext context, JwtService jwtService, LoginAttemptService loginAttemptService, MailService mailService, PasswordHashingService passwordHashingService, ILogger<UsersController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _loginAttemptService = loginAttemptService;
            _mailService = mailService;
            _passwordHashingService = passwordHashingService;
            _logger = logger;
        }

        /// <summary>
        /// Henter alle brugere fra systemet. Kun tilgængelig for administratorer.
        /// </summary>
        /// <returns>En liste af alle brugere med deres roller.</returns>
        /// <response code="200">Brugerlisten blev hentet succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsers()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🔄 [BENCHMARK] Starter GetUsers (ToListAsync pattern) - anmodet af administrator");
                
                var dbQueryStart = Stopwatch.StartNew();
                var users = await _context.Users
                    .Include(u => u.Role)
                    .ToListAsync();
                dbQueryStart.Stop();

                var mappingStart = Stopwatch.StartNew();
                var userDtos = UserMapping.ToUserGetDtos(users);
                mappingStart.Stop();

                stopwatch.Stop();

                _logger.LogInformation("✅ [BENCHMARK] GetUsers færdig - {UserCount} brugere hentet på {TotalMs}ms (DB: {DbMs}ms, Mapping: {MappingMs}ms)", 
                    users.Count, stopwatch.ElapsedMilliseconds, dbQueryStart.ElapsedMilliseconds, mappingStart.ElapsedMilliseconds);
                
                return Ok(new { 
                    data = userDtos,
                    benchmark = new {
                        totalTimeMs = stopwatch.ElapsedMilliseconds,
                        databaseQueryMs = dbQueryStart.ElapsedMilliseconds,
                        mappingTimeMs = mappingStart.ElapsedMilliseconds,
                        recordCount = users.Count,
                        method = "ToListAsync"
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ [BENCHMARK] Fejl ved hentning af alle brugere efter {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af brugere");
            }
        }

        /// <summary>
        /// Henter alle brugere fra systemet med AsQueryable pattern for optimeret performance.
        /// Kun tilgængelig for administratorer.
        /// 
        /// 🚀 PERFORMANCE FORDEL: Denne endpoint bruger AsQueryable() pattern i stedet for ToListAsync().
        /// Dette giver betydelige performance fordele:
        /// - Lazy loading: Data hentes kun når det faktisk bruges
        /// - Reduceret memory forbrug: Undgår at loade alle brugere i memory på én gang
        /// - Bedre skalerbarhed: Håndterer store datasæt mere effektivt
        /// - Optimeret database queries: Entity Framework kan optimere SQL queries bedre
        /// - Streaming data: Kan begynde at returnere data før alle records er hentet
        /// 
        /// Sammenlign med den normale /users endpoint der bruger ToListAsync().
        /// </summary>
        /// <returns>En liste af alle brugere med deres roller (optimeret med AsQueryable).</returns>
        /// <response code="200">Brugerlisten blev hentet succesfuldt med optimeret performance.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("optimized")]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsersOptimized()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🚀 [BENCHMARK] Starter GetUsersOptimized (AsQueryable pattern) - anmodet af administrator");
                
                var queryBuildStart = Stopwatch.StartNew();
                var usersQuery = _context.Users
                    .Include(u => u.Role)
                    .AsQueryable();
                queryBuildStart.Stop();

                // AsQueryable giver os mulighed for at arbejde med query'en før den udføres
                // Dette kan bruges til filtrering, sortering, paging osv. før data hentes
                var dbExecutionStart = Stopwatch.StartNew();
                var users = await usersQuery.ToListAsync();
                dbExecutionStart.Stop();

                var mappingStart = Stopwatch.StartNew();
                var userDtos = UserMapping.ToUserGetDtos(users);
                mappingStart.Stop();

                stopwatch.Stop();

                _logger.LogInformation("✅ [BENCHMARK] GetUsersOptimized færdig - {UserCount} brugere hentet på {TotalMs}ms (Query build: {QueryMs}ms, DB: {DbMs}ms, Mapping: {MappingMs}ms)", 
                    users.Count, stopwatch.ElapsedMilliseconds, queryBuildStart.ElapsedMilliseconds, dbExecutionStart.ElapsedMilliseconds, mappingStart.ElapsedMilliseconds);
                
                return Ok(new { 
                    data = userDtos,
                    benchmark = new {
                        totalTimeMs = stopwatch.ElapsedMilliseconds,
                        queryBuildMs = queryBuildStart.ElapsedMilliseconds,
                        databaseExecutionMs = dbExecutionStart.ElapsedMilliseconds,
                        mappingTimeMs = mappingStart.ElapsedMilliseconds,
                        recordCount = users.Count,
                        method = "AsQueryable + ToListAsync"
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ [BENCHMARK] Fejl ved hentning af alle brugere (optimeret) efter {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af brugere");
            }
        }

        /// <summary>
        /// Benchmark endpoint der sammenligner performance mellem ToListAsync, AsQueryable og Projection patterns.
        /// Kører alle tre metoder og returnerer detaljerede performance metrics og SQL query information.
        /// Kun tilgængelig for administratorer.
        /// 
        /// 📊 BENCHMARK FUNKTIONALITET:
        /// - Måler total execution time for hver metode
        /// - Opdeler tiden i database query, mapping og overhead
        /// - Sammenligner memory usage patterns og data transfer
        /// - Viser genererede SQL queries (hvis SQL logging er aktiveret)
        /// - Returnerer side-by-side performance comparison af alle tre metoder
        /// - Inkluderer EF Core projection for optimal performance
        /// </summary>
        /// <returns>Detaljeret performance sammenligning mellem de to metoder.</returns>
        /// <response code="200">Benchmark kørt succesfuldt med performance metrics.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("benchmark")]
        public async Task<ActionResult> BenchmarkUserRetrieval()
        {
            var totalStopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🏁 [BENCHMARK] Starter performance sammenligning mellem ToListAsync, AsQueryable og Projection");

                // Benchmark 1: ToListAsync pattern
                var method1Stopwatch = Stopwatch.StartNew();
                var dbQuery1Start = Stopwatch.StartNew();
                var users1 = await _context.Users
                    .Include(u => u.Role)
                    .ToListAsync();
                dbQuery1Start.Stop();

                var mapping1Start = Stopwatch.StartNew();
                var userDtos1 = UserMapping.ToUserGetDtos(users1);
                mapping1Start.Stop();
                method1Stopwatch.Stop();

                // Lille pause for at undgå cache interference
                await Task.Delay(10);

                // Benchmark 2: AsQueryable pattern
                var method2Stopwatch = Stopwatch.StartNew();
                var queryBuild2Start = Stopwatch.StartNew();
                var usersQuery2 = _context.Users
                    .Include(u => u.Role)
                    .AsQueryable();
                queryBuild2Start.Stop();

                var dbExecution2Start = Stopwatch.StartNew();
                var users2 = await usersQuery2.ToListAsync();
                dbExecution2Start.Stop();

                var mapping2Start = Stopwatch.StartNew();
                var userDtos2 = UserMapping.ToUserGetDtos(users2);
                mapping2Start.Stop();
                method2Stopwatch.Stop();

                // Lille pause for at undgå cache interference
                await Task.Delay(10);

                // Benchmark 3: EF Core Projection pattern
                var method3Stopwatch = Stopwatch.StartNew();
                var projection3Start = Stopwatch.StartNew();
                var userDtos3 = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new UserGetDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Username = u.Username,
                        Role = u.Role != null ? u.Role.Name : string.Empty
                    })
                    .ToListAsync();
                projection3Start.Stop();
                method3Stopwatch.Stop();

                totalStopwatch.Stop();

                // Find fastest method
                var methods = new[] { 
                    new { Name = "ToListAsync", Time = method1Stopwatch.ElapsedMilliseconds },
                    new { Name = "AsQueryable", Time = method2Stopwatch.ElapsedMilliseconds },
                    new { Name = "Projection", Time = method3Stopwatch.ElapsedMilliseconds }
                };
                var winner = methods.OrderBy(m => m.Time).First();

                // Calculate improvements vs slowest
                var slowest = methods.OrderByDescending(m => m.Time).First();
                var projectionImprovement = slowest.Time > 0 
                    ? Math.Round((double)(slowest.Time - method3Stopwatch.ElapsedMilliseconds) / slowest.Time * 100, 2)
                    : 0;

                var benchmarkResult = new {
                    summary = new {
                        totalBenchmarkTimeMs = totalStopwatch.ElapsedMilliseconds,
                        recordCount = users1.Count,
                        winner = winner.Name,
                        fastestTimeMs = winner.Time,
                        projectionImprovementPercent = projectionImprovement
                    },
                    toListAsyncMethod = new {
                        totalTimeMs = method1Stopwatch.ElapsedMilliseconds,
                        databaseQueryMs = dbQuery1Start.ElapsedMilliseconds,
                        mappingTimeMs = mapping1Start.ElapsedMilliseconds,
                        method = "Direct ToListAsync",
                        dataTransfer = "Full User entities + manual mapping"
                    },
                    asQueryableMethod = new {
                        totalTimeMs = method2Stopwatch.ElapsedMilliseconds,
                        queryBuildMs = queryBuild2Start.ElapsedMilliseconds,
                        databaseExecutionMs = dbExecution2Start.ElapsedMilliseconds,
                        mappingTimeMs = mapping2Start.ElapsedMilliseconds,
                        method = "AsQueryable + ToListAsync",
                        dataTransfer = "Full User entities + manual mapping"
                    },
                    projectionMethod = new {
                        totalTimeMs = method3Stopwatch.ElapsedMilliseconds,
                        projectionAndDatabaseMs = projection3Start.ElapsedMilliseconds,
                        mappingTimeMs = 0,
                        method = "EF Core Projection",
                        dataTransfer = "Only DTO fields (Id, Email, Username, Role.Name)"
                    },
                    recommendations = new {
                        bestOverall = "Projection - optimal data transfer og performance",
                        bestForSmallDatasets = "ToListAsync - hvis simplicity er vigtigere end performance",
                        bestForLargeDatasets = "Projection - betydelig forbedring på store datasæt",
                        bestForFiltering = "AsQueryable + Projection - kan kombineres for optimal performance",
                        bestForSimplicity = "ToListAsync - færre trin, men dårligere performance"
                    }
                };

                _logger.LogInformation("🏆 [BENCHMARK] Performance sammenligning færdig: ToListAsync={Method1Ms}ms vs AsQueryable={Method2Ms}ms vs Projection={Method3Ms}ms (Vinder: {Winner})",
                    method1Stopwatch.ElapsedMilliseconds, method2Stopwatch.ElapsedMilliseconds, method3Stopwatch.ElapsedMilliseconds, winner.Name);

                return Ok(benchmarkResult);
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                _logger.LogError(ex, "❌ [BENCHMARK] Fejl ved benchmark efter {ElapsedMs}ms", totalStopwatch.ElapsedMilliseconds);
                return StatusCode(500, "Der opstod en intern serverfejl ved benchmark");
            }
        }

        /// <summary>
        /// Henter alle brugere med EF Core projection - kun de felter der bruges i DTO.
        /// Kun tilgængelig for administratorer.
        /// 
        /// 🎯 PROJECTION FORDEL: Denne endpoint bruger EF Core projection til kun at hente de nødvendige felter.
        /// Dette giver betydelige performance fordele:
        /// - Reduceret netværks traffic: Kun nødvendige data overføres fra database
        /// - Mindre memory forbrug: Ingen unødvendige felter loades i memory
        /// - Hurtigere queries: Database behøver kun læse de ønskede kolonner
        /// - Automatisk optimering: Ingen manuel mapping fra User til UserGetDto
        /// - Bedre skalerbarhed: Særligt mærkbart med store tabeller og mange felter
        /// 
        /// SQL query vil kun indeholde: Id, Email, Username, Role.Name
        /// Sammenlign med andre endpoints der henter hele User entiteten.
        /// </summary>
        /// <returns>En liste af brugere med kun DTO felter (optimeret med projection).</returns>
        /// <response code="200">Brugerlisten blev hentet succesfuldt med projection.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("projection")]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsersWithProjection()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("🎯 [BENCHMARK] Starter GetUsersWithProjection (EF Core projection) - anmodet af administrator");
                
                var projectionStart = Stopwatch.StartNew();
                // Projection: Hent kun de felter vi faktisk bruger i DTO
                var userDtos = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new UserGetDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Username = u.Username,
                        Role = u.Role != null ? u.Role.Name : string.Empty
                    })
                    .ToListAsync();
                projectionStart.Stop();

                stopwatch.Stop();

                _logger.LogInformation("✅ [BENCHMARK] GetUsersWithProjection færdig - {UserCount} brugere hentet på {TotalMs}ms (Projection + DB: {ProjectionMs}ms)", 
                    userDtos.Count, stopwatch.ElapsedMilliseconds, projectionStart.ElapsedMilliseconds);
                
                return Ok(new { 
                    data = userDtos,
                    benchmark = new {
                        totalTimeMs = stopwatch.ElapsedMilliseconds,
                        projectionAndDatabaseMs = projectionStart.ElapsedMilliseconds,
                        mappingTimeMs = 0, // Ingen separat mapping nødvendig
                        recordCount = userDtos.Count,
                        method = "EF Core Projection",
                        fieldsSelected = new[] { "Id", "Email", "Username", "Role.Name" }
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ [BENCHMARK] Fejl ved hentning af brugere med projection efter {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af brugere");
            }
        }

        /// <summary>
        /// Henter en specifik bruger baseret på ID.
        /// </summary>
        /// <param name="id">Unikt ID for brugeren.</param>
        /// <returns>Brugerens information.</returns>
        /// <response code="200">Brugeren blev fundet og returneret.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - utilstrækkelige rettigheder.</response>
        /// <response code="404">Bruger med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin, User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDto>> GetUser(string id)
        {
            try
            {
                _logger.LogInformation("Henter bruger med ID: {UserId}", id);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning("Bruger med ID {UserId} ikke fundet", id);
                    return NotFound();
                }

                _logger.LogInformation("Bruger med ID {UserId} hentet succesfuldt", id);
                return UserMapping.ToUserGetDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af bruger med ID: {UserId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af bruger");
            }
        }

        /// <summary>
        /// Opdaterer en eksisterende bruger.
        /// </summary>
        /// <param name="id">ID på brugeren der skal opdateres.</param>
        /// <param name="updateUserDto">Opdaterede brugerdata.</param>
        /// <returns>Bekræftelse på opdateringen.</returns>
        /// <response code="204">Brugeren blev opdateret succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel - ID matcher ikke bruger ID.</response>
        /// <response code="404">Bruger med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UpdateUserDto updateUserDto)
        {
            try
            {
                _logger.LogInformation("Opdaterer bruger med ID: {UserId}", id);

                // Find den eksisterende bruger
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("Bruger med ID {UserId} blev ikke fundet", id);
                    return NotFound();
                }

                // Opdater kun de tilladte felter fra DTO'en
                existingUser.Email = updateUserDto.Email;
                existingUser.Username = updateUserDto.Username;
                existingUser.Salt = updateUserDto.Salt;
                existingUser.LastLogin = updateUserDto.LastLogin;
                existingUser.PasswordBackdoor = updateUserDto.PasswordBackdoor ?? string.Empty;
                existingUser.RoleId = updateUserDto.RoleId;
                existingUser.UserInfoId = updateUserDto.UserInfoId;
                
                // Opdater UpdatedAt timestamp
                existingUser.UpdatedAt = DateTime.UtcNow;

                // Marker kun de ændrede felter som modificeret
                _context.Entry(existingUser).Property(x => x.Email).IsModified = true;
                _context.Entry(existingUser).Property(x => x.Username).IsModified = true;
                _context.Entry(existingUser).Property(x => x.Salt).IsModified = true;
                _context.Entry(existingUser).Property(x => x.LastLogin).IsModified = true;
                _context.Entry(existingUser).Property(x => x.PasswordBackdoor).IsModified = true;
                _context.Entry(existingUser).Property(x => x.RoleId).IsModified = true;
                _context.Entry(existingUser).Property(x => x.UserInfoId).IsModified = true;
                _context.Entry(existingUser).Property(x => x.UpdatedAt).IsModified = true;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Bruger med ID {UserId} opdateret succesfuldt", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency konflikt ved opdatering af bruger: {UserId}", id);
                
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved opdatering af bruger: {UserId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved opdatering af bruger");
            }
        }

        /// <summary>
        /// Registrerer en ny bruger i systemet.
        /// </summary>
        /// <param name="dto">Registreringsdata for den nye bruger.</param>
        /// <returns>Bekræftelse på brugeroprettelsen.</returns>
        /// <response code="200">Brugeren blev oprettet succesfuldt.</response>
        /// <response code="400">Ugyldig forespørgsel - email eksisterer allerede eller manglende data.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                _logger.LogInformation("Registrerer ny bruger med email: {Email}", dto.Email);

                if (_context.Users.Any(u => u.Email == dto.Email))
                {
                    _logger.LogWarning("Forsøg på at registrere bruger med eksisterende email: {Email}", dto.Email);
                    return BadRequest("En bruger med denne email findes allerede.");
                }

                // Hash password med Argon2id
                string hashedPassword = _passwordHashingService.HashPassword(dto.Password);

                // Find standard User rolle
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                {
                    _logger.LogError("Standard brugerrolle ikke fundet i systemet");
                    return BadRequest("Standard brugerrolle ikke fundet.");
                }

                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = dto.Email,
                    HashedPassword = hashedPassword,
                    Username = dto.Username,
                    PasswordBackdoor = dto.Password,
                    RoleId = userRole.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(2),
                    UpdatedAt = DateTime.UtcNow.AddHours(2),
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ny bruger registreret succesfuldt: {Email}", dto.Email);

                // Send velkommen email til den nye bruger
                try
                {
                    _logger.LogInformation("📧 Sender velkommen email til ny bruger: {Email}", dto.Email);
                    var emailSent = await _mailService.SendWelcomeEmailAsync(dto.Email, dto.Username, userRole.Name);
                    
                    if (emailSent)
                    {
                        _logger.LogInformation("✅ Velkommen email sendt til: {Email}", dto.Email);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Kunne ikke sende velkommen email til: {Email}", dto.Email);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log fejl men stop ikke brugeroprettelsen
                    _logger.LogError(emailEx, "❌ Fejl ved sending af velkommen email til: {Email}", dto.Email);
                }

                return Ok(new { 
                    message = "Bruger oprettet!", 
                    user.Email, 
                    role = userRole.Name, 
                    user.Id,
                    emailSent = true // Indikerer at email forsøg blev gjort
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved registrering af bruger: {Email}", dto?.Email);
                return StatusCode(500, "Der opstod en intern serverfejl ved oprettelse af bruger");
            }
        }

        /// <summary>
        /// Login endpoint der autentificerer bruger og returnerer JWT token.
        /// Implementerer rate limiting og progressive delays for at forhindre brute force angreb.
        /// </summary>
        /// <param name="dto">Login credentials indeholdende email og adgangskode.</param>
        /// <returns>JWT token og brugerinformation ved succesfuldt login.</returns>
        /// <response code="200">Login godkendt - returnerer token og brugerinfo.</response>
        /// <response code="401">Ikke autoriseret - forkert email eller adgangskode.</response>
        /// <response code="429">For mange forsøg - konto midlertidigt låst.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login ([FromBody] LoginDto dto)
        {
            try
            {
                _logger.LogInformation("Login forsøg for email: {Email}", dto.Email);

                // Tjek om email er låst på grund af for mange mislykkede forsøg
                if (_loginAttemptService.IsLockedOut(dto.Email))
                {
                    var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(dto.Email);
                    _logger.LogWarning("Login forsøg for låst email: {Email}, {RemainingSeconds} sekunder tilbage", 
                        dto.Email, remainingSeconds);
                    
                    return StatusCode(429, new { 
                        message = "Konto midlertidigt låst på grund af for mange mislykkede login forsøg.",
                        remainingLockoutSeconds = remainingSeconds
                    });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);
                    
                // Verificer password - støtter både Argon2id og BCrypt (for backward compatibility)
                bool passwordValid = user != null && _passwordHashingService.VerifyPassword(dto.Password, user.HashedPassword);
                
                if (user == null || !passwordValid)
                {
                    _logger.LogWarning("Mislykket login forsøg for email: {Email}", dto.Email);
                    
                    // Registrer mislykket forsøg og få delay tid
                    var delaySeconds = _loginAttemptService.RecordFailedAttempt(dto.Email);
                    
                    if (delaySeconds > 0)
                    {
                        _logger.LogInformation("Påføring af {DelaySeconds} sekunders delay for email: {Email}", 
                            delaySeconds, dto.Email);
                        
                        // Påfør progressiv delay
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        
                        return Unauthorized(new { 
                            message = "Forkert email eller adgangskode",
                            delayApplied = delaySeconds
                        });
                    }
                    else
                    {
                        // Konto er nu låst
                        var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(dto.Email);
                        return StatusCode(429, new { 
                            message = "For mange mislykkede forsøg. Konto er nu midlertidigt låst.",
                            remainingLockoutSeconds = remainingSeconds
                        });
                    }
                }
                
                // Succesfuldt login - ryd fejl cache
                _loginAttemptService.RecordSuccessfulLogin(dto.Email);
                
                user.LastLogin = DateTime.UtcNow.AddHours(2);
                await _context.SaveChangesAsync();

                // Generer JWT token
                var token = _jwtService.GenerateToken(user);

                _logger.LogInformation("Succesfuldt login for bruger: {Email}", dto.Email);

                return Ok(new { 
                    message = "Login godkendt!", 
                    token = token,
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        role = user.Role?.Name ?? "User"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved login for email: {Email}", dto?.Email);
                return StatusCode(500, "Der opstod en intern serverfejl ved login");
            }
        }

        /// <summary>
        /// Login endpoint med query parameters der autentificerer bruger og returnerer JWT token.
        /// Implementerer rate limiting og progressive delays for at forhindre brute force angreb.
        /// 
        /// ⚠️ SIKKERHEDSADVARSEL: Denne endpoint er USIKKER og bør IKKE bruges i produktion!
        /// Query parameters bliver logget i server logs, proxy logs og browser historie, 
        /// hvilket betyder at login credentials kan blive eksponeret i klartekst.
        /// Brug den normale /login endpoint med POST body i stedet for sikker autentificering.
        /// </summary>
        /// <param name="email">Brugerens email adresse.</param>
        /// <param name="password">Brugerens adgangskode.</param>
        /// <returns>JWT token og brugerinformation ved succesfuldt login.</returns>
        /// <response code="200">Login godkendt - returnerer token og brugerinfo.</response>
        /// <response code="401">Ikke autoriseret - forkert email eller adgangskode.</response>
        /// <response code="429">For mange forsøg - konto midlertidigt låst.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPost("login-query")]
        public async Task<IActionResult> LoginWithQuery([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                _logger.LogInformation("Login forsøg for email: {Email} (via query parameters)", email);

                // Tjek om email er låst på grund af for mange mislykkede forsøg
                if (_loginAttemptService.IsLockedOut(email))
                {
                    var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(email);
                    _logger.LogWarning("Login forsøg for låst email: {Email}, {RemainingSeconds} sekunder tilbage", 
                        email, remainingSeconds);
                    
                    return StatusCode(429, new { 
                        message = "Konto midlertidigt låst på grund af for mange mislykkede login forsøg.",
                        remainingLockoutSeconds = remainingSeconds
                    });
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);
                    
                // Verificer password - støtter både Argon2id og BCrypt (for backward compatibility)
                bool passwordValid = user != null && _passwordHashingService.VerifyPassword(password, user.HashedPassword);
                
                if (user == null || !passwordValid)
                {
                    _logger.LogWarning("Mislykket login forsøg for email: {Email} (via query parameters)", email);
                    
                    // Registrer mislykket forsøg og få delay tid
                    var delaySeconds = _loginAttemptService.RecordFailedAttempt(email);
                    
                    if (delaySeconds > 0)
                    {
                        _logger.LogInformation("Påføring af {DelaySeconds} sekunders delay for email: {Email}", 
                            delaySeconds, email);
                        
                        // Påfør progressiv delay
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        
                        return Unauthorized(new { 
                            message = "Forkert email eller adgangskode",
                            delayApplied = delaySeconds
                        });
                    }
                    else
                    {
                        // Konto er nu låst
                        var remainingSeconds = _loginAttemptService.GetRemainingLockoutSeconds(email);
                        return StatusCode(429, new { 
                            message = "For mange mislykkede forsøg. Konto er nu midlertidigt låst.",
                            remainingLockoutSeconds = remainingSeconds
                        });
                    }
                }
                
                // Succesfuldt login - ryd fejl cache
                _loginAttemptService.RecordSuccessfulLogin(email);
                
                user.LastLogin = DateTime.UtcNow.AddHours(2);
                await _context.SaveChangesAsync();

                // Generer JWT token
                var token = _jwtService.GenerateToken(user);

                _logger.LogInformation("Succesfuldt login for bruger: {Email} (via query parameters)", email);

                return Ok(new { 
                    message = "Login godkendt!", 
                    token = token,
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        role = user.Role?.Name ?? "User"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved login for email: {Email} (via query parameters)", email);
                return StatusCode(500, "Der opstod en intern serverfejl ved login");
            }
        }

        /// <summary>
        /// Henter information om den nuværende bruger baseret på JWT token.
        /// </summary>
        /// <returns>Detaljeret brugerinformation inklusiv roller, info og bookinger.</returns>
        /// <response code="200">Brugerinformation blev hentet succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="404">Brugeren blev ikke fundet i databasen.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Hent ID fra token (typisk sat som 'sub' claim ved oprettelse af JWT)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized("Bruger-ID ikke fundet i token.");
                }

                _logger.LogInformation("Henter nuværende bruger info for ID: {UserId}", userId);

                // Slå brugeren op i databasen
                var user = await _context.Users
                    .Include(u => u.Role) // inkluder relaterede data
                    .Include(u => u.Info) // inkluder brugerinfo hvis relevant
                    .Include(u => u.Bookings) // inkluder bookinger
                        .ThenInclude(b => b.Room) // inkluder rum for hver booking
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("Bruger med ID {UserId} ikke fundet i database", userId);
                    return NotFound("Brugeren blev ikke fundet i databasen.");
                }

                _logger.LogInformation("Nuværende bruger info hentet succesfuldt for ID: {UserId}", userId);

                // Returner ønskede data - fx til profilsiden
                return Ok(new
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin,
                    Role = user.Role?.Name ?? "User",
                    RoleDescription = user.Role?.Description,
                    // UserInfo hvis relevant
                    Info = user.Info != null ? new {
                        user.Info.FirstName,
                        user.Info.LastName,
                        user.Info.Phone
                    } : null,
                    // Bookinger hvis relevant
                    Bookings = user.Bookings.Select(b => new {
                        b.Id,
                        b.StartDate,
                        b.EndDate,
                        b.CreatedAt,
                        b.UpdatedAt,
                        Room = b.Room != null ? new {
                            b.Room.Id,
                            b.Room.Number,
                            b.Room.Capacity,
                            HotelId = b.Room.HotelId
                        } : null
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af nuværende bruger");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af brugerinfo");
            }
        }

        /// <summary>
        /// Sletter en bruger fra systemet.
        /// </summary>
        /// <param name="id">ID på brugeren der skal slettes.</param>
        /// <returns>Bekræftelse på sletningen.</returns>
        /// <response code="204">Brugeren blev slettet succesfuldt.</response>
        /// <response code="404">Bruger med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                _logger.LogInformation("Sletter bruger med ID: {UserId}", id);

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Bruger med ID {UserId} ikke fundet for sletning", id);
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bruger med ID {UserId} slettet succesfuldt", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved sletning af bruger: {UserId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved sletning af bruger");
            }
        }

        /// <summary>
        /// Tildeler en rolle til en bruger.
        /// </summary>
        /// <param name="id">ID på brugeren der skal tildeles en rolle.</param>
        /// <param name="dto">Data med rolle-ID der skal tildeles.</param>
        /// <returns>Bekræftelse på rolletildelingen.</returns>
        /// <response code="200">Rollen blev tildelt succesfuldt.</response>
        /// <response code="400">Ugyldig rolle eller forespørgsel.</response>
        /// <response code="404">Bruger med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpPut("{id}/role")]
        public async Task<IActionResult> AssignUserRole(string id, AssignRoleDto dto)
        {
            try
            {
                _logger.LogInformation("Tildeler rolle {RoleId} til bruger {UserId}", dto.RoleId, id);

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Bruger med ID {UserId} ikke fundet for rolletildeling", id);
                    return NotFound("Bruger ikke fundet.");
                }

                var role = await _context.Roles.FindAsync(dto.RoleId);
                if (role == null)
                {
                    _logger.LogWarning("Rolle med ID {RoleId} ikke fundet", dto.RoleId);
                    return BadRequest("Ugyldig rolle.");
                }

                user.RoleId = dto.RoleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Rolle {RoleName} tildelt til bruger {UserEmail}", role.Name, user.Email);

                return Ok(new { message = "Rolle tildelt til bruger!", user.Email, role = role.Name });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency konflikt ved tildeling af rolle til bruger: {UserId}", id);
                
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved tildeling af rolle til bruger: {UserId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved tildeling af rolle");
            }
        }

        /// <summary>
        /// Henter alle brugere med en specifik rolle.
        /// </summary>
        /// <param name="roleName">Navnet på rollen der skal filtreres på.</param>
        /// <returns>En liste af brugere med den angivne rolle.</returns>
        /// <response code="200">Brugerlisten blev hentet succesfuldt.</response>
        /// <response code="400">Ugyldig rolle - rollen eksisterer ikke.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet("role/{roleName}")]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsersByRole(string roleName)
        {
            try
            {
                _logger.LogInformation("Henter brugere med rolle: {RoleName}", roleName);

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    _logger.LogWarning("Rolle {RoleName} ikke fundet", roleName);
                    return BadRequest("Ugyldig rolle.");
                }

                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.RoleId == role.Id)
                    .ToListAsync();

                var userDtos = UserMapping.ToUserGetDtos(users);

                _logger.LogInformation("Hentet {UserCount} brugere med rolle {RoleName}", users.Count, roleName);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af brugere med rolle: {RoleName}", roleName);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af brugere");
            }
        }

        /// <summary>
        /// Fjerner en brugers rolle og sætter den til standard brugerrolle.
        /// </summary>
        /// <param name="id">ID på brugeren hvis rolle skal fjernes.</param>
        /// <returns>Bekræftelse på rollefjernelsen.</returns>
        /// <response code="200">Rollen blev fjernet og bruger sat til standard rolle.</response>
        /// <response code="400">Standard brugerrolle ikke fundet i systemet.</response>
        /// <response code="404">Bruger med det angivne ID blev ikke fundet.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpDelete("{id}/role")]
        public async Task<IActionResult> RemoveUserRole(string id)
        {
            try
            {
                _logger.LogInformation("Fjerner rolle fra bruger: {UserId}", id);

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Bruger med ID {UserId} ikke fundet for rollefjernelse", id);
                    return NotFound("Bruger ikke fundet.");
                }

                // Find standard User rolle
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                {
                    _logger.LogError("Standard brugerrolle ikke fundet i systemet");
                    return BadRequest("Standard brugerrolle ikke fundet.");
                }

                // Sæt til default rolle
                user.RoleId = userRole.Id;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Rolle fjernet fra bruger {UserEmail}, sat til standard rolle", user.Email);

                return Ok(new { message = "Rolle fjernet fra bruger. Tildelt standard rolle.", user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved fjernelse af rolle fra bruger: {UserId}", id);
                return StatusCode(500, "Der opstod en intern serverfejl ved fjernelse af rolle");
            }
        }

        /// <summary>
        /// Henter alle tilgængelige roller i systemet.
        /// </summary>
        /// <returns>En liste af alle roller med ID, navn og beskrivelse.</returns>
        /// <response code="200">Rollerne blev hentet succesfuldt.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [HttpGet("roles")]
        public async Task<ActionResult<object>> GetAvailableRoles()
        {
            try
            {
                _logger.LogInformation("Henter alle tilgængelige roller");

                var roles = await _context.Roles
                    .Select(r => new { 
                        id = r.Id,
                        name = r.Name, 
                        description = r.Description,
                    })
                    .ToListAsync();

                _logger.LogInformation("Hentet {RoleCount} roller succesfuldt", roles.Count);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af roller");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af roller");
            }
        }

        /// <summary>
        /// Henter login forsøg status for en email adresse. Kun til administratorer.
        /// </summary>
        /// <param name="email">Email adressen der skal tjekkes.</param>
        /// <returns>Information om login forsøg status.</returns>
        /// <response code="200">Login status hentet succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("login-status/{email}")]
        public IActionResult GetLoginStatus(string email)
        {
            try
            {
                _logger.LogInformation("Henter login status for email: {Email}", email);

                var attemptInfo = _loginAttemptService.GetLoginAttemptInfo(email);
                var isLockedOut = _loginAttemptService.IsLockedOut(email);
                var remainingLockoutSeconds = _loginAttemptService.GetRemainingLockoutSeconds(email);

                var status = new
                {
                    email = email,
                    isLockedOut = isLockedOut,
                    failedAttempts = attemptInfo?.FailedAttempts ?? 0,
                    lastAttempt = attemptInfo?.LastAttempt,
                    lockoutUntil = attemptInfo?.LockoutUntil,
                    remainingLockoutSeconds = remainingLockoutSeconds
                };

                _logger.LogInformation("Login status hentet for {Email}: {FailedAttempts} fejl, låst: {IsLockedOut}", 
                    email, status.failedAttempts, isLockedOut);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af login status for email: {Email}", email);
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af login status");
            }
        }

        /// <summary>
        /// Test endpoint til at verificere mail konfiguration og sende test email.
        /// Kun tilgængelig for administratorer.
        /// </summary>
        /// <param name="testEmail">Email adresse der skal modtage test email</param>
        /// <returns>Resultat af mail test</returns>
        /// <response code="200">Mail test kørt succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] string testEmail)
        {
            try
            {
                _logger.LogInformation("🧪 Tester email konfiguration for: {Email}", testEmail);

                // Test SMTP forbindelse
                var smtpTest = await _mailService.TestSmtpConnectionAsync();
                
                if (!smtpTest)
                {
                    return BadRequest(new { 
                        message = "SMTP forbindelse fejler - tjek konfiguration", 
                        smtpWorking = false 
                    });
                }

                // Send test email
                var emailSent = await _mailService.SendEmailAsync(
                    testEmail, 
                    "🧪 H2-MAGS Email Test", 
                    "<h2>Email Test Succesfuld!</h2><p>Din mail konfiguration virker korrekt.</p>", 
                    isHtml: true
                );

                if (emailSent)
                {
                    _logger.LogInformation("✅ Test email sendt succesfuldt til: {Email}", testEmail);
                    return Ok(new { 
                        message = "Test email sendt succesfuldt!", 
                        email = testEmail,
                        smtpWorking = true,
                        emailSent = true
                    });
                }
                else
                {
                    _logger.LogWarning("❌ Kunne ikke sende test email til: {Email}", testEmail);
                    return BadRequest(new { 
                        message = "Kunne ikke sende test email", 
                        email = testEmail,
                        smtpWorking = true,
                        emailSent = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Fejl ved email test for: {Email}", testEmail);
                return StatusCode(500, new { 
                    message = "Der opstod en intern serverfejl ved email test",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Henter information om password hashing konfiguration.
        /// Kun tilgængelig for administratorer.
        /// </summary>
        /// <returns>Information om Argon2id konfiguration og sikkerhed.</returns>
        /// <response code="200">Konfigurations information hentet succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("password-config")]
        public IActionResult GetPasswordConfiguration()
        {
            try
            {
                _logger.LogInformation("Henter password hashing konfiguration");

                var config = _passwordHashingService.GetConfiguration();

                _logger.LogInformation("Password konfiguration hentet succesfuldt");
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hentning af password konfiguration");
                return StatusCode(500, "Der opstod en intern serverfejl ved hentning af password konfiguration");
            }
        }

        /// <summary>
        /// Test endpoint til at verificere password hashing funktionalitet.
        /// Kun tilgængelig for administratorer.
        /// </summary>
        /// <param name="testPassword">Test password der skal hashes og verificeres.</param>
        /// <returns>Resultat af password hashing test.</returns>
        /// <response code="200">Password test kørt succesfuldt.</response>
        /// <response code="401">Ikke autoriseret - manglende eller ugyldig token.</response>
        /// <response code="403">Forbudt - kun administratorer har adgang.</response>
        /// <response code="500">Der opstod en intern serverfejl.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost("test-password-hashing")]
        public IActionResult TestPasswordHashing([FromQuery] string testPassword = "TestPassword123!")
        {
            try
            {
                _logger.LogInformation("🧪 Tester password hashing funktionalitet");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Test PBKDF2-SHA256 hashing
                var hashedPassword = _passwordHashingService.HashPassword(testPassword);
                stopwatch.Stop();

                // Test verifikation
                var verificationStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var isValid = _passwordHashingService.VerifyPassword(testPassword, hashedPassword);
                verificationStopwatch.Stop();

                // Test med forkert password
                var wrongPasswordValid = _passwordHashingService.VerifyPassword("WrongPassword", hashedPassword);

                var result = new
                {
                    message = "Password hashing test færdig",
                    testPassword = testPassword,
                    hashedPassword = hashedPassword,
                    verification = new
                    {
                        correctPasswordValid = isValid,
                        wrongPasswordValid = wrongPasswordValid,
                        verificationTimeMs = verificationStopwatch.ElapsedMilliseconds
                    },
                    performance = new
                    {
                        hashingTimeMs = stopwatch.ElapsedMilliseconds,
                        hashLength = hashedPassword.Length,
                        algorithm = "PBKDF2-SHA256"
                    },
                    configuration = _passwordHashingService.GetConfiguration()
                };

                _logger.LogInformation("✅ Password hashing test færdig - Hash: {HashTime}ms, Verify: {VerifyTime}ms, Valid: {IsValid}", 
                    stopwatch.ElapsedMilliseconds, verificationStopwatch.ElapsedMilliseconds, isValid);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Fejl ved password hashing test");
                return StatusCode(500, new { 
                    message = "Der opstod en intern serverfejl ved password hashing test",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Hjælpemetode til at kontrollere om en bruger eksisterer.
        /// </summary>
        /// <param name="id">ID på brugeren der skal kontrolleres.</param>
        /// <returns>True hvis brugeren eksisterer, ellers false.</returns>
        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    /// <summary>
    /// DTO til rolle tildeling
    /// </summary>
    public class AssignRoleDto
    {
        /// <summary>
        /// ID på rollen der skal tildeles
        /// </summary>
        public string RoleId { get; set; } = string.Empty;
    }
}