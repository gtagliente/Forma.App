using System;
using Forma.CoreContext.SharedKernel;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate.Events;

public abstract class ExerciseBaseEvent : BaseEvent
{
    protected ExerciseBaseEvent(
        Guid aggregateId,
        MuscleGroup muscleGroup,
        string name,
        string description
        )
    {
        AggregateId = aggregateId;
        MuscleGroup = muscleGroup;
        Name = name;
        Description = description;
    }

    public Guid Id { get; private init; }
    public MuscleGroup MuscleGroup { get; private init; }
    public string Name { get; private init; }
    public string Description { get; private init; }
}