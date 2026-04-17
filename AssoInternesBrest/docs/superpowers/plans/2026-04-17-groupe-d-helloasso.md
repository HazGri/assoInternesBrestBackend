# Groupe D — HelloAsso Simplifié Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Store a HelloAsso registration URL on Event entities and expose a `/api/events/{id}/registration` endpoint that returns whether an event has a registration form.

**Architecture:** Minimal: add `HelloAssoUrl` (nullable string) to `Event` entity + migration, update all Event DTOs, add `UpdateEventAsync` in service/repo to support it, add GET `/api/events/{id}/registration` endpoint returning `{ helloAssoUrl, hasRegistration }`.

**Tech Stack:** ASP.NET Core .NET 10, C# 12 primary constructors, Entity Framework Core + PostgreSQL, AutoMapper, xUnit + Moq.

---

### Task 1: Add HelloAssoUrl to Event Entity + Migration

**Files:**
- Modify: `API/Entities/Event.cs`
- Modify: `API/Data/Migrations/<timestamp>_AddHelloAssoUrl.cs` (via dotnet ef)

- [ ] **Step 1: Add HelloAssoUrl property to Event entity**

In `API/Entities/Event.cs`, add:

```csharp
public string? HelloAssoUrl { get; set; }
```

The full entity should look like (add only this property, don't change others):

```csharp
public string? HelloAssoUrl { get; set; }
```

- [ ] **Step 2: Create and apply migration**

```bash
dotnet ef migrations add AddHelloAssoUrl --project API --startup-project API
dotnet ef database update --project API --startup-project API
```

Expected: Migration created, `HelloAssoUrl` column added to `Events` table (nullable varchar).

- [ ] **Step 3: Commit**

```bash
git add API/Entities/Event.cs API/Data/Migrations/
git commit -m "feat: add HelloAssoUrl nullable field to Event entity"
```

---

### Task 2: Update Event DTOs + AutoMapper

**Files:**
- Modify: `API/DTOs/Events/EventDto.cs`
- Modify: `API/DTOs/Events/CreateEventDto.cs`
- Create: `API/DTOs/Events/UpdateEventDto.cs` (if not already created by Groupe B)
- Modify: `API/Mappings/EventProfile.cs`

> **Note:** Groupe B may have already created `UpdateEventDto.cs` and added `GetByIdAsync` / `UpdateEventAsync`. Read the current file state before editing to avoid duplication.

- [ ] **Step 1: Check what already exists from Groupe B**

Read `API/DTOs/Events/UpdateEventDto.cs` and `API/Services/IEventService.cs` to see what was already implemented.

- [ ] **Step 2: Add HelloAssoUrl to EventDto**

In `API/DTOs/Events/EventDto.cs`, add:

```csharp
public string? HelloAssoUrl { get; set; }
```

- [ ] **Step 3: Add HelloAssoUrl to CreateEventDto**

In `API/DTOs/Events/CreateEventDto.cs`, add:

```csharp
public string? HelloAssoUrl { get; set; }
```

- [ ] **Step 4: Add HelloAssoUrl to UpdateEventDto**

In `API/DTOs/Events/UpdateEventDto.cs`, add (if not already there):

```csharp
public string? HelloAssoUrl { get; set; }
```

- [ ] **Step 5: Verify AutoMapper EventProfile maps HelloAssoUrl**

In `API/Mappings/EventProfile.cs`, check that the mapping `Event → EventDto` and `CreateEventDto → Event` covers all properties. Since AutoMapper maps by convention (same property name), `HelloAssoUrl` should be included automatically. If the profile uses explicit mappings, add:

```csharp
// In CreateMap<Event, EventDto>() — add if explicit:
.ForMember(dest => dest.HelloAssoUrl, opt => opt.MapFrom(src => src.HelloAssoUrl))

// In CreateMap<CreateEventDto, Event>() — add if explicit:
.ForMember(dest => dest.HelloAssoUrl, opt => opt.MapFrom(src => src.HelloAssoUrl))
```

- [ ] **Step 6: Build to verify no errors**

```bash
dotnet build API
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Commit**

```bash
git add API/DTOs/Events/EventDto.cs API/DTOs/Events/CreateEventDto.cs API/DTOs/Events/UpdateEventDto.cs API/Mappings/EventProfile.cs
git commit -m "feat: add HelloAssoUrl to Event DTOs and AutoMapper profile"
```

---

### Task 3: Registration Endpoint (TDD)

**Files:**
- Create: `API/DTOs/Events/RegistrationDto.cs`
- Modify: `API/Controllers/EventsController.cs`
- Create: `AssoInternesBrest.Tests/Controllers/EventRegistrationTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
// AssoInternesBrest.Tests/Controllers/EventRegistrationTests.cs
using AssoInternesBrest.API.Controllers;
using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AssoInternesBrest.Tests.Controllers
{
    public class EventRegistrationTests
    {
        private readonly Mock<IEventService> _serviceMock = new();
        private readonly EventsController _controller;

        public EventRegistrationTests()
        {
            _controller = new EventsController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetRegistration_EventWithUrl_ReturnsHasRegistrationTrue()
        {
            Guid eventId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync(new EventDto { Id = eventId, HelloAssoUrl = "https://www.helloasso.com/test" });

            ActionResult<RegistrationDto> result = await _controller.GetRegistration(eventId);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            RegistrationDto dto = Assert.IsType<RegistrationDto>(ok.Value);
            Assert.True(dto.HasRegistration);
            Assert.Equal("https://www.helloasso.com/test", dto.HelloAssoUrl);
        }

        [Fact]
        public async Task GetRegistration_EventWithoutUrl_ReturnsHasRegistrationFalse()
        {
            Guid eventId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync(new EventDto { Id = eventId, HelloAssoUrl = null });

            ActionResult<RegistrationDto> result = await _controller.GetRegistration(eventId);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            RegistrationDto dto = Assert.IsType<RegistrationDto>(ok.Value);
            Assert.False(dto.HasRegistration);
            Assert.Null(dto.HelloAssoUrl);
        }

        [Fact]
        public async Task GetRegistration_UnknownEvent_Returns404()
        {
            Guid eventId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync((EventDto?)null);

            ActionResult<RegistrationDto> result = await _controller.GetRegistration(eventId);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test AssoInternesBrest.Tests --filter "FullyQualifiedName~EventRegistrationTests"
```

Expected: FAIL — RegistrationDto not defined, GetRegistration not defined, GetEventByIdAsync may not exist.

- [ ] **Step 3: Create RegistrationDto**

```csharp
// API/DTOs/Events/RegistrationDto.cs
namespace AssoInternesBrest.API.DTOs.Events
{
    public class RegistrationDto
    {
        public string? HelloAssoUrl { get; set; }
        public bool HasRegistration { get; set; }
    }
}
```

- [ ] **Step 4: Ensure GetEventByIdAsync exists in IEventService**

Check `API/Services/IEventService.cs`. If `GetEventByIdAsync(Guid id)` was not added by Groupe B, add it now:

```csharp
Task<EventDto?> GetEventByIdAsync(Guid id);
```

And implement in `API/Services/EventService.cs`:

```csharp
public async Task<EventDto?> GetEventByIdAsync(Guid id)
{
    Event? ev = await _eventRepository.GetByIdAsync(id);
    if (ev == null) return null;
    return _mapper.Map<EventDto>(ev);
}
```

And ensure `IEventRepository` has `GetByIdAsync(Guid id)` (should be added by Groupe B).

- [ ] **Step 5: Add GetRegistration endpoint to EventsController**

In `API/Controllers/EventsController.cs`, add:

```csharp
[HttpGet("{id}/registration")]
public async Task<ActionResult<RegistrationDto>> GetRegistration(Guid id)
{
    EventDto? eventDto = await _eventService.GetEventByIdAsync(id);
    if (eventDto == null)
        return NotFound();

    RegistrationDto registration = new()
    {
        HelloAssoUrl = eventDto.HelloAssoUrl,
        HasRegistration = eventDto.HelloAssoUrl != null
    };
    return Ok(registration);
}
```

- [ ] **Step 6: Run tests to verify they pass**

```bash
dotnet test AssoInternesBrest.Tests --filter "FullyQualifiedName~EventRegistrationTests"
```

Expected: 3/3 PASS.

- [ ] **Step 7: Run all tests**

```bash
dotnet test AssoInternesBrest.Tests
```

Expected: All tests pass.

- [ ] **Step 8: Commit**

```bash
git add API/DTOs/Events/RegistrationDto.cs API/Controllers/EventsController.cs API/Services/IEventService.cs API/Services/EventService.cs AssoInternesBrest.Tests/Controllers/EventRegistrationTests.cs
git commit -m "feat: add GET /api/events/{id}/registration endpoint"
```

---

### Task 4: Close GitHub Issues

- [ ] **Step 1: Close issue #16 (HelloAssoUrl on Event)**

```bash
gh issue close 16 --comment "Implemented: HelloAssoUrl (string?, nullable) added to Event entity, migration applied, exposed in EventDto/CreateEventDto/UpdateEventDto."
```

- [ ] **Step 2: Close issue #17 (Registration endpoint)**

```bash
gh issue close 17 --comment "Implemented: GET /api/events/{id}/registration (public) — returns { helloAssoUrl, hasRegistration }. hasRegistration = helloAssoUrl != null. Frontend displays HelloAsso iframe widget when hasRegistration is true."
```
