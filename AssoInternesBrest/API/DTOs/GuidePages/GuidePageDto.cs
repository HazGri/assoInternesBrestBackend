namespace AssoInternesBrest.API.DTOs.GuidePages
{
    public class GuidePageDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
