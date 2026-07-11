# FT-003 — Exercise Update & Delete — Service Analyst Requirements

## Source

Implied by CRUD completeness of an already-decided concept (Exercise), not a new domain concept — no escalation needed. Confirmed via `../../architecture/codebase-baseline.md`: the domain method `Exercise.Update(...)` already exists and is functionally complete; only the Application/API wiring is missing (the existing `UpdateExerciseCommand`/controller route are commented-out template stubs referencing a nonexistent `Customer` concept, not usable as-is). `Exercise.Delete()` doesn't exist at all yet.

## Functional requirements

1. **Update**: allow changing an existing Exercise's `Name`, `Description`, and `MuscleGroups` — exactly the fields `Exercise.Update` already supports. Renaming re-checks uniqueness within the Exercise's own ownership scope (already implemented, per FT-001).
2. **Update does not touch** `OwnerId` (fixed at creation, FT-001) or `ParentId` (has its own dedicated `SetParent`/`ClearParent` operations, FT-002) — keeps this feature's scope to what the domain method already models, not a re-opening of prior decisions.
3. **Delete**: allow removing an Exercise entirely (hard delete — no soft-delete/archival concept exists anywhere in the domain model).
4. **Cannot delete an Exercise that has children** in the hierarchy (FT-002's `ParentId` self-reference, DB-enforced via `DeleteBehavior.Restrict`). This must surface as a clear domain-level error to the caller, not an unhandled FK-violation exception.
5. Deleting an Exercise also removes its attached `ExerciseResource`s (already cascade-configured, FT-002/pre-existing) — this is correct: they're owned child entities with no existence outside their parent.

## Explicitly missing / flagged, not decided here

- **No authorization on who may Update/Delete an Exercise** — same gap already tracked centrally as the "content curator" open question (`Forma.Claude` open-questions #1). This feature doesn't invent an authz model any more than FT-001's Create did; anyone may Update/Delete any Exercise they can address by ID, for now.
- **Cross-service reference integrity**: `training-planning-service`'s `Workout` is expected to reference `Exercise` by identity only (`Forma.Claude/docs/product/domain-model.md`). Deleting an Exercise a Workout references would orphan that reference — this service has no way to detect or prevent that today (no inter-service integration pattern decided yet, `Forma.Claude/docs/architecture/integration-patterns.md` is still empty, and `training-planning-service` doesn't exist yet either). **Flagging as a real forward risk, not solving it here** — solving it would mean inventing a cross-service check against a service that doesn't exist, which is exactly the premature complexity `CLAUDE.md` warns against.

## Output

Handed to Service Architect for design.
