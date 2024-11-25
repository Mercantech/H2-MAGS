using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Services.Mapping.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(JWTService jwtService, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpGet("login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = "https://hotel.mercantec.tech/api/GoogleAuth/response" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (result.Succeeded)
            {
                var claims = result.Principal.Identities.FirstOrDefault()
                    .Claims.Select(claim => new
                    {
                        claim.Issuer,
                        claim.OriginalIssuer,
                        claim.Type,
                        claim.Value
                    });

                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
                {
                    var token = _jwtService.GenerateJwtTokenGoogle(userId, userName);
                    return Ok(new { Token = token });
                }
            }

            return Unauthorized();
        }

        [HttpPost("login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                });

                var user = new
                {
                    payload.Email,
                    payload.Name,
                    payload.Picture
                };

                var token = _jwtService.GenerateJwtTokenGoogle(payload.Subject, payload.Name);
                return Ok(new { Token = token, User = user });
            }
            catch (InvalidJwtException ex)
            {
                return BadRequest(new { error = "Invalid Google token", details = ex.Message });
            }
        }
    }
     public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
} 