using System;
using System.Collections.Generic;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using Forma.Query.Abstractions;

namespace Forma.Query.QueriesModel;

public class ExerciseQueryModel : IQueryModel<Guid>
{
    //Public init for mappers and serialization
    public Guid Id { get;  init; }
    public string Name { get;  init; }
    public IReadOnlyCollection<MuscleGroup> MuscleGroups { get;  init; }
    public string Description { get;  init; }
    public Guid? OwnerId { get; init; }
}