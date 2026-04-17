using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class GuidePageRepository(AppDbContext context) : IGuidePageRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<GuidePage>> GetAllAsync()
        {
            return await _context.GuidePages.OrderBy(p => p.Title).ToListAsync();
        }

        public async Task<GuidePage?> GetBySlugAsync(string slug)
        {
            return await _context.GuidePages.FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<GuidePage> AddAsync(GuidePage page)
        {
            _context.GuidePages.Add(page);
            await _context.SaveChangesAsync();
            return page;
        }

        public async Task UpdateAsync(GuidePage page)
        {
            _context.GuidePages.Update(page);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(string slug)
        {
            GuidePage? page = await _context.GuidePages.FirstOrDefaultAsync(p => p.Slug == slug);
            if (page == null)
                return false;
            _context.GuidePages.Remove(page);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.GuidePages.AnyAsync(p => p.Slug == slug);
        }
    }
}
