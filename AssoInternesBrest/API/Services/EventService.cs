using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Utils;
using AutoMapper;

namespace AssoInternesBrest.API.Services
{
    public class EventService(IEventRepository repository, IMapper mapper) : IEventService
    {
        private readonly IEventRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            IEnumerable<Event> events = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<EventDto?> GetEventBySlugAsync(string slug)
        {
            Event? entity = await _repository.GetBySlugAsync(slug);
            if (entity == null)
                return null;
            return _mapper.Map<EventDto>(entity);
        }

        public async Task<EventDto> CreateEventAsync(CreateEventDto dto)
        {
            Event entity = _mapper.Map<Event>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            if (dto.EndDate.HasValue)
                entity.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);
            entity.Slug = await GenerateUniqueSlugAsync(dto.Title);
            Event created = await _repository.AddAsync(entity);
            return _mapper.Map<EventDto>(created);
        }

        public async Task<EventDto?> UpdateEventAsync(Guid id, UpdateEventDto dto)
        {
            Event? entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            entity.EndDate = dto.EndDate.HasValue ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) : null;
            entity.Location = dto.Location;
            entity.Capacity = dto.Capacity;
            entity.ImageId = dto.ImageId;
            entity.IsPublished = dto.IsPublished;
            entity.HelloAssoUrl = dto.HelloAssoUrl;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            return _mapper.Map<EventDto>(entity);
        }

        public async Task UpdateEventAsync(Event entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private async Task<string> GenerateUniqueSlugAsync(string title)
        {
            string baseSlug = SlugGenerator.Generate(title);
            string slug = baseSlug;
            int counter = 2;
            while (await _repository.SlugExistsAsync(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            return slug;
        }
    }
}
