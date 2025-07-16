using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public interface IPostalServicesController
    {
        PostalService CreatePostalService(PostalService_WithChannelAndShippingLinks PostalServiceDetails);
        void DeletePostalService(Guid idToDelete);
        List<ChannelServiceLinks> GetChannelLinks(Guid postalServiceId);
        List<PostalService_WithChannelAndShippingLinks> GetPostalServices();
        void UpdatePostalService(PostalService PostalServiceDetails);
    }
}