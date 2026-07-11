# FT-001 — Exercise Ownership & Visibility — Central Architect Gate

## Stage

Central Architect gate (`../../agents/process.md`, stage 6 — `Forma.Claude`'s system-wide Architect, cross-service impact only).

## Cross-service impact assessment

- `Exercise.OwnerId` references `identity-service`'s `User` **by ID only** — no call to `identity-service`, no shared datastore, no new API contract between services. This is exactly the shape `Forma.Claude/docs/services/exercise-service/architecture.md` already anticipated for ownership before this feature existed.
- The caller-supplied `OwnerId`/`RequestingUserId` stand-in (no real auth) doesn't create a contract another service depends on — it's an internal-to-`exercise-service` API shape that will change when `identity-service` exists, not something other services integrate against today.
- No new domain concept was invented — ownership/visibility was already decided centrally (`Forma.Claude/docs/product/domain-model.md`, ADR-001).

**Verdict: no promotion to `Forma.Claude`'s central ADRs needed.** Everything here stays local to `exercise-service`, recorded in `../../architecture/adr/ADR-001-strongly-typed-exercise-id.md` (the ID-fix decision) and this feature's own `requirements.md`/`design.md`.

## Central knowledge updated

Per Context Promotion Rules, the *fact that this is now built* (not a new decision) is reflected back into `Forma.Claude`'s exercise-service knowledge so it stays accurate for future central-loop passes:

- `Forma.Claude/docs/services/exercise-service/domain.md` — "What this service still needs to build" no longer lists ownership/visibility.
- `Forma.Claude/docs/services/exercise-service/open-questions.md` — item 8 (strongly-typed ID migration) marked resolved.

## Output

Cleared to merge (pending the user running integration tests locally per `review.md`'s verification gap).
