using System;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate.Events;

public class ExerciseCreatedEvent(
        Guid aggregateId,
        MuscleGroup muscleGroup,
        string name,
        string description) : ExerciseBaseEvent(aggregateId, muscleGroup, name, description);