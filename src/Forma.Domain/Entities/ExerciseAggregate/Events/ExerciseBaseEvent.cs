using System;
using System.Collections.Generic;
using Forma.CoreContext.SharedKernel;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate.Events;

public abstract class ExerciseBaseEvent : BaseEvent
{
    protected ExerciseBaseEvent(
        ExerciseId aggregateId,
        IReadOnlyCollection<MuscleGroup> muscleGroups,
        string name,
        string description,
        Guid? ownerId
        )
    {
        Id = Guid.NewGuid();
        AggregateId = aggregateId.Value;
        MuscleGroups = muscleGroups;
        Name = name;
        Description = description;
        OwnerId = ownerId;
    }

    public IReadOnlyCollection<MuscleGroup> MuscleGroups { get; private init; }
    public string Name { get; private init; }
    public string Description { get; private init; }
    public Guid? OwnerId { get; private init; }
}