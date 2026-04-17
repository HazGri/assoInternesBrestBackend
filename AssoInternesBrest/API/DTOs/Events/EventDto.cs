namespace AssoInternesBrest.API.DTOs.Events
{
    public class EventDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Location { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? HelloAssoUrl { get; set; }
    }
}
