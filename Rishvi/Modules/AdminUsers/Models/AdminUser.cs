using System;
using System.Collections.Generic;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;

namespace Rishvi.Modules.AdminUsers.Models
{
    public class AdminUser : IModificationHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsActive { get; set; }
        public string ForgotPasswordToken { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int WrongPasswordAttempt { get; set; }
        public DateTime? AccountLockedOn { get; set; }
        public List<AdminUsersAdminRoles> AdminUsersAdminRoles { get; set; }
    }
}
