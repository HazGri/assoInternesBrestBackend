# Backend — Tickets #4 à #17 : Design Global

**Date:** 2026-04-17
**Tickets:** #4, #5, #6, #7, #9, #10, #11, #12, #13, #14, #15, #16, #17
**Exclu:** #8 (Import Excel — pas d'email dans le fichier, reporté)
**Stratégie:** 4 groupes fonctionnels, chacun sur sa propre branche Git

---

## Conventions

- Pas de `var` — types explicites partout
- Primary constructors (C# 12)
- DI Scoped pour tous les services et repositories
- Async/Await sur toutes les opérations DB
- Pas de commentaires sauf si comportement non-évident

---

## Groupe A — Auth (#4, #5, #6, #7)

### Migration

Ajout sur `User` :
- `InvitationToken` (string?, nullable)
- `InvitationTokenExpiresAt` (DateTime?, nullable)

### Nouveaux fichiers

| Fichier | Rôle |
|---------|------|
| `API/Repositories/IUserRepository.cs` | Interface accès User |
| `API/Repositories/UserRepository.cs` | Impl : GetByEmail, GetByInvitationToken, Add, Update |
| `API/Services/IEmailService.cs` | Interface envoi email |
| `API/Services/EmailService.cs` | MailKit + Brevo SMTP |
| `API/Services/IAuthService.cs` | Interface auth (login, create, activate) |
| `API/Services/AuthService.cs` | Impl orchestrant User/Email/Password/Jwt |
| `API/Controllers/AuthController.cs` | POST /api/auth/login, POST /api/auth/activate |
| `API/Controllers/AdminController.cs` | POST /api/admin/users (Admin only) |
| `API/DTOs/Auth/LoginDto.cs` | Email, Password |
| `API/DTOs/Auth/LoginResponseDto.cs` | Token (string) |
| `API/DTOs/Auth/ActivateDto.cs` | Token, NewPassword |
| `API/DTOs/Admin/CreateUserDto.cs` | Email, FirstName, LastName, Role |

### Configuration SMTP (appsettings.Development.json)

```json
"Smtp": {
  "Host": "smtp-relay.brevo.com",
  "Port": 587,
  "Username": "",
  "Password": "",
  "FromEmail": "noreply@asso-internes-brest.fr",
  "FromName": "Asso Internes Brest"
}
```

### Routes

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| POST | /api/auth/login | Public | Email+Password → JWT token |
| POST | /api/auth/activate | Public | Token+NewPassword → active le compte |
| POST | /api/admin/users | Admin | Crée un compte + envoie invitation |

### Flows détaillés

**Login**
1. Cherche `User` par email via `IUserRepository`
2. Retourne 401 générique si introuvable ou `IsActive = false`
3. Vérifie password via `IPasswordService.Verify`
4. Retourne 401 générique si incorrect (ne pas distinguer email vs password)
5. Génère JWT via `IJwtService.GenerateToken`
6. Retourne 200 + `{ token }`

**Créer compte (Admin)**
1. Vérifie que l'email n'existe pas déjà → 409 si doublon
2. Crée `User` avec `IsActive = false`, `PasswordHash = ""`
3. Génère `InvitationToken = Guid.NewGuid().ToString("N")`, `InvitationTokenExpiresAt = UtcNow + 72h`
4. Envoie email via `IEmailService` avec lien d'activation
5. Retourne 201

**Activer compte**
1. Cherche `User` par `InvitationToken`
2. Retourne 400 si token invalide ou expiré
3. Hash le nouveau mot de passe via `IPasswordService.HashPassword`
4. Met `IsActive = true`, `PasswordHash = hash`
5. Invalide le token : `InvitationToken = null`, `InvitationTokenExpiresAt = null`
6. Retourne 200

### Program.cs — JWT Middleware

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* TokenValidationParameters */ });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("BureauOrAdmin", policy => policy.RequireRole("Bureau", "Admin"))
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
```

`UseAuthentication()` avant `UseAuthorization()` dans le pipeline.

### Protection routes existantes (Ticket #5)

- `POST /api/events` → `[Authorize(Policy = "BureauOrAdmin")]`
- `DELETE /api/events/{id}` → `[Authorize(Policy = "BureauOrAdmin")]`
- `GET /api/events`, `GET /api/events/{slug}` → publics (inchangés)

---

## Groupe B — Contenu (#9, #10, #11, #12, #15)

### Migrations

- `Article` : Id (Guid), Title, Slug (unique index), Content, AuthorId (Guid, FK User), IsPublished (default false), CreatedAt, UpdatedAt
- `BureauMember` : Id (Guid), FirstName, LastName, Role (string), Email, DisplayOrder (int)
- `GuidePage` : Id (Guid), Slug (unique index), Title, Content (Markdown string), UpdatedAt

### Pattern par entité (identique à Event)

Chaque entité suit : Repository interface → Repository impl → Service interface → Service impl → DTOs → AutoMapper profile → Controller

### Routes

| Méthode | Route | Auth | Notes |
|---------|-------|------|-------|
| PUT | /api/events/{id} | BureauOrAdmin | Met à jour UpdatedAt automatiquement |
| GET | /api/articles | Public | Articles publiés uniquement |
| GET | /api/articles/{slug} | Public | |
| POST | /api/articles | BureauOrAdmin | AuthorId extrait du claim `sub` du JWT |
| PUT | /api/articles/{id} | BureauOrAdmin | Met à jour UpdatedAt |
| DELETE | /api/articles/{id} | BureauOrAdmin | |
| GET | /api/bureau | Public | Trié par DisplayOrder |
| POST | /api/bureau | AdminOnly | |
| PUT | /api/bureau/{id} | AdminOnly | |
| DELETE | /api/bureau/{id} | AdminOnly | |
| GET | /api/guide | Public | |
| GET | /api/guide/{slug} | Public | |
| POST | /api/guide | BureauOrAdmin | |
| PUT | /api/guide/{slug} | BureauOrAdmin | |
| DELETE | /api/guide/{slug} | BureauOrAdmin | |

### Notes

- `AuthorId` sur Article : extrait de `User.FindFirstValue(ClaimTypes.NameIdentifier)` dans le controller
- `PUT /api/events/{id}` : `UpdatedAt = DateTime.UtcNow` dans le service, 404 si événement introuvable
- Content des GuidePage stocké en Markdown brut — le frontend se charge du rendu
- `SlugGenerator` existant réutilisé pour Article et GuidePage

---

## Groupe C — Config & Contact (#13, #14)

### Migration

- `AppSetting` : Key (string, PK unique), Value (string)
- Seed initial via `HasData` : `{ Key = "contact_email", Value = "contact@asso-internes-brest.fr" }`

### Nouveaux fichiers

| Fichier | Rôle |
|---------|------|
| `API/Entities/AppSetting.cs` | Entité clé/valeur |
| `API/Repositories/IAppSettingRepository.cs` | GetAll, GetByKey, Upsert |
| `API/Repositories/AppSettingRepository.cs` | Impl |
| `API/Services/IAppSettingService.cs` | GetValue(key), SetValue(key, value) |
| `API/Services/AppSettingService.cs` | Impl |
| `API/Controllers/ContactController.cs` | POST /api/contact |
| `API/DTOs/Contact/ContactDto.cs` | Name, Email, Message |

`AdminController` étendu avec :
- `GET /api/admin/settings` → AdminOnly
- `PUT /api/admin/settings/{key}` → AdminOnly

### Flow contact

1. Reçoit `ContactDto` (Name, Email, Message)
2. Lit `contact_email` via `IAppSettingService.GetValue("contact_email")`
3. Envoie email via `IEmailService`
4. 200 si succès, 500 si erreur SMTP

---

## Groupe D — HelloAsso simplifié (#16, #17)

### Migration

Ajout sur `Event` :
- `HelloAssoUrl` (string?, nullable) — URL complète du formulaire HelloAsso

### Changements DTOs

- `CreateEventDto` : ajout `HelloAssoUrl` (string?, nullable)
- `UpdateEventDto` : ajout `HelloAssoUrl` (string?, nullable)
- `EventDto` : expose `HelloAssoUrl`

### Nouveau endpoint

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| GET | /api/events/{id}/registration | Public | Retourne `{ helloAssoUrl, hasRegistration }` |

`hasRegistration = helloAssoUrl != null`

Le frontend affiche le widget iframe si `hasRegistration = true`, sinon affiche "Événement sans inscription".

---

## Ordre d'implémentation

1. **Branche `feat/groupe-a-auth`** — tickets #5, #4, #6, #7 (dans cet ordre)
2. **Branche `feat/groupe-b-contenu`** — tickets #9, #10, #11, #12, #15
3. **Branche `feat/groupe-c-config-contact`** — tickets #13, #14
4. **Branche `feat/groupe-d-helloasso`** — tickets #16, #17

---

## Critères d'acceptance globaux

- [ ] JWT middleware configuré, routes protégées
- [ ] Login retourne un JWT valide
- [ ] Admin peut créer un compte → email d'invitation reçu
- [ ] Invité peut activer son compte via token
- [ ] PUT /api/events/{id} fonctionnel
- [ ] CRUD Articles, BureauMembers, GuidePages
- [ ] AppSettings éditables via API
- [ ] Formulaire de contact envoie un email
- [ ] HelloAssoUrl stocké sur Event, endpoint registration fonctionnel
