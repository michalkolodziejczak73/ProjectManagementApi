using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.DTOs.Auth;
using ProjectManagementApi.Models;
using ProjectManagementApi.Services;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }
        /// <summary>
        /// Rejestruje nowego użytkownika i zwraca token JWT.
        /// </summary>
        /// <param name="registerDto">Dane rejestracyjne użytkownika.</param>
        /// <returns>Dane użytkownika oraz token JWT.</returns>
        /// <response code="200">Użytkownik został utworzony.</response>
        /// <response code="400">Dane rejestracyjne są nieprawidłowe.</response>
        /// <response code="409">Użytkownik o podanym adresie e-mail już istnieje.</response>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(
            RegisterDto registerDto)
        {
            var normalizedEmail = registerDto.Email.Trim().ToLowerInvariant();

            var existingUser =
                await _userManager.FindByEmailAsync(normalizedEmail);

            if (existingUser is not null)
            {
                return Conflict(new
                {
                    message = "Użytkownik z tym adresem e-mail już istnieje."
                });
            }

            var user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail
            };

            var result = await _userManager.CreateAsync(
                user,
                registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(error => error.Description)
                    .ToArray();

                return BadRequest(new
                {
                    message = "Nie udało się utworzyć użytkownika.",
                    errors
                });
            }

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Token = _tokenService.CreateToken(user),
                ExpiresAt = _tokenService.GetExpirationDate()
            };

            return Ok(response);
        }
        /// <summary>
        /// Loguje użytkownika i zwraca token JWT.
        /// </summary>
        /// <param name="loginDto">Adres e-mail i hasło użytkownika.</param>
        /// <returns>Dane użytkownika oraz token JWT.</returns>
        /// <response code="200">Logowanie zakończyło się powodzeniem.</response>
        /// <response code="400">Przesłane dane są nieprawidłowe.</response>
        /// <response code="401">Adres e-mail lub hasło są nieprawidłowe.</response>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(
            LoginDto loginDto)
        {
            var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user is null)
            {
                return Unauthorized(new
                {
                    message = "Nieprawidłowy adres e-mail lub hasło."
                });
            }

            var passwordIsValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    loginDto.Password);

            if (!passwordIsValid)
            {
                return Unauthorized(new
                {
                    message = "Nieprawidłowy adres e-mail lub hasło."
                });
            }

            var response = new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Token = _tokenService.CreateToken(user),
                ExpiresAt = _tokenService.GetExpirationDate()
            };

            return Ok(response);
        }
    }
}