using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Auth
{
    public class ActivateDto
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = null!;
    }
}
