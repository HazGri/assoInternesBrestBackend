# JWT Service Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implémenter `IJwtService` / `JwtService` capable de générer et valider des tokens JWT contenant userId, email et role.

**Architecture:** Service pur (pas de middleware auth) suivant le pattern `IPasswordService`/`PasswordService` existant. `JwtService` reçoit `IConfiguration` par injection et lit la section `Jwt` depuis `appsettings.Development.json`. Un projet de tests xUnit séparé valide le comportement.

**Tech Stack:** .NET 10, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`, xUnit

---

### Task 1: Ajouter le package NuGet JwtBearer

**Files:**
- Modify: `AssoInternesBrest/AssoInternesBrest.csproj`

- [ ] **Step 1: Installer le package**

```bash
cd AssoInternesBrest
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Expected output: ligne `<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" ...` ajoutée dans le `.csproj`.

- [ ] **Step 2: Vérifier que le projet compile**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add AssoInternesBrest.csproj
git commit -m "chore: add JwtBearer package"
```

---

### Task 2: Configurer la clé secrète JWT

**Files:**
- Modify: `AssoInternesBrest/appsettings.Development.json`

- [ ] **Step 1: Ajouter la section `Jwt`**

Ouvrir `appsettings.Development.json` et ajouter après la section `ConnectionStrings` :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Secret": "asso-internes-brest-super-secret-key-32chars!!",
    "ExpirationHours": 24,
    "Issuer": "AssoInternesBrest",
    "Audience": "AssoInternesBrest"
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add appsettings.Development.json
git commit -m "config: add JWT settings to appsettings.Development.json"
```

---

### Task 3: Créer l'interface IJwtService

**Files:**
- Create: `AssoInternesBrest/API/Services/IJwtService.cs`

- [ ] **Step 1: Créer le fichier**

```csharp
using System.Security.Claims;
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
```

- [ ] **Step 2: Vérifier que le projet compile**

```bash
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add API/Services/IJwtService.cs
git commit -m "feat: add IJwtService interface"
```

---

### Task 4: Créer le projet de tests xUnit

**Files:**
- Create: `AssoInternesBrest.Tests/AssoInternesBrest.Tests.csproj`
- Create: `AssoInternesBrest.Tests/Services/JwtServiceTests.cs`

- [ ] **Step 1: Créer le projet de tests**

```bash
cd ..
dotnet new xunit -n AssoInternesBrest.Tests
cd AssoInternesBrest.Tests
dotnet add reference ../AssoInternesBrest/AssoInternesBrest.csproj
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
```

- [ ] **Step 2: Créer le dossier Services**

```bash
mkdir Services
```

- [ ] **Step 3: Écrire les tests échouants dans `Services/JwtServiceTests.cs`**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using Microsoft.Extensions.Configuration;

namespace AssoInternesBrest.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly IJwtService _sut;
        private readonly User _testUser;

        public JwtServiceTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "test-secret-key-minimum-32-characters-long!!",
                    ["Jwt:ExpirationHours"] = "24",
                    ["Jwt:Issuer"] = "AssoInternesBrest",
                    ["Jwt:Audience"] = "AssoInternesBrest"
                })
                .Build();

            _sut = new JwtService(config);

            _testUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean.dupont@chu-brest.fr",
                PasswordHash = "hash",
                Role = UserRole.Bureau,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public void GenerateToken_ReturnsNonEmptyString()
        {
            var token = _sut.GenerateToken(_testUser);
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateToken_ContainsCorrectClaims()
        {
            var token = _sut.GenerateToken(_testUser);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(_testUser.Id.ToString(), jwt.Subject);
            Assert.Equal(_testUser.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Equal(_testUser.Role.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_ExpiresIn24Hours()
        {
            var before = DateTime.UtcNow.AddHours(24).AddSeconds(-5);
            var token = _sut.GenerateToken(_testUser);
            var after = DateTime.UtcNow.AddHours(24).AddSeconds(5);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.True(jwt.ValidTo >= before && jwt.ValidTo <= after);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
        {
            var token = _sut.GenerateToken(_testUser);
            var principal = _sut.ValidateToken(token);

            Assert.NotNull(principal);
            Assert.Equal(_testUser.Id.ToString(), principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
            Assert.Equal(_testUser.Email, principal.FindFirstValue(JwtRegisteredClaimNames.Email));
            Assert.Equal(_testUser.Role.ToString(), principal.FindFirstValue(ClaimTypes.Role));
        }

        [Fact]
        public void ValidateToken_WithGarbageToken_ReturnsNull()
        {
            var principal = _sut.ValidateToken("not.a.jwt.token");
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithEmptyString_ReturnsNull()
        {
            var principal = _sut.ValidateToken("");
            Assert.Null(principal);
        }
    }
}
```

- [ ] **Step 4: Lancer les tests pour vérifier qu'ils échouent**

```bash
dotnet test
```

Expected: erreur de compilation car `JwtService` n'existe pas encore.

---

### Task 5: Implémenter JwtService

**Files:**
- Create: `AssoInternesBrest/API/Services/JwtService.cs`

- [ ] **Step 1: Créer l'implémentation**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssoInternesBrest.API.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AssoInternesBrest.API.Services
{
    public class JwtService(IConfiguration configuration) : IJwtService
    {
        private readonly IConfiguration _configuration = configuration;

        public string GenerateToken(User user)
        {
            var secret = _configuration["Jwt:Secret"]!;
            var issuer = _configuration["Jwt:Issuer"]!;
            var audience = _configuration["Jwt:Audience"]!;
            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var secret = _configuration["Jwt:Secret"]!;
            var issuer = _configuration["Jwt:Issuer"]!;
            var audience = _configuration["Jwt:Audience"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
```

- [ ] **Step 2: Lancer les tests**

```bash
cd ../AssoInternesBrest.Tests
dotnet test --verbosity normal
```

Expected: 6 tests PASS.

- [ ] **Step 3: Commit**

```bash
cd ../AssoInternesBrest
git add API/Services/JwtService.cs
cd ../AssoInternesBrest.Tests
git add .
cd ..
git commit -m "feat: implement JwtService with GenerateToken and ValidateToken"
```

---

### Task 6: Enregistrer JwtService dans le conteneur DI

**Files:**
- Modify: `AssoInternesBrest/Program.cs`

- [ ] **Step 1: Ajouter l'enregistrement après `PasswordService`**

Dans `Program.cs`, après la ligne `builder.Services.AddScoped<IPasswordService, PasswordService>();`, ajouter :

```csharp
builder.Services.AddScoped<IJwtService, JwtService>();
```

- [ ] **Step 2: Ajouter le using manquant en haut de Program.cs**

```csharp
using AssoInternesBrest.API.Services;
```

(déjà présent — vérifier qu'il n'est pas dupliqué)

- [ ] **Step 3: Vérifier que l'app compile et démarre**

```bash
cd AssoInternesBrest
dotnet build
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add Program.cs
git commit -m "feat: register JwtService in DI container"
```

---

### Task 7: Fermer le ticket GitHub

- [ ] **Step 1: Marquer les critères d'acceptance comme complétés dans l'issue**

```bash
gh issue comment 3 --body "Implémentation terminée : IJwtService + JwtService + 6 tests xUnit passants."
gh issue close 3
```

- [ ] **Step 2: Vérification finale — lancer tous les tests**

```bash
cd ../AssoInternesBrest.Tests
dotnet test --verbosity normal
```

Expected: 6/6 tests PASS, 0 failed.
