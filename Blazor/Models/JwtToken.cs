using System.Text.Json;
using System.Text;

namespace Blazor.Models;

public class JwtToken
{
    public Dictionary<string, JsonElement> Header { get; set; } = new();
    public Dictionary<string, JsonElement> Payload { get; set; } = new();
    public string Signature { get; set; } = string.Empty;

    public static JwtToken? Decode(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var header = DecodeBase64Url(parts[0]);
            var payload = DecodeBase64Url(parts[1]);
            var signature = parts[2];

            var headerDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(header) ?? new();
            var payloadDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload) ?? new();

            return new JwtToken
            {
                Header = headerDict,
                Payload = payloadDict,
                Signature = signature
            };
        }
        catch
        {
            return null;
        }
    }

    private static string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        var padding = 4 - (base64.Length % 4);
        if (padding != 4)
            base64 += new string('=', padding);

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}

public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public DateTime? ExpirationTime { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? TokenId { get; set; }
    public string? UserId { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public static TokenValidationResult Validate(JwtToken? token)
    {
        if (token == null)
            return new TokenValidationResult { IsValid = false, ValidationErrors = { "Token kunne ikke dekodes" } };

        var result = new TokenValidationResult();
        var errors = new List<string>();

        try
        {
            // Tjek udløbsdato
            if (token.Payload.TryGetValue("exp", out var expElement))
            {
                if (expElement.TryGetInt64(out var exp))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    result.ExpirationTime = expirationTime;
                    
                    if (expirationTime <= DateTime.UtcNow)
                    {
                        errors.Add("Token er udløbet");
                    }
                }
            }

            // Tjek udgivelsesdato
            if (token.Payload.TryGetValue("iat", out var iatElement))
            {
                if (iatElement.TryGetInt64(out var iat))
                {
                    result.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;
                }
            }

            // Hent andre claims
            if (token.Payload.TryGetValue("iss", out var issElement))
                result.Issuer = issElement.GetString();

            if (token.Payload.TryGetValue("aud", out var audElement))
                result.Audience = audElement.GetString();

            if (token.Payload.TryGetValue("jti", out var jtiElement))
                result.TokenId = jtiElement.GetString();

            if (token.Payload.TryGetValue("sub", out var subElement))
                result.UserId = subElement.GetString();

            // Tjek algoritme
            if (token.Header.TryGetValue("alg", out var algElement))
            {
                var algorithm = algElement.GetString();
                if (string.IsNullOrEmpty(algorithm))
                {
                    errors.Add("Manglende algoritme i header");
                }
            }

            result.IsValid = errors.Count == 0;
            result.ValidationErrors = errors;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ValidationErrors.Add($"Validering fejlede: {ex.Message}");
        }

        return result;
    }
}
