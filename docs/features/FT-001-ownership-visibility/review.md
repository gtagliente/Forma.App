# FT-001 — Exercise Ownership & Visibility — Review

## Stages

Developer peer review + Service Architect conformance review (`../../agents/process.md`, stages 4–5), done together in this pass.

## Conformance against `design.md`

- `OwnerId` (`Guid?`) added to `Exercise` as a scalar attribute, no new aggregate — matches "Aggregate boundary" decision.
- Nullability alone carries visibility (no separate `Visibility` field) — matches "Data model decision".
- `CreateExerciseCommand.OwnerId` / `GetAllExerciseQuery.RequestingUserId` are both caller-supplied, both optional — matches "Auth stand-in".
- Uniqueness checker signature scoped to `(name, ownerId)`, enforced in both `Exercise.Create`/`Update` and two filtered unique DB indexes (`UQ_Exercise_Name_Shared`, `UQ_Exercise_Name_PerOwner`) — matches "Name-uniqueness scoping".
- Read side (`ExerciseQueryModel`, event→model mapping, `GetVisibleToAsync` Mongo filter, per-user cache key) all updated — matches "Read side".

## Verified

- `dotnet build` clean across `Forma.PublicApi`, `Forma.ArchitectureTests`, `Forma.IntegrationTests` (0 errors).
- `Forma.ArchitectureTests` suite passes (aggregate-boundary guard on `ExerciseResource.Create` still holds — untouched by this feature).
- EF Core migration generated via `dotnet ef migrations add` (not hand-authored) — `Up`/`Down` reviewed, matches the intended column + filtered-index shape.
- Grepped for stale call sites against every changed signature (`GetAllAsync()`, single-arg `IsUniqueAsync`, parameterless `GetAllExerciseQuery`) — none found.

## Not verified (environment gap, not a code defect)

- **Integration tests were not run** — `Forma.IntegrationTests` uses `Testcontainers.MsSql`, and Docker is not available in this environment. `ExercisesControllerTests` (existing `Should_ReturnsHttpStatus400_When_Post_ExerciseNameIsNotUnique`, seeding a shared Exercise via raw SQL with no `OwnerId` column in the insert list) should still pass as-is — the new column is nullable and defaults to `NULL`, i.e. "shared", which is exactly the scope that test's uniqueness check exercises — but this is reasoned from reading the test, not an actual run. **Flagging for the user to run `dotnet test tests/Forma.IntegrationTests` locally (Docker available) before merging.**

## Findings

None blocking. One pre-existing gap noted but explicitly out of scope for this feature (per `requirements.md`): no access control on who may create a shared-library Exercise — anyone can pass `OwnerId: null`. Tracked centrally as the "content curator" open question, not introduced or worsened by this feature.

## Output

Ready for Central Architect gate.
