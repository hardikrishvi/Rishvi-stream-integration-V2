using System.Linq;
using AutoMapper;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.Mappers
{
    public class AdminRoleMappingProfile : Profile
    {
        public AdminRoleMappingProfile()
        {
            // Create
            CreateMap<AdminRoleCreateDto, AdminRole>();

            // Edit
            CreateMap<AdminRoleEditDto, AdminRole>();

            //CreateMap<AdminRole, AdminRoleEditDto>()
            //  .ForMember(m => m.Permissions, opt => opt.MapFrom(v => v.AdminRolesAdminPermissionses.Select(s => new IdNameDto() { Id = s.AdminPermission.Id, Name = s.AdminPermission.Name }).ToList()))
            //  .ForMember(m => m.PermissionIds, opt => opt.MapFrom(v => v.AdminRolesAdminPermissionses.Select(s => s.AdminPermission.Id).ToList()));
        }
    }
}
