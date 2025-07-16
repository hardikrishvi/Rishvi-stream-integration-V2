using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

namespace Rishvi.Modules.Core.Authorization
{
    public class APIRequestResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public APIRequestResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Stream originalRequestBody = httpContext.Request.Body;
            bool isErrorOnRequest = false;
            try
            {
                var routeData = httpContext.GetRouteData();
                string currentArea = Convert.ToString(routeData.Values["area"] ?? string.Empty);
                string currentAction = Convert.ToString(routeData.Values["action"] ?? string.Empty);
                string currentController = Convert.ToString(routeData.Values["controller"] ?? string.Empty);
                var url = UriHelper.GetDisplayUrl(httpContext.Request);

                httpContext.Request.EnableBuffering();
                using var reader = new StreamReader(httpContext.Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                var requestBody = await reader.ReadToEndAsync();
                var httpMethod = httpContext.Request.Method;
                var token = httpContext.Request.Headers["Authorization"];
                var ApiName = $"{(!string.IsNullOrWhiteSpace(currentArea) ? "/" + currentArea.Trim() : string.Empty)}/{currentController.Trim()}{(!string.IsNullOrWhiteSpace(currentAction) ? "/" + currentAction.Trim() : string.Empty)}";
                httpContext.Request.Body.Position = 0;
            }
            catch
            {
                isErrorOnRequest = true;
                httpContext.Request.Body = originalRequestBody;
            }

            if (!isErrorOnRequest)
            {
                Stream originalBody = httpContext.Response.Body;
                try
                {
                    using var memStream = new MemoryStream();
                    httpContext.Response.Body = memStream;
                    await _next(httpContext);
                    memStream.Position = 0;
                    string responseBody = new StreamReader(memStream).ReadToEnd();
                    var RequestResponseBody = $"{httpContext.Response.StatusCode}: {responseBody}";
                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                }
                finally
                {
                    httpContext.Response.Body = originalBody;
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}