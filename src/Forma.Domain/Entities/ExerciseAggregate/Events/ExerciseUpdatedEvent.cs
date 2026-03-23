using System;
using System.Collections.Generic;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate.Events;

public class ExerciseUpdatedEvent(
        ExerciseId aggregateId,
        IReadOnlyCollection<MuscleGroup> muscleGroup,
        string name,
        string description) : ExerciseBaseEvent(aggregateId, muscleGroup, name, description);