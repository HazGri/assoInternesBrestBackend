using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Utils;
using AutoMapper;

namespace AssoInternesBrest.API.Services
{
    public class ArticleService(IArticleRepository repository, IMapper mapper) : IArticleService
    {
        private readonly IArticleRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<ArticleDto>> GetAllPublishedAsync()
        {
            IEnumerable<Article> articles = await _repository.GetAllPublishedAsync();
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }

        public async Task<ArticleDto?> GetBySlugAsync(string slug)
        {
            Article? article = await _repository.GetBySlugAsync(slug);
            if (article == null)
                return null;
            return _mapper.Map<ArticleDto>(article);
        }

        public async Task<ArticleDto> CreateAsync(CreateArticleDto dto, Guid authorId)
        {
            Article article = _mapper.Map<Article>(dto);
            article.Id = Guid.NewGuid();
            article.AuthorId = authorId;
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            article.Slug = await GenerateUniqueSlugAsync(dto.Title);

            Article created = await _repository.AddAsync(article);
            return _mapper.Map<ArticleDto>(created);
        }

        public async Task<ArticleDto?> UpdateAsync(Guid id, UpdateArticleDto dto)
        {
            Article? article = await _repository.GetByIdAsync(id);
            if (article == null)
                return null;

            article.Title = dto.Title;
            article.Content = dto.Content;
            article.IsPublished = dto.IsPublished;
            article.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(article);
            return _mapper.Map<ArticleDto>(article);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
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
