# docs/architecture/

## Purpose

This service's internal architecture: how `docs/product/domain-slice.md`'s concepts map to code (aggregates, persistence, API surface), and decisions local to `exercise-service`.

## What belongs here

- Internal architecture notes (aggregate boundaries within this service, persistence model, API contract shape).
- `adr/` — decisions scoped to this service only.

## What does NOT belong here

- Decisions that affect another service, cross-service APIs/events, or system-wide architecture → promote to `Forma.Claude`'s `docs/architecture/adr/` instead (same Context Promotion Rule `Forma.Claude` uses). The Central Architect gate (see `../../.claude/agents/`) is what catches this before merge.

## Current state

`adr/ADR-001-strongly-typed-exercise-id.md` (local decision) and `codebase-baseline.md` (reconnaissance notes on the inherited `Forma.App` code) exist. Three features have gone through the pipeline (`../features/`).
