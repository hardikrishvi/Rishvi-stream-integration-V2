using AutoMapper;
using Rishvi.Domain.Mapper;

namespace Rishvi.Dependencies
{
    public class AutoMapperConfiguration
    {
        public static void ConfigureAutoMapper(IServiceCollection services)
        {
            services.AddTransient<IMapper>(sp =>
            {
                var configuration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<StreamIntegrationMapping>();
                });

                return configuration.CreateMapper();
            });
        }
    }
}
