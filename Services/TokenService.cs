using Microsoft.IdentityModel.Tokens;
using ProjectManagementApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectManagementApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(ApplicationUser user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Nie skonfigurowano klucza JWT.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Nie skonfigurowano wystawcy tokenu JWT.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Nie skonfigurowano odbiorcy tokenu JWT.");

            var expiresAt = GetExpirationDate();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public DateTime GetExpirationDate()
        {
            var expirationText =
                _configuration["Jwt:ExpiresInMinutes"];

            if (!int.TryParse(expirationText, out var minutes))
            {
                minutes = 60;
            }

            return DateTime.UtcNow.AddMinutes(minutes);
        }
    }
}