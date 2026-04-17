using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class GuidePage
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
