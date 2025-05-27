using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Rishvi.Modules.Core.Authorization;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.CronJob;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi_Vault;

namespace Rishvi
{
    public static class ServiceCollectionExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration Configuration)
        {
            //var connection = AWSParameter.GetConnectionString(Configuration.GetConnectionString("Connection"));
            //DapperRepository.ConnectionString = AWSParameter.GetConnectionString(Configuration.GetConnectionString("Connection"));
            var connection = Configuration.GetConnectionString("Connection");
            DapperRepository.ConnectionString = Configuration.GetConnectionString("Connection");

            //add-migration "init"
            _ = services.AddDbContextPool<SqlContext>(options =>
                  options.UseSqlServer(connection, optionsBuilder => optionsBuilder.MigrationsAssembly("Rishvi")));

            _ = services.Configure<JwtSetting>(Configuration.GetSection("JwtSetting"));
            _ = services.Configure<RecaptchaSetting>(Configuration.GetSection("RecaptchaSetting"));
            _ = services.Configure<List<SchedulerJobSettings>>(Configuration.GetSection("SchedulerJobSettings"));
            _ = services.Configure<FtpSetting>(Configuration.GetSection("FtpSetting"));
            _ = services.Configure<LinnworkAppCredential>(Configuration.GetSection("LinnworkAppCredential"));
            _ = services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            _ = services.Configure<CourierSettings>(Configuration.GetSection("CourierSettings"));   
            _ = services.Configure<ServiceHelperSettings>(Configuration.GetSection("ServiceHelperSettings"));
            _ = services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            _ = services.Configure<MessinaSettings>(Configuration.GetSection("MessinaSettings"));
            _ = services.Configure<StreamApiSettings>(Configuration.GetSection("StreamApiSettings"));

            _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSetting:SecretKey"]))
            });

            //_ = services.AddHangfire(config =>
            //   config.UseSqlServerStorage(Configuration.GetConnectionString("Connection")));
            
            //Below setting for caching
            _ = services.AddMvc(option => option.EnableEndpointRouting = false).AddNewtonsoftJson();
            _ = services.AddControllers();//.AddNewtonsoftJson();
            //_ = services.AddControllersWithViews();
            _ = services.AddEndpointsApiExplorer();

            _ = services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                                             .AllowAnyMethod()
                                                              .AllowAnyHeader()));

            _ = services.AddAutoMapper(typeof(ServiceCollectionExtension));

            _ = services.AddSwaggerGen(options =>
            {
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                options.IgnoreObsoleteActions();
                options.IgnoreObsoleteProperties();
                options.CustomSchemaIds(type => type.FullName);

                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Rishvi API Services",
                    Version = "v1",
                    Description = "Serverless Framework Api",
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
            });

            _ = services.AddMemoryCache();
        }

        //public static void ConfigureContainer(this ContainerBuilder builder) =>
        //   // Register your own things directly with Autofac, like:
        //   _ = builder.RegisterModule(new RepositoryHandlerModule());
    }
}
