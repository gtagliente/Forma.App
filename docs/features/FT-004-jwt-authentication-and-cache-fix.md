# FT-004 — JWT Bearer Authentication & GetAll Cache-Invalidation Fix

## Status

Built and verified. Cleared to merge. Unlike FT-001/FT-003, Docker turned out to be available in this environment this time — `Forma.ArchitectureTests` and `Forma.IntegrationTests` (including a real Testcontainers SQL Server) were both run for real, not just built.

## Requirements (Service Analyst)

### Source

Already decided centrally, not invented here: `../../../Forma.Claude/docs/architecture/adr/ADR-007-jwt-bearer-authentication.md` (Accepted, product-owner sign-off) is the authoritative design for real authentication replacing the caller-supplied stand-in this repo's own `docs/features/FT-001-ownership-visibility.md` ("Auth stand-in" section) explicitly flagged as temporary and anticipated replacing. No escalation to the central Analyst needed — this closes a gap FT-001 already named, it does not introduce a new domain concept (Identity stays the minimal `User`, no roles/delegation, per ADR-007's own scoping).

A second, unrelated defect is fixed in the same pass because it was accepted at the same central sign-off and touches the same handlers this feature already has to open: `GetAllExerciseQueryHandler`'s cache key is scoped per-`RequestingUserId` (`$"{nameof(GetAllExerciseQuery)}:{request.RequestingUserId}"`), but `ExerciseEventHandler.ClearCacheAsync` invalidates only the bare `nameof(GetAllExerciseQuery)` key — a silent no-op against the real cache entries, so edited/deleted Exercises don't show up in `GetAll` until the cache TTL (2h absolute / 60s sliding) lapses.

### Functional requirements

1. **Authentication**: every request that creates, updates, or deletes an Exercise (or an ExerciseResource, or the hierarchy edges `SetParent`/`ClearParent`) must carry a valid JWT bearer token issued by `identity-service`, validated against the same HS256 shared secret, `aud`, and lifetime rules `identity-service` uses. An invalid/missing token on these operations is rejected (401) before any handler runs.
2. **Anonymous reads preserved**: `GetAll`/`GetById` remain reachable with no token — shared-library browsing must not require an account (unchanged from FT-001; ADR-007 explicitly re-confirms this, doesn't revisit it).
3. **No caller-asserted identity**: nothing in a request body or query string may claim to be "user X" anymore. The acting user's id always comes from the validated token. `GetAll`'s `requestingUserId` query parameter and `CreateExerciseCommand.OwnerId` (as a client-settable field) both stop existing as bindable inputs.
4. **Shared-vs-private signal without identity**: creating an Exercise still needs to say "shared library" vs. "mine" — but via a non-identity-bearing flag (`Shared: bool`), never by asserting a `Guid`. Creating anything (shared or private) requires authentication — per ADR-007, "anonymous" only makes sense for read-only shared browsing, not for writes.
5. **Ownership enforcement on Update/Delete**: `UpdateExerciseCommandHandler`/`DeleteExerciseCommandHandler` currently have no ownership check at all (confirmed by reading the code — neither the command nor the handler reference an owner/requesting-user id anywhere). This is a pre-existing gap, not one introduced here. Fix: if the target Exercise is private (`OwnerId != null`) and doesn't match the authenticated caller, reject with Forbidden. A shared Exercise (`OwnerId == null`) may be updated/deleted by any authenticated user — no curator/role concept exists yet (still an open central question), so this is the minimal, non-regressive choice consistent with today's behavior (anyone could already create a shared Exercise).
6. **Cache-invalidation fix**: after any successful Create/Update/Delete, the acting user's own `GetAll` cache entry must be invalidated so they see their own change immediately, regardless of whether the event-driven invalidation path (which only knows the Exercise's `OwnerId`, null for shared Exercises) fires correctly.

### Explicitly missing / not this feature's job

- **Content curator / library maintainer role** for the shared library — still unresolved centrally (same open item FT-001 already flagged). This feature does not add one; any authenticated user may still write to the shared library.
- **RSA/JWKS, token refresh, revocation** — out of scope, already decided against centrally (ADR-007, "Alternatives considered").
- **`training-planning-service`'s own JWT wiring** — a separate service, separate repo, out of scope here (ADR-007 covers both but each is implemented independently, no shared assembly).
- **ADR-006 service-to-service endpoints** (e.g. an `IsReferenced`-shaped internal endpoint on this service) staying unauthenticated — explicitly flagged by ADR-007 as a known, deferred follow-up, not solved by this feature.

## Design (Service Architect)

### Authentication wiring (`Forma.PublicApi/Program.cs`)

- New `Auth` configuration section, `JwtOptions : IAppOptions` (`Forma.CoreInfrastructure/AppSettings/JwtOptions.cs`), `ConfigSectionPath => "Auth"`, one required property `JwtSigningKey`. Registered the same way `CacheOptions`/`ConnectionOptions` are — `ConfigureAppSettings()` in `Forma.CoreInfrastructure/ConfigureServices.cs` gains `.AddOptionsWithValidation<JwtOptions>()`. Dev value (`sakjdhkjad872323`, matching `identity-service`'s real dev secret) lands in `appsettings.Development.json` under `"Auth": { "JwtSigningKey": "..." }` — plaintext, consistent with this repo's existing convention for `CacheOptions`/`ConnectionStrings` dev secrets (ADR-007 Consequences already calls this out as an accepted dev-only risk, not something to fix here).
- `Program.cs`: `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => ...)` reading `builder.Configuration.GetOptions<JwtOptions>()` for the signing key, plus `.AddAuthorization()`. `TokenValidationParameters` set exactly per ADR-007's table: `ValidateIssuerSigningKey = true` / symmetric key from `JwtSigningKey`; `ValidateIssuer = false`; `ValidateAudience = true` / `ValidAudience = "fastapi-users:auth"`; `ValidateLifetime = true`; `MapInboundClaims = false`. `app.UseAuthentication()` is already present in the pipeline immediately before `app.UseAuthorization()` (both already there from an earlier scaffold, currently a no-op since no auth scheme was registered — this feature is what makes them do something).
- `IHttpContextAccessor` is already registered (`.AddHttpContextAccessor()` in `Program.cs`) — nothing to add there.

### `ICurrentUserAccessor`

- Interface lives in `Forma.CoreInfrastructure/Abstractions/ICurrentUserAccessor.cs` (same tier as `ICacheService`, so `Forma.Application`/`Forma.Query` handlers can depend on it without a new project reference): `Guid? UserId { get; }` (parsed from the raw `sub` claim — `null` if absent/unparsable, never coalesced to `Guid.Empty`, per the Challenger's note in ADR-007's review), `bool IsAuthenticated { get; }`.
- Implementation `CurrentUserAccessor` lives in `Forma.PublicApi/Services/CurrentUserAccessor.cs` (not `Forma.Infrastructure`/`Forma.CoreInfrastructure` — those are plain `Microsoft.NET.Sdk` class libraries without a `Microsoft.AspNetCore.App` framework reference, so `IHttpContextAccessor`/`ClaimsPrincipal` aren't available there without adding one; `Forma.PublicApi` already is `Microsoft.NET.Sdk.Web` and already depends on `Forma.Application`/`Forma.Infrastructure`/`Forma.Query`, so registering the implementation at the composition root is the natural fit — mirrors how `ICacheService`'s two implementations live in `Forma.Infrastructure` but nothing stops an abstraction's implementation from living one layer further out when the concrete type needs framework types the inner layers deliberately don't reference). Registered `.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>()` in `ServicesCollectionExtensions.cs`, added to the DI chain in `Program.cs`.

### `ExercisesController`

- `GetAll`/`GetById` stay anonymous (no `[Authorize]`). `GetAll([FromQuery] Guid? requestingUserId)` loses the parameter entirely; the handler now reads `currentUserAccessor.UserId` directly inside the action and passes that into `GetAllExerciseQuery`. An anonymous caller naturally gets `null` → shared-library-only view, same filter (`GetVisibleToAsync`, untouched).
- `Create`, `Update`, `Delete`, `SetParent`, `ClearParent`, `CreateExerciseResource` get `[Authorize]`.
- `Create`: after model binding, the controller sets `command.OwnerId = command.Shared ? (Guid?)null : currentUserAccessor.UserId` before calling `mediator.Send`. Since `[Authorize]` guards the action, `UserId` is guaranteed non-null when `Shared` is false.

### `CreateExerciseCommand`

- `OwnerId` (`Guid?`) stays on the command (the handler/domain `Exercise.Create` signature is unchanged) but gets `[JsonIgnore]` so it can no longer be set via the request body — the controller is the only place that assigns it, after model binding, per ADR-007's "`[JsonIgnore]` or equivalent... populated by the controller after model binding" wording.
- New `public bool Shared { get; set; }` — the only client-supplied signal for "create in the shared library" vs. "create as mine." No identity-bearing value crosses the wire.

### Ownership enforcement (`UpdateExerciseCommandHandler`/`DeleteExerciseCommandHandler`)

- Both handlers gain a constructor dependency on `ICurrentUserAccessor`. After `repository.GetByIdAsync(...)` returns a non-null Exercise and before mutating it:
  ```
  if (exercise.OwnerId is not null && exercise.OwnerId != currentUserAccessor.UserId)
      return Result.Forbidden();
  ```
  `Result.Forbidden()` already maps to `ForbidResult` (403) via the existing `ResultExtensions.ToHttpNonSuccessResult` switch — no new plumbing needed there.
- A shared Exercise (`OwnerId == null`) is never subject to this check — any authenticated user (guaranteed by the controller's new `[Authorize]`) may update/delete it, per Requirements #5.

### Cache-invalidation fix

- New `ExerciseCacheKeys` static helper in `Forma.CoreInfrastructure/Caching/ExerciseCacheKeys.cs` — the natural shared home since both `Forma.Application` and `Forma.Query` already reference `Forma.CoreInfrastructure` (confirmed from both `.csproj` files; no new project reference introduced). `ForUser(Guid? userId) => $"{QueryName}:{userId}"`, where `QueryName` is a `private const string` literal `"GetAllExerciseQuery"` — kept as a literal rather than `nameof(GetAllExerciseQuery)` because that type lives in `Forma.Query`, which `Forma.CoreInfrastructure` must not depend on (wrong dependency direction for a shared-kernel-tier project). The literal is guaranteed identical to today's `nameof`-produced string, so existing cache entries aren't invalidated/orphaned by the rename.
- `GetAllExerciseQueryHandler` (`Forma.Query`) builds its cache key via `ExerciseCacheKeys.ForUser(request.RequestingUserId)` instead of the inline interpolated string.
- `CreateExerciseCommandHandler`/`UpdateExerciseCommandHandler`/`DeleteExerciseCommandHandler` (`Forma.Application`) each gain a constructor dependency on `ICacheService` (already registered; not previously injected into these three) and, after a successful `SaveChangesAsync`, call `await cacheService.RemoveAsync(ExerciseCacheKeys.ForUser(currentUserAccessor.UserId))`. This is the load-bearing fix — the command handler always knows the real acting user, unlike the event handler, which only has the Exercise's `OwnerId` (null for shared Exercises, useless as "who acted").
- `ExerciseEventHandler.ClearCacheAsync` (`Forma.Query`) is also fixed to call `ExerciseCacheKeys.ForUser(notification.OwnerId)` instead of the bare unscoped key — kept as a cheap redundant complement (it happens to invalidate the *owner's* cache entry on any event for their own private Exercise, and the shared-library entry — `ForUser(null)` — on any shared-Exercise event), not the primary mechanism relied on to fix the bug.

### What's explicitly not built here

- Any curator/role system for shared-library writes (Requirements above, and ADR-007 itself, both defer this).
- `training-planning-service`'s JWT wiring (separate repo, separate implementation per ADR-007).
- Closing the ADR-006 service-to-service unauthenticated-endpoint gap (explicitly flagged by ADR-007 as a follow-up, not this feature's job).

## Review (Developer peer review + Service Architect conformance review)

### Conformance against Design

- `JwtOptions` added under `Forma.CoreInfrastructure/AppSettings`, wired through the existing `AddOptionsWithValidation<T>`/`IAppOptions` pattern — matches "Authentication wiring".
- `Program.cs` now calls `AddAuthentication().AddJwtBearer(...)` and `AddAuthorization()` with the exact `TokenValidationParameters` table from ADR-007; pre-existing `UseAuthentication()`/`UseAuthorization()` calls (previously inert) now do real work.
- `ICurrentUserAccessor` interface in `Forma.CoreInfrastructure/Abstractions`, implementation in `Forma.PublicApi/Services`, registered in `ServicesCollectionExtensions` — matches "ICurrentUserAccessor" design, including the reasoning for why the implementation sits one layer further out than `ICacheService`'s.
- `ExercisesController`: `[Authorize]` added to `Create`, `Update`, `Delete`, `SetParent`, `ClearParent`, `CreateExerciseResource`; `GetAll`/`GetById` left anonymous; `GetAll`'s `requestingUserId` parameter removed, replaced by `currentUserAccessor.UserId` read inside the action — matches design.
- `CreateExerciseCommand.OwnerId` now `[JsonIgnore]`; new `Shared` bool added; controller sets `OwnerId` post-binding — matches design.
- `UpdateExerciseCommandHandler`/`DeleteExerciseCommandHandler` both gained the Forbidden-on-mismatch check, scoped to non-null `OwnerId` only — matches design and Requirements #5 exactly.
- `ExerciseCacheKeys` added to `Forma.CoreInfrastructure/Caching`, consumed by `GetAllExerciseQueryHandler` and all three command handlers (`Create`/`Update`/`Delete`), plus `ExerciseEventHandler.ClearCacheAsync` — matches "Cache-invalidation fix" exactly, including the literal-vs-`nameof` reasoning.

### Verified

- `dotnet build` run after each meaningful change (package reference add, each new file, each handler edit) — final state is 0 errors, 0 new warnings beyond the one pre-existing `ASPDEPR007` (unrelated, from `IncludeOpenAPIAnalyzers`).
- `dotnet build`/`dotnet test tests/Forma.ArchitectureTests` — passes (1/1), unaffected by this feature.
- `dotnet test tests/Forma.IntegrationTests` — **run for real** (Docker was available in this environment, unlike FT-001/FT-003's assumption). Result: 4/4 passed, including a real `Testcontainers.MsSql` SQL Server instance and a real ASP.NET Core JWT bearer pipeline validating a locally-minted test token.
- Grepped for every changed signature's call sites (`GetAllExerciseQuery` construction, `CreateExerciseCommand.OwnerId` usages, controller route for `GetAll`) — no stale caller found outside what was already updated.

### Findings

None blocking. Two deviations from a literal reading of the design, both discovered only once the integration tests were actually run (not by inspection) and both explained here:

1. `ExerciseCacheKeys`'s `QueryName` is a hardcoded string literal, not `nameof(GetAllExerciseQuery)`, because the helper lives in a lower layer (`Forma.CoreInfrastructure`) that must not reference `Forma.Query`. The produced string is identical, so this has no behavioral effect.
2. **Test fixture updates, not part of the original design but required to keep the existing suite green**:
   - `appsettings.IntegrationTesting.json` needed its own `Auth:JwtSigningKey` (a test-only value, distinct from the real dev secret) — without it, `JwtOptions`'s `[Required]` + `ValidateOnStart()` throws at test-host startup, failing every integration test, not just the auth-related ones.
   - `ExercisesControllerTests`'s two `Create` tests and both `CreateExerciseResource` tests now hit `[Authorize]`-guarded actions; added a small `TestJwtTokenFactory` (mints an HS256 token shaped like `identity-service`'s real output — `sub`/`aud`/`exp`, no `iss` — signed with the test-only key) and a `CreateAuthenticatedClient()` test helper that attaches it. Without this, all four tests would 401 instead of exercising the real handler logic.
   - `Should_ReturnsHttpStatus400_When_Post_ExerciseNameIsNotUnique` additionally needed `Shared: true` added to its `Faker<CreateExerciseCommand>` rules — the seeded conflicting Exercise is shared (`OwnerId IS NULL` in the raw-SQL insert), and the test's own new Exercise now defaults to *private* (owned by whatever random `sub` the test token carries) unless it explicitly asks for the shared library, so the uniqueness check would no longer collide without this. Caught precisely because the test was run, not just read.

## Central Architect Gate

*(`Forma.Claude`'s system-wide Architect — cross-service impact only, not a second local design/code-quality pass.)*

### Cross-service impact assessment

- Implements ADR-007 exactly as specified for `exercise-service`'s half of that decision — no local deviation that changes the cross-service contract (still HS256, same secret config key intent, same claims). `training-planning-service`'s independent implementation of the same ADR is out of scope here.
- No new domain concept, no new inter-service API/contract. `ICurrentUserAccessor` and `ExerciseCacheKeys` are both internal-to-`exercise-service` abstractions.
- Follow-up already flagged centrally by ADR-007 itself (ADR-006 service-to-service endpoints remaining unauthenticated) is not re-litigated here — it's the same tracked gap, not a new one introduced by this feature.

**Verdict: no promotion to `Forma.Claude`'s central ADRs needed.** This feature implements an already-accepted ADR verbatim; nothing decided here needs to flow back upstream beyond the "now built" status update.

### Central knowledge updated

- `Forma.Claude/docs/services/exercise-service/domain.md` / `open-questions.md` (if either lists "real authentication" as outstanding for this service) should be marked resolved for `exercise-service`'s half of ADR-007 — `training-planning-service`'s half remains open in that service's own repo.
