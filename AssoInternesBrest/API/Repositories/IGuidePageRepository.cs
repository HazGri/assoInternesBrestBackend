using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IGuidePageRepository
    {
        Task<IEnumerable<GuidePage>> GetAllAsync();
        Task<GuidePage?> GetBySlugAsync(string slug);
        Task<GuidePage> AddAsync(GuidePage page);
        Task UpdateAsync(GuidePage page);
        Task<bool> DeleteAsync(string slug);
        Task<bool> SlugExistsAsync(string slug);
    }
}
