using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminRolePermissions.Admin.CacheManagers;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Admin.Validators;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Extensions;
using Rishvi.Modules.Core.Validators;
using Rishvi.Web.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Services
{
    public interface IAdminPermissionService
    {
        Task<Result> ListAsync(AdminPermissionFilterDto dto);
        Task<Result> DeleteAsync(IList<Guid> ids);

        Task<Result> CreateAsync(AdminPermissionDto dto);
        Task<Result> ByIdAsync(Guid id);
        Task<Result> EditAsync(AdminPermissionDto dto);

        Task<Result> GetSequenceDataAsync();
        Task SaveSequenceDataAsync(IList<AdminPermissionSequenceDto> data);

        Task<Result> GetAdminPermissionsAsync();
    }

    public class AdminPermissionService : IAdminPermissionService
    {
        private readonly IRepository<AdminPermission> _adminPermissionRepository;
        private readonly AdminPermissionValidator _adminPermissionValidator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDapperRepository _dapperRepo;
        private SqlContext _context { get; }
        public AdminPermissionService(
            IRepository<AdminPermission> adminPermissionRepository,
            AdminPermissionValidator adminPermissionValidator,
            IUnitOfWork unitOfWork, IMapper mapper,
            IDapperRepository dapperRepo,
            SqlContext context)
        {
            _adminPermissionRepository = adminPermissionRepository;
            _adminPermissionValidator = adminPermissionValidator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dapperRepo = dapperRepo;
            _context = context;
        }

        //public IQueryable<AdminPermissionListDto> List(AdminPermissionFilterDto dto)
        //{
        //    var filter = dto ?? new AdminPermissionFilterDto();
        //    var query = _adminPermissionRepository.AsNoTracking();

        //    query = new AdminPermissionFilter(query, filter).FilteredQuery();
        //    query = query.OrderBy(o => o.Left);
        //    var result = new Result().SetPaging(filter, query.Count());

        //    result.Data = query.Select(x => new
        //    {
        //        x.Id,
        //        x.Name,
        //        x.DisplayName,
        //        Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < x.Left && w.Right > x.Right),
        //    })
        //        .ToPaged(result.Paging.Page, result.Paging.Size)
        //        .ToList();

        //    return result;
        //}

        public async Task<Result> ListAsync(AdminPermissionFilterDto dto)
        {
            var data = await _adminPermissionRepository.AsNoTracking()
                .OrderBy(o => o.Left)
                .Select(x => new AdminPermissionListDto()
                {
                    Id = x.Id,
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < x.Left && w.Right > x.Right),
                }).ToListAsync();

            var result = data.Select(ss => new AdminPermissionListDto()
            {
                Id = ss.Id,
                Name = ss.Name,
                DisplayName = ss.Depth == 0 ? ss.DisplayName : " | - ".Repeat(ss.Depth) + ss.DisplayName,
                Depth = ss.Depth,
            }).ToList();

            return await new Result().SetDataAsync(result);
        }
        //public IQueryable<AdminPermissionListDto> List(AdminPermissionFilterDto dto)
        //{
        //    var filter = dto ?? new AdminPermissionFilterDto();
        //    var query = _adminPermissionRepository.AsNoTracking();

        //    query = new AdminPermissionFilter(query, filter).FilteredQuery();
        //    query = query.OrderBy(o => o.Left);
        //    var result = new Result().SetPaging(filter, query.Count());

        //    result.Data = query.Select(x => new
        //    {
        //        x.Id,
        //        x.Name,
        //        x.DisplayName,
        //        Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < x.Left && w.Right > x.Right),
        //    })
        //        .ToPaged(result.Paging.Page, result.Paging.Size)
        //        .ToList();

        //    return result;
        //}

        public async Task<Result> DeleteAsync(IList<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new Result().SetError(Messages.SelectAtLeastOneItemFromList);
            }

            var query = _adminPermissionRepository.AsNoTracking()
                .Include(i => i.AdminRolesAdminPermissions)
                .Where(q => ids.Contains(q.Id));

            if (query.Any(w => w.AdminRolesAdminPermissions.Any()))
            {
                return new Result().SetError("You can't delete any record(s) which are assigned to any other record(s).");
            }

            foreach (var item in ids)
            {
                var modules = _adminPermissionRepository.AsNoTracking().Where(w => w.ParentId == item).ToList();
                foreach (var childItem in modules)
                {
                    _adminPermissionRepository.Delete(childItem.Id);
                }
                _adminPermissionRepository.Delete(item);
                _unitOfWork.Commit();

                try
                {
                    using (var context = _context)
                    {
                        NestedSet.BuildTree(context, "AdminPermission");
                    }
                }
                catch
                {
                }               
            }

            var result = new Result().SetSuccess(Messages.RecordDelete, await query.CountAsync());
            AdminRoleCacheManager.ClearCache();

            return result;
        }

        public async Task<Result> CreateAsync(AdminPermissionDto dto)
        {
            var result = await _adminPermissionValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = _mapper.Map<AdminPermission>(dto);

            await _adminPermissionRepository.AddAsync(entity);

            await _unitOfWork.CommitAsync();

            if (dto.ParentId == null)
            {
                var maxNestedSet = _adminPermissionRepository.Get().Max(c => c.Right) ?? 0;
                entity.Left = maxNestedSet + 1;
                entity.Right = maxNestedSet + 2;
                entity.Depth = 0;
            }
            else
            {
                var parentAdminPermission = _adminPermissionRepository.Get().First(w => w.Id == entity.ParentId);
                var valNode = (parentAdminPermission.Left + 1 == parentAdminPermission.Right)
                    ? parentAdminPermission.Left
                    : parentAdminPermission.Right - 1;

                var rightNodes = _adminPermissionRepository.Get().Where(f => f.Right > valNode).ToList();
                rightNodes.ForEach(c => c.Right = c.Right + 2);

                var leftNodes = _adminPermissionRepository.Get().Where(f => f.Left > valNode).ToList();
                leftNodes.ForEach(c => c.Left = c.Left + 2);

                entity.Left = valNode + 1;
                entity.Right = valNode + 2;
                entity.Depth = parentAdminPermission.Depth + 1;
            }

            await _adminPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.CommitAsync();
            AdminPermissionCacheManager.ClearCache();

            result.Id = entity.Id;

    //        var extraWhereCondition = "";
    //        string query = string.Format(
    //                    @"WITH DbLevels AS
				//(
				//	SELECT
				//	Id,
				//	CONVERT(VARCHAR(MAX), Id) AS thePath,
				//	1 AS Level
				//	FROM [{0}]
				//	WHERE ParentId IS NULL {1}
   
				//	UNION ALL
   
				//	SELECT
				//	e.Id,
				//	x.thePath + '.' + CONVERT(VARCHAR(MAX), e.Id) AS thePath,
				//		x.Level + 1 AS Level
				//	FROM DbLevels x 
				//		JOIN [{0}] e on e.ParentId = x.Id {1}
				//		),
				//	DbRows AS
				//	(
				//		SELECT
				//			DbLevels.*,
				//	ROW_NUMBER() OVER (ORDER BY thePath) AS Row
				//	FROM DbLevels
				//		)
				//	UPDATE
				//		[{0}]
				//	SET
				//	[{0}].[Left] = (ER.Row * 2) - ER.Level,
				//	[{0}].[Right] = ((ER.Row * 2) - ER.Level) + 
				//							   (
				//								   SELECT COUNT(*) * 2
				//	FROM DbRows ER2 
				//		WHERE ER2.thePath LIKE ER.thePath + '.%'
				//		) + 1
				//	FROM
				//		DbRows AS ER
				//	WHERE [{0}].Id = ER.Id;", "AdminPermission", extraWhereCondition);
    //        _dapperRepo.Execute(query, new { });

            return await result.SetSuccessAsync(Messages.RecordSaved);
        }

        public async Task<Result> ByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return await new Result().SetDataAsync(new AdminPermissionDto());
            }

            var entity = await _adminPermissionRepository.GetByIdAsync(id);
            return await new Result().SetDataAsync(_mapper.Map<AdminPermissionDto>(entity));
        }

        public async Task<Result> EditAsync(AdminPermissionDto dto)
        {
            var result = await _adminPermissionValidator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _adminPermissionRepository.GetByIdAsync(dto.Id);

            if (entity == null)
            {
                return null;
            }

            _mapper.Map(dto, entity);
            await _adminPermissionRepository.UpdateAsync(entity);
            await _unitOfWork.CommitAsync();

            try
            {
                using (var context = _context)
                {
                    NestedSet.BuildTree(context, "AdminPermission");
                }

                //using (var context = _context)
                //{
                //    NestedSet.MoveToParentNode(context, "AdminPermission", dto.Id,
                //     dto.IsParentSelected == true ? dto.ParentId : null);
                //}
            }
            catch
            {
            }            

            AdminPermissionCacheManager.ClearCache();

            await result.SetIdAsync(entity.Id);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        public async Task<Result> GetSequenceDataAsync()
        {
            var data = await _adminPermissionRepository.AsNoTracking()
                .OrderBy(o => o.Left)
                .Select(s => new AdminPermissionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < s.Left && w.Right > s.Right),
                    Left = s.Left,
                    Right = s.Right,
                    ParentId = s.ParentId ?? Guid.Parse("00000000-0000-0000-0000-000000000000")
                }).ToListAsync();

            return await new Result().SetDataAsync(data);
        }

        public async Task SaveSequenceDataAsync(IList<AdminPermissionSequenceDto> data)
        {
            var adminPermissions = await _adminPermissionRepository.AsNoTracking().ToListAsync();

            var sequence = 1;
            var result = await SetPermissionTreeAsync(adminPermissions, data, null, sequence);
            await _unitOfWork.CommitAsync();

            AdminRoleCacheManager.ClearCache();
        }

        private async Task<(List<AdminPermission> adminPermissions, int sequence)> SetPermissionTreeAsync(List<AdminPermission> adminPermissions, IEnumerable<AdminPermissionSequenceDto> data, Guid? parentId, int sequence)
        {
            foreach (var perms in data)
            {
                var adminPermission = adminPermissions.First(w => w.Id == perms.Item.Id);

                adminPermission.Left = sequence++;
                adminPermission.ParentId = parentId;

                if (perms.Children != null && perms.Children.Any())
                {
                    return await SetPermissionTreeAsync(adminPermissions, perms.Children, adminPermission.Id, sequence);
                }

                adminPermission.Right = sequence++;
                await _adminPermissionRepository.UpdateAsync(adminPermission);
            }

            return (adminPermissions, sequence);
        }

        public async Task<Result> GetAdminPermissionsAsync()
        {

            var data = await _adminPermissionRepository.AsNoTracking()
                .OrderBy(o => o.Left)
                .Select(s => new AdminPermissionDropDownDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Depth = _adminPermissionRepository.AsNoTracking().Count(w => w.Left < s.Left && w.Right > s.Right),
                    Left = s.Left,
                    Right = s.Right,
                    ParentId = s.ParentId ?? Guid.Parse("00000000-0000-0000-0000-000000000000")
                }).ToListAsync();
            return await new Result().SetDataAsync(data);
        }

    }
}
