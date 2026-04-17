using AssoInternesBrest.API.DTOs.GuidePages;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Utils;
using AutoMapper;

namespace AssoInternesBrest.API.Services
{
    public class GuidePageService(IGuidePageRepository repository, IMapper mapper) : IGuidePageService
    {
        private readonly IGuidePageRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<GuidePageDto>> GetAllAsync()
        {
            IEnumerable<GuidePage> pages = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<GuidePageDto>>(pages);
        }

        public async Task<GuidePageDto?> GetBySlugAsync(string slug)
        {
            GuidePage? page = await _repository.GetBySlugAsync(slug);
            if (page == null)
                return null;
            return _mapper.Map<GuidePageDto>(page);
        }

        public async Task<GuidePageDto> CreateAsync(CreateGuidePageDto dto)
        {
            GuidePage page = _mapper.Map<GuidePage>(dto);
            page.Id = Guid.NewGuid();
            page.Slug = await GenerateUniqueSlugAsync(dto.Title);
            page.UpdatedAt = DateTime.UtcNow;
            GuidePage created = await _repository.AddAsync(page);
            return _mapper.Map<GuidePageDto>(created);
        }

        public async Task<GuidePageDto?> UpdateAsync(string slug, UpdateGuidePageDto dto)
        {
            GuidePage? page = await _repository.GetBySlugAsync(slug);
            if (page == null)
                return null;
            page.Title = dto.Title;
            page.Content = dto.Content;
            page.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(page);
            return _mapper.Map<GuidePageDto>(page);
        }

        public async Task<bool> DeleteAsync(string slug)
        {
            return await _repository.DeleteAsync(slug);
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
