using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminRolePermissions.Admin.CacheManagers;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Admin.Validators;
using Rishvi.Modules.AdminRolePermissions.Filters;
using Rishvi.Modules.AdminRolePermissions.ListOrders;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.Core.Extensions;
using Rishvi.Modules.Core.Validators;
using Z.EntityFramework.Plus;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Services
{
    public interface IAdminRoleService
    {
        Task<Result> ListAsync(AdminRoleFilterDto dto);
        Task<Result> DeleteAsync(IList<Guid> ids);
        Task<Result> CreateAsync(AdminRoleCreateDto dto);
        Task<Result> ByIdAsync(Guid id);
        Task<Result> EditAsync(AdminRoleEditDto dto);

        Task<Result> GetRolesAsync();
    }

    public class AdminRoleService : IAdminRoleService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<AdminRole> _adminRoleRepository;
        private readonly AdminRoleCreateValidator _adminRoleCreateValidator;
        private readonly AdminRoleEditValidator _adminRoleEditValidator;
        private readonly IRepository<AdminPermission> _adminPermissionRepository;
        private readonly IRepository<AdminRolesAdminPermissions> _adminRolesAdminPermissionsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminRoleService(
            IMapper mapper,
            IRepository<AdminRole> adminRoleRepository,
            AdminRoleCreateValidator adminRoleCreateValidator,
            AdminRoleEditValidator adminRoleEditValidator,
            IRepository<AdminPermission> adminPermissionRepository,
            IRepository<AdminRolesAdminPermissions> adminRolesAdminPermissionsRepository,
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _adminRoleRepository = adminRoleRepository;
            _adminRoleCreateValidator = adminRoleCreateValidator;
            _adminRoleEditValidator = adminRoleEditValidator;
            _adminPermissionRepository = adminPermissionRepository;
            _adminRolesAdminPermissionsRepository = adminRolesAdminPermissionsRepository;
            _unitOfWork = unitOfWork;

        }

        public async Task<Result> ListAsync(AdminRoleFilterDto dto)
        {
            var filter = dto ?? new AdminRoleFilterDto();

            var query = _adminRoleRepository.AsNoTracking()
              .Include(i => i.AdminRolesAdminPermissionses).ThenInclude(i => i.AdminPermission)
              .Select(t => new AdminRole
              {
                  Id = t.Id,
                  Name = t.Name,
                  AdminRolesAdminPermissionses = t.AdminRolesAdminPermissionses
              });

            query = new AdminRoleFilter(query, filter).FilteredQuery();
            query = new AdminRoleListOrder(query, filter).OrderByQuery();

            var result = await new Result().SetPagingAsync(filter, query.Count());

            result.Data = query.ToPaged(result.Paging.Page, result.Paging.Size).ToList()
                .Select(s => new AdminRoleListDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Permissions = s.AdminRolesAdminPermissionses.Select(t => new IdNameDto() { Id = t.AdminPermission.Id, Name = t.AdminPermission.Name }).ToList()
                });

            return await Task.FromResult(result);
        }

        public async Task<Result> ByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return await new Result().SetDataAsync(new AdminRoleEditDto());
            }

            var entity = await _adminRoleRepository.AsNoTracking()
                .Include(i => i.AdminRolesAdminPermissionses).ThenInclude(i => i.AdminPermission)
                .Where(w => w.Id == id)
                .Select(t => new AdminRoleEditDto() { 
                    Id = t.Id,
                    Name = t.Name,
                    PermissionIds = t.AdminRolesAdminPermissionses.Select(s => s.AdminPermission.Id).ToList()
                }).FirstOrDefaultAsync();

            if (entity != null)
            {
                entity.AllPermissions = await _adminPermissionRepository.AsNoTracking()
                .OrderBy(o => o.Left)
                //.Include(i => i.Children)
                //.Where(w => w.ParentId == null)
                .Select(s => new AdminPermissionDropDownDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < s.Left && w.Right > s.Right),
                    Left = s.Left,
                    Right = s.Right,
                    ParentId = s.ParentId
                    //Children = s.Children.Select(s => new PermissionChildItemDto() {
                    //    Id = s.Id,
                    //    Name = s.Name,
                    //    DisplayName = s.DisplayName,
                    //    Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < s.Left && w.Right > s.Right),
                    //    Left = s.Left,
                    //    Right = s.Right,
                    //    ParentId = s.ParentId
                    //}).ToList()
                }).ToListAsync();
            }

            //adminRoleEditDto = _mapper.Map<AdminRoleEditDto>(entity);

            return await new Result().SetDataAsync(entity);
        }

        public async Task<Result> CreateAsync(AdminRoleCreateDto dto)
        {
            var result = await _adminRoleCreateValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = _mapper.Map<AdminRole>(dto);
            entity.SystemName = await GenerateUniqueSlugAsync(entity.Name, slugFieldName: "SystemName");
            entity.AdminRolesAdminPermissionses = new List<AdminRolesAdminPermissions>();
            await _adminRoleRepository.AddAsync(entity);

            foreach (var roleId in dto.Permissions)
            {
                entity.AdminRolesAdminPermissionses.Add(new AdminRolesAdminPermissions()
                {
                    AdminPermission = await _adminPermissionRepository.GetByIdAsync(roleId)
                });
            }

            await _unitOfWork.CommitAsync();
            AdminRoleCacheManager.ClearCache();

            result.Id = entity.Id;
            return await result.SetSuccessAsync(Messages.RecordSaved);
        }

        public async Task<string> GenerateUniqueSlugAsync(string phrase, Guid? id = null, string slugFieldName = "Slug")
        {
            int? loop = null;
            var slug = phrase.GenerateSlug();
            var query = _adminRoleRepository.AsNoTracking().Where(x => x.SystemName == slug).AsQueryable();
            if (id != null)
            {
                query = query.Where(x => x.Id != id).AsQueryable();
            }

            while (await query.AnyAsync())
            {
                loop = loop == null ? 1 : loop + 1;
                slug = phrase.GenerateSlug() + "-" + loop;
            }

            return slug;
        }

        public async Task<Result> DeleteAsync(IList<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new Result().SetError(Messages.SelectAtLeastOneItemFromList);
            }

            var query = _adminRoleRepository.AsNoTracking()
                .Include(i => i.AdminUsersAdminRoles)
                .Where(q => ids.Contains(q.Id));

            if (query.Any(w => w.AdminUsersAdminRoles.Any()))
            {
                return new Result().SetError("You can't delete any record(s) which are assigned to any other record(s).");
            }

            var result = new Result().SetSuccess(Messages.RecordDelete, query.Count());
            await _adminRolesAdminPermissionsRepository.AsNoTracking().Where(w => ids.Contains(w.AdminRoleId)).DeleteAsync();

            await query.DeleteAsync();
            AdminRoleCacheManager.ClearCache();

            return result;
        }

        public async Task<Result> EditAsync(AdminRoleEditDto dto)
        {
            var result = await _adminRoleEditValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _adminRoleRepository.AsNoTracking().Include(i => i.AdminRolesAdminPermissionses)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (entity == null)
            {
                return null;
            }

            _mapper.Map(dto, entity);
            await _adminRoleRepository.UpdateAsync(entity);
            await ChildPermissionUpdateAsync(entity, dto);

            await _unitOfWork.CommitAsync();
            AdminRoleCacheManager.ClearCache();

            await result.SetIdAsync(entity.Id);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        private async Task ChildPermissionUpdateAsync(AdminRole entity, AdminRoleEditDto dto)
        {
            var currentRecords = entity.AdminRolesAdminPermissionses.Select(s => s.AdminPermissionId).ToList();

            var addedRecords = dto.PermissionIds.Except(currentRecords).ToList();

            foreach (var record in addedRecords)
            {
                entity.AdminRolesAdminPermissionses.Add(
                    new AdminRolesAdminPermissions()
                    {
                        AdminPermission = await _adminPermissionRepository.GetByIdAsync(record)
                    });
            }

            var deletedRecords = currentRecords.Except(dto.PermissionIds).ToList();

            foreach (var record in deletedRecords)
            {
                entity.AdminRolesAdminPermissionses.Remove(entity.AdminRolesAdminPermissionses.First(w => w.AdminPermissionId == record));
            }
        }

        public async Task<Result> GetRolesAsync()
        {
            var data = await _adminRoleRepository.AsNoTracking()
              .Select(t => new IdNameDto
              {
                  Id = t.Id,
                  Name = t.Name,
              }).ToListAsync();
            return await new Result().SetDataAsync(data);
        }
    }
    //public interface IAdminUserService
    //{
    //  IQueryable<AdminUserListDto> List();

    //  Result ById(int id);

    //  Result Create(AdminUserCreateDto dto);

    //  Result Edit(int id, AdminUserEditDto dto);

    //  Result Active(IdsDto dto);

    //  Result InActive(IdsDto dto);

    //  Result Delete(IdsDto dto);
    //}

    //public class AdminRoleService : IAdminUserService
    //{
    //  private readonly IMapper _mapper;
    //  private readonly IRepository<AdminPermission> _adminUserRepository;
    //  private readonly IUnitOfWork _unitOfWork;

    //  private readonly AdminUserCreateValidator _adminUserCreateValidator;
    //  private readonly AdminUserEditValidator _adminUserEditValidator;
    //  private readonly AdminUserActiveValidator _adminUserActiveValidator;
    //  private readonly AdminUserInActiveValidator _adminUserInActiveValidator;
    //  private readonly AdminUserDeleteValidator _adminUserDeleteValidator;

    //  public AdminRoleService(
    //      IRepository<AdminPermission> adminUserRepository,
    //      IMapper mapper,
    //      IUnitOfWork unitOfWork,

    //      AdminUserCreateValidator adminUserCreateValidator,
    //      AdminUserEditValidator adminUserEditValidator,
    //      AdminUserActiveValidator adminUserActiveValidator,
    //      AdminUserInActiveValidator adminUserInActiveValidator,
    //      AdminUserDeleteValidator adminUserDeleteValidator)
    //  {
    //    _adminUserRepository = adminUserRepository;
    //    _mapper = mapper;
    //    _unitOfWork = unitOfWork;

    //    _adminUserCreateValidator = adminUserCreateValidator;
    //    _adminUserEditValidator = adminUserEditValidator;
    //    _adminUserActiveValidator = adminUserActiveValidator;
    //    _adminUserInActiveValidator = adminUserInActiveValidator;
    //    _adminUserDeleteValidator = adminUserDeleteValidator;
    //  }

    //  public IQueryable<AdminUserListDto> List()
    //  {
    //    return _adminUserRepository.AsNoTracking()
    //       .Select(t => new AdminUserListDto
    //       {
    //         Id = t.Id,
    //         Name = t.Name,
    //         Email = t.Email,
    //         CreatedAt = t.CreatedAt,
    //         UpdatedAt = t.UpdatedAt,
    //         LastLoginAt = t.LastLoginAt,
    //         IsActive = t.IsActive
    //       });
    //  }

    //  public Result ById(int id)
    //  {
    //    return new Result().SetData(id <= 0
    //        ? new AdminUserEditDto() { IsActive = true }
    //        : _adminUserRepository.AsNoTracking()
    //            .Where(w => w.Id == id)
    //            .ProjectTo<AdminUserEditDto>(new MapperConfiguration(mcg => mcg.CreateMap<AdminPermission, AdminUserEditDto>()))
    //            .FirstOrDefault());
    //  }

    //  public Result Create(AdminUserCreateDto dto)
    //  {
    //    var result = _adminUserCreateValidator.ValidateResult(dto);
    //    if (!result.Success) return result;

    //    var entity = _mapper.Map<AdminPermission>(dto);
    //    _adminUserRepository.Add(entity);

    //    //foreach (var roleId in dto.Roles)
    //    //  entity.AdminUsersAdminRoles.Add(new AdminUsersAdminRoles
    //    //  {
    //    //    AdminRole = _adminRoleRepository.Find(roleId)
    //    //  });

    //    _unitOfWork.Commit();
    //    ClearCache();

    //    return result.SetId(entity.Id).SetSuccess(Messages.RecordSaved);
    //  }

    //  public Result Edit(int id, AdminUserEditDto dto)
    //  {
    //    dto ??= new AdminUserEditDto();
    //    dto.Id = id;

    //    var result = _adminUserEditValidator.ValidateResult(dto);
    //    if (!result.Success) return result;

    //    var entity = _adminUserRepository.AsNoTracking()
    //        .First(s => s.Id == dto.Id);

    //    _mapper.Map(dto, entity);
    //    _adminUserRepository.Update(entity);

    //    _unitOfWork.Commit();
    //    ClearCache();

    //    return result.SetId(entity.Id).SetSuccess(Messages.RecordSaved);
    //  }

    //  public Result Active(IdsDto dto)
    //  {
    //    var result = _adminUserActiveValidator.ValidateResult(dto?.Ids);
    //    if (!result.Success) return result;

    //    var query = _adminUserRepository.AsNoTracking()
    //      .Where(q => dto.Ids.Contains(q.Id));

    //    foreach (var entity in query)
    //    {
    //      entity.IsActive = true;
    //      _adminUserRepository.Update(entity);
    //    }

    //    _unitOfWork.Commit();
    //    ClearCache();

    //    return new Result().SetSuccess(Messages.RecordActivate, query.Count());
    //  }

    //  public Result InActive(IdsDto dto)
    //  {
    //    var result = _adminUserActiveValidator.ValidateResult(dto?.Ids);
    //    if (!result.Success) return result;

    //    var query = _adminUserRepository.AsNoTracking()
    //      .Where(q => dto.Ids.Contains(q.Id));

    //    foreach (var entity in query)
    //    {
    //      entity.IsActive = false;
    //      _adminUserRepository.Update(entity);
    //    }

    //    _unitOfWork.Commit();
    //    ClearCache();

    //    return new Result().SetSuccess(Messages.RecordActivate, query.Count());
    //  }

    //  public Result Delete(IdsDto dto)
    //  {
    //    if (dto == null || dto.Ids.Count == 0)
    //      return new Result().SetError(Messages.SelectAtLeastOneItemFromList);

    //    var result = _adminUserDeleteValidator.ValidateResult(dto?.Ids);
    //    if (!result.Success) return result;

    //    foreach (var id in dto.Ids)
    //    {
    //      _adminUserRepository.Delete(id);
    //    }

    //    _unitOfWork.Commit();
    //    ClearCache();

    //    return new Result().SetSuccess(Messages.RecordDelete, dto.Ids.Count());
    //  }

    //  private static void ClearCache()
    //  {
    //    //AdminUserCacheManager.ClearCache();
    //  }
    //}
}
