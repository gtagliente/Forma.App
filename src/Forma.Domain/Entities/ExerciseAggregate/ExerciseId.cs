using System;

namespace Forma.Domain.Entities.ExerciseAggregate;
public readonly record struct ExerciseId(Guid Value)
{
    public static ExerciseId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
