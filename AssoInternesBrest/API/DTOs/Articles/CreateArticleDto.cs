using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Articles
{
    public class CreateArticleDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public bool IsPublished { get; set; } = false;
    }
}
