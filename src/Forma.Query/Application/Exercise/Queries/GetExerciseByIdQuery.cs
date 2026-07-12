using System;
using Ardalis.Result;
using MediatR;
using Forma.Query.QueriesModel;

namespace Forma.Query.Application.Exercise.Queries;

/// <summary>
/// Backs both the public GetById surface and, once consumed by training-planning-service,
/// ADR-006 Rule 1's Exercise-existence check at Workout create/edit
/// (Forma.Claude/docs/architecture/adr/ADR-006-cross-service-reference-integrity.md).
/// </summary>
public class GetExerciseByIdQuery(Guid id) : IRequest<Result<ExerciseQueryModel>>
{
    public Guid Id { get; } = id;
}
