namespace AssoInternesBrest.API.DTOs.Events
{
    public class EventDto
    {
        public string Title { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Location { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }
}
