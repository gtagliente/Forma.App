# FT-001 — Exercise Ownership & Visibility

## Status

Built. Cleared to merge (pending: user runs integration tests locally — Docker isn't available in this environment; see Review below).

## Requirements (Service Analyst)

### Source

Already decided centrally, not invented here: `domain-slice.md` → Exercise, "Ownership"; `Forma.Claude/docs/product/domain-model.md` → Exercise, "Ownership (decided)"; `Forma.Claude/docs/architecture/adr/ADR-001-user-model-iteration-1.md` (single normal-user model, no delegation). No escalation to the central Analyst needed — this is a new *capability* on an already-defined concept, not a new domain concept.

### Functional requirements

1. An `Exercise` is either:
   - part of the **shared library** — visible to every user, or
   - **privately owned** by exactly one user — visible only to that user.
2. `Exercise` creation must let the caller specify which of the two applies, and for a private Exercise, which user owns it.
3. Listing Exercises must return only: every shared-library Exercise, plus the requesting user's own private Exercises. Never another user's private Exercises.
4. Name uniqueness is **scoped to visibility**, not global:
   - Two Exercise names must be unique within the shared library.
   - A given user's private Exercise names must be unique among *that user's own* private Exercises.
   - A private Exercise's name **may** coincide with a shared-library Exercise's name, or with another user's private Exercise name — those are different, non-comparable scopes from the owning user's point of view.
5. Ownership is fixed at creation. Changing an Exercise's owner or promoting a private Exercise into the shared library is **out of scope** for this feature — already deferred centrally (`domain-slice.md`: "a mechanism for a user to promote a private Exercise into the shared library is deliberately deferred").
6. Cross-visibility interaction with the Exercise hierarchy (can a private Exercise specialize a shared one, or vice versa?) is **out of scope** here — belongs to FT-002 (Hierarchy), and is itself still an open central question (`open-questions.md` #6 in `Forma.Claude`).

### Explicitly missing / not this feature's job

- **No real user authentication exists yet** (`identity-service` is a placeholder, not implemented — see `Forma.Claude/docs/services/identity-service/README.md`). This feature cannot derive "the requesting user" from a validated identity. Per ADR-001's single-normal-user model, the Service Architect must decide a pragmatic stand-in (e.g., an explicit caller-supplied user identifier) rather than inventing an auth system — that would be solving a problem nobody asked this feature to solve yet.
- **Content curator / library maintainer** for the shared library (`Forma.Claude` open item, Users #3) is unresolved — this feature does not add any mechanism for *who* is allowed to create a shared-library (non-owned) Exercise. Default assumption, to be confirmed by the Architect: for iteration 1, any caller may create either kind; access control on shared-library writes is a future concern once a curator role is decided.

## Design (Service Architect)

### Aggregate boundary

No change to the aggregate shape decided centrally (`Forma.Claude/docs/services/exercise-service/architecture.md`): ownership is a scalar attribute of `Exercise` itself, referencing `identity-service`'s `User` **by ID only** (never loaded, no cross-service join, no FK constraint across datastores per ADR-005).

### Data model decision: nullable `OwnerId`, no separate `Visibility` field

`Exercise.OwnerId` is `Guid?`:
- `null` → shared-library Exercise, visible to everyone.
- non-null → private Exercise, visible only to that user.

Rejected a separate `Visibility` enum alongside `OwnerId` — it would be redundant state (visibility is always fully determined by whether an owner exists) and risks the two falling out of sync. One field, one source of truth.

### Auth stand-in (explicit, deliberate simplification)

No `identity-service` exists yet (still a placeholder — `Forma.Claude/docs/services/identity-service/README.md`), and per `Forma.Claude`'s ADR-001 this iteration has exactly one user persona with no delegation. Building real authentication is out of scope for this feature and this repo's current stage. Decision: the acting user is passed explicitly by the caller —
- `CreateExerciseCommand.OwnerId` (`Guid?`, optional — omitted means "create in the shared library").
- `GetAllExerciseQuery.RequestingUserId` (`Guid?`, optional query-string parameter — omitted returns shared-library Exercises only, never "everything").

This is flagged in `open-questions.md` as a gap to close once `identity-service` provides real auth (e.g., replace the explicit parameter with a value derived from the request's authenticated principal) — not something to solve now.

### Name-uniqueness scoping

`IExerciseUniquenessChecker.IsUniqueAsync(string name)` → `IsUniqueAsync(string name, Guid? ownerId)`. Implementation checks `Name == name && OwnerId == ownerId` — EF Core translates nullable-Guid equality correctly, so this naturally scopes shared-vs-shared and per-owner-vs-same-owner without special-casing null.

Enforced at two levels (defense in depth, standard for this codebase's existing uniqueness pattern):
- Domain: `Exercise.Create`/`Update` call the scoped checker before mutating.
- Database: replace the single `UQ_Exercise_Name` unique index with two filtered indexes —
  - `UQ_Exercise_Name_Shared`: unique on `Name` where `OwnerId IS NULL`.
  - `UQ_Exercise_Name_PerOwner`: unique composite on `(Name, OwnerId)` (only meaningful/enforced for non-null `OwnerId` rows — SQL Server unique composite indexes already treat distinct `(Name, OwnerId)` non-null pairs correctly; the shared-scope filtered index above covers the null case, since SQL Server would otherwise treat every `NULL` as distinct and fail to catch shared-name collisions).

### Read side

- `ExerciseQueryModel` gains `OwnerId` (`Guid?`).
- `ExerciseBaseEvent`/`ExerciseCreatedEvent`/`ExerciseUpdatedEvent` gain `OwnerId` so the projection has it to map.
- `IExerciseReadOnlyRepository.GetAllAsync()` → `GetVisibleToAsync(Guid? requestingUserId)`: Mongo filter `OwnerId == null || OwnerId == requestingUserId`.
- `GetAllExerciseQueryHandler`'s cache key must incorporate `RequestingUserId` (currently a single fixed key for all callers — would leak one user's private Exercises into another user's cached response otherwise). New key: `$"{nameof(GetAllExerciseQuery)}:{request.RequestingUserId}"`.

### What's explicitly not built here

- Promotion of a private Exercise to shared (deferred centrally, per Requirements above).
- Any access control on *who* may create a shared-library (unowned) Exercise — no curator role decided yet (`Forma.Claude` open item). Anyone may create either kind for now.
- Real authentication — see "Auth stand-in" above.

## Review (Developer peer review + Service Architect conformance review)

### Conformance against Design

- `OwnerId` (`Guid?`) added to `Exercise` as a scalar attribute, no new aggregate — matches "Aggregate boundary" decision.
- Nullability alone carries visibility (no separate `Visibility` field) — matches "Data model decision".
- `CreateExerciseCommand.OwnerId` / `GetAllExerciseQuery.RequestingUserId` are both caller-supplied, both optional — matches "Auth stand-in".
- Uniqueness checker signature scoped to `(name, ownerId)`, enforced in both `Exercise.Create`/`Update` and two filtered unique DB indexes (`UQ_Exercise_Name_Shared`, `UQ_Exercise_Name_PerOwner`) — matches "Name-uniqueness scoping".
- Read side (`ExerciseQueryModel`, event→model mapping, `GetVisibleToAsync` Mongo filter, per-user cache key) all updated — matches "Read side".

### Verified

- `dotnet build` clean across `Forma.PublicApi`, `Forma.ArchitectureTests`, `Forma.IntegrationTests` (0 errors).
- `Forma.ArchitectureTests` suite passes (aggregate-boundary guard on `ExerciseResource.Create` still holds — untouched by this feature).
- EF Core migration generated via `dotnet ef migrations add` (not hand-authored) — `Up`/`Down` reviewed, matches the intended column + filtered-index shape.
- Grepped for stale call sites against every changed signature (`GetAllAsync()`, single-arg `IsUniqueAsync`, parameterless `GetAllExerciseQuery`) — none found.

### Not verified (environment gap, not a code defect)

- **Integration tests were not run** — `Forma.IntegrationTests` uses `Testcontainers.MsSql`, and Docker is not available in this environment. `ExercisesControllerTests` (existing `Should_ReturnsHttpStatus400_When_Post_ExerciseNameIsNotUnique`, seeding a shared Exercise via raw SQL with no `OwnerId` column in the insert list) should still pass as-is — the new column is nullable and defaults to `NULL`, i.e. "shared", which is exactly the scope that test's uniqueness check exercises — but this is reasoned from reading the test, not an actual run. **Flagging for the user to run `dotnet test tests/Forma.IntegrationTests` locally (Docker available) before merging.**

### Findings

None blocking. One pre-existing gap noted but explicitly out of scope for this feature (per Requirements above): no access control on who may create a shared-library Exercise — anyone can pass `OwnerId: null`. Tracked centrally as the "content curator" open question, not introduced or worsened by this feature.

## Central Architect Gate

*(`Forma.Claude`'s system-wide Architect — cross-service impact only, not a second local design/code-quality pass.)*

### Cross-service impact assessment

- `Exercise.OwnerId` references `identity-service`'s `User` **by ID only** — no call to `identity-service`, no shared datastore, no new API contract between services. This is exactly the shape `Forma.Claude/docs/services/exercise-service/architecture.md` already anticipated for ownership before this feature existed.
- The caller-supplied `OwnerId`/`RequestingUserId` stand-in (no real auth) doesn't create a contract another service depends on — it's an internal-to-`exercise-service` API shape that will change when `identity-service` exists, not something other services integrate against today.
- No new domain concept was invented — ownership/visibility was already decided centrally (`Forma.Claude/docs/product/domain-model.md`, ADR-001).

**Verdict: no promotion to `Forma.Claude`'s central ADRs needed.** Everything here stays local to `exercise-service`, recorded in `../architecture/adr/ADR-001-strongly-typed-exercise-id.md` (the ID-fix decision) and this feature's own Requirements/Design above.

### Central knowledge updated

Per Context Promotion Rules, the *fact that this is now built* (not a new decision) is reflected back into `Forma.Claude`'s exercise-service knowledge so it stays accurate for future central-loop passes:

- `Forma.Claude/docs/services/exercise-service/domain.md` — "What this service still needs to build" no longer lists ownership/visibility.
- `Forma.Claude/docs/services/exercise-service/open-questions.md` — item 8 (strongly-typed ID migration) marked resolved.
