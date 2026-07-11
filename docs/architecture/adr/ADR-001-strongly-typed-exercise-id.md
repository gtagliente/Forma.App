# ADR-001: Resolve dual-identity bug on Exercise by adopting strongly-typed `BaseEntity<TKey>`

## Status

Accepted.

## Context

While starting the first feature pipeline pass (Ownership/Visibility, see `../../features/`), reconnaissance of the codebase (`../codebase-baseline.md`) surfaced a live correctness bug, not just cosmetic debt: `Exercise` carried two identity concepts — `BaseEntity.Id` (`Guid`) and a separate `ExerciseId` "convenience" property. `ExerciseConfiguration` mapped the actual EF primary key to `ExerciseId`, but nothing in `Exercise.Create` ever assigned it, so every new `Exercise` was inserted with PK `Guid.Empty`. A second `Exercise` created against a real relational database would fail on a primary-key violation.

A fix already existed, unmerged, on local branch `fix/ExerciseResourceMigration_stronglytypedIds` (tip `3bc7d4a`), which makes `BaseEntity` generic over `TKey` (`BaseEntity<TKey>`) and removes the duplicate `ExerciseId` property entirely — `Exercise` becomes `BaseEntity<ExerciseId>`, with exactly one identity. `../../services/exercise-service/architecture.md` (the central-loop output, `Forma.Claude`) had already flagged this branch and recommended merging it before building the Exercise-hierarchy self-reference; this pass found the bug is broader than that — it blocks reliable `Exercise` creation at all, so it was treated as a prerequisite for Feature 1 as well, not deferred to the hierarchy feature.

## Decision

Merged `fix/ExerciseResourceMigration_stronglytypedIds` into `feature/claude_integration` (fast-forward, `00f0f3c` → `3bc7d4a`). `Exercise` (and `ExerciseResource`) now use a single strongly-typed ID (`BaseEntity<ExerciseId>` / `BaseEntity<ExerciseResourceId>`) properly assigned via `ExerciseId.New()` at creation.

While verifying the build post-merge, also found and fixed an unrelated, pre-existing package-version pin conflict: `Directory.Packages.props` pinned `Microsoft.Extensions.DependencyInjection` to a fixed patch version (`9.0.11`, bumped by the merge to `9.0.14`) lower than what `MediatR 13.0.*` transitively requires via `Microsoft.Extensions.Logging` (`>= 9.0.17`), which made every build fail restore (`NU1109`). Changed the pin to `9.*`, matching the floating pattern already used for its sibling `Microsoft.Extensions.DependencyInjection.Abstractions` package. Confirmed `Forma.PublicApi`, `Forma.ArchitectureTests`, and `Forma.IntegrationTests` all build clean afterward, and the architecture test suite (aggregate-boundary guard) still passes.

## Alternatives considered

- **Leave the bug and work around it per-feature** — rejected: every feature in the current pipeline (Ownership, Hierarchy, Update/Delete) creates or persists `Exercise` instances; building on top of a PK collision would just move the failure into new code instead of fixing it at the source.
- **Reimplement the fix from scratch instead of merging** — rejected: a correct, already-tested fix existed on a branch in this same repo; merging it was strictly less risky than re-deriving the same change.

## Consequences

- `open-questions.md` item 8 ("merge before hierarchy, or proceed in parallel?") is resolved: merged, and treated as broader-scoped than originally framed.
- Any code still referencing `Exercise.ExerciseId` (the old convenience property) no longer compiles — none was found outside the merged diff's own files.
- Domain events (`ExerciseCreatedEvent`, etc.) now carry the real aggregate ID, so `open-questions.md`'s read-model concerns are not compounded by a corrupted `Id`.
- No cross-service impact — this is entirely internal to `exercise-service`'s own persistence model.

## References

- `../codebase-baseline.md` — full description of the bug as found.
- `../../../../Forma.Claude/docs/services/exercise-service/architecture.md` — original recommendation (central-loop Architect pass) to merge this branch before the hierarchy feature.
- `../../../../Forma.Claude/docs/services/exercise-service/open-questions.md` — item 8.
