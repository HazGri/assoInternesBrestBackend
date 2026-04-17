using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Events
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;

        public int? Capacity { get; set; }

        public Guid? ImageId { get; set; }

        public bool IsPublished { get; set; }

        public string? HelloAssoUrl { get; set; }
    }
}
