using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            var events = await _eventService.GetAllEventsAsync();

            return Ok(events);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<EventDto>> GetEventBySlug(string slug)
        {
            var eventDto = await _eventService.GetEventBySlugAsync(slug);

            if (eventDto == null)
                return NotFound();

            return Ok(eventDto);
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto dto)
        {
            var createdEvent = await _eventService.CreateEventAsync(dto);

            return CreatedAtAction(
                nameof(GetEventBySlug),
                new { slug = createdEvent.Slug },
                createdEvent
            );
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEvent(Guid id)
        {
            var isDelete = await _eventService.DeleteEventAsync(id);
            if (isDelete)
            {
                return Ok(200);
            }
            return NoContent();
        }
    }
}
