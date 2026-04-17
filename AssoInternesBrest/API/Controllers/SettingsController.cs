using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController(IAppSettingService appSettingService) : ControllerBase
    {
        private static readonly HashSet<string> PublicKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "contact_email",
        };

        private readonly IAppSettingService _appSettingService = appSettingService;

        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetPublic()
        {
            IEnumerable<AppSetting> all = await _appSettingService.GetAllAsync();
            Dictionary<string, string> publicSettings = all
                .Where(s => PublicKeys.Contains(s.Key))
                .ToDictionary(s => s.Key, s => s.Value);
            return Ok(publicSettings);
        }
    }
}
