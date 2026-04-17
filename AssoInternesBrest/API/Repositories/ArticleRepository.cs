using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class ArticleRepository(AppDbContext context) : IArticleRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<Article>> GetAllPublishedAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Image)
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article?> GetBySlugAsync(string slug)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Image)
                .FirstOrDefaultAsync(a => a.Slug == slug);
        }

        public async Task<Article?> GetByIdAsync(Guid id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Image)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Article> AddAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            Article? article = await _context.Articles.FindAsync(id);
            if (article == null)
                return false;
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Articles.AnyAsync(a => a.Slug == slug);
        }
    }
}
