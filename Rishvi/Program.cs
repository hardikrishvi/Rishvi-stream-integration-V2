using Autofac;
using Autofac.Extensions.DependencyInjection;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Helpers;
using Rishvi;
using SyncApiController.Services;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;
using Rishvi_Vault;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Aws;
using Rishvi.Modules.ShippingIntegrations.Core.Helper;
using Rishvi.Modules.ShippingIntegrations.Core;
using Rishvi.Modules.ShippingIntegrations.Api;
using Hangfire;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Dependencies;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Rishvi.Modules.ShippingIntegrations.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Store configuration reference early
var config = builder.Configuration;

#region Serilog Configuration
var elasticUri = builder.Configuration["ElasticsearchSettings:Uri"];    
var applicationName = builder.Configuration["ElasticsearchSettings:ApplicationName"];
var env = builder.Environment.EnvironmentName;

var indexFormat = $"{applicationName?.ToLower()}-{env?.ToLower()}-{DateTime.UtcNow:yyyy.MM}";
var elasticOptions = new ElasticsearchSinkOptions(new Uri(elasticUri!))
{
    IndexFormat = indexFormat,
    BatchPostingLimit = 50,
    Period = TimeSpan.FromSeconds(5),
    InlineFields = true,
};

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithCorrelationId()
        .Enrich.WithCorrelationIdHeader("X-Correlation-ID")
        .Enrich.WithEnvironmentName()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(elasticOptions);
});
#endregion

// Register services
builder.Services.AddTransient<ServiceHelper>();
builder.Services.AddScoped<AwsS3>();
builder.Services.AddTransient<ConfigController>();

builder.Services.AddDbContext<SqlContext>(options =>
{
    options.UseSqlServer(config.GetConnectionString("Connection")); // Use your DB provider
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(config.GetConnectionString("Connection")); // Use your DB provider
});

// AWS Lambda hosting
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Custom services
builder.Services.ConfigureServices(config);

// Register Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Hosted background service
builder.Services.AddHostedService<SyncBackgroundService>();

// Autofac container modules
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new RepositoryHandlerModule());
    containerBuilder.RegisterType<TradingApiOAuthHelper>().AsSelf();
    containerBuilder.RegisterType<ReportsController>().PropertiesAutowired();
    containerBuilder.RegisterType<SetupController>().AsSelf();
    containerBuilder.RegisterType<ServiceHelper>().AsSelf();
    containerBuilder.RegisterType<ServiceHelper>().As<IServiceHelper>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AuthorizationConfig>().As<IAuthorizationToken>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ConfigController>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<LinnworksController>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<StreamController>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ManageToken>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ApplicationDbContext>()
       .AsSelf()
       .InstancePerLifetimeScope();

    containerBuilder.RegisterGeneric(typeof(Repository<>))
           .As(typeof(IRepository<>))
           .InstancePerLifetimeScope();
});

// Safely register Hangfire
var hangfireConnectionString = config.GetConnectionString("Connection");
builder.Services.AddHangfire(configuration =>
{
    configuration.UseSqlServerStorage(hangfireConnectionString);
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 5;
});

AutoMapperConfiguration.ConfigureAutoMapper(builder.Services);

// HttpClient registration
builder.Services.AddHttpClient();

// Build and configure app
var app = builder.Build();

// Setup AppSettings
AppSettings.AppSettingsConfiguration(app.Services.GetRequiredService<IConfiguration>());
CourierSettings.CourierSettingsConfiguration(app.Services.GetRequiredService<IConfiguration>());
StreamApiSettings.StreamApiSettingsConfiguration(app.Services.GetRequiredService<IConfiguration>());
ApplicationSettings.ApplicationSettingsConfiguration(app.Services.GetRequiredService<IConfiguration>());

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequstResponseLoggingMiddleware>();

// Configure middleware
app.Configure(builder.Environment);

app.UseHangfireDashboard("/hangfire");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Run the app
app.Run();
