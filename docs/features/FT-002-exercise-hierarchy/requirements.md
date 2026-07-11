# FT-002 — Exercise Hierarchy — Service Analyst Requirements

## Source

Already decided centrally: `domain-slice.md` → Exercise, "Hierarchy"; `Forma.Claude/docs/product/domain-model.md` → Exercise, "Hierarchy (decided)". No escalation needed.

## Functional requirements

1. An `Exercise` may declare a **parent** `Exercise`, forming a generalization/specialization relationship (e.g. "Bench Press" parent, "Barbell Bench Press"/"Dumbbell Bench Press" children). Variants are modeled this way, never as one Exercise with an equipment parameter.
2. The central decision doesn't state a depth limit or restrict a parent from itself having a parent. Treating this as an open generalization tree (arbitrary depth) is simpler than inventing a one-level-only restriction nobody asked for — but the tree must stay acyclic: an Exercise can never be its own ancestor (directly or transitively). Cycle prevention is a correctness requirement, not optional.
3. A parent must reference an **existing** Exercise.
4. Parent can be set at creation, and changed or cleared later (an Exercise's classification is expected to be reorganized over time as the library grows — same rationale as `Update` already allowing renames).

## Explicitly out of scope / flagged, not decided here

- **Cross-visibility interaction** (can a private Exercise specialize a shared one, or vice versa?) is still an open *central* question (`Forma.Claude/docs/architecture/adr` open items via `open-questions.md` #6) — this feature does not restrict it. Default: permitted, any combination. This is a provisional Architect call (see `design.md`), not a resolution of the central question — revisit if/when it's answered.
- **Deleting an Exercise that has children** — Exercise has no `Delete` yet (that's FT-003). This feature only needs to decide what a future delete *would* do to preserve hierarchy integrity — see `design.md`'s FK behavior.
- Bulk re-parenting, moving an entire subtree, or hierarchy-aware queries (e.g. "get all descendants") are not requested and not built.

## Output

Handed to Service Architect for design.
