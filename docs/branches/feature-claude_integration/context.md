# Branch: feature/claude_integration

## Purpose

Bootstrap this repository's `docs/` knowledge base and `CLAUDE.md`, establishing `exercise-service`'s scoped structure (product domain slice, architecture, engineering, agents pipeline, features, branches) — the local counterpart to `Forma.Claude` becoming the system-wide orchestrator repo.

## Scope

Documentation only. No feature code was written on this branch; the pre-existing code under `../../../src/` and `../../../tests/` predates this branch and came from `Forma.App` (this service's origin before the split into independent services, [ADR-005](../../../../Forma.Claude/docs/architecture/adr/ADR-005-microservices-architecture.md)).

## Status

`docs/` and `CLAUDE.md` bootstrapped. Not yet done: reviewing the inherited `Forma.App` code against the new pipeline (`../../../.claude/agents/`) and populating `../../engineering/` accordingly.
