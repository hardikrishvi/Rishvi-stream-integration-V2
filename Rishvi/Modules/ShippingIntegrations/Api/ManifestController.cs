using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Manifest")]
    public class ManifestController : ControllerBase
    {
        private readonly IAuthorizationToken _authorizationToken;
        public ManifestController(IAuthorizationToken authorizationToken)
        {
            _authorizationToken = authorizationToken;       
        }

        [HttpPost, Route("CreateManifest")]
        public IActionResult CreateManifest(Models.CreateManifestRequest request)
        {
            try
            {

                // Validate request
                if (request == null || string.IsNullOrEmpty(request.AuthorizationToken))
                {
                    return BadRequest(new Models.CreateManifestResponse("Invalid request: AuthorizationToken is required."));
                }

                // Load authorization configuration
                var auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return Unauthorized(new Models.CreateManifestResponse("Authorization failed for token: " + request.AuthorizationToken));
                }

                // Generate dummy manifest reference
                var manifestReference = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();
                return Ok(new Models.CreateManifestResponse
                {
                    ManifestReference = manifestReference
                });

            }
            catch (Exception ex)
            {
                // Log the error (replace with ILogger for production)
                Console.WriteLine($"Error creating manifest: {ex.Message}");
                return StatusCode(500, new Models.CreateManifestResponse("An unexpected error occurred while creating the manifest."));
            }

        }

        [HttpPost, Route("PrintManifest")]
        public IActionResult PrintManifest([FromBody] Models.PrintManifestRequest request)
        {
            try
            {
                // Validate request
                if (request == null || string.IsNullOrWhiteSpace(request.AuthorizationToken))
                {
                    return BadRequest(new Models.PrintManifestResponse
                    {
                        IsError = true,
                        ErrorMessage = "Invalid request: AuthorizationToken is required."
                    });
                }

                // Load authorization configuration
                var auth = _authorizationToken.Load(request.AuthorizationToken);
                if (auth == null)
                {
                    return Unauthorized(new Models.PrintManifestResponse
                    {
                        IsError = true,
                        ErrorMessage = "Authorization failed for token: " + request.AuthorizationToken
                    });
                }

                // Check manifest support
                var response = new Models.PrintManifestResponse
                {
                    IsError = true,
                    ErrorMessage = "This integration doesn't support End Of Day manifest documentation."
                };

                // Log the unsupported feature
                Console.WriteLine("Manifest printing is not supported for this integration."); // Replace with ILogger

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with ILogger for production)
                Console.WriteLine($"Error in PrintManifest: {ex.Message}");
                return StatusCode(500, new Models.PrintManifestResponse
                {
                    IsError = true,
                    ErrorMessage = "An unexpected error occurred while processing the manifest request."
                });
            }
        }


    }
}
