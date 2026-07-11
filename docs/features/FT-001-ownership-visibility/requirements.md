# FT-001 — Exercise Ownership & Visibility

## Stage

Service Analyst (`../../agents/analyst-context.md`).

## Source

Already decided centrally, not invented here: `domain-slice.md` → Exercise, "Ownership"; `Forma.Claude/docs/product/domain-model.md` → Exercise, "Ownership (decided)"; `Forma.Claude/docs/architecture/adr/ADR-001-user-model-iteration-1.md` (single normal-user model, no delegation). No escalation to the central Analyst needed — this is a new *capability* on an already-defined concept, not a new domain concept.

## Functional requirements

1. An `Exercise` is either:
   - part of the **shared library** — visible to every user, or
   - **privately owned** by exactly one user — visible only to that user.
2. `Exercise` creation must let the caller specify which of the two applies, and for a private Exercise, which user owns it.
3. Listing Exercises must return only: every shared-library Exercise, plus the requesting user's own private Exercises. Never another user's private Exercises.
4. Name uniqueness is **scoped to visibility**, not global:
   - Two Exercise names must be unique within the shared library.
   - A given user's private Exercise names must be unique among *that user's own* private Exercises.
   - A private Exercise's name **may** coincide with a shared-library Exercise's name, or with another user's private Exercise name — those are different, non-comparable scopes from the owning user's point of view.
5. Ownership is fixed at creation. Changing an Exercise's owner or promoting a private Exercise into the shared library is **out of scope** for this feature — already deferred centrally (`domain-slice.md`: "a mechanism for a user to promote a private Exercise into the shared library is deliberately deferred").
6. Cross-visibility interaction with the Exercise hierarchy (can a private Exercise specialize a shared one, or vice versa?) is **out of scope** here — belongs to FT-002 (Hierarchy), and is itself still an open central question (`open-questions.md` #6 in `Forma.Claude`).

## Explicitly missing / not this feature's job

- **No real user authentication exists yet** (`identity-service` is a placeholder, not implemented — see `Forma.Claude/docs/services/identity-service/README.md`). This feature cannot derive "the requesting user" from a validated identity. Per ADR-001's single-normal-user model, the Service Architect must decide a pragmatic stand-in (e.g., an explicit caller-supplied user identifier) rather than inventing an auth system — that would be solving a problem nobody asked this feature to solve yet.
- **Content curator / library maintainer** for the shared library (`Forma.Claude` open item, Users #3) is unresolved — this feature does not add any mechanism for *who* is allowed to create a shared-library (non-owned) Exercise. Default assumption, to be confirmed by the Architect: for iteration 1, any caller may create either kind; access control on shared-library writes is a future concern once a curator role is decided.

## Output

Handed to Service Architect for design (`design.md` in this folder).
