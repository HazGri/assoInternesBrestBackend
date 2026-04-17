# JWT Service Design

**Date:** 2026-04-17
**Ticket:** #3 — Génération et validation JWT
**Scope:** Implémentation de `IJwtService` / `JwtService` uniquement. Le middleware d'authentification et les endpoints login/protect sont couverts par les tickets 4 et 5.

---

## Contexte

Le projet suit un pattern Repository → Service → Controller avec primary constructors et injection de dépendances. `IPasswordService` / `PasswordService` est le modèle de référence pour ce ticket. Le package `Microsoft.AspNetCore.Authentication.JwtBearer` n'est pas encore installé.

---

## Architecture

### Fichiers créés / modifiés

| Fichier | Action |
|---|---|
| `API/Services/IJwtService.cs` | Création — interface |
| `API/Services/JwtService.cs` | Création — implémentation |
| `appsettings.Development.json` | Modification — ajout section `Jwt` |
| `Program.cs` | Modification — enregistrement du service |
| `AssoInternesBrest.csproj` | Modification — ajout package `JwtBearer` |

---

## Interface

```csharp
public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
```

---

## Configuration

Section à ajouter dans `appsettings.Development.json` :

```json
"Jwt": {
  "Secret": "une-cle-secrete-longue-au-moins-32-caracteres",
  "ExpirationHours": 24,
  "Issuer": "AssoInternesBrest",
  "Audience": "AssoInternesBrest"
}
```

---

## Claims du token

| Claim | Valeur |
|---|---|
| `sub` | `user.Id.ToString()` |
| `email` | `user.Email` |
| `role` | `user.Role.ToString()` (Membre / Bureau / Admin) |

---

## Implémentation

### `GenerateToken(User user)`
1. Lire la section `Jwt` via `IConfiguration`
2. Créer les claims : `sub`, `email`, `role`
3. Générer un `JwtSecurityToken` signé avec `HmacSha256`
4. Expiration : 24 heures
5. Retourner le token sérialisé en string

### `ValidateToken(string token)`
1. Configurer `TokenValidationParameters` avec issuer, audience et clé secrète
2. Tenter la validation via `JwtSecurityTokenHandler.ValidateToken`
3. Retourner le `ClaimsPrincipal` si valide
4. Retourner `null` si le token est expiré, malformé ou invalide (catch silencieux)

---

## Enregistrement DI

```csharp
builder.Services.AddScoped<IJwtService, JwtService>();
```

---

## Critères d'acceptance (depuis le ticket)

- [ ] `IJwtService` avec `GenerateToken(User)` et `ValidateToken(string)`
- [ ] Le token contient : userId, email, role
- [ ] Clé secrète dans `appsettings.Development.json`
