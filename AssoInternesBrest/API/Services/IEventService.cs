using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetAllEventsAsync();

        Task<EventDto?> GetEventBySlugAsync(string slug);

        Task<EventDto> CreateEventAsync(CreateEventDto entity);

        Task UpdateEventAsync(Event entity);

        Task<bool> DeleteEventAsync(Guid id);
    }
}
