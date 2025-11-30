using AutoMapper;
using Forma.Domain.Entities.ExerciseAggregate.Events;
using Forma.Query.QueriesModel;

namespace Forma.Query.Profiles;

public class EventToQueryModelProfile : Profile
{
    public EventToQueryModelProfile()
    {
        CreateMap<ExerciseCreatedEvent, ExerciseQueryModel>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AggregateId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.MuscleGroup, opt => opt.MapFrom(src => src.MuscleGroup));

        //CreateMap<CustomerUpdatedEvent, CustomerQueryModel>(MemberList.Destination)
        //    .ConstructUsing(@event => CreateCustomerQueryModel(@event));

        //CreateMap<CustomerDeletedEvent, CustomerQueryModel>(MemberList.Destination)
        //    .ConstructUsing(@event => CreateCustomerQueryModel(@event));
    }

    public override string ProfileName => nameof(EventToQueryModelProfile);

 }