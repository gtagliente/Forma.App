using System;
using System.Collections.Generic;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate.Events;

public class ExerciseCreatedEvent(
        ExerciseId aggregateId,
        IReadOnlyCollection<MuscleGroup> muscleGroups,
        string name,
        string description,
        Guid? ownerId) : ExerciseBaseEvent(aggregateId, muscleGroups, name, description, ownerId);