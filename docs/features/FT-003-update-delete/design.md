# FT-003 — Exercise Update & Delete — Design

## Update

Pure wiring — the domain method already exists and needs no changes. New `UpdateExerciseCommand` (`ExerciseId`, optional `Name`/`Description`/`MuscleGroups`), validator, handler, and controller route — written fresh, not adapted from the commented-out `UpdateCustomerCommand`/`UpdateCustomerCommandHandler` stubs (those reference a nonexistent `Customer` concept and a repository interface that doesn't exist here).

Handler shape (load → mutate → **explicitly re-attach** → save): `repository.GetByIdAsync` returns an **untracked** entity (`AsNoTrackingWithIdentityResolution`, confirmed in `codebase-baseline.md`/FT-002 review). The handler must call `repository.Update(exercise)` after mutating and before `SaveChangesAsync()` — this is the exact bug found (but not fixed, out of scope) in `CreateExerciseResourceCommandHandler` during FT-002's review. FT-003's own new handlers must not repeat it.

## Delete

`Exercise` has no `Delete()` method yet (`ExerciseDeletedEvent` exists but has never been raised). Adding it:

```
public async Task Delete(IExerciseBuilder builder)
{
    if (await builder._contracts.hierarchyChecker.HasChildrenAsync(Id))
        throw new DomainArgumentException("Cannot delete an exercise that has children in the hierarchy.");

    AddDomainEvent(new ExerciseDeletedEvent(Id, MuscleGroups, Name, Description, OwnerId));
}
```

`HasChildrenAsync(ExerciseId id)` is a new method on the existing `IExerciseHierarchyChecker` contract (FT-002) — reusing the contract rather than adding a fourth checker interface, since it's the same "ask infrastructure a hierarchy question" shape as `ExistsAsync`/`WouldCreateCycleAsync`. Implemented in `ExerciseWriteOnlyRepository` as `AnyAsync(e => e.ParentId == id)`.

This is a **domain-level** guard, checked *before* the DB's `DeleteBehavior.Restrict` FK would ever fire — the caller gets a clean `DomainArgumentException` → 400 (via the existing `DomainExceptionToActionResultFilter`), not a raw SQL FK-violation surfacing as a 500. The FK constraint stays as defense in depth (per FT-002's original reasoning), not the primary enforcement mechanism.

Handler shape: load (untracked) → `exercise.Delete(builder)` (raises the check + event) → `repository.Remove(exercise)` → save. `Remove` doesn't have the same untracked-entity problem `Update` does — EF Core's `Remove()` on an untracked entity attaches it as `Deleted` directly (same pattern the commented-out `DeleteCustomerCommandHandler` stub already used correctly), so no extra step needed there.

## API surface

- `PUT /api/exercises/Update` — body: `UpdateExerciseCommand`. 200 on success, 400 (validation or domain error, e.g. name collision), 404 if `ExerciseId` doesn't exist.
- `DELETE /api/exercises/{id:guid}` — 200 on success, 400 (has children), 404 if not found. Route-by-id-in-URL, matching the original commented-out stub's shape (unlike `Update`, which mirrors `Create`'s body-carries-everything convention already live in this controller).

## What's explicitly not built here

- Any authorization check (see `requirements.md`).
- Any cross-service check for Workouts referencing the Exercise being deleted (see `requirements.md` — flagged as a real forward risk, not solved here).
- Soft-delete/archival — hard delete only, per requirements.

## Output

Handed to Backend Developer.
