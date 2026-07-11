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
            .ForMember(dest => dest.MuscleGroups, opt => opt.MapFrom(src => src.MuscleGroups))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId));

        CreateMap<ExerciseUpdatedEvent, ExerciseQueryModel>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AggregateId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.MuscleGroups, opt => opt.MapFrom(src => src.MuscleGroups))
            .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId));
    }

    public override string ProfileName => nameof(EventToQueryModelProfile);

 }