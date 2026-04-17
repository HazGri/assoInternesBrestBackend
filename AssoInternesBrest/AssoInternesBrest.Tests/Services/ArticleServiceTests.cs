using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Mappings;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AssoInternesBrest.Tests.Services
{
    public class ArticleServiceTests
    {
        private readonly Mock<IArticleRepository> _repoMock;
        private readonly IArticleService _sut;
        private readonly Guid _authorId = Guid.NewGuid();

        public ArticleServiceTests()
        {
            _repoMock = new Mock<IArticleRepository>();

            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ArticleProfile>();
            }, NullLoggerFactory.Instance);
            IMapper mapper = config.CreateMapper();

            _sut = new ArticleService(_repoMock.Object, mapper);
        }

        [Fact]
        public async Task CreateAsync_GeneratesSlugAndSetsFields()
        {
            _repoMock.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Article>()))
                .ReturnsAsync((Article a) => a);

            CreateArticleDto dto = new CreateArticleDto
            {
                Title = "Mon Article",
                Content = "Contenu de l'article",
                IsPublished = true
            };

            ArticleDto result = await _sut.CreateAsync(dto, _authorId);

            Assert.Equal("Mon Article", result.Title);
            Assert.Equal("mon-article", result.Slug);
            Assert.Equal(_authorId, result.AuthorId);
        }

        [Fact]
        public async Task UpdateAsync_WithValidId_UpdatesFields()
        {
            Article existing = new Article
            {
                Id = Guid.NewGuid(),
                Title = "Old Title",
                Slug = "old-title",
                Content = "Old content",
                AuthorId = _authorId,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);

            UpdateArticleDto dto = new UpdateArticleDto
            {
                Title = "New Title",
                Content = "New content",
                IsPublished = true
            };

            ArticleDto? result = await _sut.UpdateAsync(existing.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("New Title", result.Title);
            Assert.True(result.IsPublished);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Article?)null);

            ArticleDto? result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateArticleDto
            {
                Title = "x",
                Content = "y"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            bool result = await _sut.DeleteAsync(Guid.NewGuid());

            Assert.True(result);
        }
    }
}
