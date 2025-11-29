using System.Text.Json.Serialization;

namespace Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter<MuscleGroup>))]
public enum MuscleGroup
{
    Chest,
    Back,
    Legs,
    Shoulders,
    Arms,
    Core,
    FullBody
}