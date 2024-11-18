using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainModels;
using Microsoft.IdentityModel.Tokens;

namespace API.Services.Mapping.Users
{
    public class JWTService
    {
        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_KEY")
                        ?? _configuration["JwtSettings:Key"]
                )
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                Environment.GetEnvironmentVariable("JWT_ISSUER")
                    ?? _configuration["JwtSettings:Issuer"],
                Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                    ?? _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateJwtTokenAD(string userId, string userName)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userName)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_KEY")
                        ?? _configuration["JwtSettings:Key"]
                )
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                Environment.GetEnvironmentVariable("JWT_ISSUER")
                    ?? _configuration["JwtSettings:Issuer"],
                Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                    ?? _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string accessToken, string refreshToken) GenerateTokens(User user)
        {
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);
            return (accessToken, refreshToken);
        }

        private string GenerateRefreshToken(string userId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tokenType", "refresh") // For at identificere at dette er en refresh token
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddDays(7), // Længere udløbstid for refresh tokens
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        Environment.GetEnvironmentVariable("JWT_KEY")
                            ?? _configuration["JwtSettings:Key"]
                    )
                );

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer =
                        Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience =
                        Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                        ?? _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(
                    refreshToken,
                    tokenValidationParameters,
                    out var validatedToken
                );
                var jwtToken = validatedToken as JwtSecurityToken;

                return jwtToken != null
                    && jwtToken.Claims.Any(x => x.Type == "tokenType" && x.Value == "refresh");
            }
            catch
            {
                return false;
            }
        }
    }
}
