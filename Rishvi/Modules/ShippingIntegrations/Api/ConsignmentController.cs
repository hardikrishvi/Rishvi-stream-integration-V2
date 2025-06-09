using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public ConsignmentController(IAuthorizationToken authorizationToken)
        {
            _authorizationToken = authorizationToken;
        }

        [HttpPost(), Route("CreateOrder")]
        public GenerateLabelResponse CreateOrder([FromBody] GenerateLabelRequest request)
        {
            try
            {
                // lets authenticate the user and make sure we have their config details
                AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new GenerateLabelResponse("Authorization failed for token " + request.AuthorizationToken);
                }

                SqlHelper.SystemLogInsert("CreateOrder", "", JsonConvert.SerializeObject(request).Replace("'", "''"), "", "GenerateLabel", "", false);
                // load all the services we have (either for this user specifically or all services)
                List<CourierService> services = Services.GetServices;

                //linnworks will send the serviceId as defined in list of services, we will need to find the service by id 
                CourierService selectedService = services.Find(s => s.ServiceUniqueId == request.ServiceId);
                if (selectedService == null)
                {
                    throw new Exception("Service Id " + request.ServiceId.ToString() + " is not available");
                }

                // get the service code
                string serviceCode = selectedService.ServiceCode;
                //and some other information, whatever we need
                string VendorCode = selectedService.ServiceGroup;

                //create response class, we will be adding packages to it
                GenerateLabelResponse response = new GenerateLabelResponse();
                var streamAuth = ManageToken.GetToken(auth);
                var streamOrderResponse = StreamOrderApi.CreateOrder(request, auth.ClientId, streamAuth.AccessToken, selectedService, true, "DELIVERY", request.OrderId);
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
                    return response;
                }
                else
                {
                    return new GenerateLabelResponse(streamOrderResponse.Item2);
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"), null, "OrderCatchError", ex.Message, true);
                EmailHelper.SendEmail("Failed generate lable", ex.Message);
                return new GenerateLabelResponse("Unhandled error " + ex.Message);
            }
        }

        [HttpPost, Route("GenerateLabel")]
        public GenerateLabelResponse GenerateLabel([FromBody] GenerateLabelRequest request)
        {
            try
            {
                // lets authenticate the user and make sure we have their config details
                AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new GenerateLabelResponse("Authorization failed for token " + request.AuthorizationToken);
                }

                SqlHelper.SystemLogInsert("CreateOrder", "", JsonConvert.SerializeObject(request).Replace("'", "''"), "", "GenerateLabel", "", false);
                // load all the services we have (either for this user specifically or all services)
                List<CourierService> services = Services.GetServices;

                //linnworks will send the serviceId as defined in list of services, we will need to find the service by id 
                CourierService selectedService = services.Find(s => s.ServiceUniqueId == request.ServiceId);
                if (selectedService == null)
                {
                    throw new Exception("Service Id " + request.ServiceId.ToString() + " is not available");
                }

                // get the service code
                string serviceCode = selectedService.ServiceCode;
                //and some other information, whatever we need
                string VendorCode = selectedService.ServiceGroup;

                //create response class, we will be adding packages to it
                GenerateLabelResponse response = new GenerateLabelResponse();
                var streamAuth = ManageToken.GetToken(auth);
                var streamOrderResponse = StreamOrderApi.CreateOrder(request, auth.ClientId, streamAuth.AccessToken, selectedService, false, "DELIVERY",request.OrderId);
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
                    return response;
                }
                else
                {
                    if (streamOrderResponse.Item2.Contains("Order Number already exists"))
                    {
                        string abc = streamOrderResponse.Item2.ToString();
                        var changereq = new Dictionary<string, string>
                                {
                                  
                                    { "address1", request.AddressLine1 },
                                    { "address2", request.AddressLine2 },
                                    { "address3", request.AddressLine3 },
                                    { "orderid", request.OrderId.ToString() } // if you're using it for UpdateOrder
                                };

                        var streamOrderResponse1 = StreamOrderApi.UpdateOrder(changereq, auth.ClientId, streamAuth.AccessToken, request.OrderId.ToString());

                        if (streamOrderResponse1.Item1.response != null && string.IsNullOrEmpty(streamOrderResponse1.Item2))
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
                                    if (response.LeadTrackingNumber == "") { response.LeadTrackingNumber = streamOrderResponse1.Item1.response.trackingId; }
                                    // we now need to add packages back into response, one item per package which contains label and any associated documentation
                                    response.Package.Add(new PackageResponse()
                                    {
                                        LabelHeight = 6,                // label height in inches
                                        LabelWidth = 4,                 // label width in inches
                                        PNGLabelDataBase64 = LabelGenerator.GenerateLabel(request, item, streamOrderResponse1.Item1.response.trackingId, streamOrderResponse1.Item1.response.consignmentNo, CodeHelper.FormatAddress(request), itemCount, totalItemCount, "", ""), // generate the label image, get its bytes and convert bytes to Base64 string
                                        SequenceNumber = package.SequenceNumber,    //VERY IMPORTANT TO PRESERVE Sequence number for each package!!!!
                                        PDFBytesDocumentationBase64 = new string[] { },         // here we can add any additional documentation, such as customs forms, declarations etc. PDF files converted to Base64 string
                                        TrackingNumber = streamOrderResponse1.Item1.response.trackingId // package tracking number
                                    });

                                    /* Here you can also save the consignment data and associate package/label information in some sort of database
                                     * if you need to have manifestation or label cancelation reference numbers associated with orderReferences or Order Ids in linnworks
                                     */
                                    itemCount++;
                                }
                            }
                            return response;
                        }
                        else
                        {
                            return new GenerateLabelResponse(streamOrderResponse1.Item2);
                        }



                    }
                    else
                    {
                        return new GenerateLabelResponse(streamOrderResponse.Item2);
                    }

                   
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"), null, "OrderCatchError", ex.Message, true);
                EmailHelper.SendEmail("Failed generate lable", ex.ToString());
                return new GenerateLabelResponse("Unhandled error " + ex.ToString()) { IsError = true };
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
                SqlHelper.SystemLogInsert("DeleteOrder", null, JsonConvert.SerializeObject(request).Replace("'", "''"), null, "CancelLabelStart", null, false);

                AuthorizationConfigClass auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return new CancelLabelResponse("Authorization failed for token " + request.AuthorizationToken);
                }

                // implement label cancelation routine here 
                // remember that request will 

                //Call stream delete order api 
                var streamAuth = ManageToken.GetToken(auth);
                var streamDeleteOrderResponse = StreamOrderApi.DeleteOrder(streamAuth.AccessToken, request.OrderReference, auth.ClientId);
                if (streamDeleteOrderResponse.Item1.response == null && !string.IsNullOrEmpty(streamDeleteOrderResponse.Item2))
                {
                    return new CancelLabelResponse(streamDeleteOrderResponse.Item2);
                }
            }
            catch (Exception ex)
            {
                EmailHelper.SendEmail("Failed generate lable", ex.Message);
            }
            return new CancelLabelResponse();
        }
    }
}
