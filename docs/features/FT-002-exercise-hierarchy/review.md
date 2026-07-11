# FT-002 — Exercise Hierarchy — Review

## Stages

Developer peer review + Service Architect conformance review, done together.

## Conformance against `design.md`

- `Exercise.ParentId` (`ExerciseId?`) added, no new aggregate, child holds a reference only — matches "Aggregate boundary"/"Data model".
- `SetParent`/`ClearParent` added as dedicated methods rather than folding into `Update` — matches the tri-state reasoning in "Mutation API".
- Existence, self-parenting, and cycle checks all implemented via a new `IExerciseHierarchyChecker` contract, mirroring the existing `IExerciseUniquenessChecker` injection pattern — matches "Validation".
- No cross-visibility restriction added — matches the deliberate permissive default.
- `ParentId` mapped with a self-referencing FK, `DeleteBehavior.Restrict` — matches "Persistence".
- `CreateExerciseCommand.ParentId` accepted at creation; `SetExerciseParentCommand`/`ClearExerciseParentCommand` added for later changes — matches requirement #4.

## Verified

- `dotnet build` clean across `Forma.PublicApi`, `Forma.ArchitectureTests`, `Forma.IntegrationTests`.
- `Forma.ArchitectureTests` suite passes.
- EF Core migration generated via `dotnet ef migrations add` (`ExerciseHierarchy`) — `Up` adds `ParentId` + FK with `Restrict`, `Down` reverses cleanly.
- Grepped for stale references to changed constructors/signatures — none found.

## Findings

1. **`WouldCreateCycleAsync` walks the parent chain with one query per hop** (`ExerciseWriteOnlyRepository`), capped at 1000 iterations as a defensive guard against a pre-existing corrupt cycle in the data. Acceptable for a curated library per `design.md`'s stated assumption ("shallow trees") — flagging so a future pass doesn't mistake the cap for an arbitrary magic number: it's a safety bound, not a real depth limit.
2. ~~**Pre-existing pattern gap**~~ — **Resolved in a later pass** (not part of FT-002/FT-003 themselves): `CreateExerciseResourceCommandHandler` originally loaded the aggregate untracked and never persisted the new `ExerciseResource`. An interim `repository.Update(exercise)` fix made it worse (`DbUpdateConcurrencyException` — EF's graph-walk classifies a client-generated-key new entity as Modified, not Added). Fixed by giving `ExerciseResource` its own `IExerciseResourceWriteOnlyRepository<ExerciseResource, ExerciseResourceId>` and calling its unconditional `Add()`. See `../../../../Forma.Claude/docs/services/exercise-service/open-questions.md` item 12.

## Not verified (environment gap, carried from FT-001)

- Integration tests still can't run here (no Docker). No existing `ExercisesControllerTests` exercise hierarchy endpoints yet — this feature has no integration-test coverage at all, new or old. Flagging for the user to add coverage and run locally before merging.

## Output

Ready for Central Architect gate.
