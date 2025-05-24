using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Admin.Validators;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Encryption;
using Rishvi.Modules.Core.Validators;

namespace Rishvi.Modules.AdminUsers.Admin.Services.Auth
{
    public interface IAdminLoginService
    {
        Task<(Result result, AdminUser adminUser, List<LoginUserPermissions> permissions)> LoginAsync(AdminLoginDto dto);
        Task<Result> GetPermissionsAsync(Guid userId);
    }

    public class AdminLoginService : IAdminLoginService
    {
        private readonly IRepository<AdminUser> _adminUserRepository;
        private readonly IRepository<AdminUsersAdminRoles> _adminUsersAdminRolesRepository;
        private readonly IRepository<AdminRolesAdminPermissions> _adminRolesAdminPermissionsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AdminLoginService(
            IRepository<AdminUser> adminUserRepository,
            IRepository<AdminUsersAdminRoles> adminUsersAdminRolesRepository,
            IRepository<AdminRolesAdminPermissions> adminRolesAdminPermissionsRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _adminUserRepository = adminUserRepository;
            _adminUsersAdminRolesRepository = adminUsersAdminRolesRepository;
            _adminRolesAdminPermissionsRepository = adminRolesAdminPermissionsRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<(Result result, AdminUser adminUser, List<LoginUserPermissions> permissions)> LoginAsync(AdminLoginDto dto)
        {
            var validator = new AdminLoginValidator();
            var result = await validator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return (result, null, null);
            }

            var adminUser = await _adminUserRepository.AsNoTracking()
                 .Include(i => i.AdminUsersAdminRoles)
                 .FirstOrDefaultAsync(w => w.IsActive && w.Email == dto.Email);

            result = adminUser == null ? await SendFailedLoginResponseAsync() : await VerifyPasswordAsync(dto.Password, adminUser);

            if (adminUser != null)
            {
                var userRoles = adminUser.AdminUsersAdminRoles.Select(s => s.AdminRoleId).ToList();

                var permissions = _adminRolesAdminPermissionsRepository.AsNoTracking()
                    .Include(i => i.AdminPermission)
                    .Where(w => userRoles.Contains(w.AdminRoleId))
                    .Select(s => new LoginUserPermissions()
                    {
                        PermissionId = s.AdminPermission.Id,
                        Name = s.AdminPermission.Name,
                        DisplayName = s.AdminPermission.DisplayName
                    }).ToList();

                return (result, adminUser, permissions);
            }
            return (result, adminUser, null);
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

        private async Task<Result> VerifyPasswordAsync(string password, AdminUser adminUser)
        {
            int lockDuration = _configuration.GetValue<int>("LockDuration");
            int totalPasswordAttempt = _configuration.GetValue<int>("TotalPasswordAttemptForAdminUser");

            DateTime accountLockedDate = adminUser.AccountLockedOn == null ? DateTime.Now.Subtract(new TimeSpan(0, 0, lockDuration + 1, 0)) : (DateTime)adminUser.AccountLockedOn;

            if (!SecurityHelper.VerifyHash(password, adminUser.Password, adminUser.Salt))
            {
                adminUser.WrongPasswordAttempt++;
                if (adminUser.AccountLockedOn != null && accountLockedDate.AddMinutes(lockDuration) < DateTime.Now)
                {
                    adminUser.WrongPasswordAttempt = 0;
                }

                if (adminUser.WrongPasswordAttempt < totalPasswordAttempt)
                {
                    adminUser.AccountLockedOn = null;
                }
                else if (adminUser.AccountLockedOn == null && adminUser.WrongPasswordAttempt >= totalPasswordAttempt)
                {
                    adminUser.AccountLockedOn = DateTime.Now;
                }

                _adminUserRepository.Update(adminUser);
                _unitOfWork.Commit();
                if (adminUser.AccountLockedOn != null && adminUser.WrongPasswordAttempt >= totalPasswordAttempt)
                {
                    TimeSpan ts = ((DateTime)adminUser.AccountLockedOn).AddMinutes(lockDuration) - DateTime.Now;
                    return await new Result().SetErrorAsync("Your account has been locked. Please try to login after " + (int)Math.Ceiling(ts.TotalMinutes) + " minutes.");
                }
                else
                {
                    return await new Result().SetErrorAsync("Left " + (totalPasswordAttempt - adminUser.WrongPasswordAttempt) + " attempts");
                }
            }
            else if (adminUser.AccountLockedOn != null && DateTime.Now < accountLockedDate.AddMinutes(lockDuration))
            {
                TimeSpan ts = ((DateTime)adminUser.AccountLockedOn).AddMinutes(lockDuration) - DateTime.Now;
                return await new Result().SetErrorAsync("Your account has been locked. Please try to login after " + (int)Math.Ceiling(ts.TotalMinutes) + " minutes.");
            }

            await SetLastLoginAtAsync(adminUser);

            return await new Result().SetSuccessAsync();
        }

        private async Task<Result> SendFailedLoginResponseAsync()
        {
            return await new Result()
                .SetErrorAsync(Messages.InvalidLogin);
        }

        private async Task SetLastLoginAtAsync(AdminUser adminUser)
        {
            adminUser.LastLoginAt = DateTime.Now;
            adminUser.AccountLockedOn = null;
            adminUser.WrongPasswordAttempt = 0;
            await _adminUserRepository.UpdateAsync(adminUser);
            await _unitOfWork.CommitAsync();
        }
    }
}