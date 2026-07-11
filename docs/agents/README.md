# docs/agents/

## Purpose

Defines the roles that carry a feature through `exercise-service`, from request to merge. Unlike `Forma.Claude`'s Analyst→Architect→Challenger loop (whole-system domain/architecture discovery, no code), this repo is where code actually gets written — so the pipeline here is a development pipeline, not an analysis pipeline.

## The pipeline

See `process.md` for the full description. Summary:

```
Service Analyst
    ↓
Service Architect (design)
    ↓
Backend Developer (.NET) — implement
    ↓
Developer peer review
    ↓
Service Architect (conformance review)
    ↓
Central Architect gate (Forma.Claude)
    ↓
Merge
```

## Role files

- `analyst-context.md` — Service Analyst.
- `architect-context.md` — Service Architect. **Not the same role** as `Forma.Claude`'s central Architect — this one is scoped to `exercise-service` only and reports up to the central one at the final gate.
- `backend-developer-context.md` — .NET Backend Developer (implementation + peer review).

The final gate (Central Architect) is defined in `Forma.Claude`'s `docs/agents/architect-context.md`, not duplicated here — this repo only describes the hand-off into it.

## Current state

Pipeline and roles defined; no feature has been through it yet.
