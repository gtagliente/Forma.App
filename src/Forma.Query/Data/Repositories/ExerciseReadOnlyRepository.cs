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
    public async Task<IEnumerable<ExerciseQueryModel>> GetAllAsync()
    {
        var sort = Builders<ExerciseQueryModel>.Sort
            .Ascending(customer => customer.Name);

        var findOptions = new FindOptions<ExerciseQueryModel>
        {
            Sort = sort
        };

        using var asyncCursor = await Collection.FindAsync(Builders<ExerciseQueryModel>.Filter.Empty, findOptions);
        return await asyncCursor.ToListAsync();
    }

}