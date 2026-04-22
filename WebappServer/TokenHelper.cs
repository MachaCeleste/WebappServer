using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace WebappServer
{
    internal class TokenHelper
    {
        private string _secretKey = "default";

        public TokenHelper(string secretKey) => _secretKey = secretKey;

        public (string AccessToken, string RefreshToken) GrantTokens(string? existingUserId = null)
        {
            string userId = existingUserId ?? Guid.NewGuid().ToString();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", userId) }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            var refreshDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", userId) }),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            return (
                handler.WriteToken(handler.CreateToken(accessDescriptor)),
                handler.WriteToken(handler.CreateToken(refreshDescriptor))
                );
        }

        public string? ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var claims = handler.ValidateToken(token, validations, out _);
                return claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims.FindFirst("sub")?.Value;
            }
            catch { return null; }
        }
    }
}
