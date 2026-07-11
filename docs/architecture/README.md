# docs/architecture/

## Purpose

This service's internal architecture: how `docs/product/domain-slice.md`'s concepts map to code (aggregates, persistence, API surface), and decisions local to `exercise-service`.

## What belongs here

- Internal architecture notes (aggregate boundaries within this service, persistence model, API contract shape).
- `adr/` — decisions scoped to this service only.

## What does NOT belong here

- Decisions that affect another service, cross-service APIs/events, or system-wide architecture → promote to `Forma.Claude`'s `docs/architecture/adr/` instead (same Context Promotion Rule `Forma.Claude` uses). The Central Architect gate (see `../agents/process.md`) is what catches this before merge.

## Current state

Empty. No feature has gone through the pipeline yet, so no internal architecture or local ADRs exist. The inherited code under `../../src/` (from `Forma.App`) has not yet been documented here.
