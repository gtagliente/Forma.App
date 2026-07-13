# FT-002 — Exercise Hierarchy

## Status

Built. Cleared to merge (pending: user adds/runs integration test coverage locally — Docker isn't available in this environment; see Review below).

## Requirements (Service Analyst)

### Source

Already decided centrally: `domain-slice.md` → Exercise, "Hierarchy"; `Forma.Claude/docs/product/domain-model.md` → Exercise, "Hierarchy (decided)". No escalation needed.

### Functional requirements

1. An `Exercise` may declare a **parent** `Exercise`, forming a generalization/specialization relationship (e.g. "Bench Press" parent, "Barbell Bench Press"/"Dumbbell Bench Press" children). Variants are modeled this way, never as one Exercise with an equipment parameter.
2. The central decision doesn't state a depth limit or restrict a parent from itself having a parent. Treating this as an open generalization tree (arbitrary depth) is simpler than inventing a one-level-only restriction nobody asked for — but the tree must stay acyclic: an Exercise can never be its own ancestor (directly or transitively). Cycle prevention is a correctness requirement, not optional.
3. A parent must reference an **existing** Exercise.
4. Parent can be set at creation, and changed or cleared later (an Exercise's classification is expected to be reorganized over time as the library grows — same rationale as `Update` already allowing renames).

### Explicitly out of scope / flagged, not decided here

- **Cross-visibility interaction** (can a private Exercise specialize a shared one, or vice versa?) is still an open *central* question (`Forma.Claude/docs/architecture/adr` open items via `open-questions.md` #6) — this feature does not restrict it. Default: permitted, any combination. This is a provisional Architect call (see Design below), not a resolution of the central question — revisit if/when it's answered.
- **Deleting an Exercise that has children** — Exercise has no `Delete` yet (that's FT-003). This feature only needs to decide what a future delete *would* do to preserve hierarchy integrity — see Design's FK behavior below.
- Bulk re-parenting, moving an entire subtree, or hierarchy-aware queries (e.g. "get all descendants") are not requested and not built.

## Design (Service Architect)

### Aggregate boundary

Per `Forma.Claude/docs/services/exercise-service/architecture.md` (already decided centrally): parent and child are **separate aggregate instances**, each its own transaction boundary, even though both are the same aggregate type. The child holds a reference to the parent's `ExerciseId` only — never a loaded parent object graph. No new aggregate, no change to the one-aggregate-per-`Exercise` shape.

### Data model

`Exercise.ParentId` (`ExerciseId?`, nullable — no parent by default).

### Mutation API

Dedicated methods, matching the existing `AddResource`/`RemoveResource` precedent rather than overloading `Update` (a nullable `ExerciseId?` parameter on `Update` couldn't distinguish "leave parent unchanged" from "clear the parent" — both would be `null`):

- `SetParent(IExerciseBuilder builder, ExerciseId parentId)` — validates and assigns.
- `ClearParent()` — removes the parent link (no validation needed to remove a reference).

### Validation (on `SetParent`)

Three checks, all requiring infrastructure the domain layer can't reach directly — follows the existing `uniquenessChecker` contract-injection pattern (`IExerciseBuilder.Contracts`):

1. **Existence**: `parentId` must reference a real `Exercise`. New contract `IExerciseHierarchyChecker.ExistsAsync(ExerciseId id)`.
2. **No self-parenting**: `parentId != this.Id`, checked directly in the domain method — no infra needed.
3. **No cycles**: the proposed parent's ancestor chain must not already contain this Exercise. New contract method `IExerciseHierarchyChecker.WouldCreateCycleAsync(ExerciseId childId, ExerciseId proposedParentId)` — implemented in `Forma.Infrastructure` by walking `ParentId` upward from `proposedParentId` until null or `childId` is found. Chains are expected to be shallow (a curated exercise library, not user-generated arbitrary depth), so a straightforward loop is adequate — no need for a recursive CTE for iteration 1.

Both checks live on a new `IExerciseHierarchyChecker` contract (mirrors `IExerciseUniquenessChecker`), added to `IExerciseBuilder.Contracts` alongside the existing two checkers, and implemented by `ExerciseWriteOnlyRepository` (same class that already implements the other two checker interfaces).

### Cross-visibility: provisional, permissive default

Not restricted — a private Exercise may specialize a shared one and vice versa. This is a deliberate non-decision, not an oversight: the central open question (`Forma.Claude` open-questions #6) isn't answered, and inventing a restriction now would be exactly the premature complexity `CLAUDE.md` warns against. Practical consequence: the API only exposes `ParentId` (an ID), never the parent's full data inline, so this never leaks another user's private Exercise content — at worst, a client resolving the parent by ID gets a 403/404-shaped "not visible to you" result once GetById exists (not yet built). Flagged in `Forma.Claude`'s central open-questions as informed by this implementation, not resolved by it.

### Persistence

- `ParentId` column, nullable, `uniqueidentifier`, converted the same way as `Id`/`OwnerId`.
- **Self-referencing FK** (`Exercise.ParentId` → `Exercise.Id`), `OnDelete(DeleteBehavior.Restrict)`: since `Exercise` has no `Delete` yet (FT-003), this is forward-looking — when Delete is built, deleting an Exercise that still has children must fail loudly (FK violation surfaced as a domain error) rather than silently cascading or orphaning children. FT-003 must handle/translate that constraint explicitly rather than relying on the DB error message reaching a caller.
- No new unique index — multiple children may share the same parent by design.

### What's explicitly not built here

- Hierarchy-aware read queries (ancestors/descendants listing) — not requested.
- Any restriction on cross-visibility parenting — see above.

## Review (Developer peer review + Service Architect conformance review)

### Conformance against Design

- `Exercise.ParentId` (`ExerciseId?`) added, no new aggregate, child holds a reference only — matches "Aggregate boundary"/"Data model".
- `SetParent`/`ClearParent` added as dedicated methods rather than folding into `Update` — matches the tri-state reasoning in "Mutation API".
- Existence, self-parenting, and cycle checks all implemented via a new `IExerciseHierarchyChecker` contract, mirroring the existing `IExerciseUniquenessChecker` injection pattern — matches "Validation".
- No cross-visibility restriction added — matches the deliberate permissive default.
- `ParentId` mapped with a self-referencing FK, `DeleteBehavior.Restrict` — matches "Persistence".
- `CreateExerciseCommand.ParentId` accepted at creation; `SetExerciseParentCommand`/`ClearExerciseParentCommand` added for later changes — matches requirement #4.

### Verified

- `dotnet build` clean across `Forma.PublicApi`, `Forma.ArchitectureTests`, `Forma.IntegrationTests`.
- `Forma.ArchitectureTests` suite passes.
- EF Core migration generated via `dotnet ef migrations add` (`ExerciseHierarchy`) — `Up` adds `ParentId` + FK with `Restrict`, `Down` reverses cleanly.
- Grepped for stale references to changed constructors/signatures — none found.

### Findings

1. **`WouldCreateCycleAsync` walks the parent chain with one query per hop** (`ExerciseWriteOnlyRepository`), capped at 1000 iterations as a defensive guard against a pre-existing corrupt cycle in the data. Acceptable for a curated library per Design's stated assumption ("shallow trees") — flagging so a future pass doesn't mistake the cap for an arbitrary magic number: it's a safety bound, not a real depth limit.
2. ~~**Pre-existing pattern gap**~~ — **Resolved in a later pass** (not part of FT-002/FT-003 themselves): `CreateExerciseResourceCommandHandler` originally loaded the aggregate untracked and never persisted the new `ExerciseResource`. An interim `repository.Update(exercise)` fix made it worse (`DbUpdateConcurrencyException` — EF's graph-walk classifies a client-generated-key new entity as Modified, not Added). Fixed by giving `ExerciseResource` its own `IExerciseResourceWriteOnlyRepository<ExerciseResource, ExerciseResourceId>` and calling its unconditional `Add()`. See `../../../Forma.Claude/docs/services/exercise-service/open-questions.md` item 12.

### Not verified (environment gap, carried from FT-001)

- Integration tests still can't run here (no Docker). No existing `ExercisesControllerTests` exercise hierarchy endpoints yet — this feature has no integration-test coverage at all, new or old. Flagging for the user to add coverage and run locally before merging.

## Central Architect Gate

*(`Forma.Claude`'s system-wide Architect — cross-service impact only, not a second local design/code-quality pass.)*

### Cross-service impact assessment

- Hierarchy is entirely intra-aggregate-type (`Exercise` → `Exercise`), no reference to any other service's data. No new API contract, no shared datastore.
- The permissive cross-visibility default (private can specialize shared, and vice versa) touches a still-open **central** question (`Forma.Claude` open-questions #6) — this feature doesn't resolve it, just doesn't block on it, per Design above.

**Verdict: no promotion of a new decision needed.** The cross-visibility default is recorded as informing (not resolving) the existing central open question — see update to `open-questions.md` below.

### Central knowledge updated

- `Forma.Claude/docs/services/exercise-service/domain.md` — hierarchy no longer listed under "still needs to build".
- `Forma.Claude/docs/services/exercise-service/open-questions.md` — item 6 (cross-visibility hierarchy interaction) annotated with the implementation's provisional default, still open centrally.
