using AutoMapper;
using Rishvi.Modules.ErrorLogs.Models;
using Rishvi.Modules.ErrorLogs.Models.DTOs;
namespace Rishvi.Modules.ErrorLogs.Mappers
{
    public class SystemLogMappingProfile : Profile
    {
        public SystemLogMappingProfile()
        {
            CreateMap<SystemLog, SystemLogListDto>();
        }
    }
}
