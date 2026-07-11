# FT-003 — Exercise Update & Delete — Review

## Stages

Developer peer review + Service Architect conformance review, done together.

## Conformance against `design.md`

- `UpdateExerciseCommand`/handler written fresh (not adapted from the dead `Update`/`DeleteCustomerCommand*` stubs, which remain untouched, still commented, still tracked as open item 9). Handler explicitly calls `repository.Update(exercise)` after mutating — matches the "must not repeat the untracked-entity bug" requirement.
- `Exercise.Delete(builder)` added: checks `HasChildrenAsync` (new method on the existing `IExerciseHierarchyChecker` contract, reusing it rather than adding a fourth checker interface) before raising `ExerciseDeletedEvent` — matches design.
- `DeleteExerciseCommandHandler` calls `Delete` then `repository.Remove(exercise)`, in that order, so the domain event raised by `Delete` is present on the entity by the time `Remove` attaches it to the change tracker — matches the reasoning that `Remove` (unlike `Update`) doesn't need a separate explicit-attach step.
- Routes: `PUT /api/exercises/Update` (body-carries-everything, matching `Create`), `DELETE /api/exercises/{id:guid}` (id-in-URL, matching the original commented-out stub's shape) — matches design's stated convention split.

## Verified

- `dotnet build` clean across `Forma.PublicApi`, `Forma.ArchitectureTests`, `Forma.IntegrationTests`.
- `Forma.ArchitectureTests` suite passes.
- No schema change — `Update`/`Delete` don't touch the `Exercise` table shape, so no new EF migration needed (confirmed by inspecting `ExerciseConfiguration` — nothing added).
- Grepped for stale references to `UpdateCustomerCommand`/`DeleteCustomerCommand` — only the still-dead, still-commented original stub files themselves; nothing live references them.

## Findings

None blocking.

## Not verified (environment gap, carried from FT-001/FT-002)

- No Docker here to run `Forma.IntegrationTests`. No integration-test coverage exists yet for Update, Delete, or the children-block-delete path — flagging for the user to add coverage and run locally before merging, same as the prior two features.

## Output

Ready for Central Architect gate.
