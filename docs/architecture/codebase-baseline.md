# exercise-service — Codebase Baseline

_Reconnaissance pass over `src/` before running any feature through `../agents/process.md`. Read this before re-exploring the codebase in a future session — only re-read source directly if this looks stale or a specific detail isn't covered here._

## Solution shape

CQRS, physically separated:

- **`Forma.Domain`** — aggregates, value objects, domain events. No infra dependencies; external checks (uniqueness, etc.) are injected via a **builder+contracts** pattern (`IExerciseBuilder.Contracts`) so the aggregate's static `Create`/instance `Update` methods can call them without the domain layer referencing infrastructure.
- **`Forma.Application`** — MediatR commands/handlers/validators (FluentValidation) per use case, under `Exercise/Commands`, `Exercise/Handlers`. Returns `Ardalis.Result`.
- **`Forma.CoreContext`** — shared kernel: `BaseEntity` (see bug below), `BaseEvent`, domain exception types.
- **`Forma.CoreInfrastructure`** — cross-cutting abstractions (`IWriteOnlyRepository`, `IUnitOfWork`, `ICacheService`).
- **`Forma.Infrastructure`** — EF Core write side. `WriteDbContext` (SQL, has `Exercise`, `StaticValueObjects` DbSets — `ExerciseResource` is a child, not a DbSet) + `EventStoreDbContext` (a side event-log, not true event sourcing — the write model is plain EF Core CRUD; domain events are additionally persisted to an event store and dispatched via MediatR notifications for the read-side projection below).
- **`Forma.Query`** — read side. `NoSqlDbContext`/`ISynchronizeDb` (Mongo-shaped) holds denormalized `ExerciseQueryModel`, kept in sync by `ExerciseEventHandler` (a MediatR `INotificationHandler<ExerciseCreatedEvent>`) mapping via AutoMapper (`EventToQueryModelProfile`) — **only reacts to Created**, not Updated/Deleted, and doesn't project `Resources`.
- **`Forma.PublicApi`** — ASP.NET controllers, EF Core migrations, DI wiring, `Program.cs`.

## Exercise aggregate (`Forma.Domain/Entities/ExerciseAggregate/Exercise.cs`)

- Aggregate root: `Name` (unique), `Description`, `MuscleGroups` (value object list, backed by private field `_muscleGroups`, EF-mapped via a `ValueConverter`+`ValueComparer` to a delimited string column), child collection `Resources` (`ExerciseResource`, whose `Create` is `internal` — only reachable via `Exercise.AddResource`, enforced by an architecture test).
- Construction is factory-based: `Exercise.Create(builder, name, muscleGroups, description)` (static, async — checks name uniqueness via the injected checker) and instance `Update(builder, name?, description?, muscleGroups?)`. No public setters. **Keep this pattern for any new mutation.**
- `MuscleGroup` is a controlled vocabulary via a generic `StaticValueObjects` EF mechanism, not a raw enum column.

## ~~⚠ Known live bug: dual identity on `Exercise` (BaseEntity.Id vs ExerciseId)~~ — Fixed, see `adr/ADR-001-strongly-typed-exercise-id.md`

Not just tech debt — this broke creating a second `Exercise` against a real relational DB:

- `BaseEntity` (`Forma.CoreContext`) declares `Id` (`Guid`, `private init`), and `EntityTypeBuilderExtensions.ConfigureBaseEntity<TEntity>` maps `HasKey(e => e.Id)`.
- `Exercise` additionally declares its own `ExerciseId` (`readonly record struct ExerciseId(Guid Value)`) as a "convenience property" — but **nothing in `Exercise.Create` or `Exercise.Update` ever assigns it**. It sits at its default, `ExerciseId(Guid.Empty)`, for every instance.
- `ExerciseConfiguration.Configure` calls `builder.HasKey(e => e.ExerciseId)` **after** `ConfigureBaseEntity` already set the key to `Id` — this second `HasKey` call replaces the first, so the actual EF primary key ends up being `ExerciseId`, mapped `ValueGeneratedNever()`.
- Net effect: every newly created `Exercise` is inserted with PK `00000000-0000-0000-0000-000000000000`. The **first** insert succeeds; any **second** `Exercise` ever created hits a primary-key violation. Domain events (`ExerciseCreatedEvent(exercise.ExerciseId.Value, ...)`) also carry `Guid.Empty` as the aggregate ID, which would corrupt the read-side projection's `Id` too.
- **A fix already exists, unmerged**: local branch `fix/ExerciseResourceMigration_stronglytypedIds` (tip `3bc7d4a`, base fix commit `cc0409d` "Strongly typed Ids in BaseEntity with generics") makes `BaseEntity` generic over `TId`, removing the duplicate-identity source entirely. Diff vs. `feature/claude_integration` touches ~22 files (`BaseEntity`, `IBaseEntity`, `Exercise`, `ExerciseResource`, events, `ExerciseConfiguration`, `ExerciseResourceConfiguration`, `UnitOfWork`, repositories, DI, one migration).
- **This blocks reliable feature work on `Exercise`, not just the hierarchy self-reference** (architecture.md's original framing undersold it) — treat resolving this as a prerequisite for the very next feature touching `Exercise` creation, not an optional cleanup.

## ~~Template debt (from the "Shop/Customer" starting point)~~ — Cleaned up as of FT-003

_As found at the start of this pass, kept for history:_ `IExerciseUniquenessChecker` lived in namespace `Shop.Domain.Entities.CustomerAggregate`; `ExerciseWriteOnlyRepository` referenced the same namespace; `Forma.Query.EventHandlers.ExerciseEventHandler<T>` sat in `Shop.Query.EventHandlers`; `{Update,Delete}CustomerCommandHandler.cs` were fully commented-out `Customer`-shaped stubs.

**Now**: `IExerciseUniquenessChecker`/`IExerciseBuilder`/`ExerciseWriteOnlyRepository`/`ConfigureServices`/`ExerciseEventHandler` are all in proper `Forma.*` namespaces; `DeleteCustomerCommandHandler.cs` is deleted, `UpdateExerciseCommandHandler.cs`/`DeleteExerciseCommandHandler.cs` replace the dead stubs (`UpdateCustomerCommandHandler.cs` itself is still present but unreferenced dead code, same tier as `Forma.Domain.ValueObjects.Email` and the `Customer`-folder `GetCustomerByIdQuery*` files — harmless, not template debt in the "misleads a reader" sense anymore).

## Update/Delete status — Resolved as of FT-003

_As found_: `Exercise.Update(...)` was already implemented and functionally complete; `Exercise` had no `Delete()`; the Application/API layers were entirely stubbed out.

**Now**: full CRUD wired end-to-end — `Exercise.Delete(builder)` added (blocks via `IExerciseHierarchyChecker.HasChildrenAsync` before raising `ExerciseDeletedEvent`), `UpdateExerciseCommand`/`DeleteExerciseCommand` + handlers + controller routes (`PUT /api/exercises/Update`, `DELETE /api/exercises/{id:guid}`) all live. See `../features/FT-003-update-delete/`.

## Persistence / migrations

Migrations live under `Forma.PublicApi/Migrations/` (write side) and `Forma.PublicApi/Migrations/EventStore/` (event store side). Current migrations: `InitialMigration`, `RowVersionMigration`, `muscleGroupsMigration`, `StaticValueObjsMigration`. No `dotnet-ef` run has been verified as part of this pass — assume it needs to be run for real (`dotnet ef migrations add ... --project src/Forma.Infrastructure --startup-project src/Forma.PublicApi` was the likely working invocation given `WriteDbContext` lives in `Forma.Infrastructure` and DI/`Program.cs` in `Forma.PublicApi`) rather than hand-authored, to avoid drifting from the real model snapshot.

## Status

First pass, written while starting Feature 1 (Ownership/Visibility) of the feature pipeline (`../agents/process.md`). Update this file when a later pass finds it stale, rather than re-deriving all of the above from scratch.
