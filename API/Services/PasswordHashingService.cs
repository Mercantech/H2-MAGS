using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    /// <summary>
    /// Service til sikker password hashing og verifikation.
    /// Bruger PBKDF2 med SHA-256 algoritmen som er en moderne og sikker standard for password hashing.
    /// 
    /// 🔐 SIKKERHEDS FORDELE:
    /// - PBKDF2 er en NIST-anbefalet standard (SP 800-132)
    /// - Modstandsdygtig over for rainbow table angreb
    /// - Konfigurerbar iteration count for at tilpasse sikkerhed vs performance
    /// - Automatisk salt generation og inklusion
    /// - SHA-256 er en stærk og godkendt hash funktion
    /// - .NET indbygget implementering - ingen eksterne dependencies
    /// </summary>
    public class PasswordHashingService
    {
        private readonly ILogger<PasswordHashingService> _logger;
        
        // PBKDF2 konfiguration - sikker standard konfiguration
        private const int SaltSize = 32;      // 256-bit salt
        private const int HashSize = 32;      // 256-bit hash
        private const int IterationCount = 100000; // 100,000 iterations (NIST anbefaling)

        /// <summary>
        /// Initialiserer en ny instans af PasswordHashingService.
        /// </summary>
        /// <param name="logger">Logger til fejlrapportering og debugging.</param>
        public PasswordHashingService(ILogger<PasswordHashingService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Hasher et password med PBKDF2-SHA256 algoritmen.
        /// </summary>
        /// <param name="password">Det klartekst password der skal hashes.</param>
        /// <returns>Et hashed password med inkluderet salt og konfiguration.</returns>
        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Password kan ikke være tomt eller null", nameof(password));
                }

                _logger.LogDebug("Hasher password med PBKDF2-SHA256 (Iterations: {IterationCount})", IterationCount);

                // Generer et tilfældigt salt
                var salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Hash password med PBKDF2
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA256))
                {
                    var hashBytes = pbkdf2.GetBytes(HashSize);
                    
                    // Kombiner salt og hash til en enkelt string
                    var saltString = Convert.ToBase64String(salt);
                    var hashString = Convert.ToBase64String(hashBytes);
                    var hashedPassword = $"PBKDF2:{IterationCount}:{saltString}:{hashString}";

                    _logger.LogDebug("Password hashed succesfuldt med PBKDF2-SHA256");
                    return hashedPassword;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved hashing af password med PBKDF2-SHA256");
                throw new InvalidOperationException("Kunne ikke hashe password", ex);
            }
        }

        /// <summary>
        /// Verificerer et password mod et hashed password.
        /// Støtter både PBKDF2 og BCrypt hashes for backward compatibility.
        /// </summary>
        /// <param name="password">Det klartekst password der skal verificeres.</param>
        /// <param name="hashedPassword">Det hashed password der skal sammenlignes med.</param>
        /// <returns>True hvis password'et matcher, ellers false.</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Forsøg på at verificere tomt password");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(hashedPassword))
                {
                    _logger.LogWarning("Forsøg på at verificere mod tomt hashed password");
                    return false;
                }

                // Tjek om det er et BCrypt hash (backward compatibility)
                if (IsBcryptHash(hashedPassword))
                {
                    _logger.LogDebug("Verificerer password mod BCrypt hash (legacy)");
                    return VerifyBcryptPassword(password, hashedPassword);
                }

                // Tjek om det er et PBKDF2 hash
                if (IsPbkdf2Hash(hashedPassword))
                {
                    _logger.LogDebug("Verificerer password mod PBKDF2-SHA256 hash");
                    return VerifyPbkdf2Password(password, hashedPassword);
                }

                _logger.LogWarning("Ukendt hash format: {HashFormat}", hashedPassword.Length > 50 ? hashedPassword[..50] + "..." : hashedPassword);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved verifikation af password");
                return false; // Ved fejl, antag at password'et er forkert
            }
        }

        /// <summary>
        /// Verificerer et password mod et BCrypt hash (for backward compatibility).
        /// </summary>
        /// <param name="password">Det klartekst password der skal verificeres.</param>
        /// <param name="bcryptHash">Det BCrypt hashed password.</param>
        /// <returns>True hvis password'et matcher, ellers false.</returns>
        private bool VerifyBcryptPassword(string password, string bcryptHash)
        {
            try
            {
                // Brug BCrypt.Net til at verificere legacy passwords
                return BCrypt.Net.BCrypt.Verify(password, bcryptHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved verifikation af BCrypt password");
                return false;
            }
        }

        /// <summary>
        /// Verificerer et password mod et PBKDF2 hash.
        /// </summary>
        /// <param name="password">Det klartekst password der skal verificeres.</param>
        /// <param name="pbkdf2Hash">Det PBKDF2 hashed password.</param>
        /// <returns>True hvis password'et matcher, ellers false.</returns>
        private bool VerifyPbkdf2Password(string password, string pbkdf2Hash)
        {
            try
            {
                // Parse PBKDF2 hash format: "PBKDF2:iterations:salt:hash"
                var parts = pbkdf2Hash.Split(':');
                if (parts.Length != 4 || parts[0] != "PBKDF2")
                {
                    _logger.LogWarning("Ugyldigt PBKDF2 hash format");
                    return false;
                }

                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var storedHash = Convert.FromBase64String(parts[3]);

                // Hash password med samme salt og iterations
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var computedHash = pbkdf2.GetBytes(HashSize);
                    return storedHash.SequenceEqual(computedHash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved verifikation af PBKDF2 password");
                return false;
            }
        }

        /// <summary>
        /// Tjekker om et hashed password er i PBKDF2 format.
        /// </summary>
        /// <param name="hashedPassword">Det hashed password der skal tjekkes.</param>
        /// <returns>True hvis det er PBKDF2 format, ellers false.</returns>
        private bool IsPbkdf2Hash(string hashedPassword)
        {
            return !string.IsNullOrWhiteSpace(hashedPassword) && 
                   hashedPassword.StartsWith("PBKDF2:") &&
                   hashedPassword.Split(':').Length == 4;
        }

        /// <summary>
        /// Migrerer et BCrypt hashed password til PBKDF2-SHA256.
        /// Bruges til at konvertere eksisterende passwords fra BCrypt til PBKDF2.
        /// </summary>
        /// <param name="bcryptHash">Det BCrypt hashed password.</param>
        /// <param name="plainPassword">Det originale klartekst password.</param>
        /// <returns>Et nyt PBKDF2-SHA256 hashed password.</returns>
        public string MigrateFromBcrypt(string bcryptHash, string plainPassword)
        {
            try
            {
                _logger.LogInformation("Migrerer password fra BCrypt til PBKDF2-SHA256");

                // Verificer først at BCrypt hash'et er gyldigt
                if (string.IsNullOrWhiteSpace(bcryptHash) || !bcryptHash.StartsWith("$2"))
                {
                    throw new ArgumentException("Ugyldigt BCrypt hash format", nameof(bcryptHash));
                }

                // Hash med PBKDF2-SHA256
                var newHash = HashPassword(plainPassword);

                _logger.LogInformation("Password migreret succesfuldt fra BCrypt til PBKDF2-SHA256");
                return newHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved migrering af password fra BCrypt til PBKDF2-SHA256");
                throw new InvalidOperationException("Kunne ikke migrere password", ex);
            }
        }

        /// <summary>
        /// Tjekker om et hashed password er i BCrypt format.
        /// </summary>
        /// <param name="hashedPassword">Det hashed password der skal tjekkes.</param>
        /// <returns>True hvis det er BCrypt format, ellers false.</returns>
        public bool IsBcryptHash(string hashedPassword)
        {
            return !string.IsNullOrWhiteSpace(hashedPassword) && 
                   hashedPassword.StartsWith("$2") && 
                   hashedPassword.Length >= 60;
        }

        /// <summary>
        /// Henter information om den nuværende PBKDF2-SHA256 konfiguration.
        /// </summary>
        /// <returns>En object med konfigurations information.</returns>
        public object GetConfiguration()
        {
            return new
            {
                algorithm = "PBKDF2-SHA256",
                saltSize = SaltSize,
                hashSize = HashSize,
                iterationCount = IterationCount,
                type = "PBKDF2",
                description = "PBKDF2 med SHA-256 og sikker standard konfiguration",
                securityLevel = "Høj - NIST-anbefalet standard (SP 800-132)",
                standards = new[] { "NIST SP 800-132", "RFC 2898", "FIPS 140-2" }
            };
        }
    }
}
