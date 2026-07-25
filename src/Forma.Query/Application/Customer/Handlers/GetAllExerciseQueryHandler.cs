using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Forma.CoreInfrastructure.Abstractions;
using Forma.CoreInfrastructure.Caching;
using Forma.Query.Application.Exercise.Queries;
using Forma.Query.Data.Repositories.Abstractions;
using Forma.Query.QueriesModel;
using MediatR;


namespace Forma.Query.Application.Customer.Handlers;

public class GetAllExerciseQueryHandler(IExerciseReadOnlyRepository repository, ICacheService cacheService)
    : IRequestHandler<GetAllExerciseQuery, Result<IEnumerable<ExerciseQueryModel>>>
{
    public async Task<Result<IEnumerable<ExerciseQueryModel>>> Handle(
          GetAllExerciseQuery request,
          CancellationToken cancellationToken)
    {
        // Cache key must be scoped per requesting user — a shared key would leak one
        // user's private Exercises into another user's cached response.
        var cacheKey = ExerciseCacheKeys.ForUser(request.RequestingUserId);

        return Result<IEnumerable<ExerciseQueryModel>>.Success(
            await cacheService.GetOrCreateAsync(cacheKey, () => repository.GetVisibleToAsync(request.RequestingUserId)));
    }
}