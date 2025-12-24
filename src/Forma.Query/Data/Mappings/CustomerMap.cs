using MongoDB.Bson.Serialization;
using Forma.Query.Abstractions;
using Forma.Query.QueriesModel;
// using Forma.Query.QueriesModel;

namespace Forma.Query.Data.Mappings;

public class ExerciseMap : IReadDbMapping
{
    public void Configure()
    {
        // TryRegisterClassMap: Registers a class map if it is not already registered.
        BsonClassMap.TryRegisterClassMap<ExerciseQueryModel>(classMap =>
        {
            classMap.AutoMap();
            classMap.SetIgnoreExtraElements(true);

            classMap.MapMember(exercise => exercise.Id)
                .SetIsRequired(true);

            classMap.MapMember(exercise => exercise.Name)
                .SetIsRequired(true);

            classMap.MapMember(exercise => exercise.MuscleGroups)
                .SetIsRequired(true);

            classMap.MapMember(exercise => exercise.Description)
                .SetIsRequired(true);
        });
    }
}