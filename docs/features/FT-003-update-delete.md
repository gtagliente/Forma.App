# FT-003 — Exercise Update & Delete

## Status

Built. Cleared to merge (pending: user adds/runs integration test coverage locally — Docker isn't available in this environment; see Review below).

## Requirements (Service Analyst)

### Source

Implied by CRUD completeness of an already-decided concept (Exercise), not a new domain concept — no escalation needed. Confirmed via `../architecture/codebase-baseline.md`: the domain method `Exercise.Update(...)` already exists and is functionally complete; only the Application/API wiring is missing (the existing `UpdateExerciseCommand`/controller route are commented-out template stubs referencing a nonexistent `Customer` concept, not usable as-is). `Exercise.Delete()` doesn't exist at all yet.

### Functional requirements

1. **Update**: allow changing an existing Exercise's `Name`, `Description`, and `MuscleGroups` — exactly the fields `Exercise.Update` already supports. Renaming re-checks uniqueness within the Exercise's own ownership scope (already implemented, per FT-001).
2. **Update does not touch** `OwnerId` (fixed at creation, FT-001) or `ParentId` (has its own dedicated `SetParent`/`ClearParent` operations, FT-002) — keeps this feature's scope to what the domain method already models, not a re-opening of prior decisions.
3. **Delete**: allow removing an Exercise entirely (hard delete — no soft-delete/archival concept exists anywhere in the domain model).
4. **Cannot delete an Exercise that has children** in the hierarchy (FT-002's `ParentId` self-reference, DB-enforced via `DeleteBehavior.Restrict`). This must surface as a clear domain-level error to the caller, not an unhandled FK-violation exception.
5. Deleting an Exercise also removes its attached `ExerciseResource`s (already cascade-configured, FT-002/pre-existing) — this is correct: they're owned child entities with no existence outside their parent.

### Explicitly missing / flagged, not decided here

- **No authorization on who may Update/Delete an Exercise** — same gap already tracked centrally as the "content curator" open question (`Forma.Claude` open-questions #1). This feature doesn't invent an authz model any more than FT-001's Create did; anyone may Update/Delete any Exercise they can address by ID, for now.
- **Cross-service reference integrity**: `training-planning-service`'s `Workout` is expected to reference `Exercise` by identity only (`Forma.Claude/docs/product/domain-model.md`). Deleting an Exercise a Workout references would orphan that reference — this service has no way to detect or prevent that today (no inter-service integration pattern decided yet, `Forma.Claude/docs/architecture/integration-patterns.md` is still empty, and `training-planning-service` didn't exist yet at the time this was written either). **Flagging as a real forward risk, not solving it here** — solving it would mean inventing a cross-service check against a service that doesn't exist, which is exactly the premature complexity `CLAUDE.md` warns against.

## Design (Service Architect)

### Update

Pure wiring — the domain method already exists and needs no changes. New `UpdateExerciseCommand` (`ExerciseId`, optional `Name`/`Description`/`MuscleGroups`), validator, handler, and controller route — written fresh, not adapted from the commented-out `UpdateCustomerCommand`/`UpdateCustomerCommandHandler` stubs (those reference a nonexistent `Customer` concept and a repository interface that doesn't exist here).

Handler shape (load → mutate → **explicitly re-attach** → save): `repository.GetByIdAsync` returns an **untracked** entity (`AsNoTrackingWithIdentityResolution`, confirmed in `codebase-baseline.md`/FT-002's review). The handler must call `repository.Update(exercise)` after mutating and before `SaveChangesAsync()` — this is the exact bug found (but not fixed, out of scope) in `CreateExerciseResourceCommandHandler` during FT-002's review. FT-003's own new handlers must not repeat it.

### Delete

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

### API surface

- `PUT /api/exercises/Update` — body: `UpdateExerciseCommand`. 200 on success, 400 (validation or domain error, e.g. name collision), 404 if `ExerciseId` doesn't exist.
- `DELETE /api/exercises/{id:guid}` — 200 on success, 400 (has children), 404 if not found. Route-by-id-in-URL, matching the original commented-out stub's shape (unlike `Update`, which mirrors `Create`'s body-carries-everything convention already live in this controller).

### What's explicitly not built here

- Any authorization check (see Requirements above).
- Any cross-service check for Workouts referencing the Exercise being deleted (see Requirements above — flagged as a real forward risk, not solved here).
- Soft-delete/archival — hard delete only, per requirements.

## Review (Developer peer review + Service Architect conformance review)

### Conformance against Design

- `UpdateExerciseCommand`/handler written fresh (not adapted from the dead `Update`/`DeleteCustomerCommand*` stubs, which remain untouched, still commented, still tracked as open item 9). Handler explicitly calls `repository.Update(exercise)` after mutating — matches the "must not repeat the untracked-entity bug" requirement.
- `Exercise.Delete(builder)` added: checks `HasChildrenAsync` (new method on the existing `IExerciseHierarchyChecker` contract, reusing it rather than adding a fourth checker interface) before raising `ExerciseDeletedEvent` — matches design.
- `DeleteExerciseCommandHandler` calls `Delete` then `repository.Remove(exercise)`, in that order, so the domain event raised by `Delete` is present on the entity by the time `Remove` attaches it to the change tracker — matches the reasoning that `Remove` (unlike `Update`) doesn't need a separate explicit-attach step.
- Routes: `PUT /api/exercises/Update` (body-carries-everything, matching `Create`), `DELETE /api/exercises/{id:guid}` (id-in-URL, matching the original commented-out stub's shape) — matches design's stated convention split.

### Verified

- `dotnet build` clean across `Forma.PublicApi`, `Forma.ArchitectureTests`, `Forma.IntegrationTests`.
- `Forma.ArchitectureTests` suite passes.
- No schema change — `Update`/`Delete` don't touch the `Exercise` table shape, so no new EF migration needed (confirmed by inspecting `ExerciseConfiguration` — nothing added).
- Grepped for stale references to `UpdateCustomerCommand`/`DeleteCustomerCommand` — only the still-dead, still-commented original stub files themselves; nothing live references them.

### Findings

None blocking.

### Not verified (environment gap, carried from FT-001/FT-002)

- No Docker here to run `Forma.IntegrationTests`. No integration-test coverage exists yet for Update, Delete, or the children-block-delete path — flagging for the user to add coverage and run locally before merging, same as the prior two features.

## Central Architect Gate

*(`Forma.Claude`'s system-wide Architect — cross-service impact only, not a second local design/code-quality pass.)*

### Cross-service impact assessment

- Update/Delete are entirely intra-aggregate — no new cross-service surface.
- **Real forward risk, explicitly not solved**: `training-planning-service`'s `Workout` is expected to reference `Exercise` by identity only (`Forma.Claude/docs/product/domain-model.md`). This feature makes `Exercise` deletable, and nothing anywhere today can prevent or even detect deleting an `Exercise` that a `Workout` references — `training-planning-service` didn't exist yet at the time this was written, and the inter-service integration pattern (`Forma.Claude/docs/architecture/integration-patterns.md`) is still empty. This isn't a defect introduced by this feature (Exercise was never deletable before), but it's the first point the risk became concrete rather than theoretical.

**Verdict: no promotion of a new decision** — there's no decision to make yet, only a dependency to flag for whenever `training-planning-service` and the integration-pattern question get addressed. Recorded centrally below so it isn't lost.

### Central knowledge updated

- `Forma.Claude/docs/services/exercise-service/domain.md` — Exercise is now fully CRUD (Create/Update/Delete all wired, plus ownership/visibility and hierarchy).
- `Forma.Claude/docs/services/exercise-service/open-questions.md` — new item added: Exercise deletion has no cross-service safeguard against orphaning `Workout` references, pending `training-planning-service` and the integration-pattern decision.
- `Forma.Claude/docs/branches/analysis/pending-items.md` — the inter-service integration pattern decision (already flagged as "next steps for this branch") now has a second concrete driver beyond the existing Workout-Version-at-session-start case: Exercise deletion.
