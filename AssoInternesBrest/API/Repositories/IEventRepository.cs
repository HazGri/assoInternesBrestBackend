using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllAsync();

        Task<Event?> GetBySlugAsync(string slug);

        Task<Event> AddAsync(Event entity);

        Task UpdateAsync(Event entity);
        Task DeleteAsync(Guid id);
        Task<bool> SlugExistsAsync(string slug);
    }
}
