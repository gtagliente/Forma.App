# FT-002 — Exercise Hierarchy — Design

## Aggregate boundary

Per `Forma.Claude/docs/services/exercise-service/architecture.md` (already decided centrally): parent and child are **separate aggregate instances**, each its own transaction boundary, even though both are the same aggregate type. The child holds a reference to the parent's `ExerciseId` only — never a loaded parent object graph. No new aggregate, no change to the one-aggregate-per-`Exercise` shape.

## Data model

`Exercise.ParentId` (`ExerciseId?`, nullable — no parent by default).

## Mutation API

Dedicated methods, matching the existing `AddResource`/`RemoveResource` precedent rather than overloading `Update` (a nullable `ExerciseId?` parameter on `Update` couldn't distinguish "leave parent unchanged" from "clear the parent" — both would be `null`):

- `SetParent(IExerciseBuilder builder, ExerciseId parentId)` — validates and assigns.
- `ClearParent()` — removes the parent link (no validation needed to remove a reference).

## Validation (on `SetParent`)

Three checks, all requiring infrastructure the domain layer can't reach directly — follows the existing `uniquenessChecker` contract-injection pattern (`IExerciseBuilder.Contracts`):

1. **Existence**: `parentId` must reference a real `Exercise`. New contract `IExerciseHierarchyChecker.ExistsAsync(ExerciseId id)`.
2. **No self-parenting**: `parentId != this.Id`, checked directly in the domain method — no infra needed.
3. **No cycles**: the proposed parent's ancestor chain must not already contain this Exercise. New contract method `IExerciseHierarchyChecker.WouldCreateCycleAsync(ExerciseId childId, ExerciseId proposedParentId)` — implemented in `Forma.Infrastructure` by walking `ParentId` upward from `proposedParentId` until null or `childId` is found. Chains are expected to be shallow (a curated exercise library, not user-generated arbitrary depth), so a straightforward loop is adequate — no need for a recursive CTE for iteration 1.

Both checks live on a new `IExerciseHierarchyChecker` contract (mirrors `IExerciseUniquenessChecker`), added to `IExerciseBuilder.Contracts` alongside the existing two checkers, and implemented by `ExerciseWriteOnlyRepository` (same class that already implements the other two checker interfaces).

## Cross-visibility: provisional, permissive default

Not restricted — a private Exercise may specialize a shared one and vice versa. This is a deliberate non-decision, not an oversight: the central open question (`Forma.Claude` open-questions #6) isn't answered, and inventing a restriction now would be exactly the premature complexity `CLAUDE.md` warns against. Practical consequence: the API only exposes `ParentId` (an ID), never the parent's full data inline, so this never leaks another user's private Exercise content — at worst, a client resolving the parent by ID gets a 403/404-shaped "not visible to you" result once GetById exists (not yet built). Flagged in `Forma.Claude`'s central open-questions as informed by this implementation, not resolved by it.

## Persistence

- `ParentId` column, nullable, `uniqueidentifier`, converted the same way as `Id`/`OwnerId`.
- **Self-referencing FK** (`Exercise.ParentId` → `Exercise.Id`), `OnDelete(DeleteBehavior.Restrict)`: since `Exercise` has no `Delete` yet (FT-003), this is forward-looking — when Delete is built, deleting an Exercise that still has children must fail loudly (FK violation surfaced as a domain error) rather than silently cascading or orphaning children. FT-003 must handle/translate that constraint explicitly rather than relying on the DB error message reaching a caller.
- No new unique index — multiple children may share the same parent by design.

## What's explicitly not built here

- Hierarchy-aware read queries (ancestors/descendants listing) — not requested.
- Any restriction on cross-visibility parenting — see above.

## Output

Handed to Backend Developer.
