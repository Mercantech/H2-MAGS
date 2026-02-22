using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainModels;
using API.Services;

namespace API.Services
{
    /// <summary>
    /// Service til håndtering af JWT tokens - generering, validering og decoding
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        /// <summary>
        /// Initialiserer en ny instans af JwtService
        /// </summary>
        /// <param name="configuration">Konfiguration til JWT indstillinger</param>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789";
            _issuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "H2-2025-API";
            _audience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "H2-2025-Client";
            _expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60");
        }

        /// <summary>
        /// Genererer en JWT token for en bruger
        /// </summary>
        /// <param name="user">Brugeren der skal have en token</param>
        /// <returns>JWT token som string</returns>
        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("userId", user.Id),
                new Claim("username", user.Username)
            };

            // Tilføj rolle claim hvis brugeren har en rolle
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2).AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Genererer en JWT token for en AD bruger
        /// </summary>
        /// <param name="adUser">AD brugeren der skal have en token</param>
        /// <param name="role">Rollen der skal tildeles brugeren</param>
        /// <returns>JWT token som string</returns>
        public string GenerateTokenForADUser(ADUserInfo adUser, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adUser.SamAccountName),
                new Claim(ClaimTypes.Email, adUser.Email),
                new Claim(ClaimTypes.Name, adUser.DisplayName),
                new Claim("userId", adUser.SamAccountName),
                new Claim("username", adUser.SamAccountName),
                new Claim("adUser", "true"), // Marker som AD bruger
                new Claim("adGroups", string.Join(",", adUser.Groups))
            };

            // Tilføj rolle claim
            claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2).AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        
    }
}
