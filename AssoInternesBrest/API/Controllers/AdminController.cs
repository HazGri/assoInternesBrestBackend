using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssoInternesBrest.API.DTOs.Admin;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController(
        IAuthService authService,
        IAppSettingService appSettingService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IAppSettingService _appSettingService = appSettingService;

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            IEnumerable<User> users = await _authService.GetAllUsersAsync();
            IEnumerable<UserDto> dtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
            });
            return Ok(dtos);
        }

        [HttpPost("users")]
        public async Task<ActionResult> CreateUser(CreateUserDto dto)
        {
            try
            {
                await _authService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName, dto.Role);
                return StatusCode(201);
            }
            catch (InvalidOperationException ex) when (ex.Message == "EMAIL_EXISTS")
            {
                return Conflict(new { message = "Un compte avec cet email existe déjà." });
            }
            catch (InvalidOperationException ex) when (ex.Message == "EMAIL_SEND_FAILED")
            {
                return StatusCode(502, new
                {
                    message = "Compte non créé : l'envoi de l'email d'invitation a échoué. Vérifiez la configuration SMTP."
                });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            string? sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (sub == null || !Guid.TryParse(sub, out Guid currentUserId))
                return Unauthorized();

            try
            {
                bool deleted = await _authService.DeleteUserAsync(id, currentUserId);
                if (!deleted)
                    return NotFound();
                return Ok();
            }
            catch (InvalidOperationException ex) when (ex.Message == "SELF_DELETE_FORBIDDEN")
            {
                return BadRequest(new
                {
                    message = "Vous ne pouvez pas supprimer votre propre compte."
                });
            }
        }

        [HttpGet("settings")]
        public async Task<ActionResult<IEnumerable<AppSetting>>> GetSettings()
        {
            IEnumerable<AppSetting> settings = await _appSettingService.GetAllAsync();
            return Ok(settings);
        }

        [HttpPut("settings/{key}")]
        public async Task<ActionResult> UpdateSetting(string key, UpdateSettingDto dto)
        {
            await _appSettingService.SetValueAsync(key, dto.Value);
            return Ok();
        }
    }
}
