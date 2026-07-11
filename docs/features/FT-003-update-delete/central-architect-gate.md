# FT-003 — Exercise Update & Delete — Central Architect Gate

## Cross-service impact assessment

- Update/Delete are entirely intra-aggregate — no new cross-service surface.
- **Real forward risk, explicitly not solved**: `training-planning-service`'s `Workout` is expected to reference `Exercise` by identity only (`Forma.Claude/docs/product/domain-model.md`). This feature makes `Exercise` deletable, and nothing anywhere today can prevent or even detect deleting an `Exercise` that a `Workout` references — `training-planning-service` doesn't exist yet, and the inter-service integration pattern (`Forma.Claude/docs/architecture/integration-patterns.md`) is still empty. This isn't a defect introduced by this feature (Exercise was never deletable before), but it's the first point the risk becomes concrete rather than theoretical.

**Verdict: no promotion of a new decision** — there's no decision to make yet, only a dependency to flag for whenever `training-planning-service` and the integration-pattern question get addressed. Recorded centrally below so it isn't lost.

## Central knowledge updated

- `Forma.Claude/docs/services/exercise-service/domain.md` — Exercise is now fully CRUD (Create/Update/Delete all wired, plus ownership/visibility and hierarchy).
- `Forma.Claude/docs/services/exercise-service/open-questions.md` — new item added: Exercise deletion has no cross-service safeguard against orphaning `Workout` references, pending `training-planning-service` and the integration-pattern decision.
- `Forma.Claude/docs/branches/analysis/pending-items.md` — the inter-service integration pattern decision (already flagged as "next steps for this branch") now has a second concrete driver beyond the existing Workout-Version-at-session-start case: Exercise deletion.

## Output

Cleared to merge (pending: user adds/runs integration test coverage locally — Docker isn't available in this environment).
