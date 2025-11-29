//using System;
//using System.Collections.Concurrent;
//using System.Linq.Expressions;
//using System.Threading;
//using System.Threading.Tasks;
//using Forma.CoreContext.SharedKernel;
//using Forma.CoreInfrastructure.Abstractions;
//using MediatR;

//namespace Forma.Infrastructure.Data.Notifications;
//public class MediatorDomainEventDispatcher : IDomainEventDispatcher, IMediatorEventDispatcher
//{
//    public IMediator Mediator { get; }
//    private static readonly ConcurrentDictionary<Type, Func<BaseEvent, INotification>> _notificationFactories = new();

//    public MediatorDomainEventDispatcher(IMediator mediator)
//    {
//        Mediator = mediator;
//    }

//    public async Task DispatchAsync(BaseEvent domainEvent, CancellationToken ct = default)
//    {
//        var notification = CreateNotification(domainEvent);
//        await Mediator.Publish(notification, ct);
//    }

//    private static INotification CreateNotification(BaseEvent domainEvent)
//    {
//        var factory = _notificationFactories.GetOrAdd(domainEvent.GetType(), static t =>
//        {
//            var genericType = typeof(MediatorDomainEventNotification<>).MakeGenericType(t);
//            var ctor = genericType.GetConstructor(new[] { t })!;
//            var param = Expression.Parameter(typeof(BaseEvent), "e");
//            var cast = Expression.Convert(param, t);
//            var newExpr = Expression.New(ctor, cast);
//            var lambda = Expression.Lambda<Func<BaseEvent, INotification>>(Expression.Convert(newExpr, typeof(INotification)), param);
//            return lambda.Compile();
//        });

//        return factory(domainEvent);
//    }
//}
