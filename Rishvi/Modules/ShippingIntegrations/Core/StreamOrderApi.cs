using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ErrorLogs.Services;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Rishvi_Vault;
using Service = Rishvi.Modules.ShippingIntegrations.Models.Service;

namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public class StreamOrderApi
    {
        private IErrorLogService _errorLogService;
        private SqlContext _context { get; }
        public StreamOrderApi(IErrorLogService errorLogService, SqlContext context)
        {
            this._errorLogService = errorLogService;
            _context = context;
        }

        public static StreamGetOrderResponse.Root GetOrder(string streamAuthToken, string orderNo, string clientId)
        {
            try
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;

                var client = new RestClient(baseUrl);
                var request = new RestRequest($"/orders/orders/{orderNo}", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Stream-Nonce", uniqueCode);
                request.AddHeader("Stream-Party", clientId);
                request.AddHeader("Authorization", $"bearer {streamAuthToken}");

                RestResponse<StreamGetOrderResponse.Root> response = client.Execute<StreamGetOrderResponse.Root>(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<StreamGetOrderResponse.Root>(response.Content);
                }
                else
                {
                    SqlHelper.SystemLogInsert(
                        "GetOrder",
                        $"OrderNo: {orderNo}, ClientId: {clientId}",
                        streamAuthToken,
                        response.Content?.Replace("'", "''"),
                        "GetOrderFailed",
                        response.ErrorMessage?.Replace("'", "''"),
                        true
                    );
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert(
                    "GetOrderException",
                    $"OrderNo: {orderNo}, ClientId: {clientId}",
                    streamAuthToken,
                    null,
                    "GetOrderError",
                    ex.Message.Replace("'", "''"),
                    true
                );
            }

            return null;
        }


        public static async Task<StreamOrderResponse> CreateOrderAsync(
            GenerateLabelRequest request, string clientId, string authToken,
            CourierService service, bool onlyCreate, string type, string streamorderid)
        {
            try
            {
                if (string.IsNullOrEmpty(streamorderid))
                {
                    return await CreateNewOrderAsync(request, clientId, authToken, service, type);
                }
                else
                {
                    return HandleExistingOrder(request, clientId, authToken, streamorderid);
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("CreateOrderError", null,
                    JsonConvert.SerializeObject(request).Replace("'", "''"), null, "OrderCatchError", ex.Message, true);

                return new StreamOrderResponse
                {
                    IsError = true,
                    ErrorMessage = $"Exception: {ex.Message}"
                };
            }
        }

        private static async Task<StreamOrderResponse> CreateNewOrderAsync(
            GenerateLabelRequest request, string clientId, string authToken,
            CourierService service, string type)
        {
            var uniqueCode = CodeHelper.GenerateUniqueCode(32);
            var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;

            var client = new RestClient(baseUrl);
            var reqBody = MappingStreamOrderRequest(request, service, type);

            var requestObj = new RestRequest(AWSParameter.GetConnectionString(AppSettings.CreateOrderUrl), Method.Post)
                .AddJsonBody(reqBody)
                .AddHeader("Accept", "application/json")
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Stream-Nonce", uniqueCode)
                .AddHeader("Stream-Party", clientId)
                .AddHeader("Authorization", $"bearer {authToken}");

            var response = await client.ExecuteAsync<StreamOrderResponse>(requestObj);

            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                return JsonConvert.DeserializeObject<StreamOrderResponse>(response.Content);
            }
            else
            {
                SqlHelper.SystemLogInsert("CreateOrder", null, JsonConvert.SerializeObject(reqBody).Replace("'", "''"),
                    response.Content?.Replace("'", "''"), "OrderCreated", response.ErrorMessage?.Replace("'", "''"), true);

                return new StreamOrderResponse
                {
                    IsError = true,
                    ErrorMessage = $"Stream API error: {response.ErrorMessage ?? response.Content}"
                };
            }
        }

        private static StreamOrderResponse HandleExistingOrder(
         GenerateLabelRequest request, string clientId, string authToken, string streamOrderId)
        {
            var streamOrder = GetOrder(authToken, streamOrderId, clientId);
            var changes = PrepareOrderChanges(streamOrder, request);

            if (changes.Any())
            {
                UpdateOrder(changes, clientId, authToken, streamOrderId);
            }

            var items = streamOrder?.response?.order?.groups?.FirstOrDefault()?.items;
            if (items != null && items.Any())
            {
                request.Packages.Add(new Package
                {
                    Items = items.Select(i => new Item
                    {
                        ProductCode = i.code,
                        ItemName = i.description,
                        Quantity = i.quantity,
                        UnitWeight = i.weight
                    }).ToList()
                });
            }

            return new StreamOrderResponse
            {
                response = new Response
                {
                    trackingId = streamOrder?.response?.order?.trackingId,
                    consignmentNo = streamOrder?.response?.order?.header.consignmentNo,
                    orderNo = streamOrder?.response?.order?.header.orderNo,
                    customerOrderNo = streamOrder?.response?.order?.header.customerOrderNo,
                    trackingURL = streamOrder?.response?.order?.trackingURL,
                    valid = true,
                    errors = new List<Error>()
                    {
                        new Error
                        {
                            code = "0",
                            description = "Order updated successfully",
                            severity = "info"
                        }
                    }
                }
            };
        }

        private static Dictionary<string, string> PrepareOrderChanges(StreamGetOrderResponse.Root streamOrder, GenerateLabelRequest request)
        {
            var changes = new Dictionary<string, string>();

            if (streamOrder?.response?.order?.header?.customer?.address?.address1 != request.AddressLine1)
                changes.Add("address1", request.AddressLine1);
            if (streamOrder?.response?.order?.header?.customer?.address?.address2 != request.AddressLine2)
                changes.Add("address2", request.AddressLine2);
            if (streamOrder?.response?.order?.header?.customer?.address?.address3 != request.AddressLine3)
                changes.Add("address3", request.AddressLine3);
            if (streamOrder?.response?.order?.header?.customer?.address?.postcode != request.Postalcode)
                changes.Add("postcode", request.Postalcode);
            if (streamOrder?.response?.order?.header?.customer?.address?.name != request.Name)
                changes.Add("name", request.Name);
            if (streamOrder?.response?.order?.header?.customer?.contact?.email != request.Email)
                changes.Add("email", request.Email);
            if (streamOrder?.response?.order?.header?.customer?.contact?.tel1 != request.Phone)
                changes.Add("phone", request.Phone);
            if (streamOrder?.response?.order?.groups?.FirstOrDefault()?.driverNotes != request.DeliveryNote)
                changes.Add("deliveryNote", request.DeliveryNote);
            if (streamOrder?.response?.order?.header?.customerOrderNo != request.OrderReference)
                changes.Add("customerOrderNo", request.OrderReference);
            if (streamOrder?.response?.order?.groups?.FirstOrDefault()?.items?.Count > 0)
            {
                request.Packages.Add(new Package
                {
                    Items = streamOrder.response.order.groups.FirstOrDefault().items.Select(i => new Item
                    {
                        ProductCode = i.code,
                        ItemName = i.description,
                        Quantity = i.quantity,
                        UnitWeight = i.weight
                    }).ToList()
                });
            }
            return changes;
        }

        public static StreamOrderResponse UpdateOrder(
            Dictionary<string, string> changereq,
            string clientId,
            string streamAuthToken,
            string orderId)
        {
            var result = new StreamOrderResponse();
            try
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;

                var client = new RestClient(baseUrl);
                var request = new RestRequest($"/orders/orders/{orderId}", Method.Patch);

                // Build payload object
                var payload = new StreamOrderUpdateReq.Root
                {
                    header = new StreamOrderUpdateReq.Header
                    {
                        customer = new StreamOrderUpdateReq.Customer
                        {
                            account = (changereq.ContainsKey("accountname") || changereq.ContainsKey("accountnumber"))
                                ? new StreamOrderUpdateReq.Account
                                {
                                    name = changereq.GetValueOrDefault("accountname"),
                                    number = changereq.GetValueOrDefault("accountnumber")
                                } : null,
                            address = new StreamOrderUpdateReq.Address
                            {
                                address1 = changereq.GetValueOrDefault("address1"),
                                address2 = changereq.GetValueOrDefault("address2"),
                                address3 = changereq.GetValueOrDefault("address3"),
                                address4 = changereq.GetValueOrDefault("address4"),
                                address5 = changereq.GetValueOrDefault("address5"),
                                country = changereq.GetValueOrDefault("country"),
                                externalAddressId = changereq.GetValueOrDefault("externalAddressId"),
                                locationNotes = changereq.GetValueOrDefault("locationNotes"),
                                name = changereq.GetValueOrDefault("name"),
                                nuts = changereq.GetValueOrDefault("nuts"),
                                postcode = changereq.GetValueOrDefault("postcode"),
                                vehicleType = changereq.GetValueOrDefault("vehicleType")
                            },
                            contact = (changereq.ContainsKey("financialemail") ||
                                       changereq.ContainsKey("operationsemail") ||
                                       changereq.ContainsKey("secondaryemail"))
                                ? new StreamOrderUpdateReq.Contact
                                {
                                    altemail = new List<StreamOrderUpdateReq.Altemail>
                                    {
                                new StreamOrderUpdateReq.Altemail
                                {
                                    financial = changereq.GetValueOrDefault("financialemail"),
                                    operations = changereq.GetValueOrDefault("operationsemail"),
                                    secondary = changereq.GetValueOrDefault("secondaryemail")
                                }
                                    }
                                } : null,
                            name = changereq.GetValueOrDefault("name")
                        },
                        customerOrderNo = changereq.GetValueOrDefault("customerOrderNo"),
                        driverLinks = (changereq.ContainsKey("driverlinkdescription") || changereq.ContainsKey("driverlink"))
                            ? new List<StreamOrderUpdateReq.DriverLink>
                            {
                        new StreamOrderUpdateReq.DriverLink
                        {
                            description = changereq.GetValueOrDefault("driverlinkdescription"),
                            link = changereq.GetValueOrDefault("driverlink")
                        }
                            } : null,
                        driverNotes = changereq.GetValueOrDefault("driverNotes"),
                        orderDate = changereq.GetValueOrDefault("orderDate"),
                        orderNotes = changereq.GetValueOrDefault("orderNotes"),
                        partner = (changereq.ContainsKey("partnerid") || changereq.ContainsKey("partnername"))
                            ? new StreamOrderUpdateReq.Partner
                            {
                                id = changereq.GetValueOrDefault("partnerid"),
                                name = changereq.GetValueOrDefault("partnername")
                            } : null,
                        routeInfo = changereq.GetValueOrDefault("routeInfo"),
                        serviceLevel = changereq.GetValueOrDefault("serviceLevel")
                    }
                };

                var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var finalPayload = JsonConvert.SerializeObject(payload, jsonSettings);

                request.AddJsonBody(finalPayload);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Stream-Nonce", uniqueCode);
                request.AddHeader("Stream-Party", clientId);
                request.AddHeader("Authorization", $"bearer {streamAuthToken}");

                var response = client.Execute<StreamOrderResponse>(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    result = JsonConvert.DeserializeObject<StreamOrderResponse>(response.Content);
                }
                else
                {
                    SqlHelper.SystemLogInsert("UpdateOrder", null, finalPayload,
                        response.Content?.Replace("'", "''"),
                        "OrderUpdateFailed",
                        response.ErrorMessage?.Replace("'", "''"),
                        true);

                    result.IsError = true;
                    result.ErrorMessage = $"Stream update failed: {response.ErrorMessage ?? response.Content}";
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert("UpdateOrderException", null, null, null,
                    "OrderUpdateException", ex.Message.Replace("'", "''"), true);

                result.IsError = true;
                result.ErrorMessage = $"Exception: {ex.Message}";
            }

            return result;
        }


        public static StreamDeleteOrderResponse DeleteOrder(
             string streamAuthToken,
             string orderNo,
             string clientId)
        {
            var result = new StreamDeleteOrderResponse();
            try
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;

                var client = new RestClient(baseUrl);
                var request = new RestRequest($"{AWSParameter.GetConnectionString(AppSettings.DeleteOrderUrl)}{orderNo}", Method.Delete);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Stream-Nonce", uniqueCode);
                request.AddHeader("Stream-Party", clientId);
                request.AddHeader("Authorization", $"bearer {streamAuthToken}");

                var response = client.Execute<StreamDeleteOrderResponse>(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    result = JsonConvert.DeserializeObject<StreamDeleteOrderResponse>(response.Content);
                }
                else
                {
                    SqlHelper.SystemLogInsert(
                        "DeleteOrder",
                        $"OrderNo: {orderNo}, ClientId: {clientId}",
                        streamAuthToken,
                        response.Content?.Replace("'", "''"),
                        "OrderDeleteFailed",
                        response.ErrorMessage?.Replace("'", "''"),
                        true
                    );
                }
            }
            catch (Exception ex)
            {
                SqlHelper.SystemLogInsert(
                    "DeleteOrderException",
                    $"OrderNo: {orderNo}, ClientId: {clientId}",
                    streamAuthToken,
                    null,
                    "DeleteOrderError",
                    ex.Message.Replace("'", "''"),
                    true
                );
            }

            return result;
        }

        public static WebhookSubscribeResp.Root WebhookSubscribe(string streamAuthToken, string authToken, string eventname, string event_type, string url_path, string http_method, string content_type, string auth_header, string clientId)
        {
            var streamOrderResponse = new WebhookSubscribeResp.Root();
            string errorMessage = string.Empty;
            try
            {
                var req = new WebhookSubscribeReq()
                {
                    auth_header = auth_header,
                    content_type = content_type,
                    @event = eventname,
                    event_type = event_type,
                    url_path = url_path,
                    http_method = http_method
                };
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;
                var client = new RestClient(baseUrl); // Fix: Instantiate RestClient with the base URL
                var request = new RestRequest("/webhooks/webhooks", Method.Post);
                request.AddJsonBody(JsonConvert.SerializeObject(req));
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Stream-Nonce", uniqueCode);
                request.AddHeader("Stream-Party", clientId);
                request.AddHeader("Authorization", "bearer " + streamAuthToken);
                var response = client.Execute<WebhookSubscribeResp.Root>(request);
                if (response.IsSuccessful)
                {
                    //success order log
                    streamOrderResponse = JsonConvert.DeserializeObject<WebhookSubscribeResp.Root>(response.Content);
                }
                else
                {
                    //failed order log
                    errorMessage = response.Content;
                    SqlHelper.SystemLogInsert(
                        "WebhookSubscribe",
                        $"Event: {eventname}, ClientId: {clientId}",
                        streamAuthToken,
                        response.Content?.Replace("'", "''"),
                        "WebhookSubscribeFailed",
                        response.ErrorMessage?.Replace("'", "''"),
                        true
                    );
                }
            }
            catch (WebException ex)
            {
                SqlHelper.SystemLogInsert(
                   "WebhookSubscribeException",
                   $"Event: {eventname}, ClientId: {clientId}",
                   streamAuthToken,
                   null,
                   "WebhookSubscribeError",
                   ex.Message.Replace("'", "''"),
                   true
               );
            }
            return streamOrderResponse;
        }

        public static string MappingStreamOrderRequest(GenerateLabelRequest generateLabelRequest, CourierService service, string type)
        {
            if (type == "COLLECTION")
            {
                CollectionOrderRequest.Root streamOrderRequest = new CollectionOrderRequest.Root();
                streamOrderRequest.header = new CollectionOrderRequest.Header();
                streamOrderRequest.header.orderNo = generateLabelRequest.OrderId > 0 ? generateLabelRequest.OrderId.ToString() : "";
                if (streamOrderRequest.header.orderNo == "")
                {
                    streamOrderRequest.header.orderNo = generateLabelRequest.OrderReference;
                }
                streamOrderRequest.header.orderDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                streamOrderRequest.header.orderType = type;
                streamOrderRequest.header.services = new List<CollectionOrderRequest.Service>();
                streamOrderRequest.header.services.Add(new CollectionOrderRequest.Service { code = service.ServiceCode });
                streamOrderRequest.header.customer = new CollectionOrderRequest.Customer();
                streamOrderRequest.header.customer.name = generateLabelRequest.Name;
                streamOrderRequest.header.customer.address = new CollectionOrderRequest.Address();
                streamOrderRequest.header.customer.address.name = generateLabelRequest.Name;
                streamOrderRequest.header.customer.address.address1 = generateLabelRequest.AddressLine1;
                streamOrderRequest.header.customer.address.address2 = generateLabelRequest.AddressLine2;
                streamOrderRequest.header.customer.address.address3 = generateLabelRequest.AddressLine3;
                streamOrderRequest.header.customer.address.country = generateLabelRequest.CountryCode;
                streamOrderRequest.header.customer.address.postcode = generateLabelRequest.Postalcode;
                streamOrderRequest.header.customer.contact = new CollectionOrderRequest.Contact();
                streamOrderRequest.header.customer.contact.name = generateLabelRequest.Name;
                streamOrderRequest.header.customer.contact.tel1 = generateLabelRequest.Phone;
                streamOrderRequest.header.customer.contact.email = generateLabelRequest.Email;
                streamOrderRequest.header.customer.contact.optOutEmail = false;
                streamOrderRequest.header.customer.contact.optOutSms = false;
                streamOrderRequest.header.customerOrderNo = generateLabelRequest.OrderReference;
                streamOrderRequest.header.driverNotes = generateLabelRequest.DeliveryNote;
                streamOrderRequest.header.cutOffTimeMet = true;
                streamOrderRequest.header.updateAddresses = false;
                streamOrderRequest.collection = new CollectionOrderRequest.Collection();
                streamOrderRequest.collection.address = new CollectionOrderRequest.Address();
                streamOrderRequest.collection.address.name = generateLabelRequest.Name;
                streamOrderRequest.collection.address.address1 = generateLabelRequest.AddressLine1;
                streamOrderRequest.collection.address.address2 = generateLabelRequest.AddressLine2;
                streamOrderRequest.collection.address.address3 = generateLabelRequest.AddressLine3;
                streamOrderRequest.collection.address.country = generateLabelRequest.CountryCode;
                streamOrderRequest.collection.address.postcode = generateLabelRequest.Postalcode;
                streamOrderRequest.collection.contact = new CollectionOrderRequest.Contact();
                streamOrderRequest.collection.contact.name = generateLabelRequest.Name;
                streamOrderRequest.collection.contact.tel1 = generateLabelRequest.Phone;
                streamOrderRequest.collection.contact.email = generateLabelRequest.Email;
                streamOrderRequest.collection.contact.optOutEmail = false;
                streamOrderRequest.collection.contact.optOutSms = false;
                streamOrderRequest.collection.collectionMethod = "NORTH";
                streamOrderRequest.collection.bookingRequired = true;
                int itemCount = 1;
                streamOrderRequest.collection.items = new List<CollectionOrderRequest.Item>();
                foreach (var packages in generateLabelRequest.Packages)
                {
                    foreach (var item in packages.Items)
                    {
                        streamOrderRequest.collection.items.Add(new CollectionOrderRequest.Item
                        {
                            sequence = itemCount,
                            code = item.ProductCode,
                            description = item.ItemName,
                            quantity = item.Quantity,
                            weight = item.UnitWeight,
                            stockLocation = "SGK",
                        });
                        itemCount++;
                    }
                }
                return JsonConvert.SerializeObject(streamOrderRequest);
            }
            else
            {
                StreamOrderRequest streamOrderRequest = new StreamOrderRequest();
                streamOrderRequest.header.orderNo = generateLabelRequest.OrderId > 0 ? generateLabelRequest.OrderId.ToString() : "";
                if (streamOrderRequest.header.orderNo == "")
                {
                    streamOrderRequest.header.orderNo = generateLabelRequest.OrderReference;
                }
                streamOrderRequest.header.orderDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                // Fix for CS0118: 'Service' is a namespace but is used like a type
                // The issue occurs because the `Service` identifier conflicts with the `Rishvi.Modules.ShippingIntegrations.Models.Service` namespace.
                // To resolve this, fully qualify the type `Service` with its namespace.

                streamOrderRequest.header.services.Add(new Rishvi.Modules.ShippingIntegrations.Models.Service { code = service.ServiceCode });
                streamOrderRequest.header.orderType = type;
                //streamOrderRequest.header.services.Add(new Service { code = service.ServiceCode });
                streamOrderRequest.header.customer.name = generateLabelRequest.Name;
                streamOrderRequest.header.customer.address.name = generateLabelRequest.Name;
                streamOrderRequest.header.customer.address.address1 = generateLabelRequest.AddressLine1;
                streamOrderRequest.header.customer.address.address2 = generateLabelRequest.AddressLine2;
                streamOrderRequest.header.customer.address.address3 = generateLabelRequest.AddressLine3;
                streamOrderRequest.header.customer.address.country = generateLabelRequest.CountryCode;
                streamOrderRequest.header.customer.address.postcode = generateLabelRequest.Postalcode;
                streamOrderRequest.header.customer.contact.name = generateLabelRequest.Name;
                streamOrderRequest.header.customer.contact.tel1 = generateLabelRequest.Phone;
                streamOrderRequest.header.customer.contact.email = generateLabelRequest.Email;
                streamOrderRequest.header.customer.contact.optOutEmail = false;
                streamOrderRequest.header.customer.contact.optOutSms = false;
                streamOrderRequest.header.customerOrderNo = generateLabelRequest.OrderReference;
                streamOrderRequest.header.driverNotes = generateLabelRequest.DeliveryNote;
                streamOrderRequest.header.cutOffTimeMet = true;
                streamOrderRequest.header.updateAddresses = false;
                streamOrderRequest.delivery.address.name = generateLabelRequest.Name;
                streamOrderRequest.delivery.address.address1 = generateLabelRequest.AddressLine1;
                streamOrderRequest.delivery.address.address2 = generateLabelRequest.AddressLine2;
                streamOrderRequest.delivery.address.address3 = generateLabelRequest.AddressLine3;
                streamOrderRequest.delivery.address.country = generateLabelRequest.CountryCode;
                streamOrderRequest.delivery.address.postcode = generateLabelRequest.Postalcode;
                streamOrderRequest.delivery.contact.name = generateLabelRequest.Name;
                streamOrderRequest.delivery.contact.tel1 = generateLabelRequest.Phone;
                streamOrderRequest.delivery.contact.email = generateLabelRequest.Email;
                streamOrderRequest.delivery.contact.optOutEmail = false;
                streamOrderRequest.delivery.contact.optOutSms = false;
                streamOrderRequest.delivery.deliveryMethod = "SGK";
                streamOrderRequest.delivery.bookingRequired = true;
                int itemCount = 1;
                foreach (var packages in generateLabelRequest.Packages)
                {
                    foreach (var item in packages.Items)
                    {
                        streamOrderRequest.delivery.items.Add(new StreamOrderItem
                        {
                            sequence = itemCount,
                            code = item.ProductCode,
                            description = item.ItemName,
                            quantity = item.Quantity,
                            weight = item.UnitWeight,
                            stockLocation = "SGK",
                        });
                        itemCount++;
                    }
                }

                var streamOrderRequestJson = JsonConvert.SerializeObject(streamOrderRequest);
                return streamOrderRequestJson;
            }

        }

    }
}
