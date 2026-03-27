using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class EventRepository(AppDbContext context) : IEventRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context
                .Events.Include(e => e.Image)
                .Where(e => e.IsPublished)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<Event?> GetBySlugAsync(string slug)
        {
            return await _context
                .Events.Include(e => e.Image)
                .FirstOrDefaultAsync(e => e.Slug == slug);
        }

        public async Task<Event> AddAsync(Event entity)
        {
            _context.Events.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task UpdateAsync(Event entity)
        {
            _context.Events.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity != null)
            {
                _context.Events.Remove(eventEntity);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Events.AnyAsync(e => e.Slug == slug);
        }
    }
}
