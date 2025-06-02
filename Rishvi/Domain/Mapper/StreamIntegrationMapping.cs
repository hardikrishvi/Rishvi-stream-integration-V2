using AutoMapper;


namespace Rishvi.Domain.Mapper
{
    public class StreamIntegrationMapping : Profile
    {
        public StreamIntegrationMapping()
        {
            CreateMap<DTOs.Webhook.WebhookOrderDto, Models.WebhookOrder>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.sequence, opt => opt.MapFrom(src => src.sequence))
                .ForMember(dest => dest.order, opt => opt.MapFrom(src => src.order));

            CreateMap<DTOs.Run.RunDto, Models.Run>();

            CreateMap<DTOs.Subscription.SubscriptionDto, Models.Subscription>();

            CreateMap<DTOs.Event.EventDto, Models.Event>();

            CreateMap<DTOs.Authorization.AuthorizationDto, Models.Authorization>();
        }
    }
}
