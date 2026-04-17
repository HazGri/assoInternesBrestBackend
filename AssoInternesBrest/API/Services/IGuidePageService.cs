using AssoInternesBrest.API.DTOs.GuidePages;

namespace AssoInternesBrest.API.Services
{
    public interface IGuidePageService
    {
        Task<IEnumerable<GuidePageDto>> GetAllAsync();
        Task<GuidePageDto?> GetBySlugAsync(string slug);
        Task<GuidePageDto> CreateAsync(CreateGuidePageDto dto);
        Task<GuidePageDto?> UpdateAsync(string slug, UpdateGuidePageDto dto);
        Task<bool> DeleteAsync(string slug);
    }
}
