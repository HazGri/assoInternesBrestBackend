using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Article
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Excerpt { get; set; }
        public Guid AuthorId { get; set; }
        public User? Author { get; set; }
        public Guid? ImageId { get; set; }
        public Image? Image { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
