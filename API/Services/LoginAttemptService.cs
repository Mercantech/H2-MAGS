using Microsoft.Extensions.Caching.Memory;

namespace API.Services
{
    /// <summary>
    /// Service til håndtering af login forsøg og rate limiting
    /// </summary>
    public class LoginAttemptService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<LoginAttemptService> _logger;
        
        // Konfiguration for rate limiting
        private const int MaxAttempts = 5; // Maksimalt antal forsøg før lockout
        private const int LockoutMinutes = 15; // Lockout periode i minutter
        private const int DelayIncrementSeconds = 2; // Sekunder at tilføje per forsøg

        public LoginAttemptService(IMemoryCache cache, ILogger<LoginAttemptService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Tjekker om en email er låst på grund af for mange mislykkede forsøg
        /// </summary>
        /// <param name="email">Email adressen der skal tjekkes</param>
        /// <returns>True hvis email er låst, ellers false</returns>
        public bool IsLockedOut(string email)
        {
            var key = GetCacheKey(email);
            var attempts = _cache.Get<LoginAttemptInfo>(key);
            
            if (attempts == null) return false;
            
            // Tjek om lockout periode er udløbet
            if (attempts.LockoutUntil.HasValue && DateTime.UtcNow < attempts.LockoutUntil.Value)
            {
                _logger.LogWarning("Email {Email} er låst indtil {LockoutUntil}", email, attempts.LockoutUntil.Value);
                return true;
            }
            
            // Hvis lockout periode er udløbet, ryd cache entry
            if (attempts.LockoutUntil.HasValue && DateTime.UtcNow >= attempts.LockoutUntil.Value)
            {
                _cache.Remove(key);
                _logger.LogInformation("Lockout periode udløbet for email {Email}, cache ryddet", email);
                return false;
            }
            
            return false;
        }

        /// <summary>
        /// Registrerer et mislykket login forsøg og returnerer delay tid
        /// </summary>
        /// <param name="email">Email adressen for det mislykkede forsøg</param>
        /// <returns>Antal sekunder der skal ventes før næste forsøg</returns>
        public int RecordFailedAttempt(string email)
        {
            var key = GetCacheKey(email);
            var attempts = _cache.Get<LoginAttemptInfo>(key) ?? new LoginAttemptInfo();
            
            attempts.FailedAttempts++;
            attempts.LastAttempt = DateTime.UtcNow;
            
            _logger.LogWarning("Mislykket login forsøg #{AttemptNumber} for email {Email}", 
                attempts.FailedAttempts, email);
            
            // Beregn delay baseret på antal forsøg
            var delaySeconds = Math.Min(attempts.FailedAttempts * DelayIncrementSeconds, 30); // Max 30 sekunder delay
            
            // Hvis maksimalt antal forsøg er nået, lås kontoen
            if (attempts.FailedAttempts >= MaxAttempts)
            {
                attempts.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                _logger.LogWarning("Email {Email} låst efter {MaxAttempts} mislykkede forsøg indtil {LockoutUntil}", 
                    email, MaxAttempts, attempts.LockoutUntil.Value);
                
                // Cache i lockout periode + ekstra buffer
                _cache.Set(key, attempts, TimeSpan.FromMinutes(LockoutMinutes + 5));
                
                return 0; // Ingen delay, da kontoen er låst
            }
            else
            {
                // Cache i 1 time eller indtil lockout
                var cacheExpiration = TimeSpan.FromHours(1);
                _cache.Set(key, attempts, cacheExpiration);
                
                return delaySeconds;
            }
        }

        /// <summary>
        /// Registrerer et succesfuldt login og rydder fejl cache
        /// </summary>
        /// <param name="email">Email adressen for det succesfulde login</param>
        public void RecordSuccessfulLogin(string email)
        {
            var key = GetCacheKey(email);
            _cache.Remove(key);
            
            _logger.LogInformation("Succesfuldt login for {Email}, fejl cache ryddet", email);
        }

        /// <summary>
        /// Henter information om login forsøg for en email
        /// </summary>
        /// <param name="email">Email adressen</param>
        /// <returns>Information om login forsøg eller null</returns>
        public LoginAttemptInfo? GetLoginAttemptInfo(string email)
        {
            var key = GetCacheKey(email);
            return _cache.Get<LoginAttemptInfo>(key);
        }

        /// <summary>
        /// Henter resterende lockout tid i sekunder
        /// </summary>
        /// <param name="email">Email adressen</param>
        /// <returns>Antal sekunder til lockout udløber, eller 0 hvis ikke låst</returns>
        public int GetRemainingLockoutSeconds(string email)
        {
            var attempts = GetLoginAttemptInfo(email);
            if (attempts?.LockoutUntil == null) return 0;
            
            var remaining = (attempts.LockoutUntil.Value - DateTime.UtcNow).TotalSeconds;
            return Math.Max(0, (int)remaining);
        }

        /// <summary>
        /// Genererer cache key for en email
        /// </summary>
        private string GetCacheKey(string email)
        {
            return $"login_attempts_{email.ToLowerInvariant()}";
        }
    }

    /// <summary>
    /// Information om login forsøg for en email adresse
    /// </summary>
    public class LoginAttemptInfo
    {
        /// <summary>
        /// Antal mislykkede forsøg
        /// </summary>
        public int FailedAttempts { get; set; } = 0;

        /// <summary>
        /// Tidspunkt for sidste forsøg
        /// </summary>
        public DateTime LastAttempt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tidspunkt hvor lockout udløber (null hvis ikke låst)
        /// </summary>
        public DateTime? LockoutUntil { get; set; }
    }
}
