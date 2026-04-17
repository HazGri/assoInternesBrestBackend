using AssoInternesBrest.API.DTOs.Contact;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController(IEmailService emailService, IAppSettingService appSettingService) : ControllerBase
    {
        private readonly IEmailService _emailService = emailService;
        private readonly IAppSettingService _appSettingService = appSettingService;

        [HttpPost]
        public async Task<ActionResult> SendContact(ContactDto dto)
        {
            string? contactEmail = await _appSettingService.GetValueAsync("contact_email");
            contactEmail ??= "contact@asso-internes-brest.fr";

            try
            {
                string subject = $"Message de {dto.Name} via le site";
                string body = $"De : {dto.Name} <{dto.Email}>\n\n{dto.Message}";
                await _emailService.SendAsync(contactEmail, subject, body);
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { message = "Erreur lors de l'envoi de l'email." });
            }
        }
    }
}
