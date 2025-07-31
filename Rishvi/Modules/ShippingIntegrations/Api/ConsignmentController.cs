using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Consignment")]
    public class ConsignmentController : ControllerBase
    {
        private readonly IAuthorizationToken _authorizationToken;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly ManageToken _manageToken;
        private readonly ILogger<ConsignmentController> _logger;
        public ConsignmentController(IAuthorizationToken authorizationToken, IUnitOfWork unitOfWork, ApplicationDbContext context, ManageToken manageToken, ILogger<ConsignmentController> logger)
        {
            _authorizationToken = authorizationToken;
            _unitOfWork = unitOfWork;
            _context = context;
            _manageToken = manageToken;
            _logger = logger;
        }

        [HttpPost(), Route("CreateOrder")]
        public GenerateLabelResponse CreateOrder([FromBody] GenerateLabelRequest request)
        {
            string Email = "";

            try
            {
                _logger.LogInformation("CreateOrder called with request: {Request}", JsonConvert.SerializeObject(request));
                // lets authenticate the user and make sure we have their config details
                Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new GenerateLabelResponse("Authorization failed for token " + request.AuthorizationToken);
                }
                Email = auth.Email;

                string LocationName = "SGK";
                string HandsonDate = "";

                if (auth.HandsOnDate)
                {
                    HandsonDate = DateTime.Now.ToString();
                }
                if (auth.UseDefaultLocation && auth.DefaultLocation != "")
                {
                    LocationName = auth.DefaultLocation;
                }



                SqlHelper.SystemLogInsert("CreateOrder", "", JsonConvert.SerializeObject(request).Replace("'", "''"), "", "GenerateLabel", "", false, auth.Email);
                // load all the services we have (either for this user specifically or all services)
                List<CourierService> services = Services.GetServices;

                //linnworks will send the serviceId as defined in list of services, we will need to find the service by id 
                CourierService selectedService = services.Find(s => s.ServiceUniqueId == request.ServiceId);
                if (selectedService == null)
                {
                    _logger.LogError("CreateOrder Service Id {ServiceId} is not available", request.ServiceId);
                    throw new Exception("Service Id " + request.ServiceId.ToString() + " is not available");
                }

                // get the service code
                string serviceCode = selectedService.ServiceCode;
                //and some other information, whatever we need 
                string VendorCode = selectedService.ServiceGroup;

                //create response class, we will be adding packages to it
                GenerateLabelResponse response = new GenerateLabelResponse();
                var streamAuth = _manageToken.GetToken(auth);

                var streamOrderResponse = StreamOrderApi.CreateOrder(request, auth.ClientId, streamAuth.AccessToken, selectedService, true, "DELIVERY", request.OrderId.ToString(), LocationName, HandsonDate, auth.IsLiveAccount);
                /* If you need to do any validation of services or consignment data, do it before you generate labels and simply throw an error 
                 * on the whole request
                 */
                if (streamOrderResponse.Item1.response != null && string.IsNullOrEmpty(streamOrderResponse.Item2))
                {
                    int itemCount = 1;
                    int totalItemCount = request.Packages.Sum(s => s.Items.Count());
                    foreach (var package in request.Packages)   // we need to generate a label for each package in the consignment
                    {
                        foreach (var item in package.Items)
                        {

                            // an order may have extended property bound to it, here we can pass any specific parameter we need
                            // in this specific example we will be taking SafePlace extended property of the order and outputting it on the label
                            string safePlace1 = "";
                            if (request.OrderExtendedProperties.Find(s => s.Name == "SafePlace1") != null)
                            {
                                safePlace1 = request.OrderExtendedProperties.Find(s => s.Name == "SafePlace1").Value;
                            }

                            //generate new tracking number
                            //string newTrackingNumber = request.CountryCode + " " + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                            // each consignment must have lead tracking number. In case of multiple packages this will be the main tracking number which allows us to track the whole shipment by one tracking number. When the courier doesn't support this simply allocate the first package tracking number as a lead tracking number
                            if (response.LeadTrackingNumber == "") { response.LeadTrackingNumber = streamOrderResponse.Item1.response.trackingId; }
                            // we now need to add packages back into response, one item per package which contains label and any associated documentation
                            response.Package.Add(new PackageResponse()
                            {
                                LabelHeight = 6,                // label height in inches
                                LabelWidth = 4,                 // label width in inches
                                PNGLabelDataBase64 = LabelGenerator.GenerateLabel(request, item, streamOrderResponse.Item1.response.trackingId, streamOrderResponse.Item1.response.consignmentNo, CodeHelper.FormatAddress(request), itemCount, totalItemCount, "", ""), // generate the label image, get its bytes and convert bytes to Base64 string
                                SequenceNumber = package.SequenceNumber,    //VERY IMPORTANT TO PRESERVE Sequence number for each package!!!!
                                PDFBytesDocumentationBase64 = new string[] { },         // here we can add any additional documentation, such as customs forms, declarations etc. PDF files converted to Base64 string
                                TrackingNumber = streamOrderResponse.Item1.response.trackingId // package tracking number
                            });

                            /* Here you can also save the consignment data and associate package/label information in some sort of database
                             * if you need to have manifestation or label cancelation reference numbers associated with orderReferences or Order Ids in linnworks
                             */
                            itemCount++;
                        }
                    }
                    _logger.LogInformation("CreateOrder completed successfully for OrderId: {OrderId}", request.OrderId);
                    return response;
                }
                else
                {
                    _logger.LogError("CreateOrder failed for OrderId: {OrderId} with error: {Error}", request.OrderId, streamOrderResponse.Item2);
                    return new GenerateLabelResponse(streamOrderResponse.Item2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in CreateOrder for OrderId: {OrderId}", request.OrderId);
                SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"), null, "OrderCatchError", ex.Message, true, Email);
                EmailHelper.SendEmail("Failed generate lable", ex.Message);
                return new GenerateLabelResponse("Unhandled error " + ex.Message);
            }
        }

        [HttpPost, Route("GenerateLabel")]

        public GenerateLabelResponse GenerateLabel([FromBody] GenerateLabelRequest request)
        {
            _logger.LogInformation("GenerateLabel called with request: {Request}", JsonConvert.SerializeObject(request));
            if (request.OrderId == 100479)
            {
                string Email = "";

                try
                {
                    // lets authenticate the user and make sure we have their config details
                    Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
                    if (auth == null)
                    {
                        return new GenerateLabelResponse("Authorization failed for token " + request.AuthorizationToken);
                    }
                    Email = auth.Email;
                    string LocationName = "SGK";
                    string HandsonDate = "";

                    if (auth.HandsOnDate)
                    {
                        HandsonDate = DateTime.Now.ToString();
                    }
                    if (auth.UseDefaultLocation && auth.DefaultLocation != "")
                    {
                        LocationName = auth.DefaultLocation;
                    }

                    SqlHelper.SystemLogInsert("GenerateLabel", "", JsonConvert.SerializeObject(request).Replace("'", "''"), "", "GenerateLabel", "", false, auth.Email);
                    // load all the services we have (either for this user specifically or all services)
                    List<CourierService> services = Services.GetServices;

                    //linnworks will send the serviceId as defined in list of services, we will need to find the service by id 
                    CourierService selectedService = services.Find(s => s.ServiceUniqueId == request.ServiceId);
                    if (selectedService == null)
                    {
                        _logger.LogError("GenerateLabel Service Id {ServiceId} is not available", request.ServiceId);
                        throw new Exception("Service Id " + request.ServiceId.ToString() + " is not available");
                    }

                    // get the service code
                    string serviceCode = selectedService.ServiceCode;
                    //and some other information, whatever we need
                    string VendorCode = selectedService.ServiceGroup;

                    //create response class, we will be adding packages to it
                    GenerateLabelResponse response = new GenerateLabelResponse();
                    var streamAuth = _manageToken.GetToken(auth);

                    StreamGetOrderResponse.Root streamOrder = StreamOrderApi.GetOrder(streamAuth.AccessToken, request.OrderId.ToString(), auth.ClientId, auth.IsLiveAccount);

                    //if (streamOrder!=null)
                    //{
                    //    string ord = ;
                    //}

                    //var streamOrderResponse = StreamOrderApi.CreateOrder(request, auth.ClientId, streamAuth.AccessToken, selectedService, false, "DELIVERY",request.OrderId.ToString(),LocationName,HandsonDate);
                    ///* If you need to do any validation of services or consignment data, do it before you generate labels and simply throw an error 
                    if (streamOrder != null)
                    {
                        int itemCount = 1;
                        int totalItemCount = request.Packages.Sum(s => s.Items.Count());
                        foreach (var package in request.Packages)   // we need to generate a label for each package in the consignment
                        {
                        if (Email== "johnny@max-motorcycles.co.uk" || Email == "info@linnworkscustom.com")
                        {
                           // an order may have extended property bound to it, here we can pass any specific parameter we need
                                            // in this specific example we will be taking SafePlace extended property of the order and outputting it on the label
                                            string safePlace1 = "";
                            if (request.OrderExtendedProperties.Find(s => s.Name == "SafePlace1") != null)
                            {
                                safePlace1 = request.OrderExtendedProperties.Find(s => s.Name == "SafePlace1").Value;
                            }

                            //generate new tracking number
                            //string newTrackingNumber = request.CountryCode + " " + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                            // each consignment must have lead tracking number. In case of multiple packages this will be the main tracking number which allows us to track the whole shipment by one tracking number. When the courier doesn't support this simply allocate the first package tracking number as a lead tracking number
                            if (response.LeadTrackingNumber == "") { response.LeadTrackingNumber = streamOrder?.response?.order?.trackingId; }
                            // we now need to add packages back into response, one item per package which contains label and any associated documentation
                            response.Package.Add(new PackageResponse()
                            {
                                LabelHeight = 6,                // label height in inches
                                LabelWidth = 4,                 // label width in inches
                                PNGLabelDataBase64 = LabelGenerator.GenerateLabel_New2(request, package.Items[0], streamOrder?.response?.order?.trackingId, streamOrder?.response?.order?.header?.consignmentNo, CodeHelper.FormatAddress(request), itemCount, totalItemCount, "", ""), // generate the label image, get its bytes and convert bytes to Base64 string
                                SequenceNumber = package.SequenceNumber,    //VERY IMPORTANT TO PRESERVE Sequence number for each package!!!!
                                PDFBytesDocumentationBase64 = new string[] { },         // here we can add any additional documentation, such as customs forms, declarations etc. PDF files converted to Base64 string
                                TrackingNumber = streamOrder?.response?.order?.trackingId // package tracking number
                            });

                            /* Here you can also save the consignment data and associate package/label information in some sort of database
                             * if you need to have manifestation or label cancelation reference numbers associated with orderReferences or Order Ids in linnworks
                             */
                            itemCount++;
                        }
                        else
                        {
                            foreach (var item in package.Items)
                            {

                                // an order may have extended property bound to it, here we can pass any specific parameter we need
                                // in this specific example we will be taking SafePlace extended property of the order and outputting it on the label
                                string safePlace1 = "";
                                if (request.OrderExtendedProperties.Find(s => s.Name == "SafePlace1") != null)
                                {
                                    safePlace1 = request.OrderExtendedProperties.Find(s => s.Name == "SafePlace1").Value;
                                }

                                //generate new tracking number
                                //string newTrackingNumber = request.CountryCode + " " + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                                // each consignment must have lead tracking number. In case of multiple packages this will be the main tracking number which allows us to track the whole shipment by one tracking number. When the courier doesn't support this simply allocate the first package tracking number as a lead tracking number
                                if (response.LeadTrackingNumber == "") { response.LeadTrackingNumber = streamOrder?.response?.order?.trackingId; }
                                // we now need to add packages back into response, one item per package which contains label and any associated documentation
                                response.Package.Add(new PackageResponse()
                                {
                                    LabelHeight = 6,                // label height in inches
                                    LabelWidth = 4,                 // label width in inches
                                    PNGLabelDataBase64 = LabelGenerator.GenerateLabel(request, item, streamOrder?.response?.order?.trackingId, streamOrder?.response?.order?.header?.consignmentNo, CodeHelper.FormatAddress(request), itemCount, totalItemCount, "", ""), // generate the label image, get its bytes and convert bytes to Base64 string
                                    SequenceNumber = package.SequenceNumber,    //VERY IMPORTANT TO PRESERVE Sequence number for each package!!!!
                                    PDFBytesDocumentationBase64 = new string[] { },         // here we can add any additional documentation, such as customs forms, declarations etc. PDF files converted to Base64 string
                                    TrackingNumber = streamOrder?.response?.order?.trackingId // package tracking number
                                });

                                /* Here you can also save the consignment data and associate package/label information in some sort of database
                                 * if you need to have manifestation or label cancelation reference numbers associated with orderReferences or Order Ids in linnworks
                                 */
                                itemCount++;
                            }
                        }

                            
                        }
                        _logger.LogInformation("GenerateLabel completed successfully for OrderId: {OrderId}", request.OrderId);
                        return response;
                    }
                    else
                    {
                        _logger.LogError("GenerateLabel failed for OrderId: {OrderId}", request.OrderId);
                        return new GenerateLabelResponse("Error");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in GenerateLabel for OrderId: {OrderId}", request.OrderId);
                    SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"), null, "OrderCatchError", ex.Message, true, Email);
                    EmailHelper.SendEmail("Failed generate lable", ex.ToString());
                    return new GenerateLabelResponse("Unhandled error " + ex.ToString()) { IsError = true };
                }
            }

        }

        // Converts a System.Drawing.Image to a byte array.
        static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            // Create a memory stream to store image data
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                // Save the image in PNG format into the memory stream
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                // Convert the memory stream to a byte array
                return ms.ToArray();
            }
        }

        [HttpPost, Route("CancelLabel")]
        public CancelLabelResponse CancelLabel([FromBody] CancelLabelRequest request)
        {
            try
            {
                _logger.LogInformation("CancelLabel called with request: {Request}", JsonConvert.SerializeObject(request));
                SqlHelper.SystemLogInsert("DeleteOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"), null, "CancelLabelStart", null, false, "");

                Rishvi.Models.Authorization auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new CancelLabelResponse("Authorization failed for token " + request.AuthorizationToken);
                }

                // implement label cancelation routine here 
                // remember that request will 

                //Call stream delete order api 
                var streamAuth = _manageToken.GetToken(auth);
                
                var streamDeleteOrderResponse = StreamOrderApi.DeleteOrder(streamAuth.AccessToken, request.OrderReference, auth.ClientId, auth.IsLiveAccount);

                _logger.LogInformation("Cancel Label Request Stream Auth Access Token: {AccessToken} and orderid {OrderReference}", streamAuth.AccessToken, request.OrderReference);
                if (streamDeleteOrderResponse.Item1.response == null && !string.IsNullOrEmpty(streamDeleteOrderResponse.Item2))
                {
                    _logger.LogError("CancelLabel failed for OrderReference: {OrderReference} with error: {Error}", request.OrderReference, streamDeleteOrderResponse.Item2);
                    return new CancelLabelResponse(streamDeleteOrderResponse.Item2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in CancelLabel for OrderReference: {OrderReference}", request.OrderReference);
                EmailHelper.SendEmail("Failed generate lable", ex.Message);
            }
            return new CancelLabelResponse();
        }
    }
}
