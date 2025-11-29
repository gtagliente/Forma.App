//using Forma.CoreContext.SharedKernel;
//using Forma.CoreInfrastructure.Abstractions;

//namespace Forma.Infrastructure.Data.Notifications;
//public class MediatorDomainEventNotification<TDomainEvent> :
//        IMediatorEventNotification, IDomainEventNotification<TDomainEvent> 
//    where TDomainEvent: BaseEvent
//{
//    public TDomainEvent DomainEvent { get; }

//    private MediatorDomainEventNotification(TDomainEvent domainEvent){
//        DomainEvent = domainEvent;
//    }

//    internal static MediatorDomainEventNotification<TDomainEvent> Create(TDomainEvent domainEvent)
//        => new(domainEvent);
//}
