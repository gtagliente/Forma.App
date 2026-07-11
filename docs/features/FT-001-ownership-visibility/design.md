# FT-001 — Exercise Ownership & Visibility — Design

## Stage

Service Architect, design (`../../agents/architect-context.md`).

## Aggregate boundary

No change to the aggregate shape decided centrally (`Forma.Claude/docs/services/exercise-service/architecture.md`): ownership is a scalar attribute of `Exercise` itself, referencing `identity-service`'s `User` **by ID only** (never loaded, no cross-service join, no FK constraint across datastores per ADR-005).

## Data model decision: nullable `OwnerId`, no separate `Visibility` field

`Exercise.OwnerId` is `Guid?`:
- `null` → shared-library Exercise, visible to everyone.
- non-null → private Exercise, visible only to that user.

Rejected a separate `Visibility` enum alongside `OwnerId` — it would be redundant state (visibility is always fully determined by whether an owner exists) and risks the two falling out of sync. One field, one source of truth.

## Auth stand-in (explicit, deliberate simplification)

No `identity-service` exists yet (still a placeholder — `Forma.Claude/docs/services/identity-service/README.md`), and per `Forma.Claude`'s ADR-001 this iteration has exactly one user persona with no delegation. Building real authentication is out of scope for this feature and this repo's current stage. Decision: the acting user is passed explicitly by the caller —
- `CreateExerciseCommand.OwnerId` (`Guid?`, optional — omitted means "create in the shared library").
- `GetAllExerciseQuery.RequestingUserId` (`Guid?`, optional query-string parameter — omitted returns shared-library Exercises only, never "everything").

This is flagged in `open-questions.md` as a gap to close once `identity-service` provides real auth (e.g., replace the explicit parameter with a value derived from the request's authenticated principal) — not something to solve now.

## Name-uniqueness scoping

`IExerciseUniquenessChecker.IsUniqueAsync(string name)` → `IsUniqueAsync(string name, Guid? ownerId)`. Implementation checks `Name == name && OwnerId == ownerId` — EF Core translates nullable-Guid equality correctly, so this naturally scopes shared-vs-shared and per-owner-vs-same-owner without special-casing null.

Enforced at two levels (defense in depth, standard for this codebase's existing uniqueness pattern):
- Domain: `Exercise.Create`/`Update` call the scoped checker before mutating.
- Database: replace the single `UQ_Exercise_Name` unique index with two filtered indexes —
  - `UQ_Exercise_Name_Shared`: unique on `Name` where `OwnerId IS NULL`.
  - `UQ_Exercise_Name_PerOwner`: unique composite on `(Name, OwnerId)` (only meaningful/enforced for non-null `OwnerId` rows — SQL Server unique composite indexes already treat distinct `(Name, OwnerId)` non-null pairs correctly; the shared-scope filtered index above covers the null case, since SQL Server would otherwise treat every `NULL` as distinct and fail to catch shared-name collisions).

## Read side

- `ExerciseQueryModel` gains `OwnerId` (`Guid?`).
- `ExerciseBaseEvent`/`ExerciseCreatedEvent`/`ExerciseUpdatedEvent` gain `OwnerId` so the projection has it to map.
- `IExerciseReadOnlyRepository.GetAllAsync()` → `GetVisibleToAsync(Guid? requestingUserId)`: Mongo filter `OwnerId == null || OwnerId == requestingUserId`.
- `GetAllExerciseQueryHandler`'s cache key must incorporate `RequestingUserId` (currently a single fixed key for all callers — would leak one user's private Exercises into another user's cached response otherwise). New key: `$"{nameof(GetAllExerciseQuery)}:{request.RequestingUserId}"`.

## What's explicitly not built here

- Promotion of a private Exercise to shared (deferred centrally, per `requirements.md`).
- Any access control on *who* may create a shared-library (unowned) Exercise — no curator role decided yet (`Forma.Claude` open item). Anyone may create either kind for now.
- Real authentication — see "Auth stand-in" above.

## Output

Handed to Backend Developer for implementation.
