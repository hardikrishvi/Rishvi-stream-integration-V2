using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core.Encryption;

namespace Rishvi.Modules.AdminUsers.Admin.Models.Mappers
{
    public class AdminUserMappingProfile : Profile
    {
        public AdminUserMappingProfile()
        {
            //Create
            CreateMap<AdminUserCreateDto, AdminUser>()
              .AfterMap((dto, entity) =>
              {
                  entity.Salt = SecurityHelper.GenerateSalt();
                  entity.Password = SecurityHelper.GenerateHash(dto.Password, entity.Salt);
                  entity.AdminUsersAdminRoles = new List<AdminUsersAdminRoles>();
              });

            //Edit
            CreateMap<AdminUserEditDto, AdminUser>()
              .ForMember(m => m.Password, opt => opt.Ignore())
              .AfterMap((dto, entity) =>
              {
                  if (string.IsNullOrEmpty(dto.Password))
                  {
                      return;
                  }

                  entity.Salt = SecurityHelper.GenerateSalt();
                  entity.Password = SecurityHelper.GenerateHash(dto.Password, entity.Salt);
              });

            CreateMap<AdminUser, AdminUserEditDto>()
              .ForMember(m => m.Password, opt => opt.Ignore())
              .ForMember(m => m.Roles, opt => opt.MapFrom(v => v.AdminUsersAdminRoles.Select(s => s.AdminRoleId).ToList()));

            //Edit Profile
            CreateMap<AdminEditProfileDto, AdminUser>();
            //.ForMember(m => m.Id, opt => opt.Ignore());

            CreateMap<AdminUser, AdminEditProfileDto>();
                //.ForMember(m => m.Id, opt => opt.Ignore());

        }

    }
}
