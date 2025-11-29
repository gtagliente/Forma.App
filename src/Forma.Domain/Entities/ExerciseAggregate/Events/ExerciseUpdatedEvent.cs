using System;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate.Events;

public class ExerciseUpdatedEvent(
        Guid aggregateId,
        MuscleGroup muscleGroup,
        string name,
        string description) : ExerciseBaseEvent(aggregateId, muscleGroup, name, description);