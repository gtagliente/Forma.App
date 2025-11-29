using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Forma.CoreInfrastructure.Abstractions;
using Forma.Query.Application.Exercise.Queries;
using Forma.Query.Data.Repositories.Abstractions;
using Forma.Query.QueriesModel;
using MediatR;


namespace Forma.Query.Application.Customer.Handlers;

public class GetAllExerciseQueryHandler(IExerciseReadOnlyRepository repository, ICacheService cacheService)
    : IRequestHandler<GetAllExerciseQuery, Result<IEnumerable<ExerciseQueryModel>>>
{
    private const string CacheKey = nameof(GetAllExerciseQuery);

    public async Task<Result<IEnumerable<ExerciseQueryModel>>> Handle(
          GetAllExerciseQuery request,
          CancellationToken cancellationToken)
    {
        // This method will either return the cached data associated with the CacheKey
        // or create it by calling the GetAllAsync method.
        return Result<IEnumerable<ExerciseQueryModel>>.Success(
            await cacheService.GetOrCreateAsync(CacheKey, repository.GetAllAsync));
    }
}