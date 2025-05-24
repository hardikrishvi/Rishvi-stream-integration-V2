using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.AdminUsers.Admin.CacheManagers;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Admin.Validators;
using Rishvi.Modules.AdminUsers.Filters;
using Rishvi.Modules.AdminUsers.ListOrders;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.Core.Encryption;
using Rishvi.Modules.Core.Extensions;
using Rishvi.Modules.Core.Validators;
using Z.EntityFramework.Plus;

namespace Rishvi.Modules.AdminUsers.Admin.Services
{
    public interface IAdminUserService
    {
        Task<Result> ListAsync(AdminUserFilterDto dto);

        Task<Result> ByIdAsync(Guid id);

        Task<Result> CreateAsync(AdminUserCreateDto dto);

        Task<Result> EditAsync(AdminUserEditDto dto);

        Task<Result> ActiveAsync(IdsDto dto);

        Task<Result> InActiveAsync(IdsDto dto);

        Task<Result> DeleteAsync(IdsDto dto);

        Task<Result> GetEditProfileAsync(Guid id);

        Task<Result> SaveEditProfileAsync(AdminEditProfileDto dto);

        Task<Result> GetChangePasswordAsync(Guid id);

        Task<Result> SaveChangePasswordAsync(AdminChangePasswordDto dto);
    }

    public class AdminUserService : IAdminUserService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<AdminUser> _adminUserRepository;
        private readonly IRepository<AdminUsersAdminRoles> _adminUsersAdminRolesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<AdminRole> _adminRoleRepository;
        private readonly AdminUserCreateValidator _adminUserCreateValidator;
        private readonly AdminUserEditValidator _adminUserEditValidator;
        private readonly AdminEditProfileValidator _adminEditProfileValidator;
        private readonly AdminChangePasswordValidator _adminChangePasswordValidator;
        private readonly AdminUserActiveValidator _adminUserActiveValidator;
        private readonly AdminUserDeleteValidator _adminUserDeleteValidator;
        private readonly IRepository<AdminRolesAdminPermissions> _adminRolesAdminPermissionsRepository;

        public AdminUserService(
        IRepository<AdminUser> adminUserRepository,
        IRepository<AdminRole> adminRoleRepository,
        IRepository<AdminUsersAdminRoles> adminUsersAdminRolesRepository,
        IRepository<AdminRolesAdminPermissions> adminRolesAdminPermissionsRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        AdminUserCreateValidator adminUserCreateValidator,
        AdminUserEditValidator adminUserEditValidator,
        AdminEditProfileValidator adminEditProfileValidator,
        AdminChangePasswordValidator adminChangePasswordValidator,
        AdminUserActiveValidator adminUserActiveValidator,
        AdminUserDeleteValidator adminUserDeleteValidator)
        {
            _adminUserRepository = adminUserRepository;
            _adminRoleRepository = adminRoleRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _adminUsersAdminRolesRepository = adminUsersAdminRolesRepository;
            _adminRolesAdminPermissionsRepository = adminRolesAdminPermissionsRepository;
            _adminUserCreateValidator = adminUserCreateValidator;
            _adminUserEditValidator = adminUserEditValidator;
            _adminEditProfileValidator = adminEditProfileValidator;
            _adminChangePasswordValidator = adminChangePasswordValidator;
            _adminUserActiveValidator = adminUserActiveValidator;
            _adminUserDeleteValidator = adminUserDeleteValidator;
        }

        public async Task<Result> ListAsync(AdminUserFilterDto dto)
        {
            var filter = dto ?? new AdminUserFilterDto();

            var query = _adminUserRepository.AsNoTracking()
               .Include(i => i.AdminUsersAdminRoles).ThenInclude(i => i.AdminRole)
               .Select(t => new AdminUser
               {
                   Id = t.Id,
                   Name = t.Name,
                   Email = t.Email,
                   CreatedAt = t.CreatedAt,
                   UpdatedAt = t.UpdatedAt,
                   LastLoginAt = t.LastLoginAt,
                   IsActive = t.IsActive,
                   AdminUsersAdminRoles = t.AdminUsersAdminRoles
               });

            query = new AdminUserFilter(query, filter).FilteredQuery();
            query = new AdminUserListOrder(query, filter).OrderByQuery();

            var result = await new Result().SetPagingAsync(filter, query.Count());

            result.Data = query.ToPaged(result.Paging.Page, result.Paging.Size).ToList()
                .Select(s => new AdminUserListDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    LastLoginAt = s.LastLoginAt,
                    IsActive = s.IsActive,
                    Roles = s.AdminUsersAdminRoles.Select(t => new IdNameDto() { Id = t.AdminRole.Id, Name = t.AdminRole.Name }).ToList()
                });

            return await Task.FromResult(result);

            //return await new Result().SetDataAsync(data);
        }

        public async Task<Result> ByIdAsync(Guid id)
        {

            var entity = new AdminUser();
            var adminUserEditDto = new AdminUserEditDto();

            if (id == Guid.Empty)
            {
                return await new Result().SetDataAsync(adminUserEditDto);
            }

            //int UserId = 1;
            //var adminUserRole = await _adminUsersAdminRolesRepository.AsNoTracking()
            //    .Where(a => a.AdminUserId == UserId).FirstOrDefaultAsync();
            //var userRoleId = adminUserRole.AdminRoleId;
            //if (userRoleId != 1)
            //{

            entity = await _adminUserRepository.AsNoTracking()
                .Include(i => i.AdminUsersAdminRoles).ThenInclude(i => i.AdminRole)
                .FirstOrDefaultAsync(s => s.Id == id);// && !s.AdminUsersAdminRoles.Any(r => r.AdminRoleId == 1));

            //}
            //else
            //{
            //    entity = await _adminUserRepository.AsNoTracking()
            //            .Include(i => i.AdminUsersAdminRoles).ThenInclude(i => i.AdminRole)
            //            .Where(w => w.Id == id).FirstOrDefaultAsync();

            //}

            adminUserEditDto = _mapper.Map<AdminUserEditDto>(entity);
            return await new Result().SetDataAsync(adminUserEditDto);
        }

        public async Task<Result> CreateAsync(AdminUserCreateDto dto)
        {
            var result = await _adminUserCreateValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = _mapper.Map<AdminUser>(dto);
            await _adminUserRepository.AddAsync(entity);

            foreach (var roleId in dto.Roles)
            {
                entity.AdminUsersAdminRoles.Add(new AdminUsersAdminRoles
                {
                    AdminRole = await _adminRoleRepository.GetByIdAsync(roleId)
                });
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            await result.SetIdAsync(entity.Id);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        public async Task<Result> EditAsync(AdminUserEditDto dto)
        {
            dto ??= new AdminUserEditDto();

            var result = await _adminUserEditValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _adminUserRepository.AsNoTracking()
                .Include("AdminUsersAdminRoles")
                .FirstAsync(s => s.Id == dto.Id);
            entity.AccountLockedOn = null;
            entity.WrongPasswordAttempt = 0;
            _mapper.Map(dto, entity);
            await _adminUserRepository.UpdateAsync(entity);
            await ChildRoleUpdateAsync(entity, dto);

            await _unitOfWork.CommitAsync();
            ClearCache();

            await result.SetIdAsync(entity.Id);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        private async Task ChildRoleUpdateAsync(AdminUser entity, AdminUserEditDto dto)
        {
            var currentRecords = entity.AdminUsersAdminRoles.Select(s => s.AdminRoleId).ToList();

            var addedRecords = dto.Roles.Except(currentRecords).ToList();
            foreach (var record in addedRecords)
            {
                entity.AdminUsersAdminRoles.Add(
                    new AdminUsersAdminRoles()
                    {
                        AdminRole = await _adminRoleRepository.GetByIdAsync(record)
                    });
            }

            var deletedRecords = currentRecords.Except(dto.Roles).ToList();
            foreach (var record in deletedRecords)
            {
                entity.AdminUsersAdminRoles.Remove(entity.AdminUsersAdminRoles.First(w => w.AdminRoleId == record));
            }
        }

        public async Task<Result> ActiveAsync(IdsDto dto)
        {
            var result = await _adminUserActiveValidator.ValidateResultAsync(dto?.Ids);
            if (!result.Success)
            {
                return result;
            }

            var query = _adminUserRepository.AsNoTracking()
              .Where(q => dto.Ids.Contains(q.Id));

            foreach (var entity in query)
            {
                entity.IsActive = true;
                await _adminUserRepository.UpdateAsync(entity);
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            return await new Result().SetSuccessAsync(Messages.RecordActivate, await query.CountAsync());
        }

        public async Task<Result> InActiveAsync(IdsDto dto)
        {
            var result = await _adminUserActiveValidator.ValidateResultAsync(dto?.Ids);
            if (!result.Success)
            {
                return result;
            }

            var query = _adminUserRepository.AsNoTracking()
              .Where(q => dto.Ids.Contains(q.Id));

            foreach (var entity in query)
            {
                entity.IsActive = false;
                await _adminUserRepository.UpdateAsync(entity);
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            return await new Result().SetSuccessAsync(Messages.RecordActivate, await query.CountAsync());
        }

        public async Task<Result> DeleteAsync(IdsDto dto)
        {
            if (dto == null || dto.Ids.Count == 0)
            {
                return await new Result().SetErrorAsync(Messages.SelectAtLeastOneItemFromList);
            }

            var result = await _adminUserDeleteValidator.ValidateResultAsync(dto?.Ids);
            if (!result.Success)
            {
                return result;
            }

            foreach (var id in dto.Ids)
            {
                await _adminUserRepository.DeleteAsync(id);
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            return await new Result().SetSuccessAsync(Messages.RecordDelete, dto.Ids.Count);
        }

        public async Task<Result> GetEditProfileAsync(Guid id)
        {
            var profile = await _adminUserRepository.AsNoTracking()
                    .Where(w => w.Id == id)
                    .ProjectTo<AdminEditProfileDto>(new MapperConfiguration(mcg => mcg.CreateMap<AdminUser, AdminEditProfileDto>()))
                    .FirstOrDefaultAsync();

            return await new Result().SetDataAsync(profile);
        }
        public async Task<Result> SaveEditProfileAsync(AdminEditProfileDto dto)
        {
            dto ??= new AdminEditProfileDto();

            var result = await _adminEditProfileValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _adminUserRepository.AsNoTracking()
                         .FirstAsync(s => s.Id == dto.Id);

            _mapper.Map(dto, entity);
            await _adminUserRepository.UpdateAsync(entity);

            await _unitOfWork.CommitAsync();
            ClearCache();

            await result.SetIdAsync(entity.Id);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        public async Task<Result> GetChangePasswordAsync(Guid id)
        {
            return await new Result().SetDataAsync(new AdminChangePasswordDto()
            {
                Id = id
            });
        }
        public async Task<Result> SaveChangePasswordAsync(AdminChangePasswordDto dto)
        {
            dto ??= new AdminChangePasswordDto();

            var result = await _adminChangePasswordValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _adminUserRepository.GetByIdAsync(dto.Id);

            var salt = SecurityHelper.GenerateSalt();

            entity.Salt = salt;
            entity.Password = SecurityHelper.GenerateHash(dto.NewPassword, salt);

            await _adminUserRepository.UpdateAsync(entity);

            await _unitOfWork.CommitAsync();
            ClearCache();

            await result.SetIdAsync(entity.Id);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        private static void ClearCache()
        {
            AdminUsersCacheManager.ClearCache();
        }

        public async Task<Result> GetPermissionsAsync(Guid userId)
        {
            var roleIds = await _adminUsersAdminRolesRepository.AsNoTracking()
                .Where(w => w.AdminUserId == userId)
                .Select(s => s.AdminRoleId)
                .ToListAsync();

            var permissions = await _adminRolesAdminPermissionsRepository.AsNoTracking()
                .Include(i => i.AdminPermission)
                .Where(w => roleIds.Contains(w.AdminRoleId))
                .OrderBy(o => o.AdminPermission.Left)
                .Select(s => s.AdminPermission.Name)
                .ToListAsync();

            return await new Result().SetDataAsync(permissions);
        }
    }
}
