using AutoMapper;
using Rishvi.Domain.DTOs.Ebay;
using Rishvi.Domain.DTOs.IntegrationSettings;
using Rishvi.Domain.DTOs.LinnworksSettings;
using Rishvi.Domain.DTOs.ReportModel;
using Rishvi.Domain.DTOs.StreamOrderRecord;
using Rishvi.Domain.DTOs.StreamSettings;
using Rishvi.Domain.DTOs.SyncSettings;
using Rishvi.DTOs.Address;
using Rishvi.DTOs.CustomerInfo;
using Rishvi.DTOs.Fulfillment;
using Rishvi.DTOs.GeneralInfo;
using Rishvi.DTOs.Item;
using Rishvi.DTOs.OrderRoot;
using Rishvi.DTOs.ShippingInfo;
using Rishvi.DTOs.TaxInfo;
using Rishvi.DTOs.TotalsInfo;
using Rishvi.Models;
using Rishvi.Modules.ShippingIntegrations.Models;
using Address = Rishvi.Models.Address;


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

            CreateMap<AddressDto, Address>();
            CreateMap<CustomerInfoDto, CustomerInfo>();
            CreateMap<FulfillmentDto, Fulfillment>();
            CreateMap<GeneralInfoDto, GeneralInfo>();
          //  CreateMap<ItemDto, Item>();
            CreateMap<OrderRootDto,OrderRoot>();
            CreateMap<ShippingInfoDto,ShippingInfo>();
            CreateMap<TaxInfoDto,TaxInfo>();
            CreateMap<TotalsInfoDto, TotalsInfo>();
            
            CreateMap<SyncSettingsDto, SyncSettings>();
            CreateMap<StreamSettingsDto, StreamSettings>();
            CreateMap<LinnworksSettingsDto, LinnworksSettings>();
            CreateMap<IntegrationSettingsDto, IntegrationSettings>();
            CreateMap<EbayListDto, Ebay>();
            CreateMap<ReportModelDto, ReportModel>();
            CreateMap<StreamOrderRecordDto, StreamOrderRecord>();

        }
    }
}
