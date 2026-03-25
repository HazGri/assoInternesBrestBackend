using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Event
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Location { get; set; } = null!;

        public int? Capacity { get; set; }

        public Guid? ImageId { get; set; }
        public Image? Image { get; set; }

        public bool IsPublished { get; set; } = true;

        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
