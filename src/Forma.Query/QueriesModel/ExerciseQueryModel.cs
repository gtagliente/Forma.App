using System;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using Forma.Query.Abstractions;

namespace Forma.Query.QueriesModel;

public class ExerciseQueryModel : IQueryModel<Guid>
{
    public ExerciseQueryModel(
        Guid id,
        string name,
        MuscleGroup muscleGroup,
        string description
        )
    {
        Id = id;
        Name = name;
        MuscleGroup = muscleGroup;
        Description = description;
    }

    private ExerciseQueryModel()
    {
    }

    public Guid Id { get; private init; }
    public string Name { get; private init; }
    public MuscleGroup MuscleGroup { get; private init; }
    public string Description { get; private init; }
}