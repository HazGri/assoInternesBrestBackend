using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IArticleRepository
    {
        Task<IEnumerable<Article>> GetAllPublishedAsync();
        Task<Article?> GetBySlugAsync(string slug);
        Task<Article?> GetByIdAsync(Guid id);
        Task<Article> AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SlugExistsAsync(string slug);
    }
}
