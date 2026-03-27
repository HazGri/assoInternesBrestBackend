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
            var events = await _repository.GetAllAsync();

            return _mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<EventDto?> GetEventBySlugAsync(string slug)
        {
            var entity = await _repository.GetBySlugAsync(slug);

            if (entity == null)
                return null;

            return _mapper.Map<EventDto>(entity);
        }

        public async Task<EventDto> CreateEventAsync(CreateEventDto dto)
        {
            var entity = _mapper.Map<Event>(dto);

            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);

            if (dto.EndDate.HasValue)
            {
                entity.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);
            }

            entity.Slug = await GenerateUniqueSlug(dto.Title);

            var created = await _repository.AddAsync(entity);

            return _mapper.Map<EventDto>(created);
        }

        public async Task UpdateEventAsync(Event entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            bool isDelete = await _repository.DeleteAsync(id);
            return isDelete;
        }

        private async Task<string> GenerateUniqueSlug(string title)
        {
            var baseSlug = SlugGenerator.Generate(title);
            var slug = baseSlug;

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
