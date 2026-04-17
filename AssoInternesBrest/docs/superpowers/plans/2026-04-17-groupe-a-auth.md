# Groupe A — Auth Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implémenter l'authentification JWT complète : middleware (#5), login (#4), création de compte admin avec invitation email (#6), et activation de compte (#7).

**Architecture:** Service pur `AuthService` orchestrant `IUserRepository`, `IPasswordService`, `IJwtService`, `IEmailService`. Même pattern que les services existants (primary constructors, DI Scoped). Tests xUnit avec Moq pour AuthService.

**Tech Stack:** .NET 10, ASP.NET Core, MailKit (SMTP Brevo), Moq (tests), JWT Bearer middleware

---

## Structure des fichiers

| Fichier | Action |
|---------|--------|
| `API/Entities/User.cs` | Modifier — ajout `InvitationToken`, `InvitationTokenExpiresAt` |
| `API/Repositories/IUserRepository.cs` | Créer — interface |
| `API/Repositories/UserRepository.cs` | Créer — impl |
| `API/Services/IEmailService.cs` | Créer — interface |
| `API/Services/EmailService.cs` | Créer — impl MailKit |
| `API/Services/IAuthService.cs` | Créer — interface |
| `API/Services/AuthService.cs` | Créer — impl |
| `API/Controllers/AuthController.cs` | Créer — login + activate |
| `API/Controllers/AdminController.cs` | Créer — create user |
| `API/DTOs/Auth/LoginDto.cs` | Créer |
| `API/DTOs/Auth/LoginResponseDto.cs` | Créer |
| `API/DTOs/Auth/ActivateDto.cs` | Créer |
| `API/DTOs/Admin/CreateUserDto.cs` | Créer |
| `API/Controllers/EventsController.cs` | Modifier — [Authorize] sur POST et DELETE |
| `Program.cs` | Modifier — JWT middleware + policies + services |
| `appsettings.Development.json` | Modifier — ajout sections `Smtp` et `App` |
| `AssoInternesBrest.Tests/Services/AuthServiceTests.cs` | Créer — 7 tests |

---

### Task 1: Ajouter InvitationToken sur User + migration

**Files:**
- Modify: `API/Entities/User.cs`

- [ ] **Step 1: Ajouter les champs sur User**

Remplacer le contenu de `API/Entities/User.cs` par :

```csharp
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string? InvitationToken { get; set; }
        public DateTime? InvitationTokenExpiresAt { get; set; }
    }

    public enum UserRole
    {
        Membre,
        Bureau,
        Admin
    }
}
```

- [ ] **Step 2: Créer la migration**

```bash
cd AssoInternesBrest
dotnet ef migrations add AddInvitationToken
```

Expected: fichier `Migrations/YYYYMMDDHHMMSS_AddInvitationToken.cs` créé avec `AddColumn` pour les deux champs.

- [ ] **Step 3: Appliquer la migration**

```bash
dotnet ef database update
```

Expected: `Done.`

- [ ] **Step 4: Vérifier que le projet compile**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add API/Entities/User.cs Migrations/
git commit -m "feat: add InvitationToken fields to User entity"
```

---

### Task 2: IUserRepository + UserRepository

**Files:**
- Create: `API/Repositories/IUserRepository.cs`
- Create: `API/Repositories/UserRepository.cs`

- [ ] **Step 1: Créer l'interface**

Créer `API/Repositories/IUserRepository.cs` :

```csharp
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByInvitationTokenAsync(string token);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
```

- [ ] **Step 2: Créer l'implémentation**

Créer `API/Repositories/UserRepository.cs` :

```csharp
using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByInvitationTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.InvitationToken == token);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add API/Repositories/IUserRepository.cs API/Repositories/UserRepository.cs
git commit -m "feat: add IUserRepository and UserRepository"
```

---

### Task 3: MailKit + IEmailService + EmailService

**Files:**
- Create: `API/Services/IEmailService.cs`
- Create: `API/Services/EmailService.cs`
- Modify: `appsettings.Development.json`

- [ ] **Step 1: Installer MailKit**

```bash
dotnet add package MailKit
```

Expected: `<PackageReference Include="MailKit" ...` ajouté dans le `.csproj`.

- [ ] **Step 2: Créer l'interface**

Créer `API/Services/IEmailService.cs` :

```csharp
namespace AssoInternesBrest.API.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
```

- [ ] **Step 3: Créer l'implémentation**

Créer `API/Services/EmailService.cs` :

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AssoInternesBrest.API.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task SendAsync(string to, string subject, string body)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Smtp:FromName"]!,
                _configuration["Smtp:FromEmail"]!));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using SmtpClient client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Smtp:Host"]!,
                int.Parse(_configuration["Smtp:Port"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _configuration["Smtp:Username"]!,
                _configuration["Smtp:Password"]!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
```

- [ ] **Step 4: Ajouter la config SMTP dans appsettings.Development.json**

Ajouter après la section `Jwt` :

```json
{
  "Logging": { ... },
  "ConnectionStrings": { ... },
  "Jwt": { ... },
  "Smtp": {
    "Host": "smtp-relay.brevo.com",
    "Port": "587",
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@asso-internes-brest.fr",
    "FromName": "Asso Internes Brest"
  },
  "App": {
    "BaseUrl": "http://localhost:5173"
  }
}
```

- [ ] **Step 5: Build**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add API/Services/IEmailService.cs API/Services/EmailService.cs appsettings.Development.json AssoInternesBrest.csproj
git commit -m "feat: add IEmailService with MailKit implementation"
```

---

### Task 4: JWT Middleware + protection routes (Ticket #5)

**Files:**
- Modify: `Program.cs`
- Modify: `API/Controllers/EventsController.cs`

- [ ] **Step 1: Mettre à jour Program.cs**

Remplacer le contenu de `Program.cs` par :

```csharp
using System.Text;
using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Mappings;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<EventProfile>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

string jwtSecret = builder.Configuration["Jwt:Secret"]!;
string jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
string jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("BureauOrAdmin", policy => policy.RequireRole("Bureau", "Admin"))
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

builder.Services.AddControllers();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();

string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

- [ ] **Step 2: Protéger les routes dans EventsController**

Remplacer le contenu de `API/Controllers/EventsController.cs` par :

```csharp
using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            IEnumerable<EventDto> events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<EventDto>> GetEventBySlug(string slug)
        {
            EventDto? eventDto = await _eventService.GetEventBySlugAsync(slug);
            if (eventDto == null)
                return NotFound();
            return Ok(eventDto);
        }

        [HttpPost]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto dto)
        {
            EventDto createdEvent = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(
                nameof(GetEventBySlug),
                new { slug = createdEvent.Slug },
                createdEvent
            );
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult> DeleteEvent(Guid id)
        {
            bool isDeleted = await _eventService.DeleteEventAsync(id);
            if (isDeleted)
                return Ok();
            return NotFound();
        }
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add Program.cs API/Controllers/EventsController.cs
git commit -m "feat: configure JWT middleware and protect event routes (#5)"
```

---

### Task 5: IAuthService + AuthService (TDD)

**Files:**
- Create: `API/Services/IAuthService.cs`
- Create: `API/Services/AuthService.cs`
- Create: `AssoInternesBrest.Tests/Services/AuthServiceTests.cs`

- [ ] **Step 1: Ajouter Moq au projet de tests**

```bash
cd AssoInternesBrest.Tests
dotnet add package Moq
cd ..
```

- [ ] **Step 2: Créer l'interface IAuthService**

Créer `API/Services/IAuthService.cs` :

```csharp
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string email, string password);
        Task<User> CreateUserAsync(string email, string firstName, string lastName, UserRole role);
        Task<bool> ActivateAsync(string token, string newPassword);
    }
}
```

- [ ] **Step 3: Écrire les tests échouants**

Créer `AssoInternesBrest.Tests/Services/AuthServiceTests.cs` :

```csharp
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AssoInternesBrest.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly IAuthService _sut;
        private readonly User _activeUser;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _emailServiceMock = new Mock<IEmailService>();

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["App:BaseUrl"] = "http://localhost:5173"
                })
                .Build();

            _sut = new AuthService(
                _userRepoMock.Object,
                _passwordServiceMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object,
                config);

            _activeUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "jean@chu-brest.fr",
                PasswordHash = "hashed",
                IsActive = true,
                Role = UserRole.Membre,
                FirstName = "Jean",
                LastName = "Dupont",
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync("jean@chu-brest.fr")).ReturnsAsync(_activeUser);
            _passwordServiceMock.Setup(p => p.Verify("password", "hashed")).Returns(true);
            _jwtServiceMock.Setup(j => j.GenerateToken(_activeUser)).Returns("jwt-token");

            string? result = await _sut.LoginAsync("jean@chu-brest.fr", "password");

            Assert.Equal("jwt-token", result);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsNull()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync("jean@chu-brest.fr")).ReturnsAsync(_activeUser);
            _passwordServiceMock.Setup(p => p.Verify("wrong", "hashed")).Returns(false);

            string? result = await _sut.LoginAsync("jean@chu-brest.fr", "wrong");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ReturnsNull()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync("unknown@chu-brest.fr")).ReturnsAsync((User?)null);

            string? result = await _sut.LoginAsync("unknown@chu-brest.fr", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ReturnsNull()
        {
            User inactiveUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "inactive@chu-brest.fr",
                PasswordHash = "hashed",
                IsActive = false,
                Role = UserRole.Membre,
                FirstName = "Paul",
                LastName = "Martin",
                CreatedAt = DateTime.UtcNow
            };
            _userRepoMock.Setup(r => r.GetByEmailAsync("inactive@chu-brest.fr")).ReturnsAsync(inactiveUser);

            string? result = await _sut.LoginAsync("inactive@chu-brest.fr", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task ActivateAsync_WithValidToken_ReturnsTrueAndActivatesUser()
        {
            User pendingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "new@chu-brest.fr",
                PasswordHash = "",
                IsActive = false,
                Role = UserRole.Membre,
                FirstName = "Marie",
                LastName = "Curie",
                CreatedAt = DateTime.UtcNow,
                InvitationToken = "valid-token",
                InvitationTokenExpiresAt = DateTime.UtcNow.AddHours(72)
            };
            _userRepoMock.Setup(r => r.GetByInvitationTokenAsync("valid-token")).ReturnsAsync(pendingUser);
            _passwordServiceMock.Setup(p => p.HashPassword("newpass123")).Returns("hashed-newpass");

            bool result = await _sut.ActivateAsync("valid-token", "newpass123");

            Assert.True(result);
            Assert.True(pendingUser.IsActive);
            Assert.Equal("hashed-newpass", pendingUser.PasswordHash);
            Assert.Null(pendingUser.InvitationToken);
            Assert.Null(pendingUser.InvitationTokenExpiresAt);
        }

        [Fact]
        public async Task ActivateAsync_WithExpiredToken_ReturnsFalse()
        {
            User expiredUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "expired@chu-brest.fr",
                PasswordHash = "",
                IsActive = false,
                Role = UserRole.Membre,
                FirstName = "Alice",
                LastName = "Bernard",
                CreatedAt = DateTime.UtcNow,
                InvitationToken = "expired-token",
                InvitationTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
            _userRepoMock.Setup(r => r.GetByInvitationTokenAsync("expired-token")).ReturnsAsync(expiredUser);

            bool result = await _sut.ActivateAsync("expired-token", "newpass123");

            Assert.False(result);
        }

        [Fact]
        public async Task ActivateAsync_WithInvalidToken_ReturnsFalse()
        {
            _userRepoMock.Setup(r => r.GetByInvitationTokenAsync("invalid")).ReturnsAsync((User?)null);

            bool result = await _sut.ActivateAsync("invalid", "newpass123");

            Assert.False(result);
        }
    }
}
```

- [ ] **Step 4: Lancer les tests pour vérifier qu'ils échouent**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj
```

Expected: erreur de compilation — `AuthService` n'existe pas encore.

- [ ] **Step 5: Créer AuthService**

Créer `API/Services/AuthService.cs` :

```csharp
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;

namespace AssoInternesBrest.API.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string?> LoginAsync(string email, string password)
        {
            User? user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
                return null;
            if (!_passwordService.Verify(password, user.PasswordHash))
                return null;
            return _jwtService.GenerateToken(user);
        }

        public async Task<User> CreateUserAsync(string email, string firstName, string lastName, UserRole role)
        {
            User? existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
                throw new InvalidOperationException("EMAIL_EXISTS");

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                PasswordHash = "",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                InvitationToken = Guid.NewGuid().ToString("N"),
                InvitationTokenExpiresAt = DateTime.UtcNow.AddHours(72)
            };

            await _userRepository.AddAsync(user);

            string activationUrl = $"{_configuration["App:BaseUrl"]}/activate?token={user.InvitationToken}";
            string body = $"Bonjour {firstName},\n\nVotre compte a été créé sur le site de l'Asso Internes Brest.\n\nCliquez sur le lien suivant pour définir votre mot de passe (valable 72h) :\n\n{activationUrl}\n\nL'équipe Asso Internes Brest";
            await _emailService.SendAsync(email, "Activation de votre compte — Asso Internes Brest", body);

            return user;
        }

        public async Task<bool> ActivateAsync(string token, string newPassword)
        {
            User? user = await _userRepository.GetByInvitationTokenAsync(token);
            if (user == null || user.InvitationTokenExpiresAt < DateTime.UtcNow)
                return false;

            user.PasswordHash = _passwordService.HashPassword(newPassword);
            user.IsActive = true;
            user.InvitationToken = null;
            user.InvitationTokenExpiresAt = null;

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
```

- [ ] **Step 6: Lancer les tests**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj --verbosity normal
```

Expected: les 7 tests de `AuthServiceTests` passent (+ les 6 de `JwtServiceTests` = 13 au total).

- [ ] **Step 7: Commit**

```bash
git add API/Services/IAuthService.cs API/Services/AuthService.cs AssoInternesBrest.Tests/Services/AuthServiceTests.cs AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj
git commit -m "feat: implement AuthService with TDD (login, create, activate)"
```

---

### Task 6: DTOs Auth + AuthController (Tickets #4 et #7)

**Files:**
- Create: `API/DTOs/Auth/LoginDto.cs`
- Create: `API/DTOs/Auth/LoginResponseDto.cs`
- Create: `API/DTOs/Auth/ActivateDto.cs`
- Create: `API/Controllers/AuthController.cs`

- [ ] **Step 1: Créer le dossier et les DTOs**

Créer `API/DTOs/Auth/LoginDto.cs` :

```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
```

Créer `API/DTOs/Auth/LoginResponseDto.cs` :

```csharp
namespace AssoInternesBrest.API.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
    }
}
```

Créer `API/DTOs/Auth/ActivateDto.cs` :

```csharp
using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.Auth
{
    public class ActivateDto
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = null!;
    }
}
```

- [ ] **Step 2: Créer AuthController**

Créer `API/Controllers/AuthController.cs` :

```csharp
using AssoInternesBrest.API.DTOs.Auth;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            string? token = await _authService.LoginAsync(dto.Email, dto.Password);
            if (token == null)
                return Unauthorized();
            return Ok(new LoginResponseDto { Token = token });
        }

        [HttpPost("activate")]
        public async Task<ActionResult> Activate(ActivateDto dto)
        {
            bool success = await _authService.ActivateAsync(dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest("Token invalide ou expiré.");
            return Ok();
        }
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add API/DTOs/Auth/ API/Controllers/AuthController.cs
git commit -m "feat: add AuthController with login and activate endpoints (#4 #7)"
```

---

### Task 7: CreateUserDto + AdminController + enregistrement services (Ticket #6)

**Files:**
- Create: `API/DTOs/Admin/CreateUserDto.cs`
- Create: `API/Controllers/AdminController.cs`
- Modify: `Program.cs`

- [ ] **Step 1: Créer CreateUserDto**

Créer `API/DTOs/Admin/CreateUserDto.cs` :

```csharp
using System.ComponentModel.DataAnnotations;
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.DTOs.Admin
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; }
    }
}
```

- [ ] **Step 2: Créer AdminController**

Créer `API/Controllers/AdminController.cs` :

```csharp
using AssoInternesBrest.API.DTOs.Admin;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

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
                return Conflict("Un compte avec cet email existe déjà.");
            }
        }
    }
}
```

- [ ] **Step 3: Enregistrer IAuthService dans Program.cs**

Dans `Program.cs`, après la ligne `builder.Services.AddScoped<IEmailService, EmailService>();`, ajouter :

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

- [ ] **Step 4: Build**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 5: Lancer tous les tests**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj --verbosity normal
```

Expected: 13/13 tests PASS.

- [ ] **Step 6: Commit**

```bash
git add API/DTOs/Admin/ API/Controllers/AdminController.cs Program.cs
git commit -m "feat: add AdminController and register AuthService in DI (#6)"
```

---

### Task 8: Fermer les issues GitHub

- [ ] **Step 1: Vérification finale**

```bash
dotnet test AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj --verbosity normal
```

Expected: 13/13 PASS.

- [ ] **Step 2: Fermer les issues**

```bash
gh issue comment 5 --body "Implémentation terminée : JWT middleware configuré, POST /api/events et DELETE /api/events/{id} protégés Bureau+Admin."
gh issue close 5
gh issue comment 4 --body "Implémentation terminée : POST /api/auth/login retourne 200+token si valide, 401 sinon."
gh issue close 4
gh issue comment 6 --body "Implémentation terminée : POST /api/admin/users crée un compte et envoie un email d'invitation (token 72h)."
gh issue close 6
gh issue comment 7 --body "Implémentation terminée : POST /api/auth/activate valide le token, hash le mot de passe, active le compte."
gh issue close 7
```
