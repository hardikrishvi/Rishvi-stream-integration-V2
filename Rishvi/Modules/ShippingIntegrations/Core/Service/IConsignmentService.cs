using Rishvi.Modules.ShippingIntegrations.Models;

namespace Rishvi.Modules.ShippingIntegrations.Core.Service
{
    public interface IConsignmentService
    {
        GenerateLabelResponse CreateOrder(GenerateLabelRequest request);
        GenerateLabelResponse GenerateLabel(GenerateLabelRequest request);
        CancelLabelResponse CancelLabel(CancelLabelRequest request);
    }
}
