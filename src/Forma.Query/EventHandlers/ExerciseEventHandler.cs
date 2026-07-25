using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Forma.CoreInfrastructure.Abstractions;
using Forma.CoreInfrastructure.Caching;
using Forma.Domain.Entities.ExerciseAggregate.Events;
using Forma.Query.Abstractions;
using Forma.Query.QueriesModel;
using MediatR;
using Microsoft.Extensions.Logging;


namespace Forma.Query.EventHandlers;

public class ExerciseEventHandler(
    IMapper mapper,
    ISynchronizeDb synchronizeDb,
    ICacheService cacheService,
    ILogger<ExerciseEventHandler> logger) :
    INotificationHandler<ExerciseCreatedEvent>,
    INotificationHandler<ExerciseUpdatedEvent>,
    INotificationHandler<ExerciseDeletedEvent>
{
    public async Task Handle(ExerciseCreatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var exerciseQueryModel = mapper.Map<ExerciseQueryModel>(notification);
        await synchronizeDb.UpsertAsync(exerciseQueryModel, filter => filter.Id == notification.AggregateId);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(ExerciseUpdatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var exerciseQueryModel = mapper.Map<ExerciseQueryModel>(notification);
        await synchronizeDb.UpsertAsync(exerciseQueryModel, filter => filter.Id == notification.AggregateId);
        await ClearCacheAsync(notification);
    }

    public async Task Handle(ExerciseDeletedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        await synchronizeDb.DeleteAsync<ExerciseQueryModel>(filter => filter.Id == notification.AggregateId);
        await ClearCacheAsync(notification);
    }

    private async Task ClearCacheAsync(ExerciseBaseEvent @event)
    {
        // Cheap redundant complement to the load-bearing fix in the command handlers (which
        // always know the real acting user) — this scopes invalidation to the Exercise's own
        // OwnerId (null for shared Exercises, which correctly targets the shared-library cache
        // entry shared by every anonymous/unauthenticated GetAll caller).
        await cacheService.RemoveAsync(ExerciseCacheKeys.ForUser(@event.OwnerId));
    }

    private void LogEvent<TEvent>(TEvent @event) where TEvent : class =>
        logger.LogInformation("----- Triggering the event {EventName}, model: {EventModel}", typeof(TEvent).Name, @event.ToString());
}
