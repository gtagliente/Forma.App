using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Forma.CoreInfrastructure.Abstractions;
using Forma.Domain.Entities.ExerciseAggregate.Events;
using Forma.Query.Abstractions;
using Forma.Query.Application.Exercise.Queries;
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
        var cacheKeys = new[] { nameof(GetAllExerciseQuery)};
        await cacheService.RemoveAsync(cacheKeys);
    }

    private void LogEvent<TEvent>(TEvent @event) where TEvent : class =>
        logger.LogInformation("----- Triggering the event {EventName}, model: {EventModel}", typeof(TEvent).Name, @event.ToString());
}
