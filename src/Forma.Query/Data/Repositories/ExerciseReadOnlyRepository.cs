using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forma.Query.Abstractions;
using Forma.Query.Data.Repositories.Abstractions;
using Forma.Query.QueriesModel;
using MongoDB.Driver;

namespace Forma.Query.Data.Repositories;

internal class ExerciseReadOnlyRepository(IReadDbContext readDbContext)
    : BaseReadOnlyRepository<ExerciseQueryModel, Guid>(readDbContext), IExerciseReadOnlyRepository
{
    public async Task<IEnumerable<ExerciseQueryModel>> GetVisibleToAsync(Guid? requestingUserId)
    {
        var sort = Builders<ExerciseQueryModel>.Sort
            .Ascending(exercise => exercise.Name);

        var findOptions = new FindOptions<ExerciseQueryModel>
        {
            Sort = sort
        };

        var filter = Builders<ExerciseQueryModel>.Filter.Or(
            Builders<ExerciseQueryModel>.Filter.Eq(exercise => exercise.OwnerId, null),
            Builders<ExerciseQueryModel>.Filter.Eq(exercise => exercise.OwnerId, requestingUserId));

        using var asyncCursor = await Collection.FindAsync(filter, findOptions);
        return await asyncCursor.ToListAsync();
    }

}