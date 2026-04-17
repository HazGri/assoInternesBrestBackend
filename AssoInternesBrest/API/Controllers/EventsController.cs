using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
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
            IEnumerable<EventDto> events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<EventDto>> GetEventBySlug(string slug)
        {
            EventDto? eventDto = await _eventService.GetEventBySlugAsync(slug);
            if (eventDto == null)
                return NotFound();
            return Ok(eventDto);
        }

        [HttpPost]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto dto)
        {
            EventDto createdEvent = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(
                nameof(GetEventBySlug),
                new { slug = createdEvent.Slug },
                createdEvent
            );
        }

        [HttpGet("{id:guid}/registration")]
        public async Task<ActionResult<RegistrationDto>> GetRegistration(Guid id)
        {
            EventDto? eventDto = await _eventService.GetEventByIdAsync(id);
            if (eventDto == null)
                return NotFound();

            RegistrationDto registration = new()
            {
                HelloAssoUrl = eventDto.HelloAssoUrl,
                HasRegistration = eventDto.HelloAssoUrl != null
            };
            return Ok(registration);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<EventDto>> UpdateEvent(Guid id, UpdateEventDto dto)
        {
            EventDto? updated = await _eventService.UpdateEventAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult> DeleteEvent(Guid id)
        {
            bool isDeleted = await _eventService.DeleteEventAsync(id);
            if (isDeleted)
                return Ok();
            return NotFound();
        }
    }
}
