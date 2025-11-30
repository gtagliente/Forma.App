using System;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using Forma.Query.Abstractions;

namespace Forma.Query.QueriesModel;

public class ExerciseQueryModel : IQueryModel<Guid>
{
    public Guid Id { get; private init; }
    public string Name { get; private init; }
    public MuscleGroup MuscleGroup { get; private init; }
    public string Description { get; private init; }
}