using Newtonsoft.Json;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Service;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

namespace Rishvi.Modules.ShippingIntegrations.Core.Service
{
    public class ConsignmentService : IConsignmentService
    {
        private readonly IAuthorizationToken _authorizationToken;
        private readonly ManageToken _manageToken;

        public ConsignmentService(
            IAuthorizationToken authorizationToken,
            ManageToken manageToken)
        {
            _authorizationToken = authorizationToken;
            _manageToken = manageToken;
        }

        public GenerateLabelResponse CreateOrder(GenerateLabelRequest request)
        {
            string email = "";
            try
            {
                var auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                    return new GenerateLabelResponse($"Authorization failed for token {request.AuthorizationToken}") { IsError = true };

                email = auth.Email;
                SqlHelper.SystemLogInsert("CreateOrder", "", JsonConvert.SerializeObject(request).Replace("'", "''"),
                    "", "GenerateLabel", "", false);

                var selectedService = Services.GetServices.Find(s => s.ServiceUniqueId == request.ServiceId);
                if (selectedService == null)
                    throw new Exception($"Service Id {request.ServiceId} is not available");

                var streamAuth = _manageToken.GetToken(auth);

                // Await the asynchronous task to get the actual StreamOrderResponse object
                var streamOrderResponse = StreamOrderApi.CreateOrderAsync(
                    request, auth.ClientId, streamAuth.AccessToken, selectedService, true, "DELIVERY", request.OrderId.ToString()).Result;

                return BuildLabelResponse(request, streamOrderResponse);
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"),
                    null, "OrderCatchError", ex.Message, true);
                EmailHelper.SendEmail("Failed generate label", ex.Message);

                return new GenerateLabelResponse($"Unhandled error: {ex.Message}") { IsError = true };
            }
        }

        private GenerateLabelResponse BuildLabelResponse(GenerateLabelRequest request, StreamOrderResponse streamOrderResponse)
        {
            var response = new GenerateLabelResponse();
            int itemCount = 1;
            int totalItemCount = request.Packages.Sum(s => s.Items.Count);

            foreach (var package in request.Packages)
            {
                foreach (var item in package.Items)
                {
                    var safePlace1 = request.OrderExtendedProperties
                        .FirstOrDefault(s => s.Name == "SafePlace1")?.Value ?? "";

                    if (string.IsNullOrEmpty(response.LeadTrackingNumber))
                        response.LeadTrackingNumber = streamOrderResponse.response.trackingId;

                    response.Package.Add(new PackageResponse
                    {
                        LabelHeight = 6,
                        LabelWidth = 4,
                        PNGLabelDataBase64 = LabelGenerator.GenerateLabel(
                            request, item,
                            streamOrderResponse.response.trackingId,
                            streamOrderResponse.response.consignmentNo,
                            CodeHelper.FormatAddress(request),
                            itemCount, totalItemCount, "", ""),
                        SequenceNumber = package.SequenceNumber,
                        PDFBytesDocumentationBase64 = Array.Empty<string>(),
                        TrackingNumber = streamOrderResponse.response.trackingId
                    });

                    itemCount++;
                }
            }
            return response;
        }

        public GenerateLabelResponse GenerateLabel(GenerateLabelRequest request)
        {
            try
            {
                var auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                    return new GenerateLabelResponse($"Authorization failed for token {request.AuthorizationToken}") { IsError = true };

                SqlHelper.SystemLogInsert("CreateOrder", "", JsonConvert.SerializeObject(request).Replace("'", "''"),
                    "", "GenerateLabel", "", false);

                var selectedService = Services.GetServices.Find(s => s.ServiceUniqueId == request.ServiceId);
                if (selectedService == null)
                    throw new Exception($"Service Id {request.ServiceId} is not available");

                var streamAuth = _manageToken.GetToken(auth);

                var streamOrderResponse = StreamOrderApi.CreateOrderAsync(
                    request, auth.ClientId, streamAuth.AccessToken, selectedService, false, "DELIVERY", request.OrderId.ToString()
                ).Result;

                if (streamOrderResponse.IsError)
                {
                    if (streamOrderResponse.ErrorMessage.Contains("Order Number already exists"))
                    {
                        var changeReq = new Dictionary<string, string>
                        {
                            { "address1", request.AddressLine1 },
                            { "address2", request.AddressLine2 },
                            { "address3", request.AddressLine3 },
                            { "orderid", request.OrderId.ToString() }
                        };

                        var updateResponse = StreamOrderApi.UpdateOrder(changeReq, auth.ClientId, streamAuth.AccessToken, request.OrderId.ToString());
                        if (!updateResponse.IsError)
                        {
                            return BuildLabelResponse(request, updateResponse);
                        }
                        else
                        {
                            return new GenerateLabelResponse(updateResponse.ErrorMessage) { IsError = true };
                        }
                    }

                    return new GenerateLabelResponse(streamOrderResponse.ErrorMessage) { IsError = true };
                }

                return BuildLabelResponse(request, streamOrderResponse);
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"),
                    null, "OrderCatchError", ex.Message, true);
                EmailHelper.SendEmail("Failed generate label", ex.ToString());
                return new GenerateLabelResponse($"Unhandled error: {ex.Message}") { IsError = true };
            }
        }

        public CancelLabelResponse CancelLabel(CancelLabelRequest request)
        {
            string email = "";
            try
            {
                SqlHelper.SystemLogInsert("DeleteOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"),
                    null, "CancelLabelStart", null, false);

                var auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new CancelLabelResponse($"Authorization failed for token {request.AuthorizationToken}") { IsError = true };
                }

                var streamAuth = _manageToken.GetToken(auth);

                var streamDeleteOrderResponse = StreamOrderApi.DeleteOrder(
                    streamAuth.AccessToken, request.OrderReference, auth.ClientId
                );

                // Fix: Access the `response.errors` property directly instead of using `Item2`  
                if (streamDeleteOrderResponse.response != null && streamDeleteOrderResponse.response.errors != null && streamDeleteOrderResponse.response.errors.Any())
                {
                    return new CancelLabelResponse(string.Join(", ", streamDeleteOrderResponse.response.errors)) { IsError = true };
                }

                return new CancelLabelResponse("Order cancellation successful.");
            }
            catch (Exception ex)
            {
                EmailHelper.SendEmail("Failed to cancel label", ex.Message);
                return new CancelLabelResponse($"Unhandled error: {ex.Message}") { IsError = true };
            }
        }


    }
}
