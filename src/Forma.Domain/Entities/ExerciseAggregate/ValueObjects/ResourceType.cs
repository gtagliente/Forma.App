using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter<ResourceType>))]
public enum ResourceType
{
    Video,
    Image,
    Text,
    Uri
}
