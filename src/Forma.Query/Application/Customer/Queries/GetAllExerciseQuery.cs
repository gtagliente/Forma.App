using System;
using System.Collections.Generic;
using Ardalis.Result;
using MediatR;
using Forma.Query.QueriesModel;

namespace Forma.Query.Application.Exercise.Queries;

/// <summary>
/// Null returns shared-library Exercises only. Non-null also includes that user's own private Exercises.
/// No auth exists yet, so this is caller-supplied — see docs/features/FT-001-ownership-visibility.md (Design section).
/// </summary>
public class GetAllExerciseQuery(Guid? requestingUserId) : IRequest<Result<IEnumerable<ExerciseQueryModel>>>
{
    public Guid? RequestingUserId { get; } = requestingUserId;
}