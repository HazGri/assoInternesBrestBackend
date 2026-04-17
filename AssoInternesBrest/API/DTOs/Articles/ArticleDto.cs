namespace AssoInternesBrest.API.DTOs.Articles
{
    public class ArticleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Excerpt { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public Guid? ImageId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
