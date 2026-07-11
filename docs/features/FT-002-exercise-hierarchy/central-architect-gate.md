# FT-002 — Exercise Hierarchy — Central Architect Gate

## Cross-service impact assessment

- Hierarchy is entirely intra-aggregate-type (`Exercise` → `Exercise`), no reference to any other service's data. No new API contract, no shared datastore.
- The permissive cross-visibility default (private can specialize shared, and vice versa) touches a still-open **central** question (`Forma.Claude` open-questions #6) — this feature doesn't resolve it, just doesn't block on it, per `design.md`.

**Verdict: no promotion of a new decision needed.** The cross-visibility default is recorded as informing (not resolving) the existing central open question — see update to `open-questions.md` below.

## Central knowledge updated

- `Forma.Claude/docs/services/exercise-service/domain.md` — hierarchy no longer listed under "still needs to build".
- `Forma.Claude/docs/services/exercise-service/open-questions.md` — item 6 (cross-visibility hierarchy interaction) annotated with the implementation's provisional default, still open centrally.

## Output

Cleared to merge (pending: user adds/runs integration test coverage — none exists yet for this feature, and Docker isn't available in this environment to verify).
