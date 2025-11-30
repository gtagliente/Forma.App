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


namespace Shop.Query.EventHandlers;

public class ExerciseEventHandler<T>(
    IMapper mapper,
    ISynchronizeDb synchronizeDb,
    ICacheService cacheService,
    ILogger<ExerciseEventHandler<T>> logger) :
    INotificationHandler<ExerciseCreatedEvent> 
{
    public async Task Handle(ExerciseCreatedEvent notification, CancellationToken cancellationToken)
    {
        LogEvent(notification);

        var exerciseQueryModel = mapper.Map<ExerciseQueryModel>(notification);
        await synchronizeDb.UpsertAsync(exerciseQueryModel, filter => filter.Id == notification.AggregateId);
        await ClearCacheAsync(notification);
    }

    //public async Task Handle(CustomerDeletedEvent notification, CancellationToken cancellationToken)
    //{
    //    LogEvent(notification);

    //    await synchronizeDb.DeleteAsync<CustomerQueryModel>(filter => filter.Email == notification.Email);
    //    await ClearCacheAsync(notification);
    //}

    //public async Task Handle(CustomerUpdatedEvent notification, CancellationToken cancellationToken)
    //{
    //    LogEvent(notification);

    //    var customerQueryModel = mapper.Map<CustomerQueryModel>(notification);
    //    await synchronizeDb.UpsertAsync(customerQueryModel, filter => filter.Id == customerQueryModel.Id);
    //    await ClearCacheAsync(notification);
    //}


    private async Task ClearCacheAsync(ExerciseBaseEvent @event)
    {
        var cacheKeys = new[] { nameof(GetAllExerciseQuery)};
        await cacheService.RemoveAsync(cacheKeys);
    }

    private void LogEvent<TEvent>(TEvent @event) where TEvent : class =>
        logger.LogInformation("----- Triggering the event {EventName}, model: {EventModel}", typeof(TEvent).Name, @event.ToString());
}