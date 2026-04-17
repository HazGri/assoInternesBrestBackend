# Groupe B — Contenu Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implémenter PUT /api/events/{id} (#9), entité Article + CRUD (#10/#11), BureauMember + CRUD (#12), GuidePage + CRUD (#15).

**Architecture:** Même pattern que les entités existantes : Entity → Migration → Repository (interface+impl) → Service (interface+impl+tests) → AutoMapper Profile → Controller. Chaque entité a son propre fichier de profil AutoMapper.

**Tech Stack:** .NET 10, EF Core, AutoMapper, xUnit, Moq, PostgreSQL (Neon)

**Conventions:** Jamais de `var` — types explicites partout. Primary constructors C# 12. DI Scoped.

---

## Structure des fichiers

| Fichier | Action |
|---------|--------|
| `API/Repositories/IEventRepository.cs` | Modifier — ajout `GetByIdAsync` |
| `API/Repositories/EventRepository.cs` | Modifier — impl `GetByIdAsync` |
| `API/DTOs/Events/UpdateEventDto.cs` | Créer |
| `API/Services/IEventService.cs` | Modifier — ajout `UpdateEventAsync(Guid, UpdateEventDto)` |
| `API/Services/EventService.cs` | Modifier — impl + fix `var` → types explicites |
| `API/Controllers/EventsController.cs` | Modifier — PUT endpoint |
| `API/Entities/Article.cs` | Créer |
| `API/Data/AppDbContext.cs` | Modifier — DbSet Article, BureauMember, GuidePage |
| `API/Repositories/IArticleRepository.cs` | Créer |
| `API/Repositories/ArticleRepository.cs` | Créer |
| `API/Services/IArticleService.cs` | Créer |
| `API/Services/ArticleService.cs` | Créer |
| `API/Mappings/ArticleProfile.cs` | Créer |
| `API/DTOs/Articles/` | Créer (3 DTOs) |
| `API/Controllers/ArticlesController.cs` | Créer |
| `API/Entities/BureauMember.cs` | Créer |
| `API/Repositories/IBureauMemberRepository.cs` | Créer |
| `API/Repositories/BureauMemberRepository.cs` | Créer |
| `API/Services/IBureauMemberService.cs` | Créer |
| `API/Services/BureauMemberService.cs` | Créer |
| `API/Mappings/BureauMemberProfile.cs` | Créer |
| `API/DTOs/BureauMembers/` | Créer (3 DTOs) |
| `API/Controllers/BureauMembersController.cs` | Créer |
| `API/Entities/GuidePage.cs` | Créer |
| `API/Repositories/IGuidePageRepository.cs` | Créer |
| `API/Repositories/GuidePageRepository.cs` | Créer |
| `API/Services/IGuidePageService.cs` | Créer |
| `API/Services/GuidePageService.cs` | Créer |
| `API/Mappings/GuidePageProfile.cs` | Créer |
| `API/DTOs/GuidePages/` | Créer (3 DTOs) |
| `API/Controllers/GuideController.cs` | Créer |
| `Program.cs` | Modifier — enregistrement de tous les nouveaux services |
| `AssoInternesBrest.Tests/Services/ArticleServiceTests.cs` | Créer |

---

### Task 1: PUT /api/events/{id} (Ticket #9)

**Files:**
- Modify: `API/Repositories/IEventRepository.cs`
- Modify: `API/Repositories/EventRepository.cs`
- Create: `API/DTOs/Events/UpdateEventDto.cs`
- Modify: `API/Services/IEventService.cs`
- Modify: `API/Services/EventService.cs`
- Modify: `API/Controllers/EventsController.cs`

- [ ] **Step 1: Ajouter `GetByIdAsync` à `IEventRepository`**

Lire `API/Repositories/IEventRepository.cs` et ajouter après `GetBySlugAsync` :

```csharp
Task<Event?> GetByIdAsync(Guid id);
```

- [ ] **Step 2: Implémenter dans `EventRepository`**

Ajouter dans `API/Repositories/EventRepository.cs` après `GetBySlugAsync` :

```csharp
public async Task<Event?> GetByIdAsync(Guid id)
{
    return await _context.Events.Include(e => e.Image).FirstOrDefaultAsync(e => e.Id == id);
}
```

- [ ] **Step 3: Créer `API/DTOs/Events/UpdateEventDto.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Events
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;

        public int? Capacity { get; set; }

        public Guid? ImageId { get; set; }

        public bool IsPublished { get; set; }
    }
}
```

- [ ] **Step 4: Ajouter méthode à `IEventService`**

Dans `API/Services/IEventService.cs`, ajouter après `CreateEventAsync` :

```csharp
Task<EventDto?> UpdateEventAsync(Guid id, UpdateEventDto dto);
```

- [ ] **Step 5: Implémenter dans `EventService` et fixer les `var`**

Remplacer le contenu de `API/Services/EventService.cs` par :

```csharp
using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Utils;
using AutoMapper;

namespace AssoInternesBrest.API.Services
{
    public class EventService(IEventRepository repository, IMapper mapper) : IEventService
    {
        private readonly IEventRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
        {
            IEnumerable<Event> events = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<EventDto?> GetEventBySlugAsync(string slug)
        {
            Event? entity = await _repository.GetBySlugAsync(slug);
            if (entity == null)
                return null;
            return _mapper.Map<EventDto>(entity);
        }

        public async Task<EventDto> CreateEventAsync(CreateEventDto dto)
        {
            Event entity = _mapper.Map<Event>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            if (dto.EndDate.HasValue)
                entity.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);
            entity.Slug = await GenerateUniqueSlugAsync(dto.Title);
            Event created = await _repository.AddAsync(entity);
            return _mapper.Map<EventDto>(created);
        }

        public async Task<EventDto?> UpdateEventAsync(Guid id, UpdateEventDto dto)
        {
            Event? entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
            entity.EndDate = dto.EndDate.HasValue ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) : null;
            entity.Location = dto.Location;
            entity.Capacity = dto.Capacity;
            entity.ImageId = dto.ImageId;
            entity.IsPublished = dto.IsPublished;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            return _mapper.Map<EventDto>(entity);
        }

        public async Task UpdateEventAsync(Event entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteEventAsync(Guid id)
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
```

- [ ] **Step 6: Ajouter le PUT endpoint dans `EventsController`**

Dans `API/Controllers/EventsController.cs`, ajouter avant la méthode `DeleteEvent` :

```csharp
[HttpPut("{id}")]
[Authorize(Policy = "BureauOrAdmin")]
public async Task<ActionResult<EventDto>> UpdateEvent(Guid id, UpdateEventDto dto)
{
    EventDto? updated = await _eventService.UpdateEventAsync(id, dto);
    if (updated == null)
        return NotFound();
    return Ok(updated);
}
```

Et ajouter le using manquant en haut si pas déjà présent :
```csharp
using AssoInternesBrest.API.DTOs.Events;
```

- [ ] **Step 7: Build**

```bash
dotnet build
```
Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
git add API/Repositories/IEventRepository.cs API/Repositories/EventRepository.cs API/DTOs/Events/UpdateEventDto.cs API/Services/IEventService.cs API/Services/EventService.cs API/Controllers/EventsController.cs
git commit -m "feat: add PUT /api/events/{id} endpoint (#9)"
```

---

### Task 2: Article entity + migration (Ticket #10)

**Files:**
- Create: `API/Entities/Article.cs`
- Modify: `API/Data/AppDbContext.cs`

- [ ] **Step 1: Créer `API/Entities/Article.cs`**

```csharp
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Article
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public User? Author { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

- [ ] **Step 2: Ajouter DbSet dans `AppDbContext`**

Dans `API/Data/AppDbContext.cs`, ajouter `DbSet<Article>` :

```csharp
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
    }
}
```

- [ ] **Step 3: Créer et appliquer la migration**

```bash
dotnet ef migrations add AddArticle
dotnet ef database update
```

Expected: migration créée et appliquée.

- [ ] **Step 4: Build**

```bash
dotnet build
```

- [ ] **Step 5: Commit**

```bash
git add API/Entities/Article.cs API/Data/AppDbContext.cs Migrations/
git commit -m "feat: add Article entity and migration (#10)"
```

---

### Task 3: Article CRUD complet (Ticket #11)

**Files:**
- Create: `API/Repositories/IArticleRepository.cs`
- Create: `API/Repositories/ArticleRepository.cs`
- Create: `API/DTOs/Articles/ArticleDto.cs`
- Create: `API/DTOs/Articles/CreateArticleDto.cs`
- Create: `API/DTOs/Articles/UpdateArticleDto.cs`
- Create: `API/Mappings/ArticleProfile.cs`
- Create: `API/Services/IArticleService.cs`
- Create: `API/Services/ArticleService.cs`
- Create: `API/Controllers/ArticlesController.cs`
- Create: `AssoInternesBrest.Tests/Services/ArticleServiceTests.cs`
- Modify: `Program.cs`

- [ ] **Step 1: Créer `API/Repositories/IArticleRepository.cs`**

```csharp
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
```

- [ ] **Step 2: Créer `API/Repositories/ArticleRepository.cs`**

```csharp
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
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article?> GetBySlugAsync(string slug)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Slug == slug);
        }

        public async Task<Article?> GetByIdAsync(Guid id)
        {
            return await _context.Articles
                .Include(a => a.Author)
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
```

- [ ] **Step 3: Créer les DTOs Article**

`API/DTOs/Articles/ArticleDto.cs` :
```csharp
namespace AssoInternesBrest.API.DTOs.Articles
{
    public class ArticleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

`API/DTOs/Articles/CreateArticleDto.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Articles
{
    public class CreateArticleDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public bool IsPublished { get; set; } = false;
    }
}
```

`API/DTOs/Articles/UpdateArticleDto.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Articles
{
    public class UpdateArticleDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public bool IsPublished { get; set; }
    }
}
```

- [ ] **Step 4: Créer `API/Mappings/ArticleProfile.cs`**

```csharp
using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>()
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.Author != null
                        ? $"{src.Author.FirstName} {src.Author.LastName}"
                        : ""));

            CreateMap<CreateArticleDto, Article>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
```

- [ ] **Step 5: Créer `API/Services/IArticleService.cs`**

```csharp
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
```

- [ ] **Step 6: Écrire les tests échouants**

Créer `AssoInternesBrest.Tests/Services/ArticleServiceTests.cs` :

```csharp
using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using AutoMapper;
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
                cfg.AddProfile<AssoInternesBrest.API.Mappings.ArticleProfile>();
            });
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
```

- [ ] **Step 7: Vérifier que les tests échouent**

```bash
dotnet build AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj
```
Expected: erreur CS0246 — `ArticleService` n'existe pas encore.

- [ ] **Step 8: Créer `API/Services/ArticleService.cs`**

```csharp
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
```

- [ ] **Step 9: Lancer les tests**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj --verbosity normal
```
Expected: 17/17 PASS (13 existants + 4 ArticleServiceTests).

- [ ] **Step 10: Créer `API/Controllers/ArticlesController.cs`**

```csharp
using System.IdentityModel.Tokens.Jwt;
using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/articles")]
    public class ArticlesController(IArticleService articleService) : ControllerBase
    {
        private readonly IArticleService _articleService = articleService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAll()
        {
            IEnumerable<ArticleDto> articles = await _articleService.GetAllPublishedAsync();
            return Ok(articles);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<ArticleDto>> GetBySlug(string slug)
        {
            ArticleDto? article = await _articleService.GetBySlugAsync(slug);
            if (article == null)
                return NotFound();
            return Ok(article);
        }

        [HttpPost]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<ArticleDto>> Create(CreateArticleDto dto)
        {
            string? authorIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (authorIdStr == null || !Guid.TryParse(authorIdStr, out Guid authorId))
                return Unauthorized();

            ArticleDto created = await _articleService.CreateAsync(dto, authorId);
            return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<ArticleDto>> Update(Guid id, UpdateArticleDto dto)
        {
            ArticleDto? updated = await _articleService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await _articleService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return Ok();
        }
    }
}
```

- [ ] **Step 11: Enregistrer dans Program.cs + AutoMapper**

Dans `Program.cs`, ajouter après `builder.Services.AddScoped<IAuthService, AuthService>();` :

```csharp
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();
```

Dans `Program.cs`, modifier la ligne AddAutoMapper pour inclure ArticleProfile :

```csharp
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<EventProfile>();
    cfg.AddProfile<ArticleProfile>();
});
```

- [ ] **Step 12: Build**

```bash
dotnet build
```

- [ ] **Step 13: Commit**

```bash
git add API/Repositories/IArticleRepository.cs API/Repositories/ArticleRepository.cs API/DTOs/Articles/ API/Mappings/ArticleProfile.cs API/Services/IArticleService.cs API/Services/ArticleService.cs API/Controllers/ArticlesController.cs AssoInternesBrest.Tests/Services/ArticleServiceTests.cs Program.cs
git commit -m "feat: add Article CRUD with TDD (#10 #11)"
```

---

### Task 3: BureauMember entity + CRUD complet (Ticket #12)

**Files:**
- Create: `API/Entities/BureauMember.cs`
- Modify: `API/Data/AppDbContext.cs`
- Create: `API/Repositories/IBureauMemberRepository.cs`
- Create: `API/Repositories/BureauMemberRepository.cs`
- Create: `API/DTOs/BureauMembers/BureauMemberDto.cs`
- Create: `API/DTOs/BureauMembers/CreateBureauMemberDto.cs`
- Create: `API/DTOs/BureauMembers/UpdateBureauMemberDto.cs`
- Create: `API/Mappings/BureauMemberProfile.cs`
- Create: `API/Services/IBureauMemberService.cs`
- Create: `API/Services/BureauMemberService.cs`
- Create: `API/Controllers/BureauMembersController.cs`
- Modify: `Program.cs`

- [ ] **Step 1: Créer `API/Entities/BureauMember.cs`**

```csharp
namespace AssoInternesBrest.API.Entities
{
    public class BureauMember
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
```

- [ ] **Step 2: Ajouter DbSet dans AppDbContext**

Ajouter `public DbSet<BureauMember> BureauMembers { get; set; }` dans `AppDbContext`.

- [ ] **Step 3: Migration**

```bash
dotnet ef migrations add AddBureauMember
dotnet ef database update
```

- [ ] **Step 4: Créer `IBureauMemberRepository`**

```csharp
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IBureauMemberRepository
    {
        Task<IEnumerable<BureauMember>> GetAllOrderedAsync();
        Task<BureauMember?> GetByIdAsync(Guid id);
        Task<BureauMember> AddAsync(BureauMember member);
        Task UpdateAsync(BureauMember member);
        Task<bool> DeleteAsync(Guid id);
    }
}
```

- [ ] **Step 5: Créer `BureauMemberRepository`**

```csharp
using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class BureauMemberRepository(AppDbContext context) : IBureauMemberRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<BureauMember>> GetAllOrderedAsync()
        {
            return await _context.BureauMembers.OrderBy(b => b.DisplayOrder).ToListAsync();
        }

        public async Task<BureauMember?> GetByIdAsync(Guid id)
        {
            return await _context.BureauMembers.FindAsync(id);
        }

        public async Task<BureauMember> AddAsync(BureauMember member)
        {
            _context.BureauMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task UpdateAsync(BureauMember member)
        {
            _context.BureauMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            BureauMember? member = await _context.BureauMembers.FindAsync(id);
            if (member == null)
                return false;
            _context.BureauMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
```

- [ ] **Step 6: Créer les DTOs BureauMember**

`API/DTOs/BureauMembers/BureauMemberDto.cs` :
```csharp
namespace AssoInternesBrest.API.DTOs.BureauMembers
{
    public class BureauMemberDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
```

`API/DTOs/BureauMembers/CreateBureauMemberDto.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.BureauMembers
{
    public class CreateBureauMemberDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        public string Role { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
```

`API/DTOs/BureauMembers/UpdateBureauMemberDto.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.BureauMembers
{
    public class UpdateBureauMemberDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        public string Role { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
```

- [ ] **Step 7: Créer `API/Mappings/BureauMemberProfile.cs`**

```csharp
using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class BureauMemberProfile : Profile
    {
        public BureauMemberProfile()
        {
            CreateMap<BureauMember, BureauMemberDto>();
            CreateMap<CreateBureauMemberDto, BureauMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
```

- [ ] **Step 8: Créer `API/Services/IBureauMemberService.cs`**

```csharp
using AssoInternesBrest.API.DTOs.BureauMembers;

namespace AssoInternesBrest.API.Services
{
    public interface IBureauMemberService
    {
        Task<IEnumerable<BureauMemberDto>> GetAllAsync();
        Task<BureauMemberDto> CreateAsync(CreateBureauMemberDto dto);
        Task<BureauMemberDto?> UpdateAsync(Guid id, UpdateBureauMemberDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
```

- [ ] **Step 9: Créer `API/Services/BureauMemberService.cs`**

```csharp
using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AutoMapper;

namespace AssoInternesBrest.API.Services
{
    public class BureauMemberService(IBureauMemberRepository repository, IMapper mapper) : IBureauMemberService
    {
        private readonly IBureauMemberRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<BureauMemberDto>> GetAllAsync()
        {
            IEnumerable<BureauMember> members = await _repository.GetAllOrderedAsync();
            return _mapper.Map<IEnumerable<BureauMemberDto>>(members);
        }

        public async Task<BureauMemberDto> CreateAsync(CreateBureauMemberDto dto)
        {
            BureauMember member = _mapper.Map<BureauMember>(dto);
            member.Id = Guid.NewGuid();
            BureauMember created = await _repository.AddAsync(member);
            return _mapper.Map<BureauMemberDto>(created);
        }

        public async Task<BureauMemberDto?> UpdateAsync(Guid id, UpdateBureauMemberDto dto)
        {
            BureauMember? member = await _repository.GetByIdAsync(id);
            if (member == null)
                return null;
            member.FirstName = dto.FirstName;
            member.LastName = dto.LastName;
            member.Role = dto.Role;
            member.Email = dto.Email;
            member.DisplayOrder = dto.DisplayOrder;
            await _repository.UpdateAsync(member);
            return _mapper.Map<BureauMemberDto>(member);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
```

- [ ] **Step 10: Créer `API/Controllers/BureauMembersController.cs`**

```csharp
using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/bureau")]
    public class BureauMembersController(IBureauMemberService bureauMemberService) : ControllerBase
    {
        private readonly IBureauMemberService _bureauMemberService = bureauMemberService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BureauMemberDto>>> GetAll()
        {
            IEnumerable<BureauMemberDto> members = await _bureauMemberService.GetAllAsync();
            return Ok(members);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<BureauMemberDto>> Create(CreateBureauMemberDto dto)
        {
            BureauMemberDto created = await _bureauMemberService.CreateAsync(dto);
            return StatusCode(201, created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<BureauMemberDto>> Update(Guid id, UpdateBureauMemberDto dto)
        {
            BureauMemberDto? updated = await _bureauMemberService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await _bureauMemberService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return Ok();
        }
    }
}
```

- [ ] **Step 11: Enregistrer dans Program.cs**

Ajouter après `builder.Services.AddScoped<IArticleService, ArticleService>();` :

```csharp
builder.Services.AddScoped<IBureauMemberRepository, BureauMemberRepository>();
builder.Services.AddScoped<IBureauMemberService, BureauMemberService>();
```

Ajouter `BureauMemberProfile` dans AddAutoMapper :

```csharp
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<EventProfile>();
    cfg.AddProfile<ArticleProfile>();
    cfg.AddProfile<BureauMemberProfile>();
});
```

- [ ] **Step 12: Build**

```bash
dotnet build
```

- [ ] **Step 13: Commit**

```bash
git add API/Entities/BureauMember.cs API/Data/AppDbContext.cs Migrations/ API/Repositories/IBureauMemberRepository.cs API/Repositories/BureauMemberRepository.cs API/DTOs/BureauMembers/ API/Mappings/BureauMemberProfile.cs API/Services/IBureauMemberService.cs API/Services/BureauMemberService.cs API/Controllers/BureauMembersController.cs Program.cs
git commit -m "feat: add BureauMember entity and CRUD (#12)"
```

---

### Task 4: GuidePage entity + CRUD complet (Ticket #15)

**Files:**
- Create: `API/Entities/GuidePage.cs`
- Modify: `API/Data/AppDbContext.cs`
- Create: `API/Repositories/IGuidePageRepository.cs`
- Create: `API/Repositories/GuidePageRepository.cs`
- Create: `API/DTOs/GuidePages/GuidePageDto.cs`
- Create: `API/DTOs/GuidePages/CreateGuidePageDto.cs`
- Create: `API/DTOs/GuidePages/UpdateGuidePageDto.cs`
- Create: `API/Mappings/GuidePageProfile.cs`
- Create: `API/Services/IGuidePageService.cs`
- Create: `API/Services/GuidePageService.cs`
- Create: `API/Controllers/GuideController.cs`
- Modify: `Program.cs`

- [ ] **Step 1: Créer `API/Entities/GuidePage.cs`**

```csharp
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class GuidePage
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
```

- [ ] **Step 2: Ajouter DbSet dans AppDbContext**

Ajouter `public DbSet<GuidePage> GuidePages { get; set; }` dans `AppDbContext`.

- [ ] **Step 3: Migration**

```bash
dotnet ef migrations add AddGuidePage
dotnet ef database update
```

- [ ] **Step 4: Créer `IGuidePageRepository`**

```csharp
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
```

- [ ] **Step 5: Créer `GuidePageRepository`**

```csharp
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
```

- [ ] **Step 6: Créer les DTOs GuidePage**

`API/DTOs/GuidePages/GuidePageDto.cs` :
```csharp
namespace AssoInternesBrest.API.DTOs.GuidePages
{
    public class GuidePageDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
```

`API/DTOs/GuidePages/CreateGuidePageDto.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.GuidePages
{
    public class CreateGuidePageDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }
}
```

`API/DTOs/GuidePages/UpdateGuidePageDto.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.GuidePages
{
    public class UpdateGuidePageDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }
}
```

- [ ] **Step 7: Créer `API/Mappings/GuidePageProfile.cs`**

```csharp
using AssoInternesBrest.API.DTOs.GuidePages;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class GuidePageProfile : Profile
    {
        public GuidePageProfile()
        {
            CreateMap<GuidePage, GuidePageDto>();
            CreateMap<CreateGuidePageDto, GuidePage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
```

- [ ] **Step 8: Créer `API/Services/IGuidePageService.cs`**

```csharp
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
```

- [ ] **Step 9: Créer `API/Services/GuidePageService.cs`**

```csharp
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
```

- [ ] **Step 10: Créer `API/Controllers/GuideController.cs`**

```csharp
using AssoInternesBrest.API.DTOs.GuidePages;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/guide")]
    public class GuideController(IGuidePageService guidePageService) : ControllerBase
    {
        private readonly IGuidePageService _guidePageService = guidePageService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuidePageDto>>> GetAll()
        {
            IEnumerable<GuidePageDto> pages = await _guidePageService.GetAllAsync();
            return Ok(pages);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<GuidePageDto>> GetBySlug(string slug)
        {
            GuidePageDto? page = await _guidePageService.GetBySlugAsync(slug);
            if (page == null)
                return NotFound();
            return Ok(page);
        }

        [HttpPost]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<GuidePageDto>> Create(CreateGuidePageDto dto)
        {
            GuidePageDto created = await _guidePageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
        }

        [HttpPut("{slug}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<GuidePageDto>> Update(string slug, UpdateGuidePageDto dto)
        {
            GuidePageDto? updated = await _guidePageService.UpdateAsync(slug, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{slug}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult> Delete(string slug)
        {
            bool deleted = await _guidePageService.DeleteAsync(slug);
            if (!deleted)
                return NotFound();
            return Ok();
        }
    }
}
```

- [ ] **Step 11: Enregistrer dans Program.cs**

Ajouter après `builder.Services.AddScoped<IBureauMemberService, BureauMemberService>();` :

```csharp
builder.Services.AddScoped<IGuidePageRepository, GuidePageRepository>();
builder.Services.AddScoped<IGuidePageService, GuidePageService>();
```

Ajouter `GuidePageProfile` dans AddAutoMapper :

```csharp
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<EventProfile>();
    cfg.AddProfile<ArticleProfile>();
    cfg.AddProfile<BureauMemberProfile>();
    cfg.AddProfile<GuidePageProfile>();
});
```

- [ ] **Step 12: Build**

```bash
dotnet build
```

- [ ] **Step 13: Lancer tous les tests**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj --verbosity normal
```

Expected: 17/17 PASS.

- [ ] **Step 14: Commit**

```bash
git add API/Entities/GuidePage.cs API/Data/AppDbContext.cs Migrations/ API/Repositories/IGuidePageRepository.cs API/Repositories/GuidePageRepository.cs API/DTOs/GuidePages/ API/Mappings/GuidePageProfile.cs API/Services/IGuidePageService.cs API/Services/GuidePageService.cs API/Controllers/GuideController.cs Program.cs
git commit -m "feat: add GuidePage entity and CRUD (#15)"
```

---

### Task 5: Fermer les issues GitHub

- [ ] **Step 1: Vérification finale**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj --verbosity normal
```

Expected: 17/17 PASS.

- [ ] **Step 2: Fermer les issues**

```bash
gh issue comment 9 --body "Implémentation terminée : PUT /api/events/{id} avec UpdateEventDto, protégé Bureau+Admin, met à jour UpdatedAt."
gh issue close 9
gh issue comment 10 --body "Implémentation terminée : entité Article avec migration EF Core, slug unique."
gh issue close 10
gh issue comment 11 --body "Implémentation terminée : CRUD Articles complet (GET/POST/PUT/DELETE), AuthorId depuis JWT."
gh issue close 11
gh issue comment 12 --body "Implémentation terminée : entité BureauMember + CRUD, trié par DisplayOrder, protégé Admin."
gh issue close 12
gh issue comment 15 --body "Implémentation terminée : entité GuidePage + CRUD, contenu Markdown, protégé Bureau+Admin."
gh issue close 15
```
