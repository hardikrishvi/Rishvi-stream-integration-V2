using System.Text;

namespace Rishvi.Modules.ShippingIntegrations.Core.Middleware
{
    public class RequstResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequstResponseLoggingMiddleware> _logger;
        public RequstResponseLoggingMiddleware(RequestDelegate next, ILogger<RequstResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            // Log request
            var request = context.Request;
            var requestBodyContent = await ReadRequestBodyAsync(request);

            _logger.LogInformation("Request: {Method} {Path} - Body: {Body}",
                request.Method,
                request.Path,
                requestBodyContent);

            //capture the response
            var originalResponseBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware in the pipeline
            await _next(context);
            
            var responseBodyContent = await ReadResponseBodyAsync(context.Response);

            // Log response
            _logger.LogInformation("Outgoing Response: {StatusCode}-Body: {Body}",
                context.Response.StatusCode, responseBodyContent);

            //write the response back to the original stream
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }
        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if(request.ContentLength == 0) 
                return string.Empty;

            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0; // Reset the stream position for further processing
            return body;
        }
        private async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin); // Reset the stream position for further processing
            return body;
        }
    }
}
