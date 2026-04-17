# Groupe C — Config & Contact Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement AppSettings (key/value store editable via Admin API) and a public Contact form that sends emails via the existing EmailService.

**Architecture:** AppSetting entity with Key as primary key, repository + service layer following the existing Repository → Service → Controller pattern. ContactController calls IEmailService (already implemented in Groupe A). AdminController extended with GET/PUT settings routes.

**Tech Stack:** ASP.NET Core .NET 10, C# 12 primary constructors, Entity Framework Core + PostgreSQL, AutoMapper, xUnit + Moq. IEmailService already available.

---

### Task 1: AppSetting Entity + Migration

**Files:**
- Create: `API/Entities/AppSetting.cs`
- Modify: `API/Data/AppDbContext.cs`
- Create: `API/Data/Migrations/<timestamp>_AddAppSetting.cs` (via dotnet ef)

- [ ] **Step 1: Create AppSetting entity**

```csharp
// API/Entities/AppSetting.cs
namespace AssoInternesBrest.API.Entities
{
    public class AppSetting
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 2: Add DbSet and configure in AppDbContext**

Add to `API/Data/AppDbContext.cs`:

```csharp
public DbSet<AppSetting> AppSettings { get; set; }
```

In `OnModelCreating`, add:

```csharp
modelBuilder.Entity<AppSetting>(entity =>
{
    entity.HasKey(e => e.Key);
    entity.HasData(new AppSetting { Key = "contact_email", Value = "contact@asso-internes-brest.fr" });
});
```

- [ ] **Step 3: Create and apply migration**

```bash
dotnet ef migrations add AddAppSetting --project API --startup-project API
dotnet ef database update --project API --startup-project API
```

Expected: Migration created, database updated, `AppSettings` table created with seed row.

- [ ] **Step 4: Commit**

```bash
git add API/Entities/AppSetting.cs API/Data/AppDbContext.cs API/Data/Migrations/
git commit -m "feat: add AppSetting entity with contact_email seed"
```

---

### Task 2: AppSetting Repository + Service (TDD)

**Files:**
- Create: `API/Repositories/IAppSettingRepository.cs`
- Create: `API/Repositories/AppSettingRepository.cs`
- Create: `API/Services/IAppSettingService.cs`
- Create: `API/Services/AppSettingService.cs`
- Create: `AssoInternesBrest.Tests/Services/AppSettingServiceTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
// AssoInternesBrest.Tests/Services/AppSettingServiceTests.cs
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using Moq;

namespace AssoInternesBrest.Tests.Services
{
    public class AppSettingServiceTests
    {
        private readonly Mock<IAppSettingRepository> _repoMock = new();
        private readonly AppSettingService _service;

        public AppSettingServiceTests()
        {
            _service = new AppSettingService(_repoMock.Object);
        }

        [Fact]
        public async Task GetValueAsync_ExistingKey_ReturnsValue()
        {
            _repoMock.Setup(r => r.GetByKeyAsync("contact_email"))
                .ReturnsAsync(new AppSetting { Key = "contact_email", Value = "test@example.com" });

            string? result = await _service.GetValueAsync("contact_email");

            Assert.Equal("test@example.com", result);
        }

        [Fact]
        public async Task GetValueAsync_MissingKey_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByKeyAsync("missing")).ReturnsAsync((AppSetting?)null);

            string? result = await _service.GetValueAsync("missing");

            Assert.Null(result);
        }

        [Fact]
        public async Task SetValueAsync_CallsUpsert()
        {
            await _service.SetValueAsync("contact_email", "new@example.com");

            _repoMock.Verify(r => r.UpsertAsync(It.Is<AppSetting>(s =>
                s.Key == "contact_email" && s.Value == "new@example.com")), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSettings()
        {
            List<AppSetting> settings = new()
            {
                new AppSetting { Key = "contact_email", Value = "a@b.com" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

            IEnumerable<AppSetting> result = await _service.GetAllAsync();

            Assert.Single(result);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test AssoInternesBrest.Tests --filter "FullyQualifiedName~AppSettingServiceTests"
```

Expected: FAIL — types not defined yet.

- [ ] **Step 3: Create IAppSettingRepository**

```csharp
// API/Repositories/IAppSettingRepository.cs
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IAppSettingRepository
    {
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<AppSetting?> GetByKeyAsync(string key);
        Task UpsertAsync(AppSetting setting);
    }
}
```

- [ ] **Step 4: Create AppSettingRepository**

```csharp
// API/Repositories/AppSettingRepository.cs
using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class AppSettingRepository(AppDbContext context) : IAppSettingRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _context.AppSettings.ToListAsync();
        }

        public async Task<AppSetting?> GetByKeyAsync(string key)
        {
            return await _context.AppSettings.FindAsync(key);
        }

        public async Task UpsertAsync(AppSetting setting)
        {
            AppSetting? existing = await _context.AppSettings.FindAsync(setting.Key);
            if (existing == null)
            {
                _context.AppSettings.Add(setting);
            }
            else
            {
                existing.Value = setting.Value;
                _context.AppSettings.Update(existing);
            }
            await _context.SaveChangesAsync();
        }
    }
}
```

- [ ] **Step 5: Create IAppSettingService**

```csharp
// API/Services/IAppSettingService.cs
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IAppSettingService
    {
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value);
    }
}
```

- [ ] **Step 6: Create AppSettingService**

```csharp
// API/Services/AppSettingService.cs
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;

namespace AssoInternesBrest.API.Services
{
    public class AppSettingService(IAppSettingRepository repository) : IAppSettingService
    {
        private readonly IAppSettingRepository _repository = repository;

        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<string?> GetValueAsync(string key)
        {
            AppSetting? setting = await _repository.GetByKeyAsync(key);
            return setting?.Value;
        }

        public async Task SetValueAsync(string key, string value)
        {
            await _repository.UpsertAsync(new AppSetting { Key = key, Value = value });
        }
    }
}
```

- [ ] **Step 7: Run tests to verify they pass**

```bash
dotnet test AssoInternesBrest.Tests --filter "FullyQualifiedName~AppSettingServiceTests"
```

Expected: 4/4 PASS.

- [ ] **Step 8: Register in Program.cs**

Add to `Program.cs` before `builder.Services.AddControllers()`:

```csharp
builder.Services.AddScoped<IAppSettingRepository, AppSettingRepository>();
builder.Services.AddScoped<IAppSettingService, AppSettingService>();
```

- [ ] **Step 9: Commit**

```bash
git add API/Repositories/IAppSettingRepository.cs API/Repositories/AppSettingRepository.cs API/Services/IAppSettingService.cs API/Services/AppSettingService.cs AssoInternesBrest.Tests/Services/AppSettingServiceTests.cs Program.cs
git commit -m "feat: add AppSetting repository and service with tests"
```

---

### Task 3: Admin Settings Endpoints

**Files:**
- Modify: `API/Controllers/AdminController.cs`
- Create: `API/DTOs/Admin/UpdateSettingDto.cs`

- [ ] **Step 1: Create UpdateSettingDto**

```csharp
// API/DTOs/Admin/UpdateSettingDto.cs
namespace AssoInternesBrest.API.DTOs.Admin
{
    public class UpdateSettingDto
    {
        public string Value { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 2: Extend AdminController with settings endpoints**

Add `IAppSettingService` parameter to constructor and add endpoints to `API/Controllers/AdminController.cs`:

```csharp
using AssoInternesBrest.API.DTOs.Admin;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController(IAuthService authService, IAppSettingService appSettingService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IAppSettingService _appSettingService = appSettingService;

        [HttpPost("users")]
        public async Task<ActionResult> CreateUser(CreateUserDto dto)
        {
            try
            {
                await _authService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName, dto.Role);
                return StatusCode(201);
            }
            catch (InvalidOperationException ex) when (ex.Message == "EMAIL_EXISTS")
            {
                return Conflict(new { message = "Email already in use." });
            }
        }

        [HttpGet("settings")]
        public async Task<ActionResult<IEnumerable<AppSetting>>> GetSettings()
        {
            IEnumerable<AppSetting> settings = await _appSettingService.GetAllAsync();
            return Ok(settings);
        }

        [HttpPut("settings/{key}")]
        public async Task<ActionResult> UpdateSetting(string key, UpdateSettingDto dto)
        {
            await _appSettingService.SetValueAsync(key, dto.Value);
            return Ok();
        }
    }
}
```

- [ ] **Step 3: Build to verify no errors**

```bash
dotnet build API
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add API/Controllers/AdminController.cs API/DTOs/Admin/UpdateSettingDto.cs
git commit -m "feat: add admin settings endpoints GET/PUT /api/admin/settings"
```

---

### Task 4: Contact Controller (TDD)

**Files:**
- Create: `API/DTOs/Contact/ContactDto.cs`
- Create: `API/Controllers/ContactController.cs`
- Create: `AssoInternesBrest.Tests/Controllers/ContactControllerTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
// AssoInternesBrest.Tests/Controllers/ContactControllerTests.cs
using AssoInternesBrest.API.Controllers;
using AssoInternesBrest.API.DTOs.Contact;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AssoInternesBrest.Tests.Controllers
{
    public class ContactControllerTests
    {
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly Mock<IAppSettingService> _settingsMock = new();
        private readonly ContactController _controller;

        public ContactControllerTests()
        {
            _controller = new ContactController(_emailMock.Object, _settingsMock.Object);
        }

        [Fact]
        public async Task SendContact_ValidDto_Returns200()
        {
            _settingsMock.Setup(s => s.GetValueAsync("contact_email"))
                .ReturnsAsync("contact@asso-internes-brest.fr");

            ContactDto dto = new() { Name = "Alice", Email = "alice@example.com", Message = "Hello!" };

            ActionResult result = await _controller.SendContact(dto);

            Assert.IsType<OkResult>(result);
            _emailMock.Verify(e => e.SendAsync(
                "contact@asso-internes-brest.fr",
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendContact_EmailServiceThrows_Returns500()
        {
            _settingsMock.Setup(s => s.GetValueAsync("contact_email"))
                .ReturnsAsync("contact@asso-internes-brest.fr");
            _emailMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            ContactDto dto = new() { Name = "Alice", Email = "alice@example.com", Message = "Hello!" };

            ActionResult result = await _controller.SendContact(dto);

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, ((ObjectResult)result).StatusCode);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test AssoInternesBrest.Tests --filter "FullyQualifiedName~ContactControllerTests"
```

Expected: FAIL — ContactController not defined yet.

- [ ] **Step 3: Create ContactDto**

```csharp
// API/DTOs/Contact/ContactDto.cs
namespace AssoInternesBrest.API.DTOs.Contact
{
    public class ContactDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 4: Create ContactController**

```csharp
// API/Controllers/ContactController.cs
using AssoInternesBrest.API.DTOs.Contact;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController(IEmailService emailService, IAppSettingService appSettingService) : ControllerBase
    {
        private readonly IEmailService _emailService = emailService;
        private readonly IAppSettingService _appSettingService = appSettingService;

        [HttpPost]
        public async Task<ActionResult> SendContact(ContactDto dto)
        {
            string? contactEmail = await _appSettingService.GetValueAsync("contact_email");
            contactEmail ??= "contact@asso-internes-brest.fr";

            try
            {
                string subject = $"Message de {dto.Name} via le site";
                string body = $"De : {dto.Name} <{dto.Email}>\n\n{dto.Message}";
                await _emailService.SendAsync(contactEmail, subject, body);
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { message = "Erreur lors de l'envoi de l'email." });
            }
        }
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
dotnet test AssoInternesBrest.Tests --filter "FullyQualifiedName~ContactControllerTests"
```

Expected: 2/2 PASS.

- [ ] **Step 6: Run all tests**

```bash
dotnet test AssoInternesBrest.Tests
```

Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add API/DTOs/Contact/ContactDto.cs API/Controllers/ContactController.cs AssoInternesBrest.Tests/Controllers/ContactControllerTests.cs
git commit -m "feat: add contact form endpoint POST /api/contact"
```

---

### Task 5: Close GitHub Issues

- [ ] **Step 1: Close issue #13 (AppSettings)**

```bash
gh issue close 13 --comment "Implemented: AppSetting entity with Key/Value, migration with contact_email seed, IAppSettingRepository/Service, GET /api/admin/settings and PUT /api/admin/settings/{key} (AdminOnly)."
```

- [ ] **Step 2: Close issue #14 (Contact form)**

```bash
gh issue close 14 --comment "Implemented: POST /api/contact (public) — reads contact_email from AppSettings, sends via IEmailService (Brevo SMTP), returns 200 or 500."
```
