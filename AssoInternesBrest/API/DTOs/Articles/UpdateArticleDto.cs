using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Articles
{
    public class UpdateArticleDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [MaxLength(300)]
        public string? Excerpt { get; set; }

        public bool IsPublished { get; set; }

        public Guid? ImageId { get; set; }
    }
}
