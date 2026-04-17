using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.GuidePages
{
    public class UpdateGuidePageDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }
}
