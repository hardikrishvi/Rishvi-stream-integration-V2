using Amazon.Runtime.Internal.Transform;
using Newtonsoft.Json;
using RestSharp;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.ErrorLogs.Services;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Rishvi_Vault;
using System.Net;
using Service = Rishvi.Modules.ShippingIntegrations.Models.Service;

namespace Rishvi.Modules.ShippingIntegrations.Core
{
    public class StreamOrderApi
    {
        private IErrorLogService _errorLogService;
        private SqlContext _context { get; }
        //private readonly StreamApiSettings _streamApiSettings;
        public StreamOrderApi(IErrorLogService errorLogService, SqlContext context)
        {
            this._errorLogService = errorLogService;
            _context = context;
            //_streamApiSettings = streamApiSettings.Value;
        }
        //private string GetBaseUrl(string clientId)
        //{
        //    if (string.IsNullOrEmpty(clientId))
        //        throw new ArgumentNullException(nameof(clientId));

        //    return clientId.StartsWith("RIS") ? _streamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;
        //}

        public static StreamGetOrderResponse.Root GetOrder(string streamAuthToken, string orderNo, string clientId)
        {
            string uniqueCode = CodeHelper.GenerateUniqueCode(32);
            //var baseUrl = clientId.StartsWith("RIS") ? "https://www.demo.go2stream.net/api" : AppSettings.StreamApiBasePath;
            var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;
            var client = new RestClient(baseUrl); // Fix: Instantiate RestClient with the base URL
            var request = new RestRequest("/orders/orders/" + orderNo, Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Stream-Nonce", uniqueCode);
            request.AddHeader("Stream-Party", clientId);
            request.AddHeader("Authorization", "bearer " + streamAuthToken);
            RestResponse<StreamGetOrderResponse.Root> response = client.Execute<StreamGetOrderResponse.Root>(request); // Fix: Ensure client is properly instantiated
            if (response.IsSuccessful)
            {
                return JsonConvert.DeserializeObject<StreamGetOrderResponse.Root>(response.Content);
            }
            else
            {
                string errorMessage = response.Content;
                SqlHelper.SystemLogInsert("DeleteOrder", null, streamAuthToken, !string.IsNullOrEmpty(response.Content) ? response.Content.Replace("'", "''") : null, "OrderDeleted", !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage.Replace("'", "''") : null, true,clientId);
            }
            return null;
        }

        public static Tuple<StreamOrderResponse, string> CreateOrder(GenerateLabelRequest generateLabelRequest, string clientId, string streamAuthToken, CourierService service, bool onlycreate, string type, string streamorderid,string LocationName,string Handsondate)
        {
            StreamOrderResponse streamOrderResponse = new StreamOrderResponse();
            string errorMessage = string.Empty;
            if (streamorderid == null)
            {
                try
                {
                    string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                    var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;
                    var client = new RestClient(baseUrl); // Fix: Instantiate RestClient with the base URL
                    var request = new RestRequest(AppSettings.CreateOrderUrl, Method.Post);
                    request.AddJsonBody(MappingStreamOrderRequest(generateLabelRequest, service, type ,LocationName,Handsondate));
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Stream-Nonce", uniqueCode);
                    request.AddHeader("Stream-Party", clientId);
                    request.AddHeader("Authorization", "bearer " + streamAuthToken);
                    RestResponse<StreamOrderResponse> response = client.Execute<StreamOrderResponse>(request);
                    if (response.IsSuccessful)
                    {
                        streamOrderResponse = JsonConvert.DeserializeObject<StreamOrderResponse>(response.Content);
                    }
                    else
                    {
                        errorMessage = response.Content;
                        try
                        {
                            SqlHelper.SystemLogInsert("CreateOrder", null, MappingStreamOrderRequest(generateLabelRequest, service, type,LocationName,Handsondate), !string.IsNullOrEmpty(response.Content) ? response.Content.Replace("'", "''") : null, "OrderCreated", !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage.Replace("'", "''") : null, true, clientId);
                        }
                        catch
                        {

                        }

                    }
                }
                catch (WebException ex)
                {
                    string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                    if (ex.Response != null)
                    {
                        using (Stream responseStream = ex.Response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {

                            }
                        }
                    }
                }
            }
            else
            {
                StreamGetOrderResponse.Root streamOrder = GetOrder(streamAuthToken, streamorderid, clientId);
                var changes = new Dictionary<string, string>() { };
                //streamOrder.re.Addresses ??= new StreamGetOrderResponse.Addresses();
                //StreamGetOrderResponse.Address address = streamOrder.StreamGetOrderResponse.Addresses.Address1 ?? new StreamGetOrderResponse.Address();


                if (streamOrder?.response?.order?.header?.customer?.address?.address1 !=
                    generateLabelRequest.AddressLine1)
                {
                    changes.Add("address1", generateLabelRequest.AddressLine1);
                }
                if (streamOrder?.response?.order?.header?.customer?.address?.address2 !=
                    generateLabelRequest.AddressLine2)
                {
                    changes.Add("address2", generateLabelRequest.AddressLine2);
                }
                if (streamOrder?.response?.order?.header?.customer?.address?.address3 !=
                    generateLabelRequest.AddressLine3)
                {
                    changes.Add("address3", generateLabelRequest.AddressLine3);
                }
                if (streamOrder?.response?.order?.header?.customer?.address?.postcode !=
                    generateLabelRequest.Postalcode)
                {
                    changes.Add("postcode", generateLabelRequest.Postalcode);
                }
                if (streamOrder?.response?.order?.header?.customer?.address?.name !=
                    generateLabelRequest.Name)
                {
                    changes.Add("name", generateLabelRequest.Name);
                }
                if(streamOrder?.response?.order?.header?.customer?.contact?.email !=
                    generateLabelRequest.Email)
                {
                    changes.Add("email", generateLabelRequest.Email);
                }
                if(streamOrder?.response?.order?.header?.customer?.contact?.tel1 !=
                    generateLabelRequest.Phone)
                {
                    changes.Add("phone", generateLabelRequest.Phone);
                }
                if (streamOrder?.response?.order?.groups?.FirstOrDefault()?.driverNotes !=
                    generateLabelRequest.DeliveryNote)
                {
                    changes.Add("deliveryNote", generateLabelRequest.DeliveryNote);
                }
                if(streamOrder?.response?.order?.header?.customerOrderNo != 
                    generateLabelRequest.OrderReference)
                {
                    changes.Add("customerOrderNo", generateLabelRequest.OrderReference);
                }
                if(streamOrder?.response?.order?.groups?.FirstOrDefault().items.Count > 0)
                {
                    generateLabelRequest.Packages.Add(new Package()
                    {
                        Items = streamOrder.response.order.groups.FirstOrDefault().items.Select(i => new Item()
                        {
                            ProductCode = i.code,
                            ItemName = i.description,
                            Quantity = i.quantity,
                            UnitWeight = i.weight
                           
                        }).ToList()
                    });
                }


                if (changes.Count > 0)
                {
                    UpdateOrder(changes, clientId, streamAuthToken, streamorderid);
                }
            }
            return Tuple.Create(streamOrderResponse, errorMessage);
        }
        public static Tuple<StreamOrderResponse, string> UpdateOrder(Dictionary<string, string> changereq, string clientId, string streamAuthToken, string orderid)
        {
            StreamOrderResponse streamOrderResponse = new StreamOrderResponse();
            string errorMessage = string.Empty;
            try
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;
                var client = new RestClient(baseUrl); // Fix: Instantiate RestClient with the base URL
                var request = new RestRequest("/orders/orders/" + orderid, Method.Patch);
                string finalreq = JsonConvert.SerializeObject(new StreamOrderUpdateReq.Root()
                {
                    header = new StreamOrderUpdateReq.Header()
                    {
                        customer = new StreamOrderUpdateReq.Customer()
                        {
                            account = (changereq.ContainsKey("accountname") || changereq.ContainsKey("accountnumber")) ? new StreamOrderUpdateReq.Account()
                            {
                                name = changereq.ContainsKey("accountname") ? changereq["accountname"] : null,
                                number = changereq.ContainsKey("accountnumber") ? changereq["accountnumber"] : null
                            } : null,
                            address = new StreamOrderUpdateReq.Address()
                            {
                                address1 = changereq.ContainsKey("address1") ? changereq["address1"] : null,
                                address2 = changereq.ContainsKey("address2") ? changereq["address2"] : null,
                                address3 = changereq.ContainsKey("address3") ? changereq["address3"] : null,
                                address4 = changereq.ContainsKey("address4") ? changereq["address4"] : null,
                                address5 = changereq.ContainsKey("address5") ? changereq["address5"] : null,
                                country = changereq.ContainsKey("country") ? changereq["country"] : null,
                                externalAddressId = changereq.ContainsKey("externalAddressId") ? changereq["externalAddressId"] : null,
                                locationNotes = changereq.ContainsKey("locationNotes") ? changereq["locationNotes"] : null,
                                name = changereq.ContainsKey("name") ? changereq["name"] : null,
                                nuts = changereq.ContainsKey("nuts") ? changereq["nuts"] : null,
                                postcode = changereq.ContainsKey("postcode") ? changereq["postcode"] : null,
                                vehicleType = changereq.ContainsKey("vehicleType") ? changereq["vehicleType"] : null
                            },
                            contact = (changereq.ContainsKey("financialemail") || changereq.ContainsKey("operationsemail")
                            || changereq.ContainsKey("secondaryemail")) ? new StreamOrderUpdateReq.Contact()
                            {
                                altemail = new List<StreamOrderUpdateReq.Altemail>() {
                                                new StreamOrderUpdateReq.Altemail(){
                                                financial =  changereq.ContainsKey("financialemail") ? changereq["financialemail"] : null,
                                                operations =  changereq.ContainsKey("operationsemail") ? changereq["operationsemail"] : null,
                                                secondary = changereq.ContainsKey("secondaryemail") ? changereq["secondaryemail"] : null
                                    }
                                }
                            } : null,
                            name = changereq.ContainsKey("name") ? changereq["name"] : null

                        },
                        customerOrderNo = changereq.ContainsKey("customerOrderNo") ? changereq["customerOrderNo"] : null,
                        driverLinks = (changereq.ContainsKey("driverlinkdescription") || changereq.ContainsKey("driverlink")) ? new List<StreamOrderUpdateReq.DriverLink>()
                        {
                            new StreamOrderUpdateReq.DriverLink()
                            {
                                description = changereq.ContainsKey("driverlinkdescription") ? changereq["driverlinkdescription"] : null,
                                link = changereq.ContainsKey("driverlink") ? changereq["driverlink"] : null,
                            }
                        } : null,
                        driverNotes = changereq.ContainsKey("driverNotes") ? changereq["driverNotes"] : null,
                        orderDate = changereq.ContainsKey("orderDate") ? changereq["orderDate"] : null,
                        orderNotes = changereq.ContainsKey("orderNotes") ? changereq["orderNotes"] : null,
                        partner = (changereq.ContainsKey("partnerid") || changereq.ContainsKey("partnername")) ?
                        new StreamOrderUpdateReq.Partner()
                        {
                            id = changereq.ContainsKey("partnerid") ? changereq["partnerid"] : null,
                            name = changereq.ContainsKey("partnername") ? changereq["partnername"] : null,
                        } : null,
                        routeInfo = changereq.ContainsKey("routeInfo") ? changereq["routeInfo"] : null,
                        serviceLevel = changereq.ContainsKey("serviceLevel") ? changereq["serviceLevel"] : null,
                    }
                }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                request.AddJsonBody(finalreq);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Stream-Nonce", uniqueCode);
                request.AddHeader("Stream-Party", clientId);
                request.AddHeader("Authorization", "bearer " + streamAuthToken);
                RestResponse<StreamOrderResponse> response = client.Execute<StreamOrderResponse>(request);
                if (response.IsSuccessful)
                {
                    streamOrderResponse = JsonConvert.DeserializeObject<StreamOrderResponse>(response.Content);
                }
                else
                {
                    errorMessage = response.Content;
                    SqlHelper.SystemLogInsert("UpdateOrder", null, finalreq, !string.IsNullOrEmpty(response.Content) ? response.Content.Replace("'", "''") : null, "OrderUpdated", !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage.Replace("'", "''") : null, true, clientId);

                }
            }
            catch (WebException ex)
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                if (ex.Response != null)
                {
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {

                        }
                    }
                }
            }
            return Tuple.Create(streamOrderResponse, errorMessage);
        }

        public static Tuple<StreamDeleteOrderResponse, string> DeleteOrder(string streamAuthToken, string orderNo, string clientId)
        {
            StreamDeleteOrderResponse streamOrderResponse = new StreamDeleteOrderResponse();
            string errorMessage = string.Empty;
            try
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                var baseUrl = clientId.StartsWith("RIS") ? StreamApiSettings.DemoUrl : AppSettings.StreamApiBasePath;
                var client = new RestClient(baseUrl); // Fix: Instantiate RestClient with the base URL
                var request = new RestRequest(AWSParameter.GetConnectionString(AppSettings.DeleteOrderUrl) + orderNo, Method.Delete);
                //request.AddJsonBody(MappingStreamOrderRequest(generateLabelRequest, service));
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Stream-Nonce", uniqueCode);
                request.AddHeader("Stream-Party", clientId);
                request.AddHeader("Authorization", "bearer " + streamAuthToken);
                RestResponse<StreamDeleteOrderResponse> response = client.Execute<StreamDeleteOrderResponse>(request);
                if (response.IsSuccessful)
                {
                    //success order log
                    streamOrderResponse = JsonConvert.DeserializeObject<StreamDeleteOrderResponse>(response.Content);
                }
                else
                {
                    //failed order log
                    errorMessage = response.Content;
                    SqlHelper.SystemLogInsert("DeleteOrder", null, streamAuthToken, !string.IsNullOrEmpty(response.Content) ? response.Content.Replace("'", "''") : null, "OrderDeleted", !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage.Replace("'", "''") : null, true, clientId);
                }
            }
            catch (WebException ex)
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                if (ex.Response != null)
                {
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {

                        }
                    }
                }
            }
            return Tuple.Create(streamOrderResponse, errorMessage);
        }

        public static Tuple<WebhookSubscribeResp.Root, string> WebhookSubscribe(string streamAuthToken, string authToken, string eventname, string event_type, string url_path, string http_method, string content_type, string auth_header, string clientId)
        {
            WebhookSubscribeResp.Root streamOrderResponse = new WebhookSubscribeResp.Root();
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
                RestResponse<WebhookSubscribeResp.Root> response = client.Execute<WebhookSubscribeResp.Root>(request);
                if (response.IsSuccessful)
                {
                    //success order log
                    streamOrderResponse = JsonConvert.DeserializeObject<WebhookSubscribeResp.Root>(response.Content);
                }
                else
                {
                    //failed order log
                    errorMessage = response.Content;
                    SqlHelper.SystemLogInsert("WebhookSubscribe", null, streamAuthToken, !string.IsNullOrEmpty(response.Content) ? response.Content.Replace("'", "''") : null, "WebhookSubscribe", !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage.Replace("'", "''") : null, true, clientId);
                }
            }
            catch (WebException ex)
            {
                string uniqueCode = CodeHelper.GenerateUniqueCode(32);
                if (ex.Response != null)
                {
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {

                        }
                    }
                }
            }
            return Tuple.Create(streamOrderResponse, errorMessage);
        }

        public static string  MappingStreamOrderRequest(GenerateLabelRequest generateLabelRequest, CourierService service, string type, string LocationName, string Handsondate)
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
                            stockLocation = LocationName,
                            onHandDate = Handsondate
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
                streamOrderRequest.header.orderType = type;
                streamOrderRequest.header.services.Add(new Service { code = service.ServiceCode });
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
                            stockLocation = LocationName,
                            onHandDate = Handsondate
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
