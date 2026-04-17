using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssoInternesBrest.API.DTOs.Auth;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            string? token = await _authService.LoginAsync(dto.Email, dto.Password);
            if (token == null)
                return Unauthorized();
            return Ok(new LoginResponseDto { Token = token });
        }

        [HttpPost("activate")]
        public async Task<ActionResult> Activate(ActivateDto dto)
        {
            bool success = await _authService.ActivateAsync(dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest("Token invalide ou expiré.");
            return Ok();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
        {
            string? sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (sub == null || !Guid.TryParse(sub, out Guid userId))
                return Unauthorized();

            bool success = await _authService.ChangePasswordAsync(
                userId, dto.CurrentPassword, dto.NewPassword);

            if (!success)
                return BadRequest(new { message = "Mot de passe actuel incorrect." });

            return Ok();
        }
    }
}
