using AutoMapper;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.Users.Models;
using Rishvi.Modules.Users.Models.DTOs;

namespace Rishvi.Modules.Users.Mappers
{
    public class UserAdminMappingProfile : Profile
    {

        public UserAdminMappingProfile()
        {
            //Create
            CreateMap<UserAdminCreateDto, User>();
            //Edit
            CreateMap<UserAdminEditDto, User>();
            CreateMap<User, UserAdminEditDto>();
            CreateMap<User, UserAdminEmailDto>();
            CreateMap<UserAdminEmailDto, User>();

        }
    }

}
