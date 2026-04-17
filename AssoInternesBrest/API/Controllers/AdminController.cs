using AssoInternesBrest.API.DTOs.Admin;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController(IAuthService authService, IAppSettingService appSettingService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IAppSettingService _appSettingService = appSettingService;

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
                return Conflict("Un compte avec cet email existe déjà.");
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
