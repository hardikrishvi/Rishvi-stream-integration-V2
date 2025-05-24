using Rishvi.Modules.Users.Models;
using Rishvi.Modules.Users.Models.DTOs;
using AutoMapper;
using System.Collections.Generic;

namespace Rishvi.Modules.Users.Mappers
{
    public class UserWiseFtpMappingProfile : Profile
    {
        public UserWiseFtpMappingProfile()
        {
            CreateMap<UserWiseFtpUpdateDto, UserWiseFtp>().ForMember(dest => dest.UserWiseFtpId, o => o.Ignore())
;
            CreateMap<UserWiseFtp, UserWiseFtpUpdateDto>();
        }
    }
}
