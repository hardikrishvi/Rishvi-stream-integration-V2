using AutoMapper;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Models;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.Mappers
{
    public class AdminPermissionMappingProfile : Profile
    {
        public AdminPermissionMappingProfile()
        {
            // Create
            CreateMap<AdminPermissionDto, AdminPermission>()
              .AfterMap((dto, entity) => entity.ParentId = dto.IsParentSelected == true ? dto.ParentId : null);

            // Edit
            CreateMap<AdminPermission, AdminPermissionDto>()
              .AfterMap((dto, entity) => entity.IsParentSelected = dto.ParentId != null);
        }
    }
}
