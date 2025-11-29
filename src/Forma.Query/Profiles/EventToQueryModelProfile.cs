using AutoMapper;
using Forma.Domain.Entities.ExerciseAggregate.Events;
using Forma.Query.QueriesModel;

namespace Forma.Query.Profiles;

public class EventToQueryModelProfile : Profile
{
    public EventToQueryModelProfile()
    {
        CreateMap<ExerciseCreatedEvent, ExerciseQueryModel>(MemberList.Destination)
            .ConstructUsing(@event => CreateExerciseQueryModel(@event));

        //CreateMap<CustomerUpdatedEvent, CustomerQueryModel>(MemberList.Destination)
        //    .ConstructUsing(@event => CreateCustomerQueryModel(@event));

        //CreateMap<CustomerDeletedEvent, CustomerQueryModel>(MemberList.Destination)
        //    .ConstructUsing(@event => CreateCustomerQueryModel(@event));
    }

    public override string ProfileName => nameof(EventToQueryModelProfile);

    private static ExerciseQueryModel CreateExerciseQueryModel<TEvent>(TEvent @event) where TEvent : ExerciseBaseEvent =>
        new(@event.Id, @event.Name, @event.MuscleGroup, @event.Description);
}