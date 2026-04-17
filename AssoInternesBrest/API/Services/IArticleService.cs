using AssoInternesBrest.API.DTOs.Articles;

namespace AssoInternesBrest.API.Services
{
    public interface IArticleService
    {
        Task<IEnumerable<ArticleDto>> GetAllPublishedAsync();
        Task<ArticleDto?> GetBySlugAsync(string slug);
        Task<ArticleDto> CreateAsync(CreateArticleDto dto, Guid authorId);
        Task<ArticleDto?> UpdateAsync(Guid id, UpdateArticleDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
