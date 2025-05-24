using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;




namespace Rishvi.Modules.ShippingIntegrations.Api
{
    [Route("api/Config")]
    public class ConfigController : ControllerBase
    {
        private readonly AwsS3 _awsS3;
        private readonly ServiceHelper _serviceHelper;
        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;

        public ConfigController(AwsS3 awsS3, ServiceHelper serviceHelper, TradingApiOAuthHelper tradingApiOAuthHelper)
        {
            _awsS3 = awsS3;
            _serviceHelper = serviceHelper;
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
        }

        [HttpPost, Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegistrationData request)
        {
            try
            {
                var transformedEmail = _serviceHelper.TransformEmail(request.Email);
                if (!await AwsS3.S3FileIsExists("Authorization", "Users/" + "_register_" + transformedEmail + ".json"))
                {
                    request.Password = _serviceHelper.HashPassword(request.Password);

                    _tradingApiOAuthHelper.RegisterSave(JsonConvert.SerializeObject(request), "", transformedEmail, request.AuthorizationToken);
                    return Ok("ok");
                }
                else
                {
                    return Conflict("Email already registered.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] RegistrationData value)
        {
            try
            {
                var transformedEmail = _serviceHelper.TransformEmail(value.Email);
                var fileName = "Users/" + "_register_" + transformedEmail + ".json";
                if (!await AwsS3.S3FileIsExists("Authorization", fileName))
                {
                    return NotFound("Email not registered.");
                }
                var output = AwsS3.GetS3File("Authorization", fileName);
                var res = JsonConvert.DeserializeObject<RegistrationData>(output);
                if (res.Password == _serviceHelper.HashPassword(value.Password))
                {
                    return Ok("ok");
                }
                else
                {
                    return Unauthorized("Incorrect Password.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (use ILogger)
                Console.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet, Route("Get/{email}")]
        public async Task<IActionResult> Get(string email)
        {
            try
            {
                var transformedEmail = _serviceHelper.TransformEmail(email);
                var fileName = "Users/" + "_register_" + transformedEmail + ".json";

                // Retrieve the file directly
                if (await AwsS3.S3FileIsExists("Authorization", fileName))
                {
                    var result = AwsS3.GetS3File("Authorization", fileName);
                    var output = JsonConvert.DeserializeObject<RegistrationData>(result);
                    // Ensure SyncModel is not null
                    output.Sync ??= new SyncModel();
                    return Ok(output);

                }
                else
                {
                    return NotFound("User not found.");
                }
            }
            catch (Exception ex)
            {
                // Log the error (replace with ILogger for production use)
                Console.WriteLine($"Error retrieving user data: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost, Route("Save")]
        public async Task<IActionResult> Save([FromBody] RegistrationData value)
        {
            try
            {
                // Ensure Sync is initialized
                value.Sync ??= new SyncModel();

                var transformedEmail = _serviceHelper.TransformEmail(value.Email);
                var fileName = "Users/" + "_register_" + transformedEmail + ".json";

                // Check if file exists
                if (await AwsS3.S3FileIsExists("Authorization", fileName))
                {
                    _tradingApiOAuthHelper.RegisterSave(JsonConvert.SerializeObject(value), "", transformedEmail,value.AuthorizationToken);
                    return Ok("User data saved successfully.");
                }
                else
                {
                    return NotFound("Email not registered.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (replace with ILogger for production)
                Console.WriteLine($"Error while saving data: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
