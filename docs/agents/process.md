# Feature Development Pipeline — exercise-service

Mirrors the structure of `Forma.Claude`'s "Multi-Agent Analysis Process" (`CLAUDE.md`), adapted for a service that's actually being built rather than analyzed.

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

## Stages

1. **Service Analyst** (`analyst-context.md`) — turns a feature request into requirements scoped to this service's domain slice. Escalates to `Forma.Claude`'s Analyst if the request implies a new/changed domain concept rather than a new capability.
2. **Service Architect — design** (`architect-context.md`) — proposes the technical approach: aggregate(s) touched, persistence/EF Core implications, API surface. Output is a design, not code.
3. **Backend Developer — implement** (`backend-developer-context.md`) — builds the feature per the approved design: API/feature code + EF Core database layer.
4. **Developer peer review** (`backend-developer-context.md`, reviewer hat) — a different developer reviews for correctness, code quality, test coverage.
5. **Service Architect — conformance review** (`architect-context.md`) — checks the implementation actually matches the approved design. Not a second code-quality pass; a check that what got built is what was designed.
6. **Central Architect gate** (`Forma.Claude`'s `docs/agents/architect-context.md`) — the system-wide Architect reviews the change for **collateral effects on other services** and whether it should trigger related work elsewhere (e.g. an API contract another service will need, a domain concept that turns out to be shared). This is the only stage that requires visibility across all four services — everything before it only needs this one service's context.
7. **Merge.**

## What gets promoted centrally, and what stays local

- A design/implementation detail that stays entirely inside `exercise-service` → recorded locally (`../architecture/adr/` if it's decision-worthy).
- Anything the Central Architect gate flags as cross-service → promoted to `Forma.Claude`'s `docs/architecture/adr/`, following the same Context Promotion Rules `Forma.Claude` already uses for its own decisions.

## Current state

Defined, not yet exercised — no feature has gone through this pipeline yet.
