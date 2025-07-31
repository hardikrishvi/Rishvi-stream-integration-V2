using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Models;


namespace Rishvi.Modules.ShippingIntegrations.Api
{

    [Route("api/Reports")]
    public class ReportsController : ControllerBase
    {
        private readonly Lazy<IServiceHelper> _serviceHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportsController> _logger;
        public ReportsController(Lazy<IServiceHelper> serviceHelper, IUnitOfWork unitOfWork, ILogger<ReportsController> logger)
        {
            _serviceHelper = serviceHelper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost, Route("GetReportData")]
        // public async Task<List<ReportModel>> GetReportData([FromBody] ReportModelReq value)
        // {
        //     if (value == null || string.IsNullOrWhiteSpace(value.email))
        //     {
        //         throw new ArgumentException("Email is required for fetching report data.");
        //     }
        //
        //     var reports = await _unitOfWork.Context.Set<ReportModel>()
        //         .Where(r => r.email == value.email)
        //         .ToListAsync();
        //
        //     return reports;
        // }
        public async Task<List<ReportModel>> GetReportData([FromBody] ReportModelReq value)
        {
            try
            {
                _logger.LogInformation("Fetching report data for email: {Email}", value?.email);
                // Validate input
                if (value == null || string.IsNullOrWhiteSpace(value.email))
                {
                    _logger.LogError("Email is required for fetching report data.");
                    throw new ArgumentException("Email is required for fetching report data.");
                }

                // Transform email for S3 file path
                var filePath = $"Reports/{_serviceHelper.Value.TransformEmail(value.email)}_report.json";

                // Check if the report file exists
                if (await AwsS3.S3FileIsExists("Authorization", filePath))
                {
                    var fileContent = AwsS3.GetS3File("Authorization", filePath);
                    var reports = JsonConvert.DeserializeObject<List<ReportModel>>(fileContent);
                    _logger.LogInformation("Successfully fetched report data for email: {Email}", value.email);
                    return reports ?? new List<ReportModel>(); // Return an empty list if deserialization yields null
                }
                else
                {
                    _logger.LogWarning("Report file not found for email: {Email}", value.email);
                    return new List<ReportModel>(); // File not found, return an empty list
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching report data for email: {Email}", value?.email);
                // Log the error (replace with ILogger for production)
                Console.WriteLine($"Error fetching report data: {ex.Message}");
                return new List<ReportModel>(); // Return an empty list on error
            }
        }
    }
}
