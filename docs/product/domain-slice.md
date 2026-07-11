# exercise-service — Domain Slice

_Derived from `Forma.Claude`'s `docs/product/domain-model.md`. **Non-authoritative** — see `README.md`. Copied as of this service's `docs/` bootstrap; if it looks stale, check the source before trusting it._

## Exercise

A reusable definition of a training movement. Not tied to any one workout.

- Attributes: name, description, execution instructions, required equipment, media resources (see Media Resource, below), tags, difficulty.
- Enrichment (see below) may add: muscle groups, movement pattern, difficulty classification, progressions, regressions, alternative exercises, common mistakes, safety recommendations.
- **Ownership**: both a centralized/shared Exercise library and individually user-defined private Exercises exist. A private Exercise is visible only to the user who defined it. A mechanism for a user to promote a private Exercise into the shared library is deliberately deferred — not part of this service's current scope.
- **Hierarchy**: an Exercise may declare a **parent** Exercise, forming a generalization/specialization relationship — e.g. "Bench Press" as a general parent, with "Barbell Bench Press" and "Dumbbell Bench Press" as specializations (children). Variants are modeled as related Exercises via this hierarchy, not as one Exercise with an equipment parameter. Cross-visibility interaction (can a private Exercise specialize a shared one, or vice versa) is not yet specified.
- Still open (system-wide, not this service's call alone): Equipment as its own referenceable concept vs. a free attribute; Muscle Group/Movement Pattern as controlled vocabulary vs. free text; Difficulty as global/objective vs. subjective per user; Tags as free-form vs. controlled vocabulary.

## Media Resource

A photo or video, either uploaded by the user or an external link (e.g. to a third-party instructional video).

- Attachable to an **Exercise** (instructional/learning material) — this service's concern — and also to a Workout Session (owned by `training-execution-service`, out of this service's scope).
- Capture/storage mechanics (upload limits, hosting, thumbnailing) are this service's implementation detail to decide, not specified centrally.

## Enrichment (AI)

Enrichment stays separated from the core domain: held as a distinct, clearly-labeled layer, never auto-merged into what defines an Exercise.

- **Requirement**: needs a **promotion mechanism** — a way for a user to review a specific AI-sourced suggestion and explicitly accept it, at which point it becomes part of the Exercise's standard/definitive data. Until promoted, enrichment content stays visibly separate. Product/UX requirement, not yet designed at the implementation level.
- The external/AI intelligence capability itself (third-party API vs. in-house service vs. library) is undecided — this service depends on it one-directionally (Exercise Library must stay coherent with zero enrichment data present) but does not own it.

## What this service does NOT own

Workout, Routine, Workout Session, Progress Tracking, Set, User/Identity — all owned by other services (`training-planning-service`, `training-execution-service`, `identity-service`). This service references a `User` by ID for ownership/visibility scoping (Exercise ownership, above) but does not own the User concept itself.

## Relationships relevant to this service

```
Exercise  ──(parent of, generalization/specialization)──────────────▶  Exercise
Exercise  ──(illustrated by)──────────────────────────────────────────▶  Media Resource
Exercise  ──(referenced by identity only, no duplication)────────────▶  Workout   [training-planning-service]
```
