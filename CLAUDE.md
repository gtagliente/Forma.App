# Forma.Exercise — Service Intelligence Context

## What this repository is

This repository implements **`exercise-service`**, one of four independently deployable services that make up Forma, decided in `Forma.Claude`'s [ADR-005](../Forma.Claude/docs/architecture/adr/ADR-005-microservices-architecture.md). It owns the Exercise Library: Exercise definitions (shared/curated library plus private per-user exercises), the parent/child generalization-specialization hierarchy, attached Media Resources, and AI Enrichment as an internal capability — see `docs/product/domain-slice.md`.

It has its own independent datastore (no shared database with other services) and its own git history — it is not a module of a larger monolith.

## Source of truth

`Forma.Claude` (sibling repo, `../Forma.Claude`) is the **orchestrator**: system-wide product vision, the full cross-service domain model, and every cross-cutting ADR live there. This repository's `docs/` holds only what's specific to `exercise-service`:

- `docs/product/domain-slice.md` is a **derived, non-authoritative** copy of the Exercise-relevant parts of `Forma.Claude`'s `docs/product/domain-model.md`. If the two ever disagree, `Forma.Claude` is correct — update there first, then resync here.
- Any decision made here that turns out to affect another service must be promoted to `Forma.Claude`'s `docs/architecture/adr/`, per the Context Promotion Rules already established there — not just left local.

## How work happens here

Unlike `Forma.Claude` (analysis-only, no code), this repository is where `exercise-service` actually gets built. Work follows a five-stage pipeline instead of the system-wide Analyst→Architect→Challenger loop:

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
Central Architect gate (Forma.Claude) — cross-service impact
    ↓
Merge
```

Each role's responsibilities and boundaries are defined as live Claude Code subagents in `.claude/agents/` (`service-analyst`, `service-architect`, `backend-developer`) — invoke them directly rather than reading a separate context doc.

## Current status

`docs/` just bootstrapped (product domain slice, architecture placeholder, engineering placeholder, agents pipeline, features placeholder, branch context for `feature/claude_integration`). No feature has gone through the pipeline yet. Existing code (`src/`, `tests/`) predates this reorganization — it came from `Forma.App`, the earlier single-codebase effort this service is being split out of; nothing there has been reviewed against the pipeline yet.

## Architecure

The architecture in high level is described in the ./docs/architecture/codebase-baseline.md read this for taking context

## Engineering

Here you can find the contract exposed via rest api openapi.json