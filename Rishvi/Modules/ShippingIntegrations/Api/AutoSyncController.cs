namespace SyncApiController.Services
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SyncBackgroundService> _logger;
        private readonly string _apiUrl; // Your API endpoint

        public SyncBackgroundService(HttpClient httpClient, ILogger<SyncBackgroundService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiUrl = configuration["ApiSettings:SyncApiUrl"];
        }

        // Execute every 2 hours and handle cancellation token
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromHours(2)); // Run every 2 hours

            try
            {
                while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await SyncApiCall(stoppingToken); // Perform your sync operation
                }
            }
            catch (OperationCanceledException)
            {
                // Handle application shutdown or cancellation
                Console.WriteLine("Periodic sync operation canceled.");
            }
            finally
            {
                periodicTimer.Dispose(); // Ensure proper cleanup
            }
        }

        // This method now accepts a CancellationToken parameter
        public async Task SyncApiCall(CancellationToken cancellationToken)
        {
            try
            {
                // If cancellation is requested, exit early
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Sync operation canceled.");
                    return;
                }

                // Call the API endpoint (e.g., GET request)
                var response = await _httpClient.GetAsync(_apiUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Sync successful: {Content}", await response.Content.ReadAsStringAsync());
                }
                else
                {
                    _logger.LogWarning("Sync failed with status code: {StatusCode}", response.StatusCode);
                }

            }
            catch (OperationCanceledException)
            {
                // Handle task cancellation
                _logger.LogInformation("Sync operation canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync operation.");
            }
        }
    }
}
